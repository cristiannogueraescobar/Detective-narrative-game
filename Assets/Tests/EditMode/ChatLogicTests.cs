using NUnit.Framework;

public class ChatLogicTests
{
    [Test]
    public void LaHoraAvanzaConLasPreguntasDelDia()
    {
        string previous = null;
        for (int i = 0; i < 5; i++)
        {
            string time = GameClock.TimeOf(i, 5);
            StringAssert.IsMatch(@"^\d\d:\d\d$", time);
            if (previous != null)
                Assert.Greater(string.CompareOrdinal(time, previous), 0, "cada pregunta es más tarde que la anterior");
            previous = time;
        }
    }

    [Test]
    public void LaJornadaVaDeMananaATarde()
    {
        Assert.AreEqual("09:00", GameClock.TimeOf(0, 5));
        Assert.LessOrEqual(string.CompareOrdinal(GameClock.TimeOf(4, 5), "20:00"), 0);
    }

    [Test]
    public void FueraDeRangoNoRompe()
    {
        Assert.AreEqual(GameClock.TimeOf(0, 5), GameClock.TimeOf(-3, 5));
        StringAssert.IsMatch(@"^\d\d:\d\d$", GameClock.TimeOf(40, 5));
        StringAssert.IsMatch(@"^\d\d:\d\d$", GameClock.TimeOf(2, 0));
    }

    // Auto-scroll: se pega al final solo si el jugador ya estaba abajo
    [TestCase(0f, 2000f, 800f, true)]      // Abajo del todo
    [TestCase(0.01f, 2000f, 800f, true)]   // Casi abajo (12 px)
    [TestCase(0.5f, 2000f, 800f, false)]   // Ha subido a leer
    [TestCase(1f, 500f, 800f, true)]       // Todo cabe: no hay nada que respetar
    public void AutoScrollRespetaAlQueLee(float normalized, float content, float viewport, bool expected)
    {
        Assert.AreEqual(expected, ChatScrollPolicy.IsAtBottom(normalized, content, viewport));
    }

    [Test]
    public void EntradasDeSistemaNoSonBurbujas()
    {
        Assert.IsTrue(ChatEntry.Player("a", null, "09:00").IsBubble);
        Assert.IsTrue(ChatEntry.Suspect("X", "a", "09:00").IsBubble);
        Assert.IsFalse(ChatEntry.Day(2, "parte").IsBubble);
        Assert.IsFalse(ChatEntry.System(ChatEntryKind.Notice, "x").IsBubble);
    }
}
