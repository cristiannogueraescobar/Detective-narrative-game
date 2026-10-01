"""Conjunto de entrenamiento del LoRA de estilo: los retratos originales del juego (sin Daniel, que es otro estilo:
píxel de 9 px y cabeza 1/3,6; BRIEF de Javier, sección 3).

Prueba ciega del bloque 1 de la sesión B: 9 de 9 subagentes señalaron al Javier generado por cómo dibuja el generador
(textura ruidosa, cara realista de ojos pequeños). Un LoRA entrenado con los originales le enseña a dibujar como ellos.
Las imágenes se rellenan a cuadrado (el script recorta a cuadrado y cortaría cabeza o pies) sobre el gris del generador.
Cada descripción lleva la palabra de activación y lo que hay en la imagen, para que aprenda el estilo, no a esos
personajes. Arte del propio juego: sin problema de licencia.

Uso: python Tools/portrait_gen/lora_dataset.py C:/AI/lora/dataset"""
import json
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.abspath(os.path.join(HERE, '..', '..'))
sys.path.insert(0, os.path.join(REPO, 'Tools'))

from PIL import Image  # noqa: E402

import remove_white_bg as rwb  # noqa: E402

TRIGGER = 'jvstyle pixel art character sprite, full body'
BG = (190, 190, 194)
ITEMS = {
    'duenio_bar': 'middle-aged heavy-set bar owner, black moustache, slicked black hair, white shirt with rolled sleeves, '
                  'black vest, red apron, holding a beer mug, cigarette, hand on hip',
    'madre': 'young woman, long brown hair, red headband, salmon blouse, white apron, yellow floral skirt, hand on hip',
    'hermano': 'teenage boy, backwards green cap, messy brown hair, black jacket, white t-shirt, jeans, white sneakers, '
               'holding a yellow walkman, earphones',
    'cartero': 'postman, blue uniform and cap, moustache, brown mail bag, holding a letter, walking',
    'vecina': 'middle-aged woman, brown hair bun, purple cardigan, green floral dress, smoking a cigarette, surprised',
    'detective': 'police officer, green uniform and cap, moustache, talking on a phone, holding a notebook',
}


def main(out):
    os.makedirs(out, exist_ok=True)
    rows = []
    for key, text in ITEMS.items():
        im = Image.open(os.path.join(REPO, 'Assets/Images/Suspects', f'{key}.gif.png')).convert('RGBA')
        white = Image.new('RGBA', im.size, (255, 255, 255, 255))
        white.alpha_composite(im)
        figure = rwb.process(white.convert('RGB'))[0]            # 768x1024, figura al 90 %, pies a 40 px
        square = Image.new('RGBA', (1024, 1024), BG + (255,))
        square.alpha_composite(figure, ((1024 - 768) // 2, 0))
        name = f'{key}.png'
        square.convert('RGB').save(os.path.join(out, name))
        rows.append({'file_name': name, 'text': f'{TRIGGER}, {text}'})
    with open(os.path.join(out, 'metadata.jsonl'), 'w', encoding='utf-8') as f:
        for r in rows:
            f.write(json.dumps(r, ensure_ascii=False) + '\n')
    print(out, len(rows), 'imágenes')


if __name__ == '__main__':
    main(sys.argv[1])
