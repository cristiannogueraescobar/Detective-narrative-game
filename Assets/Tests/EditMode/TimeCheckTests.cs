using NUnit.Framework;

public class TimeCheckTests
{
    private const string Sheet = "A las 22:30 subiste el cacao. A las 23:05 fingiste encontrarla y a las 23:15 llamaste al 112.";

    [Test]
    public void LasHorasDeLaFichaNoCuentan()
    {
        CollectionAssert.IsEmpty(TimeCheck.Unknown("Subí el cacao a las 22:30 y llamé al 112 a las 23:15.", Sheet));
        CollectionAssert.IsEmpty(TimeCheck.Unknown("A las 22.30 ya estaba arriba.", Sheet), "22.30 es la misma hora");
    }

    [Test]
    public void DetectaLasInventadas()
    {
        CollectionAssert.AreEqual(new[] { "21:45" }, TimeCheck.Unknown("La vi en el salón a las 21:45.", Sheet));
        CollectionAssert.AreEqual(new[] { "0:05", "23:40" }, TimeCheck.Unknown("A las 0:05 bajó, y a las 23:40 sonó el teléfono.", Sheet));
    }

    [Test]
    public void LaHoraQueDijoElInspectorNoEsInventada()
    {
        CollectionAssert.IsEmpty(TimeCheck.Unknown("Desde las 17:00 estuve en la finca.", Sheet + "\n¿Qué hiciste desde las 17:00?"));
    }

    [Test]
    public void MismaHoraConCeroDelanteONo()
    {
        CollectionAssert.IsEmpty(TimeCheck.Unknown("Salí a las 05:15.", "A las 5:15 me sitúa el GPS."));
    }

    [Test]
    public void LasHorasEscritasEnLetraCuentanComoConocidas()
    {
        CollectionAssert.IsEmpty(TimeCheck.Unknown("Volvió a las 6:30.", "Estuve limpiando hasta las seis y media."));
        CollectionAssert.IsEmpty(TimeCheck.Unknown("Llegó a las 21:15.", "Volví a las nueve y cuarto."), "de noche también");
        CollectionAssert.IsEmpty(TimeCheck.Unknown("A las 4:45.", "A las cinco menos cuarto."));
        CollectionAssert.IsEmpty(TimeCheck.Unknown("A las 13:00.", "A la una en punto."));
        CollectionAssert.AreEqual(new[] { "6:15" }, TimeCheck.Unknown("Volvió a las 6:15.", "Hasta las seis y media."));
    }

    [Test]
    public void SinHorasNoHayNada()
    {
        CollectionAssert.IsEmpty(TimeCheck.Unknown("No me fijé en la hora, inspector.", Sheet));
        CollectionAssert.IsEmpty(TimeCheck.Unknown(null, Sheet));
    }

    // Revisión D3 n.º 6: "las 5:30 de la tarde" es la misma hora que "17:30"
    [TestCase("La vi a las 5:30, ya de tarde.", "Fichó a las 17:30.")]
    [TestCase("La vi a las 17:30.", "Salió a las 5:30 de la tarde.")]
    [TestCase("Serían las 12:10.", "Volvió a las 00:10.")]
    public void DoceHorasYVeinticuatroSonLaMismaHora(string answer, string known)
    {
        CollectionAssert.IsEmpty(TimeCheck.Unknown(answer, known));
    }

    [Test]
    public void UnaHoraDeVerdadInventadaSigueSaliendo()
    {
        CollectionAssert.AreEqual(new[] { "21:45" }, TimeCheck.Unknown("La vi a las 21:45.", "Fichó a las 17:30."));
    }

    // PERFORMANCE-AUDIT, mejora 2: comprobar horas era lo más caro de cada respuesta (dos expresiones regulares sobre
    // 6,6 KB de ficha e historial aunque la respuesta no tuviera ninguna hora)
    [Test]
    public void SinHorasEnLaRespuestaNoSeAnalizaLoConocido()
    {
        TimeCheck.ResetCache();
        string huge = string.Concat(System.Linq.Enumerable.Repeat(Sheet + "\n", 50));
        CollectionAssert.IsEmpty(TimeCheck.Unknown("No vi nada esa noche, inspector.", huge, new[] { "¿Qué viste?" }));
        Assert.AreEqual(0, TimeCheck.KnownScans, "sin horas en la respuesta, nada que analizar");
    }

    [Test]
    public void LaFichaSeAnalizaUnaVezPorSospechosoYDia()
    {
        TimeCheck.ResetCache();
        string[] history = { "¿Dónde estabas a las 21:00?", "En casa." };
        TimeCheck.Unknown("A las 22:30 subí.", Sheet, history);
        int afterFirst = TimeCheck.KnownScans;
        TimeCheck.Unknown("Y a las 23:15 llamé.", Sheet, history);
        Assert.AreEqual(afterFirst, TimeCheck.KnownScans, "misma ficha y mismos mensajes: de la caché");
        TimeCheck.Unknown("A las 23:15.", Sheet + " Día 2.", history);
        Assert.AreEqual(afterFirst + 1, TimeCheck.KnownScans, "ficha distinta (otro día): se analiza una vez más");
    }

    [Test]
    public void ConFichaEHistorialDaLoMismoQueConTodoJunto()
    {
        string[] history = { "¿Qué hiciste desde las 17:00?", "Estuve en la finca hasta las nueve y media." };
        foreach (string answer in new[] { "Desde las 17:00 en la finca.", "A las 21:30 volví.", "A las 21:45 la vi.",
                                          "Llamé a las 23:15 y a las 0:05 bajé.", "Nada." })
            CollectionAssert.AreEqual(TimeCheck.Unknown(answer, Sheet + "\n" + string.Join("\n", history)),
                                      TimeCheck.Unknown(answer, Sheet, history), answer);
    }
}
