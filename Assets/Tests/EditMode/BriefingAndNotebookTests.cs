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
}
