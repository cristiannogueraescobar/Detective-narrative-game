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
