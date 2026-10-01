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
import time

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.abspath(os.path.join(HERE, '..', '..'))
sys.path[:0] = [HERE, os.path.join(REPO, 'Tools')]
os.environ.setdefault('HF_HOME', r'C:\AI\hf-cache')
os.environ.setdefault('HF_HUB_OFFLINE', '1')  # Todo está en la caché; el SSL de este Python no ve el almacén de Windows

import torch  # noqa: E402
from PIL import Image  # noqa: E402

import colour  # noqa: E402
import pose  # noqa: E402
import remove_white_bg as rwb  # noqa: E402
import style  # noqa: E402

W, H = 864, 1152  # 3:4, múltiplos de 8, en el rango de SDXL

# BRIEF sección 4, condensado y repartido entre los dos codificadores de SDXL, que leen 77 tokens cada uno (contados
# con su tokenizador: personaje + expresión = 76, estilo = 58, negativo = 63). En la primera prueba, con el texto
# completo, se cortaron la botella, las botas y la expresión, y salió con bigote y camisa blanca: lo crítico va primero.
# Segunda prueba: sin bigote, pero joven, sin botella y con chaleco sobre camiseta blanca (las dos referencias
# llevan camisa blanca bajo algo oscuro: el IP-Adapter filtraba ropa). Palabras más fuertes y lo crítico primero.
# Cuarta prueba + A/B con las mismas semillas: el IP-Adapter, incluso solo en la capa de estilo, copiaba la ropa y la
# juventud de Álex (camiseta blanca, vaqueros) en las 4; sin él, no. Y el personaje iba solo al codificador CLIP-L: el
# que más pesa en SDXL (OpenCLIP bigG, prompt_2) recibía el estilo. Ahora el mismo texto va a los dos.
# Quinta prueba: camisa de cuadros, corpulento y barba en las 8, pero canoso del todo (≈60 años) y fondo gris carbón
# en las 8: el fondo se quita igual (remove_white_bg admite fondo liso de color) y la sombra se sintetiza.
# Revisión de Cristian (01-10-2026): los 40 parecían "un leñador de 30 años, guapo y tranquilo" con la franela roja del
# tópico. Javier: 44, curtido, canas en las sienes, ojeras, amargado y a la defensiva, corpulento, franela oliva y marrón.
JAVIER = ('pixel art sprite, full body, 44 year old weathered heavy-set man, tired bitter defensive scowl, dark circles '
          'under eyes, dark hair grey at the temples, stubble beard, no moustache, olive and brown flannel shirt, rolled '
          'sleeves, brown trousers, work boots, green beer bottle in right hand, thick black outline')
JAVIER_NEGATIVE = ('moustache, mustache, apron, vest, cigarette, beer mug, red plaid, lumberjack, young, handsome, smiling, '
                   'slim, athletic, braces, suspenders, photorealistic, 3d render, blurry, anime, chibi, cropped feet, '
                   'scenery, text, watermark, multiple characters, glasses')
# Prueba ciega (3 de 3 señalaron al candidato): paleta turbia y sin contraste, sombreado ruidoso, cara borrosa sin ojos
# claros. Variante con el estilo explícito (cel shading, colores vivos, ojos expresivos) y negativo contra el ruido.
JAVIER_CEL = ('pixel art sprite, anime style, clean flat cel shading, vibrant saturated colors, big expressive eyes, full '
              'body, 44 year old weathered heavy-set man, bitter defensive scowl, stubble beard, no moustache, dark hair '
              'grey temples, olive and brown flannel shirt, brown trousers, boots, green beer bottle')
JAVIER_CEL_NEGATIVE = ('muted colors, desaturated, dithering, noise, painterly, blurry face, realistic face, moustache, '
                       'apron, vest, mug, young, handsome, slim, suspenders, photorealistic, 3d render, anime chibi, '
                       'cropped feet, scenery, text, watermark, multiple characters')
CHARACTER = ('pixel art sprite, full body, plain light grey background, 44 year old heavy-set farmer, red plaid flannel '
             'shirt with rolled sleeves, dark stubble beard, short dark brown hair grey at the temples, tired face, '
             'holding a green beer bottle, brown work trousers, work boots, thick black outline')
STYLE = ('retro 16-bit adventure game sprite in the style of the reference, medium visible pixels, realistic adult '
         'proportions, normal head size, thick black outline, flat cel shading, warm frontal light, ochre rust olive '
         'brown palette, soft shadow under the feet, plain flat white background')
EXPRESSIONS = {
    'tranquilo': 'guarded exhausted expression, closed mouth, heavy eyelids',
}
NEGATIVE = ('black background, dark background, white hair, white beard, elderly, young, thick moustache, t-shirt, vest, '
            'jacket, suspenders, apron, gradient background, scenery, photorealistic, 3d render, blurry, anime, '
            'chibi, big head, cropped feet, text, watermark, checkerboard, multiple characters, glasses, weapon')
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
    # Todo en la GPU (12 GB). Con enable_model_cpu_offload los modelos vivían en la RAM (~13-14 GB) y Claude Code paró
    # la tanda de 40 por falta de memoria (01-10-2026, 13:45). Los codificadores salen de la GPU en main(), tras usarse.
    # Los codificadores (texto e imagen) solo se usan una vez por tanda: se quedan en la CPU y nunca suben a la GPU.
    # Con todo en la GPU, la carga reservaba 12,4 GB y desbordaba a la RAM unos segundos.
    for name in ('unet', 'controlnet', 'vae'):
        getattr(pipe, name).to('cuda')
    return pipe


def encode_once(pipe, prompt, negative, refs):
    """Codifica el prompt y las referencias UNA vez en la CPU y elimina los tres codificadores: en una tanda no cambian.
    Prueba con 2 imágenes (01-10-2026): con los codificadores pasados a la CPU después de usarlos, la RAM del proceso
    subía a 9,4 GB y la VRAM reservada a 13,9 GB en una GPU de 12,2 (el controlador desbordaba en silencio a la RAM).
    Así, y con el VAE por mosaicos, cabe: 2,6 GB de RAM y 10,8 GB de VRAM al generar.
    Devuelve ([prompt, negativo, pooled, pooled negativo], embeds de imagen), ya en la GPU."""
    import gc
    with torch.no_grad():
        embeds = pipe.encode_prompt(prompt=prompt, prompt_2=prompt, device='cpu', num_images_per_prompt=1,
                                    do_classifier_free_guidance=True, negative_prompt=negative,
                                    negative_prompt_2=negative)
        ip_embeds = pipe.prepare_ip_adapter_image_embeds(ip_adapter_image=[refs], ip_adapter_image_embeds=None,
                                                         device='cpu', num_images_per_prompt=1,
                                                         do_classifier_free_guidance=True)
    embeds = [e.to('cuda', torch.float16) for e in embeds]
    ip_embeds = [e.to('cuda', torch.float16) for e in ip_embeds]
    for name in ('text_encoder', 'text_encoder_2', 'image_encoder'):
        setattr(pipe, name, None)
    gc.collect()
    torch.cuda.empty_cache()
    pipe.vae.enable_tiling()  # Decodificar 864x1152 de una vez era el pico de VRAM (en diffusers 0.40 va en el VAE)
    return embeds, ip_embeds


def flat_border(raw):
    """Fracción del borde sin saltos bruscos entre píxeles vecinos (< 10 de diferencia RGB). Un fondo con escenario o
    textura no se puede quitar bien y se descarta; un degradado suave sí (imagen a imagen desde Marcos: el borde derecho
    sale azulado, de 172,181,188 a 183,201,210, y el criterio anterior, ±24 del color mediano, lo rechazaba)."""
    import numpy as np
    a = np.asarray(raw.convert('RGB')).astype(float)
    ring = np.concatenate([a[0], a[1:, -1], a[-1, ::-1], a[::-1, 0]])  # El borde en orden, como un anillo
    jumps = np.abs(np.diff(ring, axis=0)).max(-1)
    return float((jumps < 10).mean())


class Stats:
    """Picos de RAM (proceso y mínimo libre del sistema) y de VRAM (de torch y libre en la GPU), muestreados cada 0,25 s,
    y segundos por imagen. Se guardan en <out>/stats.txt."""

    def __init__(self):
        import threading

        import psutil
        self.proc, self.psutil = psutil.Process(), psutil
        self.peak_rss = 0
        self.min_free = psutil.virtual_memory().available
        self.min_free_vram = None
        self.times, self.marks = [], []
        self.running = True
        threading.Thread(target=self._sample, daemon=True).start()

    def _sample(self):
        while self.running:
            self.peak_rss = max(self.peak_rss, self.proc.memory_info().rss)
            self.min_free = min(self.min_free, self.psutil.virtual_memory().available)
            if torch.cuda.is_initialized():
                free = torch.cuda.mem_get_info()[0]
                self.min_free_vram = free if self.min_free_vram is None else min(self.min_free_vram, free)
            time.sleep(0.25)

    def mark(self, label):
        free = torch.cuda.mem_get_info()[0] if torch.cuda.is_initialized() else 0
        self.marks.append((label, self.proc.memory_info().rss,
                           torch.cuda.memory_reserved() if torch.cuda.is_initialized() else 0, free))

    def image(self, seconds):
        self.times.append(seconds)

    def report(self, out):
        gb = lambda b: f'{b / 1e9:.1f} GB'
        lines = [f'RAM del proceso, pico: {gb(self.peak_rss)}',
                 f'RAM libre del sistema, mínimo: {gb(self.min_free)}',
                 f'VRAM reservada por torch, pico: {gb(torch.cuda.max_memory_reserved())}',
                 f'VRAM libre en la GPU, mínimo: {gb(self.min_free_vram or 0)}',
                 f'Imágenes: {len(self.times)}; segundos por imagen: '
                 + (f'{sum(self.times) / len(self.times):.1f} (de {min(self.times):.1f} a {max(self.times):.1f})' if self.times else '-')]
        lines += [f'  {label}: RAM {gb(rss)}, VRAM reservada {gb(vram)}, VRAM libre {gb(free)}'
                  for label, rss, vram, free in self.marks]
        text = chr(10).join(lines)
        print(text, flush=True)
        with open(os.path.join(out, 'stats.txt'), 'a', encoding='utf-8') as f:
            f.write(text + chr(10) + chr(10))


def score(m, warnings, leak):
    """Menos es mejor: píxel cerca de 5, paleta cercana, una sola figura, contorno de las referencias (1-2 %), sin
    avisos y sin ropa clara comida por el relleno del fondo."""
    s = abs(m['pixel'] - 5) * 4 + m['palette'] / 5 + (m['parts'] - 1) * 10
    s += 0 if 0.008 <= m['outline'] <= 0.024 else 5
    s += 0 if leak <= 0.05 else 20 + leak * 100  # Originales sanos: 0,016-0,034; manga comida: 0,095
    return s + 10 * len(warnings)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--out', required=True)
    ap.add_argument('--count', type=int, default=40)
    ap.add_argument('--first-seed', type=int, default=1000)
    ap.add_argument('--expression', default='tranquilo')
    ap.add_argument('--lora', type=float, default=0.8)  # A 1,0 tiraba a fondos grises propios del LoRA
    # A/B 3000-3003: con el personaje solo en CLIP-L, el IP-Adapter copiaba la ropa de Álex. A/B 4000-4003 con el prompt
    # en los dos codificadores: a 0,3 da la piel cálida de los originales (paleta 21-23 frente a 23-28) casi sin filtrar
    ap.add_argument('--style', type=float, default=0.3)
    ap.add_argument('--pose', type=float, default=0.8)
    ap.add_argument('--steps', type=int, default=30)
    ap.add_argument('--cfg', type=float, default=7.0)  # Más peso al prompt: la camisa de cuadros no salía
    ap.add_argument('--cell', type=int, default=5)
    # Imagen a imagen desde un original (revisión de Cristian): mismo estilo, contorno y tipo de cara que Marcos
    ap.add_argument('--init', help='imagen de partida 864x1152 (p. ej. Marcos normalizado sobre gris claro)')
    ap.add_argument('--strength', type=float, default=0.65, help='cuánto se aleja de --init (0 = igual, 1 = nada)')
    ap.add_argument('--pose-kind', default='standing', choices=sorted(pose.POSES))
    ap.add_argument('--javier', action='store_true', help='prompt de la revisión de Cristian (JAVIER, JAVIER_NEGATIVE)')
    ap.add_argument('--cel', action='store_true', help='con --javier: variante JAVIER_CEL (tras la prueba ciega)')
    args = ap.parse_args()

    for sub in ('raw', 'final'):
        os.makedirs(os.path.join(args.out, sub), exist_ok=True)
    stats = Stats()
    pipe = load_pipeline(args.lora, args.style)
    stats.mark('cargado')
    skeleton = pose.render(pose.POSES[args.pose_kind](W, H), W, H)
    skeleton.save(os.path.join(args.out, 'pose.png'))
    refs = [on_white_square(os.path.join(REPO, p)) for p in REFERENCES]
    prompt = (JAVIER_CEL if args.cel else JAVIER) if args.javier else f'{CHARACTER}, {EXPRESSIONS[args.expression]}'
    negative = (JAVIER_CEL_NEGATIVE if args.cel else JAVIER_NEGATIVE) if args.javier else NEGATIVE

    (prompt_embeds, negative_embeds, pooled, negative_pooled), ip_embeds = encode_once(pipe, prompt, negative, refs)
    init = None
    if args.init:
        from diffusers import StableDiffusionXLControlNetImg2ImgPipeline
        # torch_dtype explícito: en diffusers 0.40 from_pipe pasa todo a float32 si no se le dice ("Half and Float")
        pipe = StableDiffusionXLControlNetImg2ImgPipeline.from_pipe(pipe, torch_dtype=torch.float16)
        init = Image.open(args.init).convert('RGB').resize((W, H))
    stats.mark('codificado')

    # Se reanuda sin perder las puntuaciones ya hechas (otras semillas de la misma carpeta)
    path_csv = os.path.join(args.out, 'scores.csv')
    rows = []
    if os.path.exists(path_csv):
        with open(path_csv, encoding='utf-8') as f:
            rows = [r for r in csv.DictReader(f)
                    if not (args.first_seed <= int(r['seed']) < args.first_seed + args.count)]
    for seed in range(args.first_seed, args.first_seed + args.count):
        raw_path = os.path.join(args.out, 'raw', f'{seed}.png')
        if not os.path.exists(raw_path):
            t0 = time.perf_counter()
            try:
                common = dict(prompt_embeds=prompt_embeds, negative_prompt_embeds=negative_embeds,
                              pooled_prompt_embeds=pooled, negative_pooled_prompt_embeds=negative_pooled,
                              controlnet_conditioning_scale=args.pose, ip_adapter_image_embeds=ip_embeds,
                              num_inference_steps=args.steps, guidance_scale=args.cfg, width=W, height=H,
                              generator=torch.Generator('cpu').manual_seed(seed))
                if init is None:
                    image = pipe(image=skeleton, **common).images[0]
                else:
                    image = pipe(image=init, control_image=skeleton, strength=args.strength, **common).images[0]
            except torch.cuda.OutOfMemoryError as e:
                # Sin plan B silencioso: se para y se enseña el error (no se vuelve a la descarga a la RAM)
                stats.report(args.out)
                sys.exit(f'CUDA sin memoria en la semilla {seed}: {e}')
            stats.image(time.perf_counter() - t0)
            image.save(raw_path)
        image = Image.open(raw_path)
        flat = flat_border(image)
        if flat < 0.9:  # Fondo que no es liso: descartado antes de puntuar
            rows.append({'seed': seed, 'score': 999, 'flat': round(flat, 2), 'warnings': 'fondo no liso'})
            print(rows[-1], flush=True)
            continue
        try:
            if args.javier:  # Revisión de Cristian: la misma cadena que finish.py (contorno, color, contraste)
                import finish
                final, report = finish.finish(image, cell=args.cell)
            else:
                clean, report = rwb.process(image)
                if report['shadow_pixels'] == 0:
                    clean = style.add_shadow(clean)  # Fondo de color: sombra sintética medida sobre los originales
                final = style.pixelate(clean, cell=args.cell)
        except ValueError as e:
            rows.append({'seed': seed, 'score': 999, 'error': str(e)})
            continue
        final.save(os.path.join(args.out, 'final', f'{seed}.png'))
        m = style.measure(final)
        leak = style.background_leak(image)
        rows.append({'seed': seed, 'score': round(score(m, report['warnings'], leak), 2), 'leak': round(leak, 3),
                     **{k: round(v, 3) for k, v in m.items()},
                     'shadow': report['shadow_pixels'], 'warnings': ' | '.join(report['warnings'])})
        print(rows[-1], flush=True)
        stats.mark(f'semilla {seed}')
        with open(path_csv, 'w', newline='', encoding='utf-8') as f:
            keys = sorted({k for r in rows for k in r}, key=lambda k: (k != 'seed', k != 'score', k))
            writer = csv.DictWriter(f, fieldnames=keys)
            writer.writeheader()
            writer.writerows(sorted(rows, key=lambda r: float(r['score'])))  # Las leídas del CSV vienen como texto
    stats.report(args.out)


if __name__ == '__main__':
    main()
