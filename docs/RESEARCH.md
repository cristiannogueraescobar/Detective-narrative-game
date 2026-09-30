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

## Android: nivel de API objetivo (Bloque E, 30-09-2026)
- Desde el **31-08-2026**, las apps nuevas y las actualizaciones en Google Play deben apuntar a **Android 16 (API
  36)**; las ya publicadas, a la 35 como mínimo para seguir visibles a usuarios nuevos; se podía pedir prórroga
  hasta el 01-11-2026. → `AndroidSetup.TargetSdk = 36`.
  Fuentes: [developer.android.com: target SDK](https://developer.android.com/google/play/requirements/target-sdk),
  [Ayuda de Play Console](https://support.google.com/googleplay/android-developer/answer/11926878).

## Lector de pantalla en Unity 6 (accesibilidad, ronda final 2)
- Unity 6 tiene un **módulo de accesibilidad** con soporte de **TalkBack (Android) y VoiceOver (iOS)**:
  `AssistiveSupport` activa el lector y recibe sus eventos; la interfaz se describe en un `AccessibilityHierarchy`
  de `AccessibilityNode` (separado de los GameObject) con rol (botón, deslizador, título, imagen), etiqueta,
  estado y acciones. Visor: *Window → Accessibility → Accessibility Hierarchy Viewer*.
- Requisito: **Android 8.0 (API 26)** e iOS 13 → si se añade, subir `AndroidSetup.MinSdk` de 25 a 26.
- Fuentes: [Unity 6: accesibilidad móvil](https://docs.unity3d.com/Manual/mobile-accessibility.html),
  [Unity 6.3: accesibilidad](https://docs.unity3d.com/Manual/accessibility.html).

## Deriva de idioma en los LLM (ronda 5)
- Fenómeno documentado ("language drift"): el modelo contesta en un idioma no pedido, sobre todo cuando el contexto
  mezcla idiomas y a medida que crece la conversación; una respuesta en otro idioma en el historial lo refuerza.
  En el bot lo vimos una vez (3A): el detective (qwen) escribió medio en chino y el sospechoso siguió en chino.
- Mitigación aplicada: detectar escrituras que el español no usa (LanguageCheck) y pedir otra vez "solo en
  español" antes de que entre en el historial; el bot, además, no envía preguntas en otro alfabeto.
- Fuentes: [Language Drift in Multilingual RAG (AAAI)](https://ojs.aaai.org/index.php/AAAI/article/view/40417),
  [arXiv 2511.09984](https://arxiv.org/html/2511.09984v1).

## Notas del jugador en la libreta (ronda 9)
- En los juegos de deducción que mejor funcionan, el jugador conduce el razonamiento: la libreta donde él mismo
  tacha lo descartado "estrecha" el caso poco a poco; que el juego lo haga todo se siente como llevar de la mano.
  → Nota por sospechoso (sospechoso / descartado) que toca el jugador; la rueda atenúa a los descartados sin
  impedir elegirlos (es su nota, no una regla).
- Fuentes: [Critical Play: Mysteries](https://mechanicsofmagic.com/2024/05/06/critical-play-mysteries-94/),
  [Detective prototype development report (Staffordshire)](https://gradex.staffs.ac.uk/wp-content/uploads/2026/05/5755_Detective-Prototype-Development-Report.pdf).

## Preguntas que no sacan nada (ronda 12)
- Opiniones de jugadores de juegos de interrogatorio con IA (Homicide Desk y similares): lo que más frustra es que
  una pregunta vaga se conteste con evasivas mientras corre el límite de preguntas, y tener que "adivinar las
  palabras" que quiere el juego. Lo que gusta: que el jugador se comporte como un investigador de verdad.
- Nuestros datos (1290 preguntas del bot en 19 rondas): con una hora ("¿viste algo raro a las 5?") salen pistas en
  el 9 %; sin hora, largas, en el 15 %; las cortas sobre un tema ("¿Tomaba Elena alguna medicación?") en el 88 %,
  aunque casi todas son las que sugiere Pensar (sesgo). Por eso el A/B del bot (`-topicQuestions`) antes de tocar
  el consejo del juego ("horas, lugares, objetos").
- Fuentes: [Homicide Desk (reseñas)](https://vaporlens.app/app/4935210/homicide_desk),
  [Narrative reliability in LLM detective games](https://arxiv.org/pdf/2609.23043).
