using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CALIBRADOR DE MENTIRAS (sesión C, mentiras de inocentes). Para cada mentiroso de las variantes pedidas (el culpable
/// y el inocente que miente sobre su secreto), con el modelo de verdad:
///   1. ¿La cuenta? Se le pregunta por su noche y por el tema de la pista que rompe su mentira, una pregunta por
///      intento, y se mira si salta su lieAnchors (objetivo: 7 de 10).
///   2. ¿Falsa mentira? Se le enseña la pista que le contradice y se mira que su respuesta NO cuente como mentir otra
///      vez (objetivo: ninguna).
/// Uso: Unity -batchmode -nographics -projectPath . -executeMethod LieCalibrator.RunFromCommandLine
///      -variants 1B,1C -tries 10. Informe: Logs/lie-calibration.md
/// </summary>
public static class LieCalibrator
{
    public const string ReportPath = "Logs/lie-calibration.md";
    public static readonly string[] GenericQuestions =
    {
        "Cuénteme qué hizo esa noche, paso a paso.",
        "¿Dónde estaba usted a la hora de los hechos?"
    };
    private const string ShowQuestion = "Mire esto. ¿Cómo lo explica?";

    // ---------- Lógica pura ----------

    public static IEnumerable<string> Liars(VariantData v) =>
        v.roles.Where(r => r.lieAnchors != null && r.lieAnchors.Length > 0 && !string.IsNullOrEmpty(r.lieQuote)).Select(r => r.characterId);

    public static ClueData BreakingClue(VariantData v, string liar) =>
        v.clues.FirstOrDefault(c => c.exposesLie && c.LiarIn(v) == liar);

    public static IEnumerable<string> Questions(VariantData v, string liar)
    {
        foreach (string q in GenericQuestions)
            yield return q;
        ClueData clue = BreakingClue(v, liar);
        if (clue?.calibrationQuestions == null)
            yield break;
        foreach (string q in clue.calibrationQuestions)
            yield return ClueCalibrator.SplitTurns(q)[0];
    }

    // ---------- Ejecución ----------

    public class LiarResult
    {
        public string variantId, liar;
        public bool culprit;
        public int told, total, falseLies, shownTotal;
        public readonly List<string> samples = new List<string>();
    }

    public static void RunFromCommandLine()
    {
        int exit = 0;
        try
        {
            ClueCalibrator.Options options = ClueCalibrator.ParseArgs(Environment.GetCommandLineArgs());
            List<LiarResult> results = Run(options);
            WriteReport(options, results);
            Console.WriteLine($"[Mentiras] Informe: {Path.GetFullPath(ReportPath)}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Mentiras] Error: {e}");
            exit = 2;
        }
        EditorApplication.Exit(exit);
    }

    public static List<LiarResult> Run(ClueCalibrator.Options options)
    {
        var results = new List<LiarResult>();
        using (var client = new HttpClient { Timeout = TimeSpan.FromMinutes(3) })
        {
            foreach (var (story, variant) in CaseLibrary.AllVariants().Where(p => options.variantIds.Contains(p.variant.id)))
            {
                foreach (string liar in Liars(variant))
                {
                    CharacterRole role = variant.Role(liar);
                    var result = new LiarResult { variantId = variant.id, liar = liar, culprit = liar == variant.culpritId };
                    string prompt = PromptBuilder.Build(story, variant, liar, ClueCalibrator.CalibrationDay, null, null);
                    List<string> questions = Questions(variant, liar).ToList();
                    for (int i = 0; i < options.tries; i++)
                    {
                        string q = questions[i % questions.Count];
                        string answer = ClueCalibrator.Chat(client, options, prompt, new List<ChatMessage> { new ChatMessage { role = "user", content = q } });
                        bool told = ClueDetector.Evaluate(role.lieAnchors, ClueDetector.Normalize(answer), negationGuard: false).Matched;
                        result.total++;
                        if (told)
                            result.told++;
                        if (result.samples.Count < 6)
                            result.samples.Add($"{(told ? "MIENTE" : "no")} · {q} → {answer}");
                        Console.WriteLine($"[Mentiras] {variant.id} {liar} {i + 1}/{options.tries}: {(told ? "miente" : "no")}");
                    }

                    ClueData breaking = BreakingClue(variant, liar);
                    if (breaking != null)
                    {
                        string shownPrompt = PromptBuilder.Build(story, variant, liar, ClueCalibrator.CalibrationDay, new[] { breaking }, null);
                        for (int i = 0; i < options.tries; i++)
                        {
                            string answer = ClueCalibrator.Chat(client, options, shownPrompt,
                                new List<ChatMessage> { new ChatMessage { role = "user", content = $"{ShowQuestion} [Le enseñas: {breaking.summary}]" } });
                            bool again = ClueDetector.Evaluate(role.lieAnchors, ClueDetector.Normalize(answer), negationGuard: false).Matched;
                            result.shownTotal++;
                            if (again)
                            {
                                result.falseLies++;
                                result.samples.Add($"FALSA MENTIRA con la prueba → {answer}");
                            }
                        }
                    }
                    results.Add(result);
                }
            }
        }
        return results;
    }

    private static void WriteReport(ClueCalibrator.Options options, List<LiarResult> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Calibración de mentiras");
        sb.AppendLine();
        sb.AppendLine($"Modelo `{options.model}`, temperatura {options.temperature}, {options.tries} intentos por medida, día {ClueCalibrator.CalibrationDay}. " +
                      "Cuenta: su lieAnchors salta al preguntarle (objetivo ≥ 7/10). Falsa mentira: salta otra vez cuando se le enseña la pista que le contradice (objetivo 0).");
        sb.AppendLine();
        sb.AppendLine("| Variante | Quién | Papel | La cuenta | Falsa mentira con la prueba |");
        sb.AppendLine("|---|---|---|---|---|");
        foreach (LiarResult r in results)
            sb.AppendLine($"| {r.variantId} | {r.liar} | {(r.culprit ? "culpable" : "inocente")} | {r.told}/{r.total} | {r.falseLies}/{r.shownTotal} |");
        sb.AppendLine();
        foreach (LiarResult r in results)
        {
            sb.AppendLine($"## {r.variantId} · {r.liar}");
            foreach (string s in r.samples)
                sb.AppendLine($"- {s.Replace("\n", " ")}");
            sb.AppendLine();
        }
        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
        File.WriteAllText(ReportPath, sb.ToString());
    }
}
