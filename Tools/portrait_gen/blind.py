"""Hojas para las pruebas ciegas de los retratos nuevos (criterios de aprobado de la sesión B).

- estilo: 4 figuras a la misma escala (Marcos, Lucía, Álex y el candidato) en orden aleatorio, sin nombres, solo con
  letras A-D. Se pregunta a un subagente sin contexto "¿cuál no pertenece al mismo juego y por qué?".
- parecido: el candidato junto a Marcos, sin nombres. Se pregunta "¿son la misma persona?".
La clave (qué letra es cada uno) se guarda aparte, en <hoja>.clave.json, y no se enseña al subagente.

Uso: python Tools/portrait_gen/blind.py estilo <candidato.png> <salida.png> <semilla>
     python Tools/portrait_gen/blind.py parecido <candidato.png> <salida.png>"""
import json
import os
import random
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.abspath(os.path.join(HERE, '..', '..'))
sys.path[:0] = [HERE, os.path.join(REPO, 'Tools')]

from PIL import Image, ImageDraw, ImageFont  # noqa: E402

import remove_white_bg as rwb  # noqa: E402

ORIGINALS = {'Marcos': 'Assets/Images/Suspects/duenio_bar.gif.png', 'Lucía': 'Assets/Images/Suspects/madre.gif.png',
             'Álex': 'Assets/Images/Suspects/hermano.gif.png'}
BG = (96, 96, 104)


def normalized(path):
    """Todos por el mismo camino (figura al 90 % de 1024, pies a 40 px): misma escala."""
    im = Image.open(path).convert('RGBA')
    if path.replace('\\', '/').endswith('.gif.png'):
        white = Image.new('RGBA', im.size, (255, 255, 255, 255))
        white.alpha_composite(im)
        im = rwb.process(white.convert('RGB'))[0]
    tile = Image.new('RGBA', (768, 1024), BG + (255,))
    tile.alpha_composite(im)
    return tile.convert('RGB')


def font(size):
    for name in ('arialbd.ttf', 'DejaVuSans-Bold.ttf'):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            pass
    return ImageFont.load_default()


def sheet(items, out):
    """items: [(etiqueta visible, imagen)] → una fila a escala 1/2, etiqueta debajo."""
    w, h = 384, 512
    canvas = Image.new('RGB', (len(items) * (w + 12) - 12, h + 46), (24, 24, 30))
    d = ImageDraw.Draw(canvas)
    for i, (label, im) in enumerate(items):
        canvas.paste(im.resize((w, h), Image.NEAREST), (i * (w + 12), 0))
        d.text((i * (w + 12) + w // 2 - 10, h + 8), label, fill=(240, 220, 160), font=font(30))
    canvas.save(out)


def style_test(candidate, out, seed):
    figures = [(name, normalized(os.path.join(REPO, p))) for name, p in ORIGINALS.items()]
    figures.append(('candidato', normalized(candidate)))
    random.Random(seed).shuffle(figures)
    letters = 'ABCD'
    sheet([(letters[i], im) for i, (_, im) in enumerate(figures)], out)
    with open(out + '.clave.json', 'w', encoding='utf-8') as f:
        json.dump({letters[i]: name for i, (name, _) in enumerate(figures)}, f, ensure_ascii=False)
    return {letters[i]: name for i, (name, _) in enumerate(figures)}


def likeness_test(candidate, out):
    sheet([('1', normalized(os.path.join(REPO, ORIGINALS['Marcos']))), ('2', normalized(candidate))], out)


if __name__ == '__main__':
    kind = sys.argv[1]
    if kind == 'estilo':
        print(style_test(sys.argv[2], sys.argv[3], int(sys.argv[4])))
    else:
        likeness_test(sys.argv[2], sys.argv[3])
        print(sys.argv[3])
