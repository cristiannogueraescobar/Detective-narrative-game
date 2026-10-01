# Auditoría de imágenes

Fecha: 2026-09-29. Imágenes analizadas por código (Pillow) y revisadas a ojo:
- **Formato real**: por la cabecera del fichero, no por la extensión.
- **Paleta**: los 5 colores dominantes, la saturación, el brillo y el porcentaje de píxeles "chillones" (saturación > 60 % con brillo > 60 %).
- **Importación**: los ajustes de Unity leídos del `.meta`.

Script: `image_audit.py` (en el scratchpad de la sesión, reproducible).

**Referencia del tema noir:** fondos casi negros (`#0F1012`), paneles grafito (`#1A1B1F`), texto blanco roto (`#E8E2D6`) y un único acento ámbar (`#D9A441`). Encaja lo oscuro y poco saturado; desentonan las zonas grandes muy saturadas y claras.

## Inventario

| Imagen | Tamaño | Real | Peso | Colores dominantes | Píx. chillones | Encaje con el tema | Problemas técnicos |
|---|---|---|---|---|---|---|---|
| `Assets/Backgrounds/intro_fondo.png.png` | 1024 × 1536 | PNG | 2,2 MB | `#342519` `#040304` `#916A3F` | 4 % | **Bueno**: sótano oscuro y cálido | doble extensión; no se usa en la escena |
| `Assets/Backgrounds/menu_fondo.png.png` | 1024 × 1536 | PNG | 2,9 MB | `#19120D` `#472711` `#76532B` | 10 % | **Aceptable**: sepia, con lámpara verde y detalles rojos | doble extensión; peso alto |
| `Assets/Images/Suspects/cartero.gif.png` | 754 × 1095 | PNG | 714 KB | `#000000` `#191717` `#756644` | 9 % | **Desentona**: pixel art alegre, azul y amarillo | doble extensión (".gif" engañoso: es PNG estático) |
| `Assets/Images/Suspects/detective.gif.png` | 1024 × 1536 | PNG | 2,3 MB | `#000000` `#1A261B` `#5D6A2D` | 10 % | **Desentona**: uniforme verde de Guardia Civil, no un inspector de paisano | doble extensión; peso alto |
| `Assets/Images/Suspects/duenio_bar.gif.png` | 745 × 1206 | PNG | 805 KB | `#000000` `#351311` `#967352` | 14 % | **Regular**: delantal rojo | doble extensión |
| `Assets/Images/Suspects/hermano.gif.png` | 641 × 1248 | PNG | 677 KB | `#000000` `#788580` `#191B1D` | 6 % | **Aceptable**: tonos fríos apagados | doble extensión |
| `Assets/Images/Suspects/madre.gif.png` | 602 × 1240 | PNG | 734 KB | `#000000` `#D08A43` `#672910` | **40 %** | **Desentona mucho**: vestido rojo y amarillo, delantal; no parece una pediatra ni una profesora | doble extensión |
| `Assets/Images/Suspects/padre.gif.png` | 760 × 1158 | PNG | 686 KB | `#000000` `#AB9861` `#31231F` | **28 %** | **Desentona mucho**: camisa de rayas amarilla y cerveza; no parece un abogado ni un olivarero | doble extensión |
| `Assets/Images/Suspects/vecina.gif.png` | 1024 × 1536 | PNG | 2,3 MB | `#000000` `#9BAA2D` `#442322` | **36 %** | **Desentona mucho**: vestido verde lima | doble extensión; peso alto |
| `Assets/UI_Images/sospechosos_imagen.png.png` | 1536 × 1024 | PNG | 1,8 MB | `#000000` `#3C3A34` `#96904C` | 16 % | **Regular**: grupo en pixel art saturado | doble extensión; horizontal en un juego vertical |
| `Assets/TextMesh Pro/Sprites/EmojiOne.png` | 512 × 512 | PNG | 110 KB | `#FDC83C` … | 72 % | Fuera de alcance: sprites de ejemplo de TextMesh Pro, no se usan | — |

**Hallazgos generales:**
- Ningún `.gif.png` es realmente un GIF: todos son PNG estáticos con la extensión doble.
- Todos los retratos son **cuerpo entero**, pero el marco del retrato es vertical y pequeño, así que la cara queda diminuta en móvil.
- El juego tiene **12 personajes** y solo **7 retratos**, compartidos entre historias. Por ejemplo, "padre" es a la vez Daniel el abogado y Javier el olivarero.

## Corregido por código (no destructivo)

Ningún archivo original se ha modificado. Todo está parametrizado en el tema (`Assets/Resources/Theme.asset`, sección "Gradación del arte existente").

1. **Gradación de retratos antiguos**: el shader `Detective/UI/Desaturate` (en `Assets/Resources/Shaders/`, incluido en las builds) se aplica con un material en tiempo de ejecución:
   - saturación al 35 %;
   - tinte sepia `(0.95, 0.88, 0.78)`;
   - brillo al 90 %.

   Solo afecta al arte antiguo: los retratos nuevos de `Assets/Art/Portraits/` se muestran tal cual.
2. **Pixel art nítido**: los retratos antiguos se muestran con filtrado sin suavizar, cambiando la propiedad de la textura en memoria, no la importación. Se desactiva con `legacyPortraitPointFilter`.
3. **Fondos e ilustraciones**: el fondo del menú y cualquier `Image` con un sprite propio (no de uGUI) reciben una gradación suave: saturación al 70 % y brillo al 75 %, para que el texto se lea encima.
4. **Protección contra el tema**: el `ThemeApplier` distingue ilustraciones de sprites de interfaz. Así no pinta el fondo del menú (`MainMenuPanel`) con el color de panel, que lo habría dejado casi negro.

Vista previa, antes (arriba) y después (abajo), simulada con los mismos valores: `docs/img/image-audit-gradacion.png`.

## Recomendaciones para el arte original (decisión tuya)

| Prioridad | Recomendación | Motivo |
|---|---|---|
| Alta | Rehacer los retratos como **busto 768 × 1024** con la paleta noir, uno por personaje (12), según `ART-NEEDED.md` | Los actuales son cuerpo entero, alegres y compartidos entre personajes distintos; la gradación solo los disimula |
| Alta | Quitar la doble extensión: `*.gif.png` → `*.png`, `*.png.png` → `*.png` | Confunde y sugiere GIF animado; renombrar en Unity conserva las referencias (`.meta`) |
| Media | Importación para móvil: `Max Size` 1024 en retratos y fondos, compresión ASTC 6×6 en Android, sin mipmaps en UI | Hoy los retratos tienen mipmaps activados (`enableMipMap: 1`) y 2048 de tamaño máximo; es memoria desperdiciada en UI |
| Media | Fondo del menú: sustituir o retocar los detalles rojos y la lámpara verde por ámbar | Es el único arte que ve todo jugador |
| Baja | Retirar `sospechosos_imagen.png.png` o rehacerlo en vertical | Horizontal en un juego vertical y con el mismo estilo que los retratos |
| Baja | `intro_fondo.png.png` no se usa en la escena | Puede servir de base para las intros de historia (`Assets/Art/Stories/`) |
