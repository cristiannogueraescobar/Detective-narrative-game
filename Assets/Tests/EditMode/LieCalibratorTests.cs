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

    // Solo lo que un jugador pregunta de entrada por su noche: las preguntas de la pista que rompe la mentira ya
    // presionan para que confiese, y confesar no es "no mentir" (calibración de la fase 1: 1C daba 4/10 por eso)
    [Test]
    public void SeLePreguntaSoloPorSuNoche()
    {
        CaseLibrary.TryFind("1B", out _, out VariantData v);
        CollectionAssert.AreEqual(LieCalibrator.GenericQuestions, LieCalibrator.Questions(v, "padre").ToArray());
    }

    [Test]
    public void LaPruebaQueSeLeEnsenaEsLaQueRompeSuMentira()
    {
        CaseLibrary.TryFind("1C", out _, out VariantData v);
        Assert.AreEqual("1C_llamada", LieCalibrator.BreakingClue(v, "padre").id);
        Assert.AreEqual("1C_gritos", LieCalibrator.BreakingClue(v, "hermano").id);
    }
}
