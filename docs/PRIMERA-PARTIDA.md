# Primera partida de un jugador nuevo (sesión C, 01-10-2026)

Mapa del recorrido (skill **journey-mapping**, *customer journey map*: una persona, un escenario) de alguien que abre
el juego por primera vez hasta terminar su primer caso. **Solo análisis: no se ha cambiado nada.**

**Fuentes.** Capturas reales del juego (`docs/screenshots/2026-09-30/galeria/`: 01 menú, 18 tutorial, 02 casos,
03 expediente, 04 interrogatorio, 16 pista, 17 contradicción, 05 y 24 libreta, 06 acusación, 07-09 finales). Datos del
bot detective (qwen con la ficha del caso y la libreta, lo mismo que ve un jugador; 18 partidas, las 9 variantes,
semilla 1919, 01-10 22:37): culpable acertado 17/18, **primera pista en la pregunta 6,1**, **88 % de las preguntas sin
pista nueva**, **2-4 de los 7 días sin ninguna pista** por partida, y la primera pista **el día 1 solo en 6 de 18
partidas** (día 2 en 4, día 3 en 8). Código: 5 preguntas al día en dificultad normal; el aviso «Aún no tienes ninguna
pista…» sale **a partir del día 3**; «Pensar» cuesta una pregunta. El bot es un jugador torpe pero constante: los
números son una cota, no la media de las personas.

**Persona.** Le gustan los misterios y los juegos de móvil cortos; no ha jugado nunca a un juego en el que se escribe
la pregunta. Escenario: primera vez, caso elegido al azar, dificultad normal.

## El recorrido

| Fase | Qué hace | Qué piensa | Qué siente | Dónde se puede perder o aburrir |
|---|---|---|---|---|
| 1. Abrir (01, 18) | Toca «Jugar»; lee el tutorial («Elige a quién interrogar y escribe tu pregunta…») | «¿Tengo que escribir yo?» | Curioso | El menú es atractivo; el tutorial es corto y claro |
| 2. Elegir caso (02) | Ve tres expedientes; toca uno o «Caso al azar» | «¿Cuál es el fácil?» | Intrigado | Nada indica por dónde empezar (las tres historias parecen iguales de difíciles) |
| 3. Expediente (03) | Lee víctima, lugar y situación; toca «Empezar» | «¿Quién lo hizo?» | Enganchado (momento de la verdad 1) | Bien: corto, con tono |
| 4. Día 1 (04) | Elige sospechoso, toca una pregunta de ejemplo o escribe | «¿Qué pregunto para que suelte algo?» | Expectante → desconcertado | **Cinco preguntas y, a menudo, ninguna pista** (fricción 1). Respuestas plausibles pero que no avanzan |
| 5. Días 2-6 (16, 17, 24, 05) | Lee el parte de la mañana, pregunta, enseña pruebas, apunta en la libreta | «¿Esto es importante? ¿A quién le pregunto ahora?» | Picos al pillar una pista o una mentira; valles largos entre medias | **Días enteros sin pistas** (2-4 de 7); no sabe que hay que insistir o enseñar una prueba para que confiesen; el parte da una palanca que no sabe usar |
| 6. Acusar (06) | Elige culpable y prueba clave en la rueda | «¿Tengo bastante?» | Tenso (momento de la verdad 2) | Una sola oportunidad: bien para la tensión; el resumen de la libreta ayuda |
| 7. Final (07-09) | Lee el veredicto, «lo que pasó de verdad» y su rango | «¡Era eso!» o «¿cómo no lo vi?» | Satisfecho o picado (fin del recorrido: lo que recordará) | Bien: el informe cierra la historia; invita a otra partida |

**Curva emocional:** sube en el expediente, **cae el día 1** (sin pistas), oscila en los días 2-6 (picos con cada
pista o contradicción, valles largos sin nada) y termina alta en el veredicto. El final es el punto más fuerte; el
primer día, el más débil, y es justo donde se decide si sigue jugando.

## Los 5 puntos de fricción más importantes, con una propuesta para cada uno

1. **El primer día suele acabar sin ninguna pista.** Primera pista en la pregunta 6,1 con 5 preguntas al día; en 12 de
   18 partidas del bot, el día 1 termina con la libreta vacía, y el aviso de atasco no sale hasta el día 3.
   **Propuesta:** asegurar un hallazgo el día 1. La más barata: que una de las preguntas de ejemplo del día 1 lleve a
   una pista abierta de un inocente (son las que salen al 90-100 %), y adelantar el aviso de atasco al final del día 1
   si la libreta está vacía.
2. **La pregunta en blanco.** El 88 % de las preguntas no da nada nuevo. El jugador nuevo no sabe qué funciona
   (horas, lugares, objetos concretos) más allá de una línea del tutorial.
   **Propuesta:** preguntas de ejemplo que cambien con lo que ya se sabe (el sospechoso que alguien acaba de nombrar,
   el objeto del último parte), y tocar un nombre o un objeto en una respuesta para preguntar por él.
3. **Días enteros sin avances** (2-4 de 7 por partida): valles largos en mitad de la partida; el parte de la mañana da
   una palanca («el restaurante no tiene su reserva») que el jugador no siempre sabe convertir en pregunta.
   **Propuesta:** que el parte de la mañana lleve una pregunta sugerida tocable para el sospechoso al que afecta.
4. **No sabe que hay que insistir o enseñar una prueba para que confiesen.** Las pistas secretas dependen de eso (la
   coartada de Daniel en 1B sale un 15-45 % de las veces incluso preguntando bien). El tutorial explica «Mostrar
   prueba», pero no relaciona el nerviosismo con insistir.
   **Propuesta:** la primera vez que un sospechoso se pone nervioso, una indicación corta: «Se ha puesto nervioso:
   insiste o enséñale algo de tu libreta».
5. **La cara de algunos dice otra cosa que su estado.** En la prueba de emoción de esta sesión, el retrato tranquilo
   de Marcos, Maruxa y Encarna se lee como «enfadado» (3 de 3 subagentes): el jugador ve la etiqueta «tranquilo» y una
   cara de enfado, y el estado de ánimo es una pista para saber cuándo insistir.
   **Propuesta:** las expresiones nuevas (rama `feature/retratos-completos`) ya ayudan en triste y nervioso; la cara
   base de esos tres, redibujada (está en `docs/art/ENCARGO.md` para las vecinas; añadir a Marcos).

**Momentos de la verdad:** el expediente (engancha), el final del día 1 (decide si sigue) y el veredicto (lo que
recordará). El primero y el último funcionan; el del día 1 es la prioridad.
