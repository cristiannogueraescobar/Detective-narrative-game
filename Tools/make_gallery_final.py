"""Capturas finales tras las fusiones del 02-10-2026: interrogatorio y acusación de las tres historias, a 1080 × 1920 y
1080 × 2400, desde main (AnimationCapture.RetratosHistoria1-3).
Uso: python Tools/make_gallery_final.py <carpeta anim> <salida.jpg>
"""
import os
import sys

from PIL import Image, ImageDraw, ImageFont

SCREENS = [
    ('Historia 1 · interrogatorio (Daniel, enfadado)', 'retratos_1_daniel_enfadado'),
    ('Historia 1 · acusación', 'acusacion_1'),
    ('Historia 2 · interrogatorio (Marcos, triste)', 'retratos_2_marcos_triste'),
    ('Historia 2 · acusación', 'acusacion_2'),
    ('Historia 3 · interrogatorio (Javier, nervioso)', 'retratos_3_javier_nervioso'),
    ('Historia 3 · acusación', 'acusacion_3'),
]
SIZES = [('', '1080 × 1920'), ('_1080x2400', '1080 × 2400')]
ROW_H = 900
PAD = 14


def font(size):
    try:
        return ImageFont.truetype('arialbd.ttf', size)
    except OSError:
        return ImageFont.load_default()


def main(folder, out):
    rows = []
    for suffix, label in SIZES:
        cells = []
        for _, base in SCREENS:
            im = Image.open(os.path.join(folder, base + suffix + '.png')).convert('RGB')
            cells.append(im.resize((round(im.width * ROW_H / im.height), ROW_H), Image.LANCZOS))
        rows.append((label, cells))
    col_w = [max(r[1][c].width for r in rows) for c in range(len(SCREENS))]
    width = PAD + sum(w + PAD for w in col_w)
    height = 60 + sum(40 + ROW_H + PAD for _ in rows) + 40
    img = Image.new('RGB', (width, height), (24, 24, 28))
    d = ImageDraw.Draw(img)
    d.text((PAD, 12), 'main tras las fusiones (02-10-2026): las tres historias', fill=(217, 164, 65), font=font(30))
    y = 60
    x = PAD
    for c, (title, _) in enumerate(SCREENS):
        d.text((x, y), title, fill=(232, 226, 214), font=font(17))
        x += col_w[c] + PAD
    y += 40
    for label, cells in rows:
        d.text((PAD, y), label, fill=(217, 164, 65), font=font(22))
        y += 34
        x = PAD
        for c, im in enumerate(cells):
            img.paste(im, (x, y))
            x += col_w[c] + PAD
        y += ROW_H + PAD
    img.save(out, quality=88)
    print(out, img.size)


if __name__ == '__main__':
    main(sys.argv[1], sys.argv[2])
