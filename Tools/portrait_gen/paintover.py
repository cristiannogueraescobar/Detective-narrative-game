"""Imagen de partida para imagen a imagen: Marcos (02) retocado por código hacia Javier ("paint-over").

Revisión de Cristian: el estilo tiene que salir de un original. Barrido de fuerza desde Marcos sin retocar (semillas
6000-6001): a 0,55-0,75 se conserva el estilo pero también su delantal, su jarra en alto y su postura; a 0,85 ya es
otro personaje pero el estilo se pierde. Retocando la imagen de partida, a fuerza media el modelo solo tiene que
"dibujar bien" lo que ya está esbozado:
- fuera la jarra, el humo, el brazo en alto, el cigarro y el bigote (el bigote, a piel: Javier lleva barba de días);
- brazo derecho colgando con una botella verde, esbozado siguiendo el esqueleto de pose.defensive_stance;
- camisa blanca → franela oliva, chaleco negro → marrón, delantal rojo y llaves → pantalón marrón.
Los cambios de color conservan la luminancia de cada píxel: se mantienen el sombreado y el contorno negro de Marcos.
Coordenadas: Marcos normalizado (remove_white_bg) y escalado a 864x1152 sobre gris claro.

Uso: python Tools/portrait_gen/paintover.py <salida.png>"""
import os
import sys

import numpy as np
from PIL import Image, ImageDraw

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.abspath(os.path.join(HERE, '..', '..'))
sys.path[:0] = [HERE, os.path.join(REPO, 'Tools')]

import pose  # noqa: E402
import remove_white_bg as rwb  # noqa: E402

W, H = 864, 1152
BG = (190, 190, 194)
SKIN = (222, 150, 100)
OLIVE = (112, 106, 58)      # Franela oliva
BROWN = (92, 66, 42)        # Chaleco → marrón
TROUSERS = (74, 54, 38)     # Pantalón de trabajo marrón
BOTTLE = (40, 120, 45)


def marcos_on_grey():
    """Marcos sobre gris claro y su máscara de figura (para no repintar el fondo)."""
    im = Image.open(os.path.join(REPO, 'Assets/Images/Suspects/duenio_bar.gif.png')).convert('RGBA')
    white = Image.new('RGBA', im.size, (255, 255, 255, 255))
    white.alpha_composite(im)
    norm = rwb.process(white.convert('RGB'))[0].resize((W, H), Image.NEAREST)
    base = Image.new('RGBA', (W, H), BG + (255,))
    base.alpha_composite(norm)
    return np.asarray(base.convert('RGB')).astype(float), np.asarray(norm)[..., 3] == 255


def recolour(a, mask, colour):
    """Pinta `colour` en la máscara con el sombreado relativo de cada píxel (luminancia / mediana de la zona)."""
    if not mask.any():
        return
    lum = a.mean(-1)
    shade = (lum / max(np.median(lum[mask]), 1))[mask][:, None]
    a[mask] = np.clip(np.array(colour, float) * shade, 0, 255)


def make():
    a, figure = marcos_on_grey()
    rgb = a.astype(int)
    lum = rgb.mean(-1)
    sat = rgb.max(-1) - rgb.min(-1)
    yy, xx = np.mgrid[0:H, 0:W]
    box = lambda x0, y0, x1, y1: (xx >= x0) & (xx < x1) & (yy >= y0) & (yy < y1)

    # Fuera humo, jarra y brazo en alto (a fondo)
    a[box(100, 80, 270, 520)] = BG
    a[box(255, 240, 300, 440)] = BG
    # Bigote y cigarro → piel; pulsera y llaves fuera
    a[box(365, 222, 472, 268) & (lum < 120)] = SKIN
    a[box(340, 255, 400, 290) & (lum > 120)] = SKIN
    a[box(575, 510, 640, 580) & (rgb[..., 0] > 150) & (rgb[..., 2] < 90)] = SKIN
    a[box(490, 595, 545, 750) & (sat > 60)] = TROUSERS

    # Ropa (sin tocar el contorno: lum < 18)
    torso = box(270, 285, 760, 610) & figure
    white = torso & (lum > 150) & (sat < 60)
    recolour(a, white, OLIVE)
    vest = box(270, 300, 610, 600) & figure & (lum >= 18) & (lum < 80)
    recolour(a, vest, BROWN)
    apron = box(255, 580, 510, 895) & figure & (rgb[..., 0] > rgb[..., 1] + 30)
    recolour(a, apron, TROUSERS)
    legs = box(250, 600, 700, 1060) & figure & (lum >= 18) & (lum < 80)
    recolour(a, legs, TROUSERS)

    # Segunda versión, tras el barrido 0,55-0,75: a 0,65 el delantal se volvía un abrigo largo → hueco entre las piernas;
    # y canas en las sienes, que no salían solo con el prompt
    a[box(445, 700, 505, 895) & figure] = BG
    a[box(440, 690, 510, 705) & figure] = (0, 0, 0)                 # Entrepierna con contorno
    temples = (box(352, 120, 384, 205) | box(540, 120, 575, 205)) & figure & (lum < 90)
    a[temples] = np.array((150, 150, 152), float) * (lum[temples] / 60.0)[:, None].clip(0.6, 1.4)

    # Brazo derecho colgando con la botella, siguiendo el esqueleto de la postura defensiva
    img = Image.fromarray(np.clip(a, 0, 255).astype(np.uint8))
    d = ImageDraw.Draw(img)
    p = pose.defensive_stance(W, H)
    (sx, sy), (ex, ey), (wx, wy) = (300, 365), p[3], p[4]     # Hombro algo más abajo: sin pico sobre el hombro
    # Grosores como los brazos de Marcos (~60-80 px a este tamaño)
    d.line([(sx, sy), (ex, ey)], fill=(0, 0, 0), width=96)
    d.line([(sx, sy), (ex, ey)], fill=OLIVE, width=80)            # Manga remangada
    d.ellipse((sx - 40, sy - 40, sx + 40, sy + 40), fill=OLIVE)    # Hombro redondeado (no la punta de la línea)
    d.ellipse((ex - 40, ey - 40, ex + 40, ey + 40), fill=OLIVE, outline=(0, 0, 0), width=8)  # Codo
    d.line([(ex, ey), (wx, wy)], fill=(0, 0, 0), width=76)
    d.line([(ex, ey), (wx, wy)], fill=SKIN, width=60)             # Antebrazo
    d.ellipse((wx - 38, wy - 24, wx + 38, wy + 42), fill=SKIN, outline=(0, 0, 0), width=8)  # Mano
    d.rectangle((wx - 11, wy + 20, wx + 11, wy + 60), fill=BOTTLE, outline=(0, 0, 0), width=6)  # Cuello de la botella
    d.rounded_rectangle((wx - 26, wy + 56, wx + 26, wy + 210), radius=18, fill=BOTTLE, outline=(0, 0, 0), width=8)
    return img


if __name__ == '__main__':
    out = sys.argv[1] if len(sys.argv) > 1 else 'init_javier.png'
    make().save(out)
    print(out)
