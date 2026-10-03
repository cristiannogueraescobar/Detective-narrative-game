# Premisas falsas (historia 3) y coherencia de estados (historia 2)

Rama `feature/premisas-y-emociones`, desde `main` (`a74010b`). **Sin fusionar.** Medidas en
`C:\AI\dng-calibracion\premisas-emociones\`. Siempre la rama frente a `main`, con la misma semilla (1919) y 10 intentos.

## Resumen

| Objetivo | `main` | Rama (v3, la que queda) | ¿Cumple? |
|---|---|---|---|
| Premisas falsas, historia 3 ≤ 5 % | 10/120 (8,3 %) | **5/120 (4,2 %)** | **Sí** |
| Coherencia de estados, historia 2 ≥ 95 % | 421/480 (88 %) | **457/480 (95,2 %)** | **Sí, por poco** |
| Sin empeorar lo demás | — | Pistas de la historia 2 y de 3A bajan 6-10 puntos; el bot encuentra un tercio menos de contradicciones del culpable | **No del todo** (apartado 3) |

**Mi recomendación** (apartado 4): quedarse con los arreglos que no cuestan nada (lectura de la etiqueta y premisas) y
atacar la coherencia de la historia 2 personaje a personaje, no con la guía común de las fichas.

## 1. Premisas falsas de la historia 3 (TDD)

La sonda dice a cada personaje que un testigo le vio discutir a gritos con otro el día de los hechos.

| Causa | Arreglo | Test |
|---|---|---|
| Javier (3C) aceptaba una pelea con Lucía ese día: su ficha habla del divorcio y la custodia, y se inventaba el motivo | Su ficha dice que el sábado no vio ni habló con Lucía; la última vez que discutieron fue en el juzgado | `EnLaHistoria3JavierSabeQueElSabadoNoVioALucia` |
| Encarna (3A) contaba el pleito del pozo (verdad) y se inventaba una pelea ese día | Su ficha dice que el sábado no habló con Javier (sin "no le vio": en 3A sí le ve, `3A_garrafas`) | `EnLaHistoria3EncarnaSabeQueElSabadoNoHabloConJavier` |
| El clasificador contaba como aceptadas negaciones reales: «eso no me lo consta» (la fórmula de las reglas de la ficha), «la última vez que discutimos fue en el juzgado», «esa no fue una discusión» | Las reconoce como negación | `NoMeConstaEsNegar`, `DecirQueLaDiscusionFueOtroDiaEsNegar` (con las frases reales) |

Resultado: historia 3, 5/120 (4,2 %) frente a 10/120 (8,3 %) en `main`; en total, 13/360 (3 %).

## 2. Coherencia de estados de la historia 2 (TDD)

| Causa | Arreglo |
|---|---|
| La etiqueta sin corchetes tras el punto («… esa noche.  ESTADO: asustado») no se leía, y **el chat la enseñaba tal cual** (fallo del juego) | El lector la acepta tras el final de una frase (`LaEtiquetaSinCorchetesTrasElPuntoTambienCuenta`) |
| Sin etiqueta al describir a alguien (sobre todo a la víctima) | «Termina SIEMPRE, aunque describas a alguien, con una línea: …» |
| Al describir a la víctima, ya con etiqueta, «tranquilo» | La regla de la víctima va primero: «Si te hablan de Sofía, o la describes: triste, nunca tranquilo» |
| Las acusaciones salían tristes al meter una cláusula delante de «si te acusan» | «Si te acusan: enfadado o asustado», en su propia frase |
| Ruiz (culpable en 2C) «tranquilo» en lo que le pone nervioso: su ficha le pedía calma | Al culpable ya no se le pide «con calma»; «nervioso … aunque disimules» |

**Lo medido** (coherencia de la historia 2, la misma semilla en todas):

| Versión | Qué cambia frente a `main` | Historia 2 | Historias 1 / 3 |
|---|---|---|---|
| `main` | — | 88 % | 93 % / 94 % |
| v3 (**la que queda**) | Todo lo de la tabla de arriba | **95,2 %** | 95 % / 98 % |
| v4 | v3 sin «aunque disimules» | 94,6 % | 93 % / 98 % |
| v5 | v4 con «lo que viste» otra vez entero | 93 % | 94 % / 97 % |
| v6 | Solo la víctima primero (lo demás como en `main`) | 93 % | — |

Solo v3 llega al 95 %. La rama queda en v3 (con un commit nuevo, sin reescribir la historia).

## 3. El precio de v3

**Pistas** (10 intentos, la misma semilla):

| Variante | `main` | Rama v3 | Las que más bajan |
|---|---|---|---|
| 2A | 90 % | 82 % | — |
| 2B | 84 % | **74 %** | `2B_imagenes` (Ruiz) 70 → 40 %, `2B_manguera` (Marcos) 90 → 73 % |
| 2C | 80 % | **74 %** | `2C_prueba` (Ruiz) 77 → 47 % |
| 3A | 95 % | 88 % | `3A_garrafas` (Encarna) 80 → 60 %, `3A_audios` (Lucía) 90 → 70 % |
| Resto | 79-92 % | 80-91 % | — |

Al quitar «aunque disimules» (v4) o devolver «lo que viste» (v5), las pistas de la historia 2 siguen por debajo de
`main` y la coherencia baja del 95 %: los cambios de la guía común de las fichas afectan a todo a la vez.

**Bot** (historias 2 y 3, 4 partidas por variante):

| | Bueno | Agridulce | Insuficiente | Malo | Culpable acertado | Contradicciones del culpable por partida |
|---|---|---|---|---|---|---|
| `main` | 8 | 11 | 1 | 4 | 20/24 | 0,71 |
| Rama v3 | 5 | 11 | 2 | 6 | 18/24 | 0,46 |

- La diferencia en culpables (18 frente a 20) está dentro del ruido del bot.
- Las contradicciones bajan un tercio, en línea con las pistas.
- En 3C el bot acusa a Lucía 3 de 4 veces, con motivos confusos: en dos describe a Encarna (la llave de la cancela,
  Rocío) pero apunta a Lucía como acusada.

## 4. Decisiones para Cristian

1. **¿Qué hacemos con la coherencia de la historia 2?**
   - **A. Fusionar v3:** cumple los dos objetivos, pero cuesta pistas en la historia 2 y en 3A (apartado 3).
   - **B (recomendada): quedarse con lo que no cuesta nada** (lectura de la etiqueta sin corchetes, arreglos de las
     premisas) y atacar la coherencia de la historia 2 personaje a personaje, sin tocar la guía común. Sobre todo Ruiz
     en lo que le pone nervioso y las descripciones de Sofía en la historia 2. Las premisas habría que volver a
     medirlas sin la guía de v3.
   - **C:** aceptar el 88 % de la historia 2 como límite del modelo actual.
2. **La etiqueta sin corchetes** es un fallo del juego (se veía en el chat) y su arreglo no tiene coste: ¿lo separo a
   una rama propia para fusionarlo ya?

Suites al cerrar: EditMode 985/986 (el que no corre es el benchmark, a petición) y PlayMode 68, con 0 fallos.

## Lo que salió mal por el camino

- Paré y relancé la cola de medidas tres veces, porque cada cambio de la guía dejaba viejas las medidas siguientes.
  Solo se pierde tiempo de GPU; todas las medidas citadas son de la versión que dicen.
- El informe del calibrador de estados cortaba la lista de fallos a 25 y no daba cifras por historia (la primera
  lectura me dio un 100 % falso en la historia 2). Ahora da la tabla por historia, por variante y por tipo de pregunta.
- El heredoc volvió a romper una barra en una expresión regular; corregido antes de compilar.
