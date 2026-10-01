using NUnit.Framework;

/// <summary>
/// PERFORMANCE-AUDIT, mejora 3: las anclas de las pistas (datos fijos de la historia) se normalizaban en cada
/// llamada a Evaluate (~60 por respuesta) y EmotionParser.Parse se repetía hasta 7 veces sobre el mismo texto.
/// Se cuenta el trabajo real, no el tiempo (los tiempos son frágiles en un test).
/// </summary>
public class ParseOnceTests
{
    [Test]
    public void LasAnclasSeNormalizanUnaSolaVez()
    {
        ClueDetector.ResetAnchorCache();
        string[][] groups = { new[] { "Bolsa de Basura", "contenedor" }, new[] { "a las 23:05", "despidió" } };
        string text = ClueDetector.Normalize("Tiró la bolsa de basura al contenedor a las 23:05 y se despidió.");
        Assert.IsTrue(ClueDetector.Evaluate(groups, text).Matched);
        int first = ClueDetector.AnchorNormalizations;
        Assert.AreEqual(4, first, "cuatro anclas distintas, una vez cada una");
        for (int i = 0; i < 10; i++)
            ClueDetector.Evaluate(groups, text);
        Assert.AreEqual(first, ClueDetector.AnchorNormalizations, "las siguientes respuestas no las vuelven a normalizar");
    }

    [Test]
    public void LaNegacionSigueFuncionandoIgual()
    {
        string[][] groups = { new[] { "vi el coche" } };
        Assert.IsFalse(ClueDetector.Evaluate(groups, ClueDetector.Normalize("Yo no vi el coche, inspector.")).Matched);
        Assert.IsTrue(ClueDetector.Evaluate(groups, ClueDetector.Normalize("No sé. Vi el coche rojo.")).Matched,
                      "la negación de otra frase no cuenta");
        Assert.IsTrue(ClueDetector.Evaluate(groups, ClueDetector.Normalize("Vi el coche.")).Matched);
    }

    [Test]
    public void LaMismaRespuestaSeAnalizaUnaSolaVez()
    {
        EmotionParser.ResetCache();
        const string raw = "Yo esa noche no salí de casa. [ESTADO: nervioso]";
        EmotionParse first = EmotionParser.Parse(raw);
        for (int i = 0; i < 6; i++)
            Assert.AreEqual(first.text, EmotionParser.Parse(raw).text);
        Assert.AreEqual(1, EmotionParser.ParseWork, "siete llamadas con el mismo texto, un solo análisis");
        Assert.AreEqual(Emotion.Nervioso, EmotionParser.Parse(raw).emotion);
        EmotionParser.Parse("Otra respuesta distinta. [ESTADO: triste]");
        Assert.AreEqual(2, EmotionParser.ParseWork);
    }
}
