using NUnit.Framework;
using UnityEngine;

public class SafeAreaFitterTests
{
    [Test]
    public void PantallaSinMuescaOcupaTodo()
    {
        var (min, max) = SafeAreaFitter.Anchors(new Rect(0, 0, 1080, 2400), new Vector2(1080, 2400));
        Assert.AreEqual(Vector2.zero, min);
        Assert.AreEqual(Vector2.one, max);
    }

    [Test]
    public void MuescaArribaYBarraAbajoRecortanElContenido()
    {
        // 120 px de muesca arriba y 60 de barra de gestos abajo
        var (min, max) = SafeAreaFitter.Anchors(new Rect(0, 60, 1080, 2220), new Vector2(1080, 2400));
        Assert.AreEqual(0f, min.x, 1e-4f);
        Assert.AreEqual(60f / 2400f, min.y, 1e-4f);
        Assert.AreEqual(1f, max.x, 1e-4f);
        Assert.AreEqual(2280f / 2400f, max.y, 1e-4f);
    }

    [Test]
    public void AreaSeguraFueraDeRangoSeRecorta()
    {
        var (min, max) = SafeAreaFitter.Anchors(new Rect(-10, -10, 2000, 3000), new Vector2(1080, 2400));
        Assert.AreEqual(Vector2.zero, min);
        Assert.AreEqual(Vector2.one, max);
    }

    [Test]
    public void PantallaDeTamanoCeroNoRompe()
    {
        var (min, max) = SafeAreaFitter.Anchors(new Rect(0, 0, 0, 0), Vector2.zero);
        Assert.AreEqual(Vector2.zero, min);
        Assert.AreEqual(Vector2.one, max);
    }
}
