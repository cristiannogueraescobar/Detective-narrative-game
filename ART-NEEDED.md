# Arte necesario

Todo el arte es **opcional**: si falta un archivo, el juego usa un sustituto y sigue funcionando. Para que una imagen aparezca, basta con soltarla en su ruta con el nombre exacto. En el editor se ve al momento; para las builds, el catálogo `Assets/Resources/ArtCatalog.asset` se regenera solo, y también desde el menú **Detective → Reconstruir catálogo de arte**.

## 1. Retratos por estado emocional

- **Ruta:** `Assets/Art/Portraits/<personaje>_<estado>.png`
- **Tamaño:** 768 × 1024 px (3:4, vertical). **Formato:** PNG, fondo transparente u oscuro liso.
- **Encuadre:** busto (de la cabeza al pecho), el mismo encuadre y la misma posición de la cara en todos los estados de un personaje, para que el cambio de estado no "salte".
- **Estilo:** noir, poco saturado, luz lateral dura. Deben casar con la paleta del tema (grises, sepias y un acento ámbar).
- **Estados:** el juego maneja cinco: `tranquilo`, `nervioso`, `asustado`, `enfadado` y `triste`. Pido **tres por personaje**. Si falta un estado se usa el sustituto: `asustado` y `enfadado` pasan a `nervioso`, `triste` pasa a `tranquilo`, y en último caso se usa `tranquilo`. El tinte y el temblor de cada estado se aplican siempre, haya imagen específica o no.

| Personaje (historia) | Archivo base | Estados pedidos | Descripción |
|---|---|---|---|
| Daniel Mendoza (1, padre) | `daniel_*.png` | tranquilo, nervioso, enfadado | 48 años, abogado, traje oscuro impecable, gesto contenido y frío |
| Carmen Vidal (1, madre) | `carmen_*.png` | tranquilo, nervioso, triste | 45 años, pediatra, cansada, ojeras, rebeca sobre ropa de hospital |
| Lucas Mendoza (1, hermano) | `lucas_*.png` | tranquilo, nervioso, triste | 16 años, sudadera con capucha, cascos al cuello, mirada esquiva |
| Rosario Gil (1, vecina) | `rosario_*.png` | tranquilo, nervioso, triste | 70 años, viuda, bata de casa, gafas colgadas, cotilla afable |
| Marcos Rial (2, dueño del bar) | `marcos_*.png` | tranquilo, nervioso, triste | 42 años, corpulento, camisa remangada, trapo al hombro |
| Andrés Souto (2, cartero) | `andres_*.png` | tranquilo, nervioso, triste | 52 años, uniforme de Correos amarillo apagado, delgado, retraído |
| Inspector Ruiz (2, inspector) | `ruiz_*.png` | tranquilo, nervioso, enfadado | 55 años, gabardina, bigote canoso, gesto cínico |
| Maruxa Pena (2, vecina) | `maruxa_*.png` | tranquilo, triste, nervioso | 74 años, pañuelo en la cabeza, manos curtidas, desconfiada |
| Javier Romero (3, padre) | `javier_*.png` | tranquilo, triste, nervioso | 44 años, olivarero, camisa de cuadros, barba de días, ojos rojos |
| Lucía Navarro (3, madre) | `lucia_*.png` | tranquilo, nervioso, triste | 41 años, profesora, jersey sobrio, pelo recogido, contenida |
| Álex Romero (3, hermano) | `alex_*.png` | tranquilo, triste, nervioso | 17 años, serio, chaqueta vaquera, mandíbula apretada |
| Encarna Molina (3, vecina) | `encarna_*.png` | tranquilo, nervioso, triste | 63 años, luto, medalla de la Virgen, sonrisa dulce que inquieta |

Los estados pedidos salen de datos: tras ajustar la guía de estados (30 sep), en ~1.500 respuestas de partidas del
bot cada personaje usa sobre todo estos (siempre `tranquilo` como cara base, más sus dos estados más frecuentes).
Por ejemplo Javier: triste 48 %, nervioso 36 %, enfadado 9 %; Marcos: nervioso 55 %, triste 17 %, enfadado 5 %.

**Total: 36 imágenes.** Si hay que priorizar, empieza por los 12 `*_tranquilo.png`: con ellos, cada personaje ya tiene cara propia en todos los estados.

Hasta que haya retratos nuevos se usan los antiguos (`Assets/Images/Suspects/*.gif.png`), que se comparten entre historias. Si no hay ninguno, se muestra un color plano.

## 2. Fondos, cabeceras e intro de caso

Todas las imágenes son verticales para móvil, con la resolución de referencia de 1080 × 1920. Si falta una, se usa un color plano del tema.

| Uso | Ruta exacta | Tamaño | Formato | Contenido |
|---|---|---|---|---|
| Fondo del menú principal | `Assets/Art/Backgrounds/menu.png` | 1080 × 1920 | PNG | Calle mojada de noche, farola, silueta con gabardina de espaldas; mucho espacio oscuro en el centro para los botones |
| Cabecera historia 1 | `Assets/Art/Stories/historia1_cabecera.png` | 1080 × 480 | PNG | Fachada de chalé en Santiago de noche, una ventana iluminada en el piso de arriba |
| Intro historia 1 | `Assets/Art/Stories/historia1_intro.png` | 1080 × 1920 | PNG | Habitación infantil a oscuras, mesilla con una taza, luz azul de ambulancia en la ventana. Parte central poco detallada (irá texto encima) |
| Cabecera historia 2 | `Assets/Art/Stories/historia2_cabecera.png` | 1080 × 480 | PNG | Bar de pueblo costero de madrugada, guirnaldas de fiesta apagadas, niebla |
| Intro historia 2 | `Assets/Art/Stories/historia2_intro.png` | 1080 × 1920 | PNG | Carretera de la costa en curva, de madrugada, un bolso en las rocas de una cala. Centro despejado para texto |
| Cabecera historia 3 | `Assets/Art/Stories/historia3_cabecera.png` | 1080 × 480 | PNG | Olivar al atardecer con una columna de humo negro al fondo |
| Intro historia 3 | `Assets/Art/Stories/historia3_intro.png` | 1080 × 1920 | PNG | Quemadero con ceniza humeante y una zapatilla medio quemada; cortijo al fondo. Centro despejado para texto |

**Composición:** las pantallas de intro llevan el texto del parte del caso encima, así que conviene dejar oscuro y con poco detalle la franja central (del 25 % al 85 % de la altura).

## 3. Iconos

- **Formato:** PNG de 128 × 128 px con fondo transparente, trazo claro (blanco roto `#E8E2D6`) o ámbar (`#D9A441`).
- **Estilo:** de línea, sencillo, legible a 48 px.

| Icono | Ruta exacta | Descripción |
|---|---|---|
| Libreta | `Assets/Art/Icons/libreta.png` | Libreta de detective con goma elástica |
| Pista | `Assets/Art/Icons/pista.png` | Lupa sobre una ficha |
| Día | `Assets/Art/Icons/dia.png` | Hoja de calendario arrancada |
| Preguntas | `Assets/Art/Icons/preguntas.png` | Bocadillo de diálogo con interrogación |
| Contradicción | `Assets/Art/Icons/contradiccion.png` | Dos flechas que chocan, o un aspa sobre una ficha |

## 4. Cómo añadir arte

1. Guarda la imagen con el nombre y la ruta exactos de esta lista (crea la carpeta si no existe).
2. En el editor aparece al momento. El catálogo para las builds (`Assets/Resources/ArtCatalog.asset`) se regenera solo.
3. No hace falta tocar ninguna escena ni ningún script.
