# Crítica de diseño (día 3, bloque C1)

Método: skill `design:design-critique` sobre capturas reales del juego a 1080x1920 y 1080x2400
(`AnimationCapture`), antes de tocar nada (copia en `docs/screenshots/2026-09-30/dia3-antes-C/`). Cada hallazgo
dice la pantalla, por qué importa y qué se hace. Estado al final de cada fila.

## Impresión general
El tono noir está bien conseguido (papel crema, sellos, tipografía de máquina de escribir, sala en penumbra) y la
jerarquía de cada pantalla se entiende. La mayor oportunidad está en **el interrogatorio al empezar**: un tercio
de la pantalla vacío entre el parte del día y las preguntas de ejemplo, justo en la pantalla donde más tiempo se
pasa. Después, pequeñas faltas de acabado: legibilidad del subtítulo del menú, expedientes medio vacíos y estados
de botón poco marcados.

## Usabilidad
| Hallazgo | Gravedad | Recomendación | Estado |
|---|---|---|---|
| **Interrogatorio al empezar: hueco de ~35 % de la pantalla** entre la tarjeta del DÍA 1 (arriba) y las preguntas de ejemplo (abajo). La vista se va arriba y la acción está abajo. | 🔴 Alta | Anclar el chat abajo cuando el contenido no llena la vista (como cualquier app de mensajería): el parte queda junto a las preguntas y el hueco pasa arriba, donde luce la sala. | Arreglado |
| **Expedientes (selección de caso) medio vacíos**: la ficha mide ~190 px y el texto ocupa la mitad superior. Parece a medio hacer. | 🟡 Media | Añadir una línea de "gancho" (quién es la víctima) y el récord (mejor final · rango) abajo a la derecha. | Arreglado |
| **Subtítulo del menú** ("Interrogatorios · Tres casos") pequeño, gris claro, sobre un dibujo muy cargado. | 🟡 Media | Velo oscuro suave detrás del bloque del título, sin tapar el dibujo. | Arreglado |
| El selector de sospechoso de la acusación repite lo que ya dice la rueda de fotos. | 🟢 Baja | Se mantiene: es la alternativa accesible a tocar las fotos y dice el nombre completo. | Se queda |
| "Prueba clave" aparece solo con pistas en la libreta; sin ellas no hay nada que elegir. | 🟢 Baja | Correcto (no mostrar controles vacíos). | Se queda |

## Jerarquía visual
- **Qué atrae primero:** menú → el título y "Jugar" (correcto). Interrogatorio → el retrato y "Acusar" en dorado.
  *Acusar* es la acción más cara y la que más brilla en la pantalla de preguntar: el dorado debería ser para
  **Enviar**. → Acusar pasa a botón secundario con texto de alerta (sigue siendo visible, deja de competir con
  preguntar; contraste 5,7:1 comprobado por el test). **Arreglado.**
- **Flujo de lectura:** HUD → retrato → acciones → chat → pruebas → campo. Correcto una vez anclado el chat abajo.
- **Énfasis:** los encabezados en dorado (LUGAR, VÍCTIMA…) y los sellos funcionan; el parte del día en cursiva
  lila pequeña es lo más débil (se lee, pero cansa): ver Accesibilidad.

## Consistencia
| Elemento | Problema | Recomendación | Estado |
|---|---|---|---|
| Botones | Los del menú usan la fuente de máquina de escribir; el resto, la de texto. | Se mantiene a propósito: el menú es la "portada". Todo lo demás usa la de texto. | Se queda |
| Estados de botón | Pulsado y desactivado se distinguen poco del normal (solo cambia un poco el tono). | Pulsado: más oscuro y 2 % más pequeño; desactivado: 45 % de opacidad. Valores en el tema. | Arreglado |
| Radios | Botones, tarjetas y burbujas usan radios parecidos pero no iguales. | Una escala en el tema (pequeño 12 / medio 20) y usarla en todo. | Arreglado: escala en el tema (6 · 12 · 16 · 20 · 36), sin radios sueltos en el código |

## Accesibilidad
- **Contraste:** todos los textos pasan WCAG AA (test automático en todos los paneles, en normal, "Muy grande" y
  alto contraste).
- **Zonas táctiles:** ≥ 48 dp en todos los controles (test); enlaces de la libreta con tolerancia de media zona.
- **Legibilidad:** el parte del día (cursiva pequeña y lila) es el texto más difícil de la pantalla principal.
  → Sin cursiva, algo más grande y en el color del texto principal. **Arreglado.**

## Lo que funciona
- La libreta de papel con enlaces y el botón Pensar al pie: se entiende sin explicación.
- Los sellos (CONFIDENCIAL, finales) y el expediente: dan identidad sin arte nuevo.
- La rueda de reconocimiento con los retratos grandes.

## Prioridades
1. **Anclar el chat abajo** — la pantalla principal deja de tener un agujero en el centro.
2. **Acusar deja de ser el botón dorado del interrogatorio** — el dorado guía a preguntar.
3. **Expedientes completos y estados de botón claros** — acabado de juego publicado.

## C3: personajes (2.5D frente a vóxel)
Prototipos con los mismos retratos (padre, madre, vecina, cartero), capturas en
`docs/screenshots/2026-09-30/c3-prototipo/` (`comparativa_*.png`).

| Opción | Cómo | Resultado |
|--------|------|-----------|
| Plano 2D (antes) | Gradación sepia (Desaturate) | Correcto pero plano: pegatina sobre el fondo. |
| **2.5D (elegida)** | Shader `Detective/UI/LitPortrait`: relieve calculado en pantalla desde el alfa y la luminancia de la propia imagen, lámpara cálida arriba a la izquierda y contraluz frío en el filo contrario | Se lee volumen y luz de interrogatorio sin perder el dibujo; ~9 lecturas de textura por píxel, sin mallas ni cámaras. |
| Vóxel 3D | Rejilla de 40 o 64 columnas extruida (hondo según la distancia al borde), cámara propia a textura | Pierde la cara y la expresión (el arte no es pixel art limpio: está reescalado); 9-23 k vértices y una cámara por retrato. Descartado. |

Aplicado a los 12 personajes (todos usan los 7 retratos antiguos): interrogatorio, rueda de reconocimiento y
mini-retrato del chat (que antes salía sin gradación). Las emociones conservan el relieve.
**Reversible:** `Theme.portraitLit = false` devuelve el plano 2D; lámpara, relieve y contraluz son valores del tema.
