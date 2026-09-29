using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class ThemeTests
{
    private Theme theme;

    [SetUp]
    public void SetUp()
    {
        theme = Theme.CreateDefault();
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(theme);
        ThemeManager.Override(null);
    }

    [Test]
    public void TodosLosColoresDelTemaSonOpacosOTransparentesAPropósito()
    {
        foreach (FieldInfo field in typeof(Theme).GetFields().Where(f => f.FieldType == typeof(Color)))
        {
            var color = (Color)field.GetValue(theme);
            Assert.Greater(color.a, 0f, field.Name);
        }
    }

    [Test]
    public void TipografiaLegibleEnMovilYConJerarquia()
    {
        Assert.GreaterOrEqual(theme.secondarySize, Theme.MinReadableSize, "secundario legible");
        Assert.Greater(theme.bodySize, theme.secondarySize);
        Assert.Greater(theme.headingSize, theme.bodySize);
        Assert.Greater(theme.titleSize, theme.headingSize);
    }

    [Test]
    public void CadaEstadoTieneTinteEnElTema()
    {
        foreach (Emotion e in Enum.GetValues(typeof(Emotion)))
            Assert.Greater(theme.EmotionTint(e).a, 0f, e.ToString());
    }

    [Test]
    public void EstiloEmocionalSaleDelTemaActivo()
    {
        theme.nerviousShake = 7f;
        ThemeManager.Override(theme);

        Assert.AreEqual(7f, EmotionStyle.For(Emotion.Nervioso).shakeAmplitude);
        Assert.AreEqual(theme.EmotionTint(Emotion.Triste), EmotionStyle.For(Emotion.Triste).tint);
    }

    [Test]
    public void SinAssetElGestorDevuelveElTemaPorDefecto()
    {
        ThemeManager.Override(null);

        Assert.IsNotNull(ThemeManager.Current);
        Assert.AreEqual(Theme.CreateDefault().bodySize, ThemeManager.Current.bodySize);
    }

    [Test]
    public void HexParaTextoEnriquecido()
    {
        Assert.AreEqual("#FF8000", Theme.Hex(new Color(1f, 0.5f, 0f)));
    }

    [Test]
    public void DuracionesPositivas()
    {
        foreach (FieldInfo field in typeof(Theme).GetFields().Where(f => f.Name.EndsWith("Duration")))
            Assert.Greater((float)field.GetValue(theme), 0f, field.Name);
    }
}
