# Generador local de retratos

Genera retratos en local con SDXL, siguiendo `docs/art/javier/BRIEF.md`. Todo lo pesado (Python, modelos y salidas)
va **fuera del repositorio**, en `C:\AI\`. En el repositorio solo hay scripts (`Tools/portrait_gen/`) y documentación.
Licencias: `LICENSES.md`.

## Equipo (medido el 01-10-2026)

- **NVIDIA GeForce RTX 5070 Ti Laptop GPU, 12 GB de VRAM, arquitectura Blackwell (compute 12.0), driver 610.78.**
  El encargo decía "RTX 5070, 8 GB", pero `nvidia-smi` dice otra cosa.
- 32 GB de RAM y 386 GB libres en C:.
- **Blackwell necesita PyTorch compilado para CUDA 12.8 o posterior.** Las ruedas de CUDA 12.1 o 12.4 no tienen
  núcleos para `sm_120`.

## Instalación (Git Bash)

```bash
mkdir -p /c/AI && cd /c/AI
python -m venv bootstrap                         # Solo para tener uv sin tocar el Python del sistema
./bootstrap/Scripts/python -m pip install uv
export UV_CACHE_DIR=/c/AI/uv-cache UV_PYTHON_INSTALL_DIR=/c/AI/python UV_NATIVE_TLS=1
./bootstrap/Scripts/uv python install 3.12
./bootstrap/Scripts/uv venv portrait-gen/.venv --python 3.12
./bootstrap/Scripts/uv pip install --python portrait-gen/.venv/Scripts/python.exe torch torchvision \
    --index-url https://download.pytorch.org/whl/cu128
./bootstrap/Scripts/uv pip install --python portrait-gen/.venv/Scripts/python.exe \
    diffusers transformers accelerate safetensors peft pillow numpy scipy huggingface_hub
```

- **`UV_NATIVE_TLS=1` es necesario aquí.** Sin él, `uv` falla con `invalid peer certificate: UnknownIssuer`, porque en
  esta máquina algo (probablemente el antivirus) inspecciona HTTPS con un certificado que solo conoce el almacén de
  Windows. Python con certifi sí funciona.
- **Comprobación:**
  ```
  portrait-gen/.venv/Scripts/python -c "import torch; print(torch.cuda.get_device_name(0), torch.cuda.get_device_capability(0))"
  ```
  Tiene que dar `(12, 0)` y ningún aviso de "no kernel image".
- Versiones instaladas: torch 2.11.0+cu128, torchvision 0.26.0+cu128, diffusers 0.40.0 y transformers 5.18.0.

## Modelos

```bash
HF_HOME=/c/AI/hf-cache /c/AI/bootstrap/Scripts/python /c/Dev/Detective-narrative-game/Tools/portrait_gen/download_models.py
```

- **Pesos:** solo fp16. El script baja ~12 GB sin el modelo de inpainting y ~17 GB con él.
- **Velocidad medida:** 1,2-2,7 MB/s, así que cuenta con 1-2 horas.
- **Inpainting:** el modelo dedicado (5 GB) solo se baja con `--inpainting`. Las expresiones se hacen primero con el
  modelo base en modo inpainting (`expressions.py`).
- **Paralelizar no acelera:** con 3 conexiones a la vez el total es el mismo, porque el límite es la línea.

## Uso

Todos con el Python del entorno, `C:\AI\portrait-gen\.venv\Scripts\python.exe`:

| Paso | Comando |
|---|---|
| Candidatos del estado base | `Tools/portrait_gen/generate.py --out C:\AI\portrait-gen\out\javier --count 40` |
| Hoja de contacto | `Tools/portrait_gen/sheet.py hoja.png out\javier\final\1003.png=1003 ...` (junto a Marcos y Lucía, a la misma escala) |
| Expresiones editando el elegido | `Tools/portrait_gen/expressions.py --raw out\javier\raw\<semilla>.png --out out\javier\triste --expression triste` |
| Comprobación de encuadre | `Tools/measure_portrait.py tranquilo.png triste.png nervioso.png` (≤1 % entre estados) |

## Cómo funciona

- **Pose y proporciones:** esqueleto OpenPose dibujado por código (`pose.py`): figura al 90 % del alto y cabeza ≈1/5.
  Va al ControlNet de pose de `xinsir`. No se usa ningún detector de pose (ver `LICENSES.md`).
- **Estilo:** LoRA `pixel-art-xl` más IP-Adapter Plus con las referencias 02 y 04, **solo en la capa de estilo**
  (InstantStyle: `up.block_0`, capa 1). Así se copia cómo están dibujadas, no a Marcos con su bigote y su delantal.
- **Prompt:** el del BRIEF, repartido entre los dos codificadores de texto de SDXL (77 tokens cada uno): el personaje
  en `prompt` y el estilo en `prompt_2`.
- **Posproceso:** `remove_white_bg.py` (fondo blanco a transparencia, conservando la sombra; 768×1024 con los pies a
  40 px). Después `style.pixelate`: rejilla de 5 px por vecino más cercano y paleta reducida.
- **Filtro automático** (`style.measure`):
  - píxel efectivo (≈5 px; Daniel da 8 y las referencias 4-5);
  - distancia a la paleta del BRIEF;
  - una sola figura;
  - contorno del 0,8-2,4 % del alto;
  - avisos de `remove_white_bg`.

  **La proporción cabeza/cuerpo no se mide automáticamente:** el intento (apertura de hombros) falló en 4 de los 7
  originales. La fija el esqueleto y se revisa a mano con regla sobre los finalistas.
