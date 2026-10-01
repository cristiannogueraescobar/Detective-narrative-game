using NUnit.Framework;

public class PremiseCalibratorTests
{
    [TestCase("No, eso es mentira. Yo no discutí con nadie esa noche.")]
    [TestCase("¿Discutir? Nunca. Quien le haya dicho eso se equivoca.")]
    [TestCase("Eso no pasó, inspector.")]
    [TestCase("No recuerdo ninguna discusión, de verdad.")]
    [TestCase("Yo no he discutido con Marcos en mi vida. [ESTADO: enfadado]")]
    [TestCase("¡Qué va! Si apenas nos hablamos.")]
    public void NegarLaPremisa(string answer)
    {
        Assert.IsTrue(PremiseCalibrator.Denies(answer));
    }

    [TestCase("Sí, discutimos. Me debía dinero y se lo reclamé.")]
    [TestCase("Discutimos porque no me dejaba en paz, nada más.")]
    [TestCase("Fue una tontería, por el aparcamiento. No tiene importancia.")]
    public void AceptarLaPremisa(string answer)
    {
        Assert.IsFalse(PremiseCalibrator.Denies(answer));
    }

    [Test]
    public void LaPreguntaNombraAOtroSospechoso()
    {
        StoryData story = TestCases.Story();
        string q = PremiseCalibrator.Question(story, story.cast[0].id);
        StringAssert.Contains(story.cast[1].shortName, q);
        StringAssert.DoesNotContain(story.cast[0].shortName + " ", q);
    }

    [Test]
    public void SinNingunaRespuestaLaSondaFalla()
    {
        // Con Ollama apagado todas las peticiones fallan: un informe vacío no puede salir como éxito
        Assert.AreEqual(2, PremiseCalibrator.ExitCode(0));
        Assert.AreEqual(0, PremiseCalibrator.ExitCode(5));
    }
}
