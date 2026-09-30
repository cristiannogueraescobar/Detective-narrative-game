"""Galería antes/después para el informe de la noche: parejas lado a lado en JPEG (docs/screenshots/galeria).

Uso: python Tools/make_gallery.py <carpeta de la fecha>   (p. ej. docs/screenshots/2026-09-30)
"""
import os
import sys

from PIL import Image, ImageDraw, ImageFont

base = sys.argv[1] if len(sys.argv) > 1 else 'docs/screenshots/2026-09-30'
out = os.path.join('docs', 'screenshots', 'galeria')
os.makedirs(out, exist_ok=True)

PAIRS = [
    ('menu', '00-inicio/1080x1920_MainMenuPanel.png', 'anim/menu_2500.png'),
    ('interrogatorio', '00-inicio/1080x1920_InterrogationPanel.png', 'anim/chatlargo_abajo.png'),
    ('intro', '00-inicio/1080x1920_IntroPanel.png', 'anim/panel_intro.png'),
    ('acusacion', '00-inicio/1080x1920_AccusatonPanel.png', 'anim/panel_acusacion.png'),
    ('libreta', '00-inicio/1080x1920_CluesPanel.png', 'final/1080x1920_CluesPanel.png'),
    ('ajustes', '00-inicio/1080x1920_SettingsPanel.png', 'anim/panel_ajustes.png'),
    ('instrucciones', '00-inicio/1080x1920_IntructionsPanel.png', 'anim/panel_instrucciones.png'),
    ('final_bueno', '00-inicio/1080x1920_Final_Good.png', 'anim/final_good_7000.png'),
    ('final_malo', '00-inicio/1080x1920_Final_Bad.png', 'anim/final_bad_7000.png'),
]

STRIPS = [
    ('efecto_pista', 'anim/pista_', 8),
    ('efecto_contradiccion', 'anim/contradiccion_', 6),
    ('efecto_dia', 'anim/dia_', 6),
    ('efecto_veredicto', 'anim/veredicto_', 6),
    ('efecto_intro', 'anim/intro_', 4),
]


def load(rel, height=960):
    im = Image.open(os.path.join(base, rel)).convert('RGB')
    return im.resize((int(im.width * height / im.height), height))


try:
    FONT = ImageFont.truetype('arial.ttf', 18)
except OSError:
    FONT = ImageFont.load_default()


def label(img, text):
    d = ImageDraw.Draw(img)
    d.rectangle((0, 0, img.width, 28), fill=(0, 0, 0))
    d.text((8, 4), text, fill=(255, 220, 120), font=FONT)


made = []
for name, before, after in PAIRS:
    try:
        a, b = load(before), load(after)
    except FileNotFoundError as e:
        print('falta', e.filename)
        continue
    sheet = Image.new('RGB', (a.width + b.width + 12, 960), (30, 30, 30))
    sheet.paste(a, (0, 0))
    sheet.paste(b, (a.width + 12, 0))
    label(sheet, f'{name}: antes (izquierda) / después (derecha)')
    path = os.path.join(out, f'{name}.jpg')
    sheet.save(path, quality=85)
    made.append(path)

for name, prefix, count in STRIPS:
    folder = os.path.join(base, os.path.dirname(prefix))
    files = sorted(f for f in os.listdir(folder) if f.startswith(os.path.basename(prefix)) and f.endswith('.png') and '_2400' not in f)[:count]
    if not files:
        continue
    frames = [load(os.path.join(os.path.dirname(prefix), f), 640) for f in files]
    sheet = Image.new('RGB', (sum(f.width + 6 for f in frames), 640), (30, 30, 30))
    x = 0
    for f in frames:
        sheet.paste(f, (x, 0))
        x += f.width + 6
    label(sheet, f'{name}: fotogramas ' + ', '.join(os.path.splitext(f)[0].split('_')[-1] + ' ms' for f in files))
    path = os.path.join(out, f'{name}.jpg')
    sheet.save(path, quality=85)
    made.append(path)

print('\n'.join(made))
