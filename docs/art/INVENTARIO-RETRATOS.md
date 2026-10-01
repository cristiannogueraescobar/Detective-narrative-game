# Inventario de retratos (sesión C, 01-10-2026)

Estado de `main` (`a05a686`). Hoy **cada personaje tiene una sola imagen**: no hay ningún retrato por expresión
(`Assets/Art/Portraits/` no existe). El estado de ánimo se nota solo por el tinte, el temblor y la etiqueta.
Las expresiones pedidas son las de `ART-NEEDED.md` (tranquilo como base y los dos estados más frecuentes en ~1 500
respuestas del bot).

| Historia | Personaje | Retrato de hoy | ¿Original o derivado? | Deriva de | Expresiones que tiene | Expresiones que faltan | ¿Se nota copiado? | Culpable en |
|---|---|---|---|---|---|---|---|---|
| 1 | Daniel Mendoza (padre) | `Images/Suspects/padre.gif.png` | Original (otro estilo: píxel 9 px, cabeza 1/3,6) | — | 1 (sin expresión) | tranquilo, nervioso, enfadado | — | 1A |
| 1 | Carmen Vidal (madre) | `Images/Suspects/madre.gif.png` | Original | — | 1 | tranquilo, nervioso, triste | — | 1B |
| 1 | Lucas Mendoza (hermano) | `Images/Suspects/hermano.gif.png` | Original | — | 1 | tranquilo, nervioso, triste | — | 1C |
| 1 | Amparo Gil (vecina) | `Art/Derived/amparo.png` | Derivado | la vecina original (canas) | 1 | tranquilo, nervioso, triste | **Sí**: misma cara y pose que Maruxa y Encarna | — |
| 2 | Marcos Rial (dueño del bar) | `Images/Suspects/duenio_bar.gif.png` | Original | — | 1 | tranquilo, nervioso, triste | — | 2A |
| 2 | Andrés Souto (cartero) | `Images/Suspects/cartero.gif.png` | Original | — | 1 | tranquilo, nervioso, triste | — | 2B |
| 2 | Inspector Ruiz | `Images/Suspects/detective.gif.png` | Original | — | 1 | tranquilo, nervioso, enfadado | — | 2C |
| 2 | Maruxa Pena (vecina) | `Art/Derived/maruxa.png` | Derivado | la vecina original | 1 | tranquilo, triste, nervioso | **Sí**: misma cara y pose que Amparo y Encarna | — |
| 3 | Javier Romero (padre) | `Art/Derived/javier.png` | Derivado (espejo) | Daniel | 1 | tranquilo, triste, nervioso | **Sí, el que más**: cara y bigote de Daniel | 3A |
| 3 | Lucía Navarro (madre) | `Art/Derived/lucia.png` | Derivado (espejo, gafas, ropa) | Carmen | 1 | tranquilo, nervioso, triste | Algo: misma cara que Carmen con gafas | 3B |
| 3 | Álex Romero (hermano) | `Art/Derived/alex.png` | Derivado (espejo, ropa) | Lucas | 1 | tranquilo, triste, nervioso | Algo: misma cara y pose que Lucas | — |
| 3 | Encarna Molina (vecina) | `Art/Derived/encarna.png` | Derivado (espejo, luto) | la vecina original | 1 | tranquilo, nervioso, triste | **Sí**: misma cara y pose que Amparo y Maruxa | 3C |

**Notas**
- La vecina original (`Images/Suspects/vecina.gif.png`) no la usa nadie tal cual: es la base de las tres vecinas.
- Dentro de una misma historia nunca coinciden dos caras; el parecido se nota entre historias (Javier/Daniel, las
  tres vecinas, Lucía/Carmen, Álex/Lucas).
- Daniel es el único original de otro estilo (píxel 9 px y cabeza grande); Javier, su derivado, lo hereda.
- Total que falta: 12 personajes × 3 expresiones = **36 imágenes** (ninguna existe hoy).
- **Integración** (para el final): lo que va a `Assets/Art/Portraits/` se trata como arte nuevo (imagen entera, sin el
  tratamiento del arte antiguo). Las expresiones derivadas de los originales deben conservar el encuadre y el
  tratamiento de su base, como hoy los derivados: hace falta ampliar `DerivedPortraits` por estado (cambio de código,
  con tests).
