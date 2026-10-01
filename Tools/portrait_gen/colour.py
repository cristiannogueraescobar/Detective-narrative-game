"""Color de los retratos generados frente a las referencias (docs/art/javier/BRIEF.md, sección 3).

Mide en CIELAB (a*: verde − / rojo +; b*: azul − / amarillo +; croma = saturación) la figura entera y la piel, y
corrige el tono frío y verdoso con una transferencia de color tipo Reinhard limitada:
- a* y b*: desplazados lo que la media de la piel del candidato se aleja de la de Marcos y Lucía (solo la media: escalar
  también la desviación, como en Reinhard, amplificaba el verde de la ropa: a* de la figura bajó a -10);
- L*: multiplicada por (L* piel referencia / L* piel candidato). Medido (01-10-2026): la mayor diferencia era el brillo
  (figuras L* ~32 frente a 48-64; piel ~57 frente a 72), no el matiz (a* de la piel ya ~24 como en las referencias). Al
  multiplicar, el negro del contorno sigue negro y las sombras conservan su relación;
- calculada sobre la PIEL (lo único comparable entre personajes: la ropa de Javier no tiene por qué parecerse al
  delantal de Lucía) y aplicada a toda la figura, que es lo que quita el tinte general;
- sin tocar el contorno (L* < 15) ni lo semitransparente (la sombra), y píxel a píxel: la rejilla de 5 px se mantiene.
Uso: python Tools/portrait_gen/colour.py medir <png> ...   |   corregir <entrada.png> <salida.png> [fuerza 0-1]"""
import os
import sys

import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.abspath(os.path.join(HERE, '..', '..'))
sys.path.insert(0, os.path.join(REPO, 'Tools'))
REFERENCES = ['Assets/Images/Suspects/duenio_bar.gif.png', 'Assets/Images/Suspects/madre.gif.png']  # Marcos, Lucía


def srgb_to_lab(rgb):
    c = rgb / 255.0
    c = np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)
    xyz = c @ np.array([[0.4124, 0.3576, 0.1805], [0.2126, 0.7152, 0.0722], [0.0193, 0.1192, 0.9505]]).T
    xyz /= np.array([0.95047, 1.0, 1.08883])
    f = np.where(xyz > 216 / 24389, np.cbrt(xyz), (24389 / 27 * xyz + 16) / 116)
    return np.stack([116 * f[..., 1] - 16, 500 * (f[..., 0] - f[..., 1]), 200 * (f[..., 1] - f[..., 2])], -1)


def lab_to_srgb(lab):
    fy = (lab[..., 0] + 16) / 116
    fx, fz = fy + lab[..., 1] / 500, fy - lab[..., 2] / 200
    f = np.stack([fx, fy, fz], -1)
    xyz = np.where(f ** 3 > 216 / 24389, f ** 3, (116 * f - 16) / (24389 / 27)) * np.array([0.95047, 1.0, 1.08883])
    c = xyz @ np.array([[3.2406, -1.5372, -0.4986], [-0.9689, 1.8758, 0.0415], [0.0557, -0.2040, 1.0570]]).T
    c = np.where(c <= 0.0031308, 12.92 * c, 1.055 * np.clip(c, 0, None) ** (1 / 2.4) - 0.055)
    return (np.clip(c, 0, 1) * 255).round()


def skin_mask(rgb, opaque):
    """Piel: tonos naranja-carne (r > g > b, matiz 8-60°, saturación media, clara), solo en la franja de la cabeza.
    Hasta 60° y no 40°: el LoRA de estilo pinta la cara amarillo limón (matiz 56°) y sin contarla como piel ni se
    corregía ni se libraba de saturar (prueba ciega 5: "piel amarillo limón", primer motivo en las tres respuestas)."""
    r, g, b = (rgb[..., i].astype(float) for i in range(3))
    mx, mn = np.maximum(np.maximum(r, g), b), np.minimum(np.minimum(r, g), b)
    d = np.maximum(mx - mn, 1e-6)
    hue = np.where(mx == r, ((g - b) / d) % 6, np.where(mx == g, (b - r) / d + 2, (r - g) / d + 4)) * 60
    sat = (mx - mn) / np.maximum(mx, 1e-6)
    tone = opaque & (r > g) & (g > b) & (hue > 8) & (hue < 60) & (sat > 0.25) & (sat < 0.8) & (mx > 110)
    # Solo en la franja de la cabeza (22 % superior de la figura): la ropa oliva-ocre tiene el mismo matiz que la piel
    # y en Javier 6111 la corrección se calculaba sobre la camisa (71 350 "píxeles de piel")
    ys = np.where(opaque.any(axis=1))[0]
    if len(ys):
        head = np.zeros_like(tone)
        head[ys.min():ys.min() + int((ys.max() - ys.min()) * 0.22)] = True
        tone &= head
    return tone


def load(path):
    """RGBA de una referencia (sobre blanco y normalizada como los candidatos) o de un candidato ya procesado."""
    im = Image.open(path).convert('RGBA')
    if path.replace('\\', '/').endswith('.gif.png'):
        import remove_white_bg as rwb
        white = Image.new('RGBA', im.size, (255, 255, 255, 255))
        white.alpha_composite(im)
        im = rwb.process(white.convert('RGB'))[0]
    return np.asarray(im)


def stats(rgba):
    opaque = rgba[..., 3] == 255
    rgb = rgba[..., :3]
    lab = srgb_to_lab(rgb.astype(float))
    figure = opaque & (lab[..., 0] >= 15)
    skin = skin_mask(rgb, figure)

    def s(m):
        v = lab[m]
        return {'L': v[:, 0].mean(), 'a': v[:, 1].mean(), 'b': v[:, 2].mean(),
                'C': np.hypot(v[:, 1], v[:, 2]).mean(), 'sa': v[:, 1].std(), 'sb': v[:, 2].std(), 'n': int(m.sum())}
    return {'figura': s(figure), 'piel': s(skin)}


import functools


@functools.lru_cache(maxsize=1)
def reference_skin_full():
    v = [stats(load(os.path.join(REPO, p)))['piel'] for p in REFERENCES]
    return {k: float(np.mean([x[k] for x in v])) for k in ('L', 'a', 'b', 'sa', 'sb')}


def correct(rgba, strength=1.0, target=None):
    """Transferencia a*/b* calculada sobre la piel y aplicada a la figura (ver docstring del módulo)."""
    target = target or reference_skin_full()
    src = stats(rgba)['piel']
    if src['n'] == 0:
        # Sin piel en la banda de la cabeza no hay nada que medir. Antes la media era NaN y la figura salía negra
        # (sesión B: 13 de 26 candidatos del LoRA de estilo).
        return rgba.copy()
    out = rgba.copy()
    opaque = rgba[..., 3] == 255
    lab = srgb_to_lab(rgba[..., :3].astype(float))
    m = opaque & (lab[..., 0] >= 15)
    ratio = 1 + strength * (target['L'] / max(src['L'], 1e-6) - 1)
    lab[..., 0] = np.where(opaque, np.clip(lab[..., 0] * ratio, 0, 100), lab[..., 0])
    for ch, mu_s, mu_t in ((1, src['a'], target['a']), (2, src['b'], target['b'])):
        lab[..., ch] = np.where(m, lab[..., ch] + strength * (mu_t - mu_s), lab[..., ch])
    rgb = lab_to_srgb(lab)
    out[..., :3] = np.where(m[..., None], rgb, rgba[..., :3]).astype(np.uint8)
    return out


def main(argv):
    if argv[0] == 'medir':
        print(f"{'':28} {'zona':6} {'L*':>5} {'a*':>6} {'b*':>6} {'croma':>6}  píxeles")
        for p in [os.path.join(REPO, r) for r in REFERENCES] + argv[1:]:
            for zone, v in stats(load(p)).items():
                print(f"{os.path.basename(p)[:28]:28} {zone:6} {v['L']:5.1f} {v['a']:6.1f} {v['b']:6.1f} {v['C']:6.1f}  {v['n']}")
    elif argv[0] == 'corregir':
        rgba = np.asarray(Image.open(argv[1]).convert('RGBA'))
        strength = float(argv[3]) if len(argv) > 3 else 1.0
        Image.fromarray(correct(rgba, strength), 'RGBA').save(argv[2])
        print(argv[2])


if __name__ == '__main__':
    main(sys.argv[1:])
