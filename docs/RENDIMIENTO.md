# Rendimiento (D2, día 3)

## Basura por fotograma (GC) — `PerformanceTests` (Play Mode, editor)
| Escena | Mediana | Del juego (restada la línea base) |
|---|---|---|
| Línea base (escena vacía: editor + test) | 6960 B | — |
| Menú con ambiente (polvo, lluvia, vapor, parpadeo, grano) | 6690 B | **≤ 0 B** |
| Interrogatorio quieto (retrato nervioso, grano) | 6690 B | **≤ 0 B** |

El juego no asigna memoria por fotograma en reposo: lo que se ve es el propio editor. El test falla si el juego
pasa de 2 KB por fotograma. Lo nuevo de hoy (SoundMix.Update, NoirPostFx, relieve) no asigna nada por fotograma
(sin LINQ, sin cadenas, sin cierres en Update).

## Coste en GPU de lo nuevo — `AnimationCapture.CosteVisual` (1080 × 1920, interrogatorio)
| Relieve (C3) | Post-proceso (C4) | ms por fotograma |
|---|---|---|
| no | no | 0,63 |
| sí | no | 0,56 |
| no | sí | 0,57 |
| sí | sí | 0,57 |

En la GPU de desarrollo (RTX 5070 Ti) las diferencias son ruido: domina el coste fijo de CPU del editor. **No es
una medida de móvil.** Estimación para un Android de gama media (Adreno 6xx / Mali-G7x):
- Relieve: 9 lecturas de textura por píxel, solo en el retrato (~15 % de la pantalla) → despreciable (< 0,2 ms).
- Post-proceso: gradación (LUT, una pasada) + bloom con filtrado barato a media resolución → del orden de 1-2 ms a
  1080 × 1920 según la documentación de URP; es lo único que puede notarse en un móvil flojo.
- Mitigación ya disponible: el ajuste "Filtro noir" lo quita todo (vuelve el lienzo superpuesto, coste cero) y el
  tema permite `postFx = false` o `postBloomIntensity = 0` (sin la pasada de bloom).
- Pendiente (necesita dispositivo): medir con el Profiler de Android en un móvil real (docs/ANDROID-BUILD.md).

## Atlas y pooling
- Casi toda la interfaz usa sprites generados en memoria (UISprites: redondeados, círculo, viñeta) que se cachean
  por radio: no hay un atlas que montar; los 5 iconos (128 px) podrían ir a un Sprite Atlas, ganancia mínima.
- Chat: las filas se reutilizan (pool de ChatView, test en ChatViewTests); efectos de FxLayer también reutilizan sus
  objetos. Materiales de gradación y relieve: uno por combinación de valores (caché en ArtGrading).
