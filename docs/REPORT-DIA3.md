# Informe del día 3 (30 sep 2026, 10:09 → ~22:00)

Rama: **`feature/dia3`** (sale de `feature/noche2`), subida a origin después de cada bloque. **`main` no se ha
tocado.** Diario minuto a minuto: `docs/NIGHT-LOG.md`. *Borrador vivo: se completa al final del día.*

---

## Informe intermedio (6 h: 10:09 → 16:09)

**Estado:** bloques 0, A, B, C, D y E terminados (su "HECHO CUANDO" cumplido o documentado: el APK de Android
necesita el módulo, que este PC no tiene). Desde ~12:45, rondas finales. Todo en `origin/feature/dia3`; `main`
intacta. Build de Windows con todo lo de hoy: SMOKE OK.

**Rondas finales hasta ahora (sobre lo de los bloques):**
- Tres revisiones de código independientes (13 + 11 + 8 hallazgos, ninguno crítico): todos resueltos, cada uno con
  su test en rojo antes del arreglo.
- Lógica: preguntas capciosas 22 % → 2 % (medido, sin perder pistas ni estados); respuestas en chino y etiquetas
  de estado con erratas que salían en el chat (encontradas por el bot) arregladas; desbloqueos que ya no dispara
  la pregunta de ejemplo (medido).
- Jugabilidad: la libreta apunta lo que ha dicho cada uno (sin enseñar la mentira del culpable antes de tiempo),
  con indicación del tutorial; "Pensar" y la prueba clave explicados donde hacen falta; rango honesto.
- Aspecto y sonido: ficha policial del culpable, pared de alturas en la rueda, retrato más grande en 20:9,
  desplegables que se abren bien, motivo musical propio en cada historia.
- Accesibilidad: auditoría WCAG 2.2 AA, lector de pantalla (TalkBack/VoiceOver), foco visible, flechas legibles.
- Bot con variantes al azar: 29/38 partidas resueltas en las seis rondas con bot (26/32 en las cinco últimas), sin rupturas de
  personaje ni confesiones.

**Tests:** EditMode 700/700, PlayMode 47/47 (inicio del día: 549 / 20).

**Errores míos, corregidos:** un `git add docs` subió ~300 MB de capturas en bruto (commit `ff30092`; retiradas en el
siguiente, historia remota sin reescribir: ver Decisiones, punto 12); un script dejó finales de línea dobles en
BotPlayer.cs (normalizado).

**Lo siguiente:** más rondas finales (capturas y crítica, revisión del diff, bot en variantes al azar, recorrido de
jugador nuevo) hasta ~22:00; informe final; limpiar el worktree temporal.

---

## 1. Resumen ejecutivo
1. **Lógica:** validador narrativo (9 reglas) en las 9 variantes, 17 incoherencias corregidas, línea temporal de
   cada variante, desbloqueos naturales por tema y máquina de estados con tests de toques reales.
2. **Jugabilidad:** "Pensar" (ayuda por niveles), dificultad (Historia/Detective/Veterano), prueba clave, rango del
   detective y "lo que se te escapó". Medido con el bot: primera pista 10,3 → 6,9 preguntas y resueltas 44 → 72 %.
3. **Aspecto:** retratos 2.5D (relieve + lámpara, elegidos frente a vóxel con capturas), post-proceso noir de URP
   con el contraste medido *después* del efecto, arte propio por historia, ficha policial del culpable en los
   finales, retrato más grande en pantallas 20:9, escala de radios, estados de botón y textos revisados. Todo
   reversible desde el tema o los Ajustes.
4. **Accesibilidad:** auditoría WCAG 2.2 AA y **lector de pantalla** (TalkBack/VoiceOver con el módulo de Unity 6),
   flechas de desplegable legibles, pista de deslizadores visible, tamaño de letra del sistema respetado.
5. **Calidad:** dos revisiones independientes (13 + 11 hallazgos, 0 críticos, todos arreglados con test en rojo
   primero), qwen ya no acepta premisas falsas (22 % → 2 %) ni arrastra respuestas en chino, 0 B de basura
   por fotograma, build de Windows con prueba de humo OK. Android configurado, pero sin APK (falta el módulo) y con
   un bloqueante real: **en el móvil ningún proveedor LLM funciona tal cual** (decisión para Cristian).

## 2. Galería antes / después
`docs/screenshots/2026-09-30/galeria/` (22 hojas; se regeneran con `python Tools/make_gallery_dia3.py`).
"Antes" = capturas del comienzo del bloque C (visualmente, el final de la noche 2).

| Hoja | Qué cambia |
|---|---|
| 01_menu | Título desde `GameTexts.GameName`, banda bajo el subtítulo, post-proceso |
| 02_casos | Expedientes con la víctima, radios de la escala |
| 03_expediente / 19_intros_por_historia | Fondo propio por historia (lluvia, cala, olivar) en vez de la sala genérica; reglas con las cifras de la dificultad |
| 04_interrogatorio | Chat anclado abajo, Acusar en aviso (no dorado), retrato 2.5D |
| 05_libreta / 22_libreta_versiones | Botón "Pensar"; lo que dice cada sospechoso, para compararlo con las pistas |
| 06_acusacion | Retratos 2.5D en la rueda sobre una pared de alturas de comisaría, prueba clave explicada (solo cuando hay pistas), flechas legibles |
| 07-09 veredicto y finales | Rango del detective, prueba clave, "lo que se te escapó", "solidez de las pruebas N de M" |
| 21_ficha_policial | Nuevo: el informe se cierra con la ficha del culpable (retrato 2.5D, nombre a máquina) |
| 10_ajustes | Dificultad, pista de los deslizadores visible (WCAG 1.4.11) |
| 12_dialogo | Un solo aviso de reinicio con botones de acción |
| 13-14 texto grande / alto contraste | Siguen en AA (test) |
| 20_personajes | Los 7 retratos: plano 2D frente a relieve 2.5D |

## 3. Métricas antes / después

| Métrica | Antes (inicio del día) | Ahora |
|---|---|---|
| Tests EditMode | 549 | **686** |
| Tests PlayMode (sin capturas) | 20 | **46** |
| Horas inventadas por qwen (A/B, 18+18 partidas) | 27 / 619 respuestas | **7 / 630** (−74 %), latencia igual |
| Primera pista (bot, preguntas) | 10,3 (8,6 antes de B2) | **6,9** con Pensar |
| Partidas sin ninguna pista (bot) | 2 de 18 | **0** |
| Partidas resueltas (bot) | 44-56 % | **72 %** con Pensar |
| Detección de pistas (calibración, 48 pistas) | 80-85 % | 83 % con fichas más ricas (540 palabras) |
| 2C_gps (la pista que falló en 0c) | 60 % | **85 %** (10 intentos) |
| Premisas falsas aceptadas (sonda de 72) | 23 % | **2 %** (sin perder pistas ni estados) |
| Incoherencias narrativas conocidas | 17 (auditoría) | 0 abiertas; validador en verde en las 9 variantes |
| Basura por fotograma del juego (reposo) | ≤ 0 B | ≤ 0 B (tras C3/C4/D1) |
| Contraste mínimo de texto tras el post-proceso | — | 6,08:1 (todos los pares suben) |
| Hallazgos de revisión abiertos | — | 0 de 24 (dos revisiones independientes) |
| Lector de pantalla | no | **sí** (jerarquía, acciones y anuncios; probado en el editor, falta un móvil real) |

## 4. Skills y herramientas usadas
- **superpowers:test-driven-development** en todo el código (cada arreglo de la revisión: test en rojo → verde).
- **design:design-critique** (C1, docs/DESIGN-CRITIQUE.md), **design:ux-copy** (C6, glosario y tabla de textos) y
  **design:accessibility-review** (auditoría WCAG 2.2 AA en las rondas finales).
- **Subagentes**: auditoría editorial (A2), investigación con búsqueda web (B1), revisión de código independiente
  con el modelo más capaz (D3).
- **Búsqueda web**: juegos de detectives (RESEARCH.md), nivel de API de Google Play (E).
- Medición propia: bot jugador (BotPlayer), calibradores de pistas/estados/premisas, capturas en batchmode,
  `Tools/check_audio.py`, `AnimationCapture.CosteVisual`.
- No disponibles en esta sesión (sustituidos por lo anterior): design-skills, unity-perf, finecomb, plugin de Unity.

## 5. Investigación y decisiones
- **RESEARCH.md**: Obra Dinn (confirmar en bloque), Golden Idol (ayudas escalonadas), Duck Detective (dificultad
  amable), Shadows of Doubt / L.A. Noire (la prueba que sostiene la acusación), juegos de interrogatorio con LLM.
  → "Pensar", dificultad, prueba clave, rango.
- **REAL-CASES.md**: patrones de casos reales (solo patrones; sin nombres ni datos de víctimas) → carácter, herida,
  cómo se nota la mentira y progresión bajo presión de cada personaje (StoriesDatabase.json).
- **Datos que decidieron**: reintento por horas (A/B), límite de 540 y luego 565 palabras (calibración), regla
  contra premisas falsas (sonda + calibración completa), 2.5D frente a vóxel (capturas y vértices), raíces de
  desbloqueo (bot: el hermano salía el día 1 en 8/12 partidas), tonos de los drones de la música (espectro).
- Documentos: GAME-DESIGN.md, DESIGN-CRITIQUE.md, CODE-REVIEW.md, RENDIMIENTO.md, ANDROID-BUILD.md, COHERENCE.md,
  STORY-AUDIT.md.

## 6. Decisiones para Cristian
Todas tomadas de forma conservadora y reversibles; aquí para que las confirmes o cambies.
1. **Proveedor LLM en el móvil (bloqueante para Android).** Ollama apunta a `localhost` y la clave de Anthropic se
   lee de un archivo que no existe en Android (y nunca debe ir dentro del APK). Opciones en ANDROID-BUILD.md:
   Ollama del PC por la wifi (pruebas), servidor propio con HTTPS (publicable) o modelo en el móvil. Recomiendo
   la primera para probar ya y la segunda para publicar. No he tocado la capa de proveedores.
2. **Nombre del juego.** El encargo traía "[NOMBRE]" sin rellenar: se ve "Detectives" (`GameTexts.GameName`);
   `productName` sigue siendo "Casos" porque cambiarlo mueve los guardados y ajustes de los jugadores.
3. **Paquete de Android provisional**: `com.cristiannoguera.casos` (no se puede cambiar una vez publicado). **API
   mínima 26** (Android 8.0): la exige el lector de pantalla; deja fuera solo Android 7.x.
4. **La vecina de la historia 1 se llama Rosario**, como una persona real condenada en el caso en que se inspira
   la historia (REAL-CASES.md). Recomiendo cambiarle el nombre.
5. **Ritmo de la vecina**: aparece pronto y lleva la pista decisiva en 4 variantes (coherente, pero hace el caso
   más fácil). Tras el día de hoy llega por tema o por el parte del día 2-3.
6. **Fichas de hasta 565 palabras** (antes 460): 540 por la profundidad de los personajes y +25 por la regla contra
   premisas falsas; medido las dos veces sin pérdida en pistas ni estados.
7. **Dificultad por defecto: Detective** (5 preguntas, "Pensar" cuesta una). Historia: 7 y gratis; Veterano: 4 y
   sin ayudas.
8. **Efectos visuales activados por defecto** (retratos 2.5D, post-proceso): se quitan con `Theme.portraitLit`,
   `Theme.postFx` o el ajuste "Filtro noir".
9. **Cabeceras de historia sin arte, a propósito**: con el fondo a pantalla completa repetirían la escena.
10. **Unity Connect**: el editor lo activó solo; revertido (no subo servicios que no has pedido).
11. **Ruta de la build**: `Builds/Windows-final/` (la habitual estaba bloqueada por el sistema en la noche 2).
12. **Historial de la rama con ~300 MB de capturas en bruto** (error mío en el commit `ff30092`, retiradas en el
    siguiente). No he reescrito la historia remota sin tu permiso. Para que `main` no las herede: fusiona con
    `git merge --squash feature/dia3`. Si prefieres limpiar la propia rama: `git rebase -i 5745bb2`, marcar
    `98f436d` ("Untrack raw captures…") como `fixup` justo debajo de `ff30092`, y `git push --force-with-lease`.
13. **"Dice: «…»" en la libreta**: sin efecto medible en el bot (A/B 18 + 18: contradicciones 3 → 2, resueltas
    13 → 12, ruido), porque su detective no compara versiones con pistas. Lo dejo por diseño (es cómo se deduce en
    Her Story o L.A. Noire); si en pruebas con personas no ayuda, se quita en `Notebook.Format` (parámetro
    `interviewed`).
14. **Tus notas por sospechoso** (sospecha / descarte) no se validan ni cuentan para el final: son del
    jugador. El bot no las usa, así que no hay métrica; la idea es de Golden Idol y de las libretas de deducción
    (RESEARCH.md); el informe final solo las recuerda. Sustantivos ("sospecha", "descarte", "(tu descarte)" en la
    rueda) en vez de "sospechoso/a": los datos no guardan el género del personaje.

## 7. Pendientes
- APK de Android (instalar el módulo) y medir el post-proceso en un móvil real (estimado 1-2 ms).
- El proveedor LLM del móvil (punto 1 de las decisiones).
- Arte definitivo (retratos nuevos por emoción, intros, cabeceras) y audio definitivo (ART-NEEDED / AUDIO-NEEDED).
- Firma de publicación (keystore fuera del repositorio).

## 8. Cómo probarlo en Unity (en este orden)
1. Abrir el proyecto con **Unity 6000.3.2f1** y esperar a que importe (la primera vez tarda).
2. *Window → General → Test Runner* → **EditMode → Run All** (686 en verde).
3. **PlayMode → Run All** (46 en verde; las capturas están marcadas *Explicit* y no corren solas).
4. Para jugar con qwen: `ollama serve` y `ollama pull qwen2.5:7b-instruct`.
5. Abrir `Assets/Scenes/Game.unity` → **Play** con la ventana *Game* en 1080×1920:
   Jugar → un caso → leer el expediente (fondo de la historia) → Empezar → tocar una pregunta de ejemplo →
   Enviar → preguntar a otros → abrir la Libreta (verás "Dice: «…»" bajo quien ya ha contestado, con una
   indicación la primera vez; toca "añadir nota" para marcar sospecha o descarte) → **Pensar** dos veces (la segunda te lleva al sospechoso con la pregunta escrita) →
   Fin del día (parte de la mañana) → … → Acusar con una **prueba clave** → final con rango y ficha policial.
6. **Ajustes**: dificultad (para el siguiente caso), texto *Muy grande*, *Alto contraste*, *Filtro noir* apagado y
   encendido (el post-proceso se va y vuelve), volumen de música mientras suena una pista (la música se aparta).
7. Build: *Detective → Build de Windows* (o `-buildPath Builds/Windows-final/Detectives.exe`) y
   `Detectives.exe -batchmode -nographics -smoketest -logFile smoke.log` → "SMOKE OK".
8. Android: ver `docs/ANDROID-BUILD.md` (módulo, *Detective → Android → Aplicar ajustes*, build).
9. Lector de pantalla en el editor: *Window → Accessibility → Accessibility Hierarchy Viewer* con la escena en Play y
   `AssistiveSupport.screenReaderStatusOverride = ForceEnabled` (como hacen ScreenReaderTests); en el móvil, activa
   TalkBack y desliza por la pantalla: lee botones y textos en orden, y anuncia respuestas, pistas y el veredicto.
