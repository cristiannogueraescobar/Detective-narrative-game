using System.Linq;
using NUnit.Framework;

/// <summary>
/// Validador de coherencia narrativa (A1): cada regla se prueba rompiendo a propósito una historia válida, y las 9
/// variantes reales tienen que pasar todas las reglas.
/// </summary>
public class NarrativeValidatorTests
{
    private static string[] Rules(StoryData story, VariantData variant)
    {
        return NarrativeValidator.Validate(story, variant).Select(i => i.rule).ToArray();
    }

    [Test]
    public void LaHistoriaDePruebaEsCoherente()
    {
        StoryData story = TestCases.Story();
        CollectionAssert.IsEmpty(NarrativeValidator.Validate(story, story.variants[0]).Where(i => i.rule != NarrativeValidator.CastSize),
            "(la historia de prueba tiene 3 personajes a propósito)");
    }

    [Test]
    public void LaMentiraDelCulpableNecesitaUnaPistaQueLaContradiga()
    {
        StoryData story = TestCases.Story();
        VariantData v = story.variants[0];
        v.Clue("x").exposesLie = false;
        CollectionAssert.Contains(Rules(story, v), NarrativeValidator.LieUncontradicted);
    }

    [Test]
    public void LaPistaQueContradiceLaMentiraNoPuedeTenerlaElCulpable()
    {
        StoryData story = TestCases.Story();
        VariantData v = story.variants[0];
        v.Clue("x").holder = "a";
        CollectionAssert.Contains(Rules(story, v), NarrativeValidator.LieUncontradicted);
    }

    [Test]
    public void UnParteDeLaMananaNoRegalaPistas()
    {
        StoryData story = TestCases.Story();
        VariantData v = story.variants[0];
        v.morningReports[2] = "Un agente encontró la taza en el cuarto (i1).";
        CollectionAssert.Contains(Rules(story, v), NarrativeValidator.ReportGivesClue);
    }

    [Test]
    public void UnParteDeLaMananaNoNombraAlCulpableComoTal()
    {
        StoryData story = TestCases.Story();
        VariantData v = story.variants[0];
        v.morningReports[3] = "Todo apunta a que fue Ana.";
        CollectionAssert.Contains(Rules(story, v), NarrativeValidator.ReportSpoils);
    }

    [Test]
    public void ElEpilogoNoTraeHorasQueNoEstenEnElCaso()
    {
        StoryData story = TestCases.Story();
        VariantData v = story.variants[0];
        v.epilogue = "A las 03:33 lo hizo A.";
        CollectionAssert.Contains(Rules(story, v), NarrativeValidator.EpilogueTime);

        v.Clue("i1").fact = "a las 03:33 viste la taza";
        CollectionAssert.DoesNotContain(Rules(story, v), NarrativeValidator.EpilogueTime, "si la hora está en una ficha, vale");
    }

    [Test]
    public void LaMismaPersonaNoTieneDosEdades()
    {
        StoryData story = TestCases.Story();
        story.cast[0].identity = "Eres Ana Gil, 40 años, abogada.";
        story.variants[0].Role("b").knowledge = new[] { "Ana Gil, de 45 años, es tu madre." };
        CollectionAssert.Contains(Rules(story, story.variants[0]), NarrativeValidator.AgeMismatch);
    }

    [Test]
    public void UnaPistaQueDescartaDebeDescartarAAlguienDelReparto()
    {
        StoryData story = TestCases.Story();
        story.variants[0].Clue("d").clears = "z";
        CollectionAssert.Contains(Rules(story, story.variants[0]), NarrativeValidator.ClearsUnknown);
    }

    [Test]
    public void UnaPistaNoPuedeDescartarAlCulpable()
    {
        StoryData story = TestCases.Story();
        story.variants[0].Clue("d").clears = "a";
        CollectionAssert.Contains(Rules(story, story.variants[0]), NarrativeValidator.ClearsCulprit);
    }

    [Test]
    public void ElRepartoTieneEntreCuatroYCincoPersonajes()
    {
        StoryData story = TestCases.Story(); // 3 personajes
        CollectionAssert.Contains(Rules(story, story.variants[0]), NarrativeValidator.CastSize);
    }

    [Test]
    public void NadieEstaEnDosSitiosALaVez()
    {
        StoryData story = TestCases.Story();
        VariantData v = story.variants[0];
        v.timeline.Add(new TimelineEvent("22:30", "a", "cocina", "prepara el cacao"));
        v.timeline.Add(new TimelineEvent("22:30", "a", "bar", "toma un café"));
        CollectionAssert.Contains(Rules(story, v), NarrativeValidator.TwoPlaces);

        v.timeline[1] = new TimelineEvent("~22:30", "a", "bar", "toma un café");
        CollectionAssert.DoesNotContain(Rules(story, v), NarrativeValidator.TwoPlaces, "una hora aproximada no cuenta");
    }

    [Test]
    public void LasHorasDeLasPistasEstanEnLaLineaTemporal()
    {
        StoryData story = TestCases.Story();
        VariantData v = story.variants[0];
        v.timeline.Add(new TimelineEvent("22:30", "a", "cocina", "prepara el cacao"));
        v.Clue("i1").fact = "a las 23:10 viste la taza";
        CollectionAssert.Contains(Rules(story, v), NarrativeValidator.TimeOffTimeline);

        v.timeline.Add(new TimelineEvent("23:10", "b", "cuarto", "ve la taza"));
        CollectionAssert.DoesNotContain(Rules(story, v), NarrativeValidator.TimeOffTimeline);
    }

    private static System.Collections.Generic.IEnumerable<TestCaseData> Real()
    {
        foreach (StoryData story in CaseLibrary.Stories)
            foreach (VariantData v in story.variants)
                yield return new TestCaseData(v.id).SetName($"Narrativa_{v.id}");
    }

    [TestCaseSource(nameof(Real))]
    public void LasVariantesRealesSonCoherentes(string variantId)
    {
        CaseLibrary.TryFind(variantId, out StoryData story, out VariantData v);
        var issues = NarrativeValidator.Validate(story, v);
        Assert.IsEmpty(issues, string.Join("\n", issues.Select(i => $"[{i.rule}] {i.detail}")));
    }
}
