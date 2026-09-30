using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Retratos 2.5D (C3): relieve + lámpara con un interruptor del tema que devuelve el plano 2D.
/// </summary>
public class LitPortraitTests
{
    private Theme theme;
    private GameObject go;

    [SetUp]
    public void SetUp()
    {
        theme = ScriptableObject.CreateInstance<Theme>();
        ThemeManager.Override(theme);
        go = new GameObject("Retrato", typeof(RawImage));
    }

    [TearDown]
    public void TearDown()
    {
        ThemeManager.Override(null);
        Object.DestroyImmediate(go);
        Object.DestroyImmediate(theme);
    }

    [Test]
    public void ShaderDeRelieveDisponibleYCompilado()
    {
        Shader shader = Shader.Find(ArtGrading.LitShaderName);

        Assert.IsNotNull(shader, "el shader debe estar en Resources para incluirse en las builds");
        Assert.IsTrue(shader.isSupported);
    }

    [Test]
    public void RetratoAntiguoConRelievePorDefecto()
    {
        var raw = go.GetComponent<RawImage>();

        ArtGrading.Apply(raw, ArtGrading.Kind.LegacyPortrait);

        Assert.AreEqual(ArtGrading.LitShaderName, raw.material.shader.name);
        Assert.AreEqual(theme.portraitAmbient, raw.material.GetFloat("_Ambient"), 1e-4);
    }

    [Test]
    public void ElTemaDevuelveElPlano2D()
    {
        theme.portraitLit = false;
        var raw = go.GetComponent<RawImage>();

        ArtGrading.Apply(raw, ArtGrading.Kind.LegacyPortrait);

        Assert.AreEqual(ArtGrading.ShaderName, raw.material.shader.name);
    }

    [Test]
    public void LosFondosNoLlevanRelieve()
    {
        var raw = go.GetComponent<RawImage>();

        ArtGrading.Apply(raw, ArtGrading.Kind.Background);

        Assert.AreEqual(ArtGrading.ShaderName, raw.material.shader.name);
    }

    [Test]
    public void LaEmocionConservaElRelieve()
    {
        var raw = go.GetComponent<RawImage>();
        ArtGrading.Apply(raw, ArtGrading.Kind.LegacyPortrait);
        var presenter = go.AddComponent<EmotionPresenter>();

        presenter.Apply(Emotion.Nervioso, true);

        Assert.AreEqual(ArtGrading.LitShaderName, raw.material.shader.name);
    }
}
