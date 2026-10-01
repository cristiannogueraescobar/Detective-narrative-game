"""Iconos de línea (color hueso) para los huecos de ArtSlots. Archivos NUEVOS en Assets/Art/Icons."""
import os
from PIL import Image, ImageDraw

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'Assets', 'Art', 'Icons')
os.makedirs(OUT, exist_ok=True)
S = 512
C = (232, 226, 214, 255)
W = 30


def canvas():
    im = Image.new('RGBA', (S, S), (0, 0, 0, 0))
    return im, ImageDraw.Draw(im)


def save(im, name):
    im.resize((128, 128), Image.LANCZOS).save(os.path.join(OUT, name))


# Libreta: tapa con anillas y renglones
im, d = canvas()
d.rounded_rectangle((120, 70, 420, 450), radius=36, outline=C, width=W)
for y in (140, 230, 320, 400):
    d.ellipse((88, y - 22, 150, y + 22), outline=C, width=22)
for y in (170, 240, 310):
    d.line((200, y, 360, y), fill=C, width=22)
save(im, 'libreta.png')

# Pista: lupa
im, d = canvas()
d.ellipse((80, 70, 340, 330), outline=C, width=W + 6)
d.line((310, 300, 440, 430), fill=C, width=W + 26)
d.arc((130, 120, 290, 280), start=200, end=260, fill=C, width=16)
save(im, 'pista.png')

# Día: hoja de calendario
im, d = canvas()
d.rounded_rectangle((80, 100, 432, 440), radius=32, outline=C, width=W)
d.rectangle((80 + W // 2, 170, 432 - W // 2, 196), fill=C)
for x in (170, 342):
    d.rounded_rectangle((x - 16, 60, x + 16, 140), radius=12, fill=C)
for gx in range(3):
    for gy in range(2):
        cx, cy = 170 + gx * 86, 270 + gy * 90
        d.rounded_rectangle((cx - 22, cy - 22, cx + 22, cy + 22), radius=8, fill=C)
save(im, 'dia.png')

# Preguntas: bocadillo con interrogación
im, d = canvas()
d.rounded_rectangle((70, 80, 442, 360), radius=70, outline=C, width=W)
d.polygon([(150, 340), (140, 450), (250, 350)], fill=C)
d.arc((196, 130, 316, 250), start=180, end=400, fill=C, width=26)
d.line((256, 250, 256, 280), fill=C, width=26)
d.ellipse((240, 300, 272, 332), fill=C)
save(im, 'preguntas.png')

# Contradicción: triángulo de aviso
im, d = canvas()
d.line([(256, 70), (450, 420), (62, 420), (256, 70)], fill=C, width=W, joint='curve')
d.line((256, 180, 256, 310), fill=C, width=34)
d.ellipse((238, 345, 274, 381), fill=C)
save(im, 'contradiccion.png')

print(sorted(os.listdir(OUT)))
