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
    // -captureHeight 1200 = móvil alargado (1080x2400 a media resolución); por defecto 1080x1920
    // -captureWidth 720 -captureHeight 1600 = 1440x3200 a media resolución. Sin el ancho, una altura distinta daba
    // proporciones que no existen (540x1920 = 9:32; sesión C: las capturas de retratos salieron así por error)
    // -captureWidth 1080 -captureHeight 1920 = píxeles reales de un móvil (filtro de importación, BRIEF 6.4)
    private static readonly int Width = Argument("-captureWidth", 540);
    private static readonly int Height = Argument("-captureHeight", 960);
    // Media resolución: el nombre lleva el tamaño de pantalla (×2). Con 1080 o más de ancho ya son píxeles reales
    private static string Suffix => Height == 960 && Width == 540 ? ""
        : Width >= 1080 ? $"_{Width}x{Height}" : $"_{Width * 2}x{Height * 2}";

    private static int Argument(string name, int fallback)
    {
        string[] args = Environment.GetCommandLineArgs();
        int i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length && int.TryParse(args[i + 1], out int v) ? v : fallback;
    }

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
        RedirectCanvases();
        yield return null;
    }

    private void RedirectCanvases()
    {
        // La cámara de captura se crea aquí (y otra vez si una recarga de escena la destruye)
        if (camera == null)
        {
            camera = new GameObject("Captura", typeof(Camera)).GetComponent<Camera>();
            camera.targetTexture = target;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.magenta;
            // El post-proceso noir (NoirPostFx) también en la captura: se ve lo que ve el jugador
            UnityEngine.Rendering.Universal.CameraExtensions.GetUniversalAdditionalCameraData(camera).renderPostProcessing = NoirPostFx.Enabled;
        }
        foreach (Canvas canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.isRootCanvas))
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 10f;
        }
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        GameSettings.UseStore(null);
        SaveSystem.Flush(); // Una escritura pendiente podría volver a crear la carpeta o bloquear el archivo
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
        File.WriteAllBytes(Path.Combine(folder, name + Suffix + ".png"), image.EncodeToPNG());
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
        yield return Click("Caso al azar");
        yield return Click("StartButton");
        yield return new WaitForSecondsRealtime(0.5f);
    }

    [UnityTest]
    public IEnumerator Menu()
    {
        yield return Frames("menu", 0.1f, 0.6f, 1.2f, 2.5f);
    }

    // D2: coste en GPU del relieve de los retratos (C3) y del post-proceso (C4), a 1080x1920 en el interrogatorio.
    // Tiempo por fotograma de cámara, sincronizando con la GPU (lectura de 1 píxel) cada 10. Deja Logs/coste-visual.md.
    [UnityTest]
    public IEnumerator CosteVisual()
    {
        yield return ToInterrogation();
        var ui = UnityEngine.Object.FindFirstObjectByType<InterrogationUI>();
        Theme theme = UnityEngine.Object.Instantiate(ThemeManager.Current);
        ThemeManager.Override(theme);

        var full = new RenderTexture(1080, 1920, 24);
        camera.targetTexture = full;
        var pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        var data = UnityEngine.Rendering.Universal.CameraExtensions.GetUniversalAdditionalCameraData(camera);
        var report = new List<string> { "# Coste visual (GPU del editor, 1080x1920, interrogatorio)", "",
            "| Relieve (C3) | Post-proceso (C4) | ms por fotograma | vs. todo apagado |", "|---|---|---|---|" };
        var results = new Dictionary<string, double>();

        foreach (bool post in new[] { false, true })
        foreach (bool lit in new[] { false, true })
        {
            theme.portraitLit = lit;
            theme.postFx = post;
            NoirPostFx.Refresh();
            data.renderPostProcessing = post;
            ui.SetEmotion(ui.CurrentSuspectId, Emotion.Nervioso); // Vuelve a poner el retrato con el material del tema
            yield return null;

            for (int i = 0; i < 20; i++) // Calentar
                camera.Render();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            const int frames = 300;
            for (int i = 0; i < frames; i++)
            {
                camera.Render();
                if (i % 10 == 9)
                {
                    RenderTexture.active = full;
                    pixel.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
                    pixel.Apply();
                    RenderTexture.active = null;
                }
            }
            results[$"{lit}|{post}"] = watch.Elapsed.TotalMilliseconds / frames;
            yield return null;
        }

        double off = results["False|False"];
        foreach (bool post in new[] { false, true })
        foreach (bool lit in new[] { false, true })
        {
            double ms = results[$"{lit}|{post}"];
            report.Add($"| {(lit ? "sí" : "no")} | {(post ? "sí" : "no")} | {ms:F2} | {(ms - off >= 0 ? "+" : "")}{ms - off:F2} ms |");
        }
        report.Add("");
        report.Add($"GPU: {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsDeviceType}).");
        File.WriteAllLines(Path.Combine("Logs", "coste-visual.md"), report);

        camera.targetTexture = target;
        full.Release();
        UnityEngine.Object.Destroy(pixel);
        ThemeManager.Override(null);
    }

    // El menú de quien vuelve: "Continuar · caso, día N" y "Caso nuevo"
    [UnityTest]
    public IEnumerator MenuConPartidaGuardada()
    {
        yield return ToInterrogation();
        UnityEngine.Object.FindFirstObjectByType<GameManager>().EndDay(); // Guarda
        yield return new WaitForSecondsRealtime(1f);
        yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
        yield return null;
        RedirectCanvases();
        yield return new WaitForSecondsRealtime(2.5f);
        Shot("menu_continuar");
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

    [UnityTest]
    public IEnumerator Intro()
    {
        yield return Click("PlayButton");
        yield return Click("Caso al azar");
        yield return Frames("intro", 0.2f, 1.5f, 4f, 8f);
    }

    // Fondo de cada historia (C5) detrás del expediente
    [UnityTest]
    public IEnumerator IntrosPorHistoria()
    {
        foreach (string id in new[] { "1", "2", "3" })
        {
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            yield return null;
            RedirectCanvases();
            yield return Click("PlayButton");
            yield return Click("Caso " + id);
            yield return new WaitForSecondsRealtime(6f);
            Shot("intro_historia" + id);
        }
    }

    [UnityTest]
    public IEnumerator Pista()
    {
        yield return ToInterrogation();
        var ui = UnityEngine.Object.FindFirstObjectByType<InterrogationUI>();
        ui.ShowClueNotification("La taza de la mesilla");
        ui.ShowClueNotification("Lo que vio la ventana");
        yield return Frames("pista", 0.1f, 0.3f, 0.6f, 0.9f, 1.8f, 2.6f, 2.9f, 4.5f);
    }

    [UnityTest]
    public IEnumerator Contradiccion()
    {
        yield return ToInterrogation();
        var ui = UnityEngine.Object.FindFirstObjectByType<InterrogationUI>();
        ui.ShowContradictionNotification("La versión de Daniel («subí a las once») choca con: Lo que vio la ventana");
        yield return Frames("contradiccion", 0.05f, 0.12f, 0.2f, 0.35f, 0.8f, 1.9f);
    }

    [UnityTest]
    public IEnumerator NuevoDia()
    {
        Tutorial.SkipAll();
        yield return ToInterrogation();
        // Con una pista del día: la tarjeta lo reconoce ("Ayer: una pista nueva.")
        var talk = UnityEngine.Object.FindFirstObjectByType<AIConversationManager>();
        talk.State.Discover(talk.State.Variant.clues[0].id);
        UnityEngine.Object.FindFirstObjectByType<GameManager>().EndDay();
        yield return Frames("dia",0.1f, 0.5f, 0.75f, 1.0f, 2.5f, 6f);
    }

    [UnityTest]
    public IEnumerator Acusacion()
    {
        yield return ToInterrogation();
        UnityEngine.Object.FindFirstObjectByType<GameManager>().ForceAccusationPanel();
        yield return Frames("acusacion", 0.1f, 1.5f, 4f);
        UnityEngine.Object.FindFirstObjectByType<InterrogationUI>().OnAccuseClick();
        yield return Frames("veredicto", 0.3f, 1.2f, 2.9f, 3.4f, 5f, 8f);
        // Informe completo (un toque lo termina) y la ficha policial del culpable al final
        var reveal = UnityEngine.Object.FindFirstObjectByType<StepReveal>();
        reveal?.OnPointerClick(null);
        yield return new WaitForSecondsRealtime(1f);
        var scroll = reveal != null ? reveal.GetComponentInParent<ScrollRect>() : null;
        if (scroll != null)
        {
            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = 0f; // Al final del informe
        }
        yield return null;
        Shot("veredicto_ficha");
    }

    // Arte nuevo (docs/art/javier/BRIEF.md 6.4): qué filtro de importación se ve más nítido sin dientes de sierra a
    // tamaño de móvil. A/B en tiempo de ejecución sobre los retratos actuales (sin tocar sus .meta): la textura tal cual
    // (bilineal con mipmaps), una copia del nivel 0 sin mipmaps con filtro Point y la misma copia en bilineal.
    // filtro_<pantalla>_<variante>; lanzar con capture-anim.sh "AnimationCapture.FiltroDeImportacion" 1920
    [UnityTest]
    public IEnumerator FiltroDeImportacion()
    {
        Tutorial.SkipAll();
        yield return Click("PlayButton");
        yield return Click("Caso 1");
        yield return Click("StartButton");
        yield return new WaitForSecondsRealtime(1.5f);
        yield return FilterShots("filtro_interrogatorio");
        UnityEngine.Object.FindFirstObjectByType<GameManager>().ForceAccusationPanel();
        yield return new WaitForSecondsRealtime(1.5f);
        yield return FilterShots("filtro_rueda");
    }

    private IEnumerator FilterShots(string name)
    {
        var originals = new Dictionary<RawImage, Texture2D>();
        foreach (RawImage raw in UnityEngine.Object.FindObjectsByType<RawImage>(FindObjectsSortMode.None))
            if (raw.isActiveAndEnabled && raw.texture is Texture2D t && t.mipmapCount > 1 && t.width >= 512)
                originals[raw] = t;
        Shot(name + "_bilineal_mipmaps");

        // Las tres variantes salen de la misma copia sin comprimir (CopyTexture falla con anchos que no son múltiplo
        // de 4 en formatos comprimidos): así la compresión no sesga la comparación
        var withMips = new Dictionary<Texture2D, Texture2D>();
        var noMips = new Dictionary<Texture2D, Texture2D>();
        foreach (Texture2D source in originals.Values.Distinct())
        {
            withMips[source] = Uncompressed(source, mipmaps: true);
            noMips[source] = Uncompressed(source, mipmaps: false);
        }
        foreach (var (variant, set, filter) in new[]
                 {
                     ("_copia_bilineal_mipmaps", withMips, FilterMode.Bilinear),
                     ("_point_sin_mipmaps", noMips, FilterMode.Point),
                     ("_bilineal_sin_mipmaps", noMips, FilterMode.Bilinear),
                 })
        {
            foreach (var pair in originals)
            {
                set[pair.Value].filterMode = filter;
                pair.Key.texture = set[pair.Value];
            }
            yield return null;
            Shot(name + variant);
        }

        foreach (var pair in originals)
            pair.Key.texture = pair.Value;
        foreach (Texture2D copy in withMips.Values.Concat(noMips.Values))
            UnityEngine.Object.Destroy(copy);
        Debug.Log($"[Filtro] {name}: {originals.Count} retratos comparados");
    }

    private static Texture2D Uncompressed(Texture2D source, bool mipmaps)
    {
        RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(source, rt);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        var copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, mipmaps);
        copy.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, mipmaps);
        copy.Apply(updateMipmaps: mipmaps);
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        return copy;
    }

    // Sesión A, bloque 4: entrada del sospechoso nuevo (fundido y deslizamiento de la figura y del busto)
    [UnityTest]
    public IEnumerator CambioDeSospechoso()
    {
        Tutorial.SkipAll();
        yield return ToInterrogation();
        var dropdown = Find("SuspectDropdown").GetComponent<TMPro.TMP_Dropdown>();
        int next = (dropdown.value + 1) % dropdown.options.Count;
        dropdown.value = next;
        dropdown.onValueChanged.Invoke(next);
        yield return Frames("cambio", 0.02f, 0.12f, 0.25f, 0.6f);
    }

    // Sesión A, bloque 4: galería antes/después con la MISMA pantalla, la MISMA historia (Caso 1) y los mismos
    // instantes. "Antes" = los ajustes del tema de la Sesión A apagados (escena, viñeta, rueda en una fila): así se
    // comprueba también que cada cambio se puede desactivar. ad_antes_* / ad_despues_*
    [UnityTest]
    public IEnumerator AntesDespues()
    {
        foreach (bool after in new[] { false, true })
        {
            Theme theme = UnityEngine.Object.Instantiate(ThemeManager.Current);
            theme.interrogationStage = after;
            theme.tensionVignette = after;
            theme.lineupSingleRow = after;
            ThemeManager.Override(theme);
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            yield return null;
            RedirectCanvases();
            Tutorial.SkipAll();
            yield return Click("PlayButton");
            yield return Click("Caso 1");
            yield return Click("StartButton");
            yield return new WaitForSecondsRealtime(1f);

            string tag = after ? "despues" : "antes";
            var ui = UnityEngine.Object.FindFirstObjectByType<InterrogationUI>();
            Shot($"ad_{tag}_interrogatorio");
            ui.SetEmotion(ui.CurrentSuspectId, Emotion.Enfadado);
            yield return new WaitForSecondsRealtime(1.5f);
            Shot($"ad_{tag}_enfado");
            ui.SetEmotion(ui.CurrentSuspectId, Emotion.Tranquilo);
            yield return new WaitForSecondsRealtime(1f);
            ui.ShowClueNotification("La taza de la mesilla");
            yield return new WaitForSecondsRealtime(0.3f);
            Shot($"ad_{tag}_pista");
            yield return new WaitForSecondsRealtime(3.5f);
            UnityEngine.Object.FindFirstObjectByType<GameManager>().ForceAccusationPanel();
            yield return new WaitForSecondsRealtime(1.5f);
            Shot($"ad_{tag}_rueda");
        }
        ThemeManager.Override(null);
    }

    // Sesión A, bloque 3: los 12 personajes de las tres historias en el interrogatorio, a tamaño real (retrato_<id>)
    [UnityTest]
    public IEnumerator TodosLosRetratos()
    {
        Tutorial.SkipAll();
        yield return ToInterrogation();
        var ui = UnityEngine.Object.FindFirstObjectByType<InterrogationUI>();
        var dropdown = Find("SuspectDropdown").GetComponent<TMPro.TMP_Dropdown>();
        // Historia a historia: los ids ("padre", "vecina"…) se repiten entre historias, como en el juego (una cada vez)
        foreach (StoryData story in CaseLibrary.Stories)
        {
            var views = story.cast.Select(c => SuspectView.From(c)).ToList();
            ui.SetSuspects(views);
            for (int i = 0; i < views.Count; i++)
            {
                dropdown.value = i;
                dropdown.onValueChanged.Invoke(i); // Aunque el índice coincida con el anterior
                yield return new WaitForSecondsRealtime(0.6f); // Fundido del cambio de retrato
                Shot("retrato_" + views[i].artId);
            }
        }
    }

    // Sesión C, arreglos de interfaz: siempre la misma historia (la 2, con cuatro sospechosos: el caso más apretado de la
    // rueda) para comparar antes y después a cada tamaño de pantalla. interfaz_interrogatorio, interfaz_acusacion
    [UnityTest]
    public IEnumerator InterfazHistoria2()
    {
        Tutorial.SkipAll();
        StoryData story = CaseLibrary.Stories[1];
        yield return Click("PlayButton");
        yield return Click("Caso " + story.id);
        yield return Click("StartButton");
        yield return new WaitForSecondsRealtime(1f);
        Shot("interfaz_interrogatorio");
        UnityEngine.Object.FindFirstObjectByType<GameManager>().ForceAccusationPanel();
        yield return new WaitForSecondsRealtime(1.5f);
        Shot("interfaz_acusacion");
    }

    // La opción B de la composición (solo la figura, sin busto), en la misma pantalla: interfaz_figura_interrogatorio
    [UnityTest]
    public IEnumerator InterfazHistoria2Figura()
    {
        Theme theme = UnityEngine.Object.Instantiate(ThemeManager.Current);
        theme.hideFlags = HideFlags.HideAndDontSave;
        theme.interrogationComposition = PortraitComposition.Figura;
        ThemeManager.Override(theme);
        try
        {
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            yield return null;
            RedirectCanvases();
            Tutorial.SkipAll();
            yield return Click("PlayButton");
            yield return Click("Caso " + CaseLibrary.Stories[1].id);
            yield return Click("StartButton");
            yield return new WaitForSecondsRealtime(1f);
            Shot("interfaz_figura_interrogatorio");
        }
        finally
        {
            ThemeManager.Override(null);
        }
    }

    // Sesión C, retratos completos: cada personaje de la historia con sus tres expresiones en el interrogatorio, y la
    // rueda de acusación. retratos_<historia>_<artId>_<estado> y acusacion_<historia>
    [UnityTest] public IEnumerator RetratosHistoria1() { yield return RetratosDe(0); }
    [UnityTest] public IEnumerator RetratosHistoria2() { yield return RetratosDe(1); }
    [UnityTest] public IEnumerator RetratosHistoria3() { yield return RetratosDe(2); }

    private IEnumerator RetratosDe(int index)
    {
        Tutorial.SkipAll();
        StoryData story = CaseLibrary.Stories[index];
        yield return Click("PlayButton");
        yield return Click("Caso " + story.id);
        yield return Click("StartButton");
        yield return new WaitForSecondsRealtime(0.5f);
        var ui = UnityEngine.Object.FindFirstObjectByType<InterrogationUI>();
        var dropdown = Find("SuspectDropdown").GetComponent<TMPro.TMP_Dropdown>();
        var views = story.cast.Select(c => SuspectView.From(c)).ToList();
        ui.SetSuspects(views);
        for (int i = 0; i < views.Count; i++)
        {
            dropdown.value = i;
            dropdown.onValueChanged.Invoke(i);
            yield return new WaitForSecondsRealtime(0.6f);
            foreach (Emotion e in new[] { Emotion.Tranquilo, Emotion.Triste, Emotion.Nervioso, Emotion.Enfadado })
            {
                if (e != Emotion.Tranquilo && ArtLibrary.Load($"{LegacyExpressions.Folder}/{views[i].artId}_{e.ToString().ToLowerInvariant()}.png") == null)
                    continue; // Solo las que tiene
                ui.SetEmotion(views[i].id, e);
                yield return new WaitForSecondsRealtime(0.6f);
                Shot($"retratos_{index + 1}_{views[i].artId}_{e.ToString().ToLowerInvariant()}");
            }
        }
        UnityEngine.Object.FindFirstObjectByType<GameManager>().ForceAccusationPanel();
        yield return new WaitForSecondsRealtime(1.5f);
        Shot($"acusacion_{index + 1}");
    }

    // Ronda 15: los partes de la mañana quedan al final de la libreta
    [UnityTest]
    public IEnumerator LibretaConPartes()
    {
        Tutorial.SkipAll();
        yield return ToInterrogation();
        var manager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
        for (int d = 0; d < 3; d++)
        {
            manager.EndDay();
            yield return new WaitForSecondsRealtime(3.5f);
            for (int t = 0; t < 2; t++) // El primer toque termina de escribir el parte; el segundo lo cierra
                foreach (TapHandler tap in UnityEngine.Object.FindObjectsByType<TapHandler>(FindObjectsSortMode.None))
                    tap.onTap?.Invoke();
            yield return new WaitForSecondsRealtime(0.6f);
        }
        yield return Click("ViewCluesButton");
        yield return new WaitForSecondsRealtime(0.8f);
        var scroll = Find("CluesPanel")?.GetComponentInChildren<ScrollRect>();
        if (scroll != null)
        {
            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = 0f;
        }
        yield return null;
        Shot("libreta_partes");
    }

    // Ronda 9: las notas del jugador (una sospechosa, otra descartada) en la libreta y en la rueda
    [UnityTest]
    public IEnumerator NotasDelJugador()
    {
        Tutorial.SkipAll();
        yield return ToInterrogation();
        var manager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
        var ids = UnityEngine.Object.FindFirstObjectByType<AIConversationManager>().Story.cast
            .Where(c => c.startsUnlocked).Select(c => c.id).ToList();
        manager.CycleNote(ids[0]); // sospechoso
        manager.CycleNote(ids[1]);
        manager.CycleNote(ids[1]); // descartado
        yield return Click("ViewCluesButton");
        yield return new WaitForSecondsRealtime(0.8f);
        Shot("notas_libreta");
        yield return Click("CloseCluesButton");
        // Y una pista de descarte ya encontrada: la rueda lo marca con las palabras de la libreta
        var talk = UnityEngine.Object.FindFirstObjectByType<AIConversationManager>();
        ClueData clearing = talk.State.Variant.clues.FirstOrDefault(c => c.kind == ClueKind.Clears && ids.Contains(c.clears));
        if (clearing != null)
            talk.State.Discover(clearing.id);
        manager.ForceAccusationPanel();
        yield return new WaitForSecondsRealtime(2f);
        Shot("notas_rueda");

        // El informe final recuerda tu nota sobre el culpable (se le apunta "sospecha" y se acusa a otro)
        var conversation = UnityEngine.Object.FindFirstObjectByType<AIConversationManager>();
        string culprit = conversation.State.Variant.culpritId;
        if (manager.Notes.TryGetValue(culprit, out SuspectNote current))
            for (int k = 0; k < 3 && manager.Notes[culprit] != SuspectNote.Sospechoso; k++)
                manager.CycleNote(culprit);
        else
            manager.CycleNote(culprit);
        UnityEngine.Object.FindFirstObjectByType<InterrogationUI>().OnAccuseClick();
        yield return new WaitForSecondsRealtime(3f);
        var reveal = UnityEngine.Object.FindFirstObjectByType<StepReveal>();
        reveal?.OnPointerClick(null);
        yield return new WaitForSecondsRealtime(1f);
        Shot("notas_final");
    }

    [UnityTest]
    public IEnumerator Finales()
    {
        yield return ToInterrogation();
        var ui = UnityEngine.Object.FindFirstObjectByType<InterrogationUI>();
        VariantData variant = CaseLibrary.AllVariants().First().variant;
        foreach (Ending e in new[] { Ending.Good, Ending.Bittersweet, Ending.Insufficient, Ending.Bad })
        {
            var result = new AccusationResult { ending = e, correct = e != Ending.Bad, evidence = e == Ending.Good ? 6 : 2, incriminatingFound = 3, contradictions = 1 };
            ui.ShowAccusationResult(result, "Daniel Mendoza", "Daniel Mendoza", 7, variant.epilogue);
            yield return Frames("final_" + e.ToString().ToLowerInvariant(), 0.15f, 0.5f, 2.5f, 7f);
        }
    }

    // Conversación real: una transcripción del jugador bot (Logs/bot/1A_1.md), con sus días y pruebas
    private static List<ChatEntry> FromTranscript(string path)
    {
        var entries = new List<ChatEntry>();
        var turn = new System.Text.RegularExpressions.Regex(@"^\*\*D(\d+) → ([^\[:*]+?)(?: \[muestra ([^\]]+)\])?:\*\* (.*)$");
        int day = 0, q = 0;
        string who = null;
        foreach (string line in File.ReadAllLines(path))
        {
            var m = turn.Match(line);
            if (m.Success)
            {
                int d = int.Parse(m.Groups[1].Value);
                if (d != day)
                {
                    if (day > 0)
                        entries.Add(ChatEntry.Day(d, "Parte de la mañana del día " + d + "."));
                    day = d;
                    q = 0;
                }
                who = m.Groups[2].Value.Trim();
                entries.Add(ChatEntry.Player(m.Groups[4].Value, m.Groups[3].Success ? m.Groups[3].Value : null, GameClock.TimeOf(q, 5)));
            }
            else if (line.StartsWith("> ") && who != null)
            {
                string answer = System.Text.RegularExpressions.Regex.Replace(line.Substring(2), @"\s*\*\([^)]*\)\*\s*$", "");
                entries.Add(ChatEntry.Suspect(who, answer, GameClock.TimeOf(q, 5)));
                q++;
            }
        }
        return entries;
    }

    [UnityTest]
    public IEnumerator ChatLargo()
    {
        const string path = "Logs/bot/1A_1.md";
        Assume.That(File.Exists(path), "falta la transcripción del bot");
        Tutorial.SkipAll();
        yield return ToInterrogation();
        List<ChatEntry> entries = FromTranscript(path);
        var chat = Find("ConversationScroll").GetComponent<ChatView>();
        var scroll = chat.GetComponent<ScrollRect>();

        chat.Show(entries);
        yield return new WaitForSecondsRealtime(0.5f);
        Shot("chatlargo_abajo");
        foreach (float p in new[] { 0.66f, 0.33f, 1f })
        {
            scroll.verticalNormalizedPosition = p;
            yield return null;
            yield return null;
            Shot($"chatlargo_{Mathf.RoundToInt(p * 100):000}");
        }

        // Llega una respuesta nueva mientras el jugador está leyendo arriba (ha arrastrado el chat): no se le mueve
        // y sale "Nuevos mensajes"
        scroll.verticalNormalizedPosition = 0.5f;
        chat.OnBeginDrag(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
        yield return null;
        entries.Add(ChatEntry.Suspect("Daniel", "Ya se lo he dicho, inspector: aquella noche no salí de mi cuarto.", "17:22"));
        chat.Show(entries, typeLast: true);
        yield return null;
        Assert.AreEqual(0.5f, scroll.verticalNormalizedPosition, 0.02f, "no se le mueve al que lee");
        Assert.IsTrue(Find("NuevosMensajes (auto)").activeInHierarchy, "aparece «Nuevos mensajes»");
        yield return new WaitForSecondsRealtime(0.4f);
        Shot("chatlargo_nuevo_mensaje");
    }

    // Todos los paneles tal como se ven en juego (con fondos, efectos y datos reales)
    [UnityTest]
    public IEnumerator Paneles()
    {
        Tutorial.SkipAll();
        yield return new WaitForSecondsRealtime(2f);
        Shot("panel_menu");
        foreach (var (open, back, name) in new[]
                 {
                     ("InstructionsButton", "BackFromInstructionsButton", "instrucciones"),
                     ("SetingsButton", "BackFromSettingsButton", "ajustes"),
                     ("AboutButton", "BackFromAboutButton", "acercade")
                 })
        {
            yield return Click(open);
            yield return new WaitForSecondsRealtime(0.5f);
            Shot("panel_" + name);
            // Lo de abajo de la lista (en Ajustes, la dificultad) también se revisa
            var scroll = Find(open.Replace("Button", "Panel").Replace("Setings", "Settings"))?.GetComponentInChildren<ScrollRect>();
            if (scroll != null)
            {
                scroll.verticalNormalizedPosition = 0f;
                yield return null;
                Shot("panel_" + name + "_final");
            }
            yield return Click(back);
        }

        yield return Click("PlayButton");
        yield return new WaitForSecondsRealtime(0.5f);
        Shot("panel_casos");
        yield return Click("Caso al azar");
        yield return new WaitForSecondsRealtime(6f);
        Shot("panel_intro");
        yield return Click("StartButton");
        yield return new WaitForSecondsRealtime(0.8f);
        Shot("panel_interrogatorio");
        yield return Click("ViewCluesButton");
        yield return new WaitForSecondsRealtime(0.8f);
        Shot("panel_libreta");
        yield return Click("CloseCluesButton");
        UnityEngine.Object.FindFirstObjectByType<GameManager>().ForceAccusationPanel();
        yield return new WaitForSecondsRealtime(2f);
        Shot("panel_acusacion");
    }

    [UnityTest]
    public IEnumerator TextoMuyGrande()
    {
        Tutorial.SkipAll();
        int before = GameSettings.TextSizeLevel;
        GameSettings.TextSizeLevel = 2;
        try
        {
            yield return ToInterrogation();
            yield return new WaitForSecondsRealtime(0.8f);
            Shot("texto_grande_interrogatorio");
            UnityEngine.Object.FindFirstObjectByType<GameManager>().ForceAccusationPanel();
            yield return new WaitForSecondsRealtime(2f);
            Shot("texto_grande_acusacion");
        }
        finally
        {
            GameSettings.TextSizeLevel = before;
        }
    }

    [UnityTest]
    public IEnumerator AltoContraste()
    {
        Tutorial.SkipAll();
        bool before = GameSettings.HighContrast;
        GameSettings.HighContrast = true;
        try
        {
            yield return ToInterrogation();
            yield return new WaitForSecondsRealtime(0.8f);
            Shot("alto_contraste_interrogatorio");
            Find("EndDayButton").GetComponent<Button>().onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.4f);
            Shot("alto_contraste_fin_del_dia");
        }
        finally
        {
            GameSettings.HighContrast = before;
        }
    }

    [UnityTest]
    public IEnumerator Desplegable()
    {
        Tutorial.SkipAll();
        yield return ToInterrogation();
        var ui = UnityEngine.Object.FindFirstObjectByType<InterrogationUI>();
        ui.SetEvidenceOptions(CaseLibrary.AllVariants().First().variant.clues);
        var dropdown = Find("EvidenceDropdown (auto)").GetComponent<TMPro.TMP_Dropdown>();
        dropdown.Show();
        yield return new WaitForSecondsRealtime(0.6f);
        Shot("desplegable_pruebas");
    }

    [UnityTest]
    public IEnumerator Dialogos()
    {
        yield return Click("SetingsButton");
        Find("AjustesControles").GetComponentsInChildren<Button>(true).First(b => b.GetComponentInChildren<TMPro.TMP_Text>().text == GameTexts.RestartButton).onClick.Invoke();
        yield return new WaitForSecondsRealtime(0.4f);
        Shot("dialogo_reiniciar");
        yield return Click("BackFromSettingsButton");

        Tutorial.Reset();
        yield return ToInterrogation();
        yield return new WaitForSecondsRealtime(1.5f);
        Shot("tutorial_preguntar");
        var fx = UnityEngine.Object.FindFirstObjectByType<FxLayer>();
        fx.Hint(Tutorial.TextOf(Tutorial.Contradiction), (RectTransform)Find("ViewCluesButton").transform, null, null);
        yield return new WaitForSecondsRealtime(0.6f);
        Shot("tutorial_contradiccion");
        fx.Hint(Tutorial.TextOf(Tutorial.Days), (RectTransform)Find("EndDayButton").transform, null, null);
        yield return new WaitForSecondsRealtime(0.6f);
        Shot("tutorial_dias");
        Find("Indicacion (auto)")?.SetActive(false);
        yield return Click("ViewCluesButton");
        yield return new WaitForSecondsRealtime(0.6f);
        fx.Hint(Tutorial.TextOf(Tutorial.Versions), (RectTransform)Find("CloseCluesButton").transform, null, null);
        yield return new WaitForSecondsRealtime(0.6f);
        Shot("tutorial_versiones");
        Find("Indicacion (auto)")?.SetActive(false);
        yield return Click("CloseCluesButton");

        Find("Indicacion (auto)")?.SetActive(false);
        Find("EndDayButton").GetComponent<Button>().onClick.Invoke();
        yield return new WaitForSecondsRealtime(0.4f);
        Shot("dialogo_fin_del_dia");
    }
}
