using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using UnityEditor;

/// <summary>
/// SONDA DE PREGUNTAS DE EJEMPLO
/// Cada pregunta candidata para los botones de "Puedes empezar por…" se hace, como primera pregunta, a cada personaje
/// de cada variante; se cuentan las pistas que destapa (con el análisis real del juego). Sirve para elegir las tres
/// que más ayudan a quien no sabe por dónde empezar.
///
/// Batchmode: -executeMethod SuggestionProbe.RunFromCommandLine [-variants 1A,2B] [-tries 1]
/// Informe: Logs/suggestion-probe.md
/// </summary>
public static class SuggestionProbe
{
    public const string ReportPath = "Logs/suggestion-probe.md";

    public static string[] Candidates(string victim)
    {
        string who = string.IsNullOrEmpty(victim) ? "la víctima" : victim;
        return new[]
        {
            "¿Dónde estabas cuando pasó?",
            $"¿Qué relación tenías con {who}?",
            "¿Viste u oíste algo raro?",
            $"¿Cuándo viste o hablaste con {who} por última vez?",
            "¿Quién puede confirmar dónde estabas?"
        };
    }

    public static void RunFromCommandLine()
    {
        int exitCode = 0;
        try
        {
            string[] args = Environment.GetCommandLineArgs();
            ClueCalibrator.Options options = ClueCalibrator.ParseArgs(args);
            if (!args.Contains("-tries"))
                options.tries = 1;
            Run(options);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Sugerencias] Error: {e}");
            exitCode = 2;
        }
        EditorApplication.Exit(exitCode);
    }

    public static void Run(ClueCalibrator.Options options)
    {
        var hits = new Dictionary<int, List<string>>(); // Índice de pregunta → pistas destapadas ("2C_gps")
        int asked = 0;
        using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(180) })
        {
            foreach (string id in options.variantIds)
            {
                if (!CaseLibrary.TryFind(id, out StoryData story, out VariantData variant))
                    continue;
                string[] questions = Candidates(story.victim);
                foreach (CharacterData character in story.cast)
                {
                    string system = PromptBuilder.Build(story, variant, character.id, 1, new ClueData[0], new ClueData[0]);
                    for (int q = 0; q < questions.Length; q++)
                    {
                        for (int t = 0; t < options.tries; t++)
                        {
                            string answer;
                            try
                            {
                                answer = ClueCalibrator.Chat(client, options, system,
                                    new List<ChatMessage> { new ChatMessage { role = "user", content = questions[q] } });
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"[Sugerencias] Petición fallida: {e.GetBaseException().Message}");
                                continue;
                            }
                            asked++;
                            string clean = EmotionParser.Parse(answer).text ?? "";
                            TurnOutcome outcome = TurnAnalyzer.Analyze(story, new InvestigationState(variant), character.id, clean, null);
                            if (!hits.ContainsKey(q))
                                hits[q] = new List<string>();
                            hits[q].AddRange(outcome.newClues.Select(c => c.id));
                            Console.WriteLine($"[Sugerencias] {variant.id} {character.id} P{q + 1}: {outcome.newClues.Count} pistas");
                        }
                    }
                }
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("# Sonda de preguntas de ejemplo");
        sb.AppendLine();
        sb.AppendLine($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm} · Modelo: `{options.model}` · Variantes: {string.Join(",", options.variantIds)} · Intentos: {options.tries} · Respuestas: {asked}");
        sb.AppendLine();
        sb.AppendLine("| Pregunta | Pistas destapadas | Pistas distintas |");
        sb.AppendLine("|---|---|---|");
        string[] labels = Candidates("{víctima}");
        for (int q = 0; q < labels.Length; q++)
        {
            List<string> list = hits.TryGetValue(q, out List<string> l) ? l : new List<string>();
            sb.AppendLine($"| {labels[q]} | {list.Count} | {list.Distinct().Count()} ({string.Join(", ", list.Distinct().OrderBy(x => x))}) |");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
        File.WriteAllText(ReportPath, sb.ToString(), new UTF8Encoding(false));
        Console.WriteLine($"[Sugerencias] Informe: {Path.GetFullPath(ReportPath)}");
    }
}
