using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Parte del caso para la pantalla de inicio: lugar, víctima y situación, con el estilo del parte de la mañana.
/// </summary>
public static class CaseBriefing
{
    public static string Format(StoryData story)
    {
        Theme t = ThemeManager.Current;
        string heading(string text) => $"<color={Theme.Hex(t.accent)}><b>{text}</b></color>";

        var sb = new StringBuilder();
        sb.AppendLine($"<size={t.secondarySize}>{heading("PARTE DEL CASO")}</size>");
        sb.AppendLine($"<size={t.headingSize}><b>{story.title.ToUpperInvariant()}</b></size>");
        sb.AppendLine();
        sb.AppendLine(heading("LUGAR"));
        sb.AppendLine(story.place);
        sb.AppendLine();
        sb.AppendLine(heading("VÍCTIMA"));
        sb.AppendLine(story.victimSummary);
        sb.AppendLine();
        sb.AppendLine(heading("SITUACIÓN"));
        sb.AppendLine(story.situation);
        sb.AppendLine();
        sb.Append($"<color={Theme.Hex(t.textSecondary)}><i>Tienes 7 días y cinco preguntas cada día. Encuentra pruebas y contradicciones antes de acusar.</i></color>");
        return sb.ToString();
    }
}

/// <summary>
/// Libreta del detective: pistas (nombre + resumen), contradicciones y sospechosos con su estado.
/// Es la referencia del jugador para decidir a quién acusar.
/// </summary>
public static class Notebook
{
    public static string Format(StoryData story, InvestigationState state, IEnumerable<string> unlocked,
                                IReadOnlyDictionary<string, Emotion> emotions, Func<ClueData, string> describeContradiction)
    {
        Theme t = ThemeManager.Current;
        string heading(string text) => $"<color={Theme.Hex(t.accent)}><b>{text}</b></color>";
        List<ClueData> clues = state.DiscoveredClueIds.Select(state.Variant.Clue).ToList();

        var sb = new StringBuilder();

        sb.AppendLine(heading($"PISTAS ({clues.Count})"));
        if (clues.Count == 0)
            sb.AppendLine("Aún no hay pistas. Pregunta por horas, lugares y objetos concretos.");
        foreach (ClueData clue in clues)
            sb.AppendLine($"• <b>{clue.playerName}</b>: {clue.summary}");
        sb.AppendLine();

        sb.AppendLine(heading($"CONTRADICCIONES ({state.ContradictionClueIds.Count})"));
        if (state.ContradictionClueIds.Count == 0)
            sb.AppendLine("Ninguna contradicción registrada.");
        foreach (string id in state.ContradictionClueIds)
            sb.AppendLine($"• <color={Theme.Hex(t.contradiction)}>{describeContradiction(state.Variant.Clue(id))}</color>");
        sb.AppendLine();

        sb.AppendLine(heading("SOSPECHOSOS"));
        var unlockedSet = new HashSet<string>(unlocked);
        foreach (CharacterData character in story.cast.Where(c => unlockedSet.Contains(c.id)))
        {
            string line = $"• <b>{character.DisplayName}</b>";

            if (emotions.TryGetValue(character.id, out Emotion emotion))
                line += $" — estado: {emotion.ToString().ToLowerInvariant()}";

            ClueData clearing = clues.FirstOrDefault(c => c.kind == ClueKind.Clears && c.clears == character.id);
            if (clearing != null)
                line += $" — <color={Theme.Hex(t.success)}>pista de descarte: {clearing.playerName}</color>";

            sb.AppendLine(line);
        }

        return sb.ToString().TrimEnd();
    }
}
