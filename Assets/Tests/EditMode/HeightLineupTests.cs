using System.Linq;
using NUnit.Framework;

/// <summary>
/// Sesión A, bloque 4: la rueda de reconocimiento en una sola fila, con alturas reales sobre la pared de comisaría.
/// Todos pisan el mismo suelo, a la misma escala que las rayas de la pared.
/// </summary>
public class HeightLineupTests
{
    private static readonly (int cm, float aspect)[] Four = { (178, 0.55f), (166, 0.39f), (174, 0.38f), (155, 0.55f) };

    [Test]
    public void LasAlturasSonProporcionalesYCabenEnUnaFila()
    {
        var plan = HeightLineup.Plan(1000f, 900f, 90f, Four);
        float total = 0f;
        for (int i = 0; i < Four.Length; i++)
        {
            Assert.AreEqual(Four[i].cm * plan.pxPerCm, plan.figureHeights[i], 0.01f, "altura real × escala");
            Assert.GreaterOrEqual(plan.cellWidths[i], plan.figureHeights[i] * Four[i].aspect - 0.01f, "cabe en su hueco");
            total += plan.cellWidths[i];
        }
        Assert.AreEqual(1000f, total, 0.5f, "los huecos llenan la fila, sin salirse");
        Assert.Greater(plan.figureHeights[0], plan.figureHeights[3], "Daniel (178) más alto que Amparo (155)");
    }

    [Test]
    public void LaParedLlegaADosMetrosYLasRayasUsanLaMismaEscala()
    {
        var plan = HeightLineup.Plan(1000f, 900f, 90f, Four);
        Assert.LessOrEqual(90f + 200f * plan.pxPerCm, 900f + 0.01f, "200 cm caben en la pared");
        Assert.AreEqual(90f + 170f * plan.pxPerCm, plan.MarkY(170), 0.01f, "la raya de 170 está a 170 cm del suelo");
    }

    [Test]
    public void ConMasSospechososLaEscalaBajaPeroNadieSeSaleDeSuHueco()
    {
        var five = Four.Concat(new (int cm, float aspect)[] { (183, 0.6f) }).ToArray();
        var plan4 = HeightLineup.Plan(1000f, 900f, 90f, Four);
        var plan5 = HeightLineup.Plan(1000f, 900f, 90f, five);
        Assert.LessOrEqual(plan5.pxPerCm, plan4.pxPerCm + 1e-4f);
        for (int i = 0; i < five.Length; i++)
            Assert.GreaterOrEqual(plan5.cellWidths[i], plan5.figureHeights[i] * five[i].aspect - 0.01f);
    }

    // Huecos a la medida de cada figura: los estrechos no gastan sitio y la escala crece
    [Test]
    public void LasFigurasEstrechasNoDesperdicianSitio()
    {
        var plan = HeightLineup.Plan(1000f, 2000f, 90f, Four);
        float sum = 0f;
        for (int i = 0; i < Four.Length; i++)
            sum += Four[i].aspect * Four[i].cm;
        Assert.AreEqual(1000f * 0.92f / sum, plan.pxPerCm, 1e-3f, "la limita el ancho total, no la figura más ancha");
    }
    // Revisión de Cristian (sesión C): con tres o más la fila la limitaba el ancho y media pantalla quedaba vacía.
    // Con solape las figuras crecen, los huecos (botón y anillo) siguen sin pisarse y nada asoma fuera de la fila
    [Test]
    public void ConSolapeLasFigurasCrecenSinSalirseDeLaFila()
    {
        var three = Four.Take(3).ToArray();
        var plain = HeightLineup.Plan(900f, 1100f, 64f, three);
        var overlapped = HeightLineup.Plan(900f, 1100f, 64f, three, 0.3f);
        Assert.Greater(overlapped.pxPerCm, plain.pxPerCm * 1.25f, "más grandes");

        float x = overlapped.startX, total = overlapped.startX;
        for (int i = 0; i < three.Length; i++)
        {
            float figure = overlapped.figureHeights[i] * three[i].aspect;
            float center = x + overlapped.cellWidths[i] * 0.5f;
            Assert.GreaterOrEqual(center - figure * 0.5f, -0.5f, $"la figura {i} no asoma por la izquierda");
            Assert.LessOrEqual(center + figure * 0.5f, 900.5f, $"la figura {i} no asoma por la derecha");
            Assert.GreaterOrEqual(overlapped.cellWidths[i], figure * 0.7f - 0.01f, "su hueco, al menos lo que no se solapa");
            x += overlapped.cellWidths[i];
            total += overlapped.cellWidths[i];
        }
        Assert.LessOrEqual(total, 900.5f, "los huecos caben en la fila");
    }
}
