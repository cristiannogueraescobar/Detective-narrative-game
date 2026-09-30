# Investigación (día 3)

Cada entrada: fuente → qué aprendí → qué decidí con ello. La búsqueda la hizo un subagente con búsqueda web (30 sep
2026); las fuentes están enlazadas para poder comprobarlas. Lo que no pude verificar va marcado.

## Deducción, ritmo y ayudas en juegos de detectives

### Return of the Obra Dinn
- https://en.wikipedia.org/wiki/Return_of_the_Obra_Dinn · https://www.hardcoregaming101.net/return-of-the-obra-dinn/
- Las respuestas correctas solo se confirman **de tres en tres**: no se puede probar a ciegas, hay que estar casi
  seguro. El cuaderno está estructurado desde el principio (lista de tripulantes, huecos que rellenar), nunca pide
  escribir prosa.
- **Decisión:** el tablero de deducción del juego (B2) es de huecos que se rellenan con pistas ya encontradas, no
  de texto libre, y no confirma hueco a hueco.

### Her Story
- https://en.wikipedia.org/wiki/Her_Story_(video_game) · https://pcgamer.com/the-story-behind-her-story/3
- La investigación es el vocabulario del jugador: cada palabra nueva es una búsqueda nueva. La teoría la construye
  el jugador, no un menú.
- **Decisión:** encaja con nuestro chat libre. Refuerza tener preguntas de ejemplo (para empezar) pero no un menú
  de preguntas cerradas.

### The Case of the Golden Idol
- https://www.gamedeveloper.com/design/case-of-the-golden-idol ·
  https://www.gamedeveloper.com/road-to-igf-2023/-the-case-of-the-golden-idol-i-used-frequent-testing-to-improve-its-mystery-solving ·
  https://www.rpgfan.com/review/the-case-of-the-golden-idol/
- Correcto/incorrecto sobre una respuesta grande frustraba; lo resolvieron con **"2 o menos huecos están mal"**, sin
  confirmar nunca un hueco suelto ("los jugadores optimizan la diversión fuera del juego"). Huecos tipados (solo
  nombres en un hueco de nombre). Mapa del "Thought Path" de cada caso. Ayudas **por niveles** con una pequeña
  fricción antes de cada una.
- **Decisión:** el tablero da un aviso parcial ("algo no cuadra" / "casi") sin decir qué hueco; la ayuda del agente
  va por niveles y cuesta algo (una pregunta del día).

### Duck Detective: The Secret Salami
- https://www.destructoid.com/review-duck-detective-the-secret-salami/ ·
  https://gamingtrend.com/reviews/duck-detective-the-secret-salami-review-its-time-to-quack-the-case-ducktective ·
  https://press-start.com.au/reviews/nintendo-switch/2024/05/24/duck-detective-the-secret-salami-review-hard-boiled-deductions/
- Las palabras clave de las conversaciones van solas al cuaderno y rellenan "deducciones". **Dos modos** que se
  cambian en cualquier momento: Detective (solo "hay algo mal") e Historia (marca los huecos malos). El detective
  "rumia" una deducción y apunta vagamente a qué averiguar.
- **Decisión:** dificultad con dos o tres niveles (Historia / Detective / Veterano) en Ajustes, cambiable en
  cualquier momento; "rumiar" = la sugerencia sutil del agente cuando el jugador se atasca.

### Shadows of Doubt
- https://gamerjournalist.com/how-to-submit-evidence-in-shadows-of-doubt ·
  https://steamcommunity.com/app/986130/discussions/0/3844430784756171853
- Solo el nombre del culpable es obligatorio; cada campo extra correcto (arma, motivo, prueba) sube la recompensa.
- **Decisión:** acusar sigue siendo elegir a una persona; el tablero es opcional y premia (rango, final) al que lo
  rellena bien.

### L.A. Noire
- https://windowscentral.com/la-noire-changes-interrogation-techniques-remaster ·
  https://breezewiki.discard.no/lanoire/wiki/Case_Report
- "Verdad / Duda / Mentira" se criticó porque el jugador no sabía qué iba a hacer el detective; en la remasterización
  pasó a "Poli bueno / Poli malo / Acusar". Pillar una mentira exige una prueba concreta. Nota de 1 a 5 estrellas.
- **Decisión:** al enseñar una prueba el efecto lo decide el juego (contradicción sí/no, con reglas), el LLM solo lo
  interpreta. Ya es así; se mantiene. Rango de detective al final.

### Juegos de interrogatorio con LLM
- Vaudeville: https://www.keengamer.com/articles/previews/vaudeville-preview-the-ai-questioning-to-nowhere/ —
  personajes que se contradicen, ignoran y repiten: el progreso depende de la suerte del modelo.
- Suck Up!: https://community.openai.com/t/vampire-game-where-you-convince-llm-to-let-you-in/604295 — un medidor
  de confianza con reglas del juego; el LLM solo da color.
- 1001 Nights: https://arxiv.org/pdf/2308.12915 — palabras clave en el texto generado se convierten en objetos del
  juego (gancho determinista sobre texto libre).
- "Structured Knowledge Trees" (arXiv 2609.23043, **no verificado por mí**): hechos autorizados por fase, con
  verificación; menos alucinaciones y spoilers, a costa de revelaciones algo forzadas.
- **Decisión:** es lo que ya hacemos (fichas + anclas deterministas decidiendo las pistas). Refuerza: 0d (reintento
  por horas inventadas), el validador narrativo (A1) y medir con el bot en vez de fiarse del modelo.

### Sistemas de ayuda
- Thimbleweed Park: https://blog.thimbleweedpark.com/hints_and_dialogs.html — la línea de ayuda sabe dónde está el
  jugador, 2–3 pistas de vaga a explícita, nunca destripa lo que aún no ha visto, y pide un pequeño esfuerzo.
- **Decisión:** ayuda del agente por niveles, contextual (sabe qué pistas faltan y a quién le quedan), sin destripar
  al culpable.

## Síntesis aplicada al juego
1. Tablero de deducción con huecos (motivo, oportunidad, prueba) rellenados con pistas encontradas.
2. Aviso parcial al comprobar (sin confirmar hueco a hueco).
3. Dificultad elegible en cualquier momento.
4. Ayuda por niveles con coste, contextual y sin spoilers.
5. Detección de atasco → pensamiento del detective.
6. El juego decide con reglas; el LLM solo interpreta (ya lo hacemos: mantener y medir).
7. Rango de detective y expediente de los 9 casos para rejugar.
8. Ritmo: que cada día pueda dar algo nuevo; nada que regale al culpable pronto.

(Las decisiones concretas y lo que se implementó están en docs/GAME-DESIGN.md.)
