"""Quita el fondo blanco de un retrato generado y lo deja listo para Assets/Art/Portraits (docs/art/<personaje>/BRIEF.md).

Los generadores de imagen no dan transparencia real: se les pide "plain flat white background" y esto la crea.
- Solo es fondo el blanco (y gris claro poco saturado) CONECTADO con el borde: relleno desde los bordes. El contorno
  negro lo para, así que una camisa blanca o el reflejo de una botella dentro de la figura siguen opacos.
- Conserva la sombra suave junto a los pies (los retratos de referencia 02-04 la llevan, ~18 % de opacidad): el gris
  claro del fondo a la altura de las suelas pasa a negro semitransparente (alfa = 255 - luminancia: sobre blanco se ve
  igual que en la imagen generada).
- Avisa de halos claros alrededor de la figura (antialiasing del generador) y del patrón de cuadros pintado.
- Escala por vecino más cercano (no emborrona el píxel) para que la figura mida el 90 % de 1024 px y la deja centrada
  en 768x1024 con las suelas a 40 px del borde inferior.

Uso: python Tools/remove_white_bg.py entrada.png salida.png [--sin-sombra]
Tests: python -m unittest discover Tools/tests
"""
import sys

import numpy as np
from PIL import Image
from scipy import ndimage

CANVAS = (768, 1024)
FIGURE_FILL = 0.90        # Alto de la figura / alto del lienzo (mediana de los retratos actuales)
FEET_MARGIN = 40          # px entre las suelas y el borde inferior
BG_SAT, BG_LUM = 30, 150  # Fondo posible: poco saturado y claro (blanco, gris de sombra, cuadros)
WHITE_LUM = 240           # A partir de aquí es blanco de fondo puro
HALO_RATIO = 1.0          # px claros pegados a la figura por px de contorno; los retratos actuales dan 0,31-0,62
PURE_WHITE = 253          # Fondo encerrado (entre brazo y cuerpo): blanco puro en los tres canales...
HOLE_MIN = 0.0002         # ...y de al menos este área (fracción de la imagen). Una camisa tiene sombreado: no lo es


def _lum_sat(rgb):
    rgb = rgb.astype(int)
    return rgb.mean(-1), rgb.max(-1) - rgb.min(-1)


def _checkerboard(rgb, lum):
    """¿Hay dos niveles claros alternando en las esquinas? (el generador pintó la rejilla de transparencia)."""
    h, w = lum.shape
    k = max(16, min(h, w) // 12)
    corners = np.concatenate([lum[:k, :k].ravel(), lum[:k, -k:].ravel(), lum[-k:, :k].ravel(), lum[-k:, -k:].ravel()])
    levels = np.round(corners / 8).astype(int)
    values, counts = np.unique(levels, return_counts=True)
    top = sorted(zip(counts, values), reverse=True)[:2]
    return len(top) == 2 and top[1][0] > 0.2 * corners.size and abs(top[0][1] - top[1][1]) >= 2


def process(image, keep_shadow=True):
    """Devuelve (imagen RGBA 768x1024, informe). El informe lleva 'warnings', 'shadow_pixels' y 'scale'."""
    if image.mode in ('RGBA', 'LA', 'P'):
        base = Image.new('RGBA', image.size, (255, 255, 255, 255))
        base.alpha_composite(image.convert('RGBA'))
        image = base
    rgb = np.asarray(image.convert('RGB'))
    h, w = rgb.shape[:2]
    lum, sat = _lum_sat(rgb)
    warnings = []

    if _checkerboard(rgb, lum):
        warnings.append('fondo de cuadros pintado por el generador: pide "plain flat white background" y repite')

    # Fondo = zona clara y poco saturada conectada con el borde
    candidate = (sat <= BG_SAT) & (lum >= BG_LUM)
    labels, _ = ndimage.label(candidate)
    border = np.unique(np.concatenate([labels[0], labels[-1], labels[:, 0], labels[:, -1]]))
    background = np.isin(labels, border[border > 0])
    # Fondo encerrado por la figura (hueco entre el brazo y el cuerpo): blanco puro y plano, sin sombreado
    pure = (rgb >= PURE_WHITE).all(-1) & ~background
    holes, n = ndimage.label(pure)
    if n:
        sizes = ndimage.sum(pure, holes, range(1, n + 1))
        big = [i + 1 for i, s in enumerate(sizes) if s >= HOLE_MIN * h * w]
        background |= np.isin(holes, big)
    figure = ~background
    ys, xs = np.where(figure)
    if len(ys) == 0:
        raise ValueError('no hay figura: la imagen es todo fondo')
    y0, y1, x0, x1 = ys.min(), ys.max() + 1, xs.min(), xs.max() + 1
    fh, fw = y1 - y0, x1 - x0

    # Sombra: gris del fondo a la altura de las suelas, cerca de los pies
    zone = np.zeros_like(background)
    zone[max(0, int(y1 - 0.08 * fh)):min(h, int(y1 + 0.05 * fh)),
         max(0, int(x0 - 0.4 * fw)):min(w, int(x1 + 0.4 * fw))] = True
    grey = background & (lum < WHITE_LUM)
    shadow = grey & zone if keep_shadow else np.zeros_like(grey)

    # Halo: gris claro del fondo pegado a la figura fuera de la zona de la sombra
    touching = ndimage.binary_dilation(figure, iterations=2)
    halo = grey & touching & ~zone
    contour = max(1, int((figure & ~ndimage.binary_erosion(figure)).sum()))
    if halo.sum() / contour > HALO_RATIO:
        warnings.append(f'halo claro alrededor de la figura ({halo.sum() / contour:.2f} px por px de contorno; los '
                        f'retratos actuales dan 0,31-0,62): bordes antialiasados de más; revisa el contorno')

    alpha = np.where(figure, 255, 0).astype(np.uint8)
    out_rgb = rgb.copy()
    alpha[shadow] = (255 - lum[shadow]).clip(0, 255).astype(np.uint8)
    out_rgb[shadow] = 0
    rgba = np.dstack([out_rgb, alpha])

    # Escala por vecino más cercano: la figura al 90 % del alto, sin pasarse de ancho
    cw, ch = CANVAS
    scale = min(FIGURE_FILL * ch / fh, 0.96 * cw / fw)
    sy, sx = np.where(alpha > 0)
    cy0, cy1, cx0, cx1 = sy.min(), sy.max() + 1, sx.min(), sx.max() + 1
    crop = Image.fromarray(rgba[cy0:cy1, cx0:cx1], 'RGBA')
    scaled = crop.resize((max(1, round(crop.width * scale)), max(1, round(crop.height * scale))), Image.NEAREST)
    canvas = Image.new('RGBA', CANVAS, (0, 0, 0, 0))
    feet = ch - FEET_MARGIN
    top = feet - round((y1 - cy0) * scale)
    left = round(cw / 2 - ((x0 + x1) / 2 - cx0) * scale)
    canvas.alpha_composite(_clip(scaled, left, top, CANVAS), (max(0, left), max(0, top)))

    a = np.asarray(canvas)[..., 3]
    return canvas, {'warnings': warnings, 'shadow_pixels': int(((a > 0) & (a < 255)).sum()), 'scale': scale}


def _clip(img, left, top, size):
    """Recorta lo que se saldría del lienzo (una sombra muy ancha, por ejemplo)."""
    w, h = size
    l, t = max(0, -left), max(0, -top)
    r, b = min(img.width, w - left), min(img.height, h - top)
    return img.crop((l, t, r, b))


def main(argv):
    if len(argv) < 2:
        sys.exit(__doc__)
    out, report = process(Image.open(argv[0]), keep_shadow='--sin-sombra' not in argv)
    out.save(argv[1])
    print(f'{argv[1]}: 768x1024, escala x{report["scale"]:.3f}, {report["shadow_pixels"]} px de sombra')
    for w in report['warnings']:
        print('  AVISO:', w)


if __name__ == '__main__':
    main(sys.argv[1:])
