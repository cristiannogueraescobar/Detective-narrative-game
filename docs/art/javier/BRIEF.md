# Encargo de arte de prueba: Javier Romero, 3 expresiones

Paquete de prueba para un solo personaje. Si sale bien, el mismo formato sirve para los otros 11.
Hoy Javier usa un retrato derivado del de Daniel, con la misma cara y el mismo bigote
(`referencias/05-actual-javier-derivado-NO-copiar-cara.png`). Este encargo le da cara propia.

## 1. Quién es (de su ficha, `Assets/Scripts/Cases/Story3HumoYSilencio.cs`)

- **Javier Romero, 44 años, olivarero** de Jaén, dueño de la finca Los Olivares. Lleva tres meses divorciado de Lucía
  y el lunes es la vista de custodia de su hija Paula, que ha desaparecido.
- **Carácter:** amargado y a la defensiva. Pasa de hacerse la víctima a enfadarse y habla mal de su ex («Claro, ahora el
  malo soy yo. Como siempre.»). La vecina dice que bebe mucho desde el divorcio.
- **Cómo se le nota la mentira:** se pone nervioso con la bebida, los audios a su ex, el quemadero y el sábado por la
  noche. Si le acusan, estalla.
- **Importante:** es el culpable en una variante (3A) e inocente en las otras dos (3B y 3C), y las tres usan las
  mismas imágenes. **Ninguna expresión puede delatar la culpa.** Tiene que ser un padre roto y a la defensiva que
  igual oculta algo o igual no.
- Personaje de ficción inspirado en el patrón de un caso real: **ningún parecido con personas reales**.

**Aspecto** (manda esta descripción; `ART-NEEDED.md` tenía dos versiones de la ropa y aquí se elige la camisa de
cuadros, que además lo separa del polo de Daniel):

- Corpulento, cara curtida y morena, mandíbula cuadrada.
- Pelo corto castaño oscuro con canas en las sienes, barba de varios días (**sin el bigote marcado de Daniel**) y ojos
  enrojecidos.
- Camisa de cuadros de franela en oliva y marrón apagados, con las mangas remangadas; pantalón de trabajo marrón con
  polvo y botas de cuero gastadas.
- Una botella de cerveza verde colgando de la mano derecha.

## 2. Las 3 expresiones (datos de unas 1500 respuestas del bot, `ART-NEEDED.md`)

| Archivo | % de respuestas | Qué tiene que transmitir |
|---|---|---|
| `javier_tranquilo.png` | base | Cansado y cerrado, sin hostilidad: un hombre que no ha dormido y espera lo peor. Boca recta, mirada al frente con párpados pesados. Es la imagen por defecto. |
| `javier_triste.png` | **48 %** | Pena por su hija y un poco de autocompasión: cejas alzadas por dentro, mirada baja, ojos rojos y húmedos, hombros caídos y la botella colgando. Que no parezca arrepentimiento. |
| `javier_nervioso.png` | **36 %** (también se usa para enfadado, 9 %, y asustado) | Acorralado y a la defensiva: ceño fruncido, mandíbula apretada, mirada de reojo, una gota de sudor y la mano apretando la botella. Con un punto de rabia, porque sustituye al enfado. |

**Misma pose y mismo encuadre en las tres:** solo cambian la cara, los hombros y cómo sujeta la botella. El juego
cambia de imagen al cambiar el estado, y si la figura se mueve, salta. `Tools/measure_portrait.py` comprueba que
coinciden.

## 3. Guía de estilo (medida sobre los retratos actuales)

| Aspecto | Valor | Cómo se midió |
|---|---|---|
| Formato | **PNG 768×1024 (3:4), fondo transparente**, sin sombra en el suelo | Formato de `Assets/Art/Portraits` |
| Encuadre | Cuerpo entero, de pie, de frente y ligeramente girado (tres cuartos), centrado, pies a un ~4 % del borde inferior. **La figura ocupa el 88-92 % del alto** (unos 900-940 px). | Figuras actuales: del 78 % (vecina) al 92 % del alto, mediana del 90 %. Pies a un 3,5-5,6 % del borde. |
| Contorno | **Negro puro (#000000)**, grueso, de un **1-2 % del alto de la figura** (12-18 px a este tamaño) | Mediana de 11-26 px en figuras de unos 1050 px |
| Píxel | Bloques de píxel grandes y visibles, sin degradados suaves ni antialiasing. Los originales **no** son pixel art estricto: tienen miles de colores y no siguen una rejilla exacta. Hay que pedir el *aspecto* de 16 bits, no una paleta de 16 colores. | 73 000-115 000 colores por imagen, sin rejilla dominante |
| Paleta | Ocres y ámbar cálidos (#F4C868, #DD9D53), óxido y teja (#A23907, #D66621), azul petróleo apagado (#10425F, #2A6581) y marrones (#5C2815, #392412). Piel morena cálida. | Cuantización a 8-10 colores de los originales |
| Luz | Frontal, suave y cálida, con sombreado plano por zonas. **Sin** luz de ambiente dramática, contraluz ni niebla. El juego ya pone el tono noir, la viñeta y el tinte por estado. | Inspección de los originales |
| Fondo | Transparente del todo. Nada de suelo, escenario ni texto. | |

## 4. Prompts (en inglés, para generadores de imagen)

**Método:** genera primero `tranquilo`. Después usa esa imagen como referencia de personaje o como imagen de entrada
(image-to-image, *character reference*, *seed* fija) para `triste` y `nervioso`, cambiando solo la expresión. Sube
también las imágenes de `referencias/01` a `04` como referencia de **estilo**.

**Base común** (va al principio de los tres):

```
16-bit pixel art character sprite, retro adventure game portrait, full body standing, front view slightly turned three-quarters, centered, feet visible near the bottom edge, figure fills 90% of the canvas height, chunky visible pixels, thick solid black outline around the silhouette, flat cel shading, soft warm frontal light, warm ochre, rust and muted teal palette, transparent background, no ground shadow, 3:4 portrait canvas 768x1024.
A 44-year-old Andalusian olive farmer, heavy build, weathered tanned face, square jaw, short dark brown hair greying at the temples, several days of short stubble beard (no thick moustache), red-rimmed tired eyes, muted olive and brown plaid flannel work shirt with rolled-up sleeves, dusty brown work trousers, worn leather work boots, a green beer bottle hanging loosely from his right hand.
```

**`javier_tranquilo`** (base común +):
```
Expression: guarded and exhausted, neutral closed mouth, heavy eyelids, looking straight ahead, shoulders squared, calm but defensive stance.
```

**`javier_triste`** (base común +):
```
Expression: grieving and self-pitying, inner eyebrows raised, eyes downcast, red watery eyes, shoulders slumped forward, the beer bottle hanging low, same pose and framing as the reference image.
```

**`javier_nervioso`** (base común +):
```
Expression: cornered and defensive with a hint of anger, furrowed brow, clenched jaw, eyes glancing sideways, a single sweat drop on the temple, fingers gripping the beer bottle tightly, same pose and framing as the reference image.
```

**Prompt negativo** (los tres):
```
photorealistic, 3d render, smooth gradients, anti-aliasing, blurry, painterly, watercolor, anime, chibi, big head, cropped feet, cut-off head, close-up, portrait crop, background scenery, floor, cast shadow, vignette, dramatic lighting, rim light, text, watermark, signature, frame, border, multiple characters, extra fingers, extra limbs, thick moustache, polo shirt, glasses, blood, weapon, real person likeness
```

En herramientas sin campo negativo, añade al final: «no background, no shadow, no text, no moustache».

## 5. Referencias de estilo (copiadas en `referencias/` para subirlas)

| Archivo | Original | Para qué |
|---|---|---|
| `01-estilo-proporciones-padre.png` | `Assets/Images/Suspects/padre.gif.png` | Proporciones, grosor de contorno y tamaño del píxel. **No la cara.** |
| `02-estilo-hombre-adulto-duenio-bar.png` | `Assets/Images/Suspects/duenio_bar.gif.png` | Hombre adulto con mangas remangadas, nivel de detalle, cómo sujeta un objeto |
| `03-estilo-paleta-historia3-madre.png` | `Assets/Images/Suspects/madre.gif.png` | Paleta cálida. Es la base de Lucía, su ex, de la misma historia. |
| `04-estilo-linea-hermano.png` | `Assets/Images/Suspects/hermano.gif.png` | Calidad de línea y pose natural |
| `05-actual-javier-derivado-NO-copiar-cara.png` | `Assets/Art/Derived/javier.png` | Lo que hay hoy. **Lo que hay que evitar:** la cara y el bigote de Daniel. |

## 6. Cómo integrarlas cuando las tengas

1. **Ajusta el tamaño:** PNG RGBA de **768×1024** con fondo transparente. Si el generador da otro tamaño, escala con
   **vecino más cercano** (nunca bilineal, que emborrona el píxel) hasta que la figura mida unos 920 px de alto, y
   rellena con transparente hasta 768×1024 dejando los pies a ~40 px del borde inferior.
2. **Nombres y carpeta:** `Assets/Art/Portraits/javier_tranquilo.png`, `javier_triste.png` y `javier_nervioso.png`.
   El juego las busca por el `artId` (`javier`), y el catálogo las registra solo al importarlas. Los estados sin
   imagen se sustituyen: enfadado y asustado usan `nervioso`; el resto, `tranquilo`.
3. **Mídelas:**
   ```
   python Tools/measure_portrait.py Assets/Art/Portraits/javier_tranquilo.png Assets/Art/Portraits/javier_triste.png Assets/Art/Portraits/javier_nervioso.png
   ```
   - Avisa si no son 768×1024, si el fondo no es transparente o si hay bordes semitransparentes.
   - Comprueba que **las tres comparten encuadre** (diferencia ≤1 %). Si no, pide de nuevo la que se sale.
   - Imprime los Rect de `figure`, `bust` y `face`. Están ajustados sobre los 7 retratos actuales: error ≤2,6 % en
     5 de 7 y de 5-8 % en los dos que tienen humo o la cabeza descentrada.
4. **Importación en Unity:** como el retrato derivado actual (Default, *Alpha Is Transparency*, filtro bilineal con
   mipmaps, tamaño máximo 1024). Si se ve borroso en el móvil, prueba con el filtro *Point*.
5. **Encuadre (cambio de código pendiente, en su propia rama y con test primero):** hoy el arte nuevo no usa los
   encuadres medidos. Se muestra entero (`PortraitCrops.Full`) en el interrogatorio y en la rueda, y la cara con un
   recorte genérico (`UISprites.FaceCrop`). `PortraitCrops.Figure` solo se aplica al arte antiguo. Hay que:
   - dar a `PortraitCrops` una entrada por `artId` para el arte nuevo (`javier` → `figure`, `bust` y `face` del paso 3);
   - usarla también cuando `PortraitOf` devuelve arte nuevo, que hoy pasa `legacy = false` y cae en `Full`;
   - tests: que `javier` tenga encuadre propio y que la altura de la rueda (`HeightLineup`, 175 cm) use su `figure`.
6. **Comprueba en el juego** (skills `batchmode-capture` y `before-after-gallery`):
   - capturas `AnimationCapture.TodosLosRetratos` y una galería antes/después de la historia 3, con interrogatorio,
     mini-retrato del chat y rueda;
   - `LayoutValidationTests` en verde;
   - en la rueda, que Javier mida 175 cm contra la pared.
7. **Cuando el arte esté integrado:** la entrada de Javier en `Assets/Scripts/Art/DerivedPortraits.cs` ya no se usa
   (`PortraitOf` prefiere el arte nuevo). Se puede quitar en el mismo cambio, con sus tests.
