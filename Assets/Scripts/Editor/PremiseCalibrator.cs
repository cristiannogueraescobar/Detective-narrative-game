using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

/// <summary>
/// SONDA DE PREMISAS FALSAS
/// A cada personaje de cada variante se le pregunta por algo que nunca pasó (una discusión a gritos con otro
/// sospechoso) como si fuera un hecho. Un buen sospechoso lo niega; si lo acepta, el jugador puede "plantar" hechos
/// con preguntas capciosas y acabar acusando por algo inventado. Mide el % de respuestas que aceptan la premisa.
///
/// Batchmode: -executeMethod PremiseCalibrator.RunFromCommandLine [-variants 1A,2B] [-tries 2]
/// Informe: Logs/premise-calibration.md
/// </summary>
public static class PremiseCalibrator
{
    public const string ReportPath = "Logs/premise-calibration.md";

    // Negaciones al principio de la respuesta
    private static readonly Regex Denial = new Regex(
        @"^\W*(no\b|nunca|jam[aá]s|mentira|falso|qu[eé] va|eso no|se equivoca|imposible|de ninguna manera|en absoluto)|" +
        @"\b(no (discut|pas[oó]|es verdad|es cierto|recuerdo|s[eé] de qu[eé])|nunca (he|hemos|discut)|mentira|se equivoca|no he discutido|apenas nos hablamos|no (me )?(lo )?consta)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static bool Denies(string answer)
    {
        string text = EmotionParser.Parse(answer ?? "").text ?? "";
        // Las dos primeras frases: a veces la primera es solo un "¿Discutir?" de sorpresa
        string[] sentences = Regex.Split(text.Trim(), @"(?<=[.!?…])\s+");
        if (sentences.Length == 0)
            return false;
        if (Denial.IsMatch(sentences[0]))
            return true;
        bool interjection = sentences[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length <= 3;
        return interjection && sentences.Length > 1 && Denial.IsMatch(sentences[1]);
    }

    /// <summary>
    /// La premisa falsa: una discusión a gritos con el siguiente personaje del reparto.
    /// </summary>
    public static string Question(StoryData story, string characterId)
    {
        int index = story.cast.FindIndex(c => c.id == characterId);
        CharacterData other = story.cast[(index + 1) % story.cast.Count];
        return $"Un testigo le vio discutir a gritos con {other.shortName} el día de los hechos. ¿Por qué discutieron?";
    }

    /// <summary>
    /// Sin ninguna respuesta (Ollama apagado, modelo o variantes mal escritos) la sonda falla en vez de dar un informe vacío.
    /// </summary>
    public static int ExitCode(int answered)
    {
        return answered == 0 ? 2 : 0;
    }

    public static void RunFromCommandLine()
    {
        int exitCode = 0;
        try
        {
            string[] args = Environment.GetCommandLineArgs();
            ClueCalibrator.Options options = ClueCalibrator.ParseArgs(args);
            if (!args.Contains("-tries"))
                options.tries = 2;
            exitCode = ExitCode(Run(options));
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Premisas] Error: {e}");
            exitCode = 2;
        }
        EditorApplication.Exit(exitCode);
    }

    public static int Run(ClueCalibrator.Options options)
    {
        var rows = new List<(string variant, string character, string question, string answer, bool denied)>();
        using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(180) })
        {
            foreach (string id in options.variantIds)
            {
                if (!CaseLibrary.TryFind(id, out StoryData story, out VariantData variant))
                {
                    Console.WriteLine($"[Premisas] Variante desconocida: {id}");
                    continue;
                }
                foreach (CharacterData character in story.cast)
                {
                    string system = PromptBuilder.Build(story, variant, character.id, 2, new ClueData[0], new ClueData[0]);
                    string question = Question(story, character.id);
                    for (int t = 0; t < options.tries; t++)
                    {
                        string answer;
                        try
                        {
                            answer = ClueCalibrator.Chat(client, options, system,
                                new List<ChatMessage> { new ChatMessage { role = "user", content = question } });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[Premisas] Petición fallida: {e.GetBaseException().Message}");
                            continue;
                        }
                        rows.Add((variant.id, character.id, question, answer, Denies(answer)));
                        Console.WriteLine($"[Premisas] {variant.id} {character.id}: {(rows[rows.Count - 1].denied ? "niega" : "ACEPTA")}");
                    }
                }
            }
        }

        int accepted = rows.Count(r => !r.denied);
        var sb = new StringBuilder();
        sb.AppendLine("# Sonda de premisas falsas");
        sb.AppendLine();
        sb.AppendLine($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm} · Modelo: `{options.model}` · Respuestas: {rows.Count}");
        sb.AppendLine();
        sb.AppendLine($"**Aceptan la premisa falsa:** {accepted}/{rows.Count} ({(rows.Count == 0 ? 0 : 100 * accepted / rows.Count)} %)");
        sb.AppendLine();
        sb.AppendLine("## Respuestas que la aceptan (revisar a mano: el clasificador es aproximado)");
        foreach (var r in rows.Where(r => !r.denied))
            sb.AppendLine($"- {r.variant} · {r.character}: {EmotionParser.Parse(r.answer).text?.Replace("\n", " ")}");
        sb.AppendLine();
        sb.AppendLine("## Muestra de respuestas que la niegan");
        foreach (var r in rows.Where(r => r.denied).Take(15))
            sb.AppendLine($"- {r.variant} · {r.character}: {EmotionParser.Parse(r.answer).text?.Replace("\n", " ")}");

        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
        File.WriteAllText(ReportPath, sb.ToString(), new UTF8Encoding(false));
        Console.WriteLine($"[Premisas] Aceptan {accepted}/{rows.Count}. Informe: {Path.GetFullPath(ReportPath)}");
        return rows.Count;
    }
}
