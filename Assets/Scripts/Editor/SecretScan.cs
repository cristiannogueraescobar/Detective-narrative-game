using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Busca claves de Anthropic en los archivos versionados (git ls-files): ninguna debe llegar al repositorio, que es
/// público. Si git no está disponible, revisa las carpetas del proyecto que se versionan. Lo usa SecretScanTests.
/// </summary>
public static class SecretScan
{
    // Montado por partes: este archivo no debe detectarse a sí mismo
    private static readonly Regex Key = new Regex(Regex.Escape("sk" + "-ant-") + @"[A-Za-z0-9_\-]*");
    private const long MaxBytes = 20_000_000;

    public static List<string> Find(string text)
    {
        return Key.Matches(text ?? "").Cast<Match>().Select(m => m.Value).ToList();
    }

    /// <summary>
    /// "ruta: prefijo y primeros caracteres…" por cada clave encontrada (recortada: el aviso no repite la clave entera).
    /// </summary>
    public static List<string> ScanTrackedFiles()
    {
        string root = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, ".."));
        var hits = new List<string>();
        foreach (string relative in TrackedFiles(root))
        {
            string path = Path.Combine(root, relative);
            if (!File.Exists(path) || new FileInfo(path).Length > MaxBytes)
                continue;
            byte[] bytes = File.ReadAllBytes(path);
            if (Array.IndexOf(bytes, (byte)0) >= 0)
                continue; // Binario (imágenes, audio)
            foreach (string key in Find(System.Text.Encoding.UTF8.GetString(bytes)))
                hits.Add($"{relative}: {key.Substring(0, Math.Min(key.Length, 18))}…");
        }
        return hits;
    }

    private static IEnumerable<string> TrackedFiles(string root)
    {
        try
        {
            var info = new ProcessStartInfo("git", "ls-files -z")
            {
                WorkingDirectory = root,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8
            };
            using (Process git = Process.Start(info))
            {
                string output = git.StandardOutput.ReadToEnd();
                git.WaitForExit();
                if (git.ExitCode == 0)
                    return output.Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }
        catch (Exception)
        {
            // Sin git: las carpetas que se versionan
        }
        return new[] { "Assets", "ProjectSettings", "Packages", "docs", "Tools" }
            .Where(d => Directory.Exists(Path.Combine(root, d)))
            .SelectMany(d => Directory.EnumerateFiles(Path.Combine(root, d), "*", SearchOption.AllDirectories))
            .Select(f => f.Substring(root.Length + 1));
    }
}
