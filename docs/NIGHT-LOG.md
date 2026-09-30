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

## Ahora (día 3)
- Ronda final 2: bot para comprobar los desbloqueos tras el n.º 7; capturas tras los cambios; recorrido mental.
  Informe intermedio a las ~16:10.
