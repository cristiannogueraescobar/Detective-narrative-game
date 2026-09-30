# Builds

## Windows (comprobado)

```
"C:\Program Files\Unity\Hub\Editor\6000.3.2f1\Editor\Unity.exe" -batchmode -nographics -projectPath . -executeMethod BuildScript.BuildWindows -logFile Logs/build.log
```

Sale en `Builds/Windows/Detectives.exe` (≈124 MB). También desde el menú **Detective → Build de Windows**.
Se abre como una **ventana vertical 9:16** al 90 % del alto de la pantalla, como un móvil (`DesktopWindow`;
en un monitor de 2560x1600, ventana de 810x1440). En móvil y en el editor no cambia nada.
Prueba de humo del ejecutable (arranca, comprueba escena, tema y arte, y se cierra):

```
Builds\Windows\Detectives.exe -batchmode -nographics -smoketest -logFile smoke.log
```

El log debe acabar con `SMOKE OK`. Resultado de la noche: **SMOKE OK**, sin errores.

Observado (sin resolver, acotado): en `-batchmode -nographics`, entre 1 de cada 12 y 1 de cada 4 ejecuciones
escriben `SMOKE OK` y luego no terminan de cerrarse (Unity ya ha hecho toda su limpieza). Con gráficos
(`-batchmode` sin `-nographics`, lo más parecido a abrir el juego): **6/6 limpias**. Medido: la build de las 03:08
0/25; las de después ~1/12–3/12, también sin audio; la bisección no es concluyente con esta frecuencia (los
commits del rango no tocan hilos ni se ejecutan al arrancar). Si al cerrar el juego con ventana se quedara
colgado, avísame: sería lo primero a mirar.

Para usar Claude en la build, `anthropic_api_key.txt` va junto a `Detectives.exe`. Con Ollama, el servidor tiene
que estar en `localhost:11434` del mismo PC.

## Android (módulo no instalado)

1. Unity Hub → Installs → 6000.3.2f1 → ⚙ → Add modules → **Android Build Support** (con *OpenJDK* y
   *Android SDK & NDK Tools*).
2. File → Build Profiles → Android → Switch Platform (la primera vez reimporta el proyecto; tarda).
3. Player Settings → Other Settings: *Package Name* `com.cristian.detectives`, *Minimum API Level* 26 o más,
   *Scripting Backend* IL2CPP y *ARM64* marcado (Google Play lo exige). La orientación ya es vertical.
4. `Unity.exe -batchmode -nographics -projectPath . -executeMethod BuildScript.BuildAndroid -logFile Logs/build-android.log`
   → `Builds/Android/Detectives.apk`.

**Ojo con el modelo en el móvil** (no se ha tocado la capa de proveedores, por las reglas de la noche):
- Ollama en `localhost` no existe en el teléfono: habría que apuntar `OllamaSettings` a la IP del PC en la red
  local (p. ej. `http://192.168.1.20:11434`, con `OLLAMA_HOST=0.0.0.0` en el PC) o usar Claude.
- La clave de Anthropic se busca junto al ejecutable (`Application.dataPath/..`); en Android eso cae dentro del
  APK y no se puede escribir. Habría que leerla de `Application.persistentDataPath` o pedirla en Ajustes.

## Rutas que no dependen del editor (revisado)

- Arte: en el editor se lee con `AssetDatabase`; en la build, del catálogo `Resources/ArtCatalog` (el script de
  build lo regenera antes de compilar). Incluye los iconos nuevos y el fondo de la intro.
- Audio: `Resources/Audio/...` (ver AUDIO-NEEDED.md); sin archivos, el juego suena en silencio.
- Guardado: `Application.persistentDataPath/partida.json`. Ajustes: `PlayerPrefs`.
- Tema y shader: `Resources/Theme.asset`, `Resources/Shaders/UIDesaturate.shader`.
