"""Expresiones EDITANDO el retrato tranquilo elegido, no regenerando (docs/art/javier/BRIEF.md, sección 4).

Se repinta solo la cara (de la frente a la barbilla) y los hombros, con la misma pose (ControlNet), el mismo estilo
(IP-Adapter en capas de estilo) y el LoRA de pixel art. Fuera de la máscara se vuelve a pegar el original píxel a píxel:
la coronilla y los pies no cambian, así que el encuadre que calcula remove_white_bg es el mismo.

Uso: python Tools/portrait_gen/expressions.py --raw <tranquilo_raw.png> --out <carpeta> --expression triste --count 8
"""
import argparse
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.abspath(os.path.join(HERE, '..', '..'))
sys.path[:0] = [HERE, os.path.join(REPO, 'Tools')]
os.environ.setdefault('HF_HOME', r'C:\AI\hf-cache')

import numpy as np  # noqa: E402
import torch  # noqa: E402
from PIL import Image, ImageFilter  # noqa: E402
from scipy import ndimage  # noqa: E402

import generate  # noqa: E402
import pose  # noqa: E402
import remove_white_bg as rwb  # noqa: E402
import style  # noqa: E402

# Prompts de edición del BRIEF (solo la expresión) + lo imprescindible para que el personaje siga siendo el mismo
EDITS = {
    'triste': ('pixel art, same man, grieving and self-pitying expression, inner eyebrows raised, eyes downcast, '
               'red watery eyes, shoulders slightly slumped, short stubble beard, clean-shaven upper lip'),
    'nervioso': ('pixel art, same man, cornered and defensive expression with a hint of anger, furrowed brow, '
                 'clenched jaw, eyes glancing sideways, a single sweat drop on the temple, shoulders tense, '
                 'short stubble beard, clean-shaven upper lip'),
}


def face_and_shoulders_mask(raw):
    """Máscara en coordenadas de la imagen generada: cara (sin la coronilla) y hombros."""
    rgb = np.asarray(raw.convert('RGB')).astype(int)
    lum = rgb.mean(-1)
    sat = rgb.max(-1) - rgb.min(-1)
    labels, _ = ndimage.label((sat <= rwb.BG_SAT) & (lum >= rwb.BG_LUM))
    border = np.unique(np.concatenate([labels[0], labels[-1], labels[:, 0], labels[:, -1]]))
    figure = ~np.isin(labels, border[border > 0])
    ys, xs = np.where(figure)
    y0, y1 = ys.min(), ys.max() + 1
    fh = y1 - y0
    head_rows = figure[y0:y0 + int(fh * 0.12)]
    hx = np.where(head_rows.any(axis=0))[0]
    cx, hw = (hx.min() + hx.max()) / 2, hx.max() - hx.min()
    mask = np.zeros(figure.shape, bool)
    mask[int(y0 + fh * 0.07):int(y0 + fh * 0.21), int(cx - hw * 0.55):int(cx + hw * 0.55)] = True   # Cara
    mask[int(y0 + fh * 0.19):int(y0 + fh * 0.29), int(cx - hw * 1.5):int(cx + hw * 1.5)] = True    # Hombros
    mask &= ndimage.binary_dilation(figure, iterations=6)  # No pintar fondo nuevo lejos de la figura
    return Image.fromarray((mask * 255).astype(np.uint8)).filter(ImageFilter.MaxFilter(9))


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--raw', required=True)
    ap.add_argument('--out', required=True)
    ap.add_argument('--expression', required=True, choices=sorted(EDITS))
    ap.add_argument('--count', type=int, default=8)
    ap.add_argument('--first-seed', type=int, default=5000)
    ap.add_argument('--strength', type=float, default=0.75)
    ap.add_argument('--cell', type=int, default=5)
    args = ap.parse_args()

    from diffusers import StableDiffusionXLControlNetInpaintPipeline
    base = generate.load_pipeline(lora_scale=1.0, style_scale=0.6)
    pipe = StableDiffusionXLControlNetInpaintPipeline.from_pipe(base)
    pipe.enable_model_cpu_offload()

    raw = Image.open(args.raw).convert('RGB')
    mask = face_and_shoulders_mask(raw)
    skeleton = pose.render(pose.standing_three_quarter(raw.width, raw.height), raw.width, raw.height)
    refs = [generate.on_white_square(os.path.join(REPO, p)) for p in generate.REFERENCES]
    os.makedirs(os.path.join(args.out, 'raw'), exist_ok=True)
    os.makedirs(os.path.join(args.out, 'final'), exist_ok=True)
    mask.save(os.path.join(args.out, 'mask.png'))
    for seed in range(args.first_seed, args.first_seed + args.count):
        edited = pipe(prompt=EDITS[args.expression], prompt_2=generate.STYLE, negative_prompt=generate.NEGATIVE,
                      negative_prompt_2=generate.NEGATIVE, image=raw, mask_image=mask, control_image=skeleton,
                      controlnet_conditioning_scale=0.8, ip_adapter_image=[refs], strength=args.strength,
                      num_inference_steps=30, guidance_scale=6.0, width=raw.width, height=raw.height,
                      generator=torch.Generator('cpu').manual_seed(seed)).images[0]
        # Fuera de la máscara, el original exacto (el VAE retoca todos los píxeles al codificar y decodificar)
        merged = Image.composite(edited.resize(raw.size), raw, mask)
        merged.save(os.path.join(args.out, 'raw', f'{seed}.png'))
        clean, report = rwb.process(merged)
        style.pixelate(clean, cell=args.cell).save(os.path.join(args.out, 'final', f'{seed}.png'))
        print(seed, report['warnings'], flush=True)


if __name__ == '__main__':
    main()
