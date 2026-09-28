using System.Linq;
using NUnit.Framework;

public class InvestigationStateTests
{
    private InvestigationState NewState() => new InvestigationState(TestCases.Variant());

    [Test]
    public void Discover_DevuelveFalsoSiRepetida()
    {
        var s = NewState();

        Assert.IsTrue(s.Discover("i1"));
        Assert.IsFalse(s.Discover("i1"));
        CollectionAssert.AreEqual(new[] { "i1" }, s.DiscoveredClueIds);
    }

    [Test]
    public void Contradiccion_RequiereMentiraOConfrontacion()
    {
        var s = NewState();
        s.Discover("x");

        Assert.IsEmpty(s.UpdateContradictions());

        s.RegisterLieTold();

        Assert.AreEqual("x", s.UpdateContradictions().Single().id);
        Assert.IsEmpty(s.UpdateContradictions());
        CollectionAssert.AreEqual(new[] { "x" }, s.ContradictionClueIds);
    }

    [Test]
    public void Contradiccion_MentiraSinPistaNoCuenta()
    {
        var s = NewState();
        s.RegisterLieTold();

        Assert.IsEmpty(s.UpdateContradictions());
    }

    [Test]
    public void Contradiccion_PorConfrontarAlCulpable()
    {
        var s = NewState();
        s.Discover("x");
        s.RegisterShown("a", "x");

        Assert.AreEqual(1, s.UpdateContradictions().Count);
    }

    [Test]
    public void Contradiccion_ConfrontarAUnInocenteNoCuenta()
    {
        var s = NewState();
        s.Discover("x");
        s.RegisterShown("b", "x");

        Assert.IsEmpty(s.UpdateContradictions());
        CollectionAssert.AreEqual(new[] { "x" }, s.ShownTo("b"));
    }

    [TestCase(new[] { "i1", "i2", "i3", "x" }, true, Ending.Good)]      // 4 + 2 = 6
    [TestCase(new[] { "i1", "i2", "x" }, true, Ending.Good)]            // 3 + 2 = 5
    [TestCase(new[] { "i1", "x" }, true, Ending.Bittersweet)]           // 2 + 2 = 4
    [TestCase(new[] { "i1", "i2", "i3" }, false, Ending.Bittersweet)]   // 3
    [TestCase(new[] { "i1", "d", "ctx" }, false, Ending.Insufficient)]  // 1: descartes y contexto no suman
    public void Final_PorEvidencia(string[] found, bool lieTold, Ending expected)
    {
        var s = NewState();
        foreach (string id in found) s.Discover(id);
        if (lieTold) s.RegisterLieTold();
        s.UpdateContradictions();

        AccusationResult r = s.Accuse("a");

        Assert.IsTrue(r.correct);
        Assert.AreEqual(expected, r.ending);
        Assert.AreEqual(s.Evidence, r.evidence);
    }

    [Test]
    public void Final_AcusarInocenteConPistaDeDescarte()
    {
        var s = NewState();
        s.Discover("d");

        AccusationResult r = s.Accuse("b");

        Assert.IsFalse(r.correct);
        Assert.AreEqual(Ending.Bad, r.ending);
        Assert.IsTrue(r.ignoredClearingClue);
    }

    [Test]
    public void Final_AcusarInocenteSinPistaDeDescarte()
    {
        AccusationResult r = NewState().Accuse("c");

        Assert.AreEqual(Ending.Bad, r.ending);
        Assert.IsFalse(r.ignoredClearingClue);
    }

    [Test]
    public void MaxEvidenceWithoutCulprit_CuentaIncriminatoriasYContradicciones()
    {
        // i1, i2, i3, x = 4 incriminatorias + 1 contradicción × 2
        Assert.AreEqual(6, InvestigationState.MaxEvidenceWithoutCulprit(TestCases.Variant()));
    }
}
