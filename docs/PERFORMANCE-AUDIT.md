# Auditoría de rendimiento (sesión extra del día 3, 30-09-2026)

*Solo análisis: no se ha cambiado código del juego.* Se pidió con la skill **finecomb**, pero no está disponible en
esta sesión ("Unknown skill"); la auditoría se ha hecho a mano con el mismo enfoque (línea a línea en los caminos
calientes) y **midiendo**, no suponiendo: un banco de pruebas temporal en EditMode (no subido) sobre datos reales
de las historias, y los contadores de Ollama.

## 1. Línea de base

**Lo primero que hay que saber:** ninguno de los cuatro archivos (`AIConversationManager`, `PromptBuilder`,
`ClueDetector`, `GameManager`) tiene `Update`: no trabajan en cada fotograma, sino **una vez por pregunta** (y al
cambiar de día, guardar o cargar). En reposo el juego ya genera **≤ 0 B de basura por fotograma** y el coste de GPU
del relieve y el post-proceso es ~0,02-0,07 ms (`docs/RENDIMIENTO.md`). Por eso la ganancia en "ms por fotograma"
de optimizar estos archivos es **cero en reposo**; lo que importa es (a) la **espera** del jugador por la respuesta
y (b) el **fotograma en que llega** la respuesta.

### Una pregunta, de principio a fin (PC portátil con RTX 5070 Ti, en el editor)

| Paso | Coste medido | Veces por pregunta |
|---|---|---|
| Modelo (qwen2.5 7B en Ollama, local) | **640 ms** (medido; el bot da media 0,9 s y p90 1,2-1,3 s) | 1 (hasta 3 con reintentos) |
| `PromptBuilder.Build` (ficha: 3 765 caracteres, 1 219 tokens con la pregunta) | 11,5 µs · ~20 KB | 1 |
| `EmotionParser.Parse` | 38,7 µs | ~4 (se vuelve a analizar el mismo texto) |
| `TimeCheck.Unknown` (ficha + 16 mensajes = 6,6 KB de contexto) | **547 µs** | 1-2 |
| `TurnAnalyzer.Analyze` (normaliza + 6 pistas + mentira + menciones) | 113 µs | 1 |
| `ClueDetector.Normalize` (179 caracteres) | 65 µs | varias |
| `ClueDetector.Evaluate` × 6 pistas | 182 µs | dentro de Analyze |
| `Notebook.Format` (peor caso: todas las pistas, versiones, notas y partes; 3 359 caracteres) | 38,7 µs · ~28 KB | 1 |
| `SaveSystem.Serialize` (partida completa, 35 turnos) | 320 µs · ~86 KB · **41,6 KB** de JSON | 1 |
| Escribir ese JSON (SSD del PC; temporal + mover, atómico) | 440 µs | 1 |
| `SaveSystem.TryDeserialize` (al continuar) | 375 µs | al cargar |

**CPU del juego por pregunta ≈ 1,6-2,2 ms en este PC** frente a **640-900 ms** del modelo: el 0,2 % del tiempo.
En un móvil de gama media la CPU es ~5-8 veces más lenta (estimación, sin medir en un dispositivo): **8-18 ms en
el fotograma en que llega la respuesta**, que puede costar **un fotograma perdido** (16,7 ms a 60 fps) en un móvil
modesto. Basura por pregunta: ~150-200 KB (ficha, libreta, guardado, textos); con el GC incremental activo
(`gcIncremental: 1`) no produce tirones medibles en el editor.

*Nota de método:* `GC.GetAllocatedBytesForCurrentThread` devuelve 0 en el Mono de Unity; la memoria se midió con el
crecimiento del montón (`Profiler.GetMonoUsedSizeLong`) en 50 llamadas, que va por páginas: las asignaciones
pequeñas salen como 0 y las cifras de KB son aproximadas.

## 2. Hallazgos por archivo

**AIConversationManager.cs**
- Sin *streaming*: el jugador espera la respuesta entera (el proveedor pide `stream = false`). Es, con diferencia,
  lo que más se nota. `keep_alive` y la precarga del modelo ya están bien.
- `EmotionParser.Parse(result.Text)` se llama 3-4 veces sobre el mismo texto (repetición, idioma, horas, final).
- La comprobación de horas concatena la ficha y todo el historial en una cadena nueva en cada respuesta.
- El historial se recorta a 16 mensajes (`ConversationWindow.Last`) y la ficha se reconstruye en cada pregunta: bien
  para la memoria; la ficha cambia al final (pruebas mostradas, lo ya contado, el día), así que el principio es
  estable y la **caché de prefijo** (Ollama, llama.cpp, *prompt caching* de Claude) funciona.

**PromptBuilder.cs** — no genera JSON: texto con un único `StringBuilder`; 11,5 µs. Sin nada que ganar. Lo
importante es el **tamaño** (1 219 tokens): en un modelo en el móvil, procesar la ficha es lo más lento (ver
ANDROID-OPTIONS.md).

**ClueDetector.cs**
- `Evaluate` normaliza cada ancla (dato fijo de la historia) en **cada** llamada: `Normalize(anchor)` dentro del
  bucle. Con 6-7 pistas y ~10 anclas cada una, ~60 normalizaciones por respuesta.
- `Normalize` descompone a FormD, filtra carácter a carácter y pasa dos expresiones regulares: 65 µs por 179
  caracteres.
- `IsNegated` crea dos `Substring` y un `Split` por cada aparición de un ancla.
- Las expresiones regulares no llevan `RegexOptions.Compiled`; en Android (IL2CPP) no aportaría (se interpreta
  igual), así que no es una mejora a perseguir.

**TimeCheck.cs** (llamado desde AIConversationManager) — el más caro: pasa dos expresiones regulares por **todo** el
contexto conocido (6,6 KB) en cada respuesta, **aunque la respuesta no tenga ninguna hora**.

**GameManager.cs**
- Guarda después de cada respuesta, nota y fin de día (~40 escrituras por caso): correcto para no perder partida;
  la escritura es atómica (temporal + mover) pero **síncrona en el hilo principal**, y el JSON va con
  `prettyPrint` (41,6 KB frente a 24,3 KB compacto: +71 %).
- `RefreshNotebook` rehace la libreta entera (39 µs) en cada cambio; TMP no vuelve a maquetar si el texto no cambia,
  y con el panel cerrado aplaza la malla. El coste real de maquetar 3 400 caracteres con enlaces en un móvil **no
  está medido**.
- Reiniciar y volver al menú recargan la escena completa (la interfaz se construye por código): **no medido** en
  móvil; candidato a revisar con el Profiler en un dispositivo.

## 3. Mejoras de alto impacto (para la siguiente sesión, por prioridad)

| # | Mejora | Ganancia estimada | Riesgo / coste |
|---|---|---|---|
| 1 | **Streaming de respuestas** (el texto empieza a escribirse con la primera palabra; la máquina de escribir ya existe) | Espera percibida: de 0,6-0,9 s a **~0,1-0,2 s** con Ollama local; en red o en un móvil, **segundos** | Toca la capa de proveedores (**decisión tuya**); la etiqueta de estado va al final y hay que retener el texto hasta verla o limpiarla al vuelo; los reintentos (repetición, idioma, horas) necesitan la respuesta entera → mostrar en streaming solo cuando no haya reintento o aceptar un "corte" raro. 1-2 días |
| 2 | **`TimeCheck` barato**: salir al instante si la respuesta no tiene ninguna hora; calcular las horas de la ficha una vez por sospechoso y día (caché) | 0,5-1,1 ms → **< 0,05 ms** por respuesta en PC (móvil: ~3-8 ms → < 0,5 ms) | Muy bajo; tests existentes de horas inventadas. 1-2 h |
| 3 | **Anclas normalizadas una sola vez** (caché por pista al cargar la historia) y `Parse` una sola vez por respuesta | `Evaluate` 182 → ~50 µs; `Parse` ×4 → ×1 (−115 µs) | Bajo; la calibración y los tests de detección lo cubren. 2-3 h |
| 4 | **Guardado compacto y fuera del hilo principal** (sin `prettyPrint` en la build; escribir en una tarea con la misma escritura atómica) | −42 % de tamaño, −30-40 % de serialización; el fotograma de la respuesta deja de esperar al disco (en móviles con flash lenta, picos de 5-30 ms **sin medir**) | Bajo-medio: cuidar que dos guardados no se crucen (cola de uno). 2-4 h |
| 5 | **Caché de prefijo en Claude** (`cache_control` en la ficha) si se elige la opción B de Android | ~−40 % del coste de entrada por caso (≈ 0,065 → 0,04 $) y algo menos de latencia | Capa de proveedores (decisión); sin efecto con Ollama |

Juntas, 2-4 dejan la CPU del juego por respuesta en ~0,3-0,5 ms en PC (≈ 2-4 ms estimados en un móvil medio): sin
fotogramas perdidos al llegar la respuesta. La 1 es la única que cambia lo que siente el jugador.

## 4. Prioridad para la siguiente sesión
1. **Medir en un Android real** antes de optimizar a ciegas: Profiler conectado al móvil, fotograma de la respuesta,
   guardado, recarga de escena y maquetación de la libreta (lo que aquí queda "sin medir").
2. Mejoras 2 y 3 (baratas, sin riesgo, con TDD y las calibraciones como red).
3. Mejora 4.
4. Mejora 1 (y 5) cuando se decida el proveedor de Android (ANDROID-OPTIONS.md): el streaming vale el doble si las
   respuestas vienen por red.

## 5. Después (sesión B, 01-10-2026, rama `feature/mejoras-seguras`)
Mejoras 2, 3 y 4 hechas con test primero. Antes y después medidos **en la misma ejecución** (`PerfBenchmark`,
Explicit; "antes" fuerza el camino antiguo: sin cachés, contexto concatenado, escritura síncrona con sangría), en este
PC y en el editor:

| Operación | Antes | Después |
|---|---|---|
| `TimeCheck.Unknown`, respuesta con hora (4 507 caracteres de contexto) | 402,9 µs | 15,0 µs |
| `TimeCheck.Unknown`, respuesta sin hora (la mayoría) | 436,8 µs | 3,6 µs |
| `EmotionParser.Parse` × 4 sobre la misma respuesta | 125,6 µs | 0,4 µs |
| `ClueDetector.Evaluate` × 6 pistas | 200,0 µs | 17,2 µs |
| Guardado: JSON | 41,8 KB | 24,0 KB |
| Guardado: tiempo en el hilo principal | 2 923 µs | 721 µs |

La CPU del juego por pregunta baja de ~1,6-2,2 ms a ~0,4-1,0 ms (estimación: la línea de base de la sección 1 menos los ahorros de esta tabla y la escritura que pasa a otro hilo; sin medir en un móvil).
Calibración completa sin regresiones atribuibles: pistas 43/48 (main) y 41/48 (rama) con 3 intentos, premisas 2 % y
4 %, estados bien formados 97 % y 97 %, bot 17/18 y 15/18 (umbral 14; ±2 es ruido). El detector decide igual que
antes (`NegationEquivalenceTests`). Mejoras 1 y 5 (streaming) siguen fuera: tocan la capa de proveedores.
