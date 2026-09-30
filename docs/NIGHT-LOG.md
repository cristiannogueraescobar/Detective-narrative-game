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

- 03:18 Cabecera del interrogatorio: retrato en plano medio con sospechoso / Fin del día / Acusar al lado: el
  chat pasa de ~36 % a ~60 % de la pantalla. Captura con una conversación real del bot (35 turnos).
- 03:22 Rueda de reconocimiento en la acusación (bustos del caso, no la foto de grupo de la historia 1).
- 03:26 Recalibración: víctima "triste" 36/36 (antes tranquilo 16/36); 3A 10/10, 3C 8/10; 2C sigue 5/10.
- 03:30 Revisión de código con subagente: 8 hallazgos (0 críticos), todos corregidos con test.
- 03:40 Etiqueta de estado en el retrato, vibración (ajuste), libreta de papel, "Continuar · caso, día N".

- 03:45 Rendimiento (PlayMode): 0 B de basura por fotograma del juego en menú e interrogatorio.
- 03:47 HUD con números resaltados; sin preguntas, "Fin del día" pasa a principal. "Nuevos mensajes" verificado.
- 03:55 Selección de caso: expedientes por historia con el mejor final conseguido; la variante sigue al azar.

- 03:58 Botón Atrás de Android. Respuestas repetidas palabra por palabra: un reintento (test).
- 04:02 Calibración final de estados (último prompt): coherente 141/144; neutra tranquilo 34/36; víctima
  triste 34/36; sensible nervioso 35/36. Icono de app generado (Tools/make_app_icon.py).

- 04:05 Segunda revisión de código: 4 menores, corregidos. Build de Windows rehecha con icono: SMOKE OK.
- 04:15 Sala de interrogatorios oscura detrás del chat; desplegables: opciones de 48 dp y la lista abierta ya no
  sale desplazada media pantalla (solo se veía en juego: capturado y corregido); diálogo de reinicio compacto.
- 04:27 2C_gps resuelto: "Nadie me vio, pero mírelo en el GPS…" → 16/20 (80 %). Las tres pistas flojas ≥ 7/10.

- 04:35 Bot final 9×2: acierta al culpable 13/18, 0 rupturas "soy una IA". Libreta: tocar pista = prueba.
  InterrogationUI partido en 3 archivos parciales. Tipografía Special Elite en títulos, sellos y HUD.
- 04:46 Audio provisional sintetizado (efectos + ambientes). Test de que los efectos se limpian solos.

- 04:50 Bot ronda de revisión (2C, 1B, 3B): sin rupturas; aún 3 repeticiones → la repetición se pide de nuevo
  con una nota oculta "no repitas". Balance del caso en el informe final (día y pistas encontradas).
- 04:58 Tercera revisión de código: la fuente de rótulos estática solo tenía ASCII (acentos de otra fuente):
  nueva fuente dinámica (asset nuevo, la original intacta) y el test de glifos ahora detecta letras de otra
  familia. Enlace de la libreta bloqueado sin entrada y sin teclado en móvil.

- 05:10 Fuente de rótulos congelada a estática (Latin-1 + puntuación del juego): sin crecer en ejecución.
- 05:20 Fundido al entrar en el expediente desde la selección de caso.
- 05:33 Tinte de ambiente por historia (intro y fondo de la sala, también al continuar); margen lateral en los
  textos con scroll para que la cursiva no se corte.
- 05:35 Revisión en tableta (4:3): todos los paneles bien. Cuelgue al salir en -nographics: 1-3/12, en ventana
  0/6 y en la build 0/25; documentado en BUILD.md (no afecta al jugador).

- 05:45 Crítica de diseño (capturas nuevas): el informe final partía cifras de línea ("CONTRADICCIONES / 1"):
  un dato por línea. La línea temporal repetía la hora ("22:30  A las 22:30, …"): se quita de la frase.
- 05:50 Preguntas de ejemplo bajo un interrogatorio sin empezar (3 botones, rellenan el campo, nunca envían; se
  apartan con el campo escrito, sin preguntas o al preguntar). Van en la columna, no encima del chat: con días
  avanzados el chat de un sospechoso nuevo ya tiene partes y avisos. Tutorial e instrucciones lo mencionan.
- 05:55 4ª revisión de código (subagente): 0 críticos/importantes, 3 menores, todos arreglados con test.
- 05:58 Bot (18 partidas, 9 variantes): el 80 % de las respuestas se etiquetaban "nervioso" (el retrato ya no
  decía nada). Causa: la guía decía "nervioso si ocultas algo" y todos ocultan algo. Ahora: nervioso solo por su
  tema de TE PONE NERVIOSO. Medición antes/después en curso.

- 06:30 Medición de la guía de estados (misma semilla 59, 18 partidas): nervioso 70 % → 42 %, triste 17 → 35 %,
  tranquilo 5 → 13 %. Calibración de estados: coherente 137/144 (95 %). Culpable 11/18 → 9/18 y pistas 1,8 → 1,4
  por partida: dentro del ruido de 18 partidas, pero se comprueba con A/B de calibración de pistas (en curso).
  Falso positivo del detector ("seguir las instrucciones" de un medicamento) corregido con test.
- 06:20 Selector de pruebas: "Mostrar prueba: ninguna · N en la libreta" y salto al llegar una prueba nueva.
  Build de Windows + prueba de humo: OK.

- 07:05 A/B de calibración de pistas (48 pistas × 2 preguntas × 3 intentos, misma temperatura): guía antigua
  40/48 (85 % de media), guía nueva 44/48 (84 %). Las pistas que fallan cambian de un brazo a otro: ruido. La guía
  nueva no cuesta pistas; la bajada del bot era ruido. Informes en Logs/bot-guia2/.
- 07:00 Acusación: "En tu libreta: N pistas y M contradicciones" / "acusar ahora es una apuesta". Maruxa habla
  español con toque gallego (su ejemplo en gallego provocaba respuestas enteras en gallego, ~1 de 60).

## Ahora
- Ronda 4: bot en 3 variantes al azar sobre HEAD, crítica de diseño, revisión de código.

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
- 2C_gps: en vez de meter el GPS en su versión (el test de datos lo prohíbe: la ficha revelaría la pista sola),
  la línea de Maruxa del cartero en 2C solo la nombra (sigue desbloqueándola) y el hecho empieza por "Nadie me
  vio, pero…", que es como respondería a "¿alguien puede confirmarlo?".
- Vibración activada por defecto (solo en móvil).
- Audio PROVISIONAL sintetizado por código (no lo he podido escuchar): niveles discretos medidos (picos 0,3–0,7,
  empalmes de bucle sin salto). Si no gusta, borrar Assets/Resources/Audio y el juego queda en silencio.
- productName sigue siendo "Casos" (cambiarlo movería guardados y ajustes del jugador).
