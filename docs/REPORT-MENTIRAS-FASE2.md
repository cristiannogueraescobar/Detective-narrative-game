# Mentiras de inocentes: decisiones de la fase 1 y fase 2

Rama `feature/mentiras-inocentes`. **Sin fusionar.** Fase 1: `docs/REPORT-MENTIRAS-FASE1.md`.

Todas las medidas están en `C:\AI\dng-calibracion\` (`fase1b`, `fase2`, `remedir`). Siempre la rama frente a `main`,
con la misma semilla del modelo (1919) y la misma N.

## Resumen

| Variante | Mentiroso inocente | Pistas (rama / `main`) | Premisas (historia) | Estados (historia) | Bot: culpable (rama / `main`) | Acusa al inocente que miente | ¿Cumple? |
|---|---|---|---|---|---|---|---|
| 1A | — (Carmen solo tiene el zolpidem) | — | — | — | 4/4 / 2/4 (\*) | — | Sin cambios |
| 1B | Daniel, la cena | 81 % / 81 % | 1 % | 98 % / 96 % | 4/4 / 4/4 (\*) | 0/4 | **Sí** |
| 1C | Daniel, la hora | 85 % / 85 % | (igual) | (igual) | 4/4 / 3/4 (\*) | 0/4 | **Sí** |
| 2A | Andrés, las postales | 89 % / 89 % | 1 % | 97 % / 90 % (\*\*) | 4/4 / 4/4 | 0/4 | **Sí** |
| 2B | Maruxa, el orujo | 83 % / 81 % | | | 1/4 / 0/4 | 2/4 (\*\*\*) | **Sí** (pistas); el bot falla igual que en `main` |
| 2C | Andrés, las postales | 82 % / 81 % | | | 2/4 / 3/4 | 0/4 | **Sí** |
| 3A | Lucía, Madrid | 94 % | 9 % (`main` 8 %) (\*\*) | 97 % / 95 % | 4/4 / 4/4 | 0/4 | **Sí** |
| 3B | Javier, lo que bebió | 84 % / 84 % | | | **2/4 / 3/4** | **2/4** | **No**: el bot le acusa por su mentira (apartado 3) |
| 3C | Álex, la fiesta | 95 % | | | 3/4 / 4/4 | 0/4 | **Sí** |

- (\*) Bot de la fase 1, con la regla nueva y antes de los datos de la fase 2.
- (\*\*) **Huecos que ya había en `main`**, con las mismas respuestas: la coherencia de los estados de la historia 2 (90 %
  en las dos versiones) y las premisas falsas de la historia 3 (8 % en `main`, 9 % en la rama; la diferencia es una
  respuesta de Lucía). Las mentiras no los causan, pero no llegan al umbral.
- (\*\*\*) En 2B, `main` también acusa a Maruxa (2 de 4): la confunde con la culpable porque es la testigo que vio la
  furgoneta. Su contradicción no salió en ninguna de esas partidas.

**Total del bot en la fase 2** (historias 2 y 3, 24 partidas): culpable acertado 16/24 en la rama frente a 20/24 en
`main`. Acusa al inocente que miente 4/24: 2 en 2B (también pasa en `main`) y 2 en 3B (por su mentira).

## 1. Las tres decisiones sobre la fase 1

### 1.1 La mentira cuenta en cuanto la versión está en la libreta (hecho, TDD)

- `HeardVersions.Heard` decide si la versión de alguien está en la libreta: ha contestado algo y no nombra a nadie que
  el jugador aún no conoce. La libreta usa esa misma función.
- `HeardVersions.RegisterLies` da por dicha la mentira de quien tenga su versión en la libreta. Lo llaman el juego,
  antes de rehacer la libreta (tras cada respuesta, pista, desbloqueo o cambio de día), y el bot, así que no pueden
  divergir.
- Tests:
  - EditMode: `LaVersionEstaEnLaLibreta…`, `LaMentiraDelInocenteCuenta…`, `YLaDelCulpableIgual` y
    `QuienNoMienteNoRegistraNada`.
  - PlayMode, jugando 1B de verdad: Daniel no dice su mentira, Amparo cuenta lo de la cena y la contradicción sale en
    la libreta (`LaMentiraDeDanielCuentaAunqueNoLaDigaEnElChat`).
- Consecuencia: cada inocente que miente lleva la mentira en su versión, porque si no, la libreta enseñaría una
  contradicción sobre algo que su "Dice:" no dice. Lo obliga un test.

**Bot en las 9 variantes, 4 partidas por variante**, rama (con la regla, antes de la fase 2) frente a `main`:

| | BUENO | AGRIDULCE | INSUFICIENTE | MALO | Culpable acertado | Contr. del culpable por partida | Contr. de inocentes por partida | Acusa al inocente que miente |
|---|---|---|---|---|---|---|---|---|
| `main` | 8 | 17 | 2 | 9 | 27/36 | 0,39 | — | — |
| Rama | **18** | 12 | 2 | 4 | **32/36** | 0,56 | 0,08 | 0/36 |

- Los finales buenos se duplican (8 → 18), como esperábamos: ya no hay que pillar al culpable diciendo su mentira.
- En 1B la contradicción de Daniel no salió en ninguna partida, porque `1B_cena` no apareció en ninguna de las 4. Es
  cuestión de lo difícil que es esa pista, no de la regla. En 1C salió en las 3 partidas en que apareció `1C_llamada`.

**Propuesta de umbral (no aplicada).** La evidencia de cada partida (pistas incriminatorias + 2 × contradicciones del
culpable), reconstruida de los registros, coincide con el final que dio el bot en 36 de 36 partidas en las dos
versiones:

| Umbral (bueno / agridulce) | Rama: B / A / I / M | `main`: B / A / I / M |
|---|---|---|
| 5 / 3 (hoy) | 18 / 12 / 2 / 4 | 8 / 17 / 2 / 9 |
| **6 / 3 (propuesta)** | **10 / 20 / 2 / 4** | 5 / 20 / 2 / 9 |
| 6 / 4 | 10 / 8 / 14 / 4 | 5 / 9 / 13 / 9 |
| 7 / 4 | 1 / 17 / 14 / 4 | 2 / 12 / 13 / 9 |

- Propongo **final bueno con 6 puntos** (antes 5) y agridulce con 3, como hoy.
- Con eso, los finales buenos entre las acusaciones acertadas quedan como hoy en `main`: 31 % (10 de 32) frente a 30 %
  (8 de 27).
- Antes de aplicarlo hay que comprobar que cada variante puede llegar a 6 sin que el culpable cuente nada
  (`FinalBuenoAlcanzableSinElCulpable`).

### 1.2 Estados emocionales con 15 intentos (cumple)

Variantes 1B y 1C:

| | Bien formada | Coherente |
|---|---|---|
| Rama | 470/480 (98 %) | 459/480 (96 %) |
| `main` | 462/480 (96 %) | 455/480 (95 %) |

La bajada de la fase 1 era ruido.

### 1.3 `1B_receta` al 80 % (cumple, por Lucas)

- **Lucas como segundo portador.** Ya lo sabía como dato suelto en su ficha; ahora es la pista, desde su punto de
  vista, y solo sobre las medicinas (hablar mal de su madre le pone nervioso).
- **Daniel:** su hecho responde a «¿quién decidía los tratamientos?».
- **La reacción de Daniel** a la prueba remite a su secreto («cuenta lo que ocultas, con sus detalles»). Antes era una
  frase hecha que repetía tal cual, y sus pistas no salían.

| Vista | Rama | `main` |
|---|---|---|
| `1B_receta` por Lucas | **90 %** | — |
| `1B_receta` por Daniel | 35 % | 75 % |
| `1B_cena` por Daniel | 70 % | 35 % |
| Media de 1B | 81 % | 81 % |

Ninguna otra pista de 1B baja. Daniel cuenta peor la receta y mejor la cena; la pista de la receta llega al jugador por
Lucas.

## 2. Fase 2, historia por historia

Datos con TDD (test rojo antes de los datos en cada historia).

| Variante | Quién miente | Su mentira, en su versión | Pista nueva que la rompe (la cuenta) |
|---|---|---|---|
| 2A | Andrés | «en quince años no se me ha quedado ni una carta» (secreto: guarda postales) | `2A_postales` «La postal que no llegó» (Maruxa) |
| 2B | Maruxa | «el humo es de la cocina de leña» (secreto: destila orujo) | `2B_orujo` «El orujo de la curva» (Ruiz) |
| 2C | Andrés | la misma | `2C_postales` (Maruxa); **7 pistas**, resoluble en 35 preguntas (`ResolvabilityTests`) |
| 3A | Lucía | «mi vida está en Granada; no pienso irme» (secreto: Madrid) | `3A_madrid` «Pisos en Madrid» (Álex) |
| 3B | Javier | «un par de cañas, nada más» (secreto: bebió mucho) | `3B_tumbos` «Un regreso dando tumbos» (Encarna) |
| 3C | Álex | «estuve en Granada con mi madre» (ya era su versión; secreto: de fiesta) | `3C_fiesta` «Una noche de fiesta» (Lucía) |

Todas las pistas nuevas son de contexto (`Context`): rompen la mentira y no incriminan a nadie.

Arreglos tras la primera medida:

- `2B_orujo` (60 % → 85 %): a «¿qué sabe de Maruxa?», Ruiz contaba lo que ya decía su ficha de ella, y el detector no
  veía «el medio pueblo compra su orujo». Ahora el hecho empieza por Maruxa y hay anclas con sus frases reales.
- `3B_tumbos` (65 % → 85 %): Encarna lo decía («muy mareado», «tambaleante», «oloroso a alcohol») y las anclas no lo
  veían.

**Historias.**

- **Historia 1:** 1A, sin mentiroso inocente; 1B y 1C cumplen.
- **Historia 2:** cumplen las tres en pistas, premisas y bot frente a `main`. Los estados quedan en 90 % de coherencia,
  como en `main`.
- **Historia 3:** cumplen 3A y 3C; **3B no**. Las premisas quedan en 9 % (`main` 8 %).

## 3. Lo que no cumple: 3B

- En las 2 partidas en que el bot acusó mal, acusó a Javier, y en las dos tenía su contradicción («un par de cañas»
  frente a «un regreso dando tumbos»). En una dice: «por su regreso borracho…».
- La causa es de diseño, no de calibración. Javier miente sobre la misma noche y el mismo sitio del crimen: volvió a la
  finca bebido a las 21:30 y no se encendió la luz de la niña.
- La regla del documento para distinguir mentiras era «otro tema, otra hora u otro sitio». Esta mentira no la cumple.

**Propuesta (no aplicada, porque cambia el inocente que aprobaste):** en 3B, que miente Encarna sobre su pleito con
Javier por el agua del pozo (un secreto que ya tiene). Lo contaría Javier: «Encarna y tú lleváis un pleito por el pozo;
te tiene rencor». Es otro tema, otra hora y otro sitio.

## 4. Herramientas de medida (y un error mío)

- **Semillas.** `-seed` en los calibradores. La primera versión daba a cada llamada «semilla + número de orden», y una
  pista nueva desplazaba todas las semillas de detrás. En la primera medida de la historia 2 las diferencias pista a
  pista eran ruido por eso. Ahora la semilla sale de quién contesta, de la conversación y de cuántas veces se ha hecho
  ya. Las re-medidas (`remedir`) son las buenas. Las comparaciones anteriores de estados (fase 1) eran válidas: las dos
  versiones hacían exactamente las mismas llamadas.
- `ClueCalibrator` con `-clues` detalla todas las pistas pedidas.
- El bot registra cada contradicción con su texto y si acusó a un inocente que mentía.
- En el worktree de `main`, el calibrador lleva la misma versión de la herramienta, sin commit. No cambia nada del juego.
- TDD: las herramientas y el test que juega 1B los escribí sin verlos antes en rojo. El motor y los datos, sí.
- El heredoc volvió a romper un `\n` (dos veces); corregido antes de compilar.

## 5. Decisiones para Cristian

1. **Umbral del final bueno:** ¿6 puntos (apartado 1.1)?
2. **3B:** ¿cambiamos a Encarna y el pleito del pozo (apartado 3), o quitamos el mentiroso de 3B?
3. **Huecos que ya había en `main`:** la coherencia de estados de la historia 2 (90 %) y las premisas falsas de la
   historia 3 (8-9 %, sobre todo Javier en 3C). ¿Los tratamos aparte?
4. **`1B_receta` por Daniel** bajó a 35 % (la pista llega por Lucas, al 90 %). ¿Lo dejamos así o buscamos que Daniel
   también la cuente?
5. **1B: `1B_cena`** casi nunca sale en las partidas del bot (Amparo la cuenta al 85-95 %, pero el bot apenas le
   pregunta por Daniel). ¿Una pista de dirección (que alguien mencione que Amparo lo ve todo de Daniel)?

Suites al cerrar: EditMode 966/967 (el que no corre es el benchmark, a petición) y PlayMode 68, con 0 fallos.

## 6. Decisiones de Cristian sobre la fase 2 (03-10-2026)

| Decisión | Resultado | Medida |
|---|---|---|
| 1. Final bueno a 6 puntos, si las 9 variantes llegan sin confesión | **Hecho.** `FinalBuenoAlcanzableSinElCulpable` y la prueba de resolubilidad (35 preguntas) pasan con 6 | Máximo sin confesión: 1A 6, 1B 9, 1C 6, 2A 6, 2B 6, 2C 7, 3A 6, 3B 8, 3C 6. **Seis variantes llegan justo a 6:** el final bueno exige todas las pistas incriminatorias y la contradicción del culpable |
| 2. 3B: miente Encarna (el pleito del pozo); Javier ya no miente | **Cumple** | Pistas 87 % (`main` 84 %; `3B_pozo` 95 %, tras ampliar las anclas a cómo lo cuenta Javier). Premisas 0 % (`main` 5 %). Estados 99/97 % (`main` 99/98 %). Bot 4/4 dos veces (`main` 3/4), sin acusar a Encarna |
| 3. `1B_receta` por Daniel se queda así | — | |
| 4. "Pensar" sugiere preguntar a Amparo por la familia si aún nadie lo ha hecho (TDD) | **Cumple** | Bot de 1B, 8 partidas antes y después con la misma semilla: `1B_cena` sale en 0/8 → **3/8**; preguntas a Amparo sobre la familia 1 → 11; contradicción de Daniel 0 → 3 partidas; culpable 8/8 en las dos, sin acusar a Daniel |

Los datos de 3B en el apartado 2 (Javier, `3B_tumbos`) quedan sustituidos por esto.
