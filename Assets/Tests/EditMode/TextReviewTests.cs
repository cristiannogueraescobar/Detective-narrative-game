using NUnit.Framework;

/// <summary>
/// Sesión C, bloque C (revisión de textos con ux-copy; tabla en docs/TEXT-REVIEW.md): concordancia sin género (la mitad
/// del reparto son mujeres), glosario (pista / prueba / caso) y números pequeños en letra.
/// </summary>
public class TextReviewTests
{
    [Test]
    public void LaContradiccionDelTutorialNoTieneGenero()
    {
        string text = Tutorial.TextOf(Tutorial.Contradiction);
        StringAssert.StartsWith("¡Has pillado una mentira!", text);
        StringAssert.DoesNotContain("Lo has pillado", text);
    }

    [Test]
    public void LaLibretaDelTutorialDiceQueSeApunta()
    {
        StringAssert.Contains("marcas sospecha o descarte", Tutorial.TextOf(Tutorial.Versions));
    }

    [TestCase(0, "Pensar")]
    [TestCase(1, "Pensar (una pregunta)")]
    [TestCase(2, "Pensar (dos preguntas)")]
    public void PensarDiceSuCosteEnLetra(int cost, string expected)
    {
        Assert.AreEqual(expected, GameTexts.ThinkLabel(cost));
    }

    [TestCase(DifficultyLevel.Historia, "Siete preguntas al día.")]
    [TestCase(DifficultyLevel.Veterano, "Cuatro preguntas al día.")]
    [TestCase(DifficultyLevel.Detective, "Cinco preguntas al día.")]
    public void LaDificultadDiceLasPreguntasEnLetra(DifficultyLevel level, string start)
    {
        StringAssert.StartsWith(start, Difficulty.Description(level));
    }

    [Test]
    public void LasInstruccionesNombranCadaFinal()
    {
        string text = GameTexts.Instructions(ThemeManager.Current);
        StringAssert.Contains("el séptimo día tendrás que hacerlo", text);
        StringAssert.Contains("Culpable sin pruebas: sobreseído, sale libre.", text);
    }
}
