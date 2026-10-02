# Licencias del generador local de retratos

El juego se puede vender, así que **solo entran modelos, LoRAs y extensiones que permiten uso comercial**. Las
licencias se comprobaron el 01-10-2026 en los metadatos que declara cada modelo en Hugging Face (`/api/models/<id>`) y,
en los casos clave, leyendo la tarjeta del modelo o el texto de la licencia. Ninguno de estos archivos entra en el
repositorio: están en `C:\AI\hf-cache` (ver `LOCAL-GEN.md`).

## En uso

| Pieza | Modelo | Licencia | ¿Uso comercial? | Enlace |
|---|---|---|---|---|
| Modelo base | `stabilityai/stable-diffusion-xl-base-1.0` | CreativeML Open RAIL++-M | **Sí.** «Licensor claims no rights in the Output You generate». Con restricciones de uso (Anexo A: nada ilegal, difamatorio, discriminatorio ni dañino; un retrato de ficción de un juego de detectives no choca con ninguna). | https://huggingface.co/stabilityai/stable-diffusion-xl-base-1.0/blob/main/LICENSE.md |
| VAE | `madebyollin/sdxl-vae-fp16-fix` | MIT | Sí | https://huggingface.co/madebyollin/sdxl-vae-fp16-fix |
| Estilo pixel art (LoRA) | `nerijs/pixel-art-xl` | CreativeML OpenRAIL-M | Sí (misma familia de licencia, restricciones de uso) | https://huggingface.co/nerijs/pixel-art-xl |
| Referencia de estilo por imagen | `h94/IP-Adapter` (`sdxl_models/ip-adapter-plus_sdxl_vit-h`) | Apache-2.0 | Sí | https://huggingface.co/h94/IP-Adapter |
| Codificador de imagen del IP-Adapter | `h94/IP-Adapter` (`models/image_encoder`, OpenCLIP ViT-H/14 de LAION) | Apache-2.0 (repositorio). El original `laion/CLIP-ViT-H-14-laion2B-s32B-b79K` es MIT. | Sí | https://huggingface.co/laion/CLIP-ViT-H-14-laion2B-s32B-b79K |
| Pose y encuadre (ControlNet) | `xinsir/controlnet-openpose-sdxl-1.0` | Apache-2.0 | Sí | https://huggingface.co/xinsir/controlnet-openpose-sdxl-1.0 |
| Inpainting (si hace falta) | `diffusers/stable-diffusion-xl-1.0-inpainting-0.1` | CreativeML Open RAIL++-M | Sí (como el modelo base) | https://huggingface.co/diffusers/stable-diffusion-xl-1.0-inpainting-0.1 |
| Software | `diffusers`, `transformers`, `accelerate`, `peft` (Apache-2.0); PyTorch (BSD-3); Pillow (MIT-CMU); NumPy y SciPy (BSD) | — | Sí | — |

## Descartados

| Modelo | Motivo |
|---|---|
| `thibaud/controlnet-openpose-sdxl-1.0` | Licencia `other`, no está claro que permita uso comercial. Se usa el de `xinsir` (Apache-2.0). |
| `lllyasviel/Annotators` (detector OpenPose y otros) | Licencia `other`; además, los pesos originales de OpenPose (CMU) solo permiten uso no comercial. **El esqueleto de pose se dibuja por código** (`Tools/portrait_gen/pose.py`), así que no hace falta ningún detector. |
| FLUX.1 [dev] | Licencia no comercial. |
| `artificialguybr/PixelArtRedmond` | Licencia válida (OpenRAIL-M), pero no se necesita: con un LoRA de pixel art basta. |

## Lo que no cubre una licencia

- **Parecido con personas reales:** el negativo excluye "real person likeness" y el personaje se describe con rasgos
  genéricos. Antes de integrar un retrato se revisa a ojo (`BRIEF.md`, punto 4).
- **Referencias de estilo (02 y 04):** son arte del propio juego. Se usan solo en las capas de estilo del IP-Adapter
  (InstantStyle), no para copiar al personaje.
- **Derechos sobre lo generado:** los modelos no reclaman las imágenes generadas. La protección por derechos de autor
  de una imagen generada por IA depende del país. Para un juego comercial, lo prudente es tratarlas como un recurso
  que puede retocarse a mano.
