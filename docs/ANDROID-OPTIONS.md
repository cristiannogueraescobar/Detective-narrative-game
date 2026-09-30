# Android: de dónde salen las respuestas de los sospechosos (decisión 1)

*Sesión extra del día 3 (30-09-2026, 21:30 → 23:30). Solo análisis: no se ha cambiado código.*

En el móvil no funciona ninguno de los dos proveedores tal cual (ver `ANDROID-BUILD.md`): Ollama apunta a
`localhost` y la clave de Anthropic se lee de un archivo que no existe en Android y **nunca** puede ir dentro del
APK. Este documento compara las cuatro salidas con **datos medidos en este proyecto** y, donde no se puede medir
aquí, con fuentes citadas.

## Resumen para decidir
- **Para probar ya en tu móvil (hoy, gratis):** Ollama del PC por la wifi (opción A1). Una tarde de trabajo como
  mucho; no sirve para nadie más.
- **Para publicar:** **Claude Haiku 4.5 detrás de un proxy propio** (opción B). Es la única que mantiene (o mejora)
  la calidad medida sin servidores con GPU: unos **4-7 céntimos de dólar por caso** completo. Hay que recalibrar
  pistas, estados y premisas con Claude (las fichas están afinadas para qwen 7B).
- **Modelo en el móvil (C/D): hoy no.** Medido con las mismas pruebas del juego: qwen2.5 **1,5B** acepta el
  **72 %** de las premisas falsas y solo suelta bien 15 de 48 pistas; **3B**, 18 % y 22 de 48. El 7B que usamos:
  2-5 % y 36-39 de 48. Un sospechoso que se traga cualquier trampa rompe el juego. Revisitar con modelos de
  móvil más capaces o reescribiendo las fichas para ellos.

## Tabla comparativa

| Opción | Calidad (medida o esperada) | Latencia por respuesta | Coste | Trabajo | Riesgos |
|---|---|---|---|---|---|
| **A1. Ollama en el PC de casa por wifi** | La de hoy (qwen 7B: pistas 80-83 %, premisas 2-5 %) | ≈ 0,6-0,9 s (medido en este PC) | 0 € | **1-2 h** (IP en *Base Url*, `OLLAMA_HOST=0.0.0.0`, build de desarrollo; ya documentado) | Solo en tu wifi y con el PC encendido. HTTP sin cifrar: solo builds de desarrollo |
| **A2. Ollama en el PC, expuesto por túnel (Cloudflare Tunnel / Tailscale) con token** | La de hoy | 0,7-1,2 s (+ red) | 0 € (túnel gratuito) + luz del PC | **0,5-1 día**: el proveedor Ollama necesita poder mandar una cabecera de autenticación (cambio en la capa de proveedores → tu decisión) | El PC es el servidor: si se apaga, no hay juego. Aguanta a pocos jugadores a la vez (una GPU, una petición cada vez) |
| **A3. Ollama en un servidor con GPU (nginx + HTTPS + token)** | La de hoy | ≈ 1 s | **≈ 200-300 $/mes** encendido 24 h (p. ej. una L4 a ~0,39 $/h); sin GPU (VPS de 10-20 $/mes) un 7B va a pocas palabras por segundo | **1-2 días** + mantenimiento (actualizaciones, certificados, caídas) | Coste fijo aunque nadie juegue; Ollama no trae autenticación: todo depende del proxy |
| **B. Claude (Haiku 4.5) vía proxy propio** (Cloudflare Worker o similar con la clave como secreto) | Probablemente **mejor** que qwen 7B en seguir reglas (premisas, mentiras, formato), pero **sin medir**: hay que recalibrar | 1-2 s (red + modelo); con *streaming* la primera palabra llega antes | **≈ 0,04-0,07 $ por caso** (cálculo abajo); el Worker, gratis hasta ~100 000 peticiones/día | **1-1,5 días**: Worker (~50 líneas, límite por instalación), URL del proveedor Anthropic configurable, modelo Haiku 4.5, recalibración (½ día con las herramientas que ya hay) | Coste variable ligado al uso (poner tope por jugador/día); abuso del proxy si no hay límites; depende de un servicio externo; privacidad: las preguntas salen del móvil |
| **C. Modelo en el móvil con llama.cpp (LLMUnity) o ONNX Runtime GenAI** | **Medida aquí: mucho peor.** 1,5B: premisas 72 %, pistas 15/48 ≥ 2/3 (media 45 %), 29 fallos de estilo. 3B: premisas 18 %, pistas 22/48 (56 %) | Estimada: primera pregunta a cada sospechoso **5-15 s** (hay que procesar ~1 200 tokens de ficha en la CPU del móvil); las siguientes, 2-4 s si se reutiliza la caché de la ficha | 0 € por uso; **+1-2 GB** de descarga (modelo aparte del APK) | **3-5 días** de integración + **reescribir y recalibrar las fichas** para un modelo pequeño (días, sin garantía) | Móviles de 6-8 GB de RAM como mínimo; batería y calor; calidad inaceptable hoy |
| **D. Híbrido: modelo pequeño descargable + nube de reserva** | La del modelo local cuando se usa (ver C) | Como C o como B | Como B en la parte de nube | **1-2 semanas** (dos caminos que mantener y calibrar) | Toda la complejidad de B y C juntas; dos comportamientos distintos del mismo personaje |

### Marcos de ejecución en el móvil (para C y D)
| Marco | Integración con Unity | Modelos | Estado |
|---|---|---|---|
| **llama.cpp vía LLMUnity** | Paquete C# para Unity (2021 LTS → Unity 6), Android e iOS; v3.0.3 de marzo de 2026 | Cualquier GGUF (qwen2.5 1,5B/3B Q4 ≈ 1-2 GB) | El más directo para este proyecto |
| **ONNX Runtime GenAI** | Paquete NuGet con enlaces C#; en Unity hay que meter a mano las bibliotecas nativas de Android (IL2CPP) | Qwen, Phi, Gemma, Llama… | Maduro; ejemplos de ~37 tok/s con un 1,7B en un móvil de gama alta de 2025 |
| **LiteRT-LM (antes MediaPipe LLM Inference)** | API Java/Kotlin: hace falta un puente (AAR + JNI) desde Unity | Gemma, Qwen 2.5 1,5B convertido, Phi… | MediaPipe tasks-genai está obsoleto; LiteRT-LM es su sucesor, con la API C++ en *preview* |

## Datos medidos en este proyecto (sesión extra)

**Calidad por tamaño de modelo** — mismas pruebas que decidieron las fichas (48 pistas × 3 intentos; sonda de 72
preguntas capciosas). Informes completos en `Logs/modelos-pequenos/` (en el PC, no versionados: `Logs/` está
en .gitignore; las cifras clave están aquí).

| Modelo | Pistas ≥ 2/3 | Detección media | Premisas falsas aceptadas | Fallos de estilo |
|---|---|---|---|---|
| qwen2.5 7B (el actual) | 36-39 / 48 | 80-83 % | 2-5 % | 8 |
| qwen2.5 3B | 22 / 48 | 56 % | 18 % | 8 |
| qwen2.5 1,5B | 15 / 48 | 45 % | **72 %** | 29 |

**Tamaño de una petición** (ficha real de la variante con más pistas, día 4, con pruebas mostradas):
**1 219 tokens** de entrada con la pregunta (3 765 caracteres, 657 palabras), más el historial (hasta 16 mensajes:
~300-700 tokens). Respuesta: 36-83 tokens (`Logs/modelos-pequenos/tokens-velocidad.md`). Las partes que cambian
(pruebas mostradas, lo ya contado, el día) van al final de la ficha: el principio es estable, y eso permite
reutilizar la caché de la ficha (en el móvil y con el *prompt caching* de Claude).

**Latencia en este PC** (RTX 5070 Ti portátil): qwen 7B 0,64 s por respuesta; 3B 0,39 s; 1,5B 0,48 s (respuestas
más largas). En el bot, media de 0,9 s y p90 de 1,2-1,3 s con 7B.

## Cálculo del coste de B (Claude Haiku 4.5)
Precios de septiembre de 2026: 1 $ por millón de tokens de entrada, 5 $ por millón de salida; lectura de caché a
0,10 $/M.
- Una pregunta: ~1 600 tokens de entrada (ficha + historial medio) y ~50 de salida → 0,0016 $ + 0,00025 $.
- Un caso completo (35 preguntas + ~5 % de reintentos): **≈ 0,065 $**. Con *prompt caching* de la ficha (cambia
  poco entre preguntas al mismo sospechoso): **≈ 0,04 $**.
- 1 000 jugadores × 3 casos ≈ **120-200 $**. Con Sonnet (lo que trae hoy `AnthropicProvider`) sería ~3 veces más.

## Recomendación
1. **Ahora:** A1 para jugar en tu móvil esta semana (sin tocar código).
2. **Si vas a publicar:** B. Pasos: Worker con la clave como secreto y límite por instalación (p. ej. N casos al
   día) → proveedor Anthropic con URL configurable y `claude-haiku-4-5` → recalibrar pistas, estados y premisas con
   las herramientas que ya existen (`ClueCalibrator`, `EmotionCalibrator`, `PremiseCalibrator`; habría que añadirles
   el proveedor Anthropic) → bot en las 9 variantes. Todo esto toca la capa de proveedores: **necesita tu visto
   bueno** (regla del día 3).
3. **C/D:** no invertir hasta que haya un modelo de móvil que pase la sonda de premisas por debajo del 10 % con
   estas fichas; la prueba es barata (`ollama pull` + las dos calibraciones, ~15 min por modelo).

## Fuentes
- LLMUnity (llama.cpp en Unity, Android/iOS): [dev.co/ai/frameworks/llmunity](https://dev.co/ai/frameworks/llmunity), [sourcepulse](https://www.sourcepulse.org/projects/1827728)
- ONNX Runtime GenAI (modelos, Android, cifras en móvil): [github.com/…/onnxruntime-genai](https://github.com/avijit-chakroborty/onnxruntime-genai), [Native-LLM-for-Android](https://www.sourcepulse.org/projects/1840135)
- LiteRT-LM / MediaPipe LLM Inference: [Google Developers Blog](https://developers.googleblog.com/en/on-device-genai-in-chrome-chromebook-plus-and-pixel-watch-with-litert-lm/), [guía Android](https://developers.google.com/edge/mediapipe/solutions/genai/llm_inference/android)
- Qwen2.5 1,5B Q4 en móvil (~1 GB, ~1,2 GB de RAM): [onenm_local_llm](https://pub.dev/documentation/onenm_local_llm/latest/), [Liquid AI: llama.cpp en móvil](https://docs.liquid.ai/deployment/on-device/llama-cpp/mobile.md)
- Proxy para no exponer la clave (Workers): [eastondev.com](https://eastondev.com/blog/en/posts/ai/20251201-workers-ai-proxy-guide/), [Cloudflare AI Gateway](https://developers.cloudflare.com/ai-gateway/integrations/coding-agents/claude-code/index.md)
- Ollama detrás de nginx/Caddy con HTTPS y autenticación: [glukhov.org](https://www.glukhov.org/llm-hosting/ollama/ollama-behind-reverse-proxy/), [dev.to](https://dev.to/baboon/securely-exposing-ollama-service-to-the-public-internetcomplete-deployment-and-remote-management-59nn)
- Precios: [Claude Haiku 4.5](https://pricepertoken.com/pricing-page/model/anthropic-claude-haiku-4.5), [RunPod (L4)](https://computeprices.com/providers/runpod/gpus/l4), [VPS para Ollama](https://www.greengeeks.com/vps-hosting/ollama/)
