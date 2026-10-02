"""Galería antes/después de los arreglos de interfaz (revisión de Cristian, sesión C).

Misma pantalla y misma historia (la 2, tres sospechosos el primer día) en cuatro tamaños. ANTES = main (b92ad83),
capturado desde un worktree con la misma prueba (AnimationCapture.InterfazHistoria2). AHORA = feature/arreglos-interfaz.
Uso: python Tools/make_gallery_arreglos.py <carpeta antes> <carpeta ahora> <carpeta de salida>
"""
import os
import sys

from PIL import Image, ImageDraw, ImageFont

SIZES = [('', '1080 × 1920 (16:9)'), ('_1080x2400', '1080 × 2400 (20:9)'), ('_1440x3200', '1440 × 3200 (20:9)'),
         ('_1080x3840', '1080 × 3840 (9:32, la proporción de las capturas de retratos)')]
ROW_H = 720
PAD = 16
HEAD = 56
LABEL = 44
BG = (24, 24, 28)
FG = (232, 226, 214)
ACCENT = (217, 164, 65)


def font(size):
    for name in ('arialbd.ttf', 'DejaVuSans-Bold.ttf'):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            continue
    return ImageFont.load_default()


def cell(path):
    if not os.path.exists(path):
        im = Image.new('RGB', (405, ROW_H), (60, 20, 20))
        ImageDraw.Draw(im).text((20, ROW_H // 2), 'sin captura', fill=FG, font=font(28))
        return im
    im = Image.open(path).convert('RGB')
    return im.resize((round(im.width * ROW_H / im.height), ROW_H), Image.LANCZOS)


def sheet(columns, title, out):
    rows = []
    for suffix, label in SIZES:
        rows.append((label, [cell(os.path.join(folder, base + suffix + '.png')) for _, folder, base in columns]))
    col_w = [max(r[1][c].width for r in rows) for c in range(len(columns))]
    width = PAD + sum(w + PAD for w in col_w)
    height = HEAD + LABEL + sum(LABEL + ROW_H + PAD for _ in rows)
    img = Image.new('RGB', (width, height), BG)
    d = ImageDraw.Draw(img)
    d.text((PAD, 12), title, fill=ACCENT, font=font(32))
    x = PAD
    for c, (name, _, _) in enumerate(columns):
        d.text((x, HEAD + 6), name, fill=FG, font=font(28))
        x += col_w[c] + PAD
    y = HEAD + LABEL
    for label, cells in rows:
        d.text((PAD, y + 8), label, fill=ACCENT, font=font(24))
        y += LABEL
        x = PAD
        for c, im in enumerate(cells):
            img.paste(im, (x + (col_w[c] - im.width) // 2, y))
            x += col_w[c] + PAD
        y += ROW_H + PAD
    img.save(out, quality=88)
    print(out, img.size)


if __name__ == '__main__':
    before, after, out = sys.argv[1:4]
    os.makedirs(out, exist_ok=True)
    sheet([('ANTES (main)', before, 'interfaz_interrogatorio'),
           ('AHORA: A, busto', after, 'interfaz_interrogatorio'),
           ('AHORA: B, figura', after, 'interfaz_figura_interrogatorio')],
          'Interrogatorio, historia 2, día 1', os.path.join(out, 'arreglos-interfaz-interrogatorio.jpg'))
    sheet([('ANTES (main)', before, 'interfaz_acusacion'), ('AHORA', after, 'interfaz_acusacion')],
          'Acusación, historia 2, día 1 (tres sospechosos)', os.path.join(out, 'arreglos-interfaz-acusacion.jpg'))
