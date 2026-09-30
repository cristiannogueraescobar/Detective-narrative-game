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

- 01:58 PlayMode: tests de humo con la escena real y un proveedor falso lento (escribiendo…, máquina de escribir,
  toque, fallo, continuar). Game.unity única escena de build (antes era SampleScene).
- 02:05 Jugador bot (BotPlayer + PlaythroughChecks con tests) en worktree C:\Dev\dng-bot; tanda 9×3 en marcha.
- 02:10 Retratos: EmotionPose (postura por estado, transición exponencial) + reacciones (sobresalto, sacudida y
  golpe de rojo, gotas de sudor, desaturación triste). Captura de fotogramas en juego: AnimationCapture
  (PlayMode [Explicit], `.superpowers/capture-anim.sh <filtro>`), salida docs/screenshots/<fecha>/anim/.
- 02:25 Menú: el arte se ESTIRABA (2:3 metido en 9:16) → fondo "cover" sin deformar; halo de lámpara con
  parpadeo, polvo en la luz, lluvia en el cristal, vapor del café; título con entrada de rótulo; sombras.
  Causa de títulos pequeños: GameManager reaplicaba el tema y pisaba tamaños → el tema ya no toca textos con
  TextStyle (test). Antes/después: 00-inicio/1080x1920_MainMenuPanel.png vs anim/menu_2500.png.

- 02:35 FxLayer: ficha de pista que cae, destella y vuela a la libreta (+ contador), sello CONTRADICCIÓN con
  destello y sacudida, hoja de calendario con el parte, viñeta y latido en la acusación, "El jurado delibera…",
  finales con sello propio, tinte y línea temporal línea a línea, CONFIDENCIAL en el expediente, grano y viñeta.
- 02:40 Intro como expediente sobre la sala de interrogatorios (arte que ya existía y no se usaba), iconos
  generados (Tools/make_icons.py, archivos nuevos en Assets/Art/Icons).
- 02:45 Sonido: SoundManager + AUDIO-NEEDED.md (probado contra el catálogo), clic en todos los botones.
- 02:52 Ajustes: secciones con scroll, música/efectos, tamaño de texto en caliente, filtro noir, alto contraste
  (AAA). Test de layout también con texto "muy grande". Bug: sprites generados tomados por ilustración.
- 02:55 Bot ronda 1 (27 partidas): ver Logs/bot-playthroughs.md. Pistas flojas y emoción recalibradas.
- 03:04 Instrucciones y Acerca de reescritos, HUD con "quedan N preguntas", tutorial de 4 indicaciones.
- 03:08 Build de Windows OK + prueba de humo del .exe (SMOKE OK). Android: sin módulo → docs/BUILD.md.
- 03:15 Retratos en plano medio (recorte medido del pixel art) y caras en el chat.

## Ahora
- Bot ronda 2 y recalibración (pistas 2C/3C, estados) con los últimos cambios.
- Chat con conversación real larga (transcripción del bot) en captura.

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
- Filtro noir ACTIVADO por defecto (grano 0,05, viñeta 0,45: sutil). Se quita en Ajustes.
- No se ha tocado la capa de proveedores: la clave de Anthropic en Android y la URL de Ollama en móvil quedan
  documentadas en docs/BUILD.md como pendientes.
- Tamaño de las fichas: se mantuvo el límite de 460 palabras (test) acortando reglas en vez de subirlo.
- Los retratos antiguos se muestran en plano medio (recorte), no de cuerpo entero.
