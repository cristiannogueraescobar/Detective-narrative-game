**Mira primero:** `docs/art/retratos_sesion_b.jpg` (los 12 retratos del juego al cierre; Javier sigue con el derivado de Daniel) y después `docs/art/javier/evolucion_sesion_b.jpg` (todo lo que se probó para Javier, de izquierda a derecha).

# Informe — Sesión B: sesión autónoma de 8 horas (01-10-2026, 16:18 → 00:18)

## Resumen en 5 líneas
1. **Javier no aprueba.** 12 pruebas ciegas, 34 de 36 subagentes señalan al generado; lo mejor, 2 de 3 en las rondas 9 y 10 (los dos primeros fallos de la sesión). No se integra nada: Javier sigue con el derivado de Daniel.
2. **El bloque 2 se saltó**, como mandaban las reglas si Javier no pasaba.
3. **Bloque 3, rendimiento: hecho** (mejoras 2, 3 y 4 con test primero). En la misma ejecución: TimeCheck 437 → 3,6 µs (respuesta sin hora), Parse ×4 126 → 0,4 µs, Evaluate ×6 200 → 17 µs, guardado 41,8 → 24,0 KB y 2,9 → 0,7 ms en el hilo principal. Sin regresiones atribuibles.
4. **Bloque 3, segundas vías de 1B_cena y 1C_llamada: probadas y revertidas.** Con 3 intentos parecían subir (33 → 56 %); con 10 no (20 % y 37 % frente a 35 % y 53 % en main). Daniel niega la aventura aunque le pongan delante al testigo.
5. Ramas sin fusionar: **`feature/mejoras-seguras`** (lista para fusionar) y **`feature/retratos-javier`** (herramientas de arte, informes y este documento). `main` sin tocar.

## Tabla por bloque

| Bloque | HECHO CUANDO | Resultado | Rama y commits |
|---|---|---|---|
| 1. Retrato de Javier | Prueba ciega: los subagentes no lo distinguen de los originales, y no se parece a Marcos | **NO aprueba** (cerrado a las 18:00; trabajo extra 19:50-20:37, tampoco) | `feature/retratos-javier`: `b23bac2` … `e05015d` (ronda 12: `cfd95b9`) |
| 2. (dependía de Javier) | — | **Saltado** por regla | — |
| 3a. Mejoras de rendimiento 2-4 | Tests en verde, antes/después medido, calibración sin bajar | ✓ (+ revisión independiente: 1 Importante arreglado) | `feature/mejoras-seguras`: `560e71e`, `2e6b103`, `99d79a4`, `a86f6e1`, `69470d7`, `5f3f9c4`, `2aa7841` |
| 3b. Segundas vías 1B_cena / 1C_llamada | Que suban de forma medible | ✗ No suben; **revertido** | `b92d6c2` → revert `848a690` |
| 3c. Calibración completa + bot | pistas ≥80 %, premisas ≤5 %, estados ≥95 %, bot ≥14/18 | ✓ en la rama (82 %, 4 %, 97 %, 15/18) | Logs en `../dng-*/Logs` (no versionados) |

Tests al cerrar `feature/mejoras-seguras` (tras `2aa7841`): **EditMode 756/757 (el que no corre es `PerfBenchmark`, Explicit), 0 fallos; PlayMode 64 pasan, 0 fallos** (24 ignorados, como siempre). Tests de herramientas (`Tools/tests`): **33/33**.

## Bloque 1: Javier, por personaje

| Personaje | ¿Aprueba? | Hoja de comparación | Prueba ciega | Rama |
|---|---|---|---|---|
| Javier | **No** | `docs/art/javier/evolucion_sesion_b.jpg` | 12 rondas × 3 subagentes: 34/36 lo señalan. Parecido con Marcos: falla en la ronda 4 ("la misma persona"), pasa desde la 5 ("personas distintas", confianza alta) | `feature/retratos-javier` |

**Qué se probó, en orden (cada fila con su motivo de la prueba ciega anterior):**

| Ronda | Candidato | Cambio | Resultado | Motivo común de los que acertaron |
|---|---|---|---|---|
| 1-3 | 6106, 6101, 7002 | img2img desde paint-over de Marcos, cel, IP-Adapter | 3/3 cada una | textura ruidosa, cara realista, paleta apagada |
| 4 | 8405 | **LoRA de estilo propio** (6 originales, 800 pasos) | 3/3, confianza media; "misma persona que Marcos" | ojos pequeños, grano, piel saturada |
| 5 | 8603 | Prompt de ojos grandes, lienzo gris en vez de Marcos | 3/3; parecido **pasa** | **piel amarillo limón** (fallo mío, ver abajo) |
| 6 | 8611 | Máscara de piel arreglada | 3/3, dos dudan | cara, grano |
| 7 | 8611 aplanado | Aplanado fuerte en toda la figura | 3/3 | la cara empeora ("manchas") |
| 8 | 8611 + cara | **LoRA v2** (con recortes de cara) + **facefix** (repinta la cabeza a 1024) | 3/3, confianzas media/baja | "cabeza pequeña, ~1/7" |
| **9** | **8702** | Cabeza del esqueleto 0,20 (valor por defecto de `pose.py`) → 0,24 | **2/3** | grano, piel naranja |
| **10** | **8700** | Ropa aplanada fuera de la cabeza | **2/3** (agrupa a Javier con Marcos y Lucía) | "contorno negro grueso y uniforme" |
| 11 | 8700 sin contorno | Sin el contorno reforzado | 3/3 | "borde blando": el contorno ayuda |
| 12 | 8700 selout | Contorno coloreado (oscuro de cada zona) | 3/3 | "contorno blando y roto": el casi negro era mejor |

**Por qué no llega, en una frase:** lo que aún lo delata es de dibujo, no de color: los originales tienen la cara **dibujada con líneas** (ojos con blanco y pupila) y la ropa en dos o tres tonos limpios, y el generador pinta la cara con manchas y la ropa con grano. (El contorno coloreado se probó en la ronda 12 y salió peor.)

**Hallazgo de la revisión de herramientas (20:55):** en los brutos del LoRA v2 la máscara de piel apenas ve la piel (0,3-0,6 % de la cabeza: saturación > 0,8, por encima de su límite). Así `colour.correct` no la corrige y `saturate` la satura como si fuera ropa: es muy probablemente el "naranja encendido" que citan las rondas 9-12. Primer arreglo para una próxima sesión: subir el límite de saturación de `skin_mask` (con test) y repetir la ronda 10.

**Siguiente paso, si se quiere seguir por aquí:** un LoRA con más recortes de cara y más pasos, y limpiar la ropa a mano o con una paleta fija por zona. **Mi recomendación honesta: encargarlo a un artista con `docs/art/javier/BRIEF.md`**: el generador ha pasado de "otro juego" a "casi", pero 12 rondas sin aprobar dicen que el último tramo es de oficio.

Todo lo pesado está fuera del repositorio: modelos en `C:\AI\hf-cache`, LoRA en `C:\AI\lora\jvstyle` y `jvstyle2` (entrenados con arte del propio juego; script oficial de diffusers, Apache-2.0), salidas en `C:\AI\portrait-gen\out`.

## Bloque 3: antes y después

**Rendimiento** (`PerfBenchmark`, Explicit, la misma ejecución; "antes" fuerza el camino antiguo). Escrito también en `docs/PERFORMANCE-AUDIT.md` §5:

| Operación | Antes | Después |
|---|---|---|
| `TimeCheck.Unknown`, respuesta con hora (4 507 caracteres de contexto) | 402,9 µs | 15,0 µs |
| `TimeCheck.Unknown`, respuesta sin hora (la mayoría) | 436,8 µs | 3,6 µs |
| `EmotionParser.Parse` × 4 sobre la misma respuesta | 125,6 µs | 0,4 µs |
| `ClueDetector.Evaluate` × 6 pistas | 200,0 µs | 17,2 µs |
| Guardado: JSON | 41,8 KB | 24,0 KB |
| Guardado: tiempo en el hilo principal | 2 923 µs | 721 µs |

CPU por pregunta estimada: de ~1,6-2,2 ms a ~0,4-1,0 ms (sin medir en un móvil). El guardado antiguo con sangría se sigue leyendo (test). La escritura va en otro hilo, en una cola de uno: nunca se cruzan, una antigua no pisa a una nueva y un borrado no lo resucita una escritura atrasada (tests).

**Calidad** (qwen2.5:7b-instruct, worktrees de main y de la rama, en serie, mismo umbral; bot con semilla 1919, 2 partidas × 9 variantes):

| Medida | main | rama | Umbral |
|---|---|---|---|
| Pistas ≥ 2/3 (3 intentos) | 43/48, media 86 % | 41/48, media 82 % | ≥ 80 % ✓ |
| Premisas falsas aceptadas | 2/72 (2 %) | 3/72 (4 %) | ≤ 5 % ✓ |
| Estados bien formados | 140/144 (97 %) | 140/144 (97 %) | ≥ 95 % ✓ |
| Bot: culpable acertado | 17/18 | 15/18 | ≥ 14/18 ✓ (±2 es ruido) |

Las pistas que cambiaron de lado se midieron otra vez con **10 intentos**, en los dos:

| Pista | main | rama |
|---|---|---|
| 1B_cena (con la segunda vía en la rama) | 7/20 = 35 % | 6/30 = 20 % |
| 1C_llamada (ídem) | 16/30 = 53 % | 11/30 = 37 % |
| 1A_papeles | 24/30 = 80 % | 18/30 = 60 % |
| 1C_pantalla | 10/20 = 50 % | 8/20 = 40 % |
| 2C_grabacion | 10/20 = 50 % | 9/20 = 45 % |
| 3C_bar | 17/20 = 85 % | 18/20 = 90 % |

1A_papeles no tiene ningún dato cambiado. Para descartar mi código, `NegationEquivalenceTests` compara el detector nuevo con el antiguo (copiado en el test) para todas las anclas de todas las variantes sobre todos los textos de las historias: **deciden igual**. Es variación del modelo. Las segundas vías se revirtieron; la rama, tal como queda, no cambia ningún dato de las historias.

### Revisión independiente de `feature/mejoras-seguras` (20:40, subagente sin contexto, solo lectura)
Sin defectos críticos. **Importante, arreglado con test primero (`2aa7841`):** al pasar el guardado a otro hilo, si
Android mata la app en segundo plano justo después de una respuesta podía perderse el último guardado (Android no suele
lanzar `Application.quitting`). Ahora `OnApplicationPause(true)` y `OnApplicationQuit` vacían la cola
(`AlPausarLaAppElUltimoGuardadoLlegaAlDisco`, en rojo antes del arreglo). Menores arreglados en el mismo commit: la
guarda de versión no tenía test propio (`UnGuardadoAtrasadoNoSeEscribeSiHayOtroMasNuevo`), el test del hilo podía pasar
con un valor de otro test, el TearDown no vaciaba la cola, y la caché de TimeCheck queda documentada como "solo hilo
principal" (comprobado: hoy no se llama desde otro). Revisado y correcto: la cadena de escrituras no se rompe ni se
bloquea, el orden y el borrado, la equivalencia de TimeCheck y de IsNegated, y la caché de EmotionParser.

### Revisión independiente de las herramientas de retratos (20:52)
Sin defectos importantes. Arreglados con test primero: `colour.correct` exige al menos 200 píxeles de piel (con 3 movía
toda la figura) y `facefix` ya no repinta el fondo en silencio si el recuadro no tiene tono de piel (umbral medido:
cabezas reales 15,6-20,8 %, fondo 1,2 %; mi primer umbral rechazaba cabezas reales y lo corregí antes de subirlo).
**Menores aplazados:** la franja de la cabeza es un 22 % fijo aunque `--head-ratio` cambie; `finish()` no expone
`selout`; `lora_dataset.head_box` no se recorta al borde; `--jv`/`--jv2` sin `--javier` se ignoran sin avisar.

## Lo que salió mal y corregí

1. **Horas estimadas en vez de leídas, dos veces.** En el bloque 1 escribí 16:45-17:38 cuando el reloj decía 16:25-16:40 (y por eso creí que se acababa el tiempo del bloque); después, 18:03 y 18:06 a las 18:00. Corregido en el NIGHT-LOG las dos veces (`a9691b7`, `032922c`); desde entonces cada hora sale de `date`.
2. **Regla de la GPU incumplida sin verlo.** La suite PlayMode carga qwen en Ollama (5,6 GB) y la lancé a las 16:50 mientras entrenaba el LoRA. Lo vi a las 17:16 con `ollama ps`; confirmado a las 19:47 (al acabar la suite estaba cargado otra vez). Desde entonces, `ollama ps` y `ollama stop` antes de cada tarea pesada.
3. **Diagnóstico equivocado de las siluetas negras.** Lo atribuí a un fondo oscuro (17:20) y cambié el prompt; la causa real era `colour.correct` con la máscara de piel vacía: media NaN → negro. Arreglado con test primero (`0f03a3e`). Le siguió el mismo patrón con la piel amarilla (máscara de 8-40°, la cara estaba a 56°; `e7dfeb1` sube el límite a 60° solo en la franja de la cabeza).
4. **Segunda vía como séptima pista.** Mi primer intento añadía una pista a 1B, que ya tenía 6, contra el diseño aprobado de 5-6. El test `4-6 pistas` lo paró; lo convertí en testigo.
5. **Casi doy por buena una mejora que era ruido** (33 → 56 % con 3 intentos). La confirmación con 10 intentos la desmintió y lo revertí (`848a690`).
6. **Estimación de CPU mal hecha** en PERFORMANCE-AUDIT (0,5-0,8 ms): rehecha con la derivación escrita, 0,4-1,0 ms (`5f3f9c4`).
7. Antes de la compactación del contexto: un heredoc convirtió `\n` en saltos de línea reales dentro de código (TimeCheckTests) y mezcló CRLF; lo arreglé reescribiendo con Write. Incumplí mi propia regla de no editar código con heredocs.
8. La hoja de evolución salió primero sin el candidato 7002 (ruta equivocada); rehecha.

## Decisiones para Cristian

1. **Fusionar `feature/mejoras-seguras` a `main` primero.** 9 commits (revisado por un subagente independiente), 16 archivos (código: TimeCheck, ClueDetector, EmotionParser, SaveSystem, AIConversationManager, GameManager; tests; docs). Ningún dato de historias cambia. Se deshace con `git revert -m 1 <merge>`.
2. **Después, `feature/retratos-javier`**, si quieres las herramientas de arte (`Tools/portrait_gen`, tests), los informes, el NIGHT-LOG y este documento. Toca 6 archivos del juego, todos de esta mañana, antes de la sesión B (`8261f81` encuadre del arte nuevo `PortraitCrops`, `e01f67d` filtro de importación, y sus tests). Ningún retrato del juego cambia. Si prefieres no fusionar código de arte aún, basta con traer `docs/`.
3. **Javier:** encargo a un artista con el BRIEF (recomendado), o una sesión más con un LoRA de caras con más pasos y la ropa limpiada por zonas (el contorno coloreado ya se probó en la ronda 12 y salió peor). No integrar nada generado hasta que pase la prueba ciega y tu revisión.
4. **1B_cena y 1C_llamada siguen en 20-55 %.** Dos salidas, las dos tuyas: (a) que una pista admita dos portadores (cambio de código en TurnAnalyzer, PromptBuilder, HintAdvisor y el calibrador), o (b) permitir 7 pistas en 1B (cambia el diseño de 5-6). Mientras tanto, nada cambia.
5. **La suite PlayMode carga qwen en Ollama aunque los tests usan un proveedor falso.** Causa: `OllamaProvider.preloadOnStart = true`
   (`OllamaProvider.cs:16`); al cargar la escena, `AIConversationManager.cs:53` llama a `WarmUpAsync()` antes de que el test
   cambie de proveedor. No lo toqué (capa de proveedores, fuera de límites). Propuesta: que los tests pongan
   `preloadOnStart = false` antes de cargar la escena (cambio solo en tests), o aceptarlo y descargar con `ollama stop`.

## Pendiente y riesgos
- Los worktrees de calibración (`../dng-main`, `../dng-mejoras`) se borran al cerrar; los informes de calibración vivían en sus `Logs/` (no versionados). Las cifras quedan en este informe, en CLUE-LOGIC.md y en PERFORMANCE-AUDIT.md.
- `feature/dia3` y las etiquetas `backup/*` siguen (hasta ~08-10-2026), sin tocar.
- Detalle hora a hora: `docs/NIGHT-LOG.md`, sección "# Sesión B".
