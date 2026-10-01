"""Icono de la aplicación (archivo NUEVO: Assets/Art/Icons/app_icon.png, 1024 px): lupa ámbar con una
interrogación sobre fondo noir con viñeta. Paleta del tema (ámbar #D9A441, hueso #E8E2D6, fondo #1A1B1F)."""
import math
import os
from PIL import Image, ImageDraw, ImageFilter, ImageFont

S = 1024
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'Assets', 'Art', 'Icons', 'app_icon.png')
AMBER = (217, 164, 65)
BONE = (232, 226, 214)

# Fondo: degradado radial oscuro con luz cálida arriba a la izquierda (la lámpara del despacho)
bg = Image.new('RGB', (S, S))
px = bg.load()
for y in range(S):
    for x in range(S):
        d = math.hypot(x - S * 0.35, y - S * 0.3) / S
        k = max(0.0, 1.0 - d * 1.35)
        r = int(20 + 38 * k * k)
        g = int(21 + 30 * k * k)
        b = int(24 + 14 * k * k)
        px[x, y] = (r, g, b)

layer = Image.new('RGBA', (S * 2, S * 2), (0, 0, 0, 0))  # Supermuestreo para bordes suaves
d = ImageDraw.Draw(layer)
cx, cy, r = S * 0.9, S * 0.86, S * 0.5
d.line([(cx + r * 0.62, cy + r * 0.62), (S * 1.72, S * 1.72)], fill=AMBER + (255,), width=int(S * 0.2))
d.ellipse([cx - r, cy - r, cx + r, cy + r], outline=AMBER + (255,), width=int(S * 0.1))
d.ellipse([cx - r * 0.8, cy - r * 0.8, cx + r * 0.8, cy + r * 0.8], fill=(30, 32, 38, 235))
# Brillo del cristal
d.arc([cx - r * 0.68, cy - r * 0.68, cx + r * 0.68, cy + r * 0.68], start=200, end=250, fill=BONE + (150,), width=int(S * 0.035))

try:
    font = ImageFont.truetype('georgiab.ttf', int(S * 0.62))
except OSError:
    font = ImageFont.load_default()
d.text((cx, cy + S * 0.02), '?', font=font, fill=BONE + (255,), anchor='mm')

shadow = layer.split()[3].filter(ImageFilter.GaussianBlur(24))
shade = Image.new('RGBA', layer.size, (0, 0, 0, 0))
shade.putalpha(shadow.point(lambda a: int(a * 0.6)))
composed = Image.alpha_composite(shade.transform(layer.size, Image.AFFINE, (1, 0, -18, 0, 1, -18)), layer)
icon = bg.convert('RGBA')
icon.alpha_composite(composed.resize((S, S), Image.LANCZOS))
icon.convert('RGB').save(OUT)
icon.convert('RGB').resize((256, 256), Image.LANCZOS).save(os.path.join(os.path.dirname(OUT), '..', '..', '..', '.superpowers', 'icon_preview.png'))
print(os.path.abspath(OUT))
