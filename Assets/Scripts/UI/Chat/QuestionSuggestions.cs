using System.Collections.Generic;

/// <summary>
/// Preguntas de ejemplo que se ofrecen en un interrogatorio sin empezar: el chat vacío deja de ser un hueco y en
/// el móvil se ahorra teclear la primera. Tocarlas solo rellena el campo; la pregunta se gasta al enviarla.
/// </summary>
public static class QuestionSuggestions
{
    public static bool ShouldShow(IReadOnlyList<ChatEntry> entries)
    {
        if (entries == null)
            return false;
        foreach (ChatEntry entry in entries)
        {
            if (entry.kind == ChatEntryKind.Player)
                return false;
        }
        return true;
    }

    public static string[] For(string victim)
    {
        string who = string.IsNullOrEmpty(victim) ? "la víctima" : victim;
        // Elegidas con datos (SuggestionProbe, Logs/sugerencias.md): son las que más pistas destapan como primera
        // pregunta; "¿Qué relación tenías con…?" sonaba natural pero no abría casi nada
        return new[]
        {
            "¿Dónde estabas cuando pasó?",
            $"¿Cuándo viste o hablaste con {who} por última vez?",
            "¿Viste u oíste algo raro?"
        };
    }
}
