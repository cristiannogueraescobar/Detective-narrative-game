"""Descarga los modelos del generador local de retratos (docs/art/LOCAL-GEN.md, licencias en docs/art/LICENSES.md).
Solo pesos fp16 y lo imprescindible, a la caché HF_HOME (C:/AI/hf-cache), fuera del repositorio del juego.
El modelo de inpainting dedicado (5 GB) solo se baja con --inpainting: las expresiones se intentan antes con el base.
Uso: HF_HOME=/c/AI/hf-cache python Tools/portrait_gen/download_models.py [--inpainting]"""
import os
import sys

os.environ.setdefault('HF_HOME', r'C:\AI\hf-cache')
from huggingface_hub import snapshot_download  # noqa: E402

MODELS = [
    # (repo, patrones, licencia)
    ('stabilityai/stable-diffusion-xl-base-1.0',
     ['model_index.json', 'scheduler/*', 'tokenizer/*', 'tokenizer_2/*', 'text_encoder/config.json',
      'text_encoder/model.fp16.safetensors', 'text_encoder_2/config.json', 'text_encoder_2/model.fp16.safetensors',
      'unet/config.json', 'unet/diffusion_pytorch_model.fp16.safetensors', 'vae/config.json',
      # diffusers exige este VAE al comprobar la caché aunque se le pase el fp16-fix
      'vae_1_0/config.json', 'vae_1_0/diffusion_pytorch_model.fp16.safetensors'], 'openrail++'),
    ('madebyollin/sdxl-vae-fp16-fix', ['config.json', 'diffusion_pytorch_model.safetensors'], 'mit'),
    ('nerijs/pixel-art-xl', ['pixel-art-xl.safetensors'], 'creativeml-openrail-m'),
    ('h94/IP-Adapter', ['sdxl_models/ip-adapter-plus_sdxl_vit-h.safetensors', 'models/image_encoder/config.json',
                        'models/image_encoder/model.safetensors'], 'apache-2.0'),
    ('xinsir/controlnet-openpose-sdxl-1.0', ['config.json', 'diffusion_pytorch_model.safetensors'], 'apache-2.0'),
]
OPTIONAL = [
    ('diffusers/stable-diffusion-xl-1.0-inpainting-0.1',
     ['model_index.json', 'scheduler/*', 'unet/config.json', 'unet/diffusion_pytorch_model.fp16.safetensors'],
     'openrail++'),
]

for repo, patterns, lic in MODELS + (OPTIONAL if '--inpainting' in sys.argv else []):
    path = snapshot_download(repo, allow_patterns=patterns)
    size = sum(os.path.getsize(os.path.join(r, f)) for r, _, fs in os.walk(path) for f in fs) / 1e9
    print(f'{repo:52} {lic:24} {size:5.2f} GB  {path}', flush=True)
print('OK', file=sys.stderr)
