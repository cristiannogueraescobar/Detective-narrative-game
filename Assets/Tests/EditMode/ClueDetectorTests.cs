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

    [TestCase("No, no vi ninguna taza de cacao en la mesilla.")]
    [TestCase("Nunca hubo cacao en esa taza.")]
    [TestCase("Nadie dejó una taza de cacao ahí.")]
    public void Evaluate_IgnoraAnclasNegadas(string denial)
    {
        var groups = new[] { new[] { "taza" }, new[] { "cacao" } };

        Assert.IsFalse(ClueDetector.Evaluate(groups, ClueDetector.Normalize(denial)).Matched);
    }

    [Test]
    public void Evaluate_NegacionEnOtraFraseNoAfecta()
    {
        var groups = new[] { new[] { "taza" }, new[] { "cacao" } };

        Assert.IsTrue(ClueDetector.Evaluate(groups, ClueDetector.Normalize("No sé, tío. Vi una taza de cacao.")).Matched);
        Assert.IsTrue(ClueDetector.Evaluate(groups, ClueDetector.Normalize("No, había una taza de cacao.")).Matched);
    }

    [Test]
    public void Evaluate_BastaUnaOcurrenciaNoNegada()
    {
        var groups = new[] { new[] { "cacao" }, new[] { "taza" } };

        Assert.IsTrue(ClueDetector.Evaluate(groups,
            ClueDetector.Normalize("Elena nunca tomaba cacao, pero vi una taza de cacao en la mesilla.")).Matched);
    }

    [Test]
    public void Evaluate_AnclaQueEmpiezaPorNoSeRespeta()
    {
        var groups = new[] { new[] { "no quiero mas" }, new[] { "pared" } };

        Assert.IsTrue(ClueDetector.Evaluate(groups, ClueDetector.Normalize("A través de la pared decía: no quiero más.")).Matched);
    }

    [Test]
    public void Evaluate_SinGuardiaDeNegacionDetectaNegaciones()
    {
        // Las mentiras del culpable son a menudo negaciones: se evalúan sin guardia
        var lie = new[] { new[] { "mi cuarto" }, new[] { "cascos" } };
        string text = ClueDetector.Normalize("Nunca salí de mi cuarto, tenía los cascos.");

        Assert.IsFalse(ClueDetector.Evaluate(lie, text).Matched);
        Assert.IsTrue(ClueDetector.Evaluate(lie, text, negationGuard: false).Matched);
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
        Assert.IsTrue(ClueDetector.MentionsAny(ClueDetector.Normalize("Amparo, la de enfrente"), new[] { "Amparo" }));
        Assert.IsFalse(ClueDetector.MentionsAny(ClueDetector.Normalize("Nadie más"), new[] { "Amparo" }));
    }
}
