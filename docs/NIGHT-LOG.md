# Diario de la noche 2 (2026-09-30)

Inicio: 01:32. Fin previsto: ~12:00–13:30 (10–12 h).
Rama de trabajo: `feature/noche2` (sale de `feature/ui-layout`, que sale de `feature/ui-movil`).
Si el contexto se compacta: releer este archivo y seguir por "Ahora".

## Hecho
- 01:30 Arreglo de raíz del layout (rama `feature/ui-layout`, 3 commits): LayoutKit + LayoutGroups en todos los
  paneles, política de texto (TextStyle: una línea con "…", varias líneas con autoajuste, scroll a tamaño fijo),
  chat solo vertical, barra superior en dos filas, test `LayoutValidationTests` (desbordes, solapes, glifos,
  contraste WCAG, 3 pantallas × 9 paneles). 337/337.
  - Causa de los 72 px en Ajustes: TMP reinicia a sus valores por defecto los textos creados en un panel inactivo
    la primera vez que se activa. `TextStyle` reaplica en `OnEnable` (con `[ExecuteAlways]`).
  - Rojo de peligro aclarado a #E06A5E (5,2:1 sobre el panel; antes 3,2:1).
- `.vscode/`, `*.slnx` y `.superpowers/` al `.gitignore`. Worktree `dng-layout` borrado.

- 01:40 Capturas: `ScreenshotTool.CaptureFromCommandLine` (batchmode sin -nographics, cámara temporal +
  RenderTexture): todos los paneles + 4 finales + chat arriba, a 1080x1920 y 1080x2400, en
  docs/screenshots/<fecha>/<etiqueta>/. Script: `.superpowers/shots.sh <etiqueta> [escala]`. FUNCIONA.
- 01:45 Layout ronda 1: zonas táctiles de 48 dp (120 px de lienzo) comprobadas por el test, SafeAreaFitter en
  cada columna (fondo a sangre, controles dentro), scrollbar fina, campo de pregunta vacío (la escena traía la
  pregunta de ejemplo ESCRITA, no como placeholder), "Volver" secundario, retratos en la vista previa.
- 01:52 Chat de verdad: ChatEntry (guardado v2, lee v1), burbujas con mini-retrato y hora de juego, avisos
  centrados, "escribiendo…", máquina de escribir (toque = completar), auto-scroll que respeta al que lee +
  "Nuevos mensajes", pool de filas. Tests: ChatViewTests, ChatLogicTests, ConversationStoreTests, guardado.

## Ahora
- Chat: capturas con conversación real larga; revisar en juego (PlayMode test si es posible).

## Después (orden)
1. Layout: 3 rondas de render→revisión, safe area, zonas táctiles ≥ 48 px (añadir al test).
2. Chat con burbujas, "escribiendo…", máquina de escribir saltable, auto-scroll respetuoso, pooling.
3. Animación y ambientación (menú vivo, retratos por emoción, pista, contradicción, día, acusación, finales,
   expediente, filtro noir).
4. SoundManager + volúmenes + AUDIO-NEEDED.md.
5. Instrucciones, tutorial ligero, Acerca de, revisión de textos.
6. Pistas flojas (2C_gps, 3A_audios, 3C_fotos), emoción al preguntar por la víctima, jugador bot.
7. Accesibilidad: tamaño de texto, alto contraste, velocidad de texto, filtro noir.
8. Build de Windows (y APK si hay módulo).
FINAL. Rondas de revisión.

## Decisiones (opción conservadora, para revisar)
- Zona táctil mínima = 120 px de lienzo (48 dp en un móvil de 411 dp de ancho). Más estricto que "48 px".
- Guardado v2: los guardados v1 se cargan, con cada conversación antigua como un bloque de texto.
- Hora del juego: 09:00 + 2 h por pregunta (con un desfase para que no parezca un reloj), sin afectar a la lógica.
- La pregunta del jugador aparece al enviar; si falla la petición, se retira y vuelve al campo.
