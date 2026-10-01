"""Galería antes/después del día 3 (docs/screenshots/2026-09-30/galeria/): cada pantalla con la captura de antes
(dia3-antes-C: comienzo del bloque C, que visualmente es el final de la noche 2) y la de ahora (anim/), y una hoja
de los 7 retratos plano 2D / relieve 2.5D. JPEG para no engordar el repositorio.
Nota: las capturas eligen historia al azar; algunas parejas muestran casos distintos.
Uso: python Tools/make_gallery_dia3.py
"""
import os

from PIL import Image, ImageDraw, ImageFont

BASE = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'docs', 'screenshots', '2026-09-30')
OUT = os.path.join(BASE, 'galeria')
os.makedirs(OUT, exist_ok=True)

SCREENS = [
    ('01_menu', 'panel_menu'), ('02_casos', 'panel_casos'), ('03_expediente', 'panel_intro'),
    ('04_interrogatorio', 'panel_interrogatorio'), ('05_libreta', 'panel_libreta'),
    ('06_acusacion', 'panel_acusacion'), ('07_veredicto', 'veredicto_8000'), ('08_final_bueno', 'final_good_7000'),
    ('09_final_malo', 'final_bad_7000'), ('10_ajustes', 'panel_ajustes'), ('11_instrucciones', 'panel_instrucciones'),
    ('12_dialogo', 'dialogo_reiniciar'), ('13_texto_grande', 'texto_grande_interrogatorio'),
    ('14_alto_contraste', 'alto_contraste_interrogatorio'), ('15_emocion', 'emocion_nervioso_1500'),
    ('16_pista', 'pista_1800'), ('17_contradiccion', 'contradiccion_1900'), ('18_tutorial', 'tutorial_preguntar'),
    ('21_ficha_policial', 'veredicto_ficha'), ('24_libreta_partes', 'libreta_partes'), ('25_tutorial_libreta', 'tutorial_versiones'),
]


def font(size):
    for name in ('arialbd.ttf', 'DejaVuSans-Bold.ttf'):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            pass
    return ImageFont.load_default()


def labelled(images, labels, gap=12, band=44):
    w = sum(im.width for im in images) + gap * (len(images) - 1)
    h = max(im.height for im in images) + band
    out = Image.new('RGB', (w, h), (24, 24, 28))
    d = ImageDraw.Draw(out)
    x = 0
    for im, text in zip(images, labels):
        d.text((x + 10, 10), text, fill=(232, 226, 214), font=font(24))
        out.paste(im, (x, band))
        x += im.width + gap
    return out


made = 0
for name, shot in SCREENS:
    before = os.path.join(BASE, 'dia3-antes-C', shot + '.png')
    after = os.path.join(BASE, 'anim', shot + '.png')
    if not os.path.exists(after):
        print('falta', shot)
        continue
    if os.path.exists(before):
        sheet = labelled([Image.open(before).convert('RGB'), Image.open(after).convert('RGB')], ['ANTES', 'AHORA'])
    else:  # Pantalla nueva de hoy: no hay antes
        sheet = labelled([Image.open(after).convert('RGB')], ['NUEVO'])
    sheet.save(os.path.join(OUT, name + '.jpg'), quality=85)
    made += 1

# Intros de las tres historias (no había arte propio antes: una sala genérica)
intros = [Image.open(os.path.join(BASE, 'anim', f'intro_historia{i}.png')).convert('RGB') for i in (1, 2, 3)]
labelled([Image.open(os.path.join(BASE, 'dia3-antes-C', 'panel_intro.png')).convert('RGB')] + intros,
         ['ANTES (todas)', 'AHORA: historia 1', 'historia 2', 'historia 3']).save(os.path.join(OUT, '19_intros_por_historia.jpg'), quality=85)

# Libreta con las versiones de cada sospechoso (render del editor: peor caso, todos interrogados)
import glob
renders = sorted(glob.glob(os.path.join(BASE, '[0-9][0-9][0-9][0-9]', '1080x1920_CluesPanel.png')))
if renders:
    labelled([Image.open(os.path.join(BASE, 'dia3-antes-C', 'panel_libreta.png')).convert('RGB'),
              Image.open(renders[-1]).convert('RGB').resize((540, 960))],
             ['ANTES (vacía)', 'AHORA: con lo que dice cada uno']).save(os.path.join(OUT, '22_libreta_versiones.jpg'), quality=85)

# Notas del jugador (ronda 9-12): libreta, rueda y el informe que las recuerda
notes = [os.path.join(BASE, 'anim', n + '.png') for n in ('notas_libreta', 'notas_rueda', 'notas_final')]
if all(os.path.exists(n) for n in notes):
    labelled([Image.open(n).convert('RGB') for n in notes],
             ['NUEVO: tus notas', 'la rueda tacha tu descarte', 'el final lo recuerda']).save(os.path.join(OUT, '23_notas.jpg'), quality=85)

# Personajes: los 7 retratos, plano (antes) y relieve (ahora)
proto = os.path.join(BASE, 'c3-prototipo')
chars = ['padre', 'madre', 'hermano', 'vecina', 'cartero', 'duenio_bar', 'detective']
rows = []
for kind, title in (('1_plano', 'ANTES: plano 2D'), ('2_relieve', 'AHORA: relieve 2.5D')):
    ims = [Image.open(os.path.join(proto, f'{c}_{kind}.png')).convert('RGB').resize((270, 360)) for c in chars]
    rows.append(labelled(ims, [title] + [''] * (len(ims) - 1), gap=6))
sheet = Image.new('RGB', (rows[0].width, rows[0].height * 2 + 10), (24, 24, 28))
sheet.paste(rows[0], (0, 0))
sheet.paste(rows[1], (0, rows[0].height + 10))
sheet.save(os.path.join(OUT, '20_personajes.jpg'), quality=85)
print(f'{made + 2} hojas en {os.path.relpath(OUT)}')
