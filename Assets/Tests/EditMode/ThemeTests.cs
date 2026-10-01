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

public class HighContrastTests
{
    private class MemoryStore : ISettingsStore
    {
        private readonly System.Collections.Generic.Dictionary<string, float> values = new System.Collections.Generic.Dictionary<string, float>();
        public float Get(string key, float fallback) => values.TryGetValue(key, out float v) ? v : fallback;
        public void Set(string key, float value) => values[key] = value;
    }

    [TearDown]
    public void TearDown() => GameSettings.UseStore(null);

    private static float Contrast(UnityEngine.Color a, UnityEngine.Color b)
    {
        float L(UnityEngine.Color c)
        {
            float ch(float v) => v <= 0.03928f ? v / 12.92f : UnityEngine.Mathf.Pow((v + 0.055f) / 1.055f, 2.4f);
            return 0.2126f * ch(c.r) + 0.7152f * ch(c.g) + 0.0722f * ch(c.b);
        }
        float la = L(a), lb = L(b);
        return (UnityEngine.Mathf.Max(la, lb) + 0.05f) / (UnityEngine.Mathf.Min(la, lb) + 0.05f);
    }

    [Test]
    public void ElAjusteCambiaElTemaActivo()
    {
        GameSettings.UseStore(new MemoryStore());
        Theme normal = ThemeManager.Current;
        GameSettings.HighContrast = true;
        Theme high = ThemeManager.Current;

        Assert.AreNotSame(normal, high);
        Assert.AreSame(high, ThemeManager.Current, "la variante se reutiliza");
        GameSettings.HighContrast = false;
        Assert.AreSame(normal, ThemeManager.Current);
    }

    [Test]
    public void AltoContrasteSubeElContrasteDeTodosLosTextos()
    {
        GameSettings.UseStore(new MemoryStore());
        Theme n = ThemeManager.Current;
        Theme h = ThemeManager.HighContrastOf(n);

        Assert.Greater(Contrast(h.textPrimary, h.panel), Contrast(n.textPrimary, n.panel));
        Assert.Greater(Contrast(h.textSecondary, h.panel), Contrast(n.textSecondary, n.panel));
        Assert.GreaterOrEqual(Contrast(h.textSecondary, h.panel), 7f, "AAA para el texto secundario");
        Assert.GreaterOrEqual(Contrast(h.textPrimary, h.playerBubble), 7f);
        Assert.GreaterOrEqual(Contrast(h.textPrimary, h.suspectBubble), 7f);
        Assert.AreEqual(0f, h.grainIntensity);
    }
}
