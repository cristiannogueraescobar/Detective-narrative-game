"""Galería de la Sesión A (docs/screenshots/2026-10-01/galeria/).
- retratos_12.jpg: los 12 personajes de las tres historias en el interrogatorio, a tamaño real del juego (zona del
  retrato de cada captura retrato_<artId>.png, ampliada x2 sin suavizar).
- retratos_antes_despues.jpg: los cinco que compartían retrato (y Amparo, envejecida), antes y ahora.
Uso: python Tools/make_gallery_sesion_a.py
"""
import os

from PIL import Image, ImageDraw, ImageFont

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..')
BASE = os.path.join(ROOT, 'docs', 'screenshots', '2026-10-01')
ANIM = os.path.join(BASE, 'anim')
OUT = os.path.join(BASE, 'galeria')
CROP = (12, 82, 168, 285)  # Zona del retrato en la captura de 540 x 960

STORIES = [
    ('Historia 1 · La hija perfecta', [('daniel', 'Daniel'), ('carmen', 'Carmen'), ('lucas', 'Lucas'), ('rosario', 'Amparo')]),
    ('Historia 2 · Noche de verano', [('marcos', 'Marcos'), ('andres', 'Andrés'), ('ruiz', 'Ruiz'), ('maruxa', 'Maruxa')]),
    ('Historia 3 · Humo y silencio', [('javier', 'Javier'), ('lucia', 'Lucía'), ('alex', 'Álex'), ('encarna', 'Encarna')]),
]


def font(size):
    for name in ('arialbd.ttf', 'DejaVuSans-Bold.ttf'):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            pass
    return ImageFont.load_default()


def cell(art_id):
    im = Image.open(os.path.join(ANIM, f'retrato_{art_id}.png')).convert('RGB').crop(CROP)
    return im.resize((im.width * 2, im.height * 2), Image.NEAREST)


def main():
    os.makedirs(OUT, exist_ok=True)
    w, h = (CROP[2] - CROP[0]) * 2, (CROP[3] - CROP[1]) * 2
    gap, band, label = 14, 44, 34
    sheet = Image.new('RGB', (4 * w + 5 * gap, 3 * (band + h + label) + gap), (24, 24, 30))
    d = ImageDraw.Draw(sheet)
    y = gap
    for title, people in STORIES:
        d.text((gap, y + 8), title, fill=(232, 200, 120), font=font(26))
        y += band
        for i, (art, name) in enumerate(people):
            x = gap + i * (w + gap)
            sheet.paste(cell(art), (x, y))
            d.text((x + 6, y + h + 4), name, fill=(232, 226, 214), font=font(24))
        y += h + label
    sheet.save(os.path.join(OUT, 'retratos_12.jpg'), quality=88)

    # Antes (compartían el retrato de otro) y ahora
    pairs = [('daniel', 'javier', 'Javier (antes: el de Daniel)'), ('carmen', 'lucia', 'Lucía (antes: el de Carmen)'),
             ('lucas', 'alex', 'Álex (antes: el de Lucas)')]
    rows = Image.new('RGB', (2 * w + 3 * gap, len(pairs) * (h + label) + 40), (24, 24, 30))
    d = ImageDraw.Draw(rows)
    d.text((gap, 8), 'ANTES                         AHORA', fill=(232, 200, 120), font=font(22))
    y = 40
    for before, after, text in pairs:
        rows.paste(cell(before), (gap, y))
        rows.paste(cell(after), (2 * gap + w, y))
        d.text((gap + 4, y + h + 4), text, fill=(232, 226, 214), font=font(20))
        y += h + label
    rows.save(os.path.join(OUT, 'retratos_antes_despues.jpg'), quality=88)
    print('ok')


if __name__ == '__main__':
    main()
