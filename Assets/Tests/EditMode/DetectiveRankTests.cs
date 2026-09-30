using NUnit.Framework;

public class DetectiveRankTests
{
    [TestCase(Ending.Bad, false, 0, 0, "Novato")]
    [TestCase(Ending.Insufficient, false, 0, 0, "Agente")]
    [TestCase(Ending.Bittersweet, false, 0, 0, "Sabueso")]
    [TestCase(Ending.Good, false, 0, 0, "Inspector")]
    [TestCase(Ending.Good, true, 0, 2, "Inspector jefe")]   // prueba clave y dos días de sobra
    [TestCase(Ending.Good, true, 3, 0, "Inspector")]        // muchas ayudas restan
    [TestCase(Ending.Bad, true, 0, 5, "Novato")]            // acusar a otro nunca pasa de novato
    public void RangoSegunElCaso(Ending ending, bool keyClue, int hints, int daysLeft, string rank)
    {
        Assert.AreEqual(rank, DetectiveRank.For(ending, keyClue, hints, daysLeft));
    }

    [Test]
    public void LosRangosEstanOrdenados()
    {
        Assert.Less(DetectiveRank.Order("Novato"), DetectiveRank.Order("Sabueso"));
        Assert.Less(DetectiveRank.Order("Inspector"), DetectiveRank.Order("Inspector jefe"));
        Assert.AreEqual(-1, DetectiveRank.Order(null));
    }
}
