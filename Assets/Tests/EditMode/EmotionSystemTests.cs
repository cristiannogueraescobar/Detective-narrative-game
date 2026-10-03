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

        StringAssert.Contains("Si te hablan de Elena", prompt);
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

    // Sesión C (coherencia de estados de la historia 2): sin etiqueta al describir a alguien; el culpable "tranquilo" en lo
    // que le pone nervioso (su ficha le pedía calma) y en general disimulando
    [Test]
    public void LaEtiquetaSeAcuerdaTambienAlDescribirAAlguien()
    {
        StringAssert.Contains("aunque describas a alguien", EmotionParser.TagInstruction);
    }

    // Tercera medida: "aunque disimules" les ponía nerviosos y evasivos justo en los temas de sus pistas (Ruiz: 2C_prueba
    // 77 → 47 %, 2B_imagenes 70 → 40 %). Fuera: lo que le pone nervioso vuelve a decirse sin más
    [Test]
    public void LoQueTePoneNerviosoSinAunqueDisimules()
    {
        string guide = PromptBuilder.EmotionGuide("Sofía");
        StringAssert.Contains("Nervioso solo si tocan lo de TE PONE NERVIOSO.", guide);
        StringAssert.DoesNotContain("aunque disimules", guide);
        // Primera medida: con la cláusula metida antes de "si te acusan", las acusaciones salían tristes (69 de 360 frente
        // a 15 en main). La acusación va antes, en su propia frase
        StringAssert.Contains("Si te acusan: enfadado o asustado.", guide);
        Assert.Less(guide.IndexOf("Si te acusan", System.StringComparison.Ordinal), guide.IndexOf("Nervioso", System.StringComparison.Ordinal));
    }

    [Test]
    public void AlCulpableYaNoSeLePideCalma()
    {
        CaseLibrary.TryFind("2C", out StoryData story, out VariantData v);
        string prompt = PromptBuilder.Build(story, v, v.culpritId, 1, null, null);
        StringAssert.Contains("ERES EL CULPABLE", prompt);
        StringAssert.DoesNotContain("con calma", prompt);
    }

    // Segunda medida (coherencia de estados): al describir a la víctima ya ponían etiqueta, pero "tranquilo" (66 de 360 en
    // las preguntas sobre ella). La víctima va primero, también al describirla
    [Test]
    public void LaVictimaVaPrimeroTambienAlDescribirla()
    {
        string guide = PromptBuilder.EmotionGuide("Sofía");
        StringAssert.StartsWith("Si te hablan de Sofía, o la describes: triste, nunca tranquilo.", guide);
    }
}
