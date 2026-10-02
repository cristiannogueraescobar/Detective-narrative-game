# Diseño: los inocentes también mienten

Estado: **propuesta para revisar. No se ha tocado código ni fichas.** Rama `feature/diseno-mentiras-inocentes`.

## El problema

Hoy solo miente el culpable. Las contradicciones se calculan solo contra él (`InvestigationState.UpdateContradictions`,
que mira `CulpritToldLie` y lo que se le enseña al culpable). El texto de la contradicción lleva siempre su nombre
(`AIConversationManager.DescribeContradiction`). La primera contradicción, por tanto, resuelve el caso.

Lo que propones: que algún inocente mienta sobre **su propio secreto**. Una contradicción demostraría que alguien
miente, no que sea culpable, y el jugador tendría que deducir **sobre qué** miente cada uno.

Al revisar el código han salido más fugas del mismo tipo. Se arreglan con el mismo cambio y están en el apartado 5:

- Enseñar una pista ⚡ a cada sospechoso funciona como detector del culpable.
- La libreta solo retrasa el "Dice:" del culpable.
- "Pensar" sugiere primero las pistas del culpable.

## 1. Mentira de un inocente por variante

Criterios para elegir:

- El secreto ya existe en su ficha.
- La mentira es creíble y va sobre algo suyo, no sobre el crimen.
- Si es posible, la contradice una pista que ya existe o un segundo portador (`alsoHeldBy`, que no suma pistas).
- La ficha tiene margen de palabras. Calculado con la construcción de `PromptBuilder` y el día 1; el tope es 565 y
  entre paréntesis va lo que queda hoy.

Las fichas casi llenas no se tocan: Carmen en 1C (564), Marcos en 2B (561), Lucas en 1A (542) y Ruiz en 2A (546).

| Var. | Culpable y su mentira | Inocente que miente (margen) | Su mentira (cita para la libreta) | Qué la contradice | Pista nueva |
|---|---|---|---|---|---|
| 1A | Daniel: «no entré en su cuarto» (22:30, el cuarto de Elena) | Carmen (25) | «lo mío es melatonina, nada más» (secreto: zolpidem cada noche) | `1A_frasco` «El frasco medio vacío», con Amparo de segundo portador: «Carmen compra zolpidem en la farmacia cada mes» | No (segundo portador) |
| 1B | Carmen: «estaba perfecta; la encontré a las 23:10» | Daniel (23) | «salí a cenar con clientes» (**ya lo dice en su versión**; secreto: una aventura) | `1B_cena` «La cena del viernes», con Amparo de segundo portador (sale el 90 %) | No |
| 1C | Lucas: «estuve con los cascos, no oí nada» (21:45, la escalera) | Daniel (51) | «llegué a las 23:00» (**ya lo dice**; secreto: llegó a las 22:15, vio el chichón y no la llevó al hospital) | `1C_llamada`, en la versión de Amparo: «a las diez y cuarto llegó el coche del padre» | No |
| 2A | Marcos: «no salí del bar» (5:05-6:30) | Andrés (83) | «en quince años no se me ha quedado una carta» (secreto: guarda postales sin entregar) | Nueva: Maruxa (80): «la postal de mi sobrina nunca llegó; dicen que el cartero se las guarda» | Sí (5 → 6) |
| 2B | Andrés: «a las cinco y cuarto ya estaba clasificando» | Maruxa (81) | «el humo es de la cocina de leña, fillo» (secreto: destila orujo sin licencia) | Nueva: Ruiz (40): «medio pueblo le compra el orujo a Maruxa» | Sí (5 → 6) |
| 2C | Ruiz: «estuve en comisaría hasta las seis» | Andrés (47) | «en quince años no se me ha quedado una carta» | Nueva: Maruxa (76), la de 2A | Sí (6 → **7**: hay que subir el tope del test o quitar una) |
| 3A | Javier: «cenó tranquila y se acostó a las diez» | Lucía (108) | «mi vida está en Granada; no pienso irme» (secreto: iba a mudarse a Madrid con Paula) | Nueva: Álex (91): «mamá ya miraba pisos en Madrid» | Sí (5 → 6) |
| 3B | Lucía: «estuve todo el sábado en Granada» (desde las 19:00) | Javier (33) | «dos cañas, nada más; volví sereno» (secreto: bebió y no entró a ver a Paula) | Nueva: Encarna (75): «a las 21:40 volvió haciendo eses y no se encendió la luz del cuarto de la niña» | Sí (5 → 6) |
| 3C | Encarna: «el sábado no vi a Paula» (20:00, su finca) | Álex (131) | «estuve en casa con mi madre toda la noche» (secreto: de fiesta, sin mirar el móvil hasta medianoche) | Nueva: Lucía (124): «salió de fiesta y volvió a la una» | Sí (5 → 6) |

Notas por variante:

- **1B y 1C no necesitan pista nueva.** Daniel ya miente en su versión; solo falta que el juego lo registre. 1B es el
  ejemplo ideal: lo que destapa su mentira (`1B_cena`) **le descarta**, porque estaba fuera de casa a la hora del crimen.
- **1C es el caso más difícil a propósito.** Daniel miente sobre esa misma noche: llegó a las 22:15, no a las 23:00.
  Pero su secreto es haber visto el golpe y no actuar, no el golpe en sí. La pista que le contradice incrimina a Lucas
  (la llamada llorando a las 21:52). Lo recomiendo, pero es el que más hay que calibrar con el bot.
- **1A:** la mentira de Carmen roza el arma (el zolpidem). Es buena pista falsa, porque la deja sospechosa sin serlo. El
  riesgo es que el jugador acuse a la madre. Alternativa más suave: Lucas y los porros, pero su ficha no tiene sitio y
  1A ya tiene 6 pistas.
- **Descartados:**
  - Amparo y los prismáticos: si miente sobre cómo vio, el jugador duda de su propia ⚡.
  - Marcos y el tabaco en 2C: el puerto y las lanchas son el motivo del culpable, Ruiz.
  - Ruiz y las copas en 2A: solo lo sabe Marcos, que es el culpable.
  - Álex en 3B: ya miente, pero repitiendo la coartada de su madre, que es la culpable. Esa mentira apunta a Lucía y se
    queda como está.
- Con esto hay **6 pistas nuevas** (2A, 2B, 2C, 3A, 3B y 3C) y **2 segundos portadores** (1A, y el de 1B que ya
  existe). 2C pasa de 6 a 7: hay que subir el tope de `EstructuraDePistas` a 7 o quitar una pista que se solape.

## 2. Cómo distingue el jugador una mentira "irrelevante" de la del culpable

El juego no lo dice: todas las contradicciones salen igual. El jugador tiene cuatro preguntas, y las pistas están
pensadas para que se puedan contestar con lo que ya tiene en la libreta:

1. **¿Sobre qué tema miente?** El culpable miente sobre dónde estaba o qué hizo en la franja del crimen. Los inocentes,
   sobre un vicio o un plan suyo: la aventura, el zolpidem, el orujo, las postales, Madrid, la fiesta, la bebida.
2. **¿Sobre qué hora?** La mentira del culpable cubre la hora del forense (el parte del día 2 la da). Las de los
   inocentes van sobre otro día (las postales, Madrid, el orujo) u otra franja de esa noche. Por ejemplo, Daniel en 1C
   habla de las 22:15-23:00, no de las 21:45.
3. **¿Sobre qué lugar?** El culpable, sobre la escena o su camino. Daniel en 1B estaba en un restaurante; Álex en 3C,
   de fiesta en la ciudad, no en la finca.
4. **¿Qué demuestra la pista que la rompe?** La del culpable le pone en la escena a la hora del crimen. La del inocente
   le pone **en otro sitio** (`1B_cena` le descarta) o destapa algo que no tiene que ver con la muerte.

Además, las pistas que incriminan (`Incriminates`) siguen apuntando solo al culpable. Un inocente que miente puede
tener una contradicción, pero no pistas que le acusen.

Regla de datos para que se cumpla: la pista que rompe la mentira de un inocente nunca es de tipo `Incriminates`
contra él. Es `Clears` (le descarta) o `Context` (contexto). La comprobaría un test nuevo (apartado 5).

## 3. Puntuación final

| Situación | Hoy | Propuesta |
|---|---|---|
| Contradicciones que suman | Todas, ×2 | **Solo las del culpable** ×2. Las de los inocentes no suman (ni restan) |
| `MaxEvidenceWithoutCulprit` | Cuenta todas las ⚡ | Cuenta solo las ⚡ de la mentira del culpable (si no, inflaría el máximo) |
| Prueba clave "rompía su coartada" | Cualquier ⚡ | Solo si rompe la mentira **del acusado y este es el culpable** |
| Acusar a un inocente que mintió | Final `Bad`, rango Novato | Igual: final `Bad`, rango Novato. Y una línea nueva en el epílogo que enseña la diferencia: «X mentía, sí: sobre su aventura, no sobre la noche del viernes» (el tema de su secreto, en la voz del informe) |
| Acusar a un inocente con una pista que lo descartaba | `ignoredClearingClue`, con su texto | Igual. En 1B coinciden: `1B_cena` destapa la mentira de Daniel y le descarta |

Para que la puntuación sepa de quién es cada contradicción, hace falta un dato nuevo en la pista: **de quién es la
mentira que rompe** (`exposesLieOf`, el id del personaje; vacío = la del culpable). Las ⚡ que existen hoy no cambian.

## 4. Libreta y tarjeta

Mismo formato para todas, sin distinguir culpable de inocente:

> La versión de {quien miente} («{su cita}») choca con: {pista}

- `DescribeContradiction` toma la cita del personaje de `exposesLieOf`, no del culpable.
- El sello "CONTRADICCIÓN", la sacudida del retrato y el aviso del chat, iguales para todos.
- La libreta y la tarjeta "TUS PRUEBAS" ya usan esa función, así que no cambian.
- En la libreta, el "Dice:" de cada sospechoso sale igual para todos: hoy el del culpable aparece más tarde, y eso
  también le delata.

## 5. Lo que hay que cambiar (para la estimación, no se toca aún)

- **Datos:**
  - Cada personaje puede tener `lieAnchors` y `lieQuote` en su papel de esa variante (hoy solo el culpable).
  - `ClueData.exposesLieOf`.
- **Estado:**
  - `CulpritToldLie` pasa a un conjunto de quién ha mentido.
  - `UpdateContradictions` cruza cada ⚡ con la mentira de su personaje, contada o enseñada a él.
  - El guardado (`SaveSystem`) guarda ese conjunto, y una partida guardada antigua sigue cargando (migración).
- **Análisis del turno:** la detección de mentira se hace para cualquier personaje con mentira (hoy solo el culpable,
  en `TurnAnalyzer`).
- **Ficha (`PromptBuilder`):** a los inocentes con mentira, la misma instrucción que al culpable para sostenerla. Hoy
  solo se les pide negar sus pistas secretas la primera vez.
- **Fugas que se cierran a la vez:**
  - Enseñar una ⚡ al culpable ya no es un detector único.
  - El "Dice:" de la libreta sale igual para todos.
  - "Pensar" (`HintAdvisor`) no ordena primero las pistas del culpable.
  - `CoherenceReport` no da por hecho que toda ⚡ es del culpable.
- **Tests nuevos:**
  - La mentira de cada inocente se detecta en su versión y no salta con sus admisiones.
  - La pista que rompe la mentira de un inocente nunca le incrimina.
  - Las contradicciones de inocentes no suman evidencia.
  - Acusar a un inocente que mintió da `Bad` y el epílogo nuevo.
  - Por variante, la evidencia máxima contra el culpable sigue llegando al final bueno (`ResolvabilityTests`).
  - `LaFichaDelPortadorNoDisparaSusPropiasPistas`, también para los segundos portadores.
- **Tests que cambian:** los que usan `CulpritToldLie`, `lieQuote` o `DescribeContradiction`. Son unos 12:
  InvestigationState, BriefingAndNotebook, Conversation, NarrativeValidator, ArreglosInterfaz, TarjetaPruebas,
  GameSmoke…

## 6. Riesgos

| Riesgo | Por qué | Cómo se controla |
|---|---|---|
| Tope de 565 palabras | Cada mentira añade una instrucción (unas 15 palabras) a la ficha de quien miente, y cada pista nueva, su tema y su hecho (20-30) a la de quien la cuenta | Elegidos los que tienen margen. Los más justos: Daniel en 1B (23) y Carmen en 1A (25), que solo añaden la mentira. `FichasDentroDelLimiteDePalabras` lo vigila |
| Recalibración de pistas | 6 pistas nuevas y 2 vistas de segundo portador | `ClueCalibrator` con 10 intentos, cada una ≥ 7/10 (la regla de CLUE-LOGIC) |
| Detección de las mentiras nuevas | Ningún calibrador mide hoy si se detectan las mentiras | Calibrador nuevo (o modo del de pistas): el inocente dice su mentira al preguntarle por su tema (≥ 7/10) y no salta cuando admite |
| Premisas y estados | Cambian las fichas | `PremiseCalibrator` (≤ 5 %) y `EmotionCalibrator` (≥ 95 %) |
| Que el culpable deje de ser deducible | Más ruido | Las pistas que incriminan siguen siendo solo suyas. Test de resolubilidad por variante. Bot con 4 partidas o más por variante: hoy acierta 16-17 de 18. Ver que no baja de 14 y medir **cuántas veces acusa a un inocente que mintió** |
| El modelo miente de más | Un inocente con "miente sobre X" puede extender la mentira a otras cosas | Instrucción acotada al tema; las premisas lo miden |
| Partidas guardadas | Cambia lo que se guarda | Migración: lo antiguo se lee como "solo el culpable" |

## 7. Estimación

| Bloque | Trabajo |
|---|---|
| Modelo, estado, análisis, guardado y puntuación, con TDD | 1 sesión (3-4 h) |
| Datos de las 9 variantes: mentiras, 6 pistas nuevas, 2 segundos portadores, fichas dentro del tope | 1 sesión (3-4 h) |
| Fugas (detector de ⚡, "Dice:", "Pensar", epílogo nuevo) | Media sesión (1-2 h) |
| Calibración: pistas y mentiras (10 intentos), premisas, estados y bot (4 partidas × 9 variantes), y arreglos según resultados | 1-2 sesiones (4-8 h, sobre todo tiempo de GPU) |
| **Total** | **3,5-4,5 sesiones** de trabajo autónomo, más tu revisión |

Orden propuesto, de menos a más riesgo:

1. El motor (puntuación por personaje y formato igual para todos).
2. 1B y 1C, que no necesitan pistas nuevas: Daniel ya miente. Calibrar y probar con el bot.
3. El resto de variantes, solo si 1B y 1C salen bien.

## Decisiones para ti

1. **¿1C con Daniel?** Es la más interesante: miente sobre la misma noche, pero por su secreto. También la que más
   puede confundir.
2. **1A:** ¿Carmen y el zolpidem (pista falsa fuerte) o nada en 1A?
3. **2C:** ¿subir el tope del test a 7 pistas o quitar una?
4. **Epílogo al acusar a un inocente que mintió:** ¿decir sobre qué mentía? Recomiendo que sí: enseña a jugar.
5. ¿Una sola mentira de inocente por variante (lo propuesto) o dos en algunas?
