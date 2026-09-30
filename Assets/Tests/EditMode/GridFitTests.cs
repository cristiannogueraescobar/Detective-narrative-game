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
}
