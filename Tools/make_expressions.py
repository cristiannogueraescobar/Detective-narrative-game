"""Expresiones (triste, nervioso, enfadado) de los 12 personajes, editando solo la cara de su retrato del juego.

Sesión C. Sin IA: sobre la imagen que usa hoy cada personaje (original o derivado), en la rejilla de su pixel art:
- triste: cejas con el extremo interior alto, una lágrima bajo cada ojo y, si la boca se ve, comisuras hacia abajo;
- nervioso: cejas altas y arqueadas, una gota de sudor junto a la sien y, si la boca se ve, boca en zigzag;
- enfadado: cejas en V (el extremo interior bajo) y las mejillas encendidas.
tranquilo es el retrato de hoy (no se toca). El encuadre no cambia: solo se editan píxeles dentro de la cara.
Coordenadas en píxeles del ORIGINAL de cada base; los derivados en espejo se reflejan aquí.

Uso: python Tools/make_expressions.py [--preview carpeta]"""
import os
import sys

import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.abspath(os.path.join(HERE, '..'))
OUT = os.path.join(REPO, 'Assets', 'Art', 'Derived', 'Expressions')

# Caras de cada base (píxeles del original): cejas (izq, der), ojos (izq, der), boca (None si la tapa un bigote),
# celda (tamaño del píxel del arte)
FACES = {
    'padre': dict(brows=((285, 180, 332, 200), (392, 182, 452, 200)), eyes=((285, 222, 345, 262), (370, 222, 432, 262)),
                  mouth=None, cell=10),
    'javier': dict(brows=((285, 205, 332, 222), (393, 205, 452, 222)), eyes=((285, 240, 345, 262), (370, 240, 432, 262)),
                   mouth=(305, 330, 405, 346), cell=10),
    'madre': dict(brows=((200, 180, 240, 190), (293, 168, 343, 178)), eyes=((203, 194, 238, 234), (290, 194, 343, 234)),
                  mouth=(240, 268, 282, 278), cell=6),
    'hermano': dict(brows=((237, 173, 274, 182), (297, 163, 344, 172)), eyes=((242, 189, 277, 219), (302, 180, 347, 220)),
                    mouth=(270, 243, 309, 250), cell=6),
    'duenio_bar': dict(brows=((277, 186, 317, 203), (352, 178, 412, 198)), eyes=((277, 213, 312, 233), (357, 213, 402, 233)),
                       mouth=None, cell=6),
    'cartero': dict(brows=((392, 159, 422, 165), (447, 159, 469, 165)), eyes=((392, 167, 422, 187), (447, 167, 467, 187)),
                    mouth=None, cell=5),
    'detective': dict(brows=((424, 235, 454, 241), (509, 235, 544, 241)), eyes=((426, 243, 451, 263), (511, 243, 541, 263)),
                      mouth=None, cell=8),
    'vecina': dict(brows=((345, 265, 392, 285), (428, 262, 495, 282)), eyes=((345, 290, 392, 336), (426, 290, 496, 336)),
                   mouth=(380, 355, 440, 412), cell=8),
}

# artId del juego → (imagen de hoy, cara, en espejo respecto a su base, expresiones que faltan; ART-NEEDED.md)
CHARACTERS = {
    'daniel': ('Images/Suspects/padre.gif.png', 'padre', False, ('nervioso', 'enfadado')),
    'carmen': ('Images/Suspects/madre.gif.png', 'madre', False, ('nervioso', 'triste')),
    'lucas': ('Images/Suspects/hermano.gif.png', 'hermano', False, ('nervioso', 'triste')),
    'rosario': ('Art/Derived/amparo.png', 'vecina', False, ('nervioso', 'triste')),
    'marcos': ('Images/Suspects/duenio_bar.gif.png', 'duenio_bar', False, ('nervioso', 'triste')),
    'andres': ('Images/Suspects/cartero.gif.png', 'cartero', False, ('nervioso', 'triste')),
    'ruiz': ('Images/Suspects/detective.gif.png', 'detective', False, ('nervioso', 'enfadado')),
    'maruxa': ('Art/Derived/maruxa.png', 'vecina', False, ('triste', 'nervioso')),
    'javier': ('Art/Derived/javier.png', 'javier', True, ('triste', 'nervioso')),
    'lucia': ('Art/Derived/lucia.png', 'madre', True, ('nervioso', 'triste')),
    'alex': ('Art/Derived/alex.png', 'hermano', True, ('triste', 'nervioso')),
    'encarna': ('Art/Derived/encarna.png', 'vecina', True, ('nervioso', 'triste')),
}

DARK = (35, 20, 14)
TEAR = (150, 205, 245)
TEAR_EDGE = (60, 110, 160)


def _mirror(box, width, mirrored):
    x0, y0, x1, y1 = box
    return (width - x1, y0, width - x0, y1) if mirrored else box


def _paint(a, box, rgb):
    x0, y0, x1, y1 = [int(round(v)) for v in box]
    a[max(0, y0):max(0, y1), max(0, x0):max(0, x1), :3] = rgb
    a[max(0, y0):max(0, y1), max(0, x0):max(0, x1), 3] = 255


def _skin(a, box):
    """Color de la piel justo debajo de un ojo (la mediana de lo cálido y claro)."""
    x0, y0, x1, y1 = [int(v) for v in box]
    patch = a[y1 + 2:y1 + 2 + (y1 - y0), x0:x1, :3].reshape(-1, 3).astype(int)
    warm = patch[(patch[:, 0] > patch[:, 1]) & (patch[:, 1] > patch[:, 2]) & (patch[:, 0] > 150)]
    return tuple(int(c) for c in np.median(warm if len(warm) else patch, axis=0))


def _clear_brow(a, box):
    """Quita la ceja de hoy: sus píxeles oscuros pasan al color de la frente que la rodea (no un rectángulo plano)."""
    x0, y0, x1, y1 = [max(0, int(v)) for v in box]
    sub = a[y0:y1, x0:x1]
    rgb = sub[..., :3].astype(int)
    dark = (rgb.max(-1) < 110) & (sub[..., 3] > 128)
    ring = a[max(0, y0 - (y1 - y0)):y0, x0:x1, :3].reshape(-1, 3).astype(int)
    light = ring[ring.max(-1) >= 110]
    if len(light) == 0:
        light = rgb[~dark].reshape(-1, 3)
    if len(light) == 0:
        return
    sub[dark, :3] = np.median(light, axis=0).astype(np.uint8)


def expression(img, face, mirrored, kind):
    a = np.asarray(img.convert('RGBA')).copy()
    w = a.shape[1]
    cell = face['cell']
    eyes = sorted(_mirror(b, w, mirrored) for b in face['eyes'])     # de izquierda a derecha en la imagen final
    brows = sorted(_mirror(b, w, mirrored) for b in face['brows'])
    skin = _skin(a, eyes[0])
    left_index = 0
    for i, (bx0, by0, bx1, by1) in enumerate(brows):
        _clear_brow(a, (bx0 - cell, by0 - cell, bx1 + cell, by1 + cell))        # la ceja de hoy fuera
        thick = max(cell, by1 - by0)
        steps = 3
        width = (bx1 - bx0) / steps
        image_left = (i == left_index)
        for k in range(steps):
            inner = steps - 1 - k if image_left else k          # 0 = junto a la nariz
            outer = steps - 1 - inner
            if kind == 'triste':      # extremo interior (junto a la nariz) arriba
                dy = -(steps - 1 - inner) * cell * 0.9 + cell * 0.6
            elif kind == 'enfadado':  # extremo interior abajo: V
                dy = (steps - 1 - inner) * cell * 0.9 - cell * 0.8
            else:  # nervioso: altas y arqueadas
                dy = -cell * 1.8 - (cell * 0.7 if k == 1 else 0)
            x = bx0 + k * width
            _paint(a, (x, by0 + dy, x + width + 1, by0 + dy + thick), DARK)
    if kind == 'triste':
        for (ex0, ey0, ex1, ey1) in eyes:
            mx = (ex0 + ex1) / 2
            _paint(a, (mx - cell, ey1 + cell * 0.5, mx + cell, ey1 + cell * 3), TEAR_EDGE)
            _paint(a, (mx - cell * 0.5, ey1 + cell * 0.5, mx + cell * 0.5, ey1 + cell * 2.5), TEAR)
    elif kind == 'nervioso':
        right = eyes[1 - left_index]
        sx = right[2] + cell * 1.2
        sy = right[1] - cell * 2
        _paint(a, (sx - cell * 0.4, sy - cell * 0.4, sx + cell * 2.0, sy + cell * 3.4), TEAR_EDGE)
        _paint(a, (sx, sy, sx + cell * 1.6, sy + cell * 3), TEAR)
    elif kind == 'enfadado':
        for (ex0, ey0, ex1, ey1) in eyes:
            y0, y1, x0, x1 = int(ey1 + cell * 0.5), int(ey1 + cell * 2.5), int(ex0 - cell), int(ex1 + cell)
            sub = a[y0:y1, x0:x1].astype(int)
            mask = sub[..., 3] > 128
            sub[..., 0] = np.where(mask, np.minimum(255, sub[..., 0] + 45), sub[..., 0])
            sub[..., 1] = np.where(mask, (sub[..., 1] * 0.78).astype(int), sub[..., 1])
            a[y0:y1, x0:x1] = sub.astype(np.uint8)
    if face['mouth'] is not None and kind in ('triste', 'nervioso'):
        mx0, my0, mx1, my1 = _mirror(face['mouth'], w, mirrored)
        _paint(a, (mx0, my0 - cell, mx1, my1 + cell), skin)
        mid = (my0 + my1) / 2
        if kind == 'triste':   # comisuras hacia abajo
            _paint(a, (mx0 + cell, mid - cell * 0.5, mx1 - cell, mid + cell * 0.5), (110, 45, 30))
            _paint(a, (mx0, mid + cell * 0.3, mx0 + cell, mid + cell * 1.3), (110, 45, 30))
            _paint(a, (mx1 - cell, mid + cell * 0.3, mx1, mid + cell * 1.3), (110, 45, 30))
        else:                  # zigzag
            n = max(3, int((mx1 - mx0) // cell))
            for k in range(n):
                y = mid - cell * 0.5 + (cell * 0.6 if k % 2 else 0)
                _paint(a, (mx0 + k * cell, y, mx0 + (k + 1) * cell, y + cell * 0.8), (110, 45, 30))
    return Image.fromarray(a, 'RGBA')


def build(key):
    path, face, mirrored, kinds = CHARACTERS[key]
    img = Image.open(os.path.join(REPO, 'Assets', path)).convert('RGBA')
    return {kind: expression(img, FACES[face], mirrored, kind) for kind in kinds}


def main():
    out = OUT
    if '--preview' in sys.argv:
        out = sys.argv[sys.argv.index('--preview') + 1]
    os.makedirs(out, exist_ok=True)
    only = [a for a in sys.argv[1:] if a in CHARACTERS]
    for key in only or CHARACTERS:
        for kind, img in build(key).items():
            img.save(os.path.join(out, f'{key}_{kind}.png'))
        print('ok', key)


if __name__ == '__main__':
    main()
