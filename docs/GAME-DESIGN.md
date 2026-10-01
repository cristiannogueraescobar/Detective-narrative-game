# Diseño de juego: deducción, ritmo y recompensa (día 3)

Base: la investigación de docs/RESEARCH.md y los datos del bot (línea base del día 3, 18 partidas: primera pista
en la pregunta 8,6 de media, **95 % de turnos sin información nueva**, 2 partidas sin ninguna pista, culpable
acertado en el 56 %).

## Problemas que atacamos
1. **Se acumula, no se deduce.** Las pistas caen en la libreta y la acusación solo pide un nombre: nada obliga a
   razonar *por qué* es esa persona (Golden Idol, Shadows of Doubt: la respuesta tiene partes).
2. **Atascos largos.** 95 % de turnos "muertos"; un jugador que no sabe qué preguntar no tiene salida más allá del
   consejo del día 3.
3. **El final no enseña.** Si fallas, no sabes qué se te escapó; no hay motivo para rejugar las otras variantes.
4. **Una sola dificultad.** 5 preguntas × 7 días para todo el mundo.

## Mejoras (por impacto)

### 1. "Pensar" (ayuda por niveles, con coste) — *Golden Idol, Duck Detective, Thimbleweed*
- Botón **Pensar** en la libreta. El detective "rumia" y da una pista **sobre cómo seguir**, nunca sobre quién
  fue:
  - nivel 1 (vago): "Quizá **Amparo** sabe más de lo que ha contado.";
  - nivel 2 (concreto, si vuelve a pedirla sobre la misma pista): "Pregúntale a **Amparo** por *lo que vio esa
    noche en la casa de enfrente*."
- Elige una pista **aún no encontrada** de alguien **ya disponible**; primero las que no son la decisiva (⚡), para
  que la ayuda no regale el caso.
- **Cuesta una pregunta del día** (fricción de Golden Idol). Sin preguntas, no se puede pensar.
- Todo sale de los datos (portador y tema de la pista): determinista y testeable; el LLM no interviene.

### 2. "La prueba clave" en la acusación — *Shadows of Doubt, L.A. Noire*
- Tras elegir a quién acusas: "¿Qué prueba lo demuestra?" (opcional, entre las pistas de tu libreta).
- Si eliges una pista que **rompe su coartada** (⚡) o que le incrimina, el informe lo reconoce y sube tu
  **rango**; si eliges una que no le señala, el informe te lo dice. El final (bueno/agridulce/…) no cambia: la
  prueba clave premia el razonamiento sin castigar a quien no la usa.

### 3. Resumen del caso en el final — *Obra Dinn, L.A. Noire*
- Rango del detective (*Novato → Sabueso → Inspector jefe*), según el final, la prueba clave, las ayudas y los
  días que sobraron.
- **Pistas que se te escaparon**: cuáles eran y quién las sabía (así se aprende a preguntar y dan ganas de
  rejugar), junto a la línea temporal real que ya existe.
- El mejor rango de cada historia queda en la selección de caso (junto al mejor final).

### 4. Dificultad — *Duck Detective*
- **Historia**: 7 preguntas al día y "Pensar" gratis. **Detective** (por defecto): 5 preguntas, pensar cuesta una.
  **Veterano**: 4 preguntas y sin "Pensar".
- En Ajustes, se aplica al empezar un caso (no cambia una partida a medias: conservador).

## Lo que NO se hace (y por qué)
- **Tablero con validación por lotes** (Obra Dinn / Golden Idol): con 5–6 pistas y 4 sospechosos, cualquier
  validación parcial permite adivinar al culpable por descarte. La "prueba clave" consigue lo mismo (razonar
  el porqué) sin ese agujero.
- **Medidor de tensión visible**: el estado emocional ya lo da el retrato; un medidor encima lo duplicaría.

## Cómo se mide (B3)
Bot con las mismas semillas, antes y después: preguntas hasta la primera pista, turnos sin pista nueva,
respuestas repetidas, % de partidas resueltas y duración. El bot usa "Pensar" igual que un jugador atascado
(cuando lleva varias preguntas sin nada nuevo).

### Resultado (30-09, semilla 59, 18 partidas por lado, qwen2.5:7b, reintento de horas activo)
Mismo código y mismas semillas; lo único que cambia es `-noHints`. Registros en `Logs/dia3-b3/{sin,con}/`.

| Métrica | Sin "Pensar" | Con "Pensar" | Cambio |
|---------|-------------:|-------------:|--------|
| Preguntas hasta la primera pista | 10,3 | **6,9** | −33 % |
| Partidas sin ninguna pista | 2 | **0** | nadie se queda en blanco |
| Turnos sin pista nueva | 96 % | **88 %** | −8 pp |
| Respuestas repetidas | 0 | 1 | igual |
| Partidas resueltas (culpable) | 44 % | **72 %** | +28 pp |
| Latencia media por respuesta | 974 ms | 1085 ms | ruido de la máquina (misma capa LLM) |
| Ayudas usadas | 0 | 100 (5,6 por partida) | — |

Lectura: "Pensar" cumple su papel (desatascar), pero el bot la pide cada 4 turnos vacíos, más que un jugador.
Por eso en dificultad Detective cuesta una pregunta y en Veterano no existe: la ayuda no puede ser gratis y
constante. La prueba clave, el rango y el resumen del final no los mide el bot (no cambian lo que pregunta);
se comprueban con tests (DetectiveRankTests, StateMachineTests) y con las capturas del final.
Pendiente: el 88 % de turnos sin pista nueva sigue alto porque cuenta cada pregunta de ambiente; la métrica que
importa al jugador (tiempo hasta la primera pista y partidas en blanco) es la que mejora.

## Rondas finales (tarde del día 3): lo que se añadió y por qué
- **La libreta apunta lo que dice cada uno** ("Dice: «…»" bajo cada sospechoso en cuanto ha contestado). La
  deducción de Her Story y L.A. Noire es comparar lo que alguien afirma con lo que prueban los hechos; hasta ahora
  el jugador tenía que recordarlo. Una quinta indicación del tutorial lo explica la primera vez. **Medido con el
  bot** (A/B 18 + 18): sin efecto (contradicciones 3 → 2, resueltas 13 → 12, ruido), porque su detective no
  compara versiones; se mantiene por diseño y queda como decisión para Cristian si las pruebas con personas no lo
  confirman.
- **"Pensar" y la prueba clave, explicados donde hacen falta**: el consejo de atasco del parte menciona Pensar (si
  la dificultad tiene ayudas); la explicación de la prueba clave sale en el resumen de la acusación solo cuando hay
  pistas (sin pistas el selector no aparece y la frase confundía).
- **Rango honesto**: acertar sin pruebas ("Sobreseído") ya no da un rango alto; los días de sobra solo premian un
  caso bien cerrado.
- **Finales con cara**: la ficha policial del culpable cierra el informe; "solidez de las pruebas N de M" en vez
  de "evidencia N/M", que se confundía con el recuento de pistas.
- **Preguntas capciosas**: los sospechosos ya no aceptan premisas falsas (23 % → 2-5 %, sin perder pistas): el
  jugador no puede "sembrar" hechos, y lo que niega un sospechoso vuelve a significar algo.
- **Tus notas en la libreta** (*Golden Idol*, libretas de deducción): junto a cada sospechoso, "añadir nota" →
  "sospecha" → "descarte" (sustantivos: valen para cualquiera). Es el razonamiento del jugador, no del juego: nada se valida (así no reabre el
  agujero del tablero por lotes). En la rueda, a quien descartaste se le ve tachado y atenuado, "(tu descarte)",
  pero se puede acusar igual. Se guarda con la partida, y el informe final lo recuerda ("Tu nota sobre X ya
  decía «sospecha»: buen olfato", o «descarte»: "era quien lo hizo").
- **La rueda marca también a quien descarta una pista ya encontrada** ("(pista de descarte)", como la libreta): en
  la 2B el bot acusaba a Marcos teniendo la grabación que lo descarta, 10 de 10 veces; un jugador cansado puede
  caer en lo mismo. No impide elegirlo (el final malo lo explica si lo hace).
  Medido: con las marcas de la rueda a la vista, el bot pasa de 0/20 a 3/8 en la 2B (rondas 25-26).
- **Rejugar trae otro culpable**: la historia elige primero una variante que no has resuelto; con las tres
  jugadas, nunca la misma dos veces seguidas (antes, al azar: la misma solución 1 de cada 3 veces).
- **Los partes de la mañana quedan en la libreta** para releerlos (antes solo en la tarjeta del día), y el
  informe final acaba con los culpables posibles que quedan por ver.
- **La tarjeta del día reconoce lo de ayer** ("Ayer: dos pistas nuevas y una contradicción"): casi todas las
  preguntas no dan nada nuevo, y el progreso se nota donde el día cambia.
