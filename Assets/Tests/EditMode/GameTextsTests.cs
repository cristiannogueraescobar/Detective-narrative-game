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
    [TestCase(1, 0, "En tu libreta: 1 pista y ninguna contradicción. Elige también la prueba clave: si lo demuestra, tu rango sube.")]
    [TestCase(3, 1, "En tu libreta: 3 pistas y 1 contradicción. Elige también la prueba clave: si lo demuestra, tu rango sube.")]
    [TestCase(5, 2, "En tu libreta: 5 pistas y 2 contradicciones. Elige también la prueba clave: si lo demuestra, tu rango sube.")]
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
    public void ElExpedienteDiceQuienEsLaVictimaSinDestripar()
    {
        CaseLibrary.TryFind("1A", out StoryData story, out _);
        string hook = GameTexts.CaseHook(story);
        StringAssert.StartsWith("Víctima: Elena", hook);
        StringAssert.DoesNotContain("112", hook, "solo la primera frase: quién era, no qué pasó");
        foreach (StoryData s in CaseLibrary.Stories)
            StringAssert.EndsWith(".", GameTexts.CaseHook(s));
    }

    [Test]
    public void ElExpedienteDiceElMejorFinalYElMejorRango()
    {
        Theme t = ThemeManager.Current;
        Assert.AreEqual("SIN RESOLVER", GameTexts.RecordLabel(null, null, t));
        Assert.AreEqual(EndingStyle.For(Ending.Good, t).stamp + " · SABUESO", GameTexts.RecordLabel(Ending.Good, "Sabueso", t));
        Assert.AreEqual(EndingStyle.For(Ending.Bad, t).stamp, GameTexts.RecordLabel(Ending.Bad, null, t), "partidas anteriores al rango");
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
        StringAssert.Contains("cuando preguntas por lo que ellos saben", text, "cómo aparecen los personajes nuevos");
        StringAssert.Contains("siete en Historia", text, "las preguntas según la dificultad");
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

    // C6: las reglas dicen las preguntas de la dificultad elegida, no siempre cinco
    [TestCase(7, "siete preguntas")]
    [TestCase(5, "cinco preguntas")]
    [TestCase(4, "cuatro preguntas")]
    public void ReglasConLasPreguntasDeLaDificultad(int perDay, string expected)
    {
        StringAssert.Contains(expected, GameTexts.RulesLine(7, perDay));
        StringAssert.Contains("siete días", GameTexts.RulesLine(7, perDay));
        StringAssert.Contains(expected, Tutorial.TextOf(Tutorial.Days, perDay));
        StringAssert.Contains(expected, CaseBriefing.Format(CaseLibrary.Stories[0], perDay));
    }

    [Test]
    public void SinPreguntasDiceQueHacer()
    {
        StringAssert.Contains("Fin del día", GameTexts.NoQuestionsLeft);
    }

    [Test]
    public void LosErroresDicenQueHacer()
    {
        StringAssert.Contains("inténtalo", GameTexts.AskFailed);
        StringAssert.DoesNotContain("Error inesperado", GameTexts.AskFailed);
        StringAssert.DoesNotContain("Error inesperado", GameTexts.ShowAnswerFailed);
    }

    [Test]
    public void ReiniciarUsaElMismoAvisoQueEmpezarDeNuevo()
    {
        Assert.AreEqual("Seguir con este caso", GameTexts.NewGameNo, "el botón de cancelar dice lo que hace");
        StringAssert.Contains("caso", GameTexts.NewGameConfirm);
    }

    // Revisión D3 n.º 10: las instrucciones sacan las cifras de la dificultad, no las repiten a mano
    [Test]
    public void InstruccionesConLasCifrasDeLaDificultad()
    {
        string text = GameTexts.Instructions(ThemeManager.Current);
        StringAssert.Contains(GameTexts.NumberWord(Difficulty.QuestionsPerDay(DifficultyLevel.Historia)) + " en Historia", text);
        StringAssert.Contains(GameTexts.NumberWord(Difficulty.QuestionsPerDay(DifficultyLevel.Veterano)) + " en Veterano", text);
    }

    // Ronda final 2 (recorrido de jugador nuevo): quien se atasca sabe que existe "Pensar"; la prueba clave se explica
    [Test]
    public void ElConsejoDeAtascoRecuerdaPensarSiHayAyudas()
    {
        StringAssert.Contains("Pensar", GameTexts.StuckHint(3, 0, canThink: true));
        StringAssert.DoesNotContain("Pensar", GameTexts.StuckHint(3, 0, canThink: false), "en Veterano no hay ayudas");
    }

    [Test]
    public void LaAcusacionExplicaLaPruebaClaveSoloSiHayPistas()
    {
        StringAssert.Contains("prueba clave", GameTexts.AccusationSummary(2, 0));
        StringAssert.DoesNotContain("prueba clave", GameTexts.AccusationSummary(0, 0), "sin pistas no hay selector");
        StringAssert.DoesNotContain("prueba clave", GameTexts.AccusationPrompt);
    }

    // Accesibilidad: el tamaño de letra del sistema decide el de la primera partida
    [TestCase(1f, 0)]
    [TestCase(1.15f, 0)]
    [TestCase(1.3f, 1)]
    [TestCase(1.5f, 1)]
    [TestCase(1.8f, 2)]
    [TestCase(2.2f, 2)]
    public void ElTamanoDeLetraDelSistemaEligeElPrimero(float fontScale, int level)
    {
        Assert.AreEqual(level, GameSettings.TextSizeForSystemScale(fontScale));
    }

    [Test]
    public void LasInstruccionesExplicanLasVersionesDeLaLibreta()
    {
        StringAssert.Contains("lo que dice cada uno", GameTexts.Instructions(ThemeManager.Current));
    }

    // Glosario (C6): "caso", no "partida"; el botón de Ajustes dice lo mismo que el aviso que abre
    [Test]
    public void ElBotonDeReinicioUsaElMismoTexto()
    {
        Assert.AreEqual(GameTexts.NewGameYes, GameTexts.RestartButton);
        StringAssert.DoesNotContain("partida", GameTexts.RestartButton.ToLowerInvariant());
    }
}
