"""Arte de cada historia (C5), generado por código: archivos NUEVOS en Assets/Art/Stories.

Pixel art a 1/4 de resolución (270x480 -> 1080x1920) con paleta limitada por historia y tramado ordenado
(Bayer 4x4), para casar con los retratos. La parte de arriba queda oscura (el expediente se lee encima); lo que
cuenta la escena va en la mitad de abajo.

  historia1  La hija perfecta  - Santiago, urbanización, noche de lluvia: una ventana encendida, farola.
  historia2  Noche de verano   - Costa gallega, fiestas, madrugada: guirnalda, faro, cala, primer azul.
  historia3  Humo y silencio   - Olivar en Jaén, octubre: hileras de olivos, cortijo y la columna de humo.

Las cabeceras (historiaN_cabecera.png) NO se generan: con el fondo a pantalla completa repetirían la escena
encima del expediente. Quedan para un artista (ART-NEEDED.md).
Uso: python Tools/make_story_art.py   (determinista: misma semilla, mismos archivos)
"""
import math
import os

import numpy as np
from PIL import Image, ImageDraw

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'Assets', 'Art', 'Stories')
os.makedirs(OUT, exist_ok=True)

W, H = 270, 480      # Intro (x4)
SCALE = 4

BAYER = (np.array([[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]) / 16.0) - 0.5


def hexes(*codes):
    return np.array([[int(c[i:i + 2], 16) for i in (1, 3, 5)] for c in codes], dtype=float)


def quantize(img, palette, strength=28.0):
    """Paleta limitada con tramado ordenado (img float HxWx3 en 0-255)."""
    h, w, _ = img.shape
    tile = np.tile(BAYER, (h // 4 + 1, w // 4 + 1))[:h, :w]
    dithered = img + tile[..., None] * strength
    dist = ((dithered[:, :, None, :] - palette[None, None, :, :]) ** 2).sum(-1)
    return palette[dist.argmin(-1)]


def vgradient(h, w, stops):
    """Degradado vertical: stops = [(y 0-1, (r,g,b)), ...]."""
    ys = np.linspace(0, 1, h)
    out = np.zeros((h, w, 3))
    for c in range(3):
        out[:, :, c] = np.interp(ys, [s[0] for s in stops], [s[1][c] for s in stops])[:, None]
    return out


def value_noise(h, w, cell, rng):
    gh, gw = h // cell + 2, w // cell + 2
    grid = rng.random((gh, gw))
    ys, xs = np.mgrid[0:h, 0:w] / cell
    y0, x0 = ys.astype(int), xs.astype(int)
    fy, fx = ys - y0, xs - x0
    fy, fx = fy * fy * (3 - 2 * fy), fx * fx * (3 - 2 * fx)
    a = grid[y0, x0] * (1 - fx) + grid[y0, x0 + 1] * fx
    b = grid[y0 + 1, x0] * (1 - fx) + grid[y0 + 1, x0 + 1] * fx
    return a * (1 - fy) + b * fy


def paint(img, mask, color, alpha=1.0):
    img[mask] = img[mask] * (1 - alpha) + np.array(color, dtype=float) * alpha


def poly_mask(h, w, points):
    m = Image.new('L', (w, h), 0)
    ImageDraw.Draw(m).polygon([(float(x), float(y)) for x, y in points], fill=255)
    return np.array(m) > 0


def ellipse_mask(h, w, box):
    m = Image.new('L', (w, h), 0)
    ImageDraw.Draw(m).ellipse(box, fill=255)
    return np.array(m) > 0


def glow(h, w, cx, cy, radius, power=2.0):
    ys, xs = np.mgrid[0:h, 0:w]
    d = np.sqrt((xs - cx) ** 2 + (ys - cy) ** 2) / radius
    return np.clip(1 - d, 0, 1) ** power


def save(arr, name, size):
    im = Image.fromarray(np.clip(arr, 0, 255).astype(np.uint8), 'RGB')
    im = im.resize((size[0] * SCALE, size[1] * SCALE), Image.NEAREST)
    im.save(os.path.join(OUT, name))
    print('ok', name, im.size)


# ---------------------------------------------------------------- Historia 1: lluvia en la urbanización

def story1(h, w, horizon, seed=1):
    rng = np.random.default_rng(seed)
    img = vgradient(h, w, [(0, (8, 10, 16)), (0.55, (18, 24, 38)), (1, (22, 28, 40))])
    # Nubes bajas
    clouds = value_noise(h, w, 22, rng) * 0.6 + value_noise(h, w, 9, rng) * 0.4
    sky = np.arange(h)[:, None] < horizon
    soft = np.clip((clouds - 0.45) * 2.2, 0, 1) * sky
    img = img * (1 - soft[..., None] * 0.5) + np.array([30, 38, 56]) * soft[..., None] * 0.5

    # Casas: tejados a dos aguas, la del centro con una ventana encendida arriba
    base = horizon
    houses = [(-10, 70, 38), (60, 150, 52), (150, 230, 44), (228, 300, 36)]
    for i, (x0, x1, roof) in enumerate(houses):
        top = base - roof
        mid = (x0 + x1) / 2
        body = poly_mask(h, w, [(x0, base), (x0, top), (mid, top - 26), (x1, top), (x1, base)])
        paint(img, body, (12, 14, 20))
        if i == 1:
            # Ventana de Elena (arriba) y la del salón a oscuras
            win = poly_mask(h, w, [(mid - 9, top + 8), (mid + 9, top + 8), (mid + 9, top + 22), (mid - 9, top + 22)])
            img += glow(h, w, mid, top + 15, 40, 2.5)[..., None] * np.array([120, 90, 40])
            paint(img, win, (236, 188, 102))
            paint(img, poly_mask(h, w, [(mid - 1, top + 8), (mid + 1, top + 8), (mid + 1, top + 22), (mid - 1, top + 22)]), (60, 44, 24))
            paint(img, poly_mask(h, w, [(mid - 9, top + 14), (mid + 9, top + 14), (mid + 9, top + 15), (mid - 9, top + 15)]), (60, 44, 24))
            # Silueta tras la cortina
            paint(img, ellipse_mask(h, w, (mid + 2, top + 10, mid + 7, top + 15)), (140, 100, 50), 0.8)
            door = poly_mask(h, w, [(mid - 6, base), (mid - 6, base - 18), (mid + 6, base - 18), (mid + 6, base)])
            paint(img, door, (20, 22, 30))
        else:
            for wx in (x0 + 12, x1 - 22):
                wm = poly_mask(h, w, [(wx, top + 10), (wx + 10, top + 10), (wx + 10, top + 20), (wx, top + 20)])
                paint(img, wm, (22, 26, 36))
    # Setos y verja
    paint(img, poly_mask(h, w, [(0, base + 4), (w, base + 4), (w, base - 6), (0, base - 6)]) & (value_noise(h, w, 4, rng) > 0.35), (14, 22, 20))

    # Calle mojada con reflejos
    street = np.arange(h)[:, None] > base + 4
    img[street[:, 0]] = np.array([16, 18, 26]) + 0 * img[street[:, 0]]
    lamp_x, lamp_top = int(w * 0.83), base - 72
    paint(img, poly_mask(h, w, [(lamp_x - 1, base + 30), (lamp_x - 1, lamp_top), (lamp_x + 1, lamp_top), (lamp_x + 1, base + 30)]), (52, 56, 68))
    paint(img, poly_mask(h, w, [(lamp_x - 10, lamp_top), (lamp_x + 2, lamp_top), (lamp_x + 2, lamp_top + 3), (lamp_x - 10, lamp_top + 3)]), (30, 32, 40))
    cone = poly_mask(h, w, [(lamp_x - 8, lamp_top + 3), (lamp_x - 40, base + 60), (lamp_x + 24, base + 60)])
    fade = np.clip(1 - (np.arange(h)[:, None] - lamp_top) / 180.0, 0, 1) * np.ones((1, w))
    img[cone] += (fade[cone][:, None] * np.array([46, 38, 20]))
    img += glow(h, w, lamp_x - 8, lamp_top + 4, 26, 2)[..., None] * np.array([200, 160, 80])
    # Reflejos verticales en el asfalto
    for rx, col, strength in ((lamp_x - 8, (200, 160, 80), 0.5), (houses[1][0] + 45, (236, 188, 102), 0.35)):
        ys = np.arange(base + 8, h)
        for y in ys:
            if rng.random() < 0.55:
                span = int(2 + (y - base) * 0.05)
                x = int(rx + rng.integers(-span, span + 1))
                if 0 <= x < w:
                    img[y, x] = img[y, x] * (1 - strength) + np.array(col) * strength

    # Lluvia
    for _ in range(int(w * h * 0.004)):
        x, y = rng.integers(0, w), rng.integers(0, h)
        for k in range(rng.integers(3, 7)):
            yy, xx = y + k, x - k // 3
            if 0 <= yy < h and 0 <= xx < w:
                img[yy, xx] = img[yy, xx] * 0.55 + np.array([120, 140, 170]) * 0.45

    palette = hexes('#07090f', '#0d1018', '#141a26', '#1c2436', '#28324a', '#3a4660', '#566482', '#7c8aa6',
                    '#14201c', '#3c2c18', '#8a6630', '#c89a4c', '#ecbc66', '#f4dca0')
    return quantize(img, palette)


# ---------------------------------------------------------------- Historia 2: fiestas en la costa

def story2(h, w, horizon, seed=2):
    rng = np.random.default_rng(seed)
    img = vgradient(h, w, [(0, (10, 8, 22)), (horizon / h * 0.7, (34, 22, 52)), (horizon / h, (120, 62, 60)),
                           (horizon / h + 0.001, (18, 22, 40)), (1, (8, 10, 20))])
    # Estrellas en lo alto
    for _ in range(45):
        x, y = rng.integers(0, w), rng.integers(0, int(horizon * 0.6))
        img[y, x] = (130, 122, 160) if rng.random() < 0.3 else (70, 64, 96)  # Tenues: hay texto encima

    # Mar: franjas de luz del amanecer
    for y in range(horizon + 1, h):
        t = (y - horizon) / (h - horizon)
        for _ in range(int(3 + 20 * (1 - t))):
            if rng.random() < 0.5 * (1 - t) + 0.1:
                x = int(w * 0.35 + rng.normal(0, 18 + 60 * t))
                ln = int(rng.integers(2, 6 + 8 * t))
                img[y, max(0, x):min(w, x + ln)] = img[y, max(0, x):min(w, x + ln)] * 0.4 + np.array([170, 96, 80]) * 0.6

    # Acantilado de la cala a la derecha con el faro
    cliff = [(w, horizon + 60), (w, horizon - 40), (w - 30, horizon - 44), (w - 58, horizon - 30), (w - 80, horizon - 18),
             (w - 96, horizon + 4), (w - 120, horizon + 30), (w - 150, horizon + 70), (w - 160, horizon + 120), (w, horizon + 120)]
    paint(img, poly_mask(h, w, cliff), (12, 10, 18))
    fx, fy = w - 34, horizon - 68
    paint(img, poly_mask(h, w, [(fx - 5, horizon - 43), (fx - 3, fy), (fx + 3, fy), (fx + 5, horizon - 43)]), (40, 36, 50))
    paint(img, poly_mask(h, w, [(fx - 4, fy), (fx + 4, fy), (fx + 4, fy - 6), (fx - 4, fy - 6)]), (240, 210, 140))
    beam = poly_mask(h, w, [(fx - 2, fy - 3), (0, fy - 50), (0, fy + 14)])
    bfade = (np.arange(w)[None, :] / fx) * np.ones((h, 1)) * 0.22
    img[beam] = img[beam] * (1 - bfade[beam][:, None]) + np.array([200, 170, 120]) * bfade[beam][:, None]
    img += glow(h, w, fx, fy - 3, 20)[..., None] * np.array([200, 170, 110])

    # Paseo y bar de la fiesta (izquierda), con la guirnalda de bombillas
    ground = poly_mask(h, w, [(0, h), (0, horizon + 70), (80, horizon + 80), (150, horizon + 110), (175, h)])
    paint(img, ground, (14, 12, 20))
    bar = poly_mask(h, w, [(4, horizon + 80), (4, horizon + 40), (66, horizon + 40), (66, horizon + 80)])
    paint(img, bar, (30, 24, 36))
    paint(img, poly_mask(h, w, [(0, horizon + 42), (35, horizon + 24), (72, horizon + 42)]), (58, 34, 40))
    for wx in (12, 42):
        paint(img, poly_mask(h, w, [(wx, horizon + 50), (wx + 12, horizon + 50), (wx + 12, horizon + 62), (wx, horizon + 62)]), (200, 130, 70))
        paint(img, poly_mask(h, w, [(wx + 5, horizon + 50), (wx + 7, horizon + 50), (wx + 7, horizon + 62), (wx + 5, horizon + 62)]), (60, 34, 30))
    paint(img, poly_mask(h, w, [(29, horizon + 80), (29, horizon + 60), (37, horizon + 60), (37, horizon + 80)]), (14, 10, 16))
    img += glow(h, w, 35, horizon + 56, 44, 2.2)[..., None] * np.array([110, 60, 30])
    colors = [(236, 190, 90), (220, 90, 70), (120, 170, 210), (240, 230, 190)]
    for y0 in (horizon - 20, horizon + 6):
        for i, x in enumerate(range(-4, 190, 9)):
            sag = 18 * math.sin(math.pi * ((x + 4) % 96) / 96)
            y = int(y0 + sag)
            if 0 <= y < h and 0 <= x < w:
                c = colors[(i + y0) % len(colors)]
                img[y, x] = c
                img[max(0, y - 1):y + 2, max(0, x - 1):x + 2] = img[max(0, y - 1):y + 2, max(0, x - 1):x + 2] * 0.6 + np.array(c) * 0.4
    # Una figura sola caminando hacia la cala
    px, py = 128, horizon + 98
    paint(img, poly_mask(h, w, [(px - 2, py), (px - 2, py - 10), (px, py - 13), (px + 2, py - 10), (px + 2, py)]), (6, 6, 10))
    paint(img, ellipse_mask(h, w, (px - 2, py - 17, px + 2, py - 13)), (6, 6, 10))

    palette = hexes('#08070f', '#120e1e', '#1e1630', '#2e2044', '#4a2e52', '#78404e', '#a85c50', '#d08a64',
                    '#101628', '#1a2440', '#28304a', '#f0d28c', '#dc5a46', '#78aad2', '#f0e6be', '#b46e3c')
    return quantize(img, palette)


# ---------------------------------------------------------------- Historia 3: el olivar y el humo

def story3(h, w, horizon, seed=3):
    rng = np.random.default_rng(seed)
    img = vgradient(h, w, [(0, (14, 10, 8)), (horizon / h * 0.78, (40, 24, 14)), (horizon / h, (168, 104, 48)),
                           (horizon / h + 0.001, (40, 34, 20)), (1, (18, 14, 10))])
    # Sol bajo detrás del humo
    sx, sy = int(w * 0.3), horizon - 30
    img += glow(h, w, sx, sy, 70, 1.8)[..., None] * np.array([120, 70, 20])
    paint(img, ellipse_mask(h, w, (sx - 11, sy - 11, sx + 11, sy + 11)), (240, 190, 110))

    # Lomas en capas
    for k, (amp, off, col) in enumerate([(10, -26, (70, 48, 28)), (14, -10, (52, 40, 24)), (8, 0, (38, 32, 18))]):
        xs = np.arange(w)
        ridge = horizon + off + amp * np.sin(xs / (30 + 12 * k) + k) + 4 * np.sin(xs / 7 + k * 2)
        mask = np.arange(h)[:, None] > ridge[None, :]
        paint(img, mask, col)

    # Cortijo encalado con la camioneta
    cx, cy = int(w * 0.66), horizon + 2
    paint(img, poly_mask(h, w, [(cx, cy), (cx, cy - 18), (cx + 40, cy - 18), (cx + 40, cy)]), (170, 150, 120))
    paint(img, poly_mask(h, w, [(cx - 3, cy - 18), (cx + 20, cy - 28), (cx + 43, cy - 18)]), (110, 60, 40))
    for wx in (cx + 6, cx + 28):
        paint(img, poly_mask(h, w, [(wx, cy - 12), (wx + 5, cy - 12), (wx + 5, cy - 6), (wx, cy - 6)]), (40, 30, 20))
    tx = cx - 26
    paint(img, poly_mask(h, w, [(tx, cy), (tx, cy - 6), (tx + 8, cy - 6), (tx + 10, cy - 11), (tx + 18, cy - 11), (tx + 20, cy - 6), (tx + 22, cy - 6), (tx + 22, cy)]), (30, 24, 20))

    # Columna de humo negro desde el quemadero (izquierda)
    qx, qy = int(w * 0.18), horizon + 6
    srng = np.random.default_rng(33)
    for i in range(240):
        t = (i / 240) ** 0.85
        y = qy - t * (horizon - 10) + srng.normal(0, 3 + 8 * t)
        x = qx + 22 * math.sin(t * 5.0) * t + 70 * t * t + srng.normal(0, 3 + 14 * t)
        r = 3 + 16 * t + srng.random() * 10 * t
        blob = ellipse_mask(h, w, (x - r, y - r * 0.8, x + r, y + r * 0.8))
        paint(img, blob, (24, 20, 18) if srng.random() < 0.7 else (54, 44, 38), 0.18 * (1 - t * 0.5))
    img += glow(h, w, qx, qy, 14, 2)[..., None] * np.array([180, 70, 20])

    # Hileras de olivos en perspectiva (más grandes y más separados cuanto más cerca)
    vx = w * 0.5
    for row in range(10):
        t = (row + 1) / 10
        y = horizon + 10 + (h - horizon - 10) * t ** 1.6
        size = 3 + 22 * t ** 1.6
        spacing = 18 + 90 * t ** 1.6
        start = (vx - spacing * 6) + (row % 2) * spacing / 2
        for i in range(14):
            x = start + i * spacing + rng.normal(0, 1.5)
            if -size < x < w + size:
                paint(img, poly_mask(h, w, [(x - 1, y), (x - 1, y - size * 0.6), (x + 1, y - size * 0.6), (x + 1, y)]), (30, 22, 14))
                crown = ellipse_mask(h, w, (x - size, y - size * 1.5, x + size, y - size * 0.4))
                crown &= value_noise(h, w, max(2, int(size / 4)), rng) > 0.3
                paint(img, crown, (46, 54, 30) if row % 2 else (38, 46, 26))
                paint(img, crown & (np.arange(w)[None, :] < x - size * 0.2) & (np.arange(h)[:, None] < y - size * 0.9), (86, 92, 52), 0.6)
                paint(img, ellipse_mask(h, w, (x - size * 1.1, y - 2, x + size * 1.1, y + size * 0.25)), (22, 18, 10), 0.6)

    palette = hexes('#0e0a08', '#1c140e', '#2e2016', '#44301c', '#6a4424', '#a4682e', '#d49a4c', '#f0be6e',
                    '#26221c', '#262e1a', '#3c4624', '#5c6436', '#aa9678', '#6e3c28', '#b44616')
    return quantize(img, palette)


SCENES = {'1': story1, '2': story2, '3': story3}

for sid, scene in SCENES.items():
    save(scene(H, W, int(H * 0.74)), f'historia{sid}_intro.png', (W, H))
