using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class HintAdvisorTests
{
    // Historia de prueba: culpable "a"; i1, i2, d, ctx los tiene "b"; i3 y x (⚡) los tiene "c" (bloqueada al empezar)
    private StoryData story;
    private InvestigationState state;
    private HintMemory memory;

    [SetUp]
    public void SetUp()
    {
        story = TestCases.Story();
        state = new InvestigationState(story.variants[0]);
        memory = new HintMemory();
    }

    private Hint Next(params string[] unlocked)
    {
        return HintAdvisor.Next(story, state, unlocked, memory);
    }

    [Test]
    public void ApuntaAUnaPistaSinEncontrarDeAlguienDisponible()
    {
        Hint hint = Next("a", "b");
        Assert.IsNotNull(hint.clueId);
        Assert.AreEqual("b", story.variants[0].Clue(hint.clueId).holder, "c aún no está disponible");
        Assert.AreEqual("b", hint.holderId, "la ayuda dice a quién preguntar");
        Assert.IsFalse(state.IsDiscovered(hint.clueId));
    }

    [Test]
    public void PrimeroUnaIdeaVagaYLuegoLaPreguntaConcreta()
    {
        Hint first = Next("a", "b");
        Assert.AreEqual(1, first.level);
        StringAssert.Contains("Bea", first.text);
        Assert.IsNull(first.question, "el nivel 1 no da la pregunta");

        Hint second = Next("a", "b");
        Assert.AreEqual(first.clueId, second.clueId, "insistir sobre la misma pista la concreta");
        Assert.AreEqual(2, second.level);
        StringAssert.StartsWith("¿", second.question);
        StringAssert.Contains(second.question, second.text);

        Hint third = Next("a", "b");
        Assert.AreNotEqual(first.clueId, third.clueId, "después pasa a otra pista");
        Assert.AreEqual(1, third.level);
    }

    [Test]
    public void NoSenalaAlCulpableNiEmpiezaPorLaPistaDecisiva()
    {
        // Todos disponibles: primero pistas de inocentes que no son la ⚡
        Hint hint = Next("a", "b", "c");
        ClueData clue = story.variants[0].Clue(hint.clueId);
        Assert.AreNotEqual(story.variants[0].culpritId, clue.holder);
        Assert.IsFalse(clue.exposesLie);
    }

    [Test]
    public void LaDecisivaLlegaCuandoNoQuedanOtras()
    {
        foreach (string id in new[] { "i1", "i2", "i3", "d", "ctx" })
            state.Discover(id);
        Assert.AreEqual("x", Next("a", "b", "c").clueId);
    }

    [Test]
    public void SiTodoEstaEncontradoSugiereAcusar()
    {
        foreach (ClueData c in story.variants[0].clues)
            state.Discover(c.id);
        Hint hint = Next("a", "b", "c");
        Assert.IsNull(hint.clueId);
        StringAssert.Contains("acusar", hint.text);
    }

    [Test]
    public void SiLoQueFaltaLoSabeAlguienQueAunNoConocesLoDice()
    {
        foreach (string id in new[] { "i1", "i2", "d", "ctx" })
            state.Discover(id);
        Hint hint = Next("a", "b"); // Lo que falta lo tiene "c", aún bloqueada
        Assert.IsNull(hint.clueId);
        StringAssert.Contains("aún no", hint.text);
    }

    [Test]
    public void LaPreguntaSugeridaEsLaPrimeraPartedeUnaPreguntaEncadenada()
    {
        story.variants[0].Clue("i1").calibrationQuestions = new[] { "¿Qué vio al salir? || ¿Y después?" };
        memory.given["i1"] = 1;
        Hint hint = HintAdvisor.Next(story, state, new[] { "a", "b" }, memory);
        Assert.AreEqual("i1", hint.clueId);
        Assert.AreEqual("¿Qué vio al salir?", hint.question);
    }
}
