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
  - nivel 1 (vago): "Quizá **Rosario** sabe más de lo que ha contado.";
  - nivel 2 (concreto, si vuelve a pedirla sobre la misma pista): "Pregúntale a **Rosario** por *lo que vio esa
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
