docs/screenshots/2026-10-02/galeria/tarjeta-pruebas.jpg

# Tarjeta "TUS PRUEBAS" en la acusación

Adoptada por Cristian (02-10-2026). Rama `feature/propuesta-resumen-acusacion`, sin fusionar.

## Qué hace

| Punto | Comportamiento | Test |
|---|---|---|
| a | Con la tarjeta, la pregunta ya no lleva la frase gris "Tu libreta está vacía…" / "En tu libreta…". Se queda solo la explicación de la prueba clave, que no está en la tarjeta: sin ella, el desplegable "Prueba clave" no se entiende. La caja de la pregunta mide ahora lo que su texto (antes, 240 fijos con la mitad vacía). | `ConLaTarjetaNoSeRepiteLaFraseGris` |
| b | Libreta vacía: dos líneas, "TUS PRUEBAS" y "Libreta vacía: acusar ahora es una apuesta.". El resto, para la rueda. | `ConLaLibretaVaciaLaTarjetaSonDosLineas` (16:9 y 20:9) |
| — | Entera si cabe en la pared que la rueda no usa (en 20:9 la rueda la limita el ancho). La rueda no encoge. | `En20a9ConPistasLaTarjetaEntera`, `LaTarjetaEnteraNoEncogeLaRueda` |
| c | Si no cabe (16:9), una línea, "TUS PRUEBAS · tres pistas · una contradicción", con flecha. Al tocarla se despliega y empuja la rueda hacia abajo mientras está abierta. Nunca tapa la rueda, el texto de arriba ni los botones; otro toque la pliega. | `En16a9ConPistasUnaLineaQueSeDespliega`, `ConMuchasPruebasSePliegaYNoTapaNada` (6 pistas y 12 contradicciones) y el test de controles (`CheckControls`) en cada estado |
| d | Las contradicciones con el mismo texto que la libreta (`AIConversationManager.DescribeContradiction`, el que usa la libreta): "La versión de X («cita») choca con: pista". Ni más ni menos. | `LaTarjetaUsaElTextoDeLasContradiccionesTalCual` y, jugando, `TarjetaPruebasTests.LaTarjetaDiceLasContradiccionesComoLaLibreta`: cada línea de la tarjeta está en la libreta y al revés |
| e | Capturas jugando de verdad (`ScriptedPlay`): se elige al sospechoso, se pregunta y se pulsa Enviar. Las pistas salen por el detector y la contradicción, enseñando la prueba al culpable con "Mostrar prueba". Lo único con guion es el modelo, que contesta con la frase real de cada pista (sus `sampleHits`). Libreta, desplegables y textos son los del juego. | `AnimationCapture.TarjetaPruebas` |

## Para decidir

1. **La contradicción nombra a quien miente, y solo miente el culpable.** "La versión de Marcos («no salí del bar») choca
   con…" deja ver al culpable en cuanto hay una contradicción. Ya pasaba en la libreta; la tarjeta lo repite tal cual,
   como pediste, así que no revela nada nuevo. Pero en la acusación salta a la vista. Si se quiere evitar, habría que
   cambiar el texto de la libreta (`DescribeContradiction`), y la tarjeta lo seguiría.
2. Desplegada en 16:9, la rueda encoge mientras está abierta (no se tapa). La alternativa sería una capa por encima de
   la rueda, que la taparía.
3. Números en letra hasta diez y en cifras desde once, como el resto del juego (decisión de Cristian, 02-10-2026):
   "TUS PRUEBAS · tres pistas · una contradicción". Comparte el texto con el resumen de la acusación.

## Lo que salió mal por el camino

- En 20:9 la tarjeta entera no cabía por 8 px: la caja de la pregunta tenía 240 fijos. Ahora mide lo que su texto, y
  la tarjeta pone el resumen en la línea del título en lugar de dos cabeceras.
- La tarjeta cambiaba de forma en pleno rebuild del layout (error de TextMeshPro). Ahora un cambio de tamaño la marca y
  se decide en el siguiente fotograma.
- En las primeras capturas, el aviso "PISTA NUEVA" y el sello de contradicción seguían en pantalla: la captura abría la
  acusación nada más conseguir la última pista. Ahora espera a que acaben.
- El test que juega de verdad (`TarjetaPruebasTests`) lo escribí después de conectar `GameManager`; no lo vi en rojo.
  Los de la tarjeta (EditMode), sí: 14 en rojo antes de escribirla.
- Tres tests pedían la frase gris de antes (`ElResumenDeLaAcusacionSigueAlTema`, el de la música al volver de la
  acusación y el de una sola línea del estado vacío). Están adaptados: el aviso está ahora en la tarjeta.

Suites: EditMode 927/928 (el que no corre es el benchmark, a petición), PlayMode 67, 0 fallos. Ollama, sin cargar.
