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
    public void SinHorasNoHayNada()
    {
        CollectionAssert.IsEmpty(TimeCheck.Unknown("No me fijé en la hora, inspector.", Sheet));
        CollectionAssert.IsEmpty(TimeCheck.Unknown(null, Sheet));
    }
}
