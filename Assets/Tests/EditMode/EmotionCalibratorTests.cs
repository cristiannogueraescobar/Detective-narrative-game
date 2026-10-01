using NUnit.Framework;

public class EmotionCalibratorTests
{
    [Test]
    public void PreguntasDeCadaTipo()
    {
        StoryData story = TestCases.Story();
        story.victim = "Elena";
        CharacterRole role = story.variants[0].Role("b"); // nervousAbout = "el tabaco"

        Assert.AreEqual("¿A qué se dedica usted?", EmotionCalibrator.ProbeQuestion(EmotionProbe.Neutra, story, role));
        Assert.AreEqual("¿Cómo era Elena?", EmotionCalibrator.ProbeQuestion(EmotionProbe.Victima, story, role));
        Assert.AreEqual("Hablemos de el tabaco. ¿Qué me cuenta?", EmotionCalibrator.ProbeQuestion(EmotionProbe.TemaSensible, story, role));
        StringAssert.Contains("culpable", EmotionCalibrator.ProbeQuestion(EmotionProbe.Acusacion, story, role));
    }

    [Test]
    public void TemaSensibleUsaSoloLaPrimeraParte()
    {
        StoryData story = TestCases.Story();
        CharacterRole role = new CharacterRole { nervousAbout = "tus pastillas para dormir; temes que piensen que fue culpa tuya." };

        Assert.AreEqual("Hablemos de tus pastillas para dormir. ¿Qué me cuenta?",
            EmotionCalibrator.ProbeQuestion(EmotionProbe.TemaSensible, story, role));
    }

    [TestCase(8, 10, true)]
    [TestCase(7, 10, false)]
    [TestCase(0, 0, false)]
    public void UmbralOchoDeDiez(int ok, int total, bool passes)
    {
        Assert.AreEqual(passes, EmotionCalibrator.Passes(ok, total));
    }
}
