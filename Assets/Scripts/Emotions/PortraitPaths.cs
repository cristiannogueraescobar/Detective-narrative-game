using System.Collections.Generic;

/// <summary>
/// Rutas de retratos por personaje y estado: Assets/Art/Portraits/&lt;artId&gt;_&lt;estado&gt;.png.
/// Si falta el estado exacto se prueba un sustituto cercano y después el retrato tranquilo.
/// </summary>
public static class PortraitPaths
{
    public const string Folder = "Assets/Art/Portraits";

    public static string For(string artId, Emotion emotion)
    {
        return $"{Folder}/{artId}_{emotion.ToString().ToLowerInvariant()}.png";
    }

    /// <summary>
    /// Estado cuyo retrato se usa si falta el propio (el tinte y el temblor completan la diferencia).
    /// </summary>
    public static Emotion Substitute(Emotion emotion)
    {
        switch (emotion)
        {
            case Emotion.Asustado:
            case Emotion.Enfadado:
                return Emotion.Nervioso;
            default:
                return Emotion.Tranquilo;
        }
    }

    public static List<string> Candidates(string artId, Emotion emotion)
    {
        var list = new List<string> { For(artId, emotion) };

        foreach (Emotion fallback in new[] { Substitute(emotion), Emotion.Tranquilo })
        {
            string path = For(artId, fallback);
            if (!list.Contains(path))
                list.Add(path);
        }

        return list;
    }
}
