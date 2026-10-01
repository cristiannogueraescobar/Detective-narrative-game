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

- 07:06 Ronda 5: bot en 2B, 2C, 3A (semilla 5): culpable 4/6, estados nervioso 33 % · triste 29 % · tranquilo
  21 %. 5ª revisión de código: 0 críticos/importantes, 3 menores arreglados (con test). Tests 507/507 + PlayMode
  12/12 (+12 capturas explícitas).

- 07:10 Ronda 6 (diseño): "Fin del día" está justo bajo el selector de sospechoso y terminaba el día al
  momento aunque quedasen preguntas: un toque por error costaba hasta 5. Ahora pide confirmación si quedan
  ("Te quedan 3 preguntas y se perderán"); Atrás la cierra. Diálogo de confirmación común con el de reiniciar.

- 07:15 Checker del bot: los nombres que dijo el inspector en la pregunta no son inventados (Ruiz, Opel,
  Granada eran falsos positivos). Resumen de la acusación al 90 % (legible en "Muy grande"). Parte de la mañana:
  sin pistas el día 3, consejo sin spoilers; test de que el parte más largo cabe en la hoja del día (y de que el
  test detecta un parte que no cabe).

- 07:25 Libreta: tocar el nombre de un sospechoso lleva a interrogarle (no mientras se espera respuesta). Los
  enlaces de la libreta perdonan un toque cercano (a menos de media zona táctil): una línea de texto mide mucho
  menos de 48 dp.

- 07:37 Ronda 7: bot en 3B, 3C, 2A (semilla 11): culpable 2/6, triste 53 % (historia 3: casi todo gira en torno a
  Paula, así que la guía hace lo que dice; se vigila). 7ª revisión de código: 0 críticos/importantes; arreglados
  con test el diálogo reutilizable (nombres fijos), la indicación del tutorial encima del aviso de fin del día y
  el parte sin línea en blanco. Decisiones: un enlace de sospechoso tocado mientras se espera respuesta no hace
  nada (igual que los de pistas); el aviso no se recolorea si cambia el alto contraste con él abierto (los
  ajustes solo se abren desde el menú). Tests 520/520 + PlayMode 16/16.

- 07:42 El informe final solo dice cuánto faltó si acertaste sin condena segura. La build de Windows se abría a
  pantalla completa con la interfaz vertical estirada en un monitor apaisado: ahora es una ventana 9:16 al 90 %
  del alto (comprobado en la build real: 2560x1600 → 810x1440). Prueba de humo OK.

- 07:50 Sonda nueva de premisas falsas (editor): "Un testigo le vio discutir a gritos con X. ¿Por qué discutieron?"
  a los 36 personajes × 2. Línea base: 17/72 (23 %) aceptan la discusión inventada, a veces con detalles y horas
  inventados. Regla nueva en la ficha: "Si el inspector afirma algo que no está en tu ficha, niégalo." Para no
  pasar de 460 palabras se acortaron líneas comunes ("DÍA n", "NO SABES (dilo si te preguntan)", la guía de
  estados). Midiendo: premisas, estados y pistas (si empeora algo, se revierte).

- 08:17 Premisas con "afirma": 5 %; con "te atribuye": 2 % (y las dos son la custodia real de la historia 3).
  Pero la detección de pistas baja (sin regla 85 / 84 %; con regla 83 / 80 %) y 3A_audios falla: la madre niega
  que Javier la amenazase. Brazo C en curso (recortes sí, regla no) para separar la causa. Galería regenerada.

- 08:25 Brazo C (recortes sí, regla no): 81 %. La dispersión 80–85 % es sobre todo ruido, pero 3A_audios falla
  en los dos brazos con regla y pasa en todos los demás, y es una de las pistas flojas que había que subir.
  Decisión conservadora: REVERTIDA la regla y los recortes (la ficha vuelve a la versión medida). Experimento D
  en el worktree: "Si el inspector dice que hiciste algo que no está en tu ficha, niégalo" (no debería tocar
  "¿Le había amenazado Javier?"). Solo se adopta si 3A_audios aguanta y las premisas mejoran.

- 08:30 Experimento D ("dice que hiciste"): premisas 17/72 (23 %), igual que sin regla; 3A_audios 18/20. Las
  redacciones que funcionan cuestan una pista y la inocua no hace nada: la regla queda revertida y en pendientes.

- 08:33 Ronda 8: la validación de layout no cubría el alto contraste: ahora los 10 paneles pasan también con
  alto contraste (desbordes, glifos, contraste WCAG, zonas táctiles). Captura de alto contraste revisada.
  Tests 544/544. Revisión de código (8ª) en curso.

- 08:46 8ª revisión: 0 críticos/importantes. Ventana de escritorio al 85 % (cabe en un portátil de 768 px con la
  barra de tareas) y el reproductor arranca en ventana 540x960 sin cambio a pantalla completa (sin destello ni
  Alt+Intro estirando la interfaz). La build de siempre falló: Windows niega crear Builds/Windows/Detectives.exe
  (solo esa ruta, sin proceso vivo; sin admin no veo la causa). BuildScript acepta -buildPath; build final en
  Builds/Windows-final, verificada (ventana 765x1360 en 2560x1600) y prueba de humo OK.

- 08:52 Bot ronda 8 (3C, 3A, 3B, semilla 17): culpable 2/6, pistas 0,5–1,5/5. En 3C el detective se obsesiona
  con el humo (la pista falsa de la historia) y nunca pregunta por el último contacto con Paula, que es donde
  está la pista clave. Idea: elegir las preguntas de ejemplo con datos. Sonda nueva (SuggestionProbe): cada
  candidata como primera pregunta a los 36 personajes × 2, contando pistas con el análisis real. En curso.

- 09:10 Sonda de sugerencias (2 pasadas): "¿Qué relación tenías con…?" 1 y 0 pistas; "¿Cuándo supiste de X por
  última vez?" 14. Las preguntas de ejemplo pasan a ser dónde estabas / cuándo supiste de X / algo raro.
  Tests 548/548 + PlayMode 17/17.

- 09:21 Ronda 9: bot en 2B, 1B, 1A (semilla 23): culpable 4/6; estados nervioso 45 % · triste 32 % · tranquilo
  13 %. Alucinación a vigilar: la vecina de 1A "ve" a Elena a las 00:05 (ya muerta) y lo repite; límite conocido
  del 7B (horas y hechos inventados, en pendientes). 9ª revisión: sin problemas bloqueantes; sondas que fallan
  con Ollama apagado y -buildPath con nombre suelto, arreglados con test. Galería con las preguntas finales.

- 09:36 Revisión de toda la rama (subagente, modelo más capaz): 0 críticos/importantes. Arreglados con test:
  empezar otro caso con una partida a medias ahora pregunta antes de borrarla; el aviso de fin del día retira la
  indicación del tutorial que lo tapaba; Atrás con una lista desplegable abierta solo cierra la lista. Decisión:
  el caso raro "falla algo tras contar la pregunta" (excepción interna) queda anotado, sin cambio.
  Tests 549/549 + PlayMode 20/20.

- 09:37 ART-NEEDED: los estados de retrato pedidos por personaje salen ahora de los datos (frecuencias tras la
  guía nueva, ~1.500 respuestas): p. ej. Javier, Maruxa y Álex piden "triste" en vez de "enfadado".

- 09:50 Bot definitivo (18 partidas, semilla 59): culpable 9/18, pistas 1,7, horas inventadas 26/630 (4,1 %),
  0 rupturas reales (el único "IA" era "No puedo proporcionar un horario exacto": detector afinado con test).
  Estados: triste 40 % · nervioso 39 % · tranquilo 15 %. Build final en Builds/Windows-final, SMOKE OK.
  Reporte rápido enviado al usuario a petición suya.

- 09:54 Ronda 10: los tests en una copia limpia de la rama (worktree en HEAD): EditMode 549/549, PlayMode 20/20;
  nada depende de archivos sin subir. Horas inventadas de la definitiva: casi todas rutinas aproximadas
  ("me acosté sobre las 23:00"); las dañinas son avistamientos inventados (anotado en el informe).

- 10:03 Ronda 11: bot en 1A, 3B, 3C (semilla 31): culpable 1/6, 0 rupturas, 3 horas inventadas; estados triste
  49 % · nervioso 35 %. La historia 3 sigue siendo la más difícil para el bot (se centra en el humo).
- PAUSA pedida por el usuario. Todo comiteado en feature/noche2. Tests: EditMode 549/549, PlayMode 20/20.
  Build final en Builds/Windows-final (SMOKE OK).

## Ahora
- EN PAUSA. Lo siguiente que iba a hacer:
  1. Añadir la ronda 11 a la tabla del bot en docs/REPORT-NIGHT2.md.
  2. Más rondas finales (bot en 3 variantes, revisión de código, crítica de capturas) hasta ~11:30.
  3. Cierre: cifras finales en el informe, build final si cambia código, borrar el worktree C:\Dev\dng-bot
     (git worktree remove --force + prune) y mensaje final.

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
(Todo lo anterior está hecho; ver "Hecho".)

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
- Preguntas de ejemplo: rellenan el campo pero nunca envían (la pregunta se gasta solo al pulsar Enviar). Elegidas
  con datos (SuggestionProbe).
- "Fin del día" pide confirmación solo si quedan preguntas; con el día gastado sigue siendo un toque.
- Regla contra preguntas capciosas REVERTIDA: bajaba la aceptación de premisas falsas del 23 % al 2–5 %, pero
  costaba la pista 3A_audios (una de las flojas que había que subir). Queda en pendientes con datos.
- La build de Windows se abre en ventana vertical 9:16 (85 % del alto) y sin pantalla completa: es un juego de
  móvil. En móvil y en el editor no cambia nada.
- Build final en Builds/Windows-final (la ruta de siempre quedó bloqueada por Windows; BuildScript acepta
  -buildPath). No he tocado permisos ni el antivirus.
- Maruxa habla español con toque gallego (su ejemplo en gallego provocaba respuestas enteras en gallego).
- README.md no se ha tocado (desactualizado; es la cara pública del repositorio).

---

# DÍA 3 (30 sep 2026, 10:09 → ~22:00) · rama feature/dia3

Encargo: 12 h para que el juego mejore notablemente en lógica, jugabilidad y aspecto (bloques 0, A, B, C, D, E y
rondas finales). Checkpoint de 6 h (~16:00): push y "Informe intermedio" en docs/REPORT-DIA3.md.

Skills disponibles en esta sesión: superpowers (planes, TDD, debugging, verificación), design:* (design-critique,
accessibility-review, ux-copy, design-system). NO están instalados: design-skills, unity-perf, finecomb ni el
plugin de Unity (ui-ugui, optimize-text-mesh-pro, 2d-pixel-perfect, urp-postprocessing, audio-*, sprite-atlas):
sus funciones se cubren con subagentes de revisión, búsqueda web y medición propia (se indica en cada tarea).

## Hecho (día 3)
- 10:09 Cierre de la noche 2 (fila de la ronda 11). Rama feature/dia3 desde feature/noche2.
- 10:12 0a: Builds/ está en .gitignore; ninguna build ni blob > 5 MB en ninguna rama. 0b: sin secretos en los
  205 commits sin subir (patrones de claves de Anthropic, GitHub, AWS, contraseñas, claves privadas);
  anthropic_api_key.txt ignorado. Push de feature/noche2 y feature/dia3 a origin.

- 10:20 0c (skill: superpowers TDD + subagente de análisis, modelo más capaz): los 9 fallos del bot definitivo.
  8 son sobre todo culpa del bot (tenía pistas suficientes y acusó mal, ignoró pistas de descarte o repitió la
  misma pregunta ~30 veces); 1 es del juego (2C_1: dos pistas dichas y no detectadas). Arreglos del juego, con
  la frase real del bot como ejemplo positivo en los tests (RED→GREEN):
  - detección: 2B_manguera ("vino a lavarla"), 2C_puerto ("grabado algo importante… no se fiaba de la policía"),
    2C_opel ("el inspector Ruiz con su coche"), mentira de Lucía en 3B ("estaba en Granada… toda la jornada");
  - diseño: 1B_luz ahora destapa la mentira de la madre; Javier sabe que Lucía conduce un Ibiza rojo (en 3B nada
    unía el coche rojo con ella); temas más amplios donde los testigos se cerraban (1A_ventana, Maruxa, 3C);
    la versión de Lucas (1B) y la de Maruxa ya no contradicen sus propias pistas; Carmen (1A) no sabe quién cerró
    el cuarto (se lo inventó y el bot acusó por eso). Tests 561/561.
- 0e: la "ruptura" de 1C era "No puedo proporcionar un horario exacto, inspector": falso positivo, corregido con
  test al final de la sesión anterior.
- 0d en curso: A/B del reintento por horas inventadas (18 + 18 partidas, semilla 59) en el worktree.
- B1 hecho: docs/RESEARCH.md (subagente con búsqueda web; fuentes enlazadas, decisiones anotadas).

- 10:24 0f/0g: el nombre llegó sin rellenar ("[NOMBRE]"): se queda "Detectives" (lo visible sale de
  GameTexts.GameName; el título del menú ya no está fijo en la escena) y productName "Casos" sigue decidiendo la
  carpeta de guardado: decisión para Cristian. README reescrito (inglés, portafolio; secciones personales intactas).
- 10:28 A1 (skill: superpowers TDD): NarrativeValidator + 21 tests. Cada regla se demuestra rompiendo una
  historia válida; las 9 variantes pasan. Reglas: reparto 4–5; la mentira la contradice una pista ⚡ de un
  inocente; descartes válidos; partes sin pistas gratis ni spoiler; epílogo sin horas nuevas; una edad por
  persona; línea temporal (nadie en dos sitios; horas de las pistas en ella). Líneas temporales de las 9
  variantes en CaseTimelines.cs. TimeCheck entiende horas en letra ("las seis y media").
- 10:33 A2 (subagente editor de novela negra): 17 incoherencias corregidas (docs/STORY-AUDIT.md), p. ej. el parte
  de 2A decía que nadie había mirado la cámara que Ruiz sabe desenchufada; en 2C un inocente contradecía su
  propia coartada; en 1C Daniel hacía dos llamadas en el mismo minuto; 3B situaba Ayamonte en Portugal; en 3C el
  conocimiento libre de Lucía destripaba su secreto. Todo dentro de las 460 palabras.
- 10:37 Checklist del usuario (añadida al bloque A) en docs/COHERENCE.md: veredicto por variante + matriz
  generada desde los datos (CoherenceReport). Todo ✓ tras los arreglos; limitaciones de qwen documentadas.
- 10:42 A3: tabla de finales ampliada (0 evidencia, solo contradicción, umbrales) y StateMachineTests (7 tests
  con toques reales): sin preguntas, último día sin vuelta atrás, acusar el día 1 con doble toque, cerrar a
  mitad de respuesta, cambiar de sospechoso con petición en vuelo, Ollama caído y vuelta, dobles pulsaciones.
  Tests: EditMode 589/589, PlayMode 27/27 (+14 capturas).

### BLOQUE A TERMINADO (HECHO CUANDO: ✓)
- El validador pasa en las 9 variantes (con línea temporal); STORY-AUDIT.md y COHERENCE.md recogen lo encontrado
  y lo corregido; cada transición de la máquina de estados tiene su test.
- Lo que queda abierto: preguntas capciosas (23 %, limitación de qwen) y el ritmo de la vecina (se desbloquea
  pronto y lleva la pista decisiva en 4 variantes): decisión de diseño, anotada.

- 11:03 0d DATOS (A/B, 18 + 18 partidas, semilla 59, mismo código salvo el interruptor):
  | | sin reintento | con reintento |
  |---|---|---|
  | horas inventadas | 27 en 619 respuestas | **7 en 630 (−74 %)** |
  | reintentos | — | 17 (2,7 % de las respuestas; 13 mejoran) |
  | latencia media / p90 | 1012 / 1317 ms | 1009 / 1338 ms |
  | culpable acertado | 11/18 | 10/18 (ruido) |
  **Decisión: reintento activado.** Coste de latencia despreciable. Las premisas falsas quedan como limitación
  conocida de qwen (docs/COHERENCE.md). BLOQUE 0 TERMINADO (ramas en origin sin builds ni secretos; fallos (b)
  corregidos; datos de 0d aquí).
- 10:50 B2 (skill: superpowers TDD): "Pensar" (ayuda por niveles, desde los datos, con coste) y dificultad
  Historia/Detective/Veterano (Ajustes; guardada con la partida). Rango del detective y resumen del caso
  (pendiente de conectar a la pantalla final).
- 11:00 Ampliación del usuario, profundidad narrativa: docs/REAL-CASES.md (casos reales y patrones, con fuentes;
  aviso ético: la vecina de la historia 1 comparte nombre con la madre condenada del caso real → decisión para
  Cristian), Resources/Stories/StoriesDatabase.json (carácter, herida, cómo se le nota al mentir, progresión bajo
  presión, cómo sostiene la mentira el culpable) que entra en las fichas, y desbloqueos naturales: preguntar por
  el tema trae al personaje; si no, un hecho del parte (un agente lo lleva a comisaría) en el día de su historia.
  Límite de palabras 460 → 540 PROVISIONAL: calibración en curso (pistas, estados, premisas).

- 11:15 Profundidad MEDIDA (fichas de hasta 540 palabras frente a las de 460): pistas 42/48 y 83 % de media
  (antes 39–44/48, 80–85 %), estados 95 % coherentes (igual) y 99 % bien formados, premisas 22 % (antes 23 %).
  **Decisión: el límite queda en 540.** B2 completo: prueba clave, rango, resumen con "lo que se te escapó",
  mejor rango en el expediente. B3: el bot usa "Pensar" como un jugador atascado; medición en curso.
- 11:17 C1 (skill: design:design-critique) → docs/DESIGN-CRITIQUE.md. Primer arreglo: el chat se ancla abajo
  (una conversación corta queda junto al campo; se acabó el hueco en mitad de la pantalla principal).

- **11:32** C1/C2 cerrado: escala de radios en el tema (6·12·16·20·36), sin radios sueltos en el código
  (skill: design-critique, fila "Radios" → Arreglado). 2C_gps con 10 intentos: 17/20 (85 %): arreglo del juego confirmado.
- **11:50** C3 hecho: prototipos 2D / 2.5D / vóxel 40 / vóxel 64 con los mismos 4 retratos (captura
  CharacterStyleCapture). Vóxel descartado (pierde la cara; el arte está reescalado, no es pixel art limpio;
  9-23 k vértices + cámara por retrato). **2.5D elegido**: shader LitPortrait (relieve desde alfa/luminancia,
  lámpara cálida, contraluz frío en el filo). Tres iteraciones con capturas (halo azul → banda interior → filo).
  En los 12 personajes, rueda y mini-retratos del chat (antes sin gradación). Interruptor: Theme.portraitLit.
  Tests: LitPortraitTests (5) + ChatViewTests. EditMode 629/629 antes del último cambio. Skills: TDD.

- **12:00** C4 hecho: NoirPostFx (URP) — con "Filtro noir" los lienzos raíz pasan a la cámara y un Volume global
  en memoria aplica contraste +6, saturación −8, virado frío/cálido y bloom solo por encima de gamma 0,95 (el papel,
  lo más claro del tema, es 0,91: el texto no brilla). Grano y viñeta siguen en FxLayer (respetan Reducir
  animaciones). Interruptores: ajuste "Filtro noir" y Theme.postFx. **Contraste medido después del post-proceso**
  (muestras del tema pasadas por la cámara real): los 10 pares suben (p. ej. secundario/panel 5,85 → 6,08,
  Acusar 5,73 → 6,61). Tests: NoirPostFxTests (5; el de píxeles necesita GPU: capture-anim.sh NoirPostFxTests).
  EditMode 630/630, PlayMode 32/32. Coste en móvil: se mide en D2.
  Visto de paso: la parte vacía de los deslizadores casi no se ve (WCAG 1.4.11, 3:1 en componentes) → C6/rondas.

- **12:02** B3 medido (semilla 59, 18+18 partidas): con "Pensar" primera pista 10,3 → 6,9 preguntas, partidas en
  blanco 2 → 0, turnos vacíos 96 → 88 %, resueltas 44 → 72 %. En GAME-DESIGN.md. **Bloque B: HECHO CUANDO cumplido**
  (investigación, documento, 4 mejoras, medición antes/después).

- **12:18** C5 hecho: Tools/make_story_art.py (determinista) genera las 3 intros en pixel art 1/4 con paleta por
  historia y tramado Bayer: 1) urbanización con lluvia, ventana de Elena encendida, farola; 2) cala al amanecer,
  guirnaldas, faro, bar La Marea, una figura sola; 3) olivar, cortijo, camioneta y la columna de humo. Cielo oscuro
  arriba (el expediente se lee encima), la escena en el cuarto inferior. Cuatro iteraciones con capturas por
  historia (nueva captura IntrosPorHistoria). El arte nuevo ya no pasa por la gradación del arte antiguo (salía casi
  negro): Theme.introArtBrightness. Cabeceras NO generadas a propósito (repetirían la escena). Iconos: ya estaban.

- **12:28** C6 hecho (skill: ux-copy): ~120 textos revisados; tabla y glosario en DESIGN-CRITIQUE.md. Dos
  errores de lógica escondidos en textos: el expediente y el tutorial decían "cinco preguntas" en cualquier
  dificultad, y el aviso "no se ha descontado" salía también cuando la pregunta SÍ se había gastado. Errores con
  qué hacer, un solo aviso de reinicio con botones de acción, "PISTA NUEVA" en todas partes. Deslizadores: la parte
  vacía la repintaba el tema como fondo (1,5:1) → Theme.sliderTrack ≥ 3:1 (WCAG 1.4.11), test que lo cubre tras
  ThemeApplier. EditMode 638/638, PlayMode 32/32. **Bloque C: HECHO CUANDO cumplido** (falta la galería antes/después
  final, que va en el informe).

- **12:33** D1 hecho: clic, máquina, sello, golpes y lluvia ya estaban sintetizados (noches anteriores); añadido el
  **motivo noir** del menú (La menor, Am–Fmaj7–Dm6–E7b9, piano aditivo + contrabajo pizzicato + escobillas, 8
  compases a 64 ppm = 30 s exactos: el bucle no pierde el pulso; RNG propio, el resto de archivos idénticos) y la
  **mezcla** en código (SoundMix: la música se aparta ≈ 7 dB bajo los golpes y vuelve en 1,5 s; sin AudioMixer, que
  solo se crea con API interna del editor). Tools/check_audio.py: pico, DC, silencio inicial, salto de bucle → todo
  bien. Tests: SoundMixTests (5) + SoundMixPlayTests. EditMode 650/650. AUDIO-NEEDED.md al día.

- **12:35** D2 hecho: docs/RENDIMIENTO.md. GC en reposo: el juego ≤ 0 B por fotograma (menú e interrogatorio, restado
  el editor). Coste de C3/C4 medido con la nueva captura CosteVisual (1080×1920, GPU sincronizada): ruido en la RTX
  (0,56-0,63 ms en las 4 combinaciones); en móvil se estima < 0,2 ms el relieve y 1-2 ms el post-proceso, que se
  quita con el ajuste "Filtro noir". Medir en Android real queda en Pendientes. Atlas/pooling revisados (documentado).

- **12:36** D3 lanzado: revisión independiente en segundo plano (agente nuevo, modelo más capaz) sobre el diff del día.
- **12:39** E (Android): AndroidSetup.cs aplica y versiona paquete provisional, IL2CPP ARM64, API 25-36 (Google Play
  exige 36 desde el 31-08-2026, investigado → RESEARCH.md), vertical, Internet, HTTP solo en desarrollo; BuildAndroid
  lo aplica solo. docs/ANDROID-BUILD.md. **Sin módulo Android en este PC: no hay APK.** Hallazgo importante: en el
  móvil ningún proveedor funciona tal cual (Ollama en localhost; la clave de Anthropic se lee de un archivo que no
  existe en Android) → Decisiones para Cristian, con 3 opciones.

- **12:40** E: build de Windows recompilada (Builds/Windows-final/Detectives.exe, 131 MB, 0 errores, 14 s) y prueba
  de humo **SMOKE OK**; la prueba ahora comprueba también el arte de las historias y el shader de relieve en la
  build. **Bloque E: HECHO CUANDO cumplido salvo el APK** (sin módulo Android en este PC; documentado).

- **12:47** Ronda final 1: capturas 1080×2400 revisadas (retratos 2.5D en la rueda, intros, deslizadores, avisos: bien).
  Bot en 3 variantes al azar (1A, 2B, 1B; semilla 101): 6 partidas sin errores, sin nombres inventados ni
  confesiones, primera pista a las 6,0 preguntas, 3/6 resueltas. 2B perdida dos veces: **culpa del bot** (tenía
  furgoneta + manguera + las imágenes que descartaban a Marcos y acusó a Marcos; una vez sin decisión válida).
  Galería antes/después: 20 hojas (docs/screenshots/2026-09-30/galeria, Tools/make_gallery_dia3.py).
- **12:55** D3: la revisión independiente llegó → docs/CODE-REVIEW.md (0 críticos, 2 importantes, 11 menores).
- **13:02** D3 resuelto: los 13 hallazgos arreglados en una pasada, cada uno con su test en rojo primero (tabla en
  CODE-REVIEW.md). Importantes: el rango ya no contradice el final; la ayuda concreta queda en el chat que se ve.
  El n.º 7 medido con el bot (hermano de la historia 3 el día 1 en 8/12 partidas por la pregunta de ejemplo) y
  corregido con un test que impide que ninguna pregunta de ejemplo desbloquee a nadie. COHERENCE.md regenerada.
  EditMode 666/666, PlayMode 36/36. **Bloque D: HECHO CUANDO cumplido.**

- **13:08** Ronda final 2, recorrido mental de jugador nuevo: dos huecos. (1) Quien se atasca no sabe que existe
  "Pensar" (vive en la libreta) → el consejo de atasco del parte lo menciona si la dificultad tiene ayudas. (2) La
  prueba clave no se explicaba; primero la puse en la pregunta de la acusación, pero la captura mostró que sin
  pistas el selector no aparece → la explicación va en el resumen de la libreta, solo cuando hay pistas. Tests.
  Unity activó solo UnityConnectSettings (servicios): revertido, no se sube.

- **13:23** Ronda 2 (bot, 2A/2C/3A/3C, semilla 202): 6/8 resueltas, primera pista 5,6, sin errores; desbloqueos tras
  el arreglo n.º 7: Maruxa el día 2 en 4/4 (antes día 1 en 5/14), Álex el día 1 en 2/4 (antes 8/12; lo que queda
  es que otros lo mencionan, que es lo buscado). Auditoría de accesibilidad (skill accessibility-review, WCAG 2.2
  AA) → DESIGN-CRITIQUE.md: la flecha de los desplegables (20 px, ~1,5:1 en pantalla aunque fuera blanca) pasa a
  una V propia de 36 px en textSecondary; el validador ahora exige tamaño y contraste de la flecha. Pendientes
  anotados: foco visible con teclado (PC) y lector de pantalla (Unity 6 lo permite; exige API 26 → RESEARCH.md).
  EditMode 668/668.

- **13:26** ERROR MÍO: `git add docs` en ff30092 subió 512 capturas en bruto (~300 MB). Retiradas en 98f436d y
  añadidas a .gitignore; no reescribo la historia remota sin permiso → Decisiones para Cristian (fusionar con
  --squash o limpiar con rebase + force-with-lease). A partir de aquí, solo `git add` de rutas concretas.

- **13:42** Ronda final 3 (crítica de los finales): (1) "EVIDENCIA 0/6" junto a "pistas encontradas: 0 de 5"
  parecían dos cuentas de lo mismo → "SOLIDEZ DE LAS PRUEBAS 0 de 6" y "Con 2 puntos más de solidez…". (2) Los
  finales tenían media pantalla vacía y nunca enseñaban la cara del culpable → **ficha policial** (Mugshot.cs):
  foto del culpable con el relieve 2.5D en una cartulina torcida, "CULPABLE / nombre" a máquina, dentro del
  informe tras la verdad (se desplaza con él, nunca tapa texto) y aparece al terminar de descubrirse. Tres
  iteraciones con capturas (pie diminuto → nombre cortado → dos líneas). Theme.endingMugshot la quita. Nueva
  captura veredicto_ficha. EditMode 669/669, PlayMode 36/36.

- **13:52** Ronda final 3, accesibilidad: **lector de pantalla** (TalkBack/VoiceOver) con el módulo de Unity 6
  (ScreenReader.cs): jerarquía de lo visible en orden de lectura, sin lo tapado por otra pantalla (raycast en el
  centro de cada elemento), nodos que se activan como un toque (botones, casillas, desplegables, campos,
  deslizadores ±10 %), y anuncios de respuestas, pistas, contradicciones, parte del día y veredicto. Solo trabaja
  con el lector del sistema activado (coste cero si no). El tamaño de letra del sistema elige el de la primera
  partida. ScreenReaderTests (4, forzando el lector en el editor): el primer intento dejaba el menú "debajo" de la
  selección de caso → oclusión. API mínima de Android 25 → 26 (la exige el módulo). Sin móvil no está probado con
  TalkBack real: anotado. Un test de sonido dependía del orden (música aún apartada por el test anterior):
  arreglado esperando a que vuelva. EditMode 675/675, PlayMode 40/40.

- **14:01** Ronda final 4: en 1080×2400 todo el alto extra iba al chat y, al empezar, quedaba una franja vacía →
  TallScreenHeader: el retrato se lleva el 35 % del alto que pasa de 1920 (+168 px en 20:9; el chat sigue ganando
  +312) y los controles se centran a su lado. A 1920 no cambia nada (test). Theme.tallScreenHeaderShare = 0 lo
  quita. Tests: TallScreenLayoutTests (2; en el editor, abrir la escena descargaba el tema de prueba → DontSave).

- **14:08** Ronda 4: bot en 3C, 3B, 1A (semilla 404): 6/6 resueltas, primera pista 7,8, latencia media 902 ms, sin
  errores. La "confesión" marcada en 1A ("No, solo fui yo" a "¿fueron juntos a la habitación?") es un falso
  positivo del detector: es su coartada, no una confesión. Build de Windows con todo lo de la tarde: SMOKE OK.
  Foco visible con teclado/mando (WCAG 2.4.7) sin que el botón tocado se quede encendido (FocusVisibleTests).
  Galería regenerada (21 hojas). Revisión independiente de la tarde en curso. EditMode 681/681, PlayMode 40/40.

- **14:31** Revisión independiente de la tarde (0 críticos, 3 importantes, 8 menores) → CODE-REVIEW.md, todo resuelto
  con test: el lector de pantalla ya no manda el foco arriba tras cada respuesta, conserva los nodos (y el foco) al
  cambiar textos o valores, tiene zonas desplazables, respeta la transparencia; origen de los marcos confirmado
  con el manual; la cabecera alta se mide por proporción en el área segura con el lienzo real de un 20:9.
  **Lógica: premisas falsas 22 % → 2 %** con la regla "si el inspector da por hecho algo que no está en tu ficha, di
  que no te consta; lo que sí está, confírmalo" (medido en el worktree: pistas 42/48 y 83 % igual, 3A_audios 5/6,
  estados 95 % igual). Presupuesto de ficha 540 → 565 (medido con las fichas completas). COHERENCE.md: cerrada.
  Detector de confesiones del bot: "solo fui yo" ya no cuenta. EditMode 681/681, PlayMode 45/45.

- **14:40** Ronda 5: bot en 2C, 3A, 1A con la regla de premisas ya en el juego: 5/6 resueltas, primera pista 6,3,
  sin rupturas ni confesiones. Idea nueva de jugabilidad (Her Story / L.A. Noire: comparar lo que dicen con las
  pruebas): **la libreta apunta la versión de cada sospechoso** en cuanto ha contestado ("Dice: «…»"), sangrada
  bajo su nombre; las instrucciones lo explican ("si una pista no cuadra con lo que alguien dice, enséñasela").
  Medición en curso: A/B con el bot (semilla 59, 18 + 18, -noVersions) mirando contradicciones y resueltas.

- **14:51** Ronda 5, capturas de pantallas no revisadas hoy (Acerca de, tutoriales, avisos, desplegable abierto):
  la lista del desplegable se abría desplazada con la primera opción cortada — las opciones medían 48 dp pero el
  contenido de la plantilla seguía en 28 px y uGUI calculaba un hueco negativo. Arreglado (el validador lo exige
  ahora en los tres desplegables). La ficha policial suena con un sello suave al caer. EditMode 683/683.

- **15:01** Sonido, ronda 5: cada historia con su motivo (antes solo ambiente): 1) piano solo en Re menor y reloj de
  pared; 2) frase de lengüeta en Mi eolio, como gaita lejana, sobre el mar; 3) guitarra punteada (Karplus-Strong) con
  la cadencia andaluza sobre el viento. Comprobado por espectro (notas de cada compás): los drones viejos chocaban
  (Si-Fa# bajo La menor, Do-Sol bajo Mi eolio) → retocados al tono del motivo; un sed se llevó por delante el drone
  del menú y lo devolví (menu.wav idéntico al anterior). check_audio: todo bien.

- **15:07** En el brazo "sin versiones" del A/B, una partida de 3A se fue al chino: el detective del bot (qwen)
  escribió medio en chino, el sospechoso contestó en chino y el historial lo arrastró toda la partida (9
  respuestas). El jugador no escribe en chino, pero qwen puede pasarse solo: **reintento por idioma** en
  AIConversationManager (LanguageCheck: CJK, cirílico, árabe; una vez, "Responde solo en español", se queda la que
  menos caracteres extraños tenga) + el bot vuelve a decidir si su pregunta sale en otro alfabeto. Tests (2).
  BotPlayer.cs tenía finales de línea CR CR LF
 (una edición mía de la tarde): normalizado.

- **15:13** Tutorial: quinta indicación, la primera vez que la libreta ya apunta lo que dice alguien ("Si una pista
  no cuadra con su versión, enséñasela: así se pilla una mentira"). Test de juego. EditMode 686/686, PlayMode 46/46.

- **15:23** A/B de las versiones en la libreta (semilla 59, 18 + 18): contradicciones 3 → 2, resueltas 13 → 12,
  primera pista 8,0 → 6,8: **sin efecto medible en el bot** (su detective no compara versiones con pistas: límite
  del bot, no prueba en contra). Se mantiene por diseño (Her Story / L.A. Noire), anotado en Decisiones.
  Del A/B salió un **fallo visible**: qwen escribió la etiqueta como "[MESTADO: asustado]", el analizador no la
  reconocía y se veía en el chat; en el historial, el modelo la copiaba (10 respuestas seguidas en 3B). Ahora el
  analizador tolera erratas alrededor de "ESTADO" (bien formada sigue siendo solo la exacta, para las
  calibraciones) y el historial guarda la etiqueta canónica. Otra deriva al chino en 3B (brazo con el código de
  antes del reintento por idioma). EditMode 692/692, PlayMode 46/46.

- **15:36** Pared de alturas de comisaría detrás de la rueda de reconocimiento (cifras 130-190 cm; Theme.lineupWall).
  Tercera revisión independiente (0 críticos, 2 importantes, 6 menores) → CODE-REVIEW.md, todo resuelto: sobre
  todo, la libreta ya no enseña la mentira del culpable antes de que la cuente ni nombres de quien aún no está.
  Ronda 6 (bot 2C, 1C, 3B, con los arreglos de idioma y etiqueta): 0 respuestas incoherentes, 4/6 resueltas,
  primera pista 5,3. EditMode 700/700, PlayMode 46/46.

- **15:45** Ronda 7: capturas a 1920 y 2400 (pared de la rueda, texto muy grande, alto contraste: bien); galería
  regenerada; build de Windows con todo: SMOKE OK. El lector de pantalla habría leído las cifras de la pared
  ("190, 180…"): componente Decorative que el lector salta (test).

- **16:09** **CHECKPOINT 6 h**: "Informe intermedio" en docs/REPORT-DIA3.md y push de seguridad. Ronda 8 (bot 3A,
  3B, 2A): 5/6 resueltas, 0 reintentos por idioma, latencia media 839 ms.

- **16:17** Ronda 9, el menú de quien vuelve (nueva captura menu_continuar): "Continuar · caso, día N" salía como botón
  secundario y "Nueva partida" en dorado → Continuar es el principal y el otro dice "Caso nuevo" (glosario). Tests.

- **16:26** Ronda 9, idea de la web (el jugador conduce la deducción: libretas donde él tacha, Golden Idol): **notas
  del jugador** por sospechoso en la libreta ("añadir nota" → sospechoso → descartado), guardadas con la partida;
  la rueda atenúa a los descartados "(descartado)" sin impedir elegirlos. Instrucciones y RESEARCH.md al día.
  Tests: SuspectNotesTests (4) + test de juego (tocar la nota, ver la rueda). EditMode 705/705, PlayMode 48/48.

- **16:42** Ronda 10. Revisión visual de las notas: "Carmen (descartado)" no concordaba (los datos no guardan
  género) → en la rueda, nombre tachado + "(tu descarte)", que también lee el lector. Recorrido de jugador nuevo
  con lector de pantalla: **los enlaces de la libreta no existían para el lector** (ni ir a interrogar, ni la
  nota, ni enseñar una pista; hueco anterior a hoy) → cada enlace es un botón con contexto ("Interrogar a …",
  "Nota sobre …: añadir nota", "Enseñar como prueba: …"), mismo nodo al cambiar (el foco no salta). Test nuevo en
  ScreenReaderTests (visto fallar antes). EditMode 705/705, PlayMode 49/49.
  Bot ronda 10 (al azar 2A, 3A, 1C; semilla 1010): **6/6 culpables**, primera pista a las 5,7 preguntas, 0
  reintentos por idioma, 0 confesiones/incoherencias; marcadas 2 (un nombre inventado, "Iago" = 1/178, dentro de
  la tasa conocida; una hora que solo repite la franja de la pregunta). Acumulado del día: 35/44.

- **16:52** Ronda 11. Render a 2400: "tu nota: sospechoso" se partía en dos líneas → `<nobr>` (test). Revisión
  independiente del diff desde el checkpoint (CODE-REVIEW.md, cuarta): 1 importante (nota durante una pregunta
  = guardado a medio turno) y 6 menores; arreglados todos menos uno (GC con lector, anotado). Además: el informe
  final recuerda tu nota sobre el culpable. EditMode 707/707, PlayMode 51/51.

- **17:20** Ronda 12. Idea de la web (frustra la pregunta vaga que gasta turno) → medido antes de tocar nada: A/B
  del bot con preguntas cortas de un tema (`-topicQuestions`) = sin efecto (pistas 3,5 → 3,8; culpables 5 → 6 de
  12): el consejo no cambia. Pero **la 2B sale mal 10 de 10** (ronda 8 y las dos ramas): el bot acusa a Marcos
  incluso con la pista que lo descarta, y la ⚡ (el fichaje, que tiene Ruiz) no sale nunca. Causa medible: **el bot
  no leía los partes de la mañana** (el del día 5 dice que los registros de fichaje están en comisaría); un jugador
  sí los lee → todas las métricas del bot tiraban a "difícil". Arreglado en BotPlayer (lee los partes ya dados);
  ronda 13 para medirlo. De paso: el bot a veces ponía el id de una pista como sospechoso (5 turnos perdidos) → va
  a quien la sabe. Build de Windows rehecha: SMOKE OK. Galería: hoja 23 (notas).

- **17:32** Ronda 13 (mismas variantes y semilla que la rama base de la 12, ahora el bot lee los partes): culpables
  5/12 → 6/12, ruido; **2B sigue 0/4**. Diagnóstico de la 2B: la ⚡ (fichaje de Andrés, la tiene Ruiz) sale 10/10
  cuando se pregunta por los horarios del cartero (calibración) y a veces sin preguntar; el bot casi no pregunta
  a Ruiz y se obsesiona con Marcos, hasta citar la pista que lo descarta como prueba contra él. Es el techo de
  razonamiento de qwen 7B como detective, no un caso injusto → sin cambios en la historia. Para el jugador sí:
  **la rueda marca a quien descarta una pista ya encontrada** ("<s>Marcos</s> (pista de descarte)", las palabras
  de la libreta; se puede elegir igual). Tests: InvestigationState.IsClearedByClue (visto en rojo) y uno de juego.
  EditMode 708/708, PlayMode 52/52. Acumulado del bot hoy: 52/80 culpables (35/44 al azar + 17/36 en la 12-13, con la 2B: 0/12).

- **17:38** Ronda 14. Idea de la web ("una partida más": rejugar tiene que traer algo nuevo) → la variante (quién lo
  hizo) salía al azar puro: rejugar una historia repetía la solución 1 de cada 3 veces. Ahora
  `CaseRecords.PickVariant`: primero las no resueltas; con todas jugadas, nunca la última de esa historia (tests
  visto en rojo). Captura de la rueda con "(tu descarte)" y "(pista de descarte)" a la vez: legible en 2400.
  EditMode 711/711, PlayMode 52/52.

- **17:43** El informe final acaba con lo que queda por ver: "Esta historia tiene otros 2 culpables posibles: juega
  otra vez para descubrirlos" (o "Has visto todos…"), justo encima de "Jugar otra vez", que abre la selección de
  caso. En la tarjeta del expediente no cabía sin chocar con títulos largos. EditMode 712/712, PlayMode 52/52.

- **17:53** Ronda 15. Recorrido de jugador nuevo: **los partes de la mañana solo se podían leer una vez** (en la
  tarjeta del día; "los registros están en comisaría" es media pista) → la libreta los guarda al final ("PARTES DE
  LA MAÑANA", día a día) y se refresca al empezar el día (antes solo con la siguiente respuesta). Tests en rojo
  primero (EditMode y de juego) y captura limpia (`LibretaConPartes`). Copia: "otros dos culpables" en letra;
  glosario al día. Bot al azar (1A, 2C, 3B; semilla 1515): 4/6 culpables, 0 respuestas marcadas.
  EditMode 713/713, PlayMode 53/53. Acumulado del bot al azar: 39/50.

- **18:05** Quinta revisión independiente (CODE-REVIEW.md): 1 confirmado (el mapeo del bot no se ejecutaba) y 4
  plausibles; arreglados 3 + tests, 1 aceptado (decisión 15). Error mío: una edición por heredoc metió un salto de
  línea real dentro de una cadena C#; la captura "pasó" leyendo un XML viejo → ahora borro los resultados antes de
  cada captura y miro la hora del PNG. EditMode 713/713, PlayMode 54/54.

- **18:15** Ronda 16. Coste visual medido de nuevo (relieve + post-proceso: +0,02 ms por fotograma). Todas las capturas
  rehechas en 1920 y 2400 (19/19 cada una) y galería regenerada; revisadas acusación con texto grande e
  instrucciones en 2400. Instrucciones: el parte "se queda en la libreta". Final: "decía «sospecha»: ahí
  acertabas" (repetía "sospecha"). Memoria nueva: nada de heredocs para código con 
; borrar XML viejos.

- **18:21** Ronda 17, bot al azar (salieron 3A, 3B, 3C; semilla 1717): **6/6 culpables**, 0 confesiones ni
  incoherencias; 4 horas inventadas marcadas (el reintento arregló 6 de 8; residuo conocido), 1 reintento por
  idioma. Acumulado al azar: 45/56.

- **18:30** Ronda 18. Recorrido de jugador nuevo: nada le decía que puede poner notas → la indicación de la libreta
  señala «añadir nota» (24 palabras; el test pide ≤ 25, lo pillé en rojo con la primera versión). Captura y hoja
  25. La validación de layout no veía las secciones nuevas (el caso peor no tenía notas ni partes) → incluidas;
  77/77 en todos los tamaños de texto, y render del editor revisado a ojo. PlayMode 54/54.

- **18:48** Ronda 19, medición de cierre: bot en **las 9 variantes** (2 partidas cada una, semilla 1919): **15/18
  culpables (83 %)**, primera pista a las 5,7 preguntas, 0 partidas sin pistas, 0 confesiones ni incoherencias;
  fallan 2B (0/2, el techo conocido del bot) y una 3B. Horas inventadas marcadas: 8 de 530 (1,5 %), casi todas
  "no me acuerdo, supongo que hacia las 19:30" ante "¿a qué hora exactamente…?" (residuo conocido). README al día.

- **19:17** Ronda 20: A/B del reintento por horas más frío (temperatura 0,3; 3B, 1C, 3A; 12 + 12 partidas, semilla
  2020): arregla **10 de 11** horas inventadas frente a 5 de 9; quedan 2 marcadas frente a 4; culpables 11 vs 12
  (ruido); latencia igual. La marca "IA" de la rama fría es un falso positivo ("No, no puedo ayudarte con eso",
  dicho por un chaval, en personaje). **Activado por defecto** (decisión 16); el bot conserva `-warmTimeRetry`.
  EditMode 714/714, PlayMode 54/54.

- **19:25** Sexta revisión (CODE-REVIEW.md): nada para el jugador; 4 arreglos de editor/lector/tests y 2 anotados.
  Ronda 21 al azar con el reintento frío (3C, 2C, 1A; semilla 2121): 3/6 culpables, **0 respuestas marcadas**; la
  2C falla porque el bot vuelve a Marcos (como en la 2B; la 2C va 3/6 en tres rondas). Acumulado al azar: 48/62.
  EditMode 715/715, PlayMode 54/54.

- **19:26** Build de Windows rehecha con todo lo de la tarde (Builds/Windows-final): Success y **SMOKE OK** (log
  borrado antes, resultado fresco). Barrido de textos: no queda ningún "descartado/sospechoso" con género en la
  interfaz.

- **19:34** Ronda 22 al azar (2B, 1C, 3C; semilla 2222): 3/6 culpables, **0 respuestas marcadas** por segunda ronda
  seguida (el reintento frío se nota); 2B otra vez 0/2 (0/16 en todo el día). 3C va 15/20 en el día, sin patrón en
  los fallos. Acumulado al azar: 51/68.

- **19:39** Ronda 23, última prueba sobre la 2B: el parte del día 5 dice ahora quién tiene los registros ("los tiene
  Ruiz en comisaría"; validador 22/22). Bot 2B ×4 (semilla 1212): sigue 0/4 y sigue sin preguntar a Ruiz por el
  fichaje → confirmado: es el razonamiento del bot, no la información. El texto se queda (más claro para una
  persona, sin coste). 2B en el día: 0/20.

- **19:47** Ronda 24. El 86-88 % de las preguntas no da nada nuevo y la tarjeta del día solo hablaba cuando no tenías
  ninguna pista → ahora reconoce lo de ayer: "Ayer: dos pistas nuevas y una contradicción." (después del parte; al
  continuar a mitad de día cuenta desde ahí). Tests en rojo primero (texto y de juego) y captura revisada (la
  primera versión ponía la frase bajo "Parte de la mañana:", como si fuera del parte; corregido). Otra vez un sed
  metió un salto real en una cadena: arreglado con Edit. EditMode 716/716, PlayMode 55/55.

- **19:52** Ronda 25. El bot no veía la rueda (solo la libreta en texto): ahora, al acusar, se le dice lo mismo que ve
  el jugador ("aparecen tachados, porque una pista los descarta: …"; `-noLineupMarks` para comparar). 2B ×4
  (semilla 1212): **1/4, la primera 2B resuelta del día** (0/20 antes), acusando a Andrés por la furgoneta y el
  lavado. Muestra pequeña, y en otra partida acusó a Marcos con la marca delante (qwen 7B), pero va en la
  dirección esperada: las marcas de la rueda ayudan a quien las ve. Contraste del motivo en la rueda: 4,74:1 (AA).

- **20:02** Ronda 26 (2B y 2C ×4, semilla 2626, el bot ve las marcas de la rueda): **2B 2/4, 2C 3/4**; 0 respuestas
  marcadas. Con la 25: **2B 3/8 frente a 0/20** sin las marcas. El dato más claro de la tarde: tachar en la rueda a
  quien descarta una pista cambia la acusación de quien la mira. Galería regenerada con las capturas de las 19:58.

- **20:09** Séptima revisión: nada serio; arreglado el único confirmado (continuar a mitad de día perdía el "Ayer:
  …": ahora se guarda) con test visto en rojo. EditMode 716/716, PlayMode 56/56.

- **20:24** Ronda 27, cierre definitivo (las 9 variantes ×2, misma semilla que la 19, con reintento frío y marcas de la
  rueda para el bot): **14/18 culpables** (15/18 en la 19: ruido), 2B 1/2; horas inventadas marcadas **3 de 530**
  (8 en la 19; el reintento arregla 9 de 10). Pero 5 nombres inventados en una 3A: siempre "tía María" (la ficha
  hablaba de "tu tía" en Granada sin nombre y el historial lo fijaba) → la tía se llama Remedios en las fichas
  (validador y prompts 30/30). Ronda 28 (3A ×2, misma semilla): 0 nombres inventados, dice "tía Remedios".

- **20:25** Barrido de parientes sin nombre en las fichas: la tía de Sofía (historia 2) no tiene nombre pero en ~60
  partidas nunca se le ha inventado uno (siempre "la tía de Sofía"); el resto ya tiene nombre o es un personaje.
  Sin cambios.

- **20:32** Build de Windows rehecha con lo último: SMOKE OK. Ronda 29 al azar (1B, 3B, 3A; semilla 2929): **5/6**;
  4 horas inventadas, todas con la misma forma: "no me fijé en la hora, pero creo que sobre las 23:30" (obedece la
  nota y aun así da una hora aproximada) → ronda 30: A/B de una nota más estricta ("sin dar ninguna hora
  aproximada"; apagada por defecto hasta ver los datos). Acumulado al azar: 56/74.

- **20:59** Ronda 30: A/B de la nota estricta ("sin dar ninguna hora aproximada"; 3B, 1B, 3A; 12 + 12, semilla
  3030): horas inventadas marcadas 3 → 1, arreglos 8/9 vs 7/8, culpables 10 vs 11 → **no concluyente: se queda
  apagada** (`StrictTimeNudge`), anotado como dato. En la rama base, "primo Carlos" inventado 3 veces: mismo hueco que
  la tía → los primos de Granada se llaman Nerea y Hugo; el comprobador ya no marca marcas ni juegos (Fortnite,
  Nike…). Ronda 31 (3B ×2, misma semilla): 2/2, sin "Carlos"; queda una marca "Javier" que es el padre (falso
  positivo del comprobador). Validador y prompts 50/50.

- **21:06** Cierre: suites 716/716 y 56/56, build de Windows con SMOKE OK. Sonda de premisas falsas con la build
  final: 4/72 (5 %), igual que la primera sonda con la regla (5 %; la segunda dio 2 %). El informe citaba solo el 2 %
  → corregido a "23 → 2-5 %" en informe, README, GAME-DESIGN y COHERENCE (Logs/premisas/premisas-cierre.md).

- **21:16** Calibración de pistas con la build final (48 pistas): 36/48, media 80 % (83 % esta mañana: dentro del
  ruido de 3 intentos), **pero 3A_granada bajó a 3/6 por culpa mía**: con la tía con nombre, qwen dice "en casa de
  tía Remedios" sin "mi" ni "Granada" y las anclas no casaban → anclas ampliadas (remedios, nerea, hugo, toda la
  jornada) y esas respuestas reales fijadas como muestras (test). Recalibrada: **6/6 (100 %)**; 3B_noche 6/6.
  Lección: cambiar el texto de una ficha exige recalibrar sus pistas, aunque sea un nombre.

- **21:25** El test de guarda (una pista no puede dispararse con la propia ficha) pilló que "nerea"/"hugo" están en el
  secreto de Álex en 3A → fuera de las anclas. EditMode 716/716. 3A_granada recalibrada: 5/6 (83 %, como esta
  mañana). PlayMode 56/56, build de Windows y **SMOKE OK** con el código final.

## Sesión extra (21:30 → 23:30): análisis, sin cambios de código
- **Android (decisión 1)** → `docs/ANDROID-OPTIONS.md`. Medido aquí con las pruebas del juego: qwen2.5 1,5B acepta
  el 72 % de las premisas falsas y suelta bien 15/48 pistas; 3B, 18 % y 22/48 (7B: 2-5 % y 36-39/48) → hoy no
  hay modelo en el móvil viable. Una petición = 1 219 tokens de ficha + historial. Recomendación: Ollama del PC por
  wifi para probar ya; Claude Haiku 4.5 detrás de un proxy para publicar (~0,04-0,07 $ por caso), con recalibración.
- **Rendimiento** → `docs/PERFORMANCE-AUDIT.md` (finecomb no está disponible en la sesión: auditoría a mano, con
  un banco de pruebas temporal no subido). CPU del juego por pregunta ≈ 1,6-2,2 ms frente a 640-900 ms del modelo;
  el más caro, `TimeCheck` (0,5 ms). Cinco mejoras priorizadas; la que se nota es el streaming (decisión tuya).
  Nota: `GC.GetAllocatedBytesForCurrentThread` da 0 en el Mono de Unity (memoria medida por el montón).

## Ahora (día 3)
- Día cerrado: 31 rondas con el bot, 7 revisiones, suites en verde, build final con SMOKE OK, informe cerrado.
- Worktree temporal del bot (C:\Dev\dng-bot) borrado (`git worktree remove --force` + `prune`); todo en origin.

# Sesión A (01-10-2026, desde las 00:10) — feedback de Cristian tras jugar
Rama `feature/sesion-a` (sale de `feature/dia3`); `main` sin tocar. Orden: 1 memoria → 2 lógica de pistas → 3
retratos → 4 gráficos.

## Bloque 1: "los sospechosos recuerdan la partida anterior"
- **00:22** Reproducción primero (tests de juego con un proveedor que guarda todo lo que llega a Ollama): partida
  1 con un marcador, luego **Jugar otra vez, Reiniciar desde Ajustes, Caso nuevo desde el menú y la misma
  historia otra vez** → en los cuatro, historiales vacíos y el prompt de la partida 2 sin nada de la 1; **Continuar**
  conserva solo esa partida. Revisado a mano: `StartCase` vacía historiales y estados; estado, "lo ya contado" y
  pruebas mostradas cuelgan de un `InvestigationState` nuevo; notas y partes se reinician; el proveedor no guarda
  nada ni usa el campo `context` de Ollama; `CaseLibrary` (estático) no se modifica en juego. **No hay fuga en el
  código.**
- Bot: ahora usa el mismo gestor en partidas seguidas (como "Caso nuevo") y comprueba que ninguna respuesta de la
  partida anterior llega al modelo: 1A ×2 + 2B ×2 → **0 fugas** en 3 comprobaciones.
- **Lo que vio Cristian** (hipótesis con datos): qwen dice "como ya le dije", "lo que ya te conté" **sin haber
  hablado antes** en el 0,8 % de las primeras respuestas (11 de 1 342 en las rondas del día 3); con una variante
  repetida (mismos hechos) parece que recuerda la partida anterior. Arreglo (tests en rojo primero): si es la
  primera conversación con ese sospechoso y la respuesta finge memoria, se pide otra vez con una nota ("es la
  primera vez que hablas con este inspector"); el bot marca `FalseMemory` si aun así pasa.
  EditMode 723/723, PlayMode 61/61.

## Bloque 2: lógica de las pistas
- **01:00** Análisis de las 48 pistas por historia (tres agentes en paralelo sobre datos, partes, fichas y 250+
  partidas del bot; quién lo sabe y por qué, qué pregunta lo saca, cuándo es lógico, si la libreta lo explica).
  Aplicado a mano con los tests de guarda (que pillaron tres propuestas que copiaban la pista en la ficha o hacían que
  un parte la regalase). Lo más grave: la ⚡ de 3C se perdía por el ancla "despedir" (test en rojo con los datos
  viejos); Ruiz no sabía que tenía los registros de fichaje (2B); Andrés no veía raro el bar a oscuras (2A) ni la
  comisaría cerrada (2C); las fichas de Álex contradecían sus pistas; 1C_pantalla no se sostenía. **Rosario →
  Amparo** (decisión 4; la clave interna del arte `rosario` se queda: moverla sería tocar el arte original).
- **01:27** Medido: pistas 41/48 (83 %), premisas 1 %, estados 95 %, bot 14/18 → **HECHO CUANDO cumplido**. Revisadas
  las pistas que bajaron: tres eran el detector (anclas ampliadas + muestras reales), una era su pregunta de
  calibración; quedan 1B_cena y 1C_llamada (comportamiento del modelo). Un aviso de "memoria" del bot era un falso
  positivo (respuesta idéntica por ser la ficha): chequeo corregido con test. Tabla completa en `docs/CLUE-LOGIC.md`.
  EditMode 730/730, PlayMode 61/61.

## Bloque 3: personajes sin retrato propio
- **01:42** Inventario: **7 retratos para 12 personajes**. Compartían cara Daniel y Javier, Carmen y Lucía, Lucas y
  Álex, y las tres vecinas (Amparo, Maruxa, Encarna); la carpeta de retratos por personaje no existía. Seis derivados
  por código en archivos nuevos (`Tools/make_derived_portraits.py` → `Assets/Art/Derived/`, el original intacto):
  recolor por zonas y tonos (la piel y las rayas del polo comparten color: zonas además de tono), canas, gafas para
  Lucía, luto para Encarna, espejo para los de la historia 3. Se usan como los originales: mismo encuadre (el del
  original, reflejado si va en espejo), mismo tratamiento de pixel art y relieve 2.5D. Test: ningún personaje comparte
  retrato, cada derivado tiene archivo y encuadre. Galería a tamaño real (`retratos_12.jpg`); la primera captura
  mezclaba historias (los ids se repiten entre ellas) y la rehice historia a historia. Honesto: Javier y las tres
  vecinas comparten cara y pose con su original (se distinguen por ropa, pelo y orientación) → prompts en
  ART-NEEDED.md. EditMode 733/733, PlayMode 61/61.

## Bloque 4: gráficos
- **01:55** El hueco central del interrogatorio pasa a ser la **escena** (`UI/Fx/InterrogationScene.cs`): la figura
  del sospechoso grande, de la cabeza a los muslos, detrás del chat (no se desplaza con él, alfa 0,24 para no restar
  lectura); **viñeta de tensión** según el estado (nervioso, asustado, enfadado en rojo oscuro, solo bordes); **entrada**
  al cambiar de sospechoso (fundido y deslizamiento de figura y busto, medido en capturas: llega a los 0,6 s);
  **destello ámbar** al encontrar pista; **sacudida** del busto en contradicción (junto a la del sello). Con "Reducir
  animaciones": sin deslizamiento, sin temblor ni latido, fundidos cortos (tests). Primeras capturas: la viñeta roja
  teñía toda la pantalla y el destello salía como una caja (usaba el sprite de viñeta en vez del radial) → corregido
  y vuelto a capturar.
- **02:08** **Rueda en una sola fila con alturas reales** (`UI/HeightLineup.cs`): cada personaje con su altura
  (`heightCm`, 150-183 cm), de cuerpo entero (encuadre medido sobre la parte opaca de cada imagen, sin el humo del
  cigarro de la vecina), todos sobre el mismo suelo y a la misma escala que las rayas de la pared (que se recolocan).
  Huecos a la medida de cada figura (los estrechos no gastan sitio). Es un LayoutGroup: se recoloca solo al cambiar de
  tamaño (dos intentos con eventos fallaron en el test de maquetación a 19,5:9; leído cómo cambia el tamaño la vista
  previa antes del tercero). Un error mío en la captura (SetActive dentro del cálculo de layout) → escala 0.
- **02:13** **Galería antes/después** (`galeria/graficos_antes_despues.jpg`): MISMA pantalla, MISMA historia (Caso 1),
  mismos instantes; "antes" = los tres ajustes nuevos del tema apagados (`interrogationStage`, `tensionVignette`,
  `lineupSingleRow`), lo que comprueba además que todo se puede desactivar. Diferencia con el día 3 de verdad: la pared
  marca 110-200 en vez de 130-190. Una hoja de revisión me engañó (miniatura vieja con el mismo nombre): comprobado a
  tamaño real y midiendo. EditMode 740/740, PlayMode 64/64 (+24 capturas/mediciones ignoradas como siempre).
- **02:25** Informe final `docs/REPORT-SESION-A.md` (los cuatro bloques, HECHO CUANDO de cada uno, galería antes/después
  y lo pendiente). Sesión A cerrada; todo en `feature/sesion-a`, `main` sin tocar.

# Sesión de skills (01-10-2026) — rama `feature/skills` (desde `main`, sin tocar el código del juego)

## Bloque 1: por qué no cargaban los plugins
- **09:40** `finecomb`, `unity-perf`, `design-skills` y `unity` daban `Unknown skill`. `claude plugin list` los mostraba
  `✔ loaded`, y un `claude -p` nuevo los cargaba los cuatro. Causa: esta conversación era el mismo proceso desde el
  28-09 a las 20:12, y esos cuatro plugins se sincronizaron el 30-09 a las 10:05, después. Los plugins se registran al
  arrancar. No era la instalación, ni el alcance, ni la versión (2.1.285). Al crear `.claude/skills/`, la sesión volvió a
  leer la lista de skills y ya los vio. `docs/SKILLS-SETUP.md` dice cómo comprobarlo al empezar y qué hacer si no cargan.

## Bloque 2: 14 skills del proyecto (`.claude/skills/`)
- **10:20** Escritas con `writing-skills` a partir de NIGHT-LOG, los REPORT, COHERENCE, CLUE-LOGIC y CODE-REVIEW (un
  agente extrajo 4-8 casos reales por skill, con su archivo:línea). Prueba: 14 tareas simuladas en procesos nuevos,
  primero sin skills y luego con ellas, sin `CLAUDE.md` ni mapa para que cada una se activara solo por su descripción.
  Se activaron **14/14**. Comprobaciones: 53/70 → 63/70 (≈67 revisadas a mano). Herramientas: 140 → 93. Tiempo:
  13,3 → 9,9 min. Coste: 6,19 → 4,24 $. Cinco tareas ya salían 5/5 sin skill (el agente lee los docs).
  La prueba sacó dos cosas: `.gitignore` solo ignoraba `anim/` del 30-09 (las 200 capturas del 01-10 aparecían como
  nuevas), y faltaba `.claude/scheduled_tasks.lock`. Las dos están en `.gitignore`, y `git-hygiene` se volvió a probar.
  Banco de pruebas: `Tools/skill-tests/` (resultados en `Logs/skill-tests/`).
- Error mío: creé `CLAUDE.md` con las líneas base aún en marcha. Lo aparté a los ~20 s, antes de que arrancara ningún
  proceso nuevo (las 4 primeras habían empezado antes), así que la línea base no se contaminó.

## Bloque 3: mapa
- **10:35** `docs/SKILLS-MAP.md`: qué skill del proyecto y cuál instalada usar en cada tipo de tarea. Solo nombres
  comprobados en disco. `CLAUDE.md` nuevo con la línea "lee SKILLS-MAP.md al empezar". Probado en un proceso nuevo:
  leyó el mapa solo y siguió la fila de Animaciones.

# Arte de Javier, revisión de Cristian (01-10-2026)
- **Medido sobre los 7 originales:** Daniel tiene un píxel de 9 px (118 de alto) y cabeza 0,28 (≈1/3,6). Marcos y
  Álex tienen 5 px y cabeza 0,19 (≈1/5,3); Lucía queda entre medias (6 px, 0,24). La cabeza se leyó en recortes con
  regla, porque dos métodos automáticos fallaron en 5 de 7. 02-04 llevan sombra suave a la derecha de los pies (≈18 % de
  opacidad, 46-50/255); Daniel no. Referencias 02 (principal), 04 (secundaria) y 03 (solo paleta); la 01 fuera.
  Daniel, anotado en ART-NEEDED para rehacerlo.
- **`Tools/remove_white_bg.py`:** relleno desde el borde, hueco blanco encerrado, sombra conservada, avisos de halo y
  de cuadros, 768×1024 por vecino más cercano. 10 tests. Con las 7 originales sobre blanco salió un hueco opaco (el
  cable de Álex) y una falsa alarma de halo; ambos se arreglaron con test en rojo antes. El umbral de halo se calibró
  con los originales (0,31-0,62 px por px de contorno; se avisa por encima de 1,0).
- **Rama `feature/encuadre-arte-nuevo`** (desde `main`, sin fusionar): `PortraitCrops.For` y la entrada de `javier`.
  Tests primero (`NewArtCropsTests`). Mientras no haya arte nuevo, el juego sigue igual: capturas de `main` y de la
  rama dentro del ruido del grano. Una primera comparación salió distinta porque el caso era al azar (historia 2
  frente a 1), no por el cambio. Filtro de importación: A/B a 1080×1920 reales; gana bilineal con mipmaps, porque
  Point rompe las líneas finas en la rueda. EditMode 746/746, PlayMode 64 sin fallos.
- **Skill `art-brief`:** actualizada y vuelta a probar en un proceso nuevo; se activa sola y aplica las reglas nuevas.
  La prueba rápida de SKILLS-SETUP en PowerShell da LOADED.
- Errores míos: un heredoc para un script de edición (inofensivo, con asserts; luego volví a Write) y la primera
  captura del filtro, que salió a 540×1920 deformada y con `CopyTexture` fallando en anchos que no son múltiplo de 4.

# Sesión B (01-10-2026, 16:18 → 00:18, autónoma) — rama `feature/retratos-javier`

## Comprobación inicial
- **16:18** `main` = origin (`67fb27c`), nada fusionado. `ollama ps` vacío. Plugins: cargan en esta sesión (probado con
  una skill `unity:`). GPU ocupada solo por la tanda de Javier lanzada antes de empezar. No existe aún la skill
  `local-portrait-gen` (se crea en el bloque 2 si Javier aprueba).

## Bloque 1: Javier con el nuevo enfoque
- (Antes de la sesión, con tests) suelo entre los pies en `remove_white_bg.py`; `expressions.py` con la misma memoria
  que `generate.py`; `from_pipe` forzado a fp16 (diffusers 0.40 lo pasa a float32: "Half and Float").
- Imagen a imagen desde Marcos. Barrido sin retocar: a 0,55-0,75 sigue siendo Marcos (delantal, jarra en alto); a
  0,85 es otro personaje pero pierde el estilo. **Paint-over por código** (`paintover.py`): sin jarra, humo, bigote ni
  delantal; brazo colgando con botella; ropa oliva y marrón; canas. Barrido sobre él: **0,65** es el punto (0,55 deja
  el boceto, 0,75 vuelve a perder el estilo). Postura nueva `defensive` (peso en una pierna, mano en la cadera).
- **16:25** 20 candidatos a 0,65 (15 s/imagen). Contorno negro grueso en todos. Dos métricas mías fallaban y se
  arreglaron con test en rojo: la fuga daba falsos positivos con fondo en degradado (0,07-0,16 → 0,0); la "piel" de la
  corrección de color incluía la camisa ocre (71 350 px → solo la cabeza).
- **~16:28** Mejor candidato 6106: píxel 5, cabeza 0,21 (a mano), contorno 0,022, paleta 23, fuga 0,001. Prueba ciega
  en marcha: 3 subagentes (orden aleatorio) y 1 de parecido con Marcos.
- **~16:30 Prueba ciega 1** (6106): 3 de 3 señalaron al candidato (confianza 4-5): paleta turbia y estrecha, sombreado
  ruidoso, cara borrosa sin ojos claros, contorno uniforme. Parecido con Marcos: "personas distintas" (4).
- Medido: los candidatos tenían **35-38 colores** frente a 2600-3200 de los originales (mi `pixelate` cuantizaba a 40)
  y contraste L* 19-21 frente a 22,5-27. `finish.py` (posproceso sin GPU, compartido con generate.py): 160 colores,
  contraste a ~24, aplanado de motas por celdas (test) y saturación solo de la ropa. La "piel" de la corrección de color
  se medía sobre la camisa ocre (71 350 px) → solo la cabeza (test).
- **~16:34 Prueba ciega 2** (6101, prompt de cel shading + IP-Adapter 0,4 + posproceso nuevo): 3 de 3 otra vez.
  Parecido: distintos, pero "2 parece más atlético" (Javier debe ser corpulento).
- Retoque de la cara (inpainting con estilo de Lucía y Álex): a fuerza 0,7 gana estilo pero **deja de ser Javier**
  (veinteañero, sin barba, mechones rojizos de Lucía); a 0,5 sigue siendo Javier y apenas cambia.
- **~16:39 Prueba ciega 3** (7002): 3 de 3. **Total 9 de 9.** Razones constantes: textura ruidosa del generador, cara
  realista de ojos pequeños, paleta apagada, contorno que no encaja en la rejilla.
- **Corrección de horas (16:45):** las horas de arriba eran estimaciones mías y estaban mal (escribí 16:45-17:38); el
  reloj dice que todo eso pasó entre 16:25 y 16:40. Creí que quedaban 70 min del bloque y quedaban ~2 h.
- **(Decisión revocada a las 16:45)** ~~Bloque 1 NO aprueba. Paro antes de las 2,5 h~~ porque las tres rondas señalan lo mismo: la diferencia está en cómo
  dibuja el generador (SDXL + LoRA de pixel art), no en lo que el posproceso puede corregir. No integro a Javier, me
  salto el bloque 2 y paso al 3. Hoja: `docs/art/javier/evolucion_sesion_b.jpg` (40 txt2img → img2img → final).
  Propuestas en el informe: entrenar un LoRA de estilo con los 7 originales, saber con qué herramienta se hicieron los
  originales, o encargo a un artista con el BRIEF.
- **16:45 Reabro el bloque 1** con la palanca que falta: un LoRA de estilo entrenado con los originales del juego
  (Marcos, Lucía, Álex, cartero, vecina, detective; sin Daniel, que es el otro estilo). Las anteriores corregían el
  resultado o empujaban desde fuera; un LoRA enseña al generador a dibujar como ellos. Límite: 18:48.
- **16:46** LoRA de estilo: script oficial de diffusers 0.40 (`train_text_to_image_lora_sdxl.py`, Apache-2.0) en
  `C:\AI\lora`, con dos parches locales para trabajar sin conexión (tokenizador CLIP directo; el VAE fp16-fix sin
  variante). 6 originales rellenados a 1024² (`Tools/portrait_gen/lora_dataset.py`), rango 16, 800 pasos, 9,7 GB de
  VRAM, ~2 s por paso (~27 min). Mientras entrena, bloque 3 en `feature/mejoras-seguras` (sin GPU):
- **16:44** Mejoras 2 y 3 (TimeCheck con caché; anclas normalizadas una vez; Parse recordado) → commit `560e71e`.
- **16:54** Mejora 4 (guardado compacto, fuera del hilo principal, cola única) → `2e6b103`. Suites: EditMode 753/753,
  PlayMode 64 pasan y 0 fallan.
- **16:58** Segundas vías → `b92d6c2`. **Decisión:** 1B ya tenía 6 pistas y el diseño aprobado es 5-6, así que la
  segunda vía de 1B_cena no es una pista nueva (lo intenté primero; el test de 4-6 lo paró) sino un testigo: Amparo
  vio a Daniel volver con una mujer y Daniel confiesa si se lo echan en cara. 1C_llamada: el parte del día 4 dice que
  Daniel recibió una llamada corta antes de las diez. Falta medirlas con Ollama (GPU ocupada).
- **17:01** Banco de pruebas antes/después en la misma ejecución → `Logs/perf-mejoras.md` (TimeCheck 403 → 15 µs;
  Parse ×4 126 → 0,4 µs; Evaluate ×6 200 → 17 µs; guardado 41,8 → 24,0 KB y 2,9 → 0,7 ms en el hilo principal).
- **17:16** LoRA entrenado (800 pasos, `C:\AI\lora\jvstyle`). **Error mío:** Ollama tenía qwen cargado (5,6 GB) desde la
  suite PlayMode de las 16:50, mientras entrenaba: incumplí la regla de la GPU sin verlo. Descargado con `ollama stop`
  antes de generar; desde ahora miro `ollama ps` antes de cada tarea pesada, no solo la VRAM.
- **17:20-17:37** Pruebas: (1) 8 imágenes: las figuras en bruto, mucho más cerca del juego (contorno, cara con ojos
  claros); 4 se rompían en el posproceso por un fondo gris oscuro → se pide el gris claro del entrenamiento. (2) texto
  a imagen sigue con fondo oscuro (7 de 8 rotas); de imagen a imagen 8 de 8 limpias. (3) barrido del LoRA pixel-art-xl
  a 0,8 / 0,4 / 0: **sin él (solo el LoRA propio) la ropa sale más plana y la cara más limpia**. (4) 12 a 0: limpias
  8400, 8402, 8405; defecto que queda: a veces pinta la jarra de Marcos en el hombro. Candidato: **8405**.
- **17:40** Prueba ciega 4 (8405, hojas 101/103/108 con el candidato en B, D y A) + parecido: en marcha.
- **17:41 Prueba ciega 4** (8405): 3 de 3, confianza media. Parecido: **"la misma persona" que Marcos** (falla): la
  pose y la pulsera venían de partir del paint-over de Marcos y de un LoRA entrenado con él.
- **17:46** Las siluetas negras **no eran el fondo** (mi diagnóstico de las 17:20 estaba mal): `colour.correct` con la
  máscara de piel vacía daba NaN → negro. Arreglado con test primero (`0f03a3e`). Nuevo prompt JAVIER_JV2 y lienzo gris
  liso en vez de Marcos.
- **17:54 Prueba ciega 5** (8603): 3 de 3; parecido **pasa** ("personas distintas", alta). Primer motivo en las tres:
  piel amarillo limón → la máscara de piel no aceptaba matiz 56°; arreglado con test (`e7dfeb1`).
- **17:58 Prueba ciega 6** (8611, piel corregida): 3 de 3, confianza media, dos dudan con otra figura; parecido pasa.
- **17:59 Prueba ciega 7** (8611 con aplanado fuerte, solo en el scratchpad): 3 de 3; la cara empeora ("manchas").
- **18:00 Bloque 1 cerrado, NO aprueba.** (Horas leídas del reloj: antes había escrito 18:03 y 18:06, estimadas otra vez.) 7 rondas, 21 de 21. El LoRA acercó mucho (parecido resuelto, confianza baja
  de alta a media) pero la cara delata: el LoRA vio 6 figuras enteras con la cara a ~60 px y no aprende los ojos de los
  originales. Siguiente paso propuesto (no hecho): LoRA con recortes de cara ampliados, o encargo con el BRIEF.
  Hoja: `docs/art/javier/evolucion_sesion_b.jpg`. Nada se integra en el juego.
- **18:02-19:26** Bloque 3, medición completa en dos worktrees (`../dng-main` = main, `../dng-mejoras` = rama), en
  serie (una GPU), mismo modelo, umbral y semilla del bot (1919, 2 partidas × 9 variantes):
  pistas 43/48 y 86 % (main) frente a 41/48 y 82 % (rama); 1B_cena 33 → 56 %, 1C_llamada 33 → 56 %; premisas 2/72 →
  3/72 (≤5 %); estados bien formados 97 % y 97 %; bot culpable 17/18 → 15/18 (≥14; ±2 es ruido). Cambian de lado
  1A_papeles, 1C_pantalla, 3C_bar (fallan en la rama) y 2C_grabacion (falla en main): con 6-9 respuestas por pista es
  lo esperable, pero no lo doy por ruido sin medirlo.
- **19:37** Confirmación con `-tries 10` de esas 6 pistas en los dos worktrees: en marcha.
- **19:40** Confirmación con 10 intentos (main / rama): 1B_cena 35 / 20 %, 1C_llamada 53 / 37 %, 1A_papeles 80 / 60 %,
  1C_pantalla 50 / 40 %, 2C_grabacion 50 / 45 %, 3C_bar 85 / 90 %. **La subida a 56 % con 3 intentos era ruido.** La
  pregunta con la palanca de Amparo acierta 1 de 10: Daniel niega la aventura igual. **Segundas vías revertidas**
  (`848a690`, git revert). `NegationEquivalenceTests` (`a86f6e1`): el detector decide igual que antes en todas las
  anclas y textos, así que 1A_papeles es variación del modelo. CLUE-LOGIC y PERFORMANCE-AUDIT al día (`69470d7`).
- **19:47** Suites de la rama en su estado final: EditMode 754/755 (el que falta es el banco, Explicit), 0 fallos;
  PlayMode 64 pasan, 0 fallos. **La suite PlayMode carga qwen en Ollama** (confirmado: estaba cargado al acabar):
  esa fue la causa de mi error de las 16:50. Descargado con `ollama stop`.
- **19:50-20:34 Trabajo extra tras el bloque 3 (Javier; no reabre el bloque 1, que sigue cerrado sin aprobar).**
  LoRA v2 (`C:\AI\lora\jvstyle2`): las 6 figuras + recortes de cabeza ampliados de los originales, 1 200 pasos
  (`lora_dataset.py --caras`, test). `facefix.py` (pegado con test): repinta solo la cabeza a 1024 con el LoRA v2.
  `--head-ratio` en generate.py. Pruebas ciegas:
  · 8: 8611 + cara → 3/3, confianzas media/baja/baja-media; nuevo motivo común: "cabeza pequeña, ~1/7".
  · 9: 8702 (v2, cabeza 0,24, cara) → **2/3** (la 604 elige a Álex). Parecido: "personas distintas", alta.
  · 10: 8700 (v2, cara, ropa aplanada fuera de la cabeza) → **2/3** (la 701 elige a Álex y agrupa a Javier con
    Marcos y Lucía). Motivo común nuevo: "contorno negro grueso y uniforme" frente al contorno coloreado.
  · 11: el mismo sin el contorno reforzado → 3/3 ("borde blando, borroso"): el contorno ayuda; lo que falta es
    colorearlo como los originales (selout).
- **20:34 Paro el trabajo extra.** Total: 11 rondas, 31 de 33 señalan al generado. Mejor: rondas 9 y 10 (2 de 3).
  No aprueba; nada se integra. Hoja actualizada: `docs/art/javier/evolucion_sesion_b.jpg`.
- **20:35** Informe `docs/REPORT-SESION-B.md` escrito y subido.
- **20:36** Ronda 12: contorno coloreado (selout, opción nueva de `reinforce_outline` con test) → 3/3: "contorno
  blando y roto". El contorno casi negro de la ronda 10 era mejor; lo que delata es la cara y el grano.
- **20:37 Paro definitivamente el trabajo con Javier.** 12 rondas, 34 de 36. Mejor: rondas 9 y 10 (2 de 3).
  Recomendación sin cambios: encargo con el BRIEF.
- **20:38 Cierre.** Todo con commit y push; ninguna rama fusionada; `main` sin tocar (67fb27c). Worktrees temporales
  (`../dng-main`, `../dng-mejoras`) borrados. Sin procesos de generación ni modelo cargado en Ollama; GPU a 714 MiB
  (los dos python.exe que quedan son del 29-09, no de esta sesión). Cierro antes de las 23:48: los tres bloques
  están cerrados y más rondas de Javier no cambiaban la recomendación. Informe: `docs/REPORT-SESION-B.md`.
- **20:39 Reabro para revisión** (mi memoria de sesiones largas: usar todo el tiempo; cerré a las 20:38 por error).
  Causa exacta de que PlayMode cargue Ollama: `OllamaProvider.preloadOnStart` + `WarmUpAsync()` al cargar la escena
  (capa de proveedores, no la toco; al informe como decisión).
- **20:47** Revisión independiente de `feature/mejoras-seguras`: 0 críticos, 1 importante (pérdida del último guardado
  si Android mata la app en segundo plano) → arreglado con test primero en `2aa7841`, más 4 menores. EditMode 756/757,
  0 fallos; PlayMode 64, 0 fallos. Ollama descargado; worktree `../dng-fix` borrado.
- **20:50** Comprobación independiente del informe: 5 discrepancias (11 → 12 rondas, commits de la ronda 12, 16
  archivos con GameManager, decisión 3 sugería el contorno ya probado, origen del 0,20) → corregidas.
- **20:52** Revisión independiente de las herramientas: 0 importantes; 2 menores arreglados con test primero
  (mínimo de piel en `colour.correct`; `facefix` rechaza un recuadro sin cara). Mi primer umbral de "es una cabeza"
  rechazaba cabezas reales del LoRA v2 (la máscara de piel no ve su piel, demasiado saturada): medido y cambiado a
  tono cálido antes de subirlo. Ese mismo hecho explica muy probablemente la "piel naranja" de las rondas 9-12.
  Tools/tests 33/33.
