"""Genera candidatos de retrato con SDXL en local (docs/art/LOCAL-GEN.md) y los puntúa automáticamente.

Pipeline: SDXL base + LoRA pixel-art-xl + ControlNet de pose (esqueleto dibujado en pose.py: fija proporciones y
encuadre) + IP-Adapter Plus solo en las capas de estilo (InstantStyle: copia el estilo de las referencias 02/04 sin
copiar a Marcos) + VAE fp16-fix. Fondo blanco liso → Tools/remove_white_bg.py → rejilla de píxel (style.pixelate).

Uso (con el Python de C:\\AI\\portrait-gen\\.venv):
  python Tools/portrait_gen/generate.py --out C:\\AI\\portrait-gen\\out\\javier --count 40
Deja raw/<semilla>.png (lo generado), final/<semilla>.png (768x1024 listo) y scores.csv.
"""
import argparse
import csv
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.abspath(os.path.join(HERE, '..', '..'))
sys.path[:0] = [HERE, os.path.join(REPO, 'Tools')]
os.environ.setdefault('HF_HOME', r'C:\AI\hf-cache')

import torch  # noqa: E402
from PIL import Image  # noqa: E402

import pose  # noqa: E402
import remove_white_bg as rwb  # noqa: E402
import style  # noqa: E402

W, H = 864, 1152  # 3:4, múltiplos de 8, en el rango de SDXL

# BRIEF sección 4, repartido entre los dos codificadores de SDXL (77 tokens cada uno): personaje y estilo
CHARACTER = ('pixel art, full body, a 44-year-old Andalusian olive farmer, heavy build, weathered tanned face, square '
             'jaw, short dark brown hair greying at the temples, short stubble beard, clean-shaven upper lip, '
             'red-rimmed tired eyes, olive and brown plaid flannel work shirt with rolled-up sleeves, dusty brown '
             'work trousers, worn leather work boots, green beer bottle hanging from his right hand')
STYLE = ('Pixel art character sprite in the exact style of the reference image: retro 16-bit adventure game look, '
         'medium-sized visible pixels, realistic adult proportions, normal head size, thick solid black outline, '
         'flat cel shading, soft warm frontal light, warm ochre, rust, olive and brown palette, subtle soft shadow '
         'under the feet, plain flat white background')
EXPRESSIONS = {
    'tranquilo': 'guarded and exhausted expression, neutral closed mouth, heavy eyelids, looking straight ahead',
}
NEGATIVE = ('photorealistic, 3d render, smooth gradients, anti-aliasing, blurry, painterly, anime, chibi, big head, '
            'oversized head, cropped feet, cut-off head, close-up, background scenery, vignette, dramatic lighting, '
            'text, watermark, frame, checkerboard pattern, transparency grid, multiple characters, extra fingers, '
            'moustache, mustache, polo shirt, glasses, apron, cigarette, blood, weapon')
REFERENCES = ['docs/art/javier/referencias/02-estilo-hombre-adulto-duenio-bar.png',
              'docs/art/javier/referencias/04-estilo-linea-hermano.png']


def on_white_square(path, size=768):
    """Referencia sobre blanco y centrada en un cuadrado (el codificador de imagen recorta al centro)."""
    im = Image.open(path).convert('RGBA')
    side = max(im.size)
    canvas = Image.new('RGBA', (side, side), (255, 255, 255, 255))
    canvas.alpha_composite(im, ((side - im.width) // 2, (side - im.height) // 2))
    return canvas.convert('RGB').resize((size, size), Image.LANCZOS)


def load_pipeline(lora_scale, style_scale):
    from diffusers import (AutoencoderKL, ControlNetModel, DPMSolverMultistepScheduler,
                           StableDiffusionXLControlNetPipeline)
    from transformers import CLIPVisionModelWithProjection

    dtype = torch.float16
    encoder = CLIPVisionModelWithProjection.from_pretrained('h94/IP-Adapter', subfolder='models/image_encoder',
                                                            torch_dtype=dtype)
    controlnet = ControlNetModel.from_pretrained('xinsir/controlnet-openpose-sdxl-1.0', torch_dtype=dtype)
    vae = AutoencoderKL.from_pretrained('madebyollin/sdxl-vae-fp16-fix', torch_dtype=dtype)
    pipe = StableDiffusionXLControlNetPipeline.from_pretrained(
        'stabilityai/stable-diffusion-xl-base-1.0', controlnet=controlnet, vae=vae, image_encoder=encoder,
        torch_dtype=dtype, variant='fp16')
    pipe.scheduler = DPMSolverMultistepScheduler.from_config(pipe.scheduler.config, use_karras_sigmas=True)
    pipe.load_ip_adapter('h94/IP-Adapter', subfolder='sdxl_models', weight_name='ip-adapter-plus_sdxl_vit-h.safetensors')
    # InstantStyle: la referencia solo entra en el bloque de estilo (up.block_0, capa 1), no en el de contenido
    pipe.set_ip_adapter_scale({'up': {'block_0': [0.0, style_scale, 0.0]}})
    pipe.load_lora_weights('nerijs/pixel-art-xl', weight_name='pixel-art-xl.safetensors', adapter_name='pixel')
    pipe.set_adapters(['pixel'], adapter_weights=[lora_scale])
    pipe.enable_model_cpu_offload()
    return pipe


def score(m, warnings):
    """Menos es mejor: píxel cerca de 5, paleta cercana, una sola figura, contorno de las referencias (1-2 %), sin avisos."""
    s = abs(m['pixel'] - 5) * 4 + m['palette'] / 5 + (m['parts'] - 1) * 10
    s += 0 if 0.008 <= m['outline'] <= 0.024 else 5
    return s + 10 * len(warnings)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--out', required=True)
    ap.add_argument('--count', type=int, default=40)
    ap.add_argument('--first-seed', type=int, default=1000)
    ap.add_argument('--expression', default='tranquilo')
    ap.add_argument('--lora', type=float, default=1.0)
    ap.add_argument('--style', type=float, default=0.6)
    ap.add_argument('--pose', type=float, default=0.8)
    ap.add_argument('--steps', type=int, default=30)
    ap.add_argument('--cfg', type=float, default=6.0)
    ap.add_argument('--cell', type=int, default=5)
    args = ap.parse_args()

    for sub in ('raw', 'final'):
        os.makedirs(os.path.join(args.out, sub), exist_ok=True)
    pipe = load_pipeline(args.lora, args.style)
    skeleton = pose.render(pose.standing_three_quarter(W, H), W, H)
    skeleton.save(os.path.join(args.out, 'pose.png'))
    refs = [on_white_square(os.path.join(REPO, p)) for p in REFERENCES]
    prompt = f'{CHARACTER}, {EXPRESSIONS[args.expression]}'

    rows = []
    path_csv = os.path.join(args.out, 'scores.csv')
    for seed in range(args.first_seed, args.first_seed + args.count):
        raw_path = os.path.join(args.out, 'raw', f'{seed}.png')
        if not os.path.exists(raw_path):
            image = pipe(prompt=prompt, prompt_2=STYLE, negative_prompt=NEGATIVE, negative_prompt_2=NEGATIVE,
                         image=skeleton, controlnet_conditioning_scale=args.pose, ip_adapter_image=[refs],
                         num_inference_steps=args.steps, guidance_scale=args.cfg, width=W, height=H,
                         generator=torch.Generator('cpu').manual_seed(seed)).images[0]
            image.save(raw_path)
        image = Image.open(raw_path)
        try:
            clean, report = rwb.process(image)
        except ValueError as e:
            rows.append({'seed': seed, 'score': 999, 'error': str(e)})
            continue
        final = style.pixelate(clean, cell=args.cell)
        final.save(os.path.join(args.out, 'final', f'{seed}.png'))
        m = style.measure(final)
        rows.append({'seed': seed, 'score': round(score(m, report['warnings']), 2), **{k: round(v, 3) for k, v in m.items()},
                     'shadow': report['shadow_pixels'], 'warnings': ' | '.join(report['warnings'])})
        print(rows[-1], flush=True)
        with open(path_csv, 'w', newline='', encoding='utf-8') as f:
            keys = sorted({k for r in rows for k in r}, key=lambda k: (k != 'seed', k != 'score', k))
            writer = csv.DictWriter(f, fieldnames=keys)
            writer.writeheader()
            writer.writerows(sorted(rows, key=lambda r: r['score']))


if __name__ == '__main__':
    main()
