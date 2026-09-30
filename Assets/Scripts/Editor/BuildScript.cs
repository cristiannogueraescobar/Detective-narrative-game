using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Builds por línea de comandos (batchmode):
///   Unity -batchmode -nographics -projectPath . -executeMethod BuildScript.BuildWindows -quit
///   Unity -batchmode -nographics -projectPath . -executeMethod BuildScript.BuildAndroid -quit   (necesita el módulo Android)
/// Salida en Builds/ (o en la ruta de "-buildPath"). Las escenas son las activas de Build Settings (solo Game.unity).
/// </summary>
public static class BuildScript
{
    public static string[] Scenes => EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

    [MenuItem("Detective/Build de Windows")]
    public static void BuildWindows()
    {
        Build(BuildTarget.StandaloneWindows64, OutputPath(Environment.GetCommandLineArgs(), "Builds/Windows/Detectives.exe"));
    }

    [MenuItem("Detective/Build de Android (APK)")]
    public static void BuildAndroid()
    {
        EditorUserBuildSettings.buildAppBundle = false;
        Build(BuildTarget.Android, OutputPath(Environment.GetCommandLineArgs(), "Builds/Android/Detectives.apk"));
    }

    /// <summary>
    /// Ruta de salida: la de siempre, o la de "-buildPath &lt;ruta&gt;" si se pasa por línea de comandos (p. ej. si el
    /// sistema bloquea la ruta habitual).
    /// </summary>
    public static string OutputPath(string[] args, string fallback)
    {
        int i = Array.IndexOf(args, "-buildPath");
        return i >= 0 && i + 1 < args.Length && !string.IsNullOrWhiteSpace(args[i + 1]) ? args[i + 1] : fallback;
    }

    public const string AppIconPath = "Assets/Art/Icons/app_icon.png";

    /// <summary>
    /// Icono de la aplicación (generado por Tools/make_app_icon.py) para todas las plataformas.
    /// </summary>
    public static void SetAppIcon()
    {
        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AppIconPath);
        if (icon == null)
        {
            Debug.LogWarning($"[Build] Falta {AppIconPath}: la build usará el icono por defecto");
            return;
        }
        PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
        // productName ("Casos") no se toca: cambiarlo movería los guardados y los ajustes del jugador
    }

    private static void Build(BuildTarget target, string output)
    {
        ArtCatalogBuilder.Rebuild(); // El arte nuevo tiene que ir en el catálogo de la build
        SetAppIcon();
        Directory.CreateDirectory(Path.GetDirectoryName(output));

        var options = new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = output,
            target = target,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        Debug.Log($"[Build] {target}: {summary.result} · {summary.totalSize / (1024 * 1024)} MB · {summary.totalTime} · errores {summary.totalErrors} · {Path.GetFullPath(output)}");

        if (Application.isBatchMode)
            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
