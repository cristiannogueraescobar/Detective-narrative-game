using System.Linq;
using NUnit.Framework;

/// <summary>
/// Calibrador de mentiras (sesión C, mentiras de inocentes): qué se le pregunta a cada mentiroso.
/// </summary>
public class LieCalibratorTests
{
    [Test]
    public void LosMentirososDeUnaVarianteSonElCulpableYElInocenteQueMiente()
    {
        CaseLibrary.TryFind("1B", out _, out VariantData v);
        CollectionAssert.AreEquivalent(new[] { "madre", "padre" }, LieCalibrator.Liars(v).ToArray());
    }

    [Test]
    public void SeLePreguntaPorSuNocheYPorElTemaDeLaPistaQueRompeSuMentira()
    {
        CaseLibrary.TryFind("1B", out _, out VariantData v);
        string[] questions = LieCalibrator.Questions(v, "padre").ToArray();
        CollectionAssert.IsSubsetOf(LieCalibrator.GenericQuestions, questions);
        // La primera pregunta de 1B_cena (sin el segundo turno, que ya presiona)
        Assert.Contains("¿Dónde estuvo exactamente entre las 21:00 y las 23:00?", questions);
        Assert.IsTrue(questions.All(q => !q.Contains("||")));
    }

    [Test]
    public void LaPruebaQueSeLeEnsenaEsLaQueRompeSuMentira()
    {
        CaseLibrary.TryFind("1C", out _, out VariantData v);
        Assert.AreEqual("1C_llamada", LieCalibrator.BreakingClue(v, "padre").id);
        Assert.AreEqual("1C_gritos", LieCalibrator.BreakingClue(v, "hermano").id);
    }
}
