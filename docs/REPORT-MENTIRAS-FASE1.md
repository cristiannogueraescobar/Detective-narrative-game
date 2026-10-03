# Mentiras de inocentes: fase 1

Rama `feature/mentiras-inocentes`, desde `main` (`6b7cb25`). **Sin fusionar.** Diseño:
`docs/DISENO-MENTIRAS-INOCENTES.md` (rama `feature/diseno-mentiras-inocentes`).

## Veredicto

Cumple casi todo:

| Criterio | Objetivo | Resultado |
|---|---|---|
| Pistas | ≥ 80 % | 84 % de media |
| Premisas falsas | ≤ 5 % | 1 % |
| El bot acierta al culpable | ≥ 14/18 | 22/24 |
| El bot acusa al inocente que miente | — | Nunca (0/24) |

Hay **dos cosas que no puedo dar por buenas sin que decidas**:

1. **La mecánica casi no se ve en 1B.** En 16 partidas del bot, la contradicción de un inocente salió en 4, todas en 1C.
   La causa es de diseño y está en el apartado 3.
2. **Coherencia de los estados emocionales:** 93,8 % (dos pasadas), por debajo del 95 %. `main` dio 95,6 % en una
   pasada. La diferencia no es significativa con 160 respuestas por pasada, y casi todos los fallos son de personajes
   cuya ficha no ha cambiado. Pero no llega al umbral (apartado 2).

## 1. Lo hecho (TDD)

| Qué | Dónde | Tests |
|---|---|---|
| La pista sabe qué mentira rompe (`exposesLieOf`; vacío = el culpable) y el papel guarda sobre qué miente de verdad (`lieAbout`) | `CaseModels` | `MentirasInocentesTests` |
| La puntuación solo cuenta las contradicciones del culpable: evidencia, máximo posible, informe y prueba clave "rompía su coartada" | `InvestigationState`, `GameManager` | `SoloCuentanLasContradiccionesDelCulpable`, `LaEvidenciaMaximaNoCuentaLasMentirasDeInocentes` |
| La pista que rompe la mentira de un inocente nunca le incrimina; un mentiroso inocente por variante como mucho | datos | `LaPistaQueRompeLaMentiraDeUnInocenteNuncaLoIncrimina`, `UnMentirosoInocenteComoMuchoPorVariante` |
| Fuga 1: enseñar una prueba hace reaccionar también al inocente que miente. Su ficha le dice qué hacer, como al culpable | `InvestigationState`, `PromptBuilder` | `EnsenarLaPruebaAlInocenteQueMienteTambienLeContradice`, `ElInocenteQueMienteSabeQueHacerSiLeEnsenanLaPrueba` |
| Fuga 2: el "Dice:" de la libreta llega igual para todos, en cuanto contesta | `CaseBriefing` | `LaVersionLlegaIgualParaTodos` |
| Fuga 3: el orden de "Pensar" no depende de quién es el culpable | `HintAdvisor` | `ElOrdenNoDependeDeQuienEsElCulpable` |
| Mismo formato para todas las contradicciones (libreta, aviso y tarjeta): "La versión de X («cita») choca con: pista" | `Contradictions.Describe` | `ElTextoNombraAQuienMienteConSuCita` |
| La mentira de cualquiera que tenga una se detecta en su respuesta | `TurnAnalyzer` | `SeDetectaLaMentiraDelInocenteEnSuRespuesta` |
| Informe final: si acusas a un inocente que mentía, una línea dice sobre qué | `EndingReport` | `AcusarAlInocenteQueMintioEsFinalMaloYDiceSobreQueMentia` |
| El guardado recuerda quién ha mentido; las partidas guardadas antes se siguen leyendo | `SaveSystem` | `SeGuardaQuienHaMentidoYUnaPartidaAntiguaSeSigueLeyendo` |
| Tope de pistas a 7 (decisión para 2C) | `CaseDataValidationTests` | `EstructuraDePistas` |
| **1B:** Daniel miente con «salí a cenar con clientes» (su secreto, la aventura). La rompe `1B_cena`, que además le descarta | `Story1HijaPerfecta` | `EnLaFase1DanielMienteSobreSuSecreto` |
| **1C:** Daniel miente con «llegué a casa a las 23:00» (su secreto: llegó a las 22:15, vio el golpe y no la llevó al hospital). La rompe `1C_llamada`, que incrimina a Lucas | `Story1HijaPerfecta` | ídem, y `LaMentiraDeDanielEn1CSeDetectaComoLaDice` |
| Calibrador nuevo de mentiras y registro de contradicciones en el bot | `LieCalibrator`, `BotPlayer` | `LieCalibratorTests` |

Las fichas de Daniel siguen dentro del tope de 565 palabras.

Suites: EditMode 956/957 (el que no corre es el benchmark, a petición) y PlayMode 67, con 0 fallos.

## 2. Calibración (10 intentos; rama frente a `main`, misma N)

Todo está en `C:\AI\dng-calibracion\fase1\`.

### Pistas de 1B y 1C

Media **84 %** en la rama y 86 % en `main`. Pistas ≥ 2/3: 11 de 13 en la rama, 10 de 13 en `main`.

| Pista | main | rama | Nota |
|---|---|---|---|
| 1B_receta (Daniel) | 90 % | 70 % | En la primera medida bajó a 40 % por mi cambio en su ficha (no lo decía o lo decía con otras palabras). Con las frases reales en las anclas y la reacción reescrita: 70 % |
| 1B_cena (Daniel / Amparo) | 55 % / 100 % | 45 % / 90 % | Por Daniel ya fallaba en `main` |
| 1C_llamada (Daniel / Amparo) | 57 % / 40 % | 70 % / 50 % | Ya fallaba en `main` |
| El resto | 90-100 % | 85-100 % | |

### Mentiras (calibrador nuevo)

La pregunta es «¿qué hizo esa noche?». La segunda columna es informativa: al enseñar la prueba, la contradicción sale
igual.

| | La cuenta al preguntarle por su noche | La repite con la prueba delante |
|---|---|---|
| 1B Carmen (culpable) | 8/10 | 0/10 |
| 1B Daniel (inocente) | **10/10** | 9/10 |
| 1C Lucas (culpable) | 9/10 | 8/10 |
| 1C Daniel (inocente) | **10/10** | 4/10 |

### Premisas falsas y estados emocionales

- **Premisas falsas:** 1/80 (1 %).
- **Estados emocionales** (160 respuestas por pasada):

| | Bien formada | Coherente |
|---|---|---|
| Rama, 1.ª pasada | 94 % | 93 % |
| Rama, 2.ª pasada | 98 % | 94 % |
| `main` | 98 % | 96 % |

Casi todos los fallos de la rama son de Lucas, Carmen y Amparo, cuyas fichas son idénticas en las dos versiones (se ha
comprobado que solo Daniel tiene la línea nueva). Dos son de Daniel: dice «tranquilo» al preguntarle por Elena.

### Bot (semilla 1919)

| | Partidas | Culpable acertado | Partidas con contradicción de un inocente | Acusó al inocente que mentía |
|---|---|---|---|---|
| `main` | 8 (4 por variante) | 8/8 | — | — |
| Rama | 8 (4 por variante) | 8/8 | sin registrar | 0/8 |
| Rama, con registro | 16 (8 por variante) | 14/16 | **4/16 (todas en 1C)** | **0/16** |

- Los dos fallos (1C) acusan a Carmen, no a Daniel, con razonamientos confusos del modelo detective (en uno da por
  buena su coartada y la acusa igual).
- Uno de esos dos tenía la contradicción de Daniel y el otro no.
- En las 3 partidas restantes con la contradicción de Daniel, el bot acertó con Lucas.

## 3. Lo que falla: por qué en 1B casi no sale la contradicción de Daniel

La contradicción necesita una de dos cosas: que Daniel diga su mentira en el chat, o que se le enseñe la prueba. El
bot le hace preguntas concretas («¿dónde estuviste justo antes de volver a las 23:05?»), así que no siempre la dice. En
1B_8, `1B_cena` salió y no hubo contradicción.

Pero desde esta fase la libreta enseña el "Dice: «salí a cenar con clientes»" en cuanto contesta (fuga 2). El jugador
ve la mentira escrita, tiene la pista que la rompe, y el juego no la cuenta. Antes no pasaba porque el "Dice:" del
culpable esperaba a su mentira, y eso era justamente la fuga.

**Propuesta, para que decidas:** la mentira cuenta como dicha en cuanto su versión está en la libreta, es decir, en
cuanto ha contestado. Igual para el culpable y para el inocente: lo que el jugador lee en la libreta es lo que el
personaje ha declarado.

| A favor | En contra (para medir) |
|---|---|
| Coherente con lo que se ve, y las contradicciones de inocentes saldrán mucho más | También saldrán más las del culpable (ya no hay que pillarle diciéndola): más finales buenos con la misma partida. Habría que volver a medir el bot y, si hace falta, subir el umbral del final bueno |

Alternativa: dejar la regla como está y que el "Dice:" espere a que lo diga, solo para quien miente. Volvería a delatar
a los mentirosos (ahora dos por variante), así que no la recomiendo.

## 4. Lo que salió mal por el camino

- La primera medida de pistas detectó la bajada de `1B_receta` (90 % → 40 %), causada por mi línea nueva en la ficha.
  Arreglada con las frases reales y otra redacción.
- La primera versión del calibrador de mentiras preguntaba también lo que presiona a confesar: daba 4/10 en 1C sin
  serlo. Ahora solo pregunta por su noche.
- La redacción nueva de la reacción de Daniel se pasaba 2 palabras del tope; acortada.
- Paré la primera serie a mitad para no medir premisas, estados y bot con una ficha que iba a cambiar.
- El bot no registraba contradicciones; lo añadí y volví a jugar con 8 partidas por variante.
- TDD: el calibrador de mentiras y el registro del bot (herramientas de medida) los escribí sin verlos antes en rojo.
  El motor y los datos, sí.

## 5. Decisiones para Cristian

1. **¿La mentira cuenta como dicha en cuanto su versión está en la libreta?** (apartado 3) Es lo que más afecta a que
   la mecánica se vea.
2. **Estados emocionales:** ¿lo damos por ruido o medimos con más N en las dos versiones (por ejemplo, 15 intentos)
   antes de la fase 2?
3. ¿Seguimos con la fase 2 (las otras variantes) después de decidir el punto 1?
