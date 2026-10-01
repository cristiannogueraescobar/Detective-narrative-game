using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// Auditoría de la máquina de estados (A3): los casos límite de cada transición, en la escena real, con un proveedor
/// falso. Los toques van por el mismo camino que un dedo (ExecuteEvents): un botón desactivado u oculto no responde.
/// </summary>
public class StateMachineTests
{
    private class FakeProvider : ILLMProvider
    {
        public readonly Queue<LLMResult> results = new Queue<LLMResult>();
        public int delayMs = 150;
        public int calls;
        public string DisplayName => "Falso";

        // Todo lo que llega a Ollama (ficha + historial), para comprobar que una partida no ve la anterior
        public readonly List<string> sent = new List<string>();

        public async Task<LLMResult> SendAsync(string systemPrompt, IReadOnlyList<ChatMessage> history, int maxTokens, float temperature)
        {
            calls++;
            sent.Add(systemPrompt + "\n" + string.Join("\n", history.Select(m => m.role + ": " + m.content)));
            await Task.Delay(delayMs);
            return results.Count > 0 ? results.Dequeue() : LLMResult.Ok($"Respuesta número {calls}, sin más. [ESTADO: tranquilo]");
        }

        public Task WarmUpAsync() => Task.CompletedTask;
    }

    private class MemoryStore : ISettingsStore
    {
        private readonly Dictionary<string, float> values = new Dictionary<string, float>();
        public float Get(string key, float fallback) => values.TryGetValue(key, out float v) ? v : fallback;
        public void Set(string key, float value) => values[key] = value;
    }

    private string saveDirectory;
    private FakeProvider provider;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        saveDirectory = Path.Combine(Path.GetTempPath(), "detective-states-" + System.Guid.NewGuid().ToString("N"));
        SaveSystem.DirectoryOverride = saveDirectory;
        GameSettings.UseStore(new MemoryStore());
        Tutorial.SkipAll();
        provider = new FakeProvider(); // Uno nuevo por test (NUnit reutiliza la instancia de la clase)
        yield return LoadGame();
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        SaveSystem.Flush(); // Una escritura pendiente podría volver a crear la carpeta o bloquear el archivo
        SaveSystem.DirectoryOverride = null;
        GameSettings.UseStore(null);
        if (Directory.Exists(saveDirectory))
            Directory.Delete(saveDirectory, true);
        yield return null;
    }

    private IEnumerator LoadGame()
    {
        yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
        yield return null;
        Object.FindFirstObjectByType<AIConversationManager>().UseProvider(provider);
        yield return null;
    }

    private static GameObject Find(string name)
    {
        return Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == name && g.scene.IsValid());
    }

    // Un toque de verdad: si el botón está desactivado u oculto, no pasa nada
    private static void TapNow(GameObject go)
    {
        ExecuteEvents.Execute(go, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
    }

    private static IEnumerator Tap(string name, int times = 1)
    {
        GameObject go = Find(name);
        Assert.IsNotNull(go, name);
        for (int i = 0; i < times; i++)
            TapNow(go); // Varios toques en el mismo fotograma: una doble pulsación
        yield return null;
        yield return null;
    }

    private static IEnumerator WaitUntil(System.Func<bool> condition, float timeout, string what)
    {
        float end = Time.realtimeSinceStartup + timeout;
        while (!condition())
        {
            Assert.Less(Time.realtimeSinceStartup, end, $"tiempo agotado esperando: {what}");
            yield return null;
        }
    }

    private static string Hud => Find("HudText").GetComponent<TMP_Text>().GetParsedText();
    private static bool CanAsk => Find("AskButton").GetComponent<Button>().interactable;

    private static IEnumerator StartNewGame()
    {
        yield return Tap("PlayButton");
        yield return Tap("Caso al azar");
        yield return Tap("StartButton");
        Assert.IsTrue(Find("InterrogationPanel").activeInHierarchy);
    }

    private IEnumerator AskAndWait(string question)
    {
        int before = provider.calls;
        Find("QuestionInput").GetComponent<TMP_InputField>().text = question;
        yield return Tap("AskButton");
        yield return WaitUntil(() => provider.calls > before && CanAsk, 8f, "respuesta a " + question);
    }

    private static List<GameObject> Rows(string name)
    {
        GameObject content = Find("ConversationScroll").GetComponent<ScrollRect>().content.gameObject;
        return content.GetComponentsInChildren<Transform>(false).Where(t => t.name == name).Select(t => t.gameObject).ToList();
    }

    // Auditoría finecomb (sesión C): EndDay y Think solo los protegía la interfaz. Con una pregunta en vuelo, terminar
    // el día guardaría un historial con la pregunta sin responder y la respuesta se cobraría al día siguiente.
    [UnityTest]
    public IEnumerator ConUnaPreguntaEnVueloNoSeTerminaElDia()
    {
        yield return StartNewGame();
        provider.delayMs = 1500;
        int before = provider.calls;
        Find("QuestionInput").GetComponent<TMP_InputField>().text = "¿Dónde estaba esa noche?";
        yield return Tap("AskButton");
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        gm.EndDay();
        Assert.IsNull(gm.Think(), "pensar tampoco, con la pregunta en vuelo");
        StringAssert.Contains("DÍA 1", Hud.ToUpperInvariant(), "el día no avanza");
        yield return WaitUntil(() => provider.calls > before && CanAsk, 8f, "respuesta");
        StringAssert.Contains("DÍA 1", Hud.ToUpperInvariant());
    }

    [UnityTest]
    public IEnumerator SinPreguntasNoSeGastaNiSeEnvia()
    {
        yield return StartNewGame();
        for (int i = 0; i < 5; i++)
            yield return AskAndWait($"¿Pregunta {i + 1}?");
        StringAssert.Contains("SIN PREGUNTAS HOY", Hud);

        Find("QuestionInput").GetComponent<TMP_InputField>().text = "¿Una más?";
        yield return Tap("AskButton");
        yield return new WaitForSecondsRealtime(0.4f);
        Assert.AreEqual(5, provider.calls, "la sexta no llega al modelo");
        Assert.AreEqual("¿Una más?", Find("QuestionInput").GetComponent<TMP_InputField>().text, "y no se pierde lo escrito");
    }

    [UnityTest]
    public IEnumerator ElUltimoDiaObligaAAcusarSinVuelta()
    {
        yield return StartNewGame();
        var game = Object.FindFirstObjectByType<GameManager>();
        for (int i = 0; i < 6; i++)
            game.EndDay();
        yield return null;
        StringAssert.Contains("DÍA 7", Hud);
        Assert.IsTrue(game.CanCancelAccusation, "el día 7 aún se puede volver de la acusación");

        game.EndDay();
        yield return null;
        Assert.IsTrue(Find("AccusatonPanel").activeInHierarchy, "al acabar el día 7 toca acusar");
        Assert.IsFalse(game.CanCancelAccusation, "y ya no hay vuelta atrás");
        Assert.IsFalse(Object.FindFirstObjectByType<InterrogationUI>().HandleBack(), "ni con el botón Atrás");
        Assert.IsTrue(Find("AccusatonPanel").activeInHierarchy);
    }

    [UnityTest]
    public IEnumerator AcusarElPrimerDiaConDobleToqueDaUnSoloFinal()
    {
        yield return StartNewGame();
        yield return AskAndWait("¿Dónde estaba?");
        Assert.IsTrue(SaveSystem.Exists);

        Object.FindFirstObjectByType<GameManager>().ForceAccusationPanel();
        yield return new WaitForSecondsRealtime(0.3f);
        yield return Tap("Accusebutton", times: 2);
        yield return WaitUntil(() => Find("ResultPanel").activeInHierarchy, 10f, "el veredicto");
        Assert.IsFalse(SaveSystem.Exists, "la partida terminada no se ofrece para continuar");
        Assert.AreEqual(1, Object.FindObjectsByType<InterrogationUI>(FindObjectsSortMode.None).Length);
    }

    [UnityTest]
    public IEnumerator ElFinalDaRangoPruebaClaveYLoQueSeEscapo()
    {
        yield return StartNewGame();
        var game = Object.FindFirstObjectByType<GameManager>();
        var ui = Object.FindFirstObjectByType<InterrogationUI>();
        var manager = Object.FindFirstObjectByType<AIConversationManager>();
        // Una pista que incrimina ya en la libreta, para elegirla como prueba clave
        ClueData key = manager.State.Variant.clues.First(c => c.kind == ClueKind.Incriminates);
        manager.State.Discover(key.id);
        ui.SetEvidenceOptions(new List<ClueData> { key });

        game.ForceAccusationPanel();
        yield return new WaitForSecondsRealtime(0.3f);
        var suspects = Find("AccusatonPanel").GetComponentsInChildren<TMP_Dropdown>(true);
        TMP_Dropdown who = suspects.First(d => d.name != "PruebaClaveDropdown (auto)");
        TMP_Dropdown keyDropdown = suspects.First(d => d.name == "PruebaClaveDropdown (auto)");
        Assert.IsTrue(keyDropdown.gameObject.activeInHierarchy, "con pistas en la libreta se puede elegir la prueba clave");
        who.value = who.options.FindIndex(o => o.text.StartsWith(manager.Story.Character(manager.State.Variant.culpritId).shortName));
        keyDropdown.value = 1;

        yield return Tap("Accusebutton");
        yield return WaitUntil(() => Find("ResultPanel").activeInHierarchy, 10f, "el veredicto");
        yield return new WaitForSecondsRealtime(0.3f);
        string report = Find("ResultPanel").GetComponentsInChildren<TMP_Text>(true).Select(t => t.text).First(t => t.Contains("TU ACUSACIÓN"));
        StringAssert.Contains("RANGO", report);
        StringAssert.Contains(key.playerName, report, "el informe juzga la prueba clave");
        StringAssert.Contains("SE TE ESCAPÓ", report, "y dice qué pistas faltaron");
        Assert.IsNotNull(CaseRecords.BestRank(manager.Story.id), "el mejor rango queda apuntado");

        // Ronda final 3: el expediente se cierra con la ficha policial del culpable de verdad
        GameObject card = Find(Mugshot.Name);
        Assert.IsNotNull(card, "ficha del culpable al final del informe");
        Assert.IsTrue(card.activeInHierarchy);
        Assert.IsNotNull(card.GetComponentInChildren<RawImage>(true).texture, "con su retrato");
        string culprit = manager.Story.Character(manager.State.Variant.culpritId).name;
        StringAssert.Contains(culprit, card.GetComponentInChildren<TMP_Text>(true).text);
        Assert.AreEqual(Find("ResultPanel").GetComponentsInChildren<TMP_Text>(true).First(t => t.text.Contains("TU ACUSACIÓN")).transform.parent,
                        card.transform.parent, "dentro del informe (se desplaza con él, nunca lo tapa)");
        // Al terminar el informe, la ficha "cae" sobre el expediente con un golpe de sello
        SoundManager.Played.Clear();
        Find("ResultPanel").GetComponentsInChildren<StepReveal>(true).First().OnPointerClick(null);
        yield return new WaitForSecondsRealtime(0.8f);
        CollectionAssert.Contains(SoundManager.Played, Sfx.Stamp);
    }

    [UnityTest]
    public IEnumerator CerrarAMitadDeUnaRespuestaNoGastaLaPregunta()
    {
        yield return StartNewGame();
        yield return AskAndWait("¿Dónde estaba?");
        StringAssert.Contains("QUEDAN 4", Hud);

        // Segunda pregunta en vuelo y se cierra el juego (otra sesión)
        provider.delayMs = 3000;
        Find("QuestionInput").GetComponent<TMP_InputField>().text = "¿Y después?";
        yield return Tap("AskButton");
        LogAssert.ignoreFailingMessages = true; // La respuesta huérfana de la sesión anterior puede llegar tarde
        provider.delayMs = 150;
        yield return LoadGame();
        yield return Tap("ContinueButton (auto)");

        Assert.IsTrue(Find("InterrogationPanel").activeInHierarchy);
        StringAssert.Contains("QUEDAN 4", Hud, "la pregunta que no llegó a contestarse no cuenta");
        Assert.AreEqual(2, Rows("Burbuja (auto)").Count, "solo la pregunta y la respuesta que se guardaron");
        yield return new WaitForSecondsRealtime(3.2f);
        LogAssert.ignoreFailingMessages = false;
    }

    [UnityTest]
    public IEnumerator ConUnaPeticionEnVueloNoSeCambiaDeSospechosoNiDeDia()
    {
        yield return StartNewGame();
        provider.delayMs = 1500;
        var dropdown = Find("SuspectDropdown").GetComponent<TMP_Dropdown>();
        int suspect = dropdown.value;
        Find("QuestionInput").GetComponent<TMP_InputField>().text = "¿Dónde estaba?";
        yield return Tap("AskButton");

        Assert.IsFalse(dropdown.interactable, "el selector de sospechoso se bloquea");
        Assert.IsFalse(Find("EndDayButton").GetComponent<Button>().interactable);
        Assert.IsFalse(Find("AcuseNowButton").GetComponent<Button>().interactable);
        yield return Tap("EndDayButton");
        Assert.IsTrue(Find("ConfirmarFinDelDia") == null || !Find("ConfirmarFinDelDia").activeInHierarchy, "Fin del día no responde");

        // Ni desde la libreta
        Object.FindFirstObjectByType<GameManager>().SendMessage("RefreshNotebook", SendMessageOptions.DontRequireReceiver);
        var link = Find("CluesPanel").GetComponentInChildren<TextLinkHandler>(true);
        string other = Object.FindFirstObjectByType<GameManager>() != null ? "hermano" : null;
        link.onLink(Notebook.SuspectLinkPrefix + other);
        Assert.AreEqual(suspect, dropdown.value);

        yield return WaitUntil(() => CanAsk, 5f, "respuesta");
        Assert.IsTrue(dropdown.interactable, "al llegar la respuesta se puede cambiar otra vez");
    }

    [UnityTest]
    public IEnumerator OllamaCaidoYDeVuelta()
    {
        yield return StartNewGame();
        provider.results.Enqueue(LLMResult.Fail("No se pudo conectar con Ollama."));
        Find("QuestionInput").GetComponent<TMP_InputField>().text = "¿Dónde estaba?";
        yield return Tap("AskButton");
        yield return WaitUntil(() => CanAsk, 5f, "el aviso de fallo");
        StringAssert.Contains("QUEDAN 5", Hud, "la pregunta fallida no se gasta");
        Assert.AreEqual("¿Dónde estaba?", Find("QuestionInput").GetComponent<TMP_InputField>().text, "vuelve al campo");

        yield return Tap("AskButton"); // Ollama ya responde
        yield return WaitUntil(() => provider.calls == 2 && CanAsk, 5f, "la respuesta");
        StringAssert.Contains("QUEDAN 4", Hud);
        Assert.AreEqual(2, Rows("Burbuja (auto)").Count, "una pregunta y una respuesta, sin duplicados");
    }

    [UnityTest]
    public IEnumerator LasDoblesPulsacionesNoDuplicanNada()
    {
        yield return Tap("PlayButton", times: 2);
        yield return Tap("Caso al azar", times: 2);
        yield return Tap("StartButton", times: 2);
        Assert.AreEqual(1, Rows("Dia (auto)").Count, "un solo parte del día 1");

        Find("QuestionInput").GetComponent<TMP_InputField>().text = "¿Dónde estaba?";
        yield return Tap("AskButton", times: 2);
        yield return WaitUntil(() => CanAsk, 5f, "respuesta");
        Assert.AreEqual(1, provider.calls, "una sola petición; burbujas " + Rows("Burbuja (auto)").Count + " · " + string.Join(" | ", Rows("Burbuja (auto)").Select(r => r.GetComponentsInChildren<TMP_Text>().Last().text)));
        StringAssert.Contains("QUEDAN 4", Hud);

        yield return Tap("EndDayButton", times: 2);
        GameObject dialog = Find("ConfirmarFinDelDia");
        Assert.IsTrue(dialog.activeInHierarchy);
        TapNow(dialog.GetComponentsInChildren<Button>().First(b => b.name == ConfirmDialog.ConfirmName).gameObject);
        TapNow(dialog.GetComponentsInChildren<Button>(true).First(b => b.name == ConfirmDialog.ConfirmName).gameObject);
        yield return null;
        StringAssert.Contains("DÍA 2", Hud, "confirmar dos veces no se salta un día");
    }

    // Revisión D3 n.º 2: la ayuda concreta de "Pensar" se lee en el chat del sospechoso al que te lleva
    [UnityTest]
    public IEnumerator LaAyudaConcretaQuedaEnElChatDeQuienSabeAlgo()
    {
        GameSettings.Difficulty = (int)DifficultyLevel.Historia; // Pensar gratis: no gasta preguntas
        yield return StartNewGame();
        var ui = Object.FindFirstObjectByType<InterrogationUI>();
        var dropdown = Find("SuspectDropdown").GetComponent<TMP_Dropdown>();

        ui.OnThinkClick(); // Nivel 1: "Quizá X sabe más de lo que ha contado."
        yield return null;
        string first = ui.Conversations.CurrentEntries.Last().text;
        StringAssert.Contains("sabe más", first);

        // Nos vamos a alguien que NO es quien sabe algo
        int other = Enumerable.Range(0, dropdown.options.Count)
            .First(k => !first.Contains(dropdown.options[k].text.Split(' ')[0]));
        dropdown.value = other;
        yield return null;
        string wrongId = ui.CurrentSuspectId;

        ui.OnThinkClick(); // Nivel 2: "Prueba a preguntarle a X: «…»", cambia de sospechoso
        yield return null;

        Assert.AreNotEqual(wrongId, ui.CurrentSuspectId, "la ayuda concreta lleva a quien sabe algo");
        Assert.IsTrue(ui.Conversations.CurrentEntries.Any(e => e.text != null && e.text.Contains("Prueba a preguntarle")),
                      "el aviso está en el chat que se ve");
        Assert.IsFalse(ui.Conversations.EntriesOf(wrongId).Any(e => e.text != null && e.text.Contains("Prueba a preguntarle")),
                       "y no en el que se deja atrás");
    }

    // Revisión D3 n.º 3: preguntar por un tema desbloquea a quien lo sabe solo si la pregunta llega de verdad
    [UnityTest]
    public IEnumerator UnaPreguntaFallidaNoDesbloqueaANadie()
    {
        yield return StartNewGame();
        var dropdown = Find("SuspectDropdown").GetComponent<TMP_Dropdown>();
        int before = dropdown.options.Count;

        provider.results.Enqueue(LLMResult.Fail("No se pudo conectar con Ollama."));
        Find("QuestionInput").GetComponent<TMP_InputField>().text = "¿Alguna vecina vio algo desde la ventana?";
        yield return Tap("AskButton");
        yield return WaitUntil(() => CanAsk, 5f, "el aviso de fallo");
        yield return null;
        Assert.AreEqual(before, dropdown.options.Count, "la vecina sigue sin aparecer: la pregunta no llegó");

        yield return AskAndWait("¿Alguna vecina vio algo desde la ventana?");
        yield return new WaitForSecondsRealtime(0.5f);
        Assert.Greater(dropdown.options.Count, before, "con la respuesta, sí");
    }

    // Ronda 5: la primera vez que la libreta ya apunta versiones, una indicación enseña a compararlas con las pistas
    [UnityTest]
    public IEnumerator LaLibretaEnsenaAUsarLasVersiones()
    {
        Tutorial.Reset();
        foreach (string id in new[] { Tutorial.Ask, Tutorial.Days, Tutorial.Evidence, Tutorial.Contradiction })
            Tutorial.MarkSeen(id);
        // Historia 1: todas las versiones de inocentes se pueden apuntar desde el principio (en la 3, la de Lucía
        // nombra a Álex, que empieza bloqueado, y espera: es lo correcto, pero no sirve para este test)
        yield return Tap("PlayButton");
        yield return Tap("Caso 1");
        yield return Tap("StartButton");
        // La versión del culpable no se apunta hasta que cuenta su mentira: se pregunta a inocentes hasta que haya una
        var manager = Object.FindFirstObjectByType<AIConversationManager>();
        var dropdown = Find("SuspectDropdown").GetComponent<TMP_Dropdown>();
        var notebook = Find("CluesPanel").GetComponentsInChildren<TMP_Text>(true);
        string culprit = manager.Story.Character(manager.State.Variant.culpritId).shortName;
        for (int k = 0; k < dropdown.options.Count && !notebook.Any(t => t.text.Contains("Dice: «")); k++)
        {
            if (dropdown.options[k].text.StartsWith(culprit))
                continue;
            dropdown.value = k;
            yield return null;
            yield return AskAndWait("¿Dónde estabas esa noche?");
        }
        Assert.IsTrue(notebook.Any(t => t.text.Contains("Dice: «")), "algún inocente ya tiene su versión apuntada");

        yield return Tap("ViewCluesButton");
        yield return new WaitForSecondsRealtime(1.5f);

        Assert.IsTrue(Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)
                .Any(t => t.isActiveAndEnabled && t.text.Contains(Tutorial.TextOf(Tutorial.Versions))),
            "indicación sobre lo que dice cada uno");
        Tutorial.SkipAll();
    }

    // Ronda 9: la nota del jugador se cambia tocándola en la libreta, y la rueda atenúa a quien ha descartado
    [UnityTest]
    public IEnumerator LaNotaDelJugadorSeTocaEnLaLibretaYSeVeEnLaRueda()
    {
        yield return StartNewGame();
        var manager = Object.FindFirstObjectByType<AIConversationManager>();
        var links = Find("CluesPanel").GetComponentInChildren<TextLinkHandler>(true);
        string id = manager.Story.cast.First(c => c.startsUnlocked).id;
        string shortName = manager.Story.Character(id).shortName;

        links.onLink(Notebook.NoteLinkPrefix + id); // sospechoso
        links.onLink(Notebook.NoteLinkPrefix + id); // descartado
        yield return null;
        string notebook = Find("CluesPanel").GetComponentsInChildren<TMP_Text>(true).Select(t => t.text).First(t => t.Contains("SOSPECHOSOS"));
        StringAssert.Contains("tu nota: descarte", notebook);

        Object.FindFirstObjectByType<GameManager>().ForceAccusationPanel();
        yield return new WaitForSecondsRealtime(0.3f);
        GameObject cell = Find("Sospechoso " + shortName);
        Assert.IsNotNull(cell);
        Assert.Less(cell.transform.Find("Marco").GetComponent<CanvasGroup>().alpha, 1f, "descartado: el retrato atenuado en la rueda (pero se puede elegir)");
        Assert.IsNull(cell.GetComponent<CanvasGroup>(), "el anillo de selección y el nombre, a todo color");
        Assert.IsTrue(cell.GetComponent<Button>().interactable);
        StringAssert.Contains("tu descarte", cell.GetComponentInChildren<TMP_Text>().text, "sin género: vale para cualquiera");
    }

    // Revisión ronda 11: tocar la nota mientras el sospechoso "escribe" no guarda medio turno (la pregunta sin
    // respuesta se quedaría en el historial guardado); la nota se guarda al llegar la respuesta
    [UnityTest]
    public IEnumerator UnaNotaDuranteUnaPreguntaNoGuardaMedioTurno()
    {
        yield return StartNewGame();
        var game = Object.FindFirstObjectByType<GameManager>();
        string id = Object.FindFirstObjectByType<AIConversationManager>().Story.cast.First(c => c.startsUnlocked).id;
        provider.delayMs = 1500;
        Find("QuestionInput").GetComponent<TMP_InputField>().text = "¿Dónde estaba a las diez?";
        yield return Tap("AskButton");
        yield return new WaitForSecondsRealtime(0.3f);
        Assert.IsFalse(CanAsk, "la pregunta está en marcha");

        game.CycleNote(id);
        if (SaveSystem.TryLoad(out SaveData during))
            Assert.IsFalse(during.histories.Any(h => h.messages.Count > 0 && h.messages[h.messages.Count - 1].role == "user"),
                "ningún historial guardado acaba en una pregunta sin respuesta");

        yield return WaitUntil(() => CanAsk, 8f, "la respuesta");
        Assert.IsTrue(SaveSystem.TryLoad(out SaveData after));
        CollectionAssert.Contains(after.suspectNotes, id + ":1", "la nota se guarda con la respuesta");
    }

    // Revisión ronda 11: las notas vuelven con "Continuar"
    [UnityTest]
    public IEnumerator LasNotasVuelvenAlContinuar()
    {
        yield return StartNewGame();
        string id = Object.FindFirstObjectByType<AIConversationManager>().Story.cast.First(c => c.startsUnlocked).id;
        Object.FindFirstObjectByType<GameManager>().CycleNote(id);
        yield return AskAndWait("¿Dónde estaba?");

        yield return LoadGame();
        yield return Tap("ContinueButton (auto)");
        yield return null;
        var game = Object.FindFirstObjectByType<GameManager>();
        Assert.IsTrue(game.Notes.TryGetValue(id, out SuspectNote note));
        Assert.AreEqual(SuspectNote.Sospechoso, note);
    }

    // Ronda 13: la rueda marca a quien descarta una pista ya encontrada (en 2B el bot lo acusaba igual)
    [UnityTest]
    public IEnumerator LaRuedaMarcaAQuienDescartaUnaPista()
    {
        yield return StartNewGame();
        var manager = Object.FindFirstObjectByType<AIConversationManager>();
        ClueData clearing = manager.State.Variant.clues.FirstOrDefault(c => c.kind == ClueKind.Clears && manager.Story.Character(c.clears).startsUnlocked);
        if (clearing == null)
            Assert.Ignore("esta variante no tiene pista de descarte");
        manager.State.Discover(clearing.id);
        var game = Object.FindFirstObjectByType<GameManager>();

        game.ForceAccusationPanel();
        yield return new WaitForSecondsRealtime(0.3f);
        GameObject cell = Find("Sospechoso " + manager.Story.Character(clearing.clears).shortName);
        Assert.IsNotNull(cell);
        StringAssert.Contains("pista de descarte", cell.GetComponentInChildren<TMP_Text>().text);
        Assert.IsTrue(cell.GetComponent<Button>().interactable, "se puede elegir igual");
    }

    // Ronda 15: al empezar un día, su parte queda en la libreta para releerlo
    [UnityTest]
    public IEnumerator ElParteDelDiaQuedaEnLaLibreta()
    {
        yield return StartNewGame();
        var game = Object.FindFirstObjectByType<GameManager>();
        game.EndDay();
        yield return null;
        string notebook = Find("CluesPanel").GetComponentsInChildren<TMP_Text>(true).Select(t => t.text).First(t => t.Contains("SOSPECHOSOS"));
        StringAssert.Contains("PARTES DE LA MAÑANA", notebook);
        StringAssert.Contains("Día 2:", notebook);
    }

    // Revisión 5: acusar apunta la variante como vista y el informe dice cuántos culpables quedan
    [UnityTest]
    public IEnumerator AcusarApuntaLaVarianteYElInformeInvitaARejugar()
    {
        yield return StartNewGame();
        string variantId = Object.FindFirstObjectByType<AIConversationManager>().State.Variant.id;
        Assert.IsFalse(CaseRecords.Played(variantId));

        Object.FindFirstObjectByType<GameManager>().ForceAccusationPanel();
        yield return new WaitForSecondsRealtime(0.3f);
        yield return Tap("Accusebutton");
        yield return WaitUntil(() => Find("ResultPanel").activeInHierarchy, 10f, "el veredicto");

        Assert.IsTrue(CaseRecords.Played(variantId), "la variante cuenta como vista");
        string report = string.Join(" ", Find("ResultPanel").GetComponentsInChildren<TMP_Text>(true).Select(t => t.text));
        StringAssert.Contains("culpables posibles", report);
    }

    // Ronda 24: la tarjeta del día nuevo reconoce lo conseguido ayer
    [UnityTest]
    public IEnumerator LaTarjetaDelDiaDiceLoConseguidoAyer()
    {
        yield return StartNewGame();
        var manager = Object.FindFirstObjectByType<AIConversationManager>();
        manager.State.Discover(manager.State.Variant.clues[0].id);
        Object.FindFirstObjectByType<GameManager>().EndDay();
        yield return new WaitForSecondsRealtime(0.5f);
        Assert.IsTrue(Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Any(t => t.text.Contains("Ayer: una pista nueva.")),
            "la tarjeta del día 2 lo dice");
    }

    // Revisión 7: continuar a mitad de día no pierde lo conseguido antes de guardar
    [UnityTest]
    public IEnumerator AlContinuarElDiaRecuerdaLoDeAntesDeGuardar()
    {
        yield return StartNewGame();
        var manager = Object.FindFirstObjectByType<AIConversationManager>();
        manager.State.Discover(manager.State.Variant.clues[0].id);
        yield return AskAndWait("¿Dónde estaba?"); // Guarda

        yield return LoadGame();
        yield return Tap("ContinueButton (auto)");
        yield return null;
        Object.FindFirstObjectByType<GameManager>().EndDay();
        yield return new WaitForSecondsRealtime(0.5f);
        Assert.IsTrue(Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Any(t => t.text.Contains("Ayer: una pista nueva.")));
    }

    // ---- Sesión A, bloque 1: cada partida nueva empieza con memoria cero ----

    private const string Marker = "ZAFIRO";

    // Tras una recarga de escena (Jugar otra vez, Reiniciar, Volver al menú) hay un gestor nuevo: se le da el proveedor
    private IEnumerator AfterReload()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        Object.FindFirstObjectByType<AIConversationManager>().UseProvider(provider);
        yield return null;
    }

    private IEnumerator PlayFirstGameWithMarker(bool accuse)
    {
        yield return StartNewGame();
        yield return AskAndWait($"¿Qué hacía con el {Marker} esa noche?");
        Assert.IsTrue(provider.sent.Last().Contains(Marker), "el marcador llega en la partida 1");
        if (accuse)
        {
            Object.FindFirstObjectByType<GameManager>().ForceAccusationPanel();
            yield return new WaitForSecondsRealtime(0.3f);
            yield return Tap("Accusebutton");
            yield return WaitUntil(() => Find("ResultPanel").activeInHierarchy, 10f, "el veredicto");
        }
    }

    private IEnumerator AssertSecondGameStartsClean(string path)
    {
        var manager = Object.FindFirstObjectByType<AIConversationManager>();
        Assert.IsTrue(manager.Histories.All(h => h.Value.Count == 0), path + ": historiales vacíos al empezar");
        int before = provider.sent.Count;
        yield return AskAndWait("¿Dónde estaba a las diez?");
        Assert.Greater(provider.sent.Count, before);
        foreach (string prompt in provider.sent.Skip(before))
            StringAssert.DoesNotContain(Marker, prompt, path + ": el prompt de la partida 2 no contiene nada de la 1");
    }

    [UnityTest]
    public IEnumerator JugarOtraVezEmpiezaSinMemoria()
    {
        yield return PlayFirstGameWithMarker(accuse: true);
        yield return Tap("RestartButton");
        yield return AfterReload();
        yield return Tap("Caso al azar");
        yield return Tap("StartButton");
        yield return AssertSecondGameStartsClean("Jugar otra vez");
    }

    [UnityTest]
    public IEnumerator ReiniciarDesdeAjustesEmpiezaSinMemoria()
    {
        yield return PlayFirstGameWithMarker(accuse: false);
        Object.FindFirstObjectByType<GameManager>().RestartGame(); // Lo que hace "Empezar de nuevo" en Ajustes
        yield return AfterReload();
        yield return Tap("Caso al azar");
        yield return Tap("StartButton");
        yield return AssertSecondGameStartsClean("Reiniciar");
    }

    [UnityTest]
    public IEnumerator CasoNuevoDesdeElMenuEmpiezaSinMemoria()
    {
        yield return PlayFirstGameWithMarker(accuse: false);
        Object.FindFirstObjectByType<GameManager>().BackToMenu();
        yield return AfterReload();
        yield return Tap("PlayButton"); // "Caso nuevo" (hay partida guardada)
        yield return Tap("Caso al azar");
        yield return Tap(ConfirmDialog.ConfirmName); // "Empezar de nuevo": se pierde la investigación a medias
        yield return Tap("StartButton");
        yield return AssertSecondGameStartsClean("Caso nuevo desde el menú");
    }

    [UnityTest]
    public IEnumerator LaMismaHistoriaOtraVezEmpiezaSinMemoria()
    {
        yield return PlayFirstGameWithMarker(accuse: true);
        string storyId = Object.FindFirstObjectByType<AIConversationManager>().Story.id;
        yield return Tap("RestartButton");
        yield return AfterReload();
        yield return Tap("Caso " + storyId);
        yield return Tap("StartButton");
        yield return AssertSecondGameStartsClean("Misma historia");
    }

    [UnityTest]
    public IEnumerator ContinuarConservaSoloLaMemoriaDeEsaPartida()
    {
        yield return PlayFirstGameWithMarker(accuse: false);
        Object.FindFirstObjectByType<GameManager>().BackToMenu();
        yield return AfterReload();
        yield return Tap("ContinueButton (auto)");
        yield return null;
        var manager = Object.FindFirstObjectByType<AIConversationManager>();
        Assert.IsTrue(manager.Histories.Any(h => h.Value.Any(m => m.content.Contains(Marker))), "Continuar recuerda esa partida");
        int before = provider.sent.Count;
        yield return AskAndWait("¿Y después?");
        Assert.IsTrue(provider.sent.Skip(before).Any(p => p.Contains(Marker)), "y se lo manda al modelo");
    }

    // ---- Sesión A, bloque 4: la escena del interrogatorio ----

    [UnityTest]
    public IEnumerator LaEscenaMuestraAlSospechosoYLaViñetaSubeConLaTension()
    {
        yield return StartNewGame();
        var scene = Object.FindFirstObjectByType<InterrogationScene>();
        Assert.IsNotNull(scene, "la escena existe");
        yield return new WaitForSecondsRealtime(0.6f);
        Assert.IsNotNull(scene.Stage.texture, "el sospechoso de cuerpo entero en el hueco del centro");
        Assert.Greater(scene.Stage.color.a, 0.05f);
        Assert.Less(scene.Stage.color.a, 0.6f, "tenue: no compite con el chat");
        Assert.AreEqual(0f, scene.VignetteAlpha, 0.01f, "tranquilo: sin viñeta");

        var ui = Object.FindFirstObjectByType<InterrogationUI>();
        string id = Object.FindFirstObjectByType<AIConversationManager>().Story.cast.First(c => c.startsUnlocked).id;
        Find("SuspectDropdown").GetComponent<TMP_Dropdown>().value = 0;
        ui.SetEmotion(id, Emotion.Nervioso);
        ui.SetEmotion(Object.FindFirstObjectByType<AIConversationManager>().Story.cast.Where(c => c.startsUnlocked).Select(c => c.id).First(), Emotion.Nervioso);
        yield return new WaitForSecondsRealtime(0.9f);
        Assert.Greater(scene.VignetteAlpha, 0.2f, "nervioso: los bordes se oscurecen");
    }

    [UnityTest]
    public IEnumerator ConReducirAnimacionesElCambioDeSospechosoNoMueveNada()
    {
        GameSettings.ReduceMotion = true;
        try
        {
            yield return StartNewGame();
            var scene = Object.FindFirstObjectByType<InterrogationScene>();
            var dropdown = Find("SuspectDropdown").GetComponent<TMP_Dropdown>();
            dropdown.value = (dropdown.value + 1) % dropdown.options.Count;
            yield return null;
            Assert.AreEqual(0f, scene.Stage.rectTransform.anchoredPosition.x, 0.01f, "sin deslizamiento");
            Assert.Less(scene.Entry, 1f, "pero sí un fundido");
        }
        finally
        {
            GameSettings.ReduceMotion = false;
        }
    }

    [UnityTest]
    public IEnumerator LaEscenaSeApagaDesdeElTema()
    {
        yield return StartNewGame();
        var scene = Object.FindFirstObjectByType<InterrogationScene>();
        bool stage = ThemeManager.Current.interrogationStage, vignette = ThemeManager.Current.tensionVignette;
        try
        {
            ThemeManager.Current.interrogationStage = false;
            ThemeManager.Current.tensionVignette = false;
            scene.ApplyTheme();
            Assert.IsFalse(scene.Stage.transform.parent.gameObject.activeSelf);
            Assert.IsFalse(scene.Vignette.gameObject.activeSelf);
        }
        finally
        {
            ThemeManager.Current.interrogationStage = stage;
            ThemeManager.Current.tensionVignette = vignette;
            scene.ApplyTheme();
        }
    }
}
