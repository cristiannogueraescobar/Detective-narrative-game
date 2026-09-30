using NUnit.Framework;

public class GameTextsTests
{
    [TestCase(1, 7, 0, 5, "DÍA 1 DE 7  ·  QUEDAN 5 PREGUNTAS")]
    [TestCase(3, 7, 4, 5, "DÍA 3 DE 7  ·  QUEDA 1 PREGUNTA")]
    [TestCase(7, 7, 5, 5, "DÍA 7 DE 7  ·  SIN PREGUNTAS HOY")]
    [TestCase(2, 7, 9, 5, "DÍA 2 DE 7  ·  SIN PREGUNTAS HOY")]
    public void BarraSuperiorCuentaLasQueQuedan(int day, int max, int used, int perDay, string expected)
    {
        Assert.AreEqual(expected, GameTexts.Hud(day, max, used, perDay));
    }

    [TestCase(0, "Mostrar prueba: ninguna")]
    [TestCase(1, "Mostrar prueba: ninguna · 1 en la libreta")]
    [TestCase(4, "Mostrar prueba: ninguna · 4 en la libreta")]
    public void SinPruebaElegidaDiceCuantasHay(int available, string expected)
    {
        Assert.AreEqual(expected, GameTexts.NoEvidenceWith(available));
    }

    [TestCase(0, 0, "Tu libreta está vacía: acusar ahora es una apuesta.")]
    [TestCase(1, 0, "En tu libreta: 1 pista y ninguna contradicción.")]
    [TestCase(3, 1, "En tu libreta: 3 pistas y 1 contradicción.")]
    [TestCase(5, 2, "En tu libreta: 5 pistas y 2 contradicciones.")]
    public void LaAcusacionRecuerdaLoQueTienes(int clues, int contradictions, string expected)
    {
        Assert.AreEqual(expected, GameTexts.AccusationSummary(clues, contradictions));
    }

    [TestCase(1, "¿Terminar el día? Te queda 1 pregunta y se perderá.")]
    [TestCase(3, "¿Terminar el día? Te quedan 3 preguntas y se perderán.")]
    public void TerminarElDiaAvisaDeLasQueQuedan(int remaining, string expected)
    {
        Assert.AreEqual(expected, GameTexts.EndDayConfirm(remaining));
    }

    [TestCase(2, 0, false)]
    [TestCase(3, 0, true)]
    [TestCase(5, 0, true)]
    [TestCase(3, 1, false)]
    public void SinPistasElTercerDiaHayUnConsejo(int day, int clues, bool hint)
    {
        string text = GameTexts.StuckHint(day, clues);
        Assert.AreEqual(hint, !string.IsNullOrEmpty(text));
        if (hint)
            StringAssert.Contains("concret", text);
    }

    [Test]
    public void ElParteDeLaMananaSinLineasVacias()
    {
        Assert.AreEqual("Consejo", GameTexts.MorningReport("", "Consejo", 3, 7), "sin parte del caso no empieza con una línea en blanco");
        Assert.AreEqual("Parte\nConsejo", GameTexts.MorningReport("Parte", "Consejo", 3, 7));
        Assert.AreEqual("Parte\nQuedan dos días de investigación.", GameTexts.MorningReport("Parte", null, 6, 7));
        StringAssert.EndsWith("tendrás que acusar a alguien.", GameTexts.MorningReport("", null, 7, 7));
        Assert.AreEqual("", GameTexts.MorningReport(null, null, 2, 7));
    }

    [Test]
    public void LasInstruccionesExplicanTodoElJuego()
    {
        string text = GameTexts.Instructions(ThemeManager.Current);
        foreach (string key in new[] { "Mostrar prueba", "Fin del día", "libreta", "contradicción", "cinco preguntas", "siete días", "caso fallido" })
            StringAssert.Contains(key, text);
    }

    [Test]
    public void LasInstruccionesCoincidenConLasReglas()
    {
        string text = GameTexts.Instructions(ThemeManager.Current);
        StringAssert.Contains(GameManager.SafetyUnlockDay == 3 ? "tercer día" : "?", text, "el día en que se desbloquea a todos");
        StringAssert.Contains("cinco", text);
    }

    [Test]
    public void ContinuarDiceQueCasoYQueDia()
    {
        Assert.AreEqual("Continuar · Noche de verano, día 3", GameTexts.Continue("Noche de verano", 3));
        Assert.AreEqual("Continuar", GameTexts.Continue(null, 3));
    }

    [Test]
    public void ElBalanceDiceDiaYPistas()
    {
        Assert.AreEqual("Día 5 de la investigación · pistas encontradas: 3 de 6", GameTexts.CaseStats(5, 3, 6));
        StringAssert.Contains("pistas encontradas: 3 de 6",
            EndingReport.Build(new AccusationResult { ending = Ending.Good }, "A", "B", 7, "", ThemeManager.Current, GameTexts.CaseStats(5, 3, 6)));
    }

    [Test]
    public void AcercaDeLlevaLaVersion()
    {
        StringAssert.Contains("versión 1.2", GameTexts.About(ThemeManager.Current, "1.2"));
    }
}
