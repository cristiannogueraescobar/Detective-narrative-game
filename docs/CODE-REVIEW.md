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
