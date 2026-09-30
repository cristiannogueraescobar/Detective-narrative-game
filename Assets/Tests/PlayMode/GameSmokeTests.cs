using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// PARTIDA DE HUMO (Play Mode): carga Game.unity de verdad, pulsa los botones como un jugador y pregunta a un
/// proveedor falso que tarda un poco. Comprueba lo que solo se ve en juego: "escribiendo…", la máquina de
/// escribir, el toque para completar, el guardado y que no salga ningún error en la consola.
/// </summary>
public class GameSmokeTests
{
    private class SlowFakeProvider : ILLMProvider
    {
        public readonly Queue<LLMResult> results = new Queue<LLMResult>();
        public int delayMs = 400;
        public int calls;

        public string DisplayName => "Falso lento";

        public async Task<LLMResult> SendAsync(string systemPrompt, IReadOnlyList<ChatMessage> history, int maxTokens, float temperature)
        {
            calls++;
            await Task.Delay(delayMs);
            return results.Count > 0 ? results.Dequeue()
                : LLMResult.Ok("Aquella noche estuve en casa, como siempre. No oí nada raro, se lo juro por lo que más quiera. [ESTADO: nervioso]");
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
    private SlowFakeProvider provider;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        saveDirectory = Path.Combine(Path.GetTempPath(), "detective-smoke-" + System.Guid.NewGuid().ToString("N"));
        SaveSystem.DirectoryOverride = saveDirectory;
        GameSettings.UseStore(new MemoryStore());

        yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
        yield return null;

        provider = new SlowFakeProvider();
        Object.FindFirstObjectByType<AIConversationManager>().UseProvider(provider);
        yield return null;
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

    private static GameObject Find(string name)
    {
        return Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == name && g.scene.IsValid());
    }

    private static IEnumerator Click(string buttonName)
    {
        GameObject go = Find(buttonName);
        Assert.IsNotNull(go, $"no existe el botón {buttonName}");
        Assert.IsTrue(go.activeInHierarchy, $"{buttonName} no está visible");
        var button = go.GetComponent<Button>();
        Assert.IsTrue(button.interactable, $"{buttonName} no se puede pulsar");
        button.onClick.Invoke();
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

    private static List<GameObject> Rows(string name)
    {
        GameObject content = Find("ConversationScroll").GetComponent<ScrollRect>().content.gameObject;
        return content.GetComponentsInChildren<Transform>(false).Where(t => t.name == name).Select(t => t.gameObject).ToList();
    }

    private static IEnumerator StartNewGame()
    {
        yield return Click("PlayButton");
        yield return Click("Caso al azar");
        Assert.IsTrue(Find("IntroPanel").activeInHierarchy, "tras Jugar se ve la intro del caso");
        yield return Click("StartButton");
        Assert.IsTrue(Find("InterrogationPanel").activeInHierarchy, "tras Empezar se ve el interrogatorio");
    }

    private static IEnumerator Ask(string question)
    {
        Find("QuestionInput").GetComponent<TMP_InputField>().text = question;
        yield return Click("AskButton");
    }

    [UnityTest]
    public IEnumerator PreguntarMuestraBurbujaEscribiendoYRespuesta()
    {
        yield return StartNewGame();
        int before = Rows("Burbuja (auto)").Count;

        yield return Ask("¿Dónde estaba usted anoche?");

        // Al momento: la pregunta y "escribiendo…"
        Assert.AreEqual(before + 1, Rows("Burbuja (auto)").Count, "la pregunta aparece al enviar");
        Assert.AreEqual(1, Rows("Escribiendo (auto)").Count, "se ve «escribiendo…» mientras responde");
        Assert.IsFalse(Find("AskButton").GetComponent<Button>().interactable, "no se puede preguntar dos veces a la vez");

        // Llega la respuesta: burbuja que se escribe letra a letra
        yield return WaitUntil(() => Rows("Burbuja (auto)").Count == before + 2, 5f, "respuesta");
        Assert.AreEqual(0, Rows("Escribiendo (auto)").Count, "«escribiendo…» desaparece");

        ChatView chat = Find("ConversationScroll").GetComponent<ChatView>();
        Assert.IsTrue(chat.IsTyping, "la respuesta se está escribiendo");
        TMP_Text answer = Rows("Burbuja (auto)").Last().GetComponentsInChildren<TMP_Text>().First(t => t.name == "Texto");
        StringAssert.DoesNotContain("[ESTADO", answer.text, "la etiqueta de estado no se ve");
        Assert.Less(answer.maxVisibleCharacters, answer.text.Length, "empieza a medio escribir");

        // Un toque la completa
        chat.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
        yield return null;
        Assert.IsFalse(chat.IsTyping);
        Assert.GreaterOrEqual(answer.maxVisibleCharacters, answer.text.Length);

        Assert.IsTrue(Find("AskButton").GetComponent<Button>().interactable, "se puede volver a preguntar");
        Assert.IsTrue(SaveSystem.Exists, "la partida se guarda tras la respuesta");
        CollectionAssert.Contains(SoundManager.Played, Sfx.Send, "suena el envío (aunque no haya archivo)");
        CollectionAssert.Contains(SoundManager.Played, Sfx.Answer);
        Assert.AreNotEqual(Music.Menu, SoundManager.CurrentMusic, "en el interrogatorio suena la música de la historia");
    }

    [UnityTest]
    public IEnumerator PeticionFallidaRetiraLaPreguntaYLaDevuelveAlCampo()
    {
        yield return StartNewGame();
        provider.results.Enqueue(LLMResult.Fail("Sin conexión con el modelo."));
        int before = Rows("Burbuja (auto)").Count;

        yield return Ask("¿Conocía a la víctima?");
        yield return WaitUntil(() => Find("AskButton").GetComponent<Button>().interactable, 5f, "fallo");

        Assert.AreEqual(before, Rows("Burbuja (auto)").Count, "la pregunta fallida no se queda en el chat");
        Assert.AreEqual("¿Conocía a la víctima?", Find("QuestionInput").GetComponent<TMP_InputField>().text);
        Assert.AreEqual(0, Rows("Escribiendo (auto)").Count);
    }

    [UnityTest]
    public IEnumerator ContinuarRecuperaLaConversacion()
    {
        yield return StartNewGame();
        yield return Ask("¿Qué hacía a las diez?");
        yield return WaitUntil(() => !Find("ConversationScroll").GetComponent<ChatView>().IsTyping && Rows("Escribiendo (auto)").Count == 0 && provider.calls == 1, 8f, "respuesta");
        yield return new WaitForSecondsRealtime(0.2f);
        int rows = Rows("Burbuja (auto)").Count;

        // Otra sesión: la escena se recarga y se pulsa "Continuar"
        yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
        yield return null;
        Object.FindFirstObjectByType<AIConversationManager>().UseProvider(provider);
        yield return null;
        yield return Click("ContinueButton (auto)");

        Assert.IsTrue(Find("InterrogationPanel").activeInHierarchy);
        Assert.AreEqual(rows, Rows("Burbuja (auto)").Count, "las burbujas vuelven tal cual");
    }

    [UnityTest]
    public IEnumerator LaPrimeraPartidaEnsenaAPreguntarYSePuedeSaltar()
    {
        yield return StartNewGame();
        yield return WaitUntil(() => Find("Indicacion (auto)") != null && Find("Indicacion (auto)").activeInHierarchy, 3f, "indicación de preguntar");

        GameObject skip = Find("Indicacion (auto)").GetComponentsInChildren<Button>().First(b => b.name == "Saltar tutorial").gameObject;
        skip.GetComponent<Button>().onClick.Invoke();
        yield return null;

        Assert.IsTrue(Find("Indicacion (auto)") == null, "la indicación se cierra");
        Assert.IsTrue(Tutorial.Finished, "saltar apaga todo el tutorial");
    }

    [UnityTest]
    public IEnumerator VolverDeLaAcusacionRecuperaLaMusicaDelCaso()
    {
        yield return StartNewGame();
        Music story = SoundManager.CurrentMusic;
        var game = Object.FindFirstObjectByType<GameManager>();

        game.ForceAccusationPanel();
        yield return null;
        Assert.AreEqual(Music.Tension, SoundManager.CurrentMusic);

        game.CancelAccusation();
        yield return null;
        Assert.AreEqual(story, SoundManager.CurrentMusic, "tras «Volver» suena otra vez la música de la historia");
        Assert.IsTrue(Find("InterrogationPanel").activeInHierarchy);
    }

    [UnityTest]
    public IEnumerator FinDelDiaMuestraElCalendarioYSeCierraConToques()
    {
        yield return StartNewGame();
        yield return Click("EndDayButton");
        yield return new WaitForSecondsRealtime(0.2f);

        GameObject card = Find("Nuevo dia (auto)");
        Assert.IsNotNull(card, "aparece la hoja del nuevo día");
        StringAssert.Contains("DÍA 2", Find("HudText").GetComponent<TMP_Text>().GetParsedText());

        var tap = card.GetComponent<TapHandler>();
        tap.onTap(); // Completa el parte si se está escribiendo
        yield return null;
        tap.onTap(); // Cierra
        yield return new WaitForSecondsRealtime(0.5f);
        Assert.IsTrue(Find("Nuevo dia (auto)") == null, "la hoja se cierra con toques");
        Assert.GreaterOrEqual(Rows("Dia (auto)").Count, 2, "el chat tiene el día 1 y el día 2");
    }

    [UnityTest]
    public IEnumerator ElBotonAtrasVuelveUnPasoSinSalirDelJuego()
    {
        var router = Object.FindFirstObjectByType<BackButtonRouter>();
        Assert.IsNotNull(router);

        yield return Click("InstructionsButton");
        Assert.IsTrue(router.Back());
        yield return null;
        Assert.IsTrue(Find("MainMenuPanel").activeInHierarchy, "de instrucciones al menú");
        Assert.IsFalse(router.Back(), "en el menú no hace nada");

        yield return Click("PlayButton");
        Assert.IsTrue(router.Back(), "cierra la selección de caso");
        yield return null;

        yield return StartNewGame();
        yield return Click("ViewCluesButton");
        Assert.IsTrue(router.Back());
        yield return null;
        Assert.IsFalse(Find("CluesPanel").activeInHierarchy, "cierra la libreta");
        Assert.IsFalse(router.Back(), "en el interrogatorio no hace nada");
    }

    [UnityTest]
    public IEnumerator JugarOtraVezAbreLaSeleccionDeCaso()
    {
        yield return StartNewGame();
        Object.FindFirstObjectByType<GameManager>().RestartGame(); // Lo que hace "Jugar otra vez"
        yield return null;
        yield return null;
        Object.FindFirstObjectByType<AIConversationManager>().UseProvider(provider);
        yield return new WaitForSecondsRealtime(0.3f);

        GameObject select = Find("Casos (auto)");
        Assert.IsNotNull(select);
        Assert.IsTrue(select.activeInHierarchy, "tras «Jugar otra vez» se elige caso");
        Assert.IsFalse(SaveSystem.Exists, "la partida anterior no se ofrece para continuar");
    }
}
