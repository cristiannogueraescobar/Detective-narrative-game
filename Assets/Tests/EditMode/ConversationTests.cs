using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class ConversationTests
{
    // ---------- Mensaje del jugador ----------

    [Test]
    public void MensajeSinPruebaEsLaPregunta()
    {
        Assert.AreEqual("¿Dónde estaba?", TurnAnalyzer.BuildUserMessage("  ¿Dónde estaba?  ", null));
    }

    [Test]
    public void MensajeConPruebaAnteponeLaPrueba()
    {
        ClueData clue = TestCases.Variant().Clue("x");

        Assert.AreEqual("[El inspector te muestra una prueba: Resumen x]\n¿Y esto?",
            TurnAnalyzer.BuildUserMessage("¿Y esto?", clue));
    }

    [Test]
    public void MensajeConPruebaYSinPreguntaUsaPreguntaPorDefecto()
    {
        ClueData clue = TestCases.Variant().Clue("x");

        StringAssert.EndsWith(TurnAnalyzer.DefaultConfrontQuestion, TurnAnalyzer.BuildUserMessage("   ", clue));
    }

    // ---------- Ventana de historial ----------

    [Test]
    public void VentanaDevuelveLosUltimosMensajesEmpezandoPorUsuario()
    {
        var history = new List<ChatMessage>();
        for (int i = 0; i < 10; i++)
        {
            history.Add(new ChatMessage { role = "user", content = $"q{i}" });
            history.Add(new ChatMessage { role = "assistant", content = $"a{i}" });
        }
        history.Add(new ChatMessage { role = "user", content = "q10" });

        List<ChatMessage> window = ConversationWindow.Last(history, 4);

        CollectionAssert.AreEqual(new[] { "q9", "a9", "q10" }, window.Select(m => m.content));
    }

    [Test]
    public void VentanaCortaDevuelveTodo()
    {
        var history = new List<ChatMessage> { new ChatMessage { role = "user", content = "q" } };

        Assert.AreEqual(1, ConversationWindow.Last(history, 16).Count);
    }

    // ---------- Análisis de la respuesta ----------

    private static (StoryData story, InvestigationState state) NewCase()
    {
        StoryData story = TestCases.Story();
        return (story, new InvestigationState(story.variants[0]));
    }

    [Test]
    public void DetectaPistasSoloDelPortador()
    {
        var (story, state) = NewCase();

        // "cortina x" es la pista x, pero su portador es c: si lo dice b no cuenta
        TurnOutcome fromB = TurnAnalyzer.Analyze(story, state, "b", "Vi una taza i1 y una cortina x", null);
        CollectionAssert.AreEqual(new[] { "i1" }, fromB.newClues.Select(c => c.id));

        TurnOutcome fromC = TurnAnalyzer.Analyze(story, state, "c", "Había una cortina x", null);
        CollectionAssert.AreEqual(new[] { "x" }, fromC.newClues.Select(c => c.id));
    }

    [Test]
    public void PistaYaDescubiertaNoSeRepite()
    {
        var (story, state) = NewCase();
        TurnAnalyzer.Analyze(story, state, "b", "taza i1", null);

        Assert.IsEmpty(TurnAnalyzer.Analyze(story, state, "b", "otra vez la taza i1", null).newClues);
    }

    [Test]
    public void MentiraSoloCuentaEnBocaDelCulpable()
    {
        var (story, state) = NewCase();

        TurnAnalyzer.Analyze(story, state, "b", "Ana estuvo en casa", null);
        Assert.IsFalse(state.CulpritToldLie);

        TurnOutcome outcome = TurnAnalyzer.Analyze(story, state, "a", "Yo estuve en casa toda la noche", null);
        Assert.IsTrue(state.CulpritToldLie);
        Assert.IsTrue(outcome.lieTold);
    }

    [Test]
    public void ContradiccionAlDescubrirPistaTrasLaMentira()
    {
        var (story, state) = NewCase();
        TurnAnalyzer.Analyze(story, state, "a", "Estuve en casa", null);

        TurnOutcome outcome = TurnAnalyzer.Analyze(story, state, "c", "Vi la cortina x", null);

        Assert.AreEqual("x", outcome.newContradictions.Single().id);
    }

    [Test]
    public void ConfrontarAlCulpableConLaPruebaCreaContradiccion()
    {
        var (story, state) = NewCase();
        TurnAnalyzer.Analyze(story, state, "c", "Vi la cortina x", null);
        ClueData x = state.Variant.Clue("x");

        TurnOutcome outcome = TurnAnalyzer.Analyze(story, state, "a", "Eso no demuestra nada.", x);

        CollectionAssert.Contains(state.ShownTo("a").ToList(), "x");
        Assert.AreEqual(1, outcome.newContradictions.Count);
    }

    [Test]
    public void DetectaMencionesDePersonajes()
    {
        var (story, state) = NewCase();

        TurnOutcome outcome = TurnAnalyzer.Analyze(story, state, "b", "Pregúntele a Carla, la de al lado.", null);

        CollectionAssert.AreEqual(new[] { "c" }, outcome.mentionedCharacters);
    }

    [Test]
    public void TrazaIncluyeCadaPistaEvaluada()
    {
        var (story, state) = NewCase();

        TurnOutcome outcome = TurnAnalyzer.Analyze(story, state, "b", "taza i1", null);

        // b porta i1, i2, d y ctx
        Assert.AreEqual(4, outcome.traces.Count);
        Assert.IsTrue(outcome.traces.Any(t => t.clue.id == "i1" && t.trace.Matched));
    }

    [Test]
    public void SecretosYaContadosPorElPersonaje()
    {
        var (story, state) = NewCase();
        TurnAnalyzer.Analyze(story, state, "b", "partida d", null);
        TurnAnalyzer.Analyze(story, state, "b", "taza i1", null);

        CollectionAssert.AreEqual(new[] { "d" }, TurnAnalyzer.RevealedSecrets(state, "b").Select(c => c.id));
    }
}
