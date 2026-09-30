using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// CAPTURA DE ANIMACIONES (no es un test: [Explicit], se lanza a mano)
/// Carga el juego, dispara un efecto y guarda fotogramas en momentos concretos para revisarlos como imágenes.
///   Unity -batchmode -projectPath . -runTests -testPlatform PlayMode -testFilter AnimationCapture
/// (sin -nographics). Salida: docs/screenshots/&lt;fecha&gt;/anim/&lt;efecto&gt;_&lt;ms&gt;.png
/// </summary>
[Explicit, Category("Capturas")]
public class AnimationCapture
{
    private const int Width = 540;
    private const int Height = 960;

    private Camera camera;
    private RenderTexture target;
    private string folder;

    private class MemoryStore : ISettingsStore
    {
        private readonly Dictionary<string, float> values = new Dictionary<string, float>();
        public float Get(string key, float fallback) => values.TryGetValue(key, out float v) ? v : fallback;
        public void Set(string key, float value) => values[key] = value;
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        GameSettings.UseStore(new MemoryStore());
        SaveSystem.DirectoryOverride = Path.Combine(Path.GetTempPath(), "detective-capture-" + Guid.NewGuid().ToString("N"));
        folder = Path.Combine("docs", "screenshots", DateTime.Now.ToString("yyyy-MM-dd"), "anim");
        Directory.CreateDirectory(folder);

        yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
        yield return null;

        // El lienzo pasa a dibujarse con una cámara sobre una textura del tamaño de un móvil
        target = new RenderTexture(Width, Height, 24);
        camera = new GameObject("Captura", typeof(Camera)).GetComponent<Camera>();
        camera.targetTexture = target;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.magenta;
        foreach (Canvas canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.isRootCanvas))
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 10f;
        }
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        GameSettings.UseStore(null);
        SaveSystem.DirectoryOverride = null;
        if (camera != null)
            camera.targetTexture = null;
        if (target != null)
            target.Release();
        yield return null;
    }

    private void Shot(string name)
    {
        Canvas.ForceUpdateCanvases();
        camera.Render();
        RenderTexture.active = target;
        var image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
        image.Apply();
        RenderTexture.active = null;
        File.WriteAllBytes(Path.Combine(folder, name + ".png"), image.EncodeToPNG());
        UnityEngine.Object.Destroy(image);
    }

    // Guarda fotogramas en los instantes pedidos (s desde ahora)
    private IEnumerator Frames(string name, params float[] times)
    {
        float start = Time.realtimeSinceStartup;
        foreach (float t in times)
        {
            while (Time.realtimeSinceStartup - start < t)
                yield return null;
            Shot($"{name}_{Mathf.RoundToInt(t * 1000):0000}");
        }
    }

    private static GameObject Find(string name)
    {
        return Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == name && g.scene.IsValid());
    }

    private static IEnumerator Click(string name)
    {
        Find(name).GetComponent<Button>().onClick.Invoke();
        yield return null;
        yield return null;
    }

    private static IEnumerator ToInterrogation()
    {
        yield return Click("PlayButton");
        yield return Click("StartButton");
        yield return new WaitForSecondsRealtime(0.5f);
    }

    [UnityTest]
    public IEnumerator Menu()
    {
        yield return Frames("menu", 0.1f, 0.6f, 1.2f, 2.5f);
    }

    [UnityTest]
    public IEnumerator Emociones()
    {
        yield return ToInterrogation();
        var ui = UnityEngine.Object.FindFirstObjectByType<InterrogationUI>();
        string id = ui.CurrentSuspectId;

        foreach (Emotion e in new[] { Emotion.Nervioso, Emotion.Asustado, Emotion.Enfadado, Emotion.Triste, Emotion.Tranquilo })
        {
            ui.SetEmotion(id, e);
            yield return Frames("emocion_" + e.ToString().ToLowerInvariant(), 0.05f, 0.2f, 0.45f, 1.5f);
        }
    }
}
