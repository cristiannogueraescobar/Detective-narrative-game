# Informe del día 3 (30 sep 2026, 10:09 → ~22:00)

Rama: **`feature/dia3`** (sale de `feature/noche2`), subida a origin después de cada bloque. **`main` no se ha
tocado.** Diario minuto a minuto: `docs/NIGHT-LOG.md`. *Borrador vivo: se completa al final del día.*

---

## 1. Resumen ejecutivo
1. **Lógica:** validador narrativo (9 reglas) en las 9 variantes, 17 incoherencias corregidas, línea temporal de
   cada variante, desbloqueos naturales por tema y máquina de estados con tests de toques reales.
2. **Jugabilidad:** "Pensar" (ayuda por niveles), dificultad (Historia/Detective/Veterano), prueba clave, rango del
   detective y "lo que se te escapó". Medido con el bot: primera pista 10,3 → 6,9 preguntas y resueltas 44 → 72 %.
3. **Aspecto:** retratos 2.5D (relieve + lámpara, elegidos frente a vóxel con capturas), post-proceso noir de URP
   con el contraste medido *después* del efecto, arte propio por historia, escala de radios, estados de botón y
   textos revisados. Todo reversible desde el tema o los Ajustes.
4. **Calidad:** revisión independiente (13 hallazgos, todos arreglados con test en rojo primero), 0 B de basura
   por fotograma, build de Windows con prueba de humo OK. Android configurado, pero sin APK (falta el módulo) y con
   un bloqueante real: **en el móvil ningún proveedor LLM funciona tal cual** (decisión para Cristian).

## 2. Galería antes / después
`docs/screenshots/2026-09-30/galeria/` (20 hojas; se regeneran con `python Tools/make_gallery_dia3.py`).
"Antes" = capturas del comienzo del bloque C (visualmente, el final de la noche 2).

| Hoja | Qué cambia |
|---|---|
| 01_menu | Título desde `GameTexts.GameName`, banda bajo el subtítulo, post-proceso |
| 02_casos | Expedientes con la víctima, radios de la escala |
| 03_expediente / 19_intros_por_historia | Fondo propio por historia (lluvia, cala, olivar) en vez de la sala genérica; reglas con las cifras de la dificultad |
| 04_interrogatorio | Chat anclado abajo, Acusar en aviso (no dorado), retrato 2.5D |
| 05_libreta | Botón "Pensar" |
| 06_acusacion | Retratos 2.5D en la rueda, prueba clave explicada |
| 07-09 veredicto y finales | Rango del detective, prueba clave, "lo que se te escapó" |
| 10_ajustes | Dificultad, pista de los deslizadores visible (WCAG 1.4.11) |
| 12_dialogo | Un solo aviso de reinicio con botones de acción |
| 13-14 texto grande / alto contraste | Siguen en AA (test) |
| 20_personajes | Los 7 retratos: plano 2D frente a relieve 2.5D |

## 3. Métricas antes / después

| Métrica | Antes (inicio del día) | Ahora |
|---|---|---|
| Tests EditMode | 549 | **668** |
| Tests PlayMode (sin capturas) | 20 | **36** |
| Horas inventadas por qwen (A/B, 18+18 partidas) | 27 / 619 respuestas | **7 / 630** (−74 %), latencia igual |
| Primera pista (bot, preguntas) | 10,3 (8,6 antes de B2) | **6,9** con Pensar |
| Partidas sin ninguna pista (bot) | 2 de 18 | **0** |
| Partidas resueltas (bot) | 44-56 % | **72 %** con Pensar |
| Detección de pistas (calibración, 48 pistas) | 80-85 % | 83 % con fichas más ricas (540 palabras) |
| 2C_gps (la pista que falló en 0c) | 60 % | **85 %** (10 intentos) |
| Incoherencias narrativas conocidas | 17 (auditoría) | 0 abiertas; validador en verde en las 9 variantes |
| Basura por fotograma del juego (reposo) | ≤ 0 B | ≤ 0 B (tras C3/C4/D1) |
| Contraste mínimo de texto tras el post-proceso | — | 6,08:1 (todos los pares suben) |
| Hallazgos de revisión abiertos | — | 0 de 13 |

## 4. Skills y herramientas usadas
- **superpowers:test-driven-development** en todo el código (cada arreglo de la revisión: test en rojo → verde).
- **design:design-critique** (C1, docs/DESIGN-CRITIQUE.md) y **design:ux-copy** (C6, glosario y tabla de textos).
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
- **Datos que decidieron**: reintento por horas (A/B), límite de 540 palabras (calibración), 2.5D frente a vóxel
  (capturas y vértices), raíces de desbloqueo (bot: el hermano salía el día 1 en 8/12 partidas).
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
3. **Paquete de Android provisional**: `com.cristiannoguera.casos` (no se puede cambiar una vez publicado).
4. **La vecina de la historia 1 se llama Rosario**, como una persona real condenada en el caso en que se inspira
   la historia (REAL-CASES.md). Recomiendo cambiarle el nombre.
5. **Ritmo de la vecina**: aparece pronto y lleva la pista decisiva en 4 variantes (coherente, pero hace el caso
   más fácil). Tras el día de hoy llega por tema o por el parte del día 2-3.
6. **Fichas de hasta 540 palabras** (antes 460): medido sin pérdida en pistas, estados ni premisas.
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

## 7. Pendientes
- APK de Android (instalar el módulo) y medir el post-proceso en un móvil real (estimado 1-2 ms).
- El proveedor LLM del móvil (punto 1 de las decisiones).
- Arte definitivo (retratos nuevos por emoción, intros, cabeceras) y audio definitivo (ART-NEEDED / AUDIO-NEEDED).
- Preguntas capciosas: qwen acepta premisas falsas ~22 % de las veces (limitación conocida, COHERENCE.md).
- Firma de publicación (keystore fuera del repositorio).

## 8. Cómo probarlo en Unity (en este orden)
1. Abrir el proyecto con **Unity 6000.3.2f1** y esperar a que importe (la primera vez tarda).
2. *Window → General → Test Runner* → **EditMode → Run All** (668 en verde).
3. **PlayMode → Run All** (36 en verde; las capturas están marcadas *Explicit* y no corren solas).
4. Para jugar con qwen: `ollama serve` y `ollama pull qwen2.5:7b-instruct`.
5. Abrir `Assets/Scenes/Game.unity` → **Play** con la ventana *Game* en 1080×1920:
   Jugar → un caso → leer el expediente (fondo de la historia) → Empezar → tocar una pregunta de ejemplo →
   Enviar → preguntar a otros → abrir la Libreta → **Pensar** dos veces (la segunda te lleva al sospechoso con la
   pregunta escrita) → Fin del día (parte de la mañana) → … → Acusar con una **prueba clave** → final con rango.
6. **Ajustes**: dificultad (para el siguiente caso), texto *Muy grande*, *Alto contraste*, *Filtro noir* apagado y
   encendido (el post-proceso se va y vuelve), volumen de música mientras suena una pista (la música se aparta).
7. Build: *Detective → Build de Windows* (o `-buildPath Builds/Windows-final/Detectives.exe`) y
   `Detectives.exe -batchmode -nographics -smoketest -logFile smoke.log` → "SMOKE OK".
8. Android: ver `docs/ANDROID-BUILD.md` (módulo, *Detective → Android → Aplicar ajustes*, build).
