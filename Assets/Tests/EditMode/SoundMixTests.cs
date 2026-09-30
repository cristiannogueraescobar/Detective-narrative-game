using NUnit.Framework;

/// <summary>
/// Mezcla (D1): la música se aparta bajo los golpes (pista, contradicción, acusación, finales) y vuelve sola.
/// </summary>
public class SoundMixTests
{
    [TestCase(Sfx.Clue, true)]
    [TestCase(Sfx.Contradiction, true)]
    [TestCase(Sfx.Accusation, true)]
    [TestCase(Sfx.DayChange, true)]
    [TestCase(Sfx.EndingBad, true)]
    [TestCase(Sfx.Click, false)]
    [TestCase(Sfx.Typing, false)]
    [TestCase(Sfx.Send, false)]
    public void SoloLosGolpesApartanLaMusica(Sfx sfx, bool ducks)
    {
        Assert.AreEqual(ducks, SoundMix.Ducks(sfx));
    }

    [Test]
    public void SinGolpeLaMusicaVaEntera()
    {
        Assert.AreEqual(1f, SoundMix.DuckGain(float.PositiveInfinity, 1f), 1e-5);
        Assert.AreEqual(1f, SoundMix.DuckGain(-1f, 1f), 1e-5, "antes de ningún golpe");
    }

    [Test]
    public void BajaRapidoAguantaYVuelveSuave()
    {
        const float hold = 1.2f;
        Assert.AreEqual(SoundMix.Depth, SoundMix.DuckGain(SoundMix.Attack + 0.01f, hold), 1e-4, "tras el ataque, abajo");
        Assert.AreEqual(SoundMix.Depth, SoundMix.DuckGain(hold, hold), 1e-4, "aguanta mientras suena el golpe");
        float mid = SoundMix.DuckGain(hold + SoundMix.Release / 2f, hold);
        Assert.Greater(mid, SoundMix.Depth);
        Assert.Less(mid, 1f);
        Assert.AreEqual(1f, SoundMix.DuckGain(hold + SoundMix.Release + 0.01f, hold), 1e-4, "vuelve entera");
    }

    [Test]
    public void NuncaPorDebajoDeLaProfundidadNiPorEncimaDeUno()
    {
        for (float s = 0f; s < 6f; s += 0.05f)
        {
            float g = SoundMix.DuckGain(s, 2f);
            Assert.GreaterOrEqual(g, SoundMix.Depth - 1e-5f);
            Assert.LessOrEqual(g, 1f + 1e-5f);
        }
    }

    [Test]
    public void ElGolpeLargoNoTieneLaMusicaCallada()
    {
        Assert.LessOrEqual(SoundMix.HoldFor(8f), SoundMix.MaxHold);
        Assert.AreEqual(0.5f, SoundMix.HoldFor(0.5f), 1e-5);
    }
}
