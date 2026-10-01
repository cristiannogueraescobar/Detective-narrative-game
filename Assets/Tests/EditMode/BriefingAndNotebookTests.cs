using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class BriefingAndNotebookTests
{
    [Test]
    public void TodasLasHistoriasTienenDatosDeIntro()
    {
        foreach (StoryData story in CaseLibrary.Stories)
        {
            Assert.IsNotEmpty(story.place, story.id);
            Assert.IsNotEmpty(story.victimSummary, story.id);
            Assert.IsNotEmpty(story.situation, story.id);
        }
    }

    [Test]
    public void IntroDelCasoIncluyeLugarVictimaYSituacion()
    {
        StoryData story = CaseLibrary.Stories[0];

        string briefing = CaseBriefing.Format(story);

        StringAssert.Contains(story.title.ToUpperInvariant(), briefing);
        StringAssert.Contains(story.place, briefing);
        StringAssert.Contains(story.victimSummary, briefing);
        StringAssert.Contains(story.situation, briefing);
        StringAssert.DoesNotContain(story.variants[0].culpritId, briefing.ToLowerInvariant().Replace(" ", "_"));
    }

    [Test]
    public void EnLaLibretaDePapelLasPistasSonEnlaces()
    {
        StoryData story = TestCases.Story();
        var state = new InvestigationState(story.variants[0]);
        state.Discover("i1");
        string paper = Notebook.Format(story, state, story.cast.Select(c => c.id), new System.Collections.Generic.Dictionary<string, Emotion>(), c => "x", onPaper: true);
        string plain = Notebook.Format(story, state, story.cast.Select(c => c.id), new System.Collections.Generic.Dictionary<string, Emotion>(), c => "x");

        StringAssert.Contains($"<link=\"{Notebook.ClueLinkPrefix}i1\">", paper);
        string suspect = story.cast[0].id;
        StringAssert.Contains($"<link=\"{Notebook.SuspectLinkPrefix}{suspect}\">", paper, "los sospechosos también son enlaces");
        StringAssert.DoesNotContain("<link", plain, "el texto para el bot no lleva enlaces");
    }

    [Test]
    public void LibretaListaPistasContradiccionesYSospechosos()
    {
        StoryData story = TestCases.Story();
        var state = new InvestigationState(story.variants[0]);
        state.Discover("i1");
        state.Discover("d");
        state.Discover("x");
        state.RegisterLieTold();
        state.UpdateContradictions();

        string notebook = Notebook.Format(story, state,
            unlocked: new[] { "a", "b" },
            emotions: new Dictionary<string, Emotion> { { "a", Emotion.Enfadado } },
            describeContradiction: clue => $"choca con {clue.playerName}");

        StringAssert.Contains("Nombre i1", notebook);
        StringAssert.Contains("Resumen i1", notebook);
        StringAssert.Contains("choca con Nombre x", notebook);
        StringAssert.Contains("Ana (madre)", notebook);
        StringAssert.Contains("enfadado", notebook);
        StringAssert.Contains("Bea (hija)", notebook);
        StringAssert.Contains("descarte", notebook.ToLowerInvariant());
        StringAssert.DoesNotContain("Carla", notebook, "los bloqueados no aparecen");
    }

    [Test]
    public void LibretaVaciaDiceQueNoHayNada()
    {
        StoryData story = TestCases.Story();

        string notebook = Notebook.Format(story, new InvestigationState(story.variants[0]),
            new[] { "a" }, new Dictionary<string, Emotion>(), clue => "");

        StringAssert.Contains("Aún no hay pistas", notebook);
        StringAssert.Contains("Ninguna contradicción", notebook);
    }

    // Ronda 5: la libreta apunta la versión de cada uno cuando ya le has interrogado (para comparar con las pistas)
    [Test]
    public void LaLibretaApuntaLaVersionDeQuienYaHasInterrogado()
    {
        StoryData story = TestCases.Story();
        var state = new InvestigationState(story.variants[0]);
        string versionB = story.variants[0].Role("b").version;
        string versionC = story.variants[0].Role("c").version;

        string notebook = Notebook.Format(story, state, new[] { "a", "b", "c" }, new Dictionary<string, Emotion>(), c => "x",
            onPaper: true, interviewed: new[] { "b" });

        StringAssert.Contains(versionB, notebook, "b ya ha hablado: su versión queda apuntada");
        StringAssert.DoesNotContain(versionC, notebook, "a c aún no le has preguntado");
    }

    // Tercera revisión: la versión del culpable es su mentira; no se apunta hasta que la ha contado
    [Test]
    public void LaMentiraDelCulpableSoloSeApuntaCuandoLaHaContado()
    {
        StoryData story = TestCases.Story();
        var state = new InvestigationState(story.variants[0]);
        string lie = story.variants[0].Role("a").version;
        string Format() => Notebook.Format(story, state, new[] { "a", "b" }, new Dictionary<string, Emotion>(), c => "x",
            onPaper: true, interviewed: new[] { "a" });

        StringAssert.DoesNotContain(lie, Format(), "contestó, pero aún no ha contado su versión");
        state.RegisterLieTold();
        StringAssert.Contains(lie, Format());
    }

    // Tercera revisión: una versión que nombra a alguien aún no disponible no se apunta todavía
    [Test]
    public void UnaVersionQueNombraANadieBloqueadoEspera()
    {
        StoryData story = TestCases.Story();
        var state = new InvestigationState(story.variants[0]);
        story.variants[0].Role("b").version = "Trabajé, y luego vi a " + story.Character("c").shortName + " en la calle.";
        string version = story.variants[0].Role("b").version;

        string locked = Notebook.Format(story, state, new[] { "a", "b" }, new Dictionary<string, Emotion>(), c => "x",
            onPaper: true, interviewed: new[] { "b" });
        string open = Notebook.Format(story, state, new[] { "a", "b", "c" }, new Dictionary<string, Emotion>(), c => "x",
            onPaper: true, interviewed: new[] { "b" });

        StringAssert.DoesNotContain(version, locked);
        StringAssert.Contains(version, open);
    }

    // Ronda 15: los partes de la mañana se podían leer una sola vez (en la transición del día); ahora quedan en la
    // libreta ("los registros están en comisaría" es media pista)
    [Test]
    public void LaLibretaGuardaLosPartesDeLaMañana()
    {
        StoryData story = TestCases.Story();
        VariantData variant = story.variants[0];
        var state = new InvestigationState(variant);

        CollectionAssert.IsEmpty(Notebook.ReportsUpTo(variant, 1), "el día 1 no hay parte");
        CollectionAssert.AreEqual(new[] { "Día 2: d2", "Día 3: d3" }, Notebook.ReportsUpTo(variant, 3));

        string day1 = Notebook.Format(story, state, new[] { "a" }, new Dictionary<string, Emotion>(), c => "x", onPaper: true,
            reports: Notebook.ReportsUpTo(variant, 1));
        StringAssert.DoesNotContain("PARTES", day1);
        string day3 = Notebook.Format(story, state, new[] { "a" }, new Dictionary<string, Emotion>(), c => "x", onPaper: true,
            reports: Notebook.ReportsUpTo(variant, 3));
        StringAssert.Contains("PARTES DE LA MAÑANA", day3);
        StringAssert.Contains("Día 3: d3", day3);
        Assert.Greater(day3.IndexOf("PARTES"), day3.IndexOf("SOSPECHOSOS"), "al final: lo principal va antes");
    }
}
