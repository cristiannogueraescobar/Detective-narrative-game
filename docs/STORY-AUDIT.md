# Auditoría de las historias (día 3)

Tres fuentes, cruzadas:
1. **Los 9 fallos del bot definitivo** (18 partidas, semilla 59), clasificados partida a partida (subagente con el
   modelo más capaz, citando las respuestas reales).
2. **Lectura de editor de novela negra** de las 9 variantes, con una línea temporal redactada para cada una
   (subagente; las líneas temporales están ahora en `Assets/Scripts/Cases/CaseTimelines.cs`).
3. **El validador narrativo** (`NarrativeValidator`, 20 tests): cada regla se prueba rompiendo a propósito una
   historia válida, y las 9 variantes tienen que pasarlas todas.

Todos los cambios respetan el límite de 460 palabras por ficha (se reescribió, nunca se subió el límite) y los
tests que ya protegían los datos (las anclas detectan el hecho y los ejemplos, no las negaciones, y la propia
ficha de un personaje no dispara sus pistas).

## 1. Los 9 fallos del bot: ¿culpa del bot o del juego?

| Partida | Clase | Por qué |
|---|---|---|
| 1A_2 | bot (+ personaje) | Tenía la pista que descarta a Carmen y la de la llave contra Daniel; acusó a Carmen por una frase que ella se inventó. |
| 1B_2 | bot (+ diseño) | Tenía 1B_luz y a Carmen contradiciéndose; acusó a Rosario. |
| 2B_1 | bot (+ detección) | Marcos dijo dos veces que Andrés lavó la furgoneta, pero la pista no se detectó ("vino a **lavarla**"). |
| 2B_2 | bot | Hizo casi 30 veces la misma pregunta. 0 pistas. |
| 2C_1 | **juego** | Dos pistas dichas y no detectadas ("grabado algo **importante**", "el inspector Ruiz **con su coche**"). |
| 3B_1 | bot (+ detección, diseño) | Tenía la pista decisiva; la mentira de Lucía ("**estaba** en Granada") no se detectaba y nada unía el coche rojo con ella. |
| 3B_2 | bot | Lucía llegó a decir "mi coche rojo"; acusó a Javier. |
| 3C_1 | bot (+ personaje) | Nunca preguntó a Álex qué le contó Paula; Javier negó haber visto nada en el quemadero. |
| 3C_2 | bot | Tenía la pista que descarta a Javier y las huellas de bota de mujer; acusó a Javier. |

**Arreglos del juego** (cada frase real del bot es ahora un ejemplo positivo en los tests; RED → GREEN):

| Dónde | Cambio |
|---|---|
| 2B_manguera | Anclas + "lavarla", "lavar la". |
| 2C_puerto | Anclas + "algo importante", "no se fiaba de la policía". |
| 2C_opel | Anclas + "Ruiz/inspector con/en su coche". |
| Mentira de Lucía (3B) | Anclas + "estaba en Granada", "en Granada todo", "toda la jornada" (tests en `LieDetectionTests`). |
| 1B_luz | Ahora destapa la mentira de la madre (la veían junto a la cama desde las 22:00; ella dice que subió a las 23:10). |
| 3B | Javier sabe que Lucía conduce un Ibiza rojo pequeño: antes nada obtenible unía el coche rojo con ella. |
| 1A_ventana, Maruxa (2A–2C), 3C_despedida, 3C_pisadas | Temas más amplios: los testigos se cerraban ante preguntas naturales ("¿viste algo raro antes de la llamada?", "¿qué te contó Paula?"). |
| Lucas (1B), Maruxa | Su versión contradecía su propia pista ("con los cascos toda la noche", "no miré por la ventana"). |
| Carmen (1A) | "No sabe quién cerró el cuarto": se lo inventó y el bot acusó por eso. |

Lo que es del bot (ignora pistas que descartan, acusa contra su propio razonamiento, repite preguntas) se anota
para mejorar el bot, no el juego.

## 2. Coherencia (línea temporal y fichas)

| Variante | Problema | Arreglo |
|---|---|---|
| 2A | El parte del día 5 dice que nadie ha revisado la cámara, pero Ruiz sabe desde el día 1 que se desenchufó a las 4:55. | Parte: "El informe del inspector local no la menciona." |
| 2A | El epílogo decía que Marcos "volvió a limpiar a las 6:30", la hora a la que él dice que terminó. | "Hacia las 6:00 volvió al bar a 'limpiar' hasta las 6:30." |
| 2B | Marcos se va a casa a las 6:30 pero ve lavar la furgoneta "al volver a casa" a las 7:30. | Versión: "Luego un café y a casa hacia las siete y media." |
| 2C | Andrés (inocente) decía "a las 5:15 entré en la oficina", contra su propia pista de descarte (GPS: 5:15 en la nacional; 5:05 en la comisaría). | Versión: "pasé antes por la comisaría… y llegué a la oficina pasadas las cinco y cuarto." |
| 1A | Lucas sale "cuando papá gritó", pero la pista dice que vio sacar la llave: los gritos llegan después de abrir. | La pista: "papá aporreaba la puerta… y sacó la llave de su bolsillo". |
| 1A | El epílogo decía "Rosario lo vio todo": con las cortinas cerradas no pudo. | "Rosario lo vio cerrar las cortinas." |
| 1B | El parte del día 5 habla de "la cena" de Daniel, que su versión no mencionaba. | Versión: "salí a cenar con clientes". |
| 1B | Epílogo: "Rosario vio la luz toda la noche". | "de 22:00 a 23:15". |
| 1C | Daniel llamaba al 112 y a Carmen en el mismo minuto (23:15). | Carmen: "me llamó pasadas las 23:15". |
| 1C | El móvil del crimen (las pastillas del TDAH) solo estaba en un parte. | Carmen lo sabe: "ella decía que él vendía sus pastillas del TDAH". |
| 3A | El secreto de Álex (fue a una fiesta, bebió y no fue a por Paula) chocaba con su coartada (Granada) y con su edad (17). | "No se lo dijiste a nadie porque estabas bebiendo con tus primos en Granada." |
| 3A | "Encarna lo vio todo". | "vio el humo y la camioneta". |
| 3B | "Encuentra a Paula en Ayamonte… con su prima en Portugal" (Ayamonte está en España). | "en Portugal, al otro lado del puente de Ayamonte". |
| 3B | El epílogo inventaba una hora (19:30) que no estaba en ninguna ficha. | "Poco después quemó…" (la hora sigue en la línea temporal como aproximada). |
| 3B | Javier denuncia el domingo por la noche sin explicar la espera. | "creí que se había ido con su madre". |
| 3C | El conocimiento libre de Lucía ("Paula iba a mudarse contigo a Madrid") destripaba su secreto. | "Paula quería vivir contigo después de la vista." |
| 2B | Nada unía las cartas anónimas con el cartero. | "…en el buzón de su tía **antes de amanecer**" (la hora del reparto). |

## 3. Lectura de editor: ¿es deducible el culpable?

Sí en las 9. Cadena mínima de cada una:

| Variante | Rompe la mentira | Refuerza | Móvil |
|---|---|---|---|
| 1A Daniel | 1A_ventana | 1A_taza, 1A_puerta | 1A_papeles |
| 1B Carmen | 1B_luz / 1B_pared | 1B_receta | 1B_historial, 1B_tutora |
| 1C Lucas | 1C_gritos | 1C_lejia, 1C_pantalla | (ahora) la madre sabe lo de las pastillas |
| 2A Marcos | 2A_aparcamiento | 2A_curva | 2A_copa |
| 2B Andrés | 2B_fichaje | 2B_furgoneta, 2B_manguera | 2B_cartas |
| 2C Ruiz | 2C_comisaria | 2C_opel | 2C_puerto, 2C_prueba |
| 3A Javier | 3A_mensaje | 3A_humo, 3A_garrafas | 3A_audios |
| 3B Lucía | 3B_noche | 3B_coche + el Ibiza (Javier) | 3B_carta, 3B_armario |
| 3C Encarna | 3C_despedida | 3C_llave, 3C_pisadas | 3C_fotos |

Las pistas falsas se resuelven: Carmen en 1A (1A_frasco), Daniel en 1B (1B_cena), el coche de Daniel en 1C
(1C_llamada), Javier en 3B/3C (3B_cuenta, 3C_bar).

## 4. Lo que se deja (y por qué)

- **La vecina se desbloquea el día 1** porque casi todos la mencionan, y en 1A, 1B, 1C y 2C tiene la pista
  decisiva: hay partidas que se pueden resolver pronto. Recortar menciones cambiaría el ritmo de todas las
  variantes a la vez y el bot ya encuentra el juego difícil (acierta 9 de 18). Queda como decisión de diseño
  para Cristian, con este dato.
- **3B, la hora del humo para Encarna** y **3C, la espera de Javier**: la ficha está a 1 palabra del límite.
- **2B: Andrés ficha a las 6:15 y a las 7:30 lava la furgoneta en su casa**, en plena jornada: se puede leer
  como un descanso; no se toca.
- **"Ticket de peaje" en 3B** (en la ruta Jaén–Huelva–Ayamonte no hay peajes): el ancla de la pista es "peaje";
  cambiarlo exigiría recalibrar la pista. Anotado.
