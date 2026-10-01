using NUnit.Framework;
using UnityEngine;

public class AmbienceTests
{
    [Test]
    public void ElParpadeoDeLaLamparaEsSutilYAcotado()
    {
        float min = float.MaxValue, max = float.MinValue;
        for (float t = 0f; t < 120f; t += 0.02f)
        {
            float v = Ambience.LampFlicker(t, 3.7f);
            min = Mathf.Min(min, v);
            max = Mathf.Max(max, v);
        }

        Assert.LessOrEqual(max, 1.06f, "nunca brilla de más");
        Assert.GreaterOrEqual(min, 0.5f, "nunca se apaga del todo");
        Assert.Less(min, 0.85f, "en dos minutos hay algún parpadeo visible");
    }

    [Test]
    public void LaMayorParteDelTiempoLaLuzEstaFirme()
    {
        int steady = 0, total = 0;
        for (float t = 0f; t < 120f; t += 0.05f, total++)
        {
            if (Ambience.LampFlicker(t, 1.3f) > 0.9f)
                steady++;
        }
        Assert.Greater((float)steady / total, 0.9f);
    }

    [Test]
    public void PosicionesEnLaImagenSeConviertenEnAnclas()
    {
        // Coordenadas de la imagen con el origen arriba (como en un editor de imagen) → anclas de Unity (origen abajo)
        Vector2 anchor = Ambience.ImageToAnchor(new Vector2(0.2f, 0.3f));
        Assert.AreEqual(0.2f, anchor.x, 1e-5f);
        Assert.AreEqual(0.7f, anchor.y, 1e-5f);
    }

    [Test]
    public void ElPolvoBrillaMasCercaDeLaLuz()
    {
        Vector2 light = new Vector2(0.2f, 0.35f);
        float near = Ambience.DustVisibility(new Vector2(0.21f, 0.37f), light, 0.25f);
        float far = Ambience.DustVisibility(new Vector2(0.6f, 0.8f), light, 0.25f);
        Assert.Greater(near, 0.8f);
        Assert.AreEqual(0f, far, 1e-5f);
    }

    [Test]
    public void LaCoberturaNoDeformaLaImagen()
    {
        // Imagen 2:3 en una pantalla 9:16: se recorta de ancho, nunca se estira
        Vector2 size = Ambience.CoverSize(new Vector2(1024, 1536), new Vector2(1080, 1920));
        Assert.AreEqual(1024f / 1536f, size.x / size.y, 1e-4f);
        Assert.GreaterOrEqual(size.x, 1080f - 0.01f);
        Assert.GreaterOrEqual(size.y, 1920f - 0.01f);
    }
}

public class TitleIntroTests
{
    [Test]
    public void EmpiezaInvisibleYTerminaEntero()
    {
        Assert.AreEqual(0f, TitleIntro.Alpha(0f), 1e-4f);
        Assert.AreEqual(1f, TitleIntro.Alpha(1f), 1e-4f);
        Assert.AreEqual(1f, TitleIntro.Alpha(3f), 1e-4f);
    }

    [Test]
    public void SeEnciendeConUnParpadeoComoUnRotulo()
    {
        // En algún momento de la entrada, la opacidad baja tras haber subido (el parpadeo)
        bool dipped = false;
        float peak = 0f;
        for (float t = 0f; t <= 1f; t += 0.01f)
        {
            float a = TitleIntro.Alpha(t);
            peak = Mathf.Max(peak, a);
            if (peak > 0.6f && a < peak - 0.3f)
                dipped = true;
        }
        Assert.IsTrue(dipped);
    }

    [Test]
    public void ElEspaciadoSeCierraAlFinal()
    {
        Assert.Greater(TitleIntro.Spacing(0f, 10f), TitleIntro.Spacing(1f, 10f));
        Assert.AreEqual(10f, TitleIntro.Spacing(1f, 10f), 1e-4f);
    }
}
