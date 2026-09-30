using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Renderiza todos los paneles (y los 4 finales) a PNG con una cámara temporal y un RenderTexture.
/// Batchmode (sin -nographics):
///   Unity -batchmode -projectPath . -executeMethod ScreenshotTool.CaptureFromCommandLine [-shotLabel antes] [-shotScale 0.5]
/// Salida: docs/screenshots/&lt;fecha&gt;/&lt;etiqueta&gt;/&lt;pantalla&gt;_&lt;vista&gt;.png
/// </summary>
public static class ScreenshotTool
{
    public static readonly (string label, Vector2 screen)[] Screens =
    {
        ("1080x1920", new Vector2(1080f, 1920f)),
        ("1080x2400", new Vector2(1080f, 2400f))
    };

    /// <summary>
    /// Una vista: nombre de archivo y cómo dejar la UI antes de mostrar el panel.
    /// </summary>
    public class View
    {
        public string name;
        public string panel;
        public Action<LayoutPreview.Session> prepare;
    }

    public static List<View> Views()
    {
        var views = LayoutPreview.Panels.Where(p => p != "ResultPanel")
            .Select(p => new View { name = p, panel = p }).ToList();

        foreach (Ending ending in Enum.GetValues(typeof(Ending)))
        {
            Ending e = ending;
            views.Add(new View { name = "Final_" + e, panel = "ResultPanel", prepare = s => LayoutPreview.ShowEnding(s, e) });
        }
        return views;
    }

    [MenuItem("Detective/Capturas de todos los paneles")]
    public static void CaptureMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;
        string folder = Capture("editor", 0.5f);
        EditorUtility.RevealInFinder(folder);
    }

    public static void CaptureFromCommandLine()
    {
        int code = 0;
        try
        {
            string label = Argument("-shotLabel") ?? DateTime.Now.ToString("HHmm");
            float scale = float.TryParse(Argument("-shotScale"), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float s) ? s : 0.5f;
            Capture(label, scale);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            code = 1;
        }
        EditorApplication.Exit(code);
    }

    private static string Argument(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        int i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }

    public static string Capture(string label, float scale)
    {
        string folder = Path.Combine("docs", "screenshots", DateTime.Now.ToString("yyyy-MM-dd"), label);
        Directory.CreateDirectory(folder);

        foreach (var screen in Screens)
        {
            Vector2 canvasSize = LayoutPreview.CanvasSize(screen.screen.x, screen.screen.y);
            LayoutPreview.Session session = LayoutPreview.Open(canvasSize);

            var cameraObject = new GameObject("Captura", typeof(Camera));
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = canvasSize.y / 2f;
            camera.aspect = canvasSize.x / canvasSize.y;
            camera.transform.position = new Vector3(0f, 0f, -1000f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.magenta; // Lo que no cubra ningún panel salta a la vista
            session.canvas.worldCamera = camera;

            int width = Mathf.RoundToInt(screen.screen.x * scale);
            int height = Mathf.RoundToInt(screen.screen.y * scale);
            var target = new RenderTexture(width, height, 24);
            camera.targetTexture = target;

            foreach (View view in Views())
            {
                view.prepare?.Invoke(session);
                LayoutPreview.ShowOnly(session, view.panel);
                camera.Render();

                RenderTexture.active = target;
                var image = new Texture2D(width, height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply();
                RenderTexture.active = null;
                File.WriteAllBytes(Path.Combine(folder, $"{screen.label}_{view.name}.png"), image.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(image);
            }

            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(target);
            LayoutPreview.Close();
        }

        Debug.Log($"[Capturas] {Path.GetFullPath(folder)}");
        return folder;
    }
}
