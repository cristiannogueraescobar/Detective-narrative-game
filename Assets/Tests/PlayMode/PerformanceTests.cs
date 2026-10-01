using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// RENDIMIENTO (Play Mode, editor): memoria que se asigna por fotograma (basura para el recolector, lo que da
/// tirones en móvil) con el menú vivo y con el interrogatorio quieto. Deja las cifras en Logs/rendimiento.md.
/// </summary>
public class PerformanceTests
{
    private class MemoryStore : ISettingsStore
    {
        private readonly Dictionary<string, float> values = new Dictionary<string, float>();
        public float Get(string key, float fallback) => values.TryGetValue(key, out float v) ? v : fallback;
        public void Set(string key, float value) => values[key] = value;
    }

    // Un poco de margen para el propio editor y el test
    private const long MaxBytesPerFrame = 2048;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        GameSettings.UseStore(new MemoryStore());
        Tutorial.SkipAll();
        SaveSystem.DirectoryOverride = Path.Combine(Path.GetTempPath(), "detective-perf-" + System.Guid.NewGuid().ToString("N"));
        yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
        yield return new WaitForSecondsRealtime(3f); // Entradas y primeras animaciones fuera
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        GameSettings.UseStore(null);
        SaveSystem.Flush(); // Una escritura pendiente podría volver a crear la carpeta o bloquear el archivo
        SaveSystem.DirectoryOverride = null;
        yield return null;
    }

    private static IEnumerator Measure(string label, int frames, List<string> report, System.Action<long> check)
    {
        using (var gc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", frames))
        {
            float start = Time.realtimeSinceStartup;
            for (int i = 0; i < frames; i++)
                yield return null;
            float ms = (Time.realtimeSinceStartup - start) * 1000f / frames;

            var samples = new List<long>();
            for (int i = 0; i < gc.Count; i++)
                samples.Add(gc.GetSample(i).Value);
            samples.Sort();
            long median = samples.Count > 0 ? samples[samples.Count / 2] : -1;
            report.Add($"| {label} | {median} B | {(samples.Count > 0 ? samples.Last() : -1)} B | {ms:F1} ms |");
            check(median);
        }
    }

    [UnityTest]
    public IEnumerator PocaBasuraPorFotograma()
    {
        var report = new List<string> { "# Rendimiento (Play Mode en el editor)", "", "| Escena | Basura por fotograma (mediana) | Máximo | Tiempo por fotograma |", "|---|---|---|---|" };
        long menu = 0, interrogation = 0, baseline = 0;

        // Línea base: lo que asignan el editor y el propio test sin el juego (escena vacía)
        Scene game = SceneManager.GetActiveScene();
        Scene empty = SceneManager.CreateScene("Vacia");
        SceneManager.SetActiveScene(empty);
        foreach (GameObject root in game.GetRootGameObjects())
            root.SetActive(false);
        yield return null;
        yield return Measure("Línea base (escena vacía: editor + test)", 180, report, v => baseline = v);
        SceneManager.SetActiveScene(game);
        foreach (GameObject root in game.GetRootGameObjects())
            root.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);

        yield return Measure("Menú con ambiente (polvo, lluvia, vapor, parpadeo, grano)", 180, report, v => menu = v);

        Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "PlayButton" && g.scene.IsValid()).GetComponent<Button>().onClick.Invoke();
        yield return null;
        Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "Caso al azar" && g.scene.IsValid()).GetComponent<Button>().onClick.Invoke();
        yield return null;
        Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "StartButton" && g.scene.IsValid()).GetComponent<Button>().onClick.Invoke();
        yield return new WaitForSecondsRealtime(1f);
        var ui = Object.FindFirstObjectByType<InterrogationUI>();
        ui.SetEmotion(ui.CurrentSuspectId, Emotion.Nervioso); // Temblor y sudor en marcha
        yield return new WaitForSecondsRealtime(0.5f);
        yield return Measure("Interrogatorio (retrato nervioso, grano)", 180, report, v => interrogation = v);

        report.Add("");
        report.Add($"Del juego (restando la línea base): menú {menu - baseline} B, interrogatorio {interrogation - baseline} B por fotograma.");
        Directory.CreateDirectory("Logs");
        File.WriteAllLines("Logs/rendimiento.md", report);
        Debug.Log(string.Join("\n", report));

        Assert.LessOrEqual(menu - baseline, MaxBytesPerFrame, "el menú no debe generar basura en cada fotograma");
        Assert.LessOrEqual(interrogation - baseline, MaxBytesPerFrame, "el interrogatorio quieto no debe generar basura en cada fotograma");
    }
}
