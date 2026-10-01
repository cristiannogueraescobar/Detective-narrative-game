"""Medidas de estilo y posproceso de los retratos generados (docs/art/javier/BRIEF.md, sección 3).
Funciona sobre la salida de Tools/remove_white_bg.py (RGBA 768x1024, figura al 90 %, pies a 40 px)."""
import os
import sys

import numpy as np
from PIL import Image
from scipy import ndimage

sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..'))  # Tools/: remove_white_bg

# Paleta medida sobre las referencias 02-04 (BRIEF 3) + negro de contorno; el oliva de la camisa de Javier se añade
PALETTE = ['#000000', '#D29F61', '#DD9D53', '#F3CE95', '#EFE0B2', '#FCEDA8', '#A44927', '#B93314', '#7A1A16',
           '#3E1411', '#5C2815', '#37322D', '#182E43', '#2A6581', '#5B5A2E', '#7A7440', '#3F4A24', '#2E5E2A']


def _rgb(hexes):
    return np.array([[int(h[i:i + 2], 16) for i in (1, 3, 5)] for h in hexes], float)


def figure_bounds(alpha):
    ys, xs = np.where(alpha >= 128)
    return ys.min(), ys.max() + 1, xs.min(), xs.max() + 1


def pixel_size(rgba):
    """Píxel efectivo: mediana de los tramos horizontales de color casi igual dentro de la figura (sin el negro)."""
    a = rgba[..., 3] >= 128
    rgb = rgba[..., :3].astype(int)
    y0, y1, _, _ = figure_bounds(rgba[..., 3])
    runs = []
    for y in range(y0 + 5, y1 - 5, 3):
        row, inside, start = rgb[y], a[y], None
        for x in range(1, rgba.shape[1]):
            same = inside[x] and inside[x - 1] and np.abs(row[x] - row[x - 1]).sum() < 18 and row[x].sum() > 90
            if same and start is None:
                start = x - 1
            if not same and start is not None:
                runs.append(x - start)
                start = None
    runs = np.array(runs)
    runs = runs[runs >= 2]
    return float(np.median(runs)) if len(runs) else 0.0


def palette_distance(rgba):
    """Distancia RGB media de los píxeles de la figura al color más cercano de la paleta (menos es mejor)."""
    px = rgba[rgba[..., 3] >= 128][:, :3].astype(float)
    px = px[np.random.default_rng(0).choice(len(px), min(len(px), 20000), replace=False)]
    pal = _rgb(PALETTE)
    return float(np.sqrt(((px[:, None, :] - pal[None]) ** 2).sum(-1)).min(1).mean())


def background_leak(raw):
    """Fracción de la figura que el relleno del fondo se comería por ropa clara sin contorno (caso real: la manga de una
    camisa blanca). Compara el fondo con la tolerancia de remove_white_bg contra el fondo de blanco puro; la diferencia
    fuera de la franja de 3 px del borde (antialiasing) y de la zona de los pies (sombra) es ropa comida."""
    import remove_white_bg as rwb
    rgb = np.asarray(raw.convert('RGB')).astype(int)
    lum, sat = rgb.mean(-1), rgb.max(-1) - rgb.min(-1)

    def flood(candidate):
        labels, _ = ndimage.label(candidate)
        border = np.unique(np.concatenate([labels[0], labels[-1], labels[:, 0], labels[:, -1]]))
        return np.isin(labels, border[border > 0])

    edge = np.concatenate([rgb[0], rgb[-1], rgb[:, 0], rgb[:, -1]]).astype(float)
    bg = np.median(edge, axis=0)
    if bg.min() >= rwb.WHITE_LUM:
        loose = flood((sat <= rwb.BG_SAT) & (lum >= rwb.BG_LUM))
        strict = flood((sat <= 8) & (lum >= 250))
    else:  # Fondo de color: la tolerancia de remove_white_bg frente a "fondo seguro"
        dist = np.sqrt(((rgb.astype(float) - bg) ** 2).sum(-1))
        spread = np.sqrt(((edge - bg) ** 2).sum(-1))
        tol = max(18.0, float(np.percentile(spread, 95)) * 1.5)
        loose = flood(dist <= tol)
        # Fondo seguro = zona lisa unida al borde: entre vecinos, saltos de 4 como mucho. Un degradado suave lo es
        # entero; cualquier borde (ropa, contorno) lo corta. Antes era "a ±6 del color mediano" y un degradado daba
        # fuga 0,07-0,16 (Javier por imagen a imagen) o 0,71 (test)
        f = rgb.astype(float)
        jump = np.zeros(lum.shape)
        jump[:, 1:] = np.maximum(jump[:, 1:], np.abs(np.diff(f, axis=1)).max(-1))
        jump[:, :-1] = np.maximum(jump[:, :-1], np.abs(np.diff(f, axis=1)).max(-1))
        jump[1:] = np.maximum(jump[1:], np.abs(np.diff(f, axis=0)).max(-1))
        jump[:-1] = np.maximum(jump[:-1], np.abs(np.diff(f, axis=0)).max(-1))
        strict = flood(loose & (jump <= 4))
    figure = ~strict
    ys, _ = np.where(figure)
    if len(ys) == 0:
        return 1.0
    feet = int(ys.max() - 0.08 * (ys.max() - ys.min()))
    eaten = loose & ~ndimage.binary_dilation(strict, iterations=3)
    eaten[feet:] = False
    return float(eaten.sum() / figure.sum())


SHADOW_ALPHA = 47  # Opacidad media medida en las sombras de Marcos, Lucía y Álex (46-50/255, ≈18 %)


def add_shadow(image):
    """Sombra suave sintética, como la de los originales 02-04: negra al ≈18 %, a la derecha de cada zapato (la luz
    viene de la izquierda) y a la altura de la suela. Para generados sobre fondo de color, donde la sombra pintada no
    se puede recuperar. Solo pinta donde es transparente y no repite si ya hay sombra."""
    rgba = np.asarray(image.convert('RGBA')).copy()
    alpha = rgba[..., 3]
    opaque = alpha >= 128
    ys, _ = np.where(opaque)
    y0, sole = ys.min(), ys.max() + 1
    fh = sole - y0
    band = slice(max(0, int(sole - 0.03 * fh)), sole)
    if ((alpha[band.start:min(alpha.shape[0], sole + 20)] > 0) & (alpha[band.start:min(alpha.shape[0], sole + 20)] < 255)).sum() > 50:
        return image.convert('RGBA')  # Ya tiene sombra
    cols = np.where(opaque[band].any(axis=0))[0]
    shoes, start = [], cols[0]
    for a, b in zip(cols[:-1], cols[1:]):
        if b - a > 3:
            shoes.append((start, a + 1))
            start = b
    shoes.append((start, cols[-1] + 1))
    shoes = [s for s in shoes if s[1] - s[0] >= 0.02 * rgba.shape[1]]
    yy, xx = np.mgrid[0:rgba.shape[0], 0:rgba.shape[1]]
    top, bottom = sole - 0.012 * fh, sole + 0.008 * fh
    for left, right in shoes:
        w = right - left
        x0, x1 = left + 0.5 * w, right + 0.35 * w
        cx, cy, rx, ry = (x0 + x1) / 2, (top + bottom) / 2, (x1 - x0) / 2, (bottom - top) / 2
        inside = ((xx - cx) / rx) ** 2 + ((yy - cy) / ry) ** 2 <= 1
        paint = inside & (alpha == 0)
        rgba[paint] = (0, 0, 0, SHADOW_ALPHA)
    return Image.fromarray(rgba, 'RGBA')


def cel_flatten(image, cell=5, passes=2, tol=20, majority=6):
    """Sombreado limpio por zonas, como el de los originales: en la rejilla de `cell` px, una celda que difiere de un
    color que comparten al menos `majority` de sus 8 vecinas (±`tol`) toma ese color. Quita las motas sueltas (la textura
    ruidosa del generador) sin mover los bordes entre zonas, donde las vecinas se reparten (p. ej. 3 y 5).
    Prueba ciega 2: "sombreado moteado, rayado en la camisa y el pantalón"."""
    rgba = np.asarray(image.convert('RGBA')).copy()
    h, w = rgba.shape[:2]
    gh, gw = h // cell, w // cell
    small = rgba[:gh * cell:cell, :gw * cell:cell].copy()
    opaque = small[..., 3] == 255
    offsets = [(dy, dx) for dy in (-1, 0, 1) for dx in (-1, 0, 1) if (dy, dx) != (0, 0)]
    for _ in range(passes):
        rgb = small[..., :3].astype(int)
        pad = np.pad(rgb, ((1, 1), (1, 1), (0, 0)), mode='edge')
        pad_op = np.pad(opaque, 1, constant_values=False)
        neigh = [pad[1 + dy:1 + dy + gh, 1 + dx:1 + dx + gw] for dy, dx in offsets]
        neigh_op = [pad_op[1 + dy:1 + dy + gh, 1 + dx:1 + dx + gw] for dy, dx in offsets]
        best_count = np.zeros((gh, gw), int)
        best = rgb.copy()
        for cand in neigh:
            count = sum((np.abs(n - cand).max(-1) <= tol) & o for n, o in zip(neigh, neigh_op))
            better = count > best_count
            best_count = np.where(better, count, best_count)
            best = np.where(better[..., None], cand, best)
        change = opaque & (best_count >= majority) & (np.abs(best - rgb).max(-1) > tol)
        small[change, :3] = best[change]
    rgba[:gh * cell, :gw * cell] = np.repeat(np.repeat(small, cell, axis=0), cell, axis=1)
    return Image.fromarray(rgba, 'RGBA')


def reinforce_outline(image, cell=5, cells=2):
    """Contorno negro grueso como el de los originales (revisión de Cristian: a los generados les faltaba). Sobre la
    rejilla de `cell` px (aplicar después de pixelate):
    1. quita el halo claro del borde (celdas claras pegadas al fondo, de los bordes suavizados del generador);
    2. quita motas sueltas (piezas de menos de 6 celdas);
    3. pinta de negro las `cells` celdas más externas de la silueta: 2 celdas = 10 px ≈ 1,1 % de una figura de 920 px,
       dentro del 1-2,4 % medido (Lucía y Álex ~1 %, Marcos 2,4 %). Con 3, lo fino (botella, dedos) quedaba todo negro.
    La sombra semitransparente no se toca."""
    rgba = np.asarray(image.convert('RGBA')).copy()
    h, w = rgba.shape[:2]
    gh, gw = h // cell, w // cell
    small = rgba[:gh * cell:cell, :gw * cell:cell].copy()
    opaque = small[..., 3] == 255
    eight = np.ones((3, 3), bool)
    for _ in range(2):  # Halo de hasta 2 celdas
        edge = opaque & ndimage.binary_dilation(~opaque, structure=eight)
        light = edge & (small[..., :3].min(-1) > 190)
        small[light] = 0
        opaque &= ~light
    parts, n = ndimage.label(opaque, structure=eight)
    if n:
        sizes = ndimage.sum(opaque, parts, range(1, n + 1))
        tiny = np.isin(parts, [i + 1 for i, s in enumerate(sizes) if s < 6])
        small[tiny] = 0
        opaque &= ~tiny
    inner = ndimage.binary_erosion(opaque, structure=eight, iterations=cells, border_value=0)
    ring = opaque & ~inner
    small[ring, :3] = 0
    big = np.repeat(np.repeat(small, cell, axis=0), cell, axis=1)
    rgba[:gh * cell, :gw * cell] = big
    return Image.fromarray(rgba, 'RGBA')


def components(rgba):
    """Cuántas piezas opacas grandes hay (una figura = 1; más, figuras repetidas o recortes sueltos)."""
    labels, n = ndimage.label(rgba[..., 3] >= 128)
    if n == 0:
        return 0
    sizes = ndimage.sum(np.ones_like(labels), labels, range(1, n + 1))
    return int((sizes > sizes.max() * 0.05).sum())


def pixelate(image, cell=5, colours=40):
    """Rejilla de `cell` px por vecino más cercano y paleta reducida, conservando la sombra semitransparente."""
    rgba = np.asarray(image.convert('RGBA')).astype(float)
    h, w = rgba.shape[:2]
    small_w, small_h = w // cell, h // cell
    a = rgba[..., 3:4] / 255
    premult = Image.fromarray(np.dstack([rgba[..., :3] * a, rgba[..., 3:4]]).clip(0, 255).astype(np.uint8), 'RGBA')
    small = np.asarray(premult.resize((small_w, small_h), Image.BOX)).astype(float)
    alpha = small[..., 3]
    rgb = np.where(alpha[..., None] > 0, small[..., :3] * 255 / np.maximum(alpha[..., None], 1), 0)
    opaque = alpha >= 128
    quant = Image.fromarray(rgb.clip(0, 255).astype(np.uint8)).quantize(colours, method=Image.MEDIANCUT,
                                                                         dither=Image.Dither.NONE).convert('RGB')
    rgb = np.asarray(quant).astype(float)
    out_a = np.where(opaque, 255, np.where(alpha >= 10, alpha, 0))
    rgb = np.where(opaque[..., None], rgb, 0)  # Lo semitransparente es sombra: negro con su alfa
    small_img = Image.fromarray(np.dstack([rgb, out_a]).astype(np.uint8), 'RGBA')
    big = small_img.resize((small_w * cell, small_h * cell), Image.NEAREST)
    canvas = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    canvas.paste(big, (0, 0))
    return canvas


def outline_thickness(rgba):
    """Mediana del grosor del contorno oscuro (lum < 45) entrando desde el borde transparente, en px."""
    a = rgba[..., 3] >= 128
    lum = rgba[..., :3].astype(float) @ [0.299, 0.587, 0.114]
    y0, y1, _, _ = figure_bounds(rgba[..., 3])
    th = []
    for y in range(int(y0 + (y1 - y0) * 0.1), int(y0 + (y1 - y0) * 0.9), 9):
        xs = np.where(a[y])[0]
        if len(xs) == 0:
            continue
        x, k = xs[0], 0
        while x + k < a.shape[1] and a[y, x + k] and lum[y, x + k] < 45:
            k += 1
        if k:
            th.append(k)
    return float(np.median(th)) if th else 0.0


def measure(image):
    rgba = np.asarray(image.convert('RGBA'))
    y0, y1, _, _ = figure_bounds(rgba[..., 3])
    # Sin medida automática de cabeza/cuerpo: el intento (apertura de hombros) falló en 4 de 7 originales. La fija el
    # esqueleto de pose (pose.py) y se comprueba a mano con regla sobre los finalistas.
    return {'pixel': pixel_size(rgba), 'palette': palette_distance(rgba),
            'outline': outline_thickness(rgba) / (y1 - y0), 'parts': components(rgba)}
