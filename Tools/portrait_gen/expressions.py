"""Ediciones sobre el retrato elegido, no regeneraciones (docs/art/javier/BRIEF.md, sección 4).

- Expresiones (triste, nervioso): se repinta solo la cara (sin la coronilla) y los hombros.
- botella: se repinta solo la mano derecha del personaje (la botella casi nunca sale al generar: 1 de ~30).
Misma pose (ControlNet con el mismo esqueleto), mismo estilo (IP-Adapter a 0,3) y LoRA. Las zonas salen del esqueleto
(pose.py): la imagen generada tiene su mismo tamaño y lo sigue. Fuera de la máscara se pega el original píxel a píxel,
así que la coronilla y los pies no cambian y el encuadre que calcula remove_white_bg es el mismo.

Uso: python Tools/portrait_gen/expressions.py --raw <base_raw.png> --out <carpeta> --edit triste --count 8
"""
import argparse
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.abspath(os.path.join(HERE, '..', '..'))
sys.path[:0] = [HERE, os.path.join(REPO, 'Tools')]
os.environ.setdefault('HF_HOME', r'C:\AI\hf-cache')
os.environ.setdefault('HF_HUB_OFFLINE', '1')

import numpy as np  # noqa: E402
import torch  # noqa: E402
from PIL import Image, ImageFilter  # noqa: E402
from scipy import ndimage  # noqa: E402

import generate  # noqa: E402
import pose  # noqa: E402
import remove_white_bg as rwb  # noqa: E402
import style  # noqa: E402

PERSON = 'pixel art, 44 year old heavy-set farmer, dark stubble beard, short dark brown hair grey at the temples'
EDITS = {
    # Prompts de edición del BRIEF (solo la expresión) + lo que mantiene al personaje
    'triste': (f'{PERSON}, grieving and self-pitying expression, inner eyebrows raised, eyes downcast, red watery '
               'eyes, shoulders slightly slumped', 'cara'),
    'nervioso': (f'{PERSON}, cornered and defensive expression with a hint of anger, furrowed brow, clenched jaw, '
                 'eyes glancing sideways, a sweat drop on the temple, shoulders tense', 'cara'),
    'botella': ('pixel art, a hand holding a green glass beer bottle by the neck, the bottle hanging down beside the '
                'leg, red plaid flannel sleeve', 'mano'),
    # Prueba ciega (6 de 6 lo señalaron): "cara realista, ojos pequeños entornados" frente a las caras de los originales
    'cara_estilo': ('pixel art, anime style face, large dark expressive eyes with white highlights, thick black '
                    'eyebrows, simple clean cel shading, bitter defensive scowl, short dark stubble beard, no moustache, '
                    '44 year old weathered man, dark hair grey at the temples', 'cara'),
    # A 0,7 la cara ganaba estilo pero dejaba de ser Javier (joven, sin barba, mechones rojizos de Lucía)
    'cara_estilo_mayor': ('pixel art, middle-aged weathered man in his forties, full dark stubble beard on cheeks and '
                          'jaw, wrinkles, tired bags under the eyes, dark expressive eyes with white highlights, thick '
                          'black eyebrows, bitter scowl, clean cel shading, dark brown hair grey at the temples', 'cara'),
}


def edit_mask(raw, zone, pose_kind='standing'):
    """Máscara de la zona a repintar, a partir de los puntos del esqueleto y limitada a la figura."""
    w, h = raw.size
    p = pose.POSES[pose_kind](w, h)
    fh = h * 0.90
    head = fh * 0.20
    mask = np.zeros((h, w), bool)
    if zone == 'cara':
        nx, ny = p[0]
        top = ny - head * 0.35              # Desde la frente (la coronilla no se toca)
        mask[int(top):int(ny + head * 0.45), int(nx - head * 0.45):int(nx + head * 0.45)] = True
        (lx, ly), (rx, _) = p[2], p[5]       # Hombros
        mask[int(ly - fh * 0.03):int(ly + fh * 0.06), int(lx - fh * 0.02):int(rx + fh * 0.02)] = True
    else:
        wx, wy = p[4]                        # Muñeca derecha del personaje
        mask[int(wy - fh * 0.04):int(wy + fh * 0.16), int(wx - fh * 0.07):int(wx + fh * 0.06)] = True
    figure = rwb.figure_mask(raw)
    if zone == 'cara':
        mask &= ndimage.binary_dilation(figure, iterations=6)  # No pintar fondo nuevo alrededor de la cabeza
    return Image.fromarray((mask * 255).astype(np.uint8)).filter(ImageFilter.MaxFilter(9))


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--raw', required=True)
    ap.add_argument('--out', required=True)
    ap.add_argument('--edit', required=True, choices=sorted(EDITS))
    ap.add_argument('--count', type=int, default=8)
    ap.add_argument('--first-seed', type=int, default=7000)
    ap.add_argument('--strength', type=float, default=0.75)
    ap.add_argument('--cell', type=int, default=5)
    ap.add_argument('--pose-kind', default='standing', choices=sorted(pose.POSES))
    ap.add_argument('--style', type=float, default=0.3)
    ap.add_argument('--refs', default='', help='referencias de estilo separadas por comas (por defecto las de generate)')
    args = ap.parse_args()

    from diffusers import StableDiffusionXLControlNetInpaintPipeline
    # Misma gestión de memoria que generate.py: modelos en la GPU, codificadores una vez en la CPU y eliminados (antes,
    # enable_model_cpu_offload: ~13-14 GB de RAM)
    base = generate.load_pipeline(lora_scale=0.8, style_scale=args.style)
    refs = [generate.on_white_square(os.path.join(REPO, p)) for p in (args.refs.split(',') if args.refs else generate.REFERENCES)]
    prompt, zone = EDITS[args.edit]
    (prompt_embeds, negative_embeds, pooled, negative_pooled), ip_embeds = generate.encode_once(
        base, prompt, generate.NEGATIVE, refs)
    # torch_dtype explícito: en diffusers 0.40 from_pipe pasa todo a float32 si no se le dice
    pipe = StableDiffusionXLControlNetInpaintPipeline.from_pipe(base, torch_dtype=torch.float16)

    raw = Image.open(args.raw).convert('RGB')
    mask = edit_mask(raw, zone, args.pose_kind)
    skeleton = pose.render(pose.POSES[args.pose_kind](raw.width, raw.height), raw.width, raw.height)
    for sub in ('raw', 'final'):
        os.makedirs(os.path.join(args.out, sub), exist_ok=True)
    mask.save(os.path.join(args.out, 'mask.png'))
    for seed in range(args.first_seed, args.first_seed + args.count):
        edited = pipe(prompt_embeds=prompt_embeds, negative_prompt_embeds=negative_embeds,
                      pooled_prompt_embeds=pooled, negative_pooled_prompt_embeds=negative_pooled,
                      image=raw, mask_image=mask, control_image=skeleton,
                      controlnet_conditioning_scale=0.8, ip_adapter_image_embeds=ip_embeds, strength=args.strength,
                      num_inference_steps=30, guidance_scale=7.0, width=raw.width, height=raw.height,
                      generator=torch.Generator('cpu').manual_seed(seed)).images[0]
        # Fuera de la máscara, el original exacto (el VAE retoca todos los píxeles al codificar y decodificar)
        merged = Image.composite(edited.resize(raw.size), raw, mask)
        merged.save(os.path.join(args.out, 'raw', f'{seed}.png'))
        import finish
        final, report = finish.finish(merged, cell=args.cell)
        final.save(os.path.join(args.out, 'final', f'{seed}.png'))
        print(seed, report['warnings'], flush=True)


if __name__ == '__main__':
    main()
