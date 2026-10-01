using System.Collections.Generic;
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

    // ---- Revisión independiente (sesión C): arreglos ----

    // Importante 1: si la cuenta Amparo, Daniel no puede "recordar" que la confesó él
    [Test]
    public void SiLaCuentaElSegundoPortadorElPrimeroNoLaRecuerdaComoSuya()
    {
        var (story, v) = Get("1B");
        var state = new InvestigationState(v);
        TurnAnalyzer.Analyze(story, state, "vecina", v.Clue("1B_cena").ForHolder("vecina").sampleHits[0], null);
        Assert.IsTrue(state.IsDiscovered("1B_cena"));
        Assert.AreEqual("vecina", state.RevealedBy("1B_cena"));
        CollectionAssert.IsEmpty(TurnAnalyzer.RevealedSecrets(state, "padre").ToList(), "Daniel no la ha contado");
    }

    [Test]
    public void SiLaConfiesaElPrimeroLaRecuerda()
    {
        var (story, v) = Get("1B");
        var state = new InvestigationState(v);
        TurnAnalyzer.Analyze(story, state, "padre", v.Clue("1B_cena").sampleHits[0], null);
        Assert.AreEqual("padre", state.RevealedBy("1B_cena"));
        CollectionAssert.AreEqual(new[] { "1B_cena" }, TurnAnalyzer.RevealedSecrets(state, "padre").Select(c => c.id).ToList());
    }

    // Menor 5: la libreta usa el resumen de quien la contó (Amparo no sabe a quién llamó Lucas)
    [Test]
    public void LaLibretaUsaElResumenDeQuienLaConto()
    {
        var (story, v) = Get("1C");
        var state = new InvestigationState(v);
        state.Discover("1C_llamada", "vecina");
        ClueData found = state.ClueAsFound("1C_llamada");
        Assert.AreEqual(v.Clue("1C_llamada").ForHolder("vecina").summary, found.summary);
        Assert.AreNotEqual(v.Clue("1C_llamada").summary, found.summary);
    }

    // Importantes 2 y 3: su habla normal y su propia ficha no disparan la pista
    [TestCase("1B", "1B_cena", "Se montó un lío esa noche en la calle, hijo.")]
    [TestCase("1B", "1B_cena", "Yo era como una amiga para esa niña. El coche del padre no volvió hasta las once.")]
    [TestCase("1C", "1C_llamada", "Lucas se quedó inmóvil en la ventana, hijo.")]
    [TestCase("1C", "1C_llamada", "Luego llamó a la ambulancia, y el coche del padre llegó a las diez y cuarto.")]
    public void ElHablaNormalDeAmparoNoLaDispara(string variantId, string clueId, string said)
    {
        var (_, v) = Get(variantId);
        Assert.IsFalse(ClueDetector.Evaluate(v.Clue(clueId).ForHolder("vecina").anchors, ClueDetector.Normalize(said)).Matched, said);
    }

    [TestCase("1B", "1B_cena")]
    [TestCase("1C", "1C_llamada")]
    public void ElHechoDeAmparoSeDetecta(string variantId, string clueId)
    {
        var (_, v) = Get(variantId);
        ClueData amparo = v.Clue(clueId).ForHolder("vecina");
        Assert.IsTrue(ClueDetector.Evaluate(amparo.anchors, ClueDetector.Normalize(amparo.fact)).Matched, amparo.fact);
    }

    // Menor 6: con los dos disponibles, la ayuda sugiere la versión que no es secreta
    [Test]
    public void ConLosDosDisponiblesSeSugiereLaVersionAbierta()
    {
        var (story, v) = Get("1B");
        var state = new InvestigationState(v);
        foreach (ClueData c in v.clues.Where(c => c.id != "1B_cena"))
            state.Discover(c.id);
        Hint hint = HintAdvisor.Next(story, state, new[] { "padre", "vecina" }, new HintMemory());
        Assert.AreEqual("vecina", hint.holderId);
    }

    // Revisión: el guardado recuerda quién la contó (y los guardados anteriores, sin ese dato, siguen cargando)
    [Test]
    public void ElGuardadoRecuerdaQuienLaConto()
    {
        var (story, v) = Get("1B");
        var data = new SaveData { variantId = "1B", discovered = new List<string> { "1B_cena" },
                                  discoveredBy = new List<string> { "1B_cena:vecina" } };
        InvestigationState state = SaveSystem.RestoreState(v, data);
        Assert.AreEqual("vecina", state.RevealedBy("1B_cena"));
        var old = new SaveData { variantId = "1B", discovered = new List<string> { "1B_cena" } };
        Assert.AreEqual("padre", SaveSystem.RestoreState(v, old).RevealedBy("1B_cena"), "sin el dato: el portador principal");
    }

    // Menor 7: las comprobaciones de datos valen para todos los portadores, no solo el principal
    [Test]
    public void CadaPortadorExtraCumpleLosMinimosDeUnaPista()
    {
        foreach (StoryData story in CaseLibrary.Stories)
            foreach (VariantData v in story.variants)
                foreach (ClueData clue in v.clues.Where(c => c.alsoHeldBy != null))
                    foreach (ClueHolder h in clue.alsoHeldBy)
                    {
                        string where = $"{clue.id}@{h.characterId}";
                        Assert.IsTrue(story.cast.Any(c => c.id == h.characterId), where + ": no está en el reparto");
                        ClueData view = clue.ForHolder(h.characterId);
                        Assert.GreaterOrEqual(view.anchors.Length, 2, where);
                        Assert.GreaterOrEqual(view.calibrationQuestions.Length, 2, where);
                        Assert.GreaterOrEqual(view.sampleHits.Length, 2, where);
                        Assert.GreaterOrEqual(view.sampleMisses.Length, 1, where);
                        foreach (string hit in view.sampleHits)
                            Assert.IsTrue(ClueDetector.Evaluate(view.anchors, ClueDetector.Normalize(hit)).Matched, where + ": " + hit);
                        foreach (string miss in view.sampleMisses)
                            Assert.IsFalse(ClueDetector.Evaluate(view.anchors, ClueDetector.Normalize(miss)).Matched, where + ": " + miss);
                    }
    }
}
