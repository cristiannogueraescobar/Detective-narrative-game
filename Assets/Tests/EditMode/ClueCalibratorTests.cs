using NUnit.Framework;

public class ClueCalibratorTests
{
    [TestCase(2, 3, true)]
    [TestCase(3, 3, true)]
    [TestCase(1, 3, false)]
    [TestCase(4, 6, true)]
    [TestCase(3, 6, false)]
    [TestCase(0, 0, false)]
    public void UmbralDeDosTercios(int hits, int total, bool expected)
    {
        Assert.AreEqual(expected, ClueCalibrator.Passes(hits, total));
    }

    [Test]
    public void SeparaTurnosPorDoblesBarras()
    {
        CollectionAssert.AreEqual(new[] { "¿Dónde estaba?", "No me mienta." },
            ClueCalibrator.SplitTurns(" ¿Dónde estaba? || No me mienta. "));
        CollectionAssert.AreEqual(new[] { "Una sola" }, ClueCalibrator.SplitTurns("Una sola"));
    }

    [Test]
    public void ParseaArgumentosDeLineaDeComandos()
    {
        var options = ClueCalibrator.ParseArgs(new[] { "Unity.exe", "-variants", "1A,1B", "-tries", "5", "-model", "m" });

        CollectionAssert.AreEqual(new[] { "1A", "1B" }, options.variantIds);
        Assert.AreEqual(5, options.tries);
        Assert.AreEqual("m", options.model);
        Assert.AreEqual("http://localhost:11434", options.ollamaUrl);
    }

    [Test]
    public void FiltroDePistasOpcional()
    {
        var options = ClueCalibrator.ParseArgs(new[] { "Unity.exe", "-clues", "1A_puerta, 1B_cena" });

        CollectionAssert.AreEqual(new[] { "1A_puerta", "1B_cena" }, options.clueIds);
        Assert.IsEmpty(ClueCalibrator.ParseArgs(new[] { "Unity.exe" }).clueIds);
    }

    [Test]
    public void SondaDePrecisionActivaPorDefectoYDesactivable()
    {
        Assert.IsTrue(ClueCalibrator.ParseArgs(new[] { "Unity.exe" }).precisionProbe);
        Assert.IsFalse(ClueCalibrator.ParseArgs(new[] { "Unity.exe", "-noprecision" }).precisionProbe);
        CollectionAssert.IsNotEmpty(ClueCalibrator.PrecisionQuestions);
    }

    [Test]
    public void SinVariantesUsaTodasLasRegistradas()
    {
        var options = ClueCalibrator.ParseArgs(new[] { "Unity.exe" });

        CollectionAssert.Contains(options.variantIds, "1A");
        Assert.AreEqual(3, options.tries);
    }
}

public class NaturalnessTests
{
    [Test]
    public void TemperaturaPorDefectoYConfigurable()
    {
        Assert.AreEqual(AIConversationManager.DefaultTemperature, ClueCalibrator.ParseArgs(new[] { "Unity.exe" }).temperature, 1e-6);
        Assert.AreEqual(0.4f, ClueCalibrator.ParseArgs(new[] { "Unity.exe", "-temperature", "0.4" }).temperature, 1e-6);
    }

    [TestCase("Estuve en casa, inspector. No sé nada más.", 0)]
    [TestCase("*se remueve en la silla* Estuve en casa.", 1)]
    [TestCase("- Estuve en casa\n- No vi nada", 1)]
    [TestCase("Como inteligencia artificial, no puedo saberlo.", 1)]
    [TestCase("Uno. Dos. Tres. Cuatro. Cinco. Seis. Siete.", 1)]
    public void DetectaViolacionesDeEstilo(string response, int expected)
    {
        Assert.AreEqual(expected, Naturalness.Violations(response).Count);
    }

    [Test]
    public void ResumenCalculaLongitudYVariedad()
    {
        var stats = Naturalness.Summarize(new[] { "Hola, soy yo.", "Hola, soy yo.", "Otra cosa distinta aquí." });

        Assert.AreEqual(3, stats.responses);
        Assert.AreEqual(2f / 3f, stats.distinctRatio, 1e-3);
        Assert.AreEqual((3 + 3 + 4) / 3f, stats.averageWords, 1e-3);
        Assert.AreEqual(0, stats.violations);
    }
}
