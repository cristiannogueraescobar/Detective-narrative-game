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
/// Salida en Builds/. Las escenas son las activas de Build Settings (solo Game.unity).
/// </summary>
public static class BuildScript
{
    public static string[] Scenes => EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

    [MenuItem("Detective/Build de Windows")]
    public static void BuildWindows()
    {
        Build(BuildTarget.StandaloneWindows64, "Builds/Windows/Detectives.exe");
    }

    [MenuItem("Detective/Build de Android (APK)")]
    public static void BuildAndroid()
    {
        EditorUserBuildSettings.buildAppBundle = false;
        Build(BuildTarget.Android, "Builds/Android/Detectives.apk");
    }

    private static void Build(BuildTarget target, string output)
    {
        ArtCatalogBuilder.Rebuild(); // El arte nuevo tiene que ir en el catálogo de la build
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
