using System.Collections.Generic;
using UnityEngine;

public struct ArtSlot
{
    public string path;
    public int width;
    public int height;
}

/// <summary>
/// Todo el arte del juego (además de los retratos) y su tamaño recomendado. Cada hueco debe estar
/// documentado en ART-NEEDED.md (lo comprueba un test). Si falta el archivo, se usa un color plano.
/// </summary>
public static class ArtSlots
{
    public const string MenuBackground = "Assets/Art/Backgrounds/menu.png";
    public const string IconNotebook = "Assets/Art/Icons/libreta.png";
    public const string IconClue = "Assets/Art/Icons/pista.png";
    public const string IconDay = "Assets/Art/Icons/dia.png";
    public const string IconQuestions = "Assets/Art/Icons/preguntas.png";
    public const string IconContradiction = "Assets/Art/Icons/contradiccion.png";

    public const int IconSize = 128;

    public static string StoryHeader(string storyId) => $"Assets/Art/Stories/historia{storyId}_cabecera.png";
    public static string StoryIntro(string storyId) => $"Assets/Art/Stories/historia{storyId}_intro.png";

    public static IEnumerable<ArtSlot> All()
    {
        yield return new ArtSlot { path = MenuBackground, width = 1080, height = 1920 };

        foreach (StoryData story in CaseLibrary.Stories)
        {
            yield return new ArtSlot { path = StoryHeader(story.id), width = 1080, height = 480 };
            yield return new ArtSlot { path = StoryIntro(story.id), width = 1080, height = 1920 };
        }

        foreach (string icon in new[] { IconNotebook, IconClue, IconDay, IconQuestions, IconContradiction })
            yield return new ArtSlot { path = icon, width = IconSize, height = IconSize };
    }

    public static Texture2D LoadOrPlaceholder(string path, Color placeholder)
    {
        return ArtLibrary.Load(path) ?? ArtLibrary.Placeholder(placeholder);
    }
}
