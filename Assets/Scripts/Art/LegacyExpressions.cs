using System.Collections.Generic;

/// <summary>
/// Expresiones de los personajes hechas editando solo la cara de su retrato del juego (sesión C,
/// Tools/make_expressions.py): Assets/Art/Derived/Expressions/&lt;artId&gt;_&lt;estado&gt;.png. Tienen el tamaño y el encuadre
/// de su retrato, así que se usan como él (arte antiguo: PortraitCrops por portraitKey y el mismo tratamiento).
/// tranquilo es el retrato de hoy. Si falta un estado, su sustituto (asustado y enfadado → nervioso).
/// </summary>
public static class LegacyExpressions
{
    public const string Folder = "Assets/Art/Derived/Expressions";

    public static IEnumerable<string> Candidates(string artId, Emotion emotion)
    {
        if (string.IsNullOrEmpty(artId) || emotion == Emotion.Tranquilo)
            yield break;
        yield return Path(artId, emotion);
        Emotion substitute = PortraitPaths.Substitute(emotion);
        if (substitute != emotion && substitute != Emotion.Tranquilo)
            yield return Path(artId, substitute);
    }

    private static string Path(string artId, Emotion emotion) => $"{Folder}/{artId}_{emotion.ToString().ToLowerInvariant()}.png";
}
