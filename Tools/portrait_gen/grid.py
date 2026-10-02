"""Hoja de revisión: todos los candidatos de una carpeta en rejilla, con su semilla y su puntuación (scores.csv).
Los descartados (fondo no liso, sin figura) salen con su imagen generada y el motivo en rojo.
Uso: python Tools/portrait_gen/grid.py <carpeta de salida de generate.py> [columnas] [subcarpeta=final]
     → <carpeta>/review_<n>[_<subcarpeta>].png  (p. ej. subcarpeta color: tras colour.py)"""
import csv
import os
import sys

from PIL import Image, ImageDraw, ImageFont

TILE_W, TILE_H, LABEL = 192, 256, 40


def font(size):
    for name in ('arialbd.ttf', 'DejaVuSans-Bold.ttf'):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            pass
    return ImageFont.load_default()


def main(folder, columns=8, sub='final'):
    scores = {}
    path_csv = os.path.join(folder, 'scores.csv')
    if os.path.exists(path_csv):
        with open(path_csv, encoding='utf-8') as f:
            scores = {r['seed']: r for r in csv.DictReader(f)}
    seeds = sorted(os.path.splitext(n)[0] for n in os.listdir(os.path.join(folder, 'raw')) if n.endswith('.png'))
    rows = (len(seeds) + columns - 1) // columns
    sheet = Image.new('RGB', (columns * TILE_W, rows * (TILE_H + LABEL)), (24, 24, 30))
    draw = ImageDraw.Draw(sheet)
    for i, seed in enumerate(seeds):
        x, y = (i % columns) * TILE_W, (i // columns) * (TILE_H + LABEL)
        final = os.path.join(folder, sub, f'{seed}.png')
        tile = Image.new('RGBA', (768, 1024), (96, 96, 104, 255))
        if os.path.exists(final):
            tile.alpha_composite(Image.open(final).convert('RGBA'))
        else:  # Descartado antes del posproceso: se ve lo generado
            tile = Image.open(os.path.join(folder, 'raw', f'{seed}.png')).convert('RGBA').resize((768, 1024))
        sheet.paste(tile.resize((TILE_W, TILE_H), Image.NEAREST).convert('RGB'), (x, y))
        r = scores.get(seed, {})
        bad = r.get('warnings') and float(r.get('score', 0)) >= 999
        text = f"{seed}  {float(r['score']):.1f}" if r.get('score') else seed
        draw.text((x + 6, y + TILE_H + 4), text, fill=(240, 220, 160), font=font(18))
        if bad:
            draw.text((x + 6, y + TILE_H + 22), r['warnings'][:22], fill=(230, 90, 80), font=font(13))
    out = os.path.join(folder, f'review_{len(seeds)}' + ('' if sub == 'final' else f'_{sub}') + '.png')
    sheet.save(out)
    print(out, sheet.size, len(seeds), 'candidatos')


if __name__ == '__main__':
    main(sys.argv[1], int(sys.argv[2]) if len(sys.argv) > 2 else 8, sys.argv[3] if len(sys.argv) > 3 else 'final')
