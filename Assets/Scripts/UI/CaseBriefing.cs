using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Parte del caso para la pantalla de inicio: lugar, víctima y situación, con el estilo del parte de la mañana.
/// </summary>
public static class CaseBriefing
{
    public static string Format(StoryData story, int questionsPerDay = 5)
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
        sb.Append($"<color={Theme.Hex(t.textSecondary)}><i>{GameTexts.RulesLine(7, questionsPerDay)}</i></color>");
        return sb.ToString();
    }
}

/// <summary>
/// Libreta del detective: pistas (nombre + resumen), contradicciones y sospechosos con su estado.
/// Es la referencia del jugador para decidir a quién acusar.
/// </summary>
public static class Notebook
{
    public const string ClueLinkPrefix = "pista:";
    public const string SuspectLinkPrefix = "sospechoso:";

    public static string Format(StoryData story, InvestigationState state, IEnumerable<string> unlocked,
                                IReadOnlyDictionary<string, Emotion> emotions, Func<ClueData, string> describeContradiction,
                                bool onPaper = false)
    {
        Theme t = ThemeManager.Current;
        // Sobre el papel de la libreta, tintas oscuras (los colores del tema no se leerían sobre crema)
        UnityEngine.Color headingColor = onPaper ? t.paperInk : t.accent;
        UnityEngine.Color contradictionColor = onPaper ? new UnityEngine.Color32(150, 62, 20, 255) : t.contradiction;
        UnityEngine.Color successColor = onPaper ? new UnityEngine.Color32(46, 100, 50, 255) : t.success;
        string heading(string text) => $"<color={Theme.Hex(headingColor)}><b>{text}</b></color>";
        List<ClueData> clues = state.DiscoveredClueIds.Select(state.Variant.Clue).ToList();

        var sb = new StringBuilder();

        sb.AppendLine(heading($"PISTAS ({clues.Count})"));
        if (clues.Count == 0)
            sb.AppendLine("Aún no hay pistas. Pregunta por horas, lugares y objetos concretos.");
        else if (onPaper)
            sb.AppendLine("<i><size=85%>Toca una pista para enseñarla en tu próxima pregunta.</size></i>");
        foreach (ClueData clue in clues)
        {
            // En la libreta de papel, el nombre es un enlace: tocarlo la prepara como prueba para enseñar
            string name = onPaper ? $"<link=\"{ClueLinkPrefix}{clue.id}\"><u>{clue.playerName}</u></link>" : clue.playerName;
            sb.AppendLine($"• <b>{name}</b>: {clue.summary}");
        }
        sb.AppendLine();

        sb.AppendLine(heading($"CONTRADICCIONES ({state.ContradictionClueIds.Count})"));
        if (state.ContradictionClueIds.Count == 0)
            sb.AppendLine("Ninguna contradicción registrada.");
        foreach (string id in state.ContradictionClueIds)
            sb.AppendLine($"• <color={Theme.Hex(contradictionColor)}>{describeContradiction(state.Variant.Clue(id))}</color>");
        sb.AppendLine();

        sb.AppendLine(heading("SOSPECHOSOS"));
        if (onPaper)
            sb.AppendLine("<i><size=85%>Toca un nombre para ir a interrogar.</size></i>");
        var unlockedSet = new HashSet<string>(unlocked);
        foreach (CharacterData character in story.cast.Where(c => unlockedSet.Contains(c.id)))
        {
            // En la libreta de papel, el nombre lleva a interrogar a esa persona
            string who = onPaper ? $"<link=\"{SuspectLinkPrefix}{character.id}\"><u>{character.DisplayName}</u></link>" : character.DisplayName;
            string line = $"• <b>{who}</b>";

            if (emotions.TryGetValue(character.id, out Emotion emotion))
                line += $" — estado: {emotion.ToString().ToLowerInvariant()}";

            ClueData clearing = clues.FirstOrDefault(c => c.kind == ClueKind.Clears && c.clears == character.id);
            if (clearing != null)
                line += $" — <color={Theme.Hex(successColor)}>pista de descarte: {clearing.playerName}</color>";

            sb.AppendLine(line);
        }

        return sb.ToString().TrimEnd();
    }
}
