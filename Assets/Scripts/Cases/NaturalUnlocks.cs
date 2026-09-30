using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Cómo aparece un personaje que empieza bloqueado, sin un "día 3 mágico":
///   - cuando alguien lo menciona en una respuesta (alias, en TurnAnalyzer), o
///   - cuando el jugador pregunta por su tema ("vecina", "curva", "caballos"…), o
///   - si nadie lo trae, con un parte de la mañana de su historia (un agente lo lleva a comisaría) el día indicado.
/// Los disparadores están en Resources/Stories/StoriesDatabase.json.
/// </summary>
public static class NaturalUnlocks
{
    public const int DefaultDay = 3; // Para personajes sin disparador en la base de datos

    public static IEnumerable<string> TriggeredBy(StoryData story, IEnumerable<string> unlocked, string question)
    {
        var have = new HashSet<string>(unlocked);
        string normalized = ClueDetector.Normalize(question ?? "");
        foreach (UnlockTrigger t in story.unlockTriggers)
        {
            if (!have.Contains(t.id) && t.playerStems != null && t.playerStems.Any(stem => normalized.Contains(ClueDetector.Normalize(stem))))
                yield return t.id;
        }
    }

    public static IEnumerable<(string id, string text)> DueOn(StoryData story, IEnumerable<string> unlocked, int day)
    {
        var have = new HashSet<string>(unlocked);
        foreach (CharacterData c in story.cast.Where(c => !have.Contains(c.id)))
        {
            UnlockTrigger t = story.unlockTriggers.FirstOrDefault(x => x.id == c.id);
            int dueDay = t != null && t.fallbackDay > 0 ? t.fallbackDay : DefaultDay;
            if (day >= dueDay)
                yield return (c.id, t != null && !string.IsNullOrEmpty(t.fallbackText) ? t.fallbackText
                                                                                        : $"Un agente te informa: conviene hablar con {c.name}.");
        }
    }
}
