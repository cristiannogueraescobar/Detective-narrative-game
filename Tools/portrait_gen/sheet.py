"""Hoja de contacto: candidatos junto a Marcos (02) y Lucía (03) a la misma escala (todos normalizados por
Tools/remove_white_bg.py a 768x1024, figura al 90 %), con la cara ampliada debajo.
Uso: python Tools/portrait_gen/sheet.py salida.png ruta1.png[=etiqueta] ruta2.png[=etiqueta] ...
("=" y no ":", que choca con la letra de unidad de Windows)"""
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.abspath(os.path.join(HERE, '..', '..'))
sys.path[:0] = [HERE, os.path.join(REPO, 'Tools')]

from PIL import Image, ImageDraw, ImageFont  # noqa: E402

import remove_white_bg as rwb  # noqa: E402

REFS = [('Assets/Images/Suspects/duenio_bar.gif.png', 'Marcos (02)'),
        ('Assets/Images/Suspects/madre.gif.png', 'Lucía (03)')]
SCALE = 0.5   # 768x1024 → 384x512 por figura
FACE = (230, 60, 540, 320)  # Zona de la cabeza en el lienzo de 768x1024 (figura al 90 %, pies a 40 px)


def font(size):
    for name in ('arialbd.ttf', 'DejaVuSans-Bold.ttf'):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            pass
    return ImageFont.load_default()


def normalized_reference(path):
    im = Image.open(os.path.join(REPO, path)).convert('RGBA')
    white = Image.new('RGBA', im.size, (255, 255, 255, 255))
    white.alpha_composite(im)
    return rwb.process(white.convert('RGB'))[0]


def tile(rgba, label, bg=(96, 96, 104)):
    w, h = int(768 * SCALE), int(1024 * SCALE)
    body = Image.new('RGBA', (768, 1024), bg + (255,))
    body.alpha_composite(rgba)
    face = body.crop(FACE).resize((w, int(w * (FACE[3] - FACE[1]) / (FACE[2] - FACE[0]))), Image.NEAREST)
    out = Image.new('RGB', (w, h + face.height + 34), (24, 24, 30))
    out.paste(body.resize((w, h), Image.NEAREST).convert('RGB'), (0, 0))
    out.paste(face.convert('RGB'), (0, h))
    ImageDraw.Draw(out).text((6, h + face.height + 6), label, fill=(240, 220, 160), font=font(20))
    return out


def main(out_path, items):
    tiles = [tile(normalized_reference(p), label) for p, label in REFS]
    for item in items:
        path, _, label = item.partition('=')
        tiles.append(tile(Image.open(path).convert('RGBA'), label or os.path.basename(path)))
    gap = 8
    sheet = Image.new('RGB', (sum(t.width for t in tiles) + gap * (len(tiles) - 1), tiles[0].height), (0, 0, 0))
    x = 0
    for t in tiles:
        sheet.paste(t, (x, 0))
        x += t.width + gap
    sheet.save(out_path)
    print(out_path, sheet.size)


if __name__ == '__main__':
    main(sys.argv[1], sys.argv[2:])
