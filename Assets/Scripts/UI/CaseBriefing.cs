using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Parte del caso para la pantalla de inicio: lugar, víctima y situación, con el estilo del parte de la mañana.
/// </summary>
public static class CaseBriefing
{
    public static string Format(StoryData story, int questionsPerDay = 5, int days = 7)
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
        sb.Append($"<color={Theme.Hex(t.textSecondary)}><i>{GameTexts.RulesLine(days, questionsPerDay)}</i></color>");
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
    public const string NoteLinkPrefix = "nota:"; // Tocar la nota del jugador la cambia (SuspectNotes)

    public static string Format(StoryData story, InvestigationState state, IEnumerable<string> unlocked,
                                IReadOnlyDictionary<string, Emotion> emotions, Func<ClueData, string> describeContradiction,
                                bool onPaper = false, IEnumerable<string> interviewed = null,
                                IReadOnlyDictionary<string, SuspectNote> notes = null, IReadOnlyList<string> reports = null)
    {
        Theme t = ThemeManager.Current;
        // Sobre el papel de la libreta, tintas oscuras (los colores del tema no se leerían sobre crema)
        UnityEngine.Color headingColor = onPaper ? t.paperInk : t.accent;
        UnityEngine.Color contradictionColor = onPaper ? new UnityEngine.Color32(150, 62, 20, 255) : t.contradiction;
        UnityEngine.Color successColor = onPaper ? new UnityEngine.Color32(46, 100, 50, 255) : t.success;
        string heading(string text) => $"<color={Theme.Hex(headingColor)}><b>{text}</b></color>";
        List<ClueData> clues = state.DiscoveredClueIds.Select(state.ClueAsFound).ToList(); // Con el resumen de quien la contó

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
            sb.AppendLine("<i><size=85%>Toca un nombre para ir a interrogar, o tu nota para cambiarla.</size></i>");
        var unlockedSet = new HashSet<string>(unlocked);
        var interviewedSet = new HashSet<string>(interviewed ?? Enumerable.Empty<string>());
        foreach (CharacterData character in story.cast.Where(c => unlockedSet.Contains(c.id)))
        {
            // En la libreta de papel, el nombre lleva a interrogar a esa persona
            string who = onPaper ? $"<link=\"{SuspectLinkPrefix}{character.id}\"><u>{character.DisplayName}</u></link>" : character.DisplayName;
            string line = $"• <b>{who}</b>";

            if (emotions.TryGetValue(character.id, out Emotion emotion))
                line += $" — estado: {emotion.ToString().ToLowerInvariant()}";

            // La nota del propio jugador (solo en la libreta de papel, donde se puede tocar)
            if (onPaper)
            {
                SuspectNote note = notes != null && notes.TryGetValue(character.id, out SuspectNote n) ? n : SuspectNote.Ninguna;
                string noteText = note == SuspectNote.Ninguna ? "añadir nota" : $"tu nota: {SuspectNotes.Label(note)}";
                line += $" — <nobr><link=\"{NoteLinkPrefix}{character.id}\"><u>{noteText}</u></link></nobr>";
            }

            ClueData clearing = clues.FirstOrDefault(c => c.kind == ClueKind.Clears && c.clears == character.id);
            if (clearing != null)
                line += $" — <color={Theme.Hex(successColor)}>pista de descarte: {clearing.playerName}</color>";

            sb.AppendLine(line);

            // Lo que dice que hizo, en cuanto ha contestado algo: el jugador lo compara con sus pistas (si una
            // pista choca con la versión de alguien, enseñársela es como se destapa una mentira)
            // Igual para todos en cuanto ha contestado (sesión C: la del culpable esperaba a su mentira y le delataba). Una
            // versión que nombra a alguien aún no disponible espera a que aparezca
            // La misma regla que da por dicha su mentira (HeardVersions)
            CharacterRole role = state.Variant.roles.FirstOrDefault(r => r.characterId == character.id);
            if (HeardVersions.Heard(story, role, interviewedSet, unlockedSet))
                sb.AppendLine($"<indent=6%><i><size=90%>Dice: «{role.version}»</size></i></indent>");
        }

        // Los partes de la mañana, para releerlos (antes solo se veían en la transición del día)
        if (reports != null && reports.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine(heading("PARTES DE LA MAÑANA"));
            foreach (string report in reports)
                sb.AppendLine($"• {report}");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Los partes recibidos hasta 'day' ("Día 2: …"); el día 1 no tiene.
    /// </summary>
    public static List<string> ReportsUpTo(VariantData variant, int day)
    {
        var list = new List<string>();
        string[] reports = variant?.morningReports ?? new string[0];
        for (int d = 2; d <= day && d - 1 < reports.Length; d++)
        {
            if (!string.IsNullOrWhiteSpace(reports[d - 1]))
                list.Add($"Día {d}: {reports[d - 1]}");
        }
        return list;
    }
}
