# Revisión de código (día 3)

Revisión independiente (agente nuevo, sin el contexto de quien escribió el código) del diff del día:
`git diff c2d5501..HEAD -- Assets/Scripts Assets/Resources/Shaders` (65 archivos, ~2270 líneas). Solo lectura;
cada hallazgo comprobado contra el código (los dudosos, marcados *Plausible*). La resolución de cada uno está al
final (**Resolución**).

## Resumen
Nada **crítico**. Lo nuevo está bien aislado y en su mayor parte es lógica pura con tests (HintAdvisor,
DetectiveRank, Difficulty, TimeCheck, NaturalUnlocks, NarrativeValidator); los guardados antiguos siguen cargando
(`difficulty = -1` → Detective, `hintsUsed` por defecto 0). Hay **2 hallazgos importantes**, ambos de texto que
engaña o se pierde para el jugador (el rango final puede contradecir el final; la ayuda concreta de "Pensar" se
escribe en el chat equivocado), y una serie de menores de coherencia de estado, una fuga pequeña en NoirPostFx,
falsos positivos de TimeCheck y documentación desplazada.

## Hallazgos

| # | Sev. | Archivo:línea | Resumen |
|---|---|---|---|
| 1 | Importante | Cases/DetectiveRank.cs:11-26 | El rango premia acertar a ciegas: un final "Insuficiente" puede dar "Inspector" |
| 2 | Importante | InterrogationUI.cs:962-970 | La ayuda de nivel 2 se añade al chat del sospechoso actual y luego se cambia a otro: el jugador no la ve |
| 3 | Menor | GameManager.cs:252-255 | El desbloqueo por tema ocurre antes de la petición, sin diferir, y sobrevive si la pregunta falla |
| 4 | Menor | GameManager.cs:145, HintAdvisor.cs:43 | `HintMemory.given` no se guarda; `count` sube también con "ayudas" que no lo son |
| 5 | Menor | UI/Fx/NoirPostFx.cs:80-102 | Fuga de VolumeProfile (2 por recarga de escena) y `Refresh` en cada cambio de ajustes |
| 6 | Menor | Cases/TimeCheck.cs:36-38, AIConversationManager.cs:124-139 | Falsos positivos de horas inventadas (12/24 h, respuestas previas); el reintento no comprueba repetición |
| 7 | Menor (diseño) | Resources/Stories/StoriesDatabase.json | Raíces muy comunes ("ultima vez", "coche", "mensaje"): el desbloqueo "natural" casi siempre cae el día 1 |
| 8 | Menor | Emotions/EmotionPresenter.cs:174 | `DestroyImmediate` en tiempo de juego |
| 9 | Menor | Varios | Comentarios `<summary>` desplazados al miembro equivocado |
| 10 | Menor | GameManager.cs:27, CaseBriefing.cs:29, GameTexts.cs:142 | Números duplicados: `questionsPerDay` serializado ya sin efecto, `RulesLine(7, …)` a mano, instrucciones con cifras de Difficulty |
| 11 | Menor | Art/VoxelPortrait.cs, Resources/Shaders/VoxelLit.shader | Prototipo solo de pruebas dentro del ensamblado del juego y de la build |
| 12 | Menor | Cases/NarrativeValidator.cs:66 | NRE si `morningReports` es null (`AllTexts` sí lo protege) |
| 13 | Menor / Plausible | UI/ButtonStateFx.cs:44, ThemeApplier.cs:289 | Doble atenuación del botón desactivado; `Update` por botón cada fotograma |

### 1. El rango contradice el final (Importante)
`DetectiveRank.For` da 1 punto a `Ending.Insufficient`, +1 por prueba clave y +1 si quedan ≥ 2 días.
- Día 1, cero pistas, acusa al culpable por intuición: `Insufficient` (1) + 6 días (+1) = **"Sabueso"**, dos
  escalones por encima de "Novato".
- `Insufficient` + prueba clave `Incriminates` + acusación temprana = 3 → **"Inspector"**, junto al texto "pruebas
  insuficientes"; además `CaseRecords.RecordRank` lo guarda como mejor rango y sale en el expediente.
- Arreglo: el bonus de días solo con `ending >= Bittersweet` y tope de `Insufficient` en "Agente". Tests.

### 2. La ayuda concreta va a otro chat (Importante)
`OnThinkClick` hace `AppendNotice(...)` (→ `conversations.AppendToCurrent`) y, si `hint.level == 2`, después cambia
`suspectDropdown.value = who` → `SelectSuspect` muestra la conversación del portador. El aviso "Piensas… Prueba a
preguntarle a Rosario: «…»" queda en el chat de la madre; la pantalla salta a Rosario, donde solo está la pregunta
en el campo. En Detective, esa ayuda costó una pregunta.
- Arreglo: cambiar de sospechoso **antes** de `AppendNotice` (o anotar en la conversación del portador).

### 3. Desbloqueo por tema fuera del flujo diferido (Menor)
`NaturalUnlocks.TriggeredBy` + `Unlock` corren antes de `notices.BeginDefer()`: si la petición falla por red, el
aviso "Tu pregunta te pone sobre la pista de…" ya está en el chat, la vecina desbloqueada y sin guardar; si va bien,
el aviso queda entre pregunta y respuesta. Arreglo: tras `BeginDefer()` y solo si `result.Success`.

### 4. Memoria de ayudas incompleta (Menor)
`ContinueSavedGame` rehace `HintMemory` solo con `count`: tras "Continuar", la siguiente ayuda vuelve al nivel 1
de una pista ya dada (y en Detective se cobra otra vez). `HintAdvisor.Next` sube `count` también cuando devuelve
"Ya tienes todo…" o "Alguien que aún no conoces…", que cuentan para el −1 del rango. Arreglo: guardar `given` y
contar solo ayudas con `clueId`.

### 5. NoirPostFx (Menor)
`Volume.profile` clona el `sharedProfile` y nunca destruye el clon; el Volume vive en la escena: cada "Reiniciar"
o "Volver al menú" deja dos perfiles `DontSave` colgados. `Refresh` va en `GameSettings.Changed`: arrastrar un
deslizador de volumen hace `FindObjectsByType<Canvas>` (y de cámaras) en cada valor. Arreglo: un solo perfil
destruido con la escena; salir si `Enabled` no cambió. (Cambio de modo de render revisado: un solo lienzo raíz,
FxLayer con `overrideSorting`, `pressEventCamera` en PortraitMotion/TextLinkHandler: sin problemas de raycast.)

### 6. TimeCheck y el reintento (Menor)
Cifras solo con una lectura (ficha "17:30", respuesta "a las 5:30 de la tarde" → inventada y llamada extra).
`known` no incluye respuestas previas del personaje: una hora ya aceptada provoca reintentos cada vez que se
repite. La respuesta del reintento no pasa `IsRepeat`. Contadores estáticos que no se reinician entre ejecuciones
del bot en el editor.

### 7. Raíces de desbloqueo demasiado comunes (Menor, diseño)
`TriggeredBy` usa `Contains(stem)` sin límites de palabra; "ultima vez", "coche", "vehicul", "mensaje" salen en
preguntas corrientes: el personaje "bloqueado" aparece en la primera o segunda pregunta. Lo confirmaría la cifra
de desbloqueos por día del bot.

### 8. `DestroyImmediate` en juego (Menor)
`AdoptMaterial` puede destruir con `DestroyImmediate` en juego (cambio plano ↔ 2.5D, alto contraste); usar el
mismo patrón que `OnDestroy`.

### 9. Documentación desplazada (Menor)
CaseRecords.cs:34, GameManager.cs:428, GameSettings.cs:161, CaseModels.cs:101-105, Theme.cs:152,
InterrogationUI.cs:105: bloques nuevos insertados entre un `<summary>` y su miembro.

### 10. Cifras duplicadas (Menor)
`ApplyDifficulty` pisa siempre `questionsPerDay` (el campo serializado ya no hace nada); `CaseBriefing` pasa `7` a
mano; `InstructionsBody` escribe "cinco (siete…, cuatro…)". Si cambian Difficulty o `maxDays`, los textos dejan de
ser verdad sin que falle ningún test.

### 11. Prototipo vóxel en la build (Menor)
`VoxelPortrait` solo lo usa una captura de pruebas, pero está en `Detective.asmdef`, y `VoxelLit.shader` en
`Resources/` entra siempre en la build.

### 12. NRE en NarrativeValidator (Menor)
`v.morningReports.Length` sin protección cuando es null.

### 13. ButtonStateFx (Menor / Plausible)
`CanvasGroup.alpha = 0,45` además del `disabledColor` del ColorBlock: botones desactivados casi invisibles (lo
confirmaría una captura). El comentario dice "sin coste por fotograma" pero hay `Update` por botón.

## Lo que está bien (según la revisión)
- Guardados compatibles; campos nuevos con valores por defecto.
- Peticiones LLM protegidas: Pensar desactivado durante la petición; `requestInFlight`; `ShowRequestFailed(refunded:)`.
- Estado entre casos reiniciado en `ChooseStory`/`ContinueSavedGame`; `StoriesDatabase.Apply` tolera que falte el JSON.
- SoundMix puro y robusto; volumen base separado del apartado.
- Retrato 2.5D: materiales cacheados, respaldo plano sin shader, `target 2.0`, 9 lecturas de textura (GLES3/Vulkan ok).
- Selector de prueba clave; herramientas del editor generadas desde los datos reales.

## Resolución (misma tarde, una sola pasada; cada arreglo con su test visto en rojo y luego en verde)

| # | Estado | Qué se hizo | Test |
|---|---|---|---|
| 1 | **Arreglado** | "Insuficiente" se queda en "Agente"; los días de sobra solo premian finales bien cerrados | DetectiveRankTests (5 casos nuevos) |
| 2 | **Arreglado** | Primero se cambia al sospechoso y luego se escribe el aviso: queda en el chat que se ve | StateMachineTests.LaAyudaConcretaQuedaEnElChatDeQuienSabeAlgo (Play) |
| 3 | **Arreglado** | El desbloqueo por tema ocurre solo con respuesta, dentro de la cola diferida (el aviso sale tras la respuesta) | StateMachineTests.UnaPreguntaFallidaNoDesbloqueaANadie (Play) |
| 4 | **Arreglado** | `HintMemory` se guarda ("pista:nivel", `SaveData.hintsGiven`); solo cuentan y se cobran las ayudas con pista; Historia avisa de que abusar baja el rango | HintAdvisorTests (2), SaveSystemTests |
| 5 | **Arreglado** | Un solo perfil (`sharedProfile`), destruido con su Volume; `Refresh` solo si cambia el estado del filtro | NoirPostFxTests.RecargarLaEscenaNoAcumulaPerfiles (Play) |
| 6 | **Arreglado** | Doble lectura 12/24 h también en cifras; lo que el personaje ya dijo cuenta como conocido; el reintento no acepta una repetición; contadores del bot a cero en cada ejecución | TimeCheckTests (4), AIConversationFlowTests (2) |
| 7 | **Arreglado (medido)** | Con el bot, el hermano (historia 3) salía el día 1 en 8/12 partidas: la pregunta de ejemplo "¿Cuándo supiste de Paula por última vez?" contenía sus raíces. Quitadas "ultima vez", "supiste de" (hermano) y "madrug", "coche", "vehicul" (vecina, historia 2) | NaturalUnlocksTests.LasPreguntasDeEjemploNoDesbloqueanANadie |
| 8 | **Arreglado** | `Destroy` en juego, `DestroyImmediate` solo en el editor | (cubierto por LitPortraitTests) |
| 9 | **Arreglado** | Los seis comentarios vuelven a su miembro | — |
| 10 | **Arreglado** | Instrucciones con las cifras de `Difficulty`; el expediente con `maxDays`; `questionsPerDay` ya no es serializado | GameTextsTests.InstruccionesConLasCifrasDeLaDificultad |
| 11 | **Arreglado** | `VoxelPortrait` y `VoxelLit.shader` movidos a `Assets/Tests/PlayMode/Prototipos/` (fuera del juego y de la build) | Captura C3 sigue funcionando |
| 12 | **Arreglado** | `morningReports ?? new string[0]` | NarrativeValidatorTests.SinPartesDeLaMananaNoRompe |
| 13 | **Ajustado** | `disabledAlpha` 0,45 → 0,6 (encima del gris del ColorBlock el botón casi desaparecía); comentario corregido (sí hay una comparación por fotograma) | LayoutValidationTests (estados de botón) |

Suites tras la pasada: ver docs/NIGHT-LOG.md (entrada de la resolución).

---

# Revisión de código (día 3, tarde)
Segunda revisión independiente, sobre `git diff 0054001..HEAD` (lector de pantalla, ficha policial, cabecera en
pantallas alargadas, flechas, textos). **0 críticos, 3 importantes, 8 menores.** Resolución en una pasada, con test:

| # | Sev. | Hallazgo | Estado |
|---|---|---|---|
| 1 | Importante | "Pantalla nueva" se decidía por la primera etiqueta (el HUD): tras cada respuesta el foco del lector saltaba arriba y cortaba el anuncio | **Arreglado**: por el primer control, y nunca en los 1,5 s siguientes a un anuncio (test CambiarElHudNoEsCambiarDePantalla) |
| 2 | Importante | Jerarquía nueva con cualquier cambio: el lector perdía el foco (p. ej. al mover un deslizador) | **Arreglado**: un nodo por objeto; textos, valores y estados se actualizan en el sitio y solo se insertan o quitan los que cambian (test UnDeslizadorConservaSuNodoAlCambiarDeValor) |
| 3 | Importante (dudoso) | Origen de `frame` sin comprobar | **Confirmado arriba-izquierda** (el manual de Unity usa `worldBound` de UI Toolkit, que va así); test LosMarcosTienenElOrigenArriba |
| 4 | Menor | La firma no incluía la identidad: nodos atados a objetos destruidos | **Arreglado** por el n.º 2 (id de instancia; acciones que comprueban el objeto) |
| 5 | Menor | Sin zonas desplazables: lo de fuera de un scroll (la ficha policial, el chat antiguo) era inalcanzable | **Arreglado**: nodo ScrollView con desplazamiento por páginas (test ElExpedienteEsUnaZonaDesplazable) |
| 6 | Menor | El recorte a 60 textos podía quitar títulos | **Arreglado**: se ordena primero y solo se recortan los mensajes más antiguos del chat |
| 7 | Menor | Solo el CanvasGroup más cercano; no el alfa del texto | **Arreglado**: se multiplican todos los grupos (respetando ignoreParentGroups) y el alfa propio (test UnTextoTransparenteNoSeLee) |
| 8 | Menor | Muchas reservas de memoria cada 0,5 s con el lector encendido | **Reducido**: esquinas, listas, PointerEventData y Regex reutilizados; el rectángulo se calcula una vez por elemento |
| 9 | Menor | Rótulo de casillas/deslizadores leído dos veces; lambdas sin null; sin OnDestroy | **Arreglado** (el rótulo usado no se lee aparte; el del deslizador es el texto justo encima) |
| 10 | Menor | La escala de letra del sistema se leía en cada consulta | **Arreglado**: una vez, en perezoso |
| 11 | Menor | La cabecera medía el lienzo raíz (con muesca) y el test usaba un lienzo que ningún móvil da | **Arreglado**: por proporción dentro del área segura; test con `LayoutPreview.CanvasSize(1080, 2400)` |

Aparte: en batchmode la "pantalla" del editor es 640×480 apaisada (el chat se queda sin alto); los tests del lector
lo tienen en cuenta.

---

# Revisión de código (día 3, tercera)
Tercera revisión independiente, sobre `git diff d4beea2..HEAD` (versiones en la libreta, reintento por idioma,
etiqueta tolerante, quinta indicación, sonido de la ficha, desplegables, motivos musicales). **0 críticos, 2
importantes, 6 menores.** Resolución:

| # | Sev. | Hallazgo | Estado |
|---|---|---|---|
| 1 | Importante | El reintento por idioma podía volver a la respuesta repetida (sin la nota de variedad ni la temperatura del reintento por repetición) | **Arreglado**: si ya se pidió variedad, el reintento por idioma la mantiene. *Decisión:* no se prohíbe del todo la repetición: una respuesta repetida hace menos daño que una en chino que se queda en el historial |
| 2 | Importante | "Dice: «…»" enseñaba el guion: la mentira del culpable antes de contarla, o el nombre de alguien aún no disponible | **Arreglado**: la del culpable solo cuando ya ha contado su mentira; una versión que nombra a alguien bloqueado espera a que aparezca (tests) |
| 3 | Menor | Hasta 3 llamadas; la respuesta del reintento por idioma no pasa el control de horas | **Documentado** como presupuesto (dos llamadas de más como mucho; horas solo si no hubo otro reintento) |
| 4 | Menor | El reintento por idioma podía elegir una respuesta vacía | **Arreglado** (test) |
| 5 | Menor | La quinta indicación podía salir con la libreta ya cerrada | **Arreglado** (condición al mostrarla) |
| 6 | Menor | La forma canónica quitaba etiquetas no reconocidas; las partidas guardadas conservaban las erratas | **Arreglado**: se conserva tal cual; al continuar, el historial se normaliza (test) |
| 7 | Menor | La etiqueta cortada con errata ("[MESTADO: nerv") seguía viéndose | **Arreglado** (test) |
| 8 | Menor (bot) | Al rehacer una pregunta en otro alfabeto, el bot perdía la ayuda de Pensar | **Arreglado** |

# Revisión de código (día 3, cuarta)
Cuarta revisión independiente (subagente, contexto limpio), sobre `git diff 87d4932..HEAD` (notas del jugador,
enlaces de la libreta para el lector, menú con "Continuar" principal). **0 críticos, 1 importante, 6 menores.**

| # | Sev. | Hallazgo | Estado |
|---|---|---|---|
| 1 | Importante | Tocar una nota con una pregunta en marcha guardaba la partida a medio turno (la pregunta sin respuesta quedaba en el historial guardado; al continuar, dos mensajes de usuario seguidos) | **Arreglado**: con la pregunta en marcha la nota se guarda al terminar el turno (test visto en rojo) |
| 2 | Menor | Ids repetidos en el lector (enlace = hash de texto + id) descuadrarían la jerarquía | **Arreglado**: el segundo id igual no entra |
| 3 | Menor | El marco del enlace usaba el índice guardado; al llegar una pista con la libreta abierta, el foco se dibujaba en otro enlace durante medio segundo | **Arreglado**: se busca por id |
| 4 | Menor | En la rueda se atenuaba la celda entera: también el anillo de selección y el nombre (contraste) | **Arreglado**: solo el retrato; el nombre en color secundario, a todo contraste |
| 5 | Menor | "tu nota: sospechoso/descartado" en masculino para todos; la ayuda prometía que la rueda tenía en cuenta "sospechoso" | **Arreglado**: "sospecha" / "descarte" (sustantivos); la ayuda dice lo que hace; el lector dice "Nota sobre X: sospecha" |
| 6 | Menor | Cadenas nuevas cada medio segundo por enlace con el lector activo (GC en móviles modestos) | **Pendiente**, anotado: solo con lector activo; cachear por texto si se nota en un móvil real |
| 7 | Menor (tests) | La nota no se probaba con "Continuar" ni durante una pregunta | **Arreglado**: dos tests de juego nuevos |
| — | Nit | Comentario de `RestartButton` pegado a `NewCaseButton` | **Arreglado** |

# Revisión de código (día 3, quinta)
Quinta revisión independiente (subagente), sobre `git diff 0e9ab1f..HEAD` (rueda con pista de descarte, variantes
no jugadas primero, línea para rejugar, partes en la libreta, cambios del bot). **1 confirmado, 4 plausibles.**
La revisión trazó y dio por buenos: el código de las variantes (sin colisiones), "Caso al azar", que se apunte una
sola vez por caso, el último día, continuar una partida, el lector y que los tests no pasen en vacío.

| # | Sev. | Hallazgo | Estado |
|---|---|---|---|
| 1 | Confirmado (bot) | El paso "id de pista como sospechoso → quien la sabe" nunca se ejecutaba (`Resolve` no devuelve null) | **Arreglado** |
| 2 | Plausible | "Nombre (pista de descarte)" en una línea se cortaría en 16:9 con nombres largos | **Arreglado**: el motivo en su propia línea, pie más alto solo si hay alguien descartado; capturas con 2 y 3 sospechosos |
| 3 | Plausible | Quien ya jugó antes de esta versión verá otra vez "otros dos culpables" | **Aceptado** (aún no publicado): decisión 15 |
| 4 | Plausible | Un caso forzado en el inspector se apuntaba como jugado en los ajustes reales | **Arreglado** |
| 5 | Tests | Nada comprobaba que acusar apunte la variante ni la línea del informe | **Arreglado**: test de juego |

# Revisión de código (día 3, sexta)
Sexta revisión independiente, sobre `git diff f2b69b4..HEAD` (arreglos de la quinta, textos, caso peor del layout,
reintento frío). **Ningún defecto para el jugador en la build.** Dio por buenos: altura del pie al rehacer la
rueda, `TryFind`, que la temperatura fría solo toque el reintento por horas y que los tests nuevos no pasen en vacío.

| # | Sev. | Hallazgo | Estado |
|---|---|---|---|
| 1 | Plausible | El pie alto (96) resta retrato a todas las celdas y ningún test lo medía | **Arreglado**: test de `GridFit` con 4-6 sospechosos (busto ≥ 200 px) |
| 2 | Confirmado (editor) | Continuar una partida no reiniciaba la marca de caso forzado | **Arreglado** |
| 3 | Confirmado (editor) | Con un caso forzado, el final contaba un culpable de más por ver | **Arreglado** |
| 4 | Plausible (bot) | Un id de pista que contenga un nombre corto se resolvería a ese sospechoso | **Anotado** (solo el bot; los ids actuales no lo hacen) |
| 5 | Confirmado, menor | El lector leía "Daniel tu descarte" sin separación | **Arreglado**: "(tu descarte)" entre paréntesis |
| 6 | Bajo | Los partes del caso peor del layout no son necesariamente los más largos | **Anotado** |
