using NUnit.Framework;
using UnityEngine;

public class DesktopWindowTests
{
    [Test]
    public void EnUnMonitorApaisadoEsUnaVentanaVertical()
    {
        Vector2Int size = DesktopWindow.SizeFor(1920, 1080);
        Assert.AreEqual(918, size.y, "85 % del alto de la pantalla (deja sitio a la barra de título y de tareas)");
        Assert.AreEqual(516, size.x, "proporción de móvil 9:16");
    }

    [Test]
    public void EnUnPortatilPequenoCabeConLaBarraDeTareas()
    {
        Vector2Int size = DesktopWindow.SizeFor(1366, 768);
        Assert.LessOrEqual(size.y + 80, 768, "ventana + barra de título + barra de tareas");
    }

    [Test]
    public void NuncaMasAnchaQueLaPantalla()
    {
        Vector2Int size = DesktopWindow.SizeFor(600, 2000);
        Assert.LessOrEqual(size.x, 510);
        Assert.AreEqual(size.x * 16, size.y * 9, 16, "sigue siendo 9:16");
    }

    [Test]
    public void PantallaDesconocidaUsaUnTamanoRazonable()
    {
        Vector2Int size = DesktopWindow.SizeFor(0, 0);
        Assert.AreEqual(new Vector2Int(540, 960), size);
    }
}
