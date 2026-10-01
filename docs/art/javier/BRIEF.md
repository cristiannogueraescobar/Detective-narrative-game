# Encargo de arte de prueba: Javier Romero, 3 expresiones

Paquete de prueba para un solo personaje. Si sale bien, el mismo formato sirve para los otros 11 (y para rehacer a
Daniel; ver `ART-NEEDED.md`). Hoy Javier usa un retrato derivado del de Daniel, con la misma cara y el mismo bigote
(`referencias/05-actual-javier-derivado-NO-copiar-cara.png`). Este encargo le da cara propia.

## 1. Quién es (de su ficha, `Assets/Scripts/Cases/Story3HumoYSilencio.cs`)

- **Javier Romero, 44 años, olivarero** de Jaén, dueño de la finca Los Olivares. Lleva tres meses divorciado de Lucía
  y el lunes es la vista de custodia de su hija Paula, que ha desaparecido.
- **Carácter:** amargado y a la defensiva. Pasa de hacerse la víctima a enfadarse y habla mal de su ex («Claro, ahora el
  malo soy yo. Como siempre.»). La vecina dice que bebe mucho desde el divorcio.
- **Cómo se le nota la mentira:** se pone nervioso con la bebida, los audios a su ex, el quemadero y el sábado por la
  noche. Si le acusan, estalla.
- **Importante:** es el culpable en una variante (3A) e inocente en las otras dos (3B y 3C), y las tres usan las
  mismas imágenes. **Ninguna expresión puede delatar la culpa.** Tiene que ser un padre roto y a la defensiva que igual
  oculta algo o igual no.
- Personaje de ficción inspirado en el patrón de un caso real: **ningún parecido con personas reales**.

**Aspecto** (manda esta descripción; `ART-NEEDED.md` tenía dos versiones de la ropa y aquí se elige la camisa de
cuadros, que además lo separa del polo de Daniel):

- Corpulento, cara curtida y morena, mandíbula cuadrada.
- Pelo corto castaño oscuro con canas en las sienes, barba de varios días (**sin bigote**) y ojos enrojecidos.
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
cambia de imagen al cambiar el estado, y si la figura se mueve, salta.

## 3. Guía de estilo (medida sobre las referencias 02-04)

**Qué se midió.** Las 7 imágenes originales no tienen un solo estilo, y por eso el estilo se toma solo de 02-04.
Daniel (`padre.gif.png`) y el Javier actual (derivado de él) son otro dibujo: más infantil y con el píxel más grande.

| | Daniel (01) | **Marcos (02)** | **Lucía (03)** | **Álex (04)** |
|---|---|---|---|---|
| Píxel efectivo | **9 px** | 5 px | 6 px | 5 px |
| Píxeles de arte de alto | **118** | 214 | 189 | 227 |
| Cabeza / cuerpo (coronilla a barbilla) | **0,28 (≈1/3,6)** | 0,19 (≈1/5,3) | 0,24 (≈1/4,2) | 0,19 (≈1/5,3) |
| Sombra bajo los pies | **no** | sí | sí | sí |

- **Píxel efectivo:** mediana de los tramos de color casi igual dentro de la figura.
- **Cabeza:** leída en recortes con regla, después de que dos métodos automáticos fallaran en 5 de 7 imágenes.
- **Las demás imágenes originales:** el cartero (3 px, 0,22), la vecina (5 px, 0,24) y el detective (5 px, 0,22)
  también encajan con 02-04.
- **Lucía (03)** está entre los dos estilos (cabeza 1/4,2), por eso solo se usa para la paleta.

**Guía:**

| Aspecto | Valor (sobre 02-04) |
|---|---|
| Formato final | **PNG 768×1024 (3:4)** con fondo transparente. El generador entrega **fondo blanco liso** y `Tools/remove_white_bg.py` lo convierte. |
| Encuadre | Cuerpo entero, de pie, de frente y ligeramente girado (tres cuartos), centrado. **La figura ocupa el 89-92 % del alto** (unos 920 px) y los pies quedan a un 3,5-5,6 % del borde (unos 40 px). |
| Proporciones | **Adultas realistas: la cabeza mide ≈1/5 de la figura** (0,19, como Marcos y Álex). Nada de cabeza grande. |
| Píxel | Bloques de píxel medianos y visibles, de **4-5 px a este tamaño** (≈200-230 píxeles de arte de alto), sin degradados suaves. No es pixel art estricto: los originales tienen decenas de miles de colores y no siguen una rejilla exacta. |
| Contorno | **Negro puro (#000000)**, grueso: un 1-2,4 % del alto de la figura (unos 10-22 px). |
| Paleta | Piel cálida (#D29F61, #DD9D53, #F3CE95), luces crema (#EFE0B2, #FCEDA8), óxido y teja (#A44927, #B93314, #7A1A16), marrones (#3E1411, #5C2815, #37322D) y azul apagado (#182E43, #2A6581). Para Javier pesan más el oliva y el marrón (camisa) y menos el azul. |
| Luz | Frontal, cálida y suave, que viene un poco **desde la izquierda**: la sombra cae a la derecha de los zapatos. Sombreado plano por zonas. **Sin** contraluz, niebla ni luz dramática: el juego ya pone el tono noir, la viñeta y el tinte por estado. |
| Sombra | **Sí, suave bajo los pies.** Gris, semitransparente (opacidad media de ≈18 %, 46-50/255), a la derecha de cada zapato y a la altura de la suela, un 3-12 % más ancha que los zapatos. Sin ella, Javier desentonaría en la rueda junto a los demás. |
| Fondo | Al generar, **blanco liso (#FFFFFF)**: nada de suelo, escenario, texto ni el patrón de cuadros de la transparencia. |

## 4. Prompts (en inglés, para generadores de imagen)

### Método: generar una vez y editar, no regenerar

Regenerar la figura entera, aunque sea con la misma semilla, cambia la camisa, las manos o la postura, y la figura
saltaría al cambiar de estado.

1. **Genera solo `javier_tranquilo`.** Prompt base más su expresión, y sube como referencias de estilo
   `referencias/02` (la principal) y `referencias/04`.
2. **Compáralo** junto a Marcos (02) y Lucía (03): proporciones, tamaño de píxel, contorno, paleta y sombra. Si no
   encaja, repite este paso. Todavía no hay nada que editar.
3. **Si encaja, crea `triste` y `nervioso` EDITANDO `tranquilo`.** Usa una herramienta que retoque solo una zona
   (*inpainting*, edición por región o edición con instrucciones): selecciona la cara y los hombros y usa los prompts
   de edición de abajo. Lo demás no se toca.
4. **Control:** pasa las tres por `Tools/remove_white_bg.py` y `Tools/measure_portrait.py` (sección 6). Si la
   diferencia de encuadre entre estados pasa del **1 %**, **rehaz la edición**. No regeneres.

### Prompt base (paso 1)

```
Pixel art character sprite in the exact style of the reference image: retro 16-bit adventure game look, medium-sized visible pixels, realistic adult proportions (not chibi, normal head size), thick solid black outline, flat cel shading, soft warm frontal light, warm ochre, rust, olive and brown palette. Full body standing, front view slightly turned three-quarters, centered, feet visible near the bottom edge, figure fills 90% of the canvas height, subtle soft shadow under the feet, plain flat white background, 3:4 portrait canvas.
A 44-year-old Andalusian olive farmer, heavy build, weathered tanned face, square jaw, short dark brown hair greying at the temples, several days of short stubble beard (no moustache), red-rimmed tired eyes, muted olive and brown plaid flannel work shirt with rolled-up sleeves, dusty brown work trousers, worn leather work boots, a green beer bottle hanging loosely from his right hand.
```

### Expresiones (base +)

**`javier_tranquilo`:**
```
Expression: guarded and exhausted, neutral closed mouth, heavy eyelids, looking straight ahead, shoulders squared, calm but defensive stance.
```

**`javier_triste`:**
```
Expression: grieving and self-pitying, inner eyebrows raised, eyes downcast, red watery eyes, shoulders slumped forward, the beer bottle hanging low, same pose and framing as the reference image.
```

**`javier_nervioso`:**
```
Expression: cornered and defensive with a hint of anger, furrowed brow, clenched jaw, eyes glancing sideways, a single sweat drop on the temple, fingers gripping the beer bottle tightly, same pose and framing as the reference image.
```

### Prompts de edición (paso 3: solo la cara y los hombros de `tranquilo`)

**`javier_triste`:**
```
Change only the facial expression and shoulders, keep everything else identical (same pixels, outline, clothes, hands, bottle, pose, framing, shadow and white background): grieving and self-pitying, inner eyebrows raised, eyes downcast, red watery eyes, shoulders slightly slumped.
```

**`javier_nervioso`:**
```
Change only the facial expression and shoulders, keep everything else identical (same pixels, outline, clothes, hands, bottle, pose, framing, shadow and white background): cornered and defensive with a hint of anger, furrowed brow, clenched jaw, eyes glancing sideways, a single sweat drop on the temple, shoulders tense.
```

### Prompt negativo (generación)

```
photorealistic, 3d render, smooth gradients, anti-aliasing, blurry, painterly, watercolor, anime, chibi, big head, oversized head, cropped feet, cut-off head, close-up, portrait crop, background scenery, vignette, dramatic lighting, rim light, text, watermark, signature, frame, border, checkerboard pattern, transparency grid, multiple characters, extra fingers, extra limbs, moustache, polo shirt, glasses, blood, weapon, real person likeness
```

En herramientas sin campo negativo, añade al final: «no moustache, no big head, no checkerboard, plain white
background».

## 5. Referencias (copiadas en `referencias/` para subirlas)

| Archivo | Original | Para qué |
|---|---|---|
| `02-estilo-hombre-adulto-duenio-bar.png` | `Assets/Images/Suspects/duenio_bar.gif.png` | **Referencia de estilo principal:** hombre adulto y corpulento, mangas remangadas, objeto en la mano, proporciones (cabeza ≈1/5), píxel, contorno y sombra |
| `04-estilo-linea-hermano.png` | `Assets/Images/Suspects/hermano.gif.png` | **Referencia de estilo secundaria:** calidad de línea, pose natural, el mismo píxel y la misma cabeza |
| `03-estilo-paleta-historia3-madre.png` | `Assets/Images/Suspects/madre.gif.png` | **Solo la paleta** (es la base de Lucía, su ex, en la misma historia). Su cabeza es algo mayor (1/4,2): no la uses para las proporciones. |
| `05-actual-javier-derivado-NO-copiar-cara.png` | `Assets/Art/Derived/javier.png` | **Lo que hay que evitar:** la cara y el bigote de Daniel, el píxel grande (9 px) y la cabeza enorme (≈1/3,6) |

`padre.gif.png` (Daniel) ya no es referencia: es de otro estilo, y en `ART-NEEDED.md` está anotado que hay que
rehacerlo.

## 6. Cómo integrarlas cuando las tengas

1. **Guárdalas tal cual salen del generador**, con fondo blanco y a cualquier tamaño, por ejemplo
   `javier_tranquilo_gen.png`.
2. **Quita el fondo y deja el tamaño final:**
   ```
   python Tools/remove_white_bg.py javier_tranquilo_gen.png Assets/Art/Portraits/javier_tranquilo.png
   python Tools/remove_white_bg.py javier_triste_gen.png Assets/Art/Portraits/javier_triste.png
   python Tools/remove_white_bg.py javier_nervioso_gen.png Assets/Art/Portraits/javier_nervioso.png
   ```
   - Pasa a transparente solo el blanco unido al borde, rellenando desde las esquinas, y el blanco puro y plano
     encerrado entre brazo y cuerpo. Una camisa o una botella con brillos sigue opaca.
   - **Conserva la sombra** junto a los pies como gris semitransparente.
   - Escala por **vecino más cercano** hasta que la figura ocupe el 90 % de 1024 px, con los pies a 40 px del borde y
     centrada en 768×1024.
   - **Avisa** si el generador pintó el patrón de cuadros o si hay un halo claro alrededor de la figura (bordes
     antialiasados de más que en los originales).
   - Validado con 10 tests y con las 7 originales sobre blanco: misma área opaca ±1,7 % y sombra conservada.
3. **Mídelas:**
   ```
   python Tools/measure_portrait.py Assets/Art/Portraits/javier_tranquilo.png Assets/Art/Portraits/javier_triste.png Assets/Art/Portraits/javier_nervioso.png
   ```
   - Comprueba el tamaño, el fondo transparente y que **las tres comparten encuadre** (diferencia ≤1 %). Si no,
     rehaz la edición (sección 4, paso 4).
   - Imprime los Rect de `figure`, `bust` y `face`, ajustados sobre los 7 retratos actuales (error ≤2,6 % en 5 de 7).
4. **Importación en Unity: filtro bilineal con mipmaps** (Default, *Alpha Is Transparency*, tamaño máximo 1024). Es lo
   que llevan los retratos de hoy y **se decidió con capturas** a píxeles reales de móvil (1080×1920,
   `AnimationCapture.FiltroDeImportacion`):
   - En el busto del interrogatorio, que se ve casi a escala 1:1, las tres opciones se ven iguales.
   - En la rueda, con las figuras a ≈1/3 de su tamaño, *Point sin mipmaps* rompe las líneas finas: el cable de los
     auriculares de Lucas sale a trozos, la boca se parte y la visera tiene escalones. Solo es un 0,1 % más nítido.
   - El bilineal sin mipmaps queda entre medias.
5. **Encuadre en el juego (preparado en la rama `feature/encuadre-arte-nuevo`, sin fusionar):**
   - `PortraitCrops.For` elige figura, busto y cara: los del arte antiguo por su `portraitKey` y los del arte nuevo
     por su `artId`.
   - `javier` ya tiene una entrada con el encuadre de este encargo (figura al 90 %, pies a 40 px). Basta con soltar
     los PNG.
   - Cuando estén medidos, **sustituye esa entrada** (`NewArtByArtId` en `Assets/Scripts/Art/PortraitCrops.cs`) por
     los tres Rect del paso 3.
   - Tests (`NewArtCropsTests`): Javier mide 175 cm en la rueda con su `figure`, y sin arte nuevo el juego sigue
     exactamente igual (comprobado también con capturas: la diferencia queda dentro del ruido del grano).
6. **Comprueba en el juego** (skills `batchmode-capture` y `before-after-gallery`):
   - capturas `AnimationCapture.TodosLosRetratos` y una galería antes/después de la historia 3, con interrogatorio,
     mini-retrato del chat y rueda;
   - `LayoutValidationTests` en verde;
   - en la rueda, que Javier mida 175 cm contra la pared.
7. **Cuando el arte esté integrado:** la entrada de Javier en `Assets/Scripts/Art/DerivedPortraits.cs` ya no se usa
   (`PortraitOf` prefiere el arte nuevo). Se puede quitar en el mismo cambio, con sus tests.
