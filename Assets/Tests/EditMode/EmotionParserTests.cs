using NUnit.Framework;

public class EmotionParserTests
{
    [Test]
    public void EtiquetaBienFormadaSeQuitaYSeLee()
    {
        EmotionParse p = EmotionParser.Parse("No entré en su cuarto, inspector.\n[ESTADO: nervioso]");

        Assert.AreEqual("No entré en su cuarto, inspector.", p.text);
        Assert.AreEqual(Emotion.Nervioso, p.emotion);
        Assert.IsTrue(p.wellFormed);
    }

    [TestCase("Hola. [estado: Enfadado]", Emotion.Enfadado)]
    [TestCase("Hola. [ESTADO:triste]", Emotion.Triste)]
    [TestCase("Hola. [ ESTADO : asustada ]", Emotion.Asustado)]
    [TestCase("Hola. [ESTADO: tranquila]", Emotion.Tranquilo)]
    public void ToleraMayusculasEspaciosYFemenino(string raw, Emotion expected)
    {
        EmotionParse p = EmotionParser.Parse(raw);

        Assert.AreEqual(expected, p.emotion);
        Assert.AreEqual("Hola.", p.text);
        Assert.IsTrue(p.wellFormed);
    }

    [Test]
    public void SinCorchetesSeLeePeroNoCuentaComoBienFormada()
    {
        EmotionParse p = EmotionParser.Parse("Déjeme en paz.\nESTADO: enfadado");

        Assert.AreEqual(Emotion.Enfadado, p.emotion);
        Assert.AreEqual("Déjeme en paz.", p.text);
        Assert.IsFalse(p.wellFormed);
    }

    [Test]
    public void EstadoDesconocidoNoSeAsignaPeroSeLimpia()
    {
        EmotionParse p = EmotionParser.Parse("Bueno. [ESTADO: confuso]");

        Assert.IsNull(p.emotion);
        Assert.AreEqual("Bueno.", p.text);
        Assert.IsFalse(p.wellFormed);
    }

    [Test]
    public void SinEtiquetaDevuelveElTextoIntacto()
    {
        EmotionParse p = EmotionParser.Parse("Estuve en casa.");

        Assert.IsNull(p.emotion);
        Assert.AreEqual("Estuve en casa.", p.text);
        Assert.IsFalse(p.wellFormed);
    }

    [Test]
    public void VariasEtiquetasGanaLaUltimaYSeQuitanTodas()
    {
        EmotionParse p = EmotionParser.Parse("[ESTADO: tranquilo] Estuve en casa. [ESTADO: nervioso]");

        Assert.AreEqual(Emotion.Nervioso, p.emotion);
        Assert.AreEqual("Estuve en casa.", p.text);
    }

    [Test]
    public void TextoNuloOVacio()
    {
        Assert.AreEqual("", EmotionParser.Parse(null).text);
        Assert.IsNull(EmotionParser.Parse("").emotion);
    }
}
