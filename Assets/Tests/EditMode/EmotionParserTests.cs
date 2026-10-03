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
    public void EtiquetaDeVariasPalabrasSeLeeYSeQuita()
    {
        EmotionParse p = EmotionParser.Parse("No sé nada. [ESTADO: muy nervioso]");

        Assert.AreEqual(Emotion.Nervioso, p.emotion);
        Assert.AreEqual("No sé nada.", p.text);
    }

    [TestCase("Estuve en casa. [ESTADO: nerv")]
    [TestCase("Estuve en casa. [ESTADO:")]
    [TestCase("Estuve en casa. [EST")]
    public void EtiquetaCortadaPorElLimiteDeTokensNoSeMuestra(string raw)
    {
        Assert.AreEqual("Estuve en casa.", EmotionParser.Parse(raw).text);
    }

    [Test]
    public void TextoNuloOVacio()
    {
        Assert.AreEqual("", EmotionParser.Parse(null).text);
        Assert.IsNull(EmotionParser.Parse("").emotion);
    }

    // Bot (ronda 5): qwen escribió "[MESTADO: asustado]" y la etiqueta se veía en el chat (y se copiaba después)
    [TestCase("No estuve en la finca.  [MESTADO: asustado]", Emotion.Asustado)]
    [TestCase("No estuve en la finca. [ESTADOS: triste]", Emotion.Triste)]
    [TestCase("No estuve en la finca. [Estado: Nervioso]", Emotion.Nervioso)]
    public void UnaEtiquetaConErratasNoSeVe(string raw, Emotion expected)
    {
        EmotionParse parse = EmotionParser.Parse(raw);
        Assert.AreEqual("No estuve en la finca.", parse.text);
        Assert.AreEqual(expected, parse.emotion);
    }

    [Test]
    public void LaEtiquetaConErratasNoCuentaComoBienFormada()
    {
        Assert.IsFalse(EmotionParser.Parse("Hola. [MESTADO: asustado]").wellFormed);
        Assert.IsTrue(EmotionParser.Parse("Hola. [ESTADO: asustado]").wellFormed);
    }

    [Test]
    public void LaFormaCanonicaCorrigeLaEtiqueta()
    {
        Assert.AreEqual("Hola.\n[ESTADO: asustado]", EmotionParser.Canonical("Hola.  [MESTADO: asustado]"));
        Assert.AreEqual("Hola.", EmotionParser.Canonical("Hola."));
    }

    // Tercera revisión: la etiqueta cortada con errata tampoco se ve; la no reconocida se conserva tal cual
    [Test]
    public void UnaEtiquetaCortadaConErrataNoSeVe()
    {
        Assert.AreEqual("Hola.", EmotionParser.Parse("Hola. [MESTADO: nerv").text);
    }

    [Test]
    public void SinEstadoReconocidoLaFormaCanonicaNoPierdeLaEtiqueta()
    {
        StringAssert.Contains("[ESTADO: pensativo]", EmotionParser.Canonical("Hola. [ESTADO: pensativo]"));
    }

    // Calibración de estados (sesión C): "… No estuve en casa esa noche.  ESTADO: asustado", sin corchetes y en la misma
    // línea; el chat lo enseñaba tal cual y el estado no contaba
    [TestCase("No estuve en casa esa noche.  ESTADO: asustado", Emotion.Asustado, "No estuve en casa esa noche.")]
    [TestCase("Nadie me vio, chaval. ESTADO: enfadado", Emotion.Enfadado, "Nadie me vio, chaval.")]
    public void LaEtiquetaSinCorchetesTrasElPuntoTambienCuenta(string raw, Emotion expected, string text)
    {
        EmotionParse p = EmotionParser.Parse(raw);
        Assert.AreEqual(expected, p.emotion);
        Assert.AreEqual(text, p.text);
    }

    [Test]
    public void LaPalabraEstadoEnMitadDeUnaFraseNoEsUnaEtiqueta()
    {
        EmotionParse p = EmotionParser.Parse("Le dije que su estado: nervioso perdido, no era normal.");
        Assert.IsNull(p.emotion);
    }
}
