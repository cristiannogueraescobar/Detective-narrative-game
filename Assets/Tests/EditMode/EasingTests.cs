using NUnit.Framework;

public class EasingTests
{
    [TestCase(0f, 0f)]
    [TestCase(1f, 1f)]
    public void ExtremosFijos(float t, float expected)
    {
        Assert.AreEqual(expected, Easing.OutCubic(t), 1e-5);
        Assert.AreEqual(expected, Easing.OutBack(t), 1e-5);
        Assert.AreEqual(expected, Easing.InOutSine(t), 1e-5);
    }

    [Test]
    public void OutBackSobrepasaAntesDeAsentarse()
    {
        Assert.Greater(Easing.OutBack(0.7f), 1f);
    }

    [Test]
    public void PulsoSubeYVuelveACero()
    {
        Assert.AreEqual(0f, Easing.Pulse(0f), 1e-5);
        Assert.AreEqual(1f, Easing.Pulse(0.5f), 1e-5);
        Assert.AreEqual(0f, Easing.Pulse(1f), 1e-5);
    }

    [Test]
    public void FueraDeRangoSeLimita()
    {
        Assert.AreEqual(1f, Easing.OutCubic(3f), 1e-5);
        Assert.AreEqual(0f, Easing.OutCubic(-1f), 1e-5);
    }
}
