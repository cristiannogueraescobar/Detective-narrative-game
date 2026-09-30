using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class FxTests
{
    [Test]
    public void ElSelloGolpeaDesdeGrandeYSeQueda()
    {
        Assert.Greater(FxCurves.StampScale(0f), 1.8f, "empieza grande, como si bajara hacia la mesa");
        Assert.AreEqual(1f, FxCurves.StampScale(1f), 1e-4f);
        Assert.AreEqual(0f, FxCurves.StampAlpha(0f), 1e-4f);
        Assert.AreEqual(1f, FxCurves.StampAlpha(0.3f), 1e-4f, "a mitad de caída ya es opaco");
        // El impacto llega pronto: después no se mueve
        Assert.AreEqual(1f, FxCurves.StampScale(0.5f), 0.02f);
    }

    [Test]
    public void LaFichaCaeGirandoYSeEndereza()
    {
        Assert.Less(FxCurves.CardFallAngle(0f), -60f);
        Assert.AreEqual(0f, FxCurves.CardFallAngle(1f), 1e-3f);
        Assert.Greater(FxCurves.CardFallOffset(0f), 200f, "empieza por encima");
        Assert.AreEqual(0f, FxCurves.CardFallOffset(1f), 1e-3f);
    }

    [Test]
    public void ElDestelloCruzaLaFichaUnaVez()
    {
        Assert.Less(FxCurves.GlintPosition(0f), 0f);
        Assert.Greater(FxCurves.GlintPosition(1f), 1f);
    }

    [Test]
    public void LaViñetaSeCierraPocoAPoco()
    {
        Assert.AreEqual(0f, FxCurves.VignetteClose(0f), 1e-4f);
        Assert.Greater(FxCurves.VignetteClose(0.5f), 0.2f);
        Assert.LessOrEqual(FxCurves.VignetteClose(10f), 1f);
    }

    [Test]
    public void ElLatidoTieneDosGolpes()
    {
        // Un ciclo: lub-dub, dos máximos
        int peaks = 0;
        float prev = FxCurves.Heartbeat(0f), prevDelta = 0f;
        for (float t = 0.005f; t < 1f; t += 0.005f)
        {
            float v = FxCurves.Heartbeat(t);
            float delta = v - prev;
            if (prevDelta > 0f && delta <= 0f && v > 0.3f)
                peaks++;
            prevDelta = delta;
            prev = v;
        }
        Assert.AreEqual(2, peaks);
    }

    [Test]
    public void LineaTemporalSeparaFrasesYHoras()
    {
        string epilogue = "Daniel llevaba un año robando. A las 22:30 le subió un cacao con zolpidem. A las 23:05 fingió encontrarla y a las 23:15 llamó al 112. Rosario lo vio todo.";
        var steps = Timeline.FromEpilogue(epilogue).ToList();

        Assert.AreEqual(4, steps.Count);
        Assert.IsNull(steps[0].time);
        Assert.AreEqual("22:30", steps[1].time);
        Assert.AreEqual("23:05", steps[2].time);
        StringAssert.StartsWith("Daniel", steps[0].text);
        StringAssert.EndsWith("112.", steps[2].text);
    }

    [Test]
    public void LineaTemporalNoPartePorAbreviaturas()
    {
        var steps = Timeline.FromEpilogue("El Sr. Gil salió a las 5:10. Volvió tarde.").ToList();
        Assert.AreEqual(2, steps.Count);
        Assert.AreEqual("5:10", steps[0].time);
    }

    [Test]
    public void LineaTemporalVacia()
    {
        CollectionAssert.IsEmpty(Timeline.FromEpilogue(""));
        CollectionAssert.IsEmpty(Timeline.FromEpilogue(null));
    }

    [TestCase(Ending.Good, "CASO CERRADO")]
    [TestCase(Ending.Bittersweet, "CERRADO CON DUDAS")]
    [TestCase(Ending.Insufficient, "SOBRESEÍDO")]
    [TestCase(Ending.Bad, "CASO FALLIDO")]
    public void CadaFinalTieneSuSello(Ending ending, string stamp)
    {
        Assert.AreEqual(stamp, EndingStyle.For(ending, ThemeManager.Current).stamp);
    }

    [Test]
    public void LosFinalesSeDistinguenPorColor()
    {
        Theme t = ThemeManager.Current;
        var colors = System.Enum.GetValues(typeof(Ending)).Cast<Ending>().Select(e => EndingStyle.For(e, t).grade).ToList();
        Assert.AreEqual(colors.Count, colors.Distinct().Count());
    }
}

public class EndingReportTests
{
    private static string Build(Ending ending, bool ignored = false)
    {
        var result = new AccusationResult { ending = ending, correct = ending != Ending.Bad, evidence = 3, incriminatingFound = 2, contradictions = 1, ignoredClearingClue = ignored };
        return EndingReport.Build(result, "Daniel Mendoza", "Carmen Vidal", 7,
            "Carmen mintió. A las 22:30 subió el cacao. A las 23:15 llamó al 112.", ThemeManager.Current);
    }

    [Test]
    public void IncluyeAcusacionCulpableYEvidencia()
    {
        string text = Build(Ending.Bad);
        StringAssert.Contains("Daniel Mendoza", text);
        StringAssert.Contains("Carmen Vidal", text);
        StringAssert.Contains("3/7", text);
    }

    [Test]
    public void LaVerdadVaComoLineaTemporalConHoras()
    {
        string[] lines = Build(Ending.Good).Split('\n');
        Assert.IsTrue(lines.Any(l => l.Contains("22:30") && l.Contains("subió el cacao")));
        Assert.IsTrue(lines.Any(l => l.Contains("23:15") && l.Contains("112")));
        Assert.IsTrue(lines.Any(l => l.Contains("Carmen mintió")), "las frases sin hora también salen");
    }

    [Test]
    public void CadaFinalUsaSuVeredicto()
    {
        foreach (Ending e in System.Enum.GetValues(typeof(Ending)))
            StringAssert.Contains(EndingStyle.For(e, ThemeManager.Current).verdict, Build(e));
    }

    [Test]
    public void DescartarUnaPistaPropiaSeExplica()
    {
        StringAssert.Contains("descartaban", Build(Ending.Bad, ignored: true));
    }
}
