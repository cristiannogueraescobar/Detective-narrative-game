using NUnit.Framework;
using UnityEngine;

public class DesktopWindowTests
{
    [Test]
    public void EnUnMonitorApaisadoEsUnaVentanaVertical()
    {
        Vector2Int size = DesktopWindow.SizeFor(1920, 1080);
        Assert.AreEqual(972, size.y, "90 % del alto de la pantalla");
        Assert.AreEqual(547, size.x, "proporción de móvil 9:16");
    }

    [Test]
    public void NuncaMasAnchaQueLaPantalla()
    {
        Vector2Int size = DesktopWindow.SizeFor(600, 2000);
        Assert.LessOrEqual(size.x, 540);
        Assert.AreEqual(size.x * 16, size.y * 9, 16, "sigue siendo 9:16");
    }

    [Test]
    public void PantallaDesconocidaUsaUnTamanoRazonable()
    {
        Vector2Int size = DesktopWindow.SizeFor(0, 0);
        Assert.AreEqual(new Vector2Int(540, 960), size);
    }
}
