---
name: art-brief
description: Use when writing an art request, illustrator brief or image-generation prompt for this game (portraits, emotional states, backgrounds, intro art, headers, icons), or updating ART-NEEDED.md.
---

# Encargos de arte

Todo encargo va en `ART-NEEDED.md`. Un test obliga a que cada hueco de arte del código esté documentado allí. Al soltar el archivo en la ruta indicada, el catálogo se regenera solo.

## Cada entrada lleva

| Campo | Valor |
|---|---|
| Ruta y nombre | Retratos: `Assets/Art/Portraits/<artId>_<estado>.png` (`artId` es el interno: Amparo = `rosario`) |
| Tamaño y formato | Retratos **768×1024 (3:4), PNG con fondo transparente**, cuerpo entero. Fondos 1080×1920. Cabeceras 1080×480. Iconos 128×128 transparentes, legibles a 48 px, trazo #E8E2D6 o #D9A441. |
| Estilo | Una sola frase de estilo compartida, igual a la de los originales ("16-bit pixel art… full body… 3:4… transparent"), más edad, oficio, ropa, objeto y expresión del personaje |
| Estados | `tranquilo` como base más los **2 más frecuentes** según los datos del bot (~1500 respuestas). Javier, Maruxa y Álex necesitan "triste", no "enfadado". 3 por personaje: 36 imágenes, no 60. Primero los 12 `*_tranquilo`. |
| Consistencia | El mismo encuadre en todos los estados: la cara no salta al cambiar de estado. |
| Restricciones de composición | Arte de intro: la franja central (25-85 % de la altura) oscura y con poco detalle, porque el texto va encima. |
| Para qué personaje es urgente | Primero los que hoy comparten cara: Javier y Daniel, y las tres vecinas. |

## Además

- Ojo con los nombres: los personajes se inspiran en casos reales, pero ningún encargo usa nombres ni rasgos de personas reales.
- Anota también lo que se deja sin hacer a propósito, y por qué (las cabeceras repetirían la escena a pantalla completa).
- Tras recibir el arte: vuelve a medir el encuadre y las alturas de la rueda (ver la skill `pixel-portrait-kitbash`) y haz una galería a tamaño de juego (ver la skill `before-after-gallery`).
