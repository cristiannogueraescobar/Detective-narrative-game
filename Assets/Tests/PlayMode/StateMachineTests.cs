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

        public async Task<LLMResult> SendAsync(string systemPrompt, IReadOnlyList<ChatMessage> history, int maxTokens, float temperature)
        {
            calls++;
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
}
