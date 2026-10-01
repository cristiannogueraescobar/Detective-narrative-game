using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Resultado de aplicar una respuesta de un sospechoso al estado de la investigación.
/// </summary>
public class TurnOutcome
{
    public List<ClueData> newClues = new List<ClueData>();
    public List<ClueData> newContradictions = new List<ClueData>();
    public List<string> mentionedCharacters = new List<string>();
    public bool lieTold;
    public List<(ClueData clue, AnchorTrace trace)> traces = new List<(ClueData, AnchorTrace)>();
    public AnchorTrace lieTrace;
}

/// <summary>
/// Lógica pura de un turno de interrogatorio: mensaje del jugador y análisis de la respuesta.
/// </summary>
public static class TurnAnalyzer
{
    public const string DefaultConfrontQuestion = "¿Qué tienes que decir a esto?";

    public static string BuildUserMessage(string question, ClueData shownClue)
    {
        // Solo el juego escribe entre corchetes: el jugador no puede imitar "[El inspector te muestra una prueba: …]"
        // para enseñar una prueba que no tiene (auditoría finecomb)
        string trimmed = (question?.Trim() ?? "").Replace('[', '(').Replace(']', ')');

        if (shownClue == null)
            return trimmed;

        if (trimmed.Length == 0)
            trimmed = DefaultConfrontQuestion;

        return $"[El inspector te muestra una prueba: {shownClue.summary}]\n{trimmed}";
    }

    /// <summary>
    /// Aplica la respuesta al estado: prueba mostrada, pistas del portador, mentira del culpable,
    /// menciones de otros personajes y contradicciones nuevas.
    /// </summary>
    public static TurnOutcome Analyze(StoryData story, InvestigationState state, string characterId,
                                      string response, ClueData shownClue)
    {
        var outcome = new TurnOutcome();
        VariantData variant = state.Variant;
        string normalized = ClueDetector.Normalize(response);

        if (shownClue != null)
            state.RegisterShown(characterId, shownClue.id);

        foreach (ClueData clue in variant.clues.Where(c => c.holder == characterId && !state.IsDiscovered(c.id)))
        {
            AnchorTrace trace = ClueDetector.Evaluate(clue.anchors, normalized);
            outcome.traces.Add((clue, trace));

            if (trace.Matched && state.Discover(clue.id))
                outcome.newClues.Add(clue);
        }

        if (characterId == variant.culpritId)
        {
            // Las mentiras suelen ser negaciones ("no entré"): sin guardia de negación
            outcome.lieTrace = ClueDetector.Evaluate(variant.Role(characterId).lieAnchors, normalized, negationGuard: false);
            if (outcome.lieTrace.Matched)
            {
                outcome.lieTold = true;
                state.RegisterLieTold();
            }
        }

        foreach (CharacterData other in story.cast.Where(c => c.id != characterId))
        {
            if (ClueDetector.MentionsAny(normalized, other.mentionAliases))
                outcome.mentionedCharacters.Add(other.id);
        }

        outcome.newContradictions = state.UpdateContradictions();
        return outcome;
    }

    /// <summary>
    /// Secretos que este personaje ya ha admitido: se recuerdan en su ficha para que no vuelva a negarlos.
    /// </summary>
    public static IEnumerable<ClueData> RevealedSecrets(InvestigationState state, string characterId)
    {
        return state.DiscoveredClueIds
            .Select(state.Variant.Clue)
            .Where(c => c.holder == characterId && c.isSecret);
    }
}

public static class ConversationWindow
{
    /// <summary>
    /// Últimos 'max' mensajes del historial, empezando siempre por un mensaje del jugador.
    /// </summary>
    public static List<ChatMessage> Last(List<ChatMessage> history, int max)
    {
        int start = System.Math.Max(0, history.Count - max);

        while (start < history.Count && history[start].role != "user")
            start++;

        return history.GetRange(start, history.Count - start);
    }
}
