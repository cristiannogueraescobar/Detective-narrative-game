# Etapa 1 · Arquitectura + Historia 1 — Plan de implementación

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Sustituir el sistema de casos/pistas actual por uno dirigido por datos (fichas por personaje y variante, detección en la respuesta del portador, contradicciones deterministas, final por evidencia), con la historia 1 completa, modo debug y calibración contra qwen.

**Architecture:** Datos puros (`Cases/`) + tres servicios puros testeables (`ClueDetector`, `PromptBuilder`, `InvestigationState`). `AIConversationManager` orquesta LLM → detector → estado y emite eventos; `GameManager` gestiona días, elenco, desbloqueos y acusación; `InterrogationUI` presenta. Los scripts pasan a un assembly `Detective` para poder testearlos desde EditMode.

**Tech Stack:** Unity 6000.3.2f1, C#, TextMeshPro/uGUI, Unity Test Framework 1.6 (NUnit), Ollama `qwen2.5:7b-instruct`.

**Spec:** `docs/superpowers/specs/2026-09-28-historias-pistas-mecanicas-design.md`

## Global Constraints

- Todo texto de juego y prompt en español; los IDs de pista nunca se muestran al jugador.
- Ficha de personaje 250-350 palabras, secciones en el orden del spec.
- Detección solo sobre la respuesta del portador, normalizada (minúsculas, sin tildes, horas `22.35`/`22h35` → `22:35`).
- El culpable nunca es portador de pistas necesarias para evidencia ≥ 5.
- Final: evidencia = pistas I + 2 × contradicciones; GOOD ≥ 5, BITTERSWEET 3-4, INSUFFICIENT ≤ 2, BAD si fallas.
- LLM: `num_ctx` 8192 (también en la precarga), historial enviado = últimos 16 mensajes, temperatura 0.6, `maxTokens` 250.
- Debug (forzar variante y log de anclas) solo en editor.
- Estilo del código existente: sin namespaces, comentarios en español, `[SerializeField] private`.

## Review Focus

1. El sospechoso niega el hecho con las mismas palabras ("no vi ninguna taza") → no debería contar como pista; mitigado con anclas de dos grupos (objeto + detalle concreto) y pruebas `sampleMisses`.
2. El jugador pulsa preguntar con el selector de prueba puesto pero sin texto → debe enviarse la confrontación con una pregunta por defecto, no dar error.
3. El jugador acusa antes de desbloquear a la vecina → la acusación solo ofrece desbloqueados; el resultado debe seguir siendo coherente.
4. Petición LLM fallida tras mostrar una prueba → no debe registrarse como mostrada ni gastar pregunta.
5. Variante forzada en debug que aún no existe (2A en etapa 1) → aviso y sorteo entre las registradas, sin excepción.

---

## Mapa de ficheros

| Fichero | Responsabilidad |
|---|---|
| `Assets/Scripts/Detective.asmdef` (nuevo) | Assembly del juego; refs `Unity.TextMeshPro`, `UnityEngine.UI` |
| `Assets/Scripts/Cases/CaseModels.cs` (nuevo) | Tipos de datos |
| `Assets/Scripts/Cases/CaseLibrary.cs` (nuevo) | Registro de historias y búsqueda de variantes |
| `Assets/Scripts/Cases/Story1HijaPerfecta.cs` (nuevo) | Datos de la historia 1 |
| `Assets/Scripts/Cases/ClueDetector.cs` (nuevo) | Normalización y anclas con traza |
| `Assets/Scripts/Cases/PromptBuilder.cs` (nuevo) | Ficha → system prompt |
| `Assets/Scripts/Cases/InvestigationState.cs` (nuevo) | Pistas, pruebas mostradas, contradicciones, final |
| `Assets/Scripts/AIConversationManager.cs` (reescrito) | Orquestación LLM + detección + eventos |
| `Assets/Scripts/GameManager.cs` (reescrito) | Flujo de días, elenco, desbloqueos, acusación, debug |
| `Assets/Scripts/InterrogationUI.cs` (modificado) | Elenco por id, selector de prueba, libreta, HUD, epílogo |
| `Assets/Scripts/LLM/OllamaProvider.cs` (modificado) | `numCtx` en chat y precarga |
| `Assets/Scripts/Editor/Detective.Editor.asmdef`, `ClueCalibrator.cs` (nuevos) | Calibración |
| `Assets/Tests/EditMode/Detective.Tests.asmdef` + tests (nuevos) | Tests EditMode |

Comando de tests (con el editor cerrado):
```
"/c/Program Files/Unity/Hub/Editor/6000.3.2f1/Editor/Unity.exe" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode-results.xml -logFile Logs/editmode.log
```
Éxito = código de salida 0 y `result="Passed"` en el XML.

---

### Task 1: Assembly + modelos + ClueDetector

**Files:**
- Create: `Assets/Scripts/Detective.asmdef`, `Assets/Scripts/Cases/CaseModels.cs`, `Assets/Scripts/Cases/ClueDetector.cs`
- Create: `Assets/Tests/EditMode/Detective.Tests.asmdef`, `Assets/Tests/EditMode/ClueDetectorTests.cs`

**Interfaces — Produces:**
- `enum ClueKind { Incriminates, Clears, Context }`
- `class ClueData { id, playerName, summary, holder, topic, fact, bool isSecret, string[][] anchors, ClueKind kind, string clears, bool exposesLie, string[] calibrationQuestions, string[] sampleHits, string[] sampleMisses }`
- `class CharacterData { id, name, shortName, roleLabel, portraitKey, identity, speech, speechExample, bool startsUnlocked, string[] mentionAliases; string DisplayName }`
- `class CharacterRole { characterId, string[] knowledge, version, secret, admitsWhen, nervousAbout, ifAccused, doesNotKnow, lieQuote, string[][] lieAnchors, versionB }`
- `class VariantData { id, culpritId, epilogue, string[] morningReports, List<CharacterRole> roles, List<ClueData> clues; Role(id); Clue(id) }`
- `class StoryData { id, title, intro, caseBrief, List<CharacterData> cast, List<VariantData> variants; Character(id) }`
- `static class ClueDetector { string Normalize(string); AnchorTrace Evaluate(string[][] groups, string normalizedText); bool MentionsAny(string normalizedText, IEnumerable<string> aliases) }`
- `class AnchorTrace { bool Matched; List<GroupTrace> Groups; string ToString() }`, `class GroupTrace { string[] Anchors; List<string> Found }`

- [ ] **Step 1: asmdefs.** `Detective.asmdef`: `{"name":"Detective","references":["Unity.TextMeshPro","UnityEngine.UI"],"autoReferenced":true}`. `Detective.Tests.asmdef`: refs `Detective`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner`; `includePlatforms:["Editor"]`, `overrideReferences:true`, `precompiledReferences:["nunit.framework.dll"]`, `defineConstraints:["UNITY_INCLUDE_TESTS"]`. Los scripts existentes conservan sus GUID: las referencias de la escena no se rompen.
- [ ] **Step 2: Tests que fallan** (`ClueDetectorTests`):
```csharp
[Test] public void Normalize_QuitaTildesMayusculasYUnificaHoras()
{
    Assert.AreEqual("a las 22:35 vi al padre", ClueDetector.Normalize("A las 22.35   vi al PADRE"));
    Assert.AreEqual("22:35 cortinas", ClueDetector.Normalize("22h35 Cortinas"));
    Assert.AreEqual("habitacion nino", ClueDetector.Normalize("Habitación niño"));
}
[Test] public void Evaluate_ExigeTodosLosGrupos()
{
    var groups = new[] { new[] { "22:35", "diez y media" }, new[] { "cortina" } };
    Assert.IsTrue(ClueDetector.Evaluate(groups, ClueDetector.Normalize("A las 22.35 cerraba las cortinas")).Matched);
    Assert.IsFalse(ClueDetector.Evaluate(groups, ClueDetector.Normalize("A las 22:35 estaba dormida")).Matched);
}
[Test] public void Evaluate_TrazaIndicaAnclasEncontradas()
{
    var trace = ClueDetector.Evaluate(new[] { new[] { "taza", "cacao" } }, "una taza de cacao");
    CollectionAssert.AreEqual(new[] { "taza", "cacao" }, trace.Groups[0].Found);
    StringAssert.Contains("taza", trace.ToString());
}
[Test] public void Evaluate_SinGruposNoDetecta() => Assert.IsFalse(ClueDetector.Evaluate(new string[0][], "x").Matched);
[Test] public void MentionsAny_DetectaAliasNormalizado() =>
    Assert.IsTrue(ClueDetector.MentionsAny(ClueDetector.Normalize("Rosario, la de enfrente"), new[] { "Rosario" }));
```
- [ ] **Step 3: Ejecutar tests** → FAIL (tipos inexistentes).
- [ ] **Step 4: Implementar** `CaseModels.cs` (campos de la sección Interfaces) y `ClueDetector.cs`:
```csharp
public static string Normalize(string text)
{
    if (string.IsNullOrEmpty(text)) return "";
    string decomposed = text.ToLowerInvariant().Normalize(NormalizationForm.FormD);
    var sb = new StringBuilder(decomposed.Length);
    foreach (char c in decomposed)
        if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
    string s = TimePattern.Replace(sb.ToString().Normalize(NormalizationForm.FormC), "$1:$2"); // \b(\d{1,2})\s*[.,h:]\s*(\d{2})\b
    return Spaces.Replace(s, " ").Trim();
}
public static AnchorTrace Evaluate(string[][] groups, string normalizedText)
{
    var trace = new AnchorTrace();
    if (groups == null || groups.Length == 0) return trace;
    trace.Matched = true;
    foreach (var group in groups)
    {
        var found = group.Where(a => normalizedText.Contains(Normalize(a))).ToList();
        trace.Groups.Add(new GroupTrace { Anchors = group, Found = found });
        if (found.Count == 0) trace.Matched = false;
    }
    return trace;
}
```
- [ ] **Step 5: Tests** → PASS. **Step 6: Commit** "Add Detective assembly, case models and clue detector".

### Task 2: InvestigationState (pistas, contradicciones, final)

**Files:** Create `Assets/Scripts/Cases/InvestigationState.cs`, `Assets/Tests/EditMode/InvestigationStateTests.cs`, `Assets/Tests/EditMode/TestCases.cs` (variante sintética mínima para tests).

**Interfaces — Produces:**
- `enum Ending { Good, Bittersweet, Insufficient, Bad }`
- `class AccusationResult { Ending ending; bool correct; string accusedId; int evidence; int incriminatingFound; int contradictions; bool ignoredClearingClue }`
- `class InvestigationState(VariantData)`: `bool Discover(string clueId)`, `bool IsDiscovered(string)`, `IReadOnlyList<string> DiscoveredClueIds`, `void RegisterShown(string characterId, string clueId)`, `IEnumerable<string> ShownTo(string characterId)`, `void RegisterLieTold()`, `bool CulpritToldLie`, `List<ClueData> UpdateContradictions()`, `IReadOnlyList<string> ContradictionClueIds`, `int IncriminatingFound`, `int Evidence`, `AccusationResult Accuse(string accusedId)`, `static int MaxEvidenceWithoutCulprit(VariantData)`; consts `GoodThreshold=5`, `BittersweetThreshold=3`, `ContradictionWeight=2`.

- [ ] **Step 1: Tests que fallan** — variante sintética con culpable `a`, pistas `i1..i3` (I, portador `b`), `x` (I + exposesLie, portador `c`), `d` (Clears `b`):
```csharp
[Test] public void Contradiccion_RequiereMentiraOConfrontacion()
{
    var s = new InvestigationState(TestCases.Variant());
    s.Discover("x");
    Assert.IsEmpty(s.UpdateContradictions());
    s.RegisterLieTold();
    Assert.AreEqual("x", s.UpdateContradictions().Single().id);
    Assert.IsEmpty(s.UpdateContradictions()); // no se duplica
}
[Test] public void Contradiccion_PorConfrontarAlCulpable()
{
    var s = new InvestigationState(TestCases.Variant());
    s.Discover("x"); s.RegisterShown("a", "x");
    Assert.AreEqual(1, s.UpdateContradictions().Count);
}
[TestCase(new[] { "i1", "i2", "i3", "x" }, true, Ending.Good)]      // 4 + 2 = 6
[TestCase(new[] { "i1", "x" }, true, Ending.Bittersweet)]           // 2 + 2 = 4
[TestCase(new[] { "i1", "i2", "i3" }, false, Ending.Bittersweet)]   // 3
[TestCase(new[] { "i1" }, false, Ending.Insufficient)]
public void Final_PorEvidencia(string[] found, bool lie, Ending expected) { ... Accuse("a").ending == expected }
[Test] public void Final_AcusarInocenteConPistaDeDescarte()
{
    var s = new InvestigationState(TestCases.Variant()); s.Discover("d");
    var r = s.Accuse("b");
    Assert.AreEqual(Ending.Bad, r.ending); Assert.IsTrue(r.ignoredClearingClue);
}
[Test] public void Discover_DevuelveFalsoSiRepetida() { ... }
```
- [ ] **Step 2: FAIL. Step 3: Implementar** según Interfaces (contradicción: pista `exposesLie` descubierta, no registrada aún, y `CulpritToldLie || ShownTo(culpritId).Contains(id)`; `Accuse`: `correct = accusedId == culpritId`; si falla → Bad con `ignoredClearingClue = discovered.Any(id => clue.kind==Clears && clue.clears==accusedId)`; si acierta → umbrales). **Step 4: PASS. Step 5: Commit** "Add investigation state with contradictions and evidence-based endings".

### Task 3: PromptBuilder

**Files:** Create `Assets/Scripts/Cases/PromptBuilder.cs`, `Assets/Tests/EditMode/PromptBuilderTests.cs`.

**Interfaces — Produces:** `static string PromptBuilder.Build(StoryData story, VariantData variant, string characterId, int day, IEnumerable<ClueData> shownToCharacter, IEnumerable<ClueData> alreadyTold)`

Estructura exacta (líneas vacías omitidas si la sección no tiene contenido):
```
{identity}
CÓMO HABLAS: {speech} Ejemplo: "{speechExample}"
EL CASO: {caseBrief}
LO QUE SABES: - {knowledge...}
SI TE PREGUNTAN, CUÉNTALO CON LA HORA Y LOS DETALLES EXACTOS:
- Si te preguntan por {topic}: {fact}          (pistas del portador, no secretas)
TU VERSIÓN: {version}
LO QUE OCULTAS: {secret} {facts de pistas secretas}. Lo admites solo si {admitsWhen}.
[culpable] ERES EL CULPABLE, pero nunca lo confiesas. Mantén tu versión con calma.
[culpable] SI EL INSPECTOR TE MUESTRA UNA PRUEBA QUE CONTRADICE TU VERSIÓN: {versionB}
TE PONE NERVIOSO: {nervousAbout}
SI TE ACUSAN: {ifAccused}
NO SABES: {doesNotKnow} Si te preguntan por eso, di que no lo sabes.
PRUEBAS QUE YA TE HAN MOSTRADO: - {summary}
YA HAS CONTADO (no te contradigas): - {fact}
HOY ES EL DÍA {day} DE LA INVESTIGACIÓN.
REGLAS: Responde en español, en primera persona, con 2 a 4 frases. Sin asteriscos, sin listas, sin acotaciones. Nunca digas que eres una IA. No inventes horas, nombres ni hechos que no estén en esta ficha.
```

- [ ] **Step 1: Tests que fallan:** incluye hechos del portador y no los de otros; pista secreta aparece bajo "LO QUE OCULTAS"; solo el culpable recibe "ERES EL CULPABLE" y versión B; aparece "HOY ES EL DÍA 3"; pruebas mostradas aparecen; ≤ 450 palabras para todas las fichas registradas (test en Task 4).
- [ ] **Step 2: FAIL. Step 3: Implementar. Step 4: PASS. Step 5: Commit** "Add prompt builder for character sheets".

### Task 4: Historia 1 + CaseLibrary + validación de datos

**Files:** Create `Assets/Scripts/Cases/Story1HijaPerfecta.cs`, `Assets/Scripts/Cases/CaseLibrary.cs`, `Assets/Tests/EditMode/CaseDataValidationTests.cs`.

**Interfaces — Produces:** `static StoryData Story1HijaPerfecta.Build()`; `static class CaseLibrary { IReadOnlyList<StoryData> Stories; IEnumerable<(StoryData story, VariantData variant)> AllVariants(); bool TryFind(string variantId, out StoryData, out VariantData) }`

Contenido: elenco (`padre`, `madre`, `hermano`, `vecina`; retratos `Padre`, `Madre`, `Hermano`, `Vecina`; la vecina `startsUnlocked=false`, alias `rosario`, `vecina`, `la de enfrente`), intro del jugador, `caseBrief`, y las variantes 1A/1B/1C con las líneas temporales, mentiras, versión B, secretos de inocentes y pistas exactamente como en el spec (secciones "Historia 1" y "Pistas por variante"). Cada pista: 2 grupos de anclas (detalle + objeto) con variantes de redacción, 2+ `calibrationQuestions`, 2+ `sampleHits`, 1+ `sampleMisses` (negación o hecho parecido). Todas las fichas de la familia mencionan a Rosario en `knowledge`. `morningReports` de 7 entradas (día 1 vacío), `epilogue` con la verdad.

- [ ] **Step 1: Tests de validación que fallan** (parametrizados sobre `CaseLibrary.AllVariants()`):
  - 4-6 pistas; ≥1 `exposesLie`; ≥1 `Clears`; ids únicos.
  - portador y `clears` existen en el elenco; roles para todo el elenco; culpable en el elenco con `lieAnchors` y `versionB`.
  - `MaxEvidenceWithoutCulprit ≥ 5`.
  - cada pista: ≥2 grupos de anclas no vacíos, ≥2 preguntas de calibración, cada `sampleHits` detecta y cada `sampleMisses` no.
  - `morningReports.Length == 7`; epílogo no vacío.
  - cada personaje bloqueado es mencionado (alias) en `knowledge` de algún personaje desbloqueado.
  - ficha construida ≤ 450 palabras.
- [ ] **Step 2: FAIL. Step 3: Escribir datos. Step 4: PASS. Step 5: Commit** "Add story 1 data with three variants".

### Task 5: Orquestación (AIConversationManager, Ollama, GameManager, UI)

**Files:** Rewrite `AIConversationManager.cs`, `GameManager.cs`; modify `InterrogationUI.cs`, `LLM/OllamaProvider.cs`, `Assets/Scenes/Game.unity` (solo `maxTokens`/`temperature` serializados).

**Interfaces:**
- `AIConversationManager`: `const int DefaultMaxTokens = 250; const float DefaultTemperature = 0.6f; const int MaxHistoryMessages = 16;` `void StartCase(StoryData, VariantData)`; `InvestigationState State`; `Task<LLMResult> AskSuspect(string characterId, string question, int day, ClueData shownClue)`; `static string BuildUserMessage(string question, ClueData shown)`; eventos `Action<ClueData> OnClueRevealed`, `Action<string> OnContradictionDetected`, `Action<string> OnCharacterMentioned`; `[SerializeField] bool debugLogClueEvaluation = true` (solo editor).
- `OllamaSettings.numCtx = 8192` enviado en chat y precarga.
- `GameManager`: `enum CaseSelection { Aleatoria, Caso1A, Caso1B, Caso1C, Caso2A, Caso2B, Caso2C, Caso3A, Caso3B, Caso3C }` `[SerializeField] debugCase` (ignorado fuera del editor); `AskQuestion(string characterId, string question, string shownClueId)`; `BeginInterrogation()`; `IReadOnlyList<SuspectView> UnlockedSuspects`.
- `struct SuspectView { string id; string displayName; string portraitKey; }` (en `CaseModels.cs`).
- `InterrogationUI`: `SetSuspects(List<SuspectView>)`, `SetEvidenceOptions(List<ClueData>)`, `UpdateCluesList(List<ClueData>)`, `ShowDayTransition(int day, string report)`, `ShowNotice(string)`, `ShowAccusationPanel(List<SuspectView>)`, `ShowAccusationResult(AccusationResult, string accusedName, string culpritName, int maxEvidence, string epilogue)`, `UpdateGameState(int day, int maxDays, int used, int max)`. Selector "Mostrar prueba": campo serializado `evidenceDropdown`; si es null se clona `suspectDropdown` y se coloca debajo (aviso en consola).

Flujo de `AskSuspect`: añade mensaje de usuario → envía system prompt + últimos 16 mensajes → si falla, retira el mensaje y no registra nada → si va bien: registra prueba mostrada; para cada pista del personaje no descubierta, evalúa anclas (log `[Pista] 1A_taza (Carmen): [taza|cacao]→taza & [solo daniel|...]→∅ ⇒ NO` en editor) y descubre; si es el culpable evalúa `lieAnchors` → `RegisterLieTold`; menciones de alias en la respuesta → `OnCharacterMentioned`; `UpdateContradictions` → `OnContradictionDetected("La versión de {shortName} («{lieQuote}») choca con: {playerName}")`.

GameManager: día 3 desbloquea pendientes con aviso; parte de la mañana al cambiar de día; aviso días 6 y 7; acusación con `State.Accuse`.

- [ ] **Step 1: Test** `BuildUserMessage` (con y sin prueba, pregunta vacía con prueba → pregunta por defecto) en `ConversationTests.cs`; FAIL → implementar → PASS.
- [ ] **Step 2: Reescribir AIConversationManager, OllamaProvider, GameManager, InterrogationUI** según Interfaces. Compilar con la batería de tests (la compilación de todo el assembly es condición para que corran).
- [ ] **Step 3: Tests** → PASS. **Step 4: Commit** "Wire data-driven cases into conversation, game flow and UI".

### Task 6: Calibrador

**Files:** Create `Assets/Scripts/Editor/Detective.Editor.asmdef` (refs `Detective`, solo Editor), `Assets/Scripts/Editor/ClueCalibrator.cs`.

**Interfaces:** `[MenuItem("Detective/Calibrar pistas")] static void RunMenu()`; `static void RunFromCommandLine()` (args `-variants 1A,1B -tries 3 -ollama http://localhost:11434 -model qwen2.5:7b-instruct`); `static CalibrationReport Run(IEnumerable<string> variantIds, int tries, Action<string,float> progress)`.

Por pista × pregunta × intento: historial vacío; turnos separados por `||` en la pregunta; prompt `PromptBuilder.Build(..., day: 2, shown: vacío, told: vacío)`; POST `/api/chat` con `HttpClient` (`stream=false`, `temperature 0.6`, `num_predict 250`, `num_ctx 8192`); detecta si alguna respuesta de la conversación hace match. Tasa por pista = aciertos / intentos totales; `< 2/3` → FALLA. Informe markdown en `Logs/clue-calibration.md` con tabla por pista y, para las fallidas, respuestas completas con traza de anclas. Código de salida 1 si alguna falla.

Comando:
```
Unity.exe -batchmode -nographics -projectPath . -executeMethod ClueCalibrator.RunFromCommandLine -variants 1A,1B,1C -tries 3 -logFile Logs/calibration-unity.log
```
- [ ] **Step 1: Implementar. Step 2: Ejecutar** sobre 1A,1B,1C. **Step 3: Ampliar anclas** (y añadir la respuesta real como `sampleHits`) de cada pista < 2/3; revisar el prompt si la IA no cuenta el hecho. Repetir hasta que todas ≥ 2/3. **Step 4: Tests** → PASS. **Step 5: Commit** "Add clue calibration tool and tune story 1 anchors".

### Task 7: Revisión final y entrega de etapa 1

- [ ] Revisión de toda la rama (subagente revisor) contra spec y plan; corregir hallazgos.
- [ ] Informar al usuario: qué probar en el editor (debug de variante en `GameManager`, log de pistas, flujo completo con final BUENO en 1A/1B/1C) y resultados de la calibración.
