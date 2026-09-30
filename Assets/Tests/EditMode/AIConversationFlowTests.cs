using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Flujo real de AIConversationManager con un proveedor falso: fallos, texto limpio, estado e historial acotado.
/// </summary>
public class AIConversationFlowTests
{
    private class FakeProvider : ILLMProvider
    {
        public readonly Queue<LLMResult> results = new Queue<LLMResult>();
        public int lastHistoryCount;
        public string lastFirstRole;
        public float lastTemperature;
        public int calls;
        public string lastUserMessage;

        public string DisplayName => "Falso";

        public Task<LLMResult> SendAsync(string systemPrompt, IReadOnlyList<ChatMessage> history, int maxTokens, float temperature)
        {
            lastHistoryCount = history.Count;
            lastTemperature = temperature;
            calls++;
            lastUserMessage = history.Count > 0 ? history[history.Count - 1].content : null;
            lastFirstRole = history.Count > 0 ? history[0].role : null;
            return Task.FromResult(results.Count > 0 ? results.Dequeue() : LLMResult.Ok("Sin más. [ESTADO: tranquilo]"));
        }

        public Task WarmUpAsync() => Task.CompletedTask;
    }

    private GameObject host;
    private AIConversationManager manager;
    private FakeProvider provider;

    [SetUp]
    public void SetUp()
    {
        host = new GameObject("AIConversationFlowTests");
        manager = host.AddComponent<AIConversationManager>();
        provider = new FakeProvider();
        manager.UseProvider(provider);

        StoryData story = TestCases.Story();
        manager.StartCase(story, story.variants[0]);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(host);
    }

    private LLMResult Ask(string characterId, string question, ClueData shown = null)
    {
        return manager.AskSuspect(characterId, question, 1, shown).GetAwaiter().GetResult();
    }

    [Test]
    public void UnaRespuestaIdenticaALaAnteriorSePideOtraVez()
    {
        provider.results.Enqueue(LLMResult.Ok("Estuve en casa toda la noche. [ESTADO: nervioso]"));
        provider.results.Enqueue(LLMResult.Ok("Estuve en casa toda la noche. [ESTADO: nervioso]"));
        provider.results.Enqueue(LLMResult.Ok("Ya se lo he dicho: no salí de casa. [ESTADO: enfadado]"));

        Ask("a", "¿Dónde estaba?");
        int before = provider.calls;
        LLMResult second = Ask("a", "¿Y a las once?");

        Assert.AreEqual(before + 2, provider.calls, "se repite la petición una vez");
        Assert.AreEqual("Ya se lo he dicho: no salí de casa.", second.Text);
        Assert.Greater(provider.lastTemperature, AIConversationManager.DefaultTemperature, "con algo más de variedad");
        StringAssert.Contains("No repitas", provider.lastUserMessage, "y se le pide que no se repita");
    }

    [Test]
    public void LaNotaDelReintentoNoSeQuedaEnElHistorial()
    {
        provider.results.Enqueue(LLMResult.Ok("Estuve en casa. [ESTADO: nervioso]"));
        provider.results.Enqueue(LLMResult.Ok("Estuve en casa. [ESTADO: nervioso]"));
        provider.results.Enqueue(LLMResult.Ok("Ya se lo dije. [ESTADO: enfadado]"));
        Ask("a", "¿Dónde estaba?");
        Ask("a", "¿Seguro?");

        Ask("a", "Otra");
        foreach (ChatMessage m in manager.Histories["a"])
            StringAssert.DoesNotContain("No repitas", m.content);
    }

    [Test]
    public void UnaHoraInventadaSePideOtraVez()
    {
        provider.results.Enqueue(LLMResult.Ok("La vi en el salón a las 21:47. [ESTADO: tranquilo]"));
        provider.results.Enqueue(LLMResult.Ok("La vi en el salón, no me fijé en la hora. [ESTADO: tranquilo]"));

        LLMResult answer = Ask("a", "¿Cuándo la vio por última vez?");

        Assert.AreEqual(2, provider.calls, "se pide una vez más");
        Assert.AreEqual("La vi en el salón, no me fijé en la hora.", answer.Text);
        StringAssert.Contains("horas", provider.lastUserMessage, "con una nota sobre las horas");
        foreach (ChatMessage m in manager.Histories["a"])
            StringAssert.DoesNotContain(AIConversationManager.TimeNudge.Trim(), m.content, "la nota no se queda en el historial");
    }

    [Test]
    public void SiElReintentoTambienInventaSeQuedaConLaQueMenosInventa()
    {
        provider.results.Enqueue(LLMResult.Ok("A las 21:47 cené. [ESTADO: tranquilo]"));
        provider.results.Enqueue(LLMResult.Ok("A las 21:47 cené y a las 22:13 me acosté. [ESTADO: tranquilo]"));
        Assert.AreEqual("A las 21:47 cené.", Ask("a", "¿Qué hizo?").Text, "el reintento no mejora: se queda la primera");
        Assert.AreEqual(2, provider.calls, "un solo reintento: nunca bloquea la partida");
    }

    [Test]
    public void LaHoraQueDijoElInspectorNoProvocaReintento()
    {
        provider.results.Enqueue(LLMResult.Ok("A las 10:37 estaba en casa. [ESTADO: tranquilo]"));
        Ask("a", "¿Dónde estaba a las 10:37?");
        Assert.AreEqual(1, provider.calls);
    }

    // Revisión D3 n.º 6: una hora que el personaje ya dijo (y se aceptó) no provoca otro reintento cada vez
    [Test]
    public void LaHoraQueElPersonajeYaDijoNoProvocaOtroReintento()
    {
        provider.results.Enqueue(LLMResult.Ok("A las 21:47 cené. [ESTADO: tranquilo]"));
        provider.results.Enqueue(LLMResult.Ok("A las 21:47 cené, ya se lo he dicho. [ESTADO: tranquilo]"));
        Ask("a", "¿Qué hizo?");
        int before = provider.calls;

        provider.results.Enqueue(LLMResult.Ok("Lo repito: a las 21:47 estaba cenando. [ESTADO: nervioso]"));
        Ask("a", "¿Seguro?");

        Assert.AreEqual(before + 1, provider.calls, "sin reintento: esa hora ya es parte de su versión");
    }

    [Test]
    public void ElReintentoPorHorasNoAceptaUnaRepeticion()
    {
        provider.results.Enqueue(LLMResult.Ok("Estuve en casa toda la noche. [ESTADO: tranquilo]"));
        Ask("a", "¿Dónde estuvo?");
        provider.results.Enqueue(LLMResult.Ok("La vi a las 21:47. [ESTADO: tranquilo]"));
        provider.results.Enqueue(LLMResult.Ok("Estuve en casa toda la noche. [ESTADO: tranquilo]"));

        Assert.AreEqual("La vi a las 21:47.", Ask("a", "¿Y la vio?").Text, "mejor una hora dudosa que repetir lo mismo");
    }

    // Ronda 5 (bot): qwen puede pasarse al chino y, con eso en el historial, seguir en chino toda la partida
    [Test]
    public void UnaRespuestaEnOtroIdiomaSePideOtraVezEnEspanol()
    {
        provider.results.Enqueue(LLMResult.Ok("我存放在一个专用的小棚子里。 [ESTADO: tranquilo]"));
        provider.results.Enqueue(LLMResult.Ok("Las guardo en el cobertizo, junto al almacén. [ESTADO: tranquilo]"));

        LLMResult answer = Ask("a", "¿Dónde guardas las garrafas?");

        Assert.AreEqual(2, provider.calls, "se pide una vez más");
        Assert.AreEqual("Las guardo en el cobertizo, junto al almacén.", answer.Text);
        StringAssert.Contains("español", provider.lastUserMessage);
        foreach (ChatMessage m in manager.Histories["a"])
            Assert.IsFalse(LanguageCheck.IsForeign(m.content), "el chino no se queda en el historial");
    }

    [Test]
    public void EnEspanolNoHayReintentoPorIdioma()
    {
        provider.results.Enqueue(LLMResult.Ok("Las guardo en el cobertizo, junto al almacén. [ESTADO: tranquilo]"));
        Ask("a", "¿Dónde guardas las garrafas?");
        Assert.AreEqual(1, provider.calls);
    }

    [Test]
    public void ElReintentoPorHorasSePuedeDesactivar()
    {
        manager.RetryInventedTimes = false;
        provider.results.Enqueue(LLMResult.Ok("La vi a las 21:47. [ESTADO: tranquilo]"));
        Ask("a", "¿Cuándo la vio?");
        Assert.AreEqual(1, provider.calls);
    }

    [Test]
    public void SiVuelveARepetirSeQuedaConLaRespuesta()
    {
        for (int i = 0; i < 3; i++)
            provider.results.Enqueue(LLMResult.Ok("Estuve en casa. [ESTADO: nervioso]"));

        Ask("a", "¿Dónde estaba?");
        LLMResult second = Ask("a", "¿Seguro?");

        Assert.IsTrue(second.Success);
        Assert.AreEqual("Estuve en casa.", second.Text, "un solo reintento: nunca bloquea la partida");
    }

    [Test]
    public void PeticionFallidaNoTocaHistorialNiRegistraLaPrueba()
    {
        ClueData x = manager.State.Variant.Clue("x");
        manager.State.Discover("x");
        provider.results.Enqueue(LLMResult.Fail("Ollama caído"));

        LLMResult failed = Ask("a", "¿Y esto?", x);

        Assert.IsFalse(failed.Success);
        CollectionAssert.IsEmpty(manager.State.ShownTo("a"));

        Ask("a", "Otra pregunta");
        Assert.AreEqual(1, provider.lastHistoryCount, "la pregunta fallida no queda en el historial");
    }

    [Test]
    public void ExitoDevuelveTextoLimpioYActualizaEstadoYPistas()
    {
        provider.results.Enqueue(LLMResult.Ok("Vi la cortina x desde casa.\n[ESTADO: nervioso]"));
        var revealed = new List<string>();
        manager.OnClueRevealed += clue => revealed.Add(clue.id);

        LLMResult result = Ask("c", "¿Qué vio?");

        Assert.AreEqual("Vi la cortina x desde casa.", result.Text);
        Assert.AreEqual(Emotion.Nervioso, manager.CurrentEmotion("c"));
        CollectionAssert.AreEqual(new[] { "x" }, revealed);
    }

    [Test]
    public void SinEtiquetaElEstadoSigueSiendoElAnterior()
    {
        provider.results.Enqueue(LLMResult.Ok("Estoy harto. [ESTADO: enfadado]"));
        provider.results.Enqueue(LLMResult.Ok("No sé nada más."));

        Ask("b", "Uno");
        Ask("b", "Dos");

        Assert.AreEqual(Emotion.Enfadado, manager.CurrentEmotion("b"));
    }

    [Test]
    public void HistorialEnviadoAcotadoYEmpiezaPorElJugador()
    {
        for (int i = 0; i < 12; i++)
            Ask("b", $"Pregunta {i}");

        Assert.LessOrEqual(provider.lastHistoryCount, AIConversationManager.MaxHistoryMessages);
        Assert.AreEqual("user", provider.lastFirstRole);
    }

    [Test]
    public void RespuestaSoloConEtiquetaNoQuedaVacia()
    {
        provider.results.Enqueue(LLMResult.Ok("[ESTADO: triste]"));

        LLMResult result = Ask("b", "¿Cómo está?");

        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Text));
        Assert.AreEqual(Emotion.Triste, manager.CurrentEmotion("b"));
    }

    [Test]
    public void SinCasoEnCursoFallaSinExcepcion()
    {
        var bare = new GameObject("SinCaso").AddComponent<AIConversationManager>();
        bare.UseProvider(provider);

        LLMResult result = bare.AskSuspect("a", "Hola", 1, null).GetAwaiter().GetResult();

        Assert.IsFalse(result.Success);
        Object.DestroyImmediate(bare.gameObject);
    }

    [Test]
    public void ElHistorialGuardaLaEtiquetaBienEscrita()
    {
        provider.results.Enqueue(LLMResult.Ok("No estuve allí. [MESTADO: asustado]"));
        Ask("a", "¿Estuvo allí?");
        ChatMessage last = manager.Histories["a"].Last(m => m.role == "assistant");
        StringAssert.Contains("[ESTADO: asustado]", last.content, "el modelo no copia la errata en la siguiente");
        StringAssert.DoesNotContain("MESTADO", last.content);
    }
}
