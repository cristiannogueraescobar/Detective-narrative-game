# Informe de trabajo autónomo (12 h)

**Estado:** hechas las 15 tareas de la cola. Todo está en ramas, sin push ni fusión a `main`.
**Tests EditMode:** 297/297 en verde en `feature/ui-movil`.
**Revisión independiente:** ver el apartado "Revisión final" al pie.

## Ramas (en cadena, cada una contiene la anterior)

| Rama | Contenido | Último commit |
|---|---|---|
| `feature/historias-etapa1` | Etapa 1, los 5 pendientes menores, historias 2 y 3, calibración y resolubilidad | `390376f` |
| `feature/estados-emocionales` | Estados emocionales, retratos por estado, efecto de escritura, calibración de la etiqueta y QA. Incluye fusionada la rama anterior | `c0c22ff` |
| `feature/ui-movil` | UI móvil, tema, animaciones, intro de caso, libreta, guardado, arte, rendimiento, auditoría de imágenes y pseudo-3D | `HEAD` |

Hay además un worktree en `C:\Dev\dng-calib`, con `feature/historias-etapa1`, que usé para calibrar sin interferir con mis ediciones. Se puede borrar con `git worktree remove ../dng-calib`.

---

## Orden de pruebas en el editor

Recomiendo abrir `feature/ui-movil` y probar en este orden (de lo más básico a lo más visual):

1. **Compilación y tests.** Window → General → Test Runner → EditMode → Run All: deben salir 297 en verde.
2. **Flujo básico en vista vertical.** En la vista Game, elige una resolución vertical (por ejemplo 1080×1920). Pulsa Play.
   - El menú debe verse con el fondo oscurecido. Si hay partida guardada, aparece "Continuar" y el botón de jugar dice "Nueva partida".
3. **Intro del caso.** Tras jugar aparece el **parte del caso** (lugar, víctima, situación) escribiéndose; un toque lo completa.
4. **Una variante concreta.** En `GameManager` → Debug Case, elige `Caso1A` y juega un par de días:
   - retrato con respiración sutil; tócalo y se inclina;
   - respuestas escribiéndose letra a letra; un toque las completa;
   - el retrato se tiñe o tiembla según el estado; la consola muestra `[Estado]` si falta la etiqueta;
   - las pistas aparecen **después** de la respuesta, con giro de carta; si salen dos a la vez, se acumulan;
   - selector "Mostrar prueba" y contradicción (destello ámbar y sacudida del HUD);
   - libreta con pistas, contradicciones y sospechosos con su estado;
   - "Acusar ya", luego **Volver**, luego acusar de verdad, y comprobar el final y el epílogo.
5. **Guardado.** A mitad de partida para Play y vuelve a pulsarlo: "Continuar" debe devolverte al día, las preguntas, las conversaciones y las pistas. "Nueva partida" descarta el guardado.
6. **Ajustes** (desde el menú): volumen, velocidad del texto, "Reducir animaciones" (todo quieto e instantáneo), y "Reiniciar partida" con confirmación.
7. **Historias 2 y 3.** Prueba `Caso2C` (Ruiz culpable) y `Caso3B` (Paula viva) como mínimo.
8. **Botones Menú y Reiniciar del resultado.** Menú vuelve al menú; Reiniciar empieza una partida nueva.
9. **Arte.** Suelta cualquier PNG como `Assets/Art/Portraits/daniel_tranquilo.png` y comprueba que aparece sin tocar nada más.
10. **Tema.** Selecciona `Assets/Resources/Theme.asset`, cambia `accent` y vuelve a pulsar Play: debe cambiar el ámbar en todo el juego.
11. **Distribución móvil.** Si algo queda fuera de sitio, se desactiva con `applyMobileLayout` en `InterrogationUI`, y las medidas están en el tema ("Distribución vertical"). Aquí es donde haremos juntos el ajuste fino.

---

## Hecho, tarea a tarea

### 1. Los 5 pendientes menores (`feature/historias-etapa1`)
- **Orden de los avisos:** una `NoticeQueue` retiene los avisos de pista, contradicción y sospechoso nuevo hasta mostrar la respuesta.
- **Varias pistas en un turno:** se acumulan en el mismo aviso, que dura más.
- **Excepción que bloqueaba el campo de texto:** `AskQuestion` captura cualquier error, reactiva la entrada y no gasta la pregunta.
- **Contradicción que citaba una mentira retirada:** las anclas de la mentira exigen la afirmación falsa en sí. Cada culpable tiene ejemplos de admisión parcial que no deben contar, y un test lo comprueba.
- **`GameManager` sin darse de baja:** se da de baja de los eventos en `OnDestroy`.

### 2. Historias 2 y 3
- **"Noche de Verano"**: 2A Marcos, 2B Andrés, 2C Ruiz.
- **"Humo y Silencio"**: 3A Javier, 3B Lucía (Paula viva), 3C Encarna.
- Todo según el diseño aprobado: fichas, 5-6 pistas, mentira con admisiones, versión B, partes de la mañana y epílogos.

### 3. Calibración contra qwen (informes en `Logs/`)

| Informe | Resultado |
|---|---|
| `calibracion-historias-2-3-ronda1.md` | 23/31 pistas ≥ 2/3 |
| `calibracion-historias-2-3-ronda2.md` | **27/31**, detección media 85 %, variedad 92 % |
| `calibracion-historias-2-3-ronda3.md` | las 4 que fallaban: `2B_furgoneta` pasa a 7/10; `2C_gps`, `3A_audios` y `3C_fotos` se quedan en 6/10 |
| `calibracion-temperatura-0.6.md` / `-0.4.md` | comparación de temperatura (abajo) |
| `calibracion-subconjunto-con-estados.md` | 12 pistas de las 3 historias con la etiqueta de estado: **10/12, 86 %** |

**Resistentes a la variación del modelo:** `2C_gps`, `3A_audios` y `3C_fotos` se quedan en 6/10, justo por debajo de 2/3. Ninguna es necesaria para el final BUENO de su variante; el test de resolubilidad lo garantiza sin ellas. `1B_cena` (Daniel) sigue siendo difícil de arrancar, como pediste; no lo he tocado.

**Sonda de precisión:** entre el 10 % y el 14 % de las respuestas a preguntas ajenas al caso revelan alguna pista. Son revelaciones espontáneas coherentes con el personaje (Maruxa y Rosario lo cuentan todo; los hijos sacan su coartada), no falsos positivos del detector.

**Lo que mejoró la detección** (y conviene mantener al escribir pistas nuevas):
- Hechos redactados como **cita en primera persona** («Mamá me dejó…»): `3B_noche` pasó de 5/10 a 10/10. Antes el modelo copiaba "Tu madre te dejó…".
- La ficha pide contar los hechos "con tus palabras, en primera persona".
- Los secretos se confiesan tras **una** insistencia.
- Las métricas de naturalidad ahora marcan inglés mezclado, fragmentos de la ficha recitados y segunda persona copiada.

**Temperatura: se mantiene 0.6.** Sobre las mismas 12 pistas, con 10 muestras cada una:

| | 0.6 | 0.4 |
|---|---|---|
| Pistas ≥ 2/3 | **9/12** | 6/12 |
| Detección media | **73 %** | 68 % |
| Variedad de respuestas | **88 %** | 83 % |
| Violaciones de estilo | 2 | 3 |

A 0.4 las respuestas son más repetitivas y, contra lo esperado, detectan peor: el modelo resume y omite detalles.

### 4. Resolubilidad
`ResolvabilityTests` simula a un jugador sensato en las **9 variantes** con el análisis real de respuestas, los desbloqueos y el presupuesto de 35 preguntas: **9/9 llegan a BUENO**. Comprobé que el test sabe fallar: con un umbral imposible se pone en rojo en las 9.

### 5. Estados emocionales (`feature/estados-emocionales`)
- **La etiqueta:** el modelo termina cada respuesta con `[ESTADO: tranquilo|nervioso|asustado|enfadado|triste]`.
  - `EmotionParser` la lee (tolera mayúsculas, espacios y femenino) y la quita antes de mostrar y de detectar pistas.
  - El historial la conserva, para que el modelo mantenga el formato.
- **Calibración** (`Logs/calibracion-estados-ronda2.md`), 144 respuestas: **bien formada 98 %, coherente 84 %**. Cumple el objetivo de 8/10.
  - Punto débil: con preguntas sobre la víctima, 16/36 son coherentes; muchas salen "tranquilo". Nombrar a la víctima en la guía subió "triste" de 10 a 15.
- **Reacción visual** por corrutinas: tinte, temblor continuo (nervioso, asustado), sacudida única (enfadado) y velocidad de escritura por estado. Todo con valores del tema.
- **Retratos por estado:** `Assets/Art/Portraits/<personaje>_<estado>.png`. Si falta el estado se usa uno cercano, luego el tranquilo, luego el retrato antiguo y, en último caso, un color plano.
- **`ART-NEEDED.md`**: 36 retratos (12 personajes × 3 estados), con tamaños y descripción.

### 6. QA
- Tests de flujo con un proveedor LLM falso: petición fallida, texto limpio, estado, historial acotado, respuesta vacía y partida sin caso.
- Revisión por código de los casos límite:
  - **Acusar el día 1:** funciona, y no se puede acusar dos veces.
  - **Sin preguntas:** la UI avisa, no envía y deja la entrada activa.
  - **Respuestas larguísimas:** están acotadas por `maxTokens` 250; el efecto de escritura y el scroll funcionan igual.
  - **Ollama caído:** la pregunta no se descuenta y vuelve al campo de texto.
  - **Cambiar de sospechoso con una petición en vuelo:** el selector está bloqueado; y si llegara una respuesta de otro sospechoso, va a su conversación.

### 7. UI móvil (`feature/ui-movil`)
- **Canvas:** `CanvasScaler` a 1080×1920 con match 0.5. **Orientación solo vertical** en Project Settings.
- **Tema central** (`Assets/Resources/Theme.asset`): todos los colores (fondos, paneles, chat, botones, texto, estados), la tipografía (título 72, encabezado 52, cuerpo 40, secundario 32 px; mínimo legible 30), las duraciones, las medidas de la distribución, la gradación del arte y la profundidad. **Cambiar la paleta es editar ese asset.** Sin asset se usan los valores por defecto del código.
- **`ThemeApplier`:** aplica el tema a toda la UI por nombre y tipo, y el componente `ThemeRole` permite forzar el papel de un elemento. No tiñe las ilustraciones.
- **Distribución para una mano** (`InterrogationLayout`): HUD compacto arriba, retrato, chat en el centro, y abajo sospechoso, prueba, pregunta y enviar. Se aplica por código; la escena no se modifica y se puede desactivar.
- **Fallos arreglados:**
  - botón Volver en la acusación;
  - botón Menú del resultado (antes no hacía nada);
  - Reiniciar empieza una partida nueva;
  - las conversaciones ya no se mezclan (`ConversationStore`, con tests); el cambio de día se anota en todas.
- **Ajustes:** volumen, velocidad del texto, reducir animaciones y reiniciar con confirmación. Se guardan en `PlayerPrefs`.

### 8. Transiciones y "juice"
- **Decisión: corrutinas, sin DOTween**, para no añadir una dependencia externa y tener las duraciones en el tema.
- Fundido entre paneles, efecto de escritura con toque para saltar, giro de carta de las pistas, y destello más sacudida en las contradicciones.
- *Nota:* el chat es un único texto, no burbujas separadas; la "aparición suave" es el efecto de escritura de cada respuesta. Las burbujas reales requieren rehacer el chat como lista y conviene decidirlo viéndolo.

### 9. Pantalla de inicio de caso
Parte del caso (lugar, víctima, situación) sacado de los datos de cada historia (`CaseBriefing`), con efecto de escritura. Incluye fondo y cabecera por historia, con color plano mientras no haya arte.

### 10. Libreta
Pistas (nombre y resumen), contradicciones y sospechosos desbloqueados, con su estado emocional y su pista de descarte si la tienen. Se abre con el botón de pistas y se refresca con cada cambio.

### 11. Guardado
- **Qué se guarda:** `partida.json` en `persistentDataPath`, con variante, día, preguntas, desbloqueos, pistas, pruebas mostradas, mentira dicha, historiales, estados y conversaciones. Las contradicciones se recalculan al cargar.
- **Cuándo:** se guarda tras cada respuesta y cada cambio de día; se borra al acusar y al reiniciar.
- **Robustez:** un guardado corrupto o incoherente se ignora sin excepción y se empieza de cero. La escritura es atómica. Hay tests para todo ello.

### 12. Arte
- `ART-NEEDED.md` ampliado: fondo del menú, cabecera e intro por historia, y 5 iconos, cada uno con ruta, tamaño, formato y descripción.
- Un test obliga a que cada hueco de arte del código esté documentado ahí.
- Si falta una imagen, se muestra un color plano. Al soltarla en su ruta aparece sola; el catálogo para las builds se regenera automáticamente.

### 13. Rendimiento
- **Ollama en esta máquina:** unos 100 tokens/s. La ficha más el historial rondan 530–900 tokens de entrada. Ollama reutiliza el prefijo del prompt (primer procesado 271 ms, siguientes 10 ms), por eso la ficha pone al final lo que cambia (día, pruebas mostradas).
- **UI:** el retrato, el texto del chat y el HUD, que se animan cada fotograma, van en **Canvas anidados** para no reconstruir todo el Canvas.
- Los efectos por fotograma no reservan memoria: un test lo verifica sobre 10 000 fotogramas simulados.

### 14. Auditoría de imágenes
Detalle completo en `docs/IMAGE-AUDIT.md`.
- **Problemas:**
  - los 7 retratos son pixel art de cuerpo entero, muy saturados y compartidos entre personajes distintos;
  - todos los `.gif.png` son en realidad PNG estáticos;
  - varias imágenes pesan más de 2 MB.
- **Corregido sin tocar los originales:** shader de desaturación y tinte (parámetros en el tema), filtrado nítido para el pixel art, y fondos oscurecidos.
- **Fallo evitado:** el tema habría pintado de gris el fondo del menú.
- **Recomendación principal:** rehacer los retratos (ver `ART-NEEDED.md`).

### 15. Pseudo-3D
- **Retratos:** respiración de 1,2 % de escala cada 4,5 s, e inclinación tipo tarjeta al tocarlos. El temblor emocional se suma por encima.
- **Fondos:** parallax de 16 px con el acelerómetro y, sin sensor, con el puntero.
- **Pistas y libreta:** giro de carta al aparecer.
- Todo se ajusta en el tema y se apaga con "Reducir animaciones". Hay `ProfilerMarker` para medirlo en el dispositivo.

---

## Decisiones que tomé por mi cuenta

1. **Temperatura 0.6** (datos arriba).
2. **Límite de la ficha subido a 460 palabras**: la regla fija de la etiqueta de estado añade unas 50.
3. **Hechos en cita de primera persona** en las pistas que fallaban. Mejora mucho la detección a cambio de respuestas más literales en esas pistas.
4. **Calibración en un worktree aparte**: mis ediciones a medio escribir abortaron una ejecución (la primera a 0.4), que descarté y repetí.
5. **Tres retratos por personaje** en lugar de cinco (36 imágenes, no 60); los estados que faltan se cubren con sustitutos, tinte y temblor.
6. **Guardar tras cada respuesta** (no solo al cambiar de día), para no perder progreso si se cierra la app.
7. **Distribución móvil aplicada por código**, sin reescribir la escena, y desactivable.
8. **Sin DOTween.**
9. **Criterio de coherencia de la etiqueta fijado antes de medir.** Con preguntas sobre la víctima, "asustado" cuenta como incoherente; no lo cambié después de ver los datos.

## Pendiente o para decidir juntos

- **Ajuste visual fino** de la distribución, los tamaños y los colores (necesita verse en pantalla).
- **Chat como burbujas reales** (lista de elementos) en lugar de un único texto.
- **Arte nuevo:** retratos, fondos e iconos (`ART-NEEDED.md`), y las recomendaciones de importación de `docs/IMAGE-AUDIT.md`.
- **Tres pistas en 6/10** (`2C_gps`, `3A_audios`, `3C_fotos`): se pueden reforzar si en la partida se atascan.
- **No hay acceso a Ajustes durante la partida** (solo desde el menú).
- **Iconos del HUD** para días y preguntas: el hueco de arte existe, pero el HUD es un único texto.

## Revisión final
(Se completa al terminar la revisión independiente.)
