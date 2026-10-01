"""Repinta solo la cara de un candidato en bruto, a 1024 px, con el LoRA de estilo (como un "face detailer").

Las 7 pruebas ciegas de la sesión B señalaron siempre la cara: "ojos pequeños, entornados", "cara emborronada, sin
ojos legibles". En la figura entera la cabeza mide ~60 px de 1024: el generador no tiene píxeles para dibujar los ojos
grandes y limpios de los originales. Aquí la cabeza se recorta, se amplía a 1024, se repinta de imagen a imagen con el
LoRA v2 (entrenado también con caras de los originales) y se pega de vuelta con un borde suave. Después, finish.py.

Uso: python Tools/portrait_gen/facefix.py <raw.png> <salida.png> <carpeta del LoRA> [fuerza] [semilla]"""
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path[:0] = [HERE, os.path.join(HERE, '..')]

import numpy as np  # noqa: E402
from PIL import Image  # noqa: E402

PROMPT = ('jvstyle pixel art character portrait, close-up of the face, big clear expressive eyes, 44 year old man, '
          'frowning, short brown beard, short dark hair grey at the temples, olive green flannel shirt')
NEGATIVE = ('small eyes, squinting, realistic face, blurry, noise, dithering, moustache, glasses, photorealistic, 3d render, '
            'text, watermark')
FEATHER = 0.08  # Fracción del lado que se mezcla en el borde


def paste_back(base, face, box):
    """Pega `face` (cualquier tamaño) en `box` de `base`, con el borde mezclado; fuera de `box` no cambia nada."""
    left, top, right, bottom = box
    w, h = right - left, bottom - top
    patch = face.convert('RGB').resize((w, h), Image.LANCZOS)
    ramp = max(1, int(min(w, h) * FEATHER))
    yy, xx = np.mgrid[0:h, 0:w]
    edge = np.minimum(np.minimum(xx + 1, w - xx), np.minimum(yy + 1, h - yy)).astype(float)
    alpha = np.clip(edge / ramp, 0, 1)
    out = np.asarray(base.convert('RGB')).astype(float).copy()
    region = out[top:bottom, left:right]
    out[top:bottom, left:right] = region * (1 - alpha[..., None]) + np.asarray(patch).astype(float) * alpha[..., None]
    return Image.fromarray(np.round(out).astype(np.uint8), 'RGB')


def head_box_raw(raw):
    """Recuadro de la cabeza en el bruto (con fondo): máscara de la figura y el mismo criterio que lora_dataset."""
    import lora_dataset
    import remove_white_bg as rwb
    mask = rwb.figure_mask(raw)
    rgba = np.dstack([np.asarray(raw.convert('RGB')), (mask * 255).astype(np.uint8)])
    left, top, right, bottom = lora_dataset.head_box(Image.fromarray(rgba, 'RGBA'))
    w, h = raw.size
    side = right - left
    left = min(max(0, left), w - side)
    top = min(max(0, top), h - side)
    return left, top, left + side, top + side


def load(lora_dir):
    import torch
    from diffusers import AutoencoderKL, StableDiffusionXLImg2ImgPipeline
    vae = AutoencoderKL.from_pretrained('madebyollin/sdxl-vae-fp16-fix', torch_dtype=torch.float16)
    pipe = StableDiffusionXLImg2ImgPipeline.from_pretrained('stabilityai/stable-diffusion-xl-base-1.0', vae=vae,
                                                            torch_dtype=torch.float16, variant='fp16')
    pipe.load_lora_weights(lora_dir, weight_name='pytorch_lora_weights.safetensors', adapter_name='jv')
    pipe.vae.enable_tiling()
    return pipe.to('cuda')


def fix(pipe, raw, strength=0.45, seed=0, prompt=PROMPT):
    import torch
    box = head_box_raw(raw)
    crop = raw.convert('RGB').crop(box).resize((1024, 1024), Image.LANCZOS)
    face = pipe(prompt=prompt, negative_prompt=NEGATIVE, image=crop, strength=strength, num_inference_steps=30,
                guidance_scale=7.0, generator=torch.Generator('cuda').manual_seed(seed)).images[0]
    return paste_back(raw, face, box), box


if __name__ == '__main__':
    src, dst, lora = sys.argv[1:4]
    strength = float(sys.argv[4]) if len(sys.argv) > 4 else 0.45
    seed = int(sys.argv[5]) if len(sys.argv) > 5 else 0
    out, _ = fix(load(lora), Image.open(src), strength, seed)
    out.save(dst)
    print(dst)
