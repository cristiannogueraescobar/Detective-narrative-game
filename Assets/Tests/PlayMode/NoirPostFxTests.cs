using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Post-proceso noir (C4): gradación y bloom sutil de URP sobre la interfaz, con el ajuste "Filtro noir" y un
/// interruptor del tema que devuelven el lienzo superpuesto de siempre.
/// </summary>
public class NoirPostFxTests
{
    private class MemoryStore : ISettingsStore
    {
        private readonly Dictionary<string, float> values = new Dictionary<string, float>();
        public float Get(string key, float fallback) => values.TryGetValue(key, out float v) ? v : fallback;
        public void Set(string key, float value) => values[key] = value;
    }

    private Theme theme;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SaveSystem.DirectoryOverride = Path.Combine(Path.GetTempPath(), "detective-post-" + System.Guid.NewGuid().ToString("N"));
        GameSettings.UseStore(new MemoryStore());
        theme = Object.Instantiate(ThemeManager.Current);
        ThemeManager.Override(theme);
        yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        ThemeManager.Override(null);
        GameSettings.UseStore(null);
        SaveSystem.DirectoryOverride = null;
        yield return null;
    }

    // WCAG 2.x (colores en gamma sRGB)
    private static float Ratio(Color a, Color b)
    {
        float la = Luminance(a), lb = Luminance(b);
        return (Mathf.Max(la, lb) + 0.05f) / (Mathf.Min(la, lb) + 0.05f);
    }

    private static float Luminance(Color c)
    {
        float L(float v) => v <= 0.04045f ? v / 12.92f : Mathf.Pow((v + 0.055f) / 1.055f, 2.4f);
        return 0.2126f * L(c.r) + 0.7152f * L(c.g) + 0.0722f * L(c.b);
    }

    private static List<Canvas> RootCanvases()
    {
        return Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.isRootCanvas).ToList();
    }

    [UnityTest]
    public IEnumerator ConFiltroLaInterfazPasaPorLaCamaraConGradacionYBloom()
    {
        Assert.IsTrue(GameSettings.NoirFilter);

        Camera camera = NoirPostFx.TargetCamera;
        Assert.IsNotNull(camera);
        Assert.IsTrue(camera.GetUniversalAdditionalCameraData().renderPostProcessing);
        foreach (Canvas canvas in RootCanvases())
        {
            Assert.AreEqual(RenderMode.ScreenSpaceCamera, canvas.renderMode, canvas.name);
            Assert.AreEqual(camera, canvas.worldCamera, canvas.name);
        }

        Volume volume = NoirPostFx.Volume;
        Assert.IsNotNull(volume);
        Assert.IsTrue(volume.isGlobal && volume.enabled);
        Assert.IsTrue(volume.profile.TryGet(out Bloom bloom) && bloom.active);
        Assert.IsTrue(volume.profile.TryGet(out ColorAdjustments grading) && grading.active);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ElBloomNoAlcanzaAlTextoNiAlPapel()
    {
        NoirPostFx.Volume.profile.TryGet(out Bloom bloom);
        float brightest = new[] { theme.textPrimary, theme.paper, theme.accent, theme.buttonPrimary }.Max(c => c.maxColorComponent);

        Assert.Greater(bloom.threshold.value, brightest, "el texto no debe brillar");
        yield return null;
    }

    [UnityTest]
    public IEnumerator SinFiltroVuelveElLienzoSuperpuesto()
    {
        GameSettings.NoirFilter = false;
        yield return null;

        Assert.IsFalse(NoirPostFx.TargetCamera.GetUniversalAdditionalCameraData().renderPostProcessing);
        Assert.IsTrue(RootCanvases().All(c => c.renderMode == RenderMode.ScreenSpaceOverlay));
        Assert.IsFalse(NoirPostFx.Volume != null && NoirPostFx.Volume.enabled);

        GameSettings.NoirFilter = true;
        yield return null;
        Assert.IsTrue(RootCanvases().All(c => c.renderMode == RenderMode.ScreenSpaceCamera));
    }

    [UnityTest]
    public IEnumerator ElTemaLoDesactiva()
    {
        theme.postFx = false;
        NoirPostFx.Refresh();
        yield return null;

        Assert.IsTrue(RootCanvases().All(c => c.renderMode == RenderMode.ScreenSpaceOverlay));
        Assert.IsFalse(NoirPostFx.TargetCamera.GetUniversalAdditionalCameraData().renderPostProcessing);
    }

    // Contraste WCAG medido DESPUÉS del post-proceso: muestras de color del tema pasadas por la cámara real
    [UnityTest]
    public IEnumerator ElContrasteSigueEnAADespuesDelPostProceso()
    {
        // Necesita GPU: con -nographics no hay nada que leer (.superpowers/capture-anim.sh NoirPostFxTests)
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            Assert.Ignore("sin gráficos (-nographics)");
        var pairs = new (string name, Color text, Color back)[]
        {
            ("texto/fondo", theme.textPrimary, theme.background),
            ("texto/panel", theme.textPrimary, theme.panel),
            ("secundario/panel", theme.textSecondary, theme.panel),
            ("sistema/panel", theme.systemText, theme.panel),
            ("acusar/botón", theme.dangerOnButton, theme.buttonSecondary),
            ("botón principal", theme.buttonPrimaryText, theme.buttonPrimary),
            ("botón secundario", theme.buttonSecondaryText, theme.buttonSecondary),
            ("jugador/burbuja", theme.textPrimary, theme.playerBubble),
            ("sospechoso/burbuja", theme.textPrimary, theme.suspectBubble),
            ("papel", theme.paperText, theme.paper),
        };

        const int size = 64;
        var target = new RenderTexture(size * pairs.Length * 2, size, 24);
        Camera camera = new GameObject("Muestras", typeof(Camera)).GetComponent<Camera>();
        camera.targetTexture = target;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
        var canvas = new GameObject("Muestras", typeof(Canvas)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 5f;
        canvas.sortingOrder = 999;
        for (int i = 0; i < pairs.Length * 2; i++)
        {
            var swatch = new GameObject("m" + i, typeof(UnityEngine.UI.Image)).GetComponent<UnityEngine.UI.Image>();
            swatch.transform.SetParent(canvas.transform, false);
            swatch.rectTransform.anchorMin = new Vector2(i / (pairs.Length * 2f), 0f);
            swatch.rectTransform.anchorMax = new Vector2((i + 1) / (pairs.Length * 2f), 1f);
            swatch.rectTransform.offsetMin = swatch.rectTransform.offsetMax = Vector2.zero;
            swatch.color = i % 2 == 0 ? pairs[i / 2].text : pairs[i / 2].back;
        }
        yield return null;
        Canvas.ForceUpdateCanvases();
        camera.Render();

        RenderTexture.active = target;
        var read = new Texture2D(target.width, size, TextureFormat.RGBA32, false);
        read.ReadPixels(new Rect(0, 0, target.width, size), 0, 0);
        read.Apply();
        RenderTexture.active = null;

        var failures = new List<string>();
        for (int p = 0; p < pairs.Length; p++)
        {
            Color text = read.GetPixel(p * 2 * size + size / 2, size / 2);
            Color back = read.GetPixel((p * 2 + 1) * size + size / 2, size / 2);
            float before = Ratio(pairs[p].text, pairs[p].back);
            float after = Ratio(text, back);
            Debug.Log($"[Post] {pairs[p].name}: {before:F2} → {after:F2} ({text} / {back})");
            if (after < 4.5f)
                failures.Add($"{pairs[p].name} {after:F2}:1");
        }

        Object.Destroy(read);
        Object.Destroy(canvas.gameObject);
        Object.Destroy(camera.gameObject);
        target.Release();
        Assert.IsEmpty(failures, string.Join(", ", failures));
    }

    // Revisión D3 n.º 5: recargar la escena no deja perfiles de post-proceso colgados
    [UnityTest]
    public IEnumerator RecargarLaEscenaNoAcumulaPerfiles()
    {
        int before = Resources.FindObjectsOfTypeAll<VolumeProfile>().Length;
        for (int i = 0; i < 3; i++)
        {
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            yield return null;
        }
        Assert.LessOrEqual(Resources.FindObjectsOfTypeAll<VolumeProfile>().Length, before);
    }
}
