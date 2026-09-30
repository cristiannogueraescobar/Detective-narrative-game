# Android: ajustes, compilación y pruebas

## Estado (30-09-2026)
- Ajustes aplicados y versionados en `ProjectSettings` con `AndroidSetup.Apply` (menú *Detective → Android →
  Aplicar ajustes*; `BuildScript.BuildAndroid` los aplica solo antes de compilar).
- **Este PC no tiene el módulo de Android instalado**: no se ha podido generar el APK aquí.
- **Bloqueante para jugar en el móvil (decisión de Cristian):** los sospechosos no tienen con quién hablar tal
  como está el código (ver "Proveedor LLM en el móvil").

## Ajustes (AndroidSetup.cs)
| Ajuste | Valor | Por qué |
|---|---|---|
| Paquete | `com.cristiannoguera.casos` | **Provisional**: una vez publicado no se puede cambiar. Decídelo antes de subir nada. |
| Backend | IL2CPP, solo ARM64 | Obligatorio en Google Play para 64 bits; ARMv7 ya no compensa. |
| API mínima | 26 (Android 8.0) | La exige el lector de pantalla (TalkBack) del módulo de accesibilidad de Unity 6; deja fuera solo Android 7.x. |
| API objetivo | **36** (Android 16) | Google Play lo exige a apps nuevas y actualizaciones desde el 31-08-2026. |
| Orientación | Vertical (sin girar boca abajo) | El juego está diseñado para una mano. |
| Internet | Forzado | Los sospechosos hablan por red. |
| HTTP sin cifrar | Solo en *Development Build* | Para Ollama en el PC de casa; la versión publicada, solo HTTPS. |
| Recorte de código | Bajo | JsonUtility y la reflexión siguen funcionando. |
| Icono | `Assets/Art/Icons/app_icon.png` | Arte nuevo generado (Tools/make_app_icon.py). |
| Zona segura | Se dibuja bajo el notch (`renderOutsideSafeArea`), la interfaz respeta el área segura | Ya estaba. |

## Proveedor LLM en el móvil (decisión pendiente)
La capa de proveedores no se ha tocado (regla del día 3). Tal como está:
- **Ollama** apunta a `http://localhost:11434`: en el móvil, *localhost* es el propio móvil, no el PC.
- **Anthropic** lee la clave de una variable de entorno o de `anthropic_api_key.txt` junto al proyecto: en Android
  no existe ese archivo. **Nunca** metas la clave dentro del APK (se extrae en minutos).

Opciones, de menos a más trabajo:
1. **Ollama en el PC de casa (pruebas):** en el PC, `OLLAMA_HOST=0.0.0.0` y abrir el puerto 11434 en el
   cortafuegos de la red privada; en Unity, en *AIConversationManager → Ollama → Base Url*, poner
   `http://<IP-del-PC>:11434`; compilar con **Development Build** (el HTTP sin cifrar solo va ahí). Móvil y PC en
   la misma wifi.
2. **Servidor intermedio propio con HTTPS** que guarde la clave de Anthropic y reenvíe las peticiones (con límite
   por usuario). Es lo publicable. Requiere un proveedor nuevo en la capa LLM.
3. **Modelo en el propio móvil** (p. ej. un 1-3B cuantizado): sin red ni costes, pero calidad de sospechoso mucho
   menor que qwen2.5 7B; habría que recalibrar pistas y emociones.

Recomendación conservadora: la 1 para probar ya en el móvil; la 2 si se quiere publicar.

## Compilar
1. Unity Hub → *Installs* → 6000.3.2f1 → *Add modules* → **Android Build Support** (con OpenJDK y Android SDK &
   NDK Tools).
2. Firma (solo para publicar): *Player Settings → Publishing Settings → Keystore Manager*. Guarda el `.keystore`
   y sus contraseñas **fuera del repositorio** (no se versionan).
3. APK de pruebas:
   `Unity -batchmode -nographics -projectPath . -executeMethod BuildScript.BuildAndroid -quit`
   → `Builds/Android/Detectives.apk` (o `-buildPath <ruta>`). Para la opción 1 de arriba, marca *Development Build*
   en *Build Settings* o compílalo desde el editor.
4. Para Google Play: *Build App Bundle (.aab)* (`EditorUserBuildSettings.buildAppBundle = true`).

## Probar en el móvil
1. Activa *Opciones de desarrollador → Depuración USB*; `adb install -r Builds/Android/Detectives.apk`.
2. Recorrido mínimo: menú → Ajustes (texto *Muy grande*, *Alto contraste*, *Filtro noir* apagado/encendido) →
   caso → 3 preguntas → Pensar → libreta → mostrar prueba → Fin del día → acusar → final.
3. Rendimiento: *Window → Analysis → Profiler* conectado al móvil (Development Build + *Autoconnect Profiler*).
   Mirar el tiempo de GPU con y sin *Filtro noir* (docs/RENDIMIENTO.md estima 1-2 ms del post-proceso).
4. Muescas y barras: probar un móvil con notch y uno de 20:9 (la interfaz está validada a 1080×1920 y 1080×2400).
