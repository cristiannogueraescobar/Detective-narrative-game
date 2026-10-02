"""Mide retratos nuevos para integrarlos (encargos de docs/art/<personaje>/BRIEF.md).

Para cada PNG: tamaño, fondo transparente y encuadre de la figura (parte opaca; ignora motas sueltas y humo separado).
Con varios estados del mismo personaje comprueba que comparten encuadre (la cara no debe saltar al cambiar de estado)
e imprime los Rect en coordenadas UV de Unity (origen abajo a la izquierda), listos para PortraitCrops:
  - figure: de los pies a la cabeza (rueda con alturas reales, PortraitCrops.Figure)
  - bust: plano medio de la cabeza a la cintura (interrogatorio)
  - face: la cabeza (mini-retrato del chat)
Son un punto de partida medido: compruébalos en captura (skill batchmode-capture) antes de darlos por buenos.

Uso: python Tools/measure_portrait.py Assets/Art/Portraits/javier_tranquilo.png Assets/Art/Portraits/javier_triste.png ...
"""
import sys

import numpy as np
from PIL import Image

try:
    from scipy import ndimage
except ImportError:  # Sin scipy: toda la parte opaca cuenta como figura
    ndimage = None

EXPECTED = (768, 1024)
MAX_SHIFT = 0.01  # Diferencia de encuadre tolerada entre estados (1 % de la imagen)


def figure_mask(alpha):
    mask = alpha > 128
    if ndimage is None:
        return mask
    labels, n = ndimage.label(mask)
    if n == 0:
        return mask
    sizes = ndimage.sum(mask, labels, range(1, n + 1))
    keep = [i + 1 for i, s in enumerate(sizes) if s > sizes.max() * 0.02]  # La figura y lo que lleva en la mano
    return np.isin(labels, keep)


def uv(x0, y0, x1, y1, w, h):
    """Píxeles (y hacia abajo) a Rect UV (y hacia arriba)."""
    return (x0 / w, 1 - y1 / h, (x1 - x0) / w, (y1 - y0) / h)


def measure(path):
    im = Image.open(path).convert('RGBA')
    w, h = im.size
    a = np.asarray(im)[..., 3]
    notes = []
    if (w, h) != EXPECTED:
        notes.append(f'tamaño {w}x{h} (se pide {EXPECTED[0]}x{EXPECTED[1]})')
    border = np.concatenate([a[0], a[-1], a[:, 0], a[:, -1]])
    if (border > 16).mean() > 0.05:
        notes.append('el borde no es transparente: ¿fondo sin quitar?')
    semi = ((a > 16) & (a < 240)).mean()
    if semi > 0.02:
        notes.append(f'{semi:.0%} de píxeles semitransparentes: ¿bordes suavizados o sombra?')
    m = figure_mask(a)
    ys, xs = np.where(m)
    x0, x1, y0, y1 = xs.min(), xs.max() + 1, ys.min(), ys.max() + 1
    fh = y1 - y0
    # Proporciones ajustadas sobre los 7 retratos con encuadre ya validado en el juego (medianas, relativas a la altura
    # de la figura): cara = cuadrado de 0,22 de alto desde 0,03 bajo la coronilla; busto 3:4 de -0,02 a 0,54.
    # Centradas en la cabeza (filas del 18 % superior de la figura).
    head_rows = m[y0:y0 + int(fh * 0.18)]
    hx = np.where(head_rows.any(axis=0))[0]
    hcx = (hx.min() + hx.max() + 1) / 2
    side = fh * 0.22
    face = (hcx - side / 2, y0 + fh * 0.03, hcx + side / 2, y0 + fh * 0.03 + side)
    bust_h = fh * 0.56
    bust = (hcx - bust_h * 0.375, y0 - fh * 0.02, hcx + bust_h * 0.375, y0 + fh * 0.54)
    clamp = lambda r: (max(0, r[0]), max(0, r[1]), min(w, r[2]), min(h, r[3]))
    return {
        'size': (w, h), 'notes': notes,
        'figure': uv(x0, y0, x1, y1, w, h),
        'bust': uv(*clamp(bust), w, h),
        'face': uv(*clamp(face), w, h),
        'fill': fh / h,
    }


def fmt(r):
    return 'new Rect({:.3f}f, {:.3f}f, {:.3f}f, {:.3f}f)'.format(*r)


def main(paths):
    results = {p: measure(p) for p in paths}
    for p, r in results.items():
        print(f'{p}: {r["size"][0]}x{r["size"][1]}, la figura ocupa el {r["fill"]:.0%} del alto')
        for n in r['notes']:
            print('  AVISO:', n)
    if len(results) > 1:
        figs = np.array([r['figure'] for r in results.values()])
        shift = np.abs(figs - figs.mean(axis=0)).max()
        verdict = 'OK' if shift <= MAX_SHIFT else 'NO: la figura se mueve entre estados; pide el mismo encuadre'
        print(f'Encuadre entre estados: diferencia máxima {shift:.3f} → {verdict}')
    mean = {k: tuple(np.mean([r[k] for r in results.values()], axis=0)) for k in ('figure', 'bust', 'face')}
    print('\nPara PortraitCrops (media de los estados):')
    for k in ('figure', 'bust', 'face'):
        print(f'  {k:6} = {fmt(mean[k])}')


if __name__ == '__main__':
    if len(sys.argv) < 2:
        sys.exit(__doc__)
    main(sys.argv[1:])
