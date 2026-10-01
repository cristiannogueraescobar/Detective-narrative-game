using System.Linq;
using NUnit.Framework;

/// <summary>
/// Sesión C, punto 4: una pista puede tener dos portadores. 1B_cena y 1C_llamada dependían solo de que Daniel confesara
/// (20-55 % con 10 intentos); un segundo portador que lo sepa de forma lógica la cuenta desde su punto de vista
/// (su tema, su hecho y sus anclas). La pista es la misma: sale con cualquiera de los dos.
/// </summary>
public class TwoHoldersTests
{
    private static (StoryData story, VariantData v) Get(string variantId)
    {
        Assert.IsTrue(CaseLibrary.TryFind(variantId, out StoryData story, out VariantData v), variantId);
        return (story, v);
    }

    [TestCase("1B", "1B_cena")]
    [TestCase("1C", "1C_llamada")]
    public void LasDosPistasFlojasTienenSegundoPortador(string variantId, string clueId)
    {
        var (story, v) = Get(variantId);
        ClueData clue = v.Clue(clueId);
        Assert.AreEqual("padre", clue.holder);
        Assert.IsTrue(clue.HeldBy("vecina"), "Amparo también lo sabe");
        Assert.IsFalse(clue.HeldBy("hermano"));
        CollectionAssert.AreEquivalent(new[] { "padre", "vecina" }, clue.Holders.ToArray());
    }

    [Test]
    public void LaVistaDelSegundoPortadorUsaSuHechoYSusAnclas()
    {
        var (_, v) = Get("1B");
        ClueData clue = v.Clue("1B_cena");
        ClueData amparo = clue.ForHolder("vecina");
        Assert.AreEqual(clue.id, amparo.id, "misma pista");
        Assert.AreEqual("vecina", amparo.holder);
        Assert.AreNotEqual(clue.fact, amparo.fact);
        Assert.AreSame(clue, clue.ForHolder("padre"), "el portador principal ve la pista tal cual");
        Assert.GreaterOrEqual(amparo.calibrationQuestions.Length, 2);
        Assert.GreaterOrEqual(amparo.sampleHits.Length, 2);
        foreach (string hit in amparo.sampleHits)
            Assert.IsTrue(ClueDetector.Evaluate(amparo.anchors, ClueDetector.Normalize(hit)).Matched, hit);
        foreach (string miss in amparo.sampleMisses)
            Assert.IsFalse(ClueDetector.Evaluate(amparo.anchors, ClueDetector.Normalize(miss)).Matched, miss);
    }

    [Test]
    public void LaPistaSaleSiLaCuentaElSegundoPortador()
    {
        var (story, v) = Get("1B");
        var state = new InvestigationState(v);
        string said = v.Clue("1B_cena").ForHolder("vecina").sampleHits[0];
        TurnOutcome outcome = TurnAnalyzer.Analyze(story, state, "vecina", said, null);
        Assert.IsTrue(state.IsDiscovered("1B_cena"));
        Assert.IsTrue(outcome.newClues.Any(c => c.id == "1B_cena"));
    }

    [Test]
    public void LaFichaDelSegundoPortadorLlevaSuHecho()
    {
        var (story, v) = Get("1B");
        ClueData amparo = v.Clue("1B_cena").ForHolder("vecina");
        string prompt = PromptBuilder.Build(story, v, "vecina", 3, new ClueData[0], new ClueData[0]);
        StringAssert.Contains(amparo.fact, prompt);
        string daniel = PromptBuilder.Build(story, v, "padre", 3, new ClueData[0], new ClueData[0]);
        StringAssert.DoesNotContain(amparo.fact, daniel, "cada portador, su versión");
    }

    [Test]
    public void LaPistaSePuedeSugerirConElSegundoPortador()
    {
        var (story, v) = Get("1B");
        var state = new InvestigationState(v);
        foreach (ClueData c in v.clues.Where(c => c.id != "1B_cena"))
            state.Discover(c.id);
        Hint hint = HintAdvisor.Next(story, state, new[] { "vecina" }, new HintMemory());
        Assert.AreEqual("1B_cena", hint.clueId, "Daniel no está disponible, Amparo sí");
        Assert.AreEqual("vecina", hint.holderId);
    }

    [TestCase("1B")]
    [TestCase("1C")]
    public void LaFichaDelSegundoPortadorNoDisparaLaPistaSinContarla(string variantId)
    {
        var (story, v) = Get(variantId);
        CharacterData amparo = story.cast.First(c => c.id == "vecina");
        CharacterRole role = v.Role("vecina");
        var own = new System.Collections.Generic.List<string> { amparo.identity, amparo.speechExample, role.version, role.secret, role.nervousAbout, role.ifAccused };
        own.AddRange(role.knowledge);
        string normalized = ClueDetector.Normalize(string.Join(" ", own));
        foreach (ClueData c in v.clues.Where(c => c.HeldBy("vecina")))
            Assert.IsFalse(ClueDetector.Evaluate(c.ForHolder("vecina").anchors, normalized).Matched, c.id);
    }
}
