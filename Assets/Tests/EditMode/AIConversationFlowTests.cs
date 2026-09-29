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

        public string DisplayName => "Falso";

        public Task<LLMResult> SendAsync(string systemPrompt, IReadOnlyList<ChatMessage> history, int maxTokens, float temperature)
        {
            lastHistoryCount = history.Count;
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
}
