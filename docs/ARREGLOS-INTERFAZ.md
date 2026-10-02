docs/screenshots/2026-10-02/galeria/arreglos-interfaz-interrogatorio.jpg

# Arreglos de interfaz (revisión de Cristian de las capturas de retratos)

Rama `feature/arreglos-interfaz`, desde `main` (`b92ad83`). Sin fusionar.

Galerías antes/después, con la misma pantalla y la misma historia (la 2, primer día, tres sospechosos) a 1080 × 1920,
1080 × 2400, 1440 × 3200 y 1080 × 3840:

- Interrogatorio: `docs/screenshots/2026-10-02/galeria/arreglos-interfaz-interrogatorio.jpg`. Columnas: antes,
  opción A (busto) y opción B (figura).
- Acusación: `docs/screenshots/2026-10-02/galeria/arreglos-interfaz-acusacion.jpg`.

"Antes" sale de un worktree de `main` con la misma prueba de captura (`AnimationCapture.InterfazHistoria2`). Las capturas
en bruto están en `C:\AI\dng-captures\antes` y `C:\AI\dng-captures\despues`.

## Primero: qué estaba ya en main y qué trajo feature/retratos-completos

Los seis problemas ya estaban en `main`; la rama de retratos no introdujo ninguno. Lo que hizo que se vieran tan mal en
`CAPTURAS-RETRATOS.jpg` fue un error mío al capturar: pedí `-captureHeight 1920` con el ancho por defecto de 540, que da
540 × 1920 (1080 × 3840, proporción 9:32). Ningún móvil tiene esa proporción. Reproducido en `main` con la misma
proporción: el mismo busto gigante, los controles en 33 px y el texto de arriba cortado.

| # | Problema | ¿En main a tamaños reales? | Causa |
|---|---|---|---|
| 1 | Controles tapados o aplastados | No. Sí a 9:32 | `TallScreenHeader` ensanchaba el busto con el alto de la pantalla, sin tope: a 9:32 se llevaba todo el ancho y dejaba los controles en 33 px |
| 2 | Sospechoso dos veces | Sí, siempre | Busto en la cabecera y figura tenue en el centro (Sesión A, `interrogationStage`) |
| 3 | Busto ampliado y borroso | Leve a 1440 × 3200; grave a 9:32 | `AspectRatioFitter` estiraba el recorte (unos 450 × 600 téxeles) a lo que midiera la caja, con filtrado bilineal |
| 4 | Bordes azules | Sí, siempre | Contraluz frío del relieve 2.5D (`portraitRimStrength` 0,7), el mismo para la rueda que para el busto grande |
| 5 | "QUEDAN 5 PR…" | No. Sí a 9:32 | Una línea que se reduce hasta 30 px y después pone "…" |
| 6 | Rueda pequeña con media pantalla vacía | Sí, sobre todo a 20:9 | Con tres o más, la fila la limita el ancho: tres figuras de cuerpo entero no caben más grandes una al lado de otra |

## Qué se ha hecho

| # | Arreglo | Test (rojo antes, verde ahora) |
|---|---|---|
| 1 | El busto ocupa como mucho el 40 % del ancho de la cabecera (`Theme.headerPortraitMaxShare`); el resto es para los controles. Test nuevo: ningún control interactivo queda fuera de pantalla, por debajo de 48 dp ni con algo encima. "Encima" es un gráfico dibujado después que recibe el toque o que se ve (alfa ≥ 0,5). Se mira en cinco puntos de cada control, en todos los paneles, a los cuatro tamaños y con texto normal y muy grande. | `LosControlesSeVenYSeTocan` (104 casos; rojo: 3 controles en 33 px a 9:32). `ElTestCazaUnControlTapadoPorUnaImagen` comprueba que el test detecta una imagen encima de un botón. `ConLaFiguraLosControlesSeVenYSeTocan` |
| 2 | Una sola composición, elegida en el tema (`interrogationComposition`). **A, busto** (por defecto): sin figura en el centro. **B, figura**: sin busto, la cabecera queda solo con los controles a todo lo ancho, la figura casi opaca (`stageAlpha` 0,95) y la etiqueta de estado sobre su cabeza; la sacudida y la entrada pasan a la figura. | `ElSospechosoSaleUnaSolaVez(Busto/Figura)`, `ConLaFiguraLaCabeceraEsSoloControles` |
| 3 | `PixelScale.Fit` + `PixelFit` sustituyen al `AspectRatioFitter`. Reducir sí; ampliar, como mucho `portraitMaxMagnification` (1 por defecto: nunca más de un píxel de pantalla por téxel) y siempre por múltiplos enteros. Cuenta la densidad de la pantalla (`Canvas.scaleFactor`). Se aplica al busto y a la figura. | `UnRetratoMasPequeñoQueSuCajaSeQuedaASuTamaño`, `…SeReduceYConservaLaProporcion`, `SiSePermiteAmpliarEsPorMultiplosEnteros`, `LaDensidadDeLaPantallaCuenta`, `ElBustoDelInterrogatorioNoPasaDe1a1` |
| 4 | Gradación nueva `ArtGrading.Kind.LegacyBust`: el busto y la figura llevan el contraluz de `Theme.bustRimStrength`, 0 por defecto (apagado). La rueda y la ficha policial conservan el suyo (0,7). Para volver a encenderlo en el busto: `bustRimStrength` > 0. | `ElBustoNoLlevaContraluzYLaRuedaSi`, `ElContraluzDelBustoSeEnciendeDesdeElTema`, `ElBustoDelInterrogatorioUsaElMaterialSinContraluz` |
| 5 | Modo de texto nuevo `OneLineOrTwo`: una línea que se reduce y, si al mínimo legible aún no cabe, dos. Las partes van con `<nobr>`, así que se parte por el punto: "DÍA 1 DE 7 ·" / "QUEDAN 5 PREGUNTAS". A tamaños reales sigue en una línea, como antes. | `LaBarraDeArribaCabeEntera` (8 casos, con "SIN PREGUNTAS HOY" y "QUEDA 1 PREGUNTA"; rojo a 9:32) |
| 6 | Las figuras de la rueda pueden solaparse un 30 % (`Theme.lineupOverlap`). Los huecos de toque (botón y anillo) no se solapan: solo asoma la figura, que no recibe toques. Nada asoma por los extremos (no tapa las cifras de la pared), y si un hueco bajara de 48 dp la rueda vuelve a no solapar. Con figuras más grandes, la cifra de 200 necesitaba sitio encima (en tableta quedaba oculta). | `LaRuedaOcupaElSitio` (rojo a 1080 × 2400: la pared de 2 m ocupaba el 50 % del alto), `ConSolapeLasFigurasCrecenSinSalirseDeLaFila` |

Suites al terminar: EditMode 901/902 (el que no corre es el benchmark, que solo se ejecuta a petición), PlayMode 66 en
verde y 0 fallos, con los tests de layout (`LayoutValidationTests`) dentro. Ollama no se cargó en ningún momento.

## Lo que no se consigue del todo (dicho claro)

- **Rueda a 20:9 con tres sospechosos:** la pared de 2 m ocupa el 68 % del alto disponible (antes, el 50 %); a 16:9,
  casi todo. Por encima queda pared sin rayas. Más que eso no se puede con tres figuras de cuerpo entero en una fila a
  escala real, salvo solapándolas más (`lineupOverlap` hasta 0,4: se amontonan) o en dos filas. He calculado que dos
  filas dan figuras más pequeñas con tres sospechosos: solo compensarían con cuatro o más. El test pide el 65 %; lo dejo
  a la vista para que no parezca más de lo que es.
- **Opción B:** cuando el chat crece, los mensajes tapan la figura de abajo arriba; por eso la etiqueta de estado va
  sobre la cabeza. Con el busto no pasa.
- A 9:32 el desplegable de sospechoso corta "Marcos (dueño…)", porque el control es más estrecho. Esa proporción no
  existe en ningún móvil; lo que importa es que se ve y se toca.

## Al fusionar

`feature/retratos-completos` encima de esta rama choca solo en `Assets/Tests/PlayMode/AnimationCapture.cs` (las
dos ramas tocan la opción de ancho de captura y añaden capturas en el mismo sitio): hay que quedarse con las dos partes.
Orden propuesto: esta rama primero y después `feature/retratos-completos`.

## Decisiones para Cristian

1. **¿Busto (A) o figura (B)?** Recomiendo A: el estado del sospechoso, que es información de juego, se ve siempre y no
   lo tapa el chat. B da más presencia al personaje. Cambiar: `Theme.interrogationComposition`.
2. **Solape de la rueda:** 0,3. Si se ve amontonada, 0,2 (pared al 60 % a 20:9); si se quiere más grande, 0,4. Cambiar:
   `Theme.lineupOverlap`. Con 0 queda como antes.
3. **Ampliación del busto:** 1 (nunca ampliar). Con 2, en pantallas muy densas el busto sería el doble, con cada téxel en
   2 × 2 píxeles (nítido). Cambiar: `Theme.portraitMaxMagnification`.
4. **Contraluz:** apagado en el busto y la figura y encendido en la rueda. Para apagarlo también en la rueda:
   `Theme.portraitRimStrength` = 0.

## Lo que salió mal por el camino

- La causa de todo el problema 1 era un error mío: capturé las fotos de retratos a 9:32 sin darme cuenta. La prueba de
  captura acepta ahora `-captureWidth`, y el nombre de archivo lleva el tamaño real (`_1080x2400`).
- Borré las capturas "antes" antes de copiarlas a `C:\AI`; las volví a sacar de un worktree de `main`.
- Un heredoc volvió a convertir un `\n` de un test en un salto de línea de verdad; corregido antes de compilar.
- El tema de prueba desaparecía al abrir la escena en la vista previa (Unity lo descargaba como asset sin dueño):
  `hideFlags = HideAndDontSave`.
- El test de la etiqueta de estado en la opción B lo escribí a la vez que el arreglo; no lo vi en rojo.
- `LaEscenaMuestraAlSospechosoYLaViñetaSubeConLaTension` (PlayMode) pedía la figura tenue de la Sesión A. Lo he cambiado
  a la regla nueva: por defecto, busto y sin figura.
