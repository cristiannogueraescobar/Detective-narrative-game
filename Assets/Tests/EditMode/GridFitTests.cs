using NUnit.Framework;
using UnityEngine;

public class GridFitTests
{
    [TestCase(2, 1016f, 800f)]
    [TestCase(4, 1016f, 800f)]
    [TestCase(4, 1183f, 500f)]
    [TestCase(6, 900f, 700f)]
    [TestCase(7, 1016f, 1100f)]
    public void LasTarjetasCabenEnElHueco(int count, float w, float h)
    {
        var (columns, cell) = GridFit.Compute(count, new Vector2(w, h), 16f, 64f, 120f);
        int rows = Mathf.CeilToInt(count / (float)columns);
        Assert.LessOrEqual(columns * cell.x + (columns - 1) * 16f, w + 0.01f);
        Assert.LessOrEqual(rows * cell.y + (rows - 1) * 16f, h + 0.01f);
    }

    [Test]
    public void ConSitioSonTarjetasDeRetrato()
    {
        var (_, cell) = GridFit.Compute(4, new Vector2(1016f, 1200f), 16f, 64f, 120f);
        Assert.AreEqual(0.75f, cell.x / (cell.y - 64f), 0.01f);
        Assert.GreaterOrEqual(cell.x, 120f);
    }

    // Revisión 6: con alguien descartado el pie mide 96 (dos líneas); con el máximo de sospechosos de una historia en
    // un móvil 16:9, los bustos siguen siendo grandes
    [Test]
    public void ConElPieAltoLosRetratosSiguenGrandes()
    {
        foreach (int count in new[] { 4, 5, 6 })
        {
            var (_, cell) = GridFit.Compute(count, new Vector2(1016f, 1000f), 16f, 96f, 120f);
            Assert.GreaterOrEqual(cell.y - 96f - 8f, 200f, count + " sospechosos: busto de al menos 200 px");
        }
    }
}
