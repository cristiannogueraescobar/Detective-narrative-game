using System;
using System.Linq;
using NUnit.Framework;

public class EmotionSystemTests
{
    [Test]
    public void LaFichaPideLaEtiquetaDeEstado()
    {
        StoryData story = TestCases.Story();
        string prompt = PromptBuilder.Build(story, story.variants[0], "b", 1, new ClueData[0], new ClueData[0]);

        StringAssert.Contains(EmotionParser.TagInstruction, prompt);
    }

    [Test]
    public void LaGuiaDeEstadoNombraALaVictima()
    {
        StoryData story = TestCases.Story();
        story.victim = "Elena";
        string prompt = PromptBuilder.Build(story, story.variants[0], "b", 1, new ClueData[0], new ClueData[0]);

        StringAssert.Contains("Triste si te hablan de Elena", prompt);
    }

    [Test]
    public void RetratoBuscaEstadoExactoLuegoSustitutoLuegoBase()
    {
        CollectionAssert.AreEqual(
            new[] { "Assets/Art/Portraits/daniel_asustado.png", "Assets/Art/Portraits/daniel_nervioso.png", "Assets/Art/Portraits/daniel_tranquilo.png" },
            PortraitPaths.Candidates("daniel", Emotion.Asustado));
    }

    [Test]
    public void RetratoTranquiloNoRepiteCandidatos()
    {
        CollectionAssert.AreEqual(new[] { "Assets/Art/Portraits/lucas_tranquilo.png" }, PortraitPaths.Candidates("lucas", Emotion.Tranquilo));
    }

    [Test]
    public void TodosLosEstadosTienenEstiloVisual()
    {
        foreach (Emotion e in Enum.GetValues(typeof(Emotion)))
        {
            EmotionStyle style = EmotionStyle.For(e);
            Assert.Greater(style.textSpeed, 0f, e.ToString());
            Assert.GreaterOrEqual(style.shakeAmplitude, 0f, e.ToString());
        }

        Assert.AreEqual(0f, EmotionStyle.For(Emotion.Tranquilo).shakeAmplitude);
        Assert.Greater(EmotionStyle.For(Emotion.Asustado).shakeAmplitude, EmotionStyle.For(Emotion.Nervioso).shakeAmplitude);
    }

    [Test]
    public void TodosLosPersonajesTienenIdDeArteUnico()
    {
        var artIds = CaseLibrary.Stories.SelectMany(s => s.cast).Select(c => c.artId).ToList();

        Assert.IsTrue(artIds.All(id => !string.IsNullOrEmpty(id)));
        Assert.AreEqual(artIds.Count, artIds.Distinct().Count(), "un id de arte por personaje");
        Assert.AreEqual(12, artIds.Count);
    }

    [TestCase(EmotionProbe.Acusacion, Emotion.Enfadado, true)]
    [TestCase(EmotionProbe.Acusacion, Emotion.Tranquilo, false)]
    [TestCase(EmotionProbe.Victima, Emotion.Triste, true)]
    [TestCase(EmotionProbe.Victima, Emotion.Enfadado, false)]
    [TestCase(EmotionProbe.Neutra, Emotion.Tranquilo, true)]
    [TestCase(EmotionProbe.Neutra, Emotion.Asustado, false)]
    [TestCase(EmotionProbe.TemaSensible, Emotion.Nervioso, true)]
    [TestCase(EmotionProbe.TemaSensible, Emotion.Tranquilo, false)]
    public void CoherenciaEsperadaPorTipoDePregunta(EmotionProbe probe, Emotion emotion, bool coherent)
    {
        Assert.AreEqual(coherent, EmotionCoherence.IsCoherent(probe, emotion));
    }
}
