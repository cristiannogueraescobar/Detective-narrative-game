**Mira primero:** `docs/art/RETRATOS-FINAL.jpg` (los 12 personajes a la misma escala y, debajo de cada uno, sus tres expresiones).

# Informe — retratos completos (sesión C, 01-10-2026)

Rama `feature/retratos-completos` (sale de `main`, sin fusionar). Método pedido: **derivar por código a partir de los
originales y editar píxel a píxel; nada de IA**. El arte original no se toca: todo lo nuevo está en archivos nuevos
(`Assets/Art/Derived/javier.png` es un derivado nuestro, regenerado; las expresiones en
`Assets/Art/Derived/Expressions/`). Herramientas: `Tools/make_derived_portraits.py` (derivados) y
`Tools/make_expressions.py` (expresiones), con tests en `Tools/tests/`.

## Resumen
- **Javier pasa**: ya no tiene la cara de Daniel (prueba de parecido 2 de 3 «personas distintas»). Integrado.
- **Las tres vecinas no pasan**: 9 de 9 subagentes dicen «la misma persona» que la vecina original. Cambié la cara
  (gafas, pañuelo, sonrisas, arrugas), pero la pose y el cuerpo son el mismo dibujo píxel a píxel, y eso decide.
  **No se integran**: el juego sigue con sus derivados de la sesión A.
- **Expresiones de los 12: pasan las 24** (las dos más frecuentes de cada uno además de tranquilo, según
  `ART-NEEDED.md`). 3 subagentes identificaron la emoción de 24 de 24 (23 a 3/3; Amparo nerviosa 2/3). Integradas.
- **Recomiendo encargar igualmente** a un artista: las tres vecinas (no hay forma de diferenciarlas sin redibujar la
  pose) y Daniel (el único original de otro estilo); ya están en `docs/art/ENCARGO.md` (rama `feature/sesion-c`).
  Aconsejo también Marcos, Maruxa y Encarna en *tranquilo* (ver «hallazgo» abajo).

## Por personaje

| Personaje | Retrato | Prueba de parecido (vs. su original) | Expresiones (prueba de emoción, 3 subagentes) | Integrado |
|---|---|---|---|---|
| Daniel | Original | — | nervioso 3/3 · enfadado 3/3 | Sí (expresiones) |
| Carmen | Original | — | nervioso 3/3 · triste 3/3 | Sí (expresiones) |
| Lucas | Original | — | nervioso 3/3 · triste 3/3 | Sí (expresiones) |
| Amparo | Derivado (sesión A) | Intento nuevo: 0/3 «distintas» → **no pasa** | nervioso 2/3 · triste 3/3 | Expresiones sí; cara nueva no |
| Marcos | Original | — | nervioso 3/3 · triste 3/3 | Sí (expresiones) |
| Andrés | Original | — | nervioso 3/3 · triste 3/3 | Sí (expresiones) |
| Ruiz | Original | — | nervioso 3/3 · enfadado 3/3 | Sí (expresiones) |
| Maruxa | Derivado (sesión A) | Intento nuevo: 0/3 → **no pasa** | triste 3/3 · nervioso 3/3 | Expresiones sí; cara nueva no |
| Javier | **Derivado nuevo** | **2/3 «distintas» → pasa** | triste 3/3 · nervioso 3/3 | **Sí** (cara y expresiones) |
| Lucía | Derivado (sesión A) | Sin cambios (se distinguía ya: gafas, ropa) | nervioso 3/3 · triste 3/3 | Sí (expresiones) |
| Álex | Derivado (sesión A) | Sin cambios | triste 3/3 · nervioso 3/3 | Sí (expresiones) |
| Encarna | Derivado (sesión A) | Intento nuevo: 0/3 → **no pasa** | nervioso 3/3 · triste 3/3 | Expresiones sí; cara nueva no |

**Qué se hizo a cada uno**
- *Javier* (`Tools/make_derived_portraits.py`, test `JavierTests`): sin bigote (labio superior repintado con el color
  de su mejilla), barba de días en tono plano que sigue la mandíbula, cejas bajas y pobladas, párpados caídos, ojeras
  rojizas, pelo casi negro con las sienes canosas y franjas verticales que convierten las rayas en cuadros. Sobre la
  rejilla del original. La misma camisa, el pantalón y la botella de la sesión A.
- *Expresiones* (`Tools/make_expressions.py`, tests `ExpressionTests` y `LegacyExpressionsTests`): solo la cara. Triste:
  cejas con el extremo interior alto, una lágrima bajo cada ojo y comisuras hacia abajo si la boca se ve; nervioso:
  cejas altas y arqueadas, gota de sudor en la sien y boca en zigzag si se ve; enfadado: cejas en V y mejillas
  encendidas. La ceja de hoy se quita cambiando solo sus píxeles oscuros por el color de la frente. Mismo tamaño y
  silueta (diferencia ≤ 1 %, test) y nada cambia por debajo de la cabeza (test).
- *Culpa*: ninguna expresión delata (Daniel 1A, Carmen 1B, Lucas 1C, Marcos 2A, Andrés 2B, Ruiz 2C, Javier 3A,
  Lucía 3B, Encarna 3C son culpables en una variante): las tres expresiones son las mismas para todos y transmiten la
  emoción (pena, nervios, enfado), no la culpa. La sonrisa nueva de Encarna (no integrada) es cálida, no inquietante.

**Hallazgo de la prueba de emoción:** entre las respuestas de control, el *tranquilo* actual de **Marcos, Maruxa y
Encarna se lee como «enfadado» (0/3)**: el dibujo original tiene el ceño fruncido (y la vecina, la boca abierta). El
jugador ve «tranquilo» en la etiqueta y «enfado» en la cara. Arreglarlo es redibujar su cara base: va al encargo.

## Integración (sección 6 del BRIEF, adaptada a arte derivado)
- Las expresiones se cargan como el retrato de su personaje (`LegacyExpressions`; `PortraitOf` las prefiere al
  retrato único): mismo encuadre de `PortraitCrops` y mismo tratamiento del arte antiguo, porque tienen su tamaño.
  Si falta un estado, su sustituto (asustado y enfadado → nervioso; el resto → tranquilo).
- El catálogo de arte (`Assets/Resources/ArtCatalog.asset`) las incluye: funcionan también en la build.
- Tests: EditMode 759/760 (con `LayoutValidationTests`; el que no corre es un banco Explicit), PlayMode 64, 0 fallos.
- Capturas del interrogatorio y la rueda de acusación de las tres historias: ver la sección siguiente.

## Capturas
`docs/art/CAPTURAS-RETRATOS.jpg` (reducción de 39 capturas a 1080×1920 de `AnimationCapture.RetratosHistoria1-3`):
el interrogatorio de cada personaje con cada una de sus expresiones y la rueda de acusación de las tres historias.
Comprobado mirándolas: la expresión se ve en el busto (lágrimas, gota de sudor, ceño y rubor), con la etiqueta de
estado, el mismo encuadre que el retrato tranquilo, y la rueda muestra a todos a su altura. Las capturas en bruto
están en `docs/screenshots/2026-10-01/anim/` (no se suben).

## Pruebas, tal cual
- Parecido: lámina con el original (1) y el derivado (2) a la misma escala; pregunta «¿la misma persona o dos
  distintas?» a 3 subagentes sin contexto por pareja. Javier: distinta (media), misma (media), distinta (media-alta).
  Vecinas: 9 de 9 «la misma», casi todas con confianza alta; motivo unánime: «misma pose, cuerpo y zapatos».
- Emoción: lámina con las 24 expresiones y 6 caras tranquilas de control, numeradas y barajadas, clave aparte;
  3 subagentes eligen calma/triste/nervioso/enfado para cada número.
