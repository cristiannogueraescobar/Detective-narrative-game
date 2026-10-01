"""Medidas de estilo y posproceso de los retratos generados (docs/art/javier/BRIEF.md, sección 3).
Funciona sobre la salida de Tools/remove_white_bg.py (RGBA 768x1024, figura al 90 %, pies a 40 px)."""
import numpy as np
from PIL import Image
from scipy import ndimage

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
