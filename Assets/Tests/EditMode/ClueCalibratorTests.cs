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
    public void SinVariantesUsaTodasLasRegistradas()
    {
        var options = ClueCalibrator.ParseArgs(new[] { "Unity.exe" });

        CollectionAssert.Contains(options.variantIds, "1A");
        Assert.AreEqual(3, options.tries);
    }
}
