using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;

/// <summary>
/// Matriz de coherencia de las 3 historias × 3 variantes, generada desde los datos reales (no se desfasa):
/// reparto y desbloqueos, culpable, móvil, mentira frente a la pista ⚡ que la rompe, el resto de pistas con su
/// portador y el resultado del validador narrativo. Escribe docs/COHERENCE.md conservando lo que haya antes de la
/// marca (la revisión a mano).
///
/// Menú: Detective/Matriz de coherencia · Batchmode: -executeMethod CoherenceReport.RunFromCommandLine
/// </summary>
public static class CoherenceReport
{
    public const string Path_ = "docs/COHERENCE.md";
    public const string Marker = "<!-- MATRIZ GENERADA (Detective/Matriz de coherencia): no editar debajo -->";

    [MenuItem("Detective/Matriz de coherencia")]
    public static void Run()
    {
        string manual = File.Exists(Path_) ? File.ReadAllText(Path_) : "# Coherencia de las historias\n\n";
        int cut = manual.IndexOf(Marker, StringComparison.Ordinal);
        if (cut >= 0)
            manual = manual.Substring(0, cut);
        File.WriteAllText(Path_, manual.TrimEnd() + "\n\n" + Marker + "\n\n" + Build(), new UTF8Encoding(false));
    }

    public static void RunFromCommandLine()
    {
        int code = 0;
        try { Run(); Console.WriteLine("[Coherencia] " + Path.GetFullPath(Path_)); }
        catch (Exception e) { Console.WriteLine("[Coherencia] Error: " + e); code = 2; }
        EditorApplication.Exit(code);
    }

    public static string Build()
    {
        var sb = new StringBuilder();
        foreach (StoryData story in CaseLibrary.Stories)
        {
            sb.AppendLine($"## Historia {story.id}: {story.title}");
            sb.AppendLine();
            sb.AppendLine($"Víctima: {story.victimSummary}");
            sb.AppendLine();
            sb.AppendLine("| Personaje | Nombre | Papel | Aparece |");
            sb.AppendLine("|---|---|---|---|");
            foreach (CharacterData c in story.cast)
            {
                UnlockTrigger t = story.unlockTriggers.FirstOrDefault(x => x.id == c.id);
                string appears = c.startsUnlocked ? "desde el día 1"
                    : $"cuando lo mencionan (alias: {string.Join(", ", c.mentionAliases ?? new string[0])})" +
                      (t != null ? $", cuando el jugador pregunta por «{string.Join("», «", t.playerStems)}» o, si no, el día {t.fallbackDay} (parte: «{t.fallbackText}»)"
                                 : $" o, como tarde, el día {NaturalUnlocks.DefaultDay}");
                sb.AppendLine($"| `{c.id}` | {c.name} | {c.roleLabel} | {appears} |");
            }
            sb.AppendLine();

            foreach (VariantData v in story.variants)
            {
                CharacterData culprit = story.cast.First(c => c.id == v.culpritId);
                CharacterRole role = v.Role(v.culpritId);
                var issues = NarrativeValidator.Validate(story, v);
                sb.AppendLine($"### {v.id} · culpable: {culprit.name} ({culprit.roleLabel})");
                sb.AppendLine();
                sb.AppendLine($"- **Qué hizo y por qué (secreto):** {role.secret}");
                sb.AppendLine($"- **Su mentira:** «{role.lieQuote}» · versión: {role.version}");
                sb.AppendLine($"- **Si le muestran la prueba (versión B):** {role.versionB}");
                foreach (ClueData c in v.clues.Where(c => c.exposesLie))
                    sb.AppendLine($"- **⚡ {c.id}** ({Holder(story, c)}{(c.holder == v.culpritId ? " — ¡ES EL CULPABLE!" : "")}): {c.fact}");
                sb.AppendLine($"- **Validador narrativo:** {(issues.Count == 0 ? "sin problemas" : string.Join("; ", issues.Select(i => i.ToString())))}");
                sb.AppendLine();
                sb.AppendLine("| Pista | Portador | Tipo | Secreta | Hecho |");
                sb.AppendLine("|---|---|---|---|---|");
                foreach (ClueData c in v.clues)
                {
                    string kind = c.kind == ClueKind.Clears ? $"descarta a `{c.clears}`" : c.kind == ClueKind.Incriminates ? "incrimina" : "contexto";
                    sb.AppendLine($"| {(c.exposesLie ? "⚡ " : "")}{c.id} | {Holder(story, c)} | {kind} | {(c.isSecret ? "sí" : "")} | {c.fact.Replace("|", "/")} |");
                }
                sb.AppendLine();
            }
        }
        return sb.ToString();
    }

    private static string Holder(StoryData story, ClueData c)
    {
        CharacterData h = story.cast.FirstOrDefault(x => x.id == c.holder);
        return h != null ? $"{h.shortName} ({h.roleLabel})" : c.holder;
    }
}
