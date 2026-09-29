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
| Lucas Mendoza (1, hermano) | `lucas_*.png` | tranquilo, nervioso, asustado | 16 años, sudadera con capucha, cascos al cuello, mirada esquiva |
| Rosario Gil (1, vecina) | `rosario_*.png` | tranquilo, nervioso, triste | 70 años, viuda, bata de casa, gafas colgadas, cotilla afable |
| Marcos Rial (2, dueño del bar) | `marcos_*.png` | tranquilo, nervioso, enfadado | 42 años, corpulento, camisa remangada, trapo al hombro |
| Andrés Souto (2, cartero) | `andres_*.png` | tranquilo, nervioso, asustado | 52 años, uniforme de Correos amarillo apagado, delgado, retraído |
| Inspector Ruiz (2, inspector) | `ruiz_*.png` | tranquilo, nervioso, enfadado | 55 años, gabardina, bigote canoso, gesto cínico |
| Maruxa Pena (2, vecina) | `maruxa_*.png` | tranquilo, nervioso, enfadado | 74 años, pañuelo en la cabeza, manos curtidas, desconfiada |
| Javier Romero (3, padre) | `javier_*.png` | tranquilo, nervioso, enfadado | 44 años, olivarero, camisa de cuadros, barba de días, ojos rojos |
| Lucía Navarro (3, madre) | `lucia_*.png` | tranquilo, nervioso, triste | 41 años, profesora, jersey sobrio, pelo recogido, contenida |
| Álex Romero (3, hermano) | `alex_*.png` | tranquilo, triste, enfadado | 17 años, serio, chaqueta vaquera, mandíbula apretada |
| Encarna Molina (3, vecina) | `encarna_*.png` | tranquilo, nervioso, triste | 63 años, luto, medalla de la Virgen, sonrisa dulce que inquieta |

**Total: 36 imágenes.** Si hay que priorizar, empieza por los 12 `*_tranquilo.png`: con ellos, cada personaje ya tiene cara propia en todos los estados.

Hasta que haya retratos nuevos se usan los antiguos (`Assets/Images/Suspects/*.gif.png`), que se comparten entre historias. Si no hay ninguno, se muestra un color plano.
