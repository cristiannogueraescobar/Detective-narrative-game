using NUnit.Framework;

public class ClueDetectorTests
{
    [Test]
    public void Normalize_QuitaTildesMayusculasYUnificaHoras()
    {
        Assert.AreEqual("a las 22:35 vi al padre", ClueDetector.Normalize("A las 22.35   vi al PADRE"));
        Assert.AreEqual("22:35 cortinas", ClueDetector.Normalize("22h35 Cortinas"));
        Assert.AreEqual("habitacion nino", ClueDetector.Normalize("Habitación niño"));
    }

    [Test]
    public void Normalize_TextoVacioDevuelveCadenaVacia()
    {
        Assert.AreEqual("", ClueDetector.Normalize(null));
        Assert.AreEqual("", ClueDetector.Normalize("   "));
    }

    [Test]
    public void Evaluate_ExigeTodosLosGrupos()
    {
        var groups = new[] { new[] { "22:35", "diez y media" }, new[] { "cortina" } };

        Assert.IsTrue(ClueDetector.Evaluate(groups, ClueDetector.Normalize("A las 22.35 cerraba las cortinas")).Matched);
        Assert.IsFalse(ClueDetector.Evaluate(groups, ClueDetector.Normalize("A las 22:35 estaba dormida")).Matched);
    }

    [Test]
    public void Evaluate_AnclasConTildesCoincidenConTextoNormalizado()
    {
        var groups = new[] { new[] { "habitación" } };

        Assert.IsTrue(ClueDetector.Evaluate(groups, ClueDetector.Normalize("Entró en la HABITACION")).Matched);
    }

    [Test]
    public void Evaluate_TrazaIndicaAnclasEncontradas()
    {
        var trace = ClueDetector.Evaluate(new[] { new[] { "taza", "cacao", "leche" } }, "una taza de cacao");

        CollectionAssert.AreEqual(new[] { "taza", "cacao" }, trace.Groups[0].Found);
        StringAssert.Contains("taza", trace.ToString());
    }

    [Test]
    public void Evaluate_SinGruposNoDetecta()
    {
        Assert.IsFalse(ClueDetector.Evaluate(new string[0][], "x").Matched);
        Assert.IsFalse(ClueDetector.Evaluate(null, "x").Matched);
    }

    [Test]
    public void MentionsAny_DetectaAliasNormalizado()
    {
        Assert.IsTrue(ClueDetector.MentionsAny(ClueDetector.Normalize("Rosario, la de enfrente"), new[] { "Rosario" }));
        Assert.IsFalse(ClueDetector.MentionsAny(ClueDetector.Normalize("Nadie más"), new[] { "Rosario" }));
    }
}
