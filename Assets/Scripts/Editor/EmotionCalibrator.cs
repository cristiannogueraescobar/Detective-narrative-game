using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CALIBRACIÓN DE LA ETIQUETA DE ESTADO
/// Para cada personaje de cada variante lanza cuatro tipos de pregunta (neutra, víctima, tema sensible, acusación)
/// y comprueba que la respuesta termina en "[ESTADO: x]" bien formada y coherente con la pregunta.
/// Objetivo: ≥ 8 de cada 10 en ambas cosas.
///
/// Menú: Detective/Calibrar estados emocionales
/// Batchmode: -executeMethod EmotionCalibrator.RunFromCommandLine [-variants 1A,2B] [-tries 1] [-temperature 0.6]
/// Informe: Logs/emotion-calibration.md
/// </summary>
public static class EmotionCalibrator
{
    public const string ReportPath = "Logs/emotion-calibration.md";

    private class Sample
    {
        public string variantId;
        public string characterId;
        public EmotionProbe probe;
        public string question;
        public string response;
        public EmotionParse parse;
        public bool Coherent => parse.emotion.HasValue && EmotionCoherence.IsCoherent(probe, parse.emotion.Value);
    }

    public static bool Passes(int ok, int total)
    {
        return total > 0 && ok * 10 >= total * 8;
    }

    public static string ProbeQuestion(EmotionProbe probe, StoryData story, CharacterRole role)
    {
        switch (probe)
        {
            case EmotionProbe.Victima:
                return $"¿Cómo era {story.victim}?";
            case EmotionProbe.TemaSensible:
                string topic = role.nervousAbout.Split(';', ',')[0].Trim().TrimEnd('.');
                return $"Hablemos de {topic}. ¿Qué me cuenta?";
            case EmotionProbe.Acusacion:
                return "Creo que usted es el culpable. ¿Qué tiene que decir?";
            default:
                return "¿A qué se dedica usted?";
        }
    }

    [MenuItem("Detective/Calibrar estados emocionales")]
    public static void RunMenu()
    {
        try
        {
            Run(ClueCalibrator.ParseArgs(new[] { "-tries", "1" }), (label, p) =>
                EditorUtility.DisplayCancelableProgressBar("Calibrando estados", label, p));
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    public static void RunFromCommandLine()
    {
        int exitCode;
        try
        {
            string[] args = Environment.GetCommandLineArgs();
            ClueCalibrator.Options options = ClueCalibrator.ParseArgs(args);
            if (!args.Contains("-tries"))
                options.tries = 1;

            bool passed = Run(options, (label, p) =>
            {
                Console.WriteLine($"[Estados] {p:P0} {label}");
                return false;
            });
            exitCode = passed ? 0 : 1;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Estados] Error: {e}");
            exitCode = 2;
        }

        EditorApplication.Exit(exitCode);
    }

    /// <summary>
    /// Devuelve true si la etiqueta llega bien formada y coherente en ≥ 80 % de las respuestas.
    /// </summary>
    public static bool Run(ClueCalibrator.Options options, Func<string, float, bool> progress)
    {
        var samples = new List<Sample>();
        var work = new List<(StoryData story, VariantData variant, CharacterData character, EmotionProbe probe)>();

        foreach (string id in options.variantIds)
        {
            if (!CaseLibrary.TryFind(id, out StoryData story, out VariantData variant))
                continue;
            foreach (CharacterData character in story.cast)
                foreach (EmotionProbe probe in Enum.GetValues(typeof(EmotionProbe)))
                    work.Add((story, variant, character, probe));
        }

        int total = work.Count * options.tries;
        int done = 0;

        using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(180) })
        {
            foreach (var (story, variant, character, probe) in work)
            {
                CharacterRole role = variant.Role(character.id);
                string systemPrompt = PromptBuilder.Build(story, variant, character.id, 2, new ClueData[0], new ClueData[0]);
                string question = ProbeQuestion(probe, story, role);

                for (int t = 0; t < options.tries; t++)
                {
                    if (progress($"{variant.id} {character.id} · {probe}", (float)done / Math.Max(1, total)))
                        return WriteReport(options, samples);

                    string response;
                    try
                    {
                        response = ClueCalibrator.Chat(client, options, systemPrompt,
                            new List<ChatMessage> { new ChatMessage { role = "user", content = question } });
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[Estados] Petición fallida: {e.GetBaseException().Message}");
                        done++;
                        continue;
                    }

                    samples.Add(new Sample
                    {
                        variantId = variant.id, characterId = character.id, probe = probe,
                        question = question, response = response, parse = EmotionParser.Parse(response)
                    });
                    done++;
                }
            }
        }

        return WriteReport(options, samples);
    }

    private static bool WriteReport(ClueCalibrator.Options options, List<Sample> samples)
    {
        int wellFormed = samples.Count(s => s.parse.wellFormed);
        int coherent = samples.Count(s => s.Coherent);
        bool passed = Passes(wellFormed, samples.Count) && Passes(coherent, samples.Count);

        var sb = new StringBuilder();
        sb.AppendLine("# Calibración de la etiqueta de estado emocional");
        sb.AppendLine();
        sb.AppendLine($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm} · Modelo: `{options.model}` · Temperatura: {options.temperature.ToString(System.Globalization.CultureInfo.InvariantCulture)} · Respuestas: {samples.Count}");
        sb.AppendLine();
        sb.AppendLine($"**Bien formada:** {wellFormed}/{samples.Count} ({Rate(wellFormed, samples.Count)}) · **Coherente:** {coherent}/{samples.Count} ({Rate(coherent, samples.Count)}) · **Objetivo 8/10:** {(passed ? "OK" : "FALLA")}");
        sb.AppendLine();
        sb.AppendLine("| Tipo de pregunta | Bien formada | Coherente | Estados obtenidos |");
        sb.AppendLine("|---|---|---|---|");

        foreach (var group in samples.GroupBy(s => s.probe))
        {
            string states = string.Join(", ", group.Where(s => s.parse.emotion.HasValue)
                .GroupBy(s => s.parse.emotion.Value).Select(g => $"{g.Key.ToString().ToLowerInvariant()} {g.Count()}"));
            sb.AppendLine($"| {group.Key} | {group.Count(s => s.parse.wellFormed)}/{group.Count()} | {group.Count(s => s.Coherent)}/{group.Count()} | {states} |");
        }

        sb.AppendLine();
        sb.AppendLine("| Historia / variante | Bien formada | Coherente |");
        sb.AppendLine("|---|---|---|");
        foreach (var story in samples.GroupBy(s => s.variantId.Substring(0, 1)).OrderBy(g => g.Key))
        {
            string byProbe = string.Join(" · ", story.GroupBy(s => s.probe).OrderBy(g => g.Key).Select(g => $"{g.Key} {g.Count(s => s.Coherent)}/{g.Count()}"));
            sb.AppendLine($"| **Historia {story.Key}** | {story.Count(s => s.parse.wellFormed)}/{story.Count()} | **{story.Count(s => s.Coherent)}/{story.Count()} ({Rate(story.Count(s => s.Coherent), story.Count())})** · {byProbe} |");
            foreach (var variant in story.GroupBy(s => s.variantId).OrderBy(g => g.Key))
                sb.AppendLine($"| {variant.Key} | {variant.Count(s => s.parse.wellFormed)}/{variant.Count()} | {variant.Count(s => s.Coherent)}/{variant.Count()} |");
        }
        sb.AppendLine();
        sb.AppendLine("## Respuestas mal formadas o incoherentes");
        foreach (Sample s in samples.Where(s => !s.parse.wellFormed || !s.Coherent).Take(25))
        {
            sb.AppendLine();
            sb.AppendLine($"- {s.variantId} · {s.characterId} · {s.probe} · \"{s.question}\" → {(s.parse.emotion?.ToString() ?? "sin estado")}{(s.parse.wellFormed ? "" : " (mal formada)")}");
            sb.AppendLine($"  - {s.response.Replace("\n", " ")}");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
        File.WriteAllText(ReportPath, sb.ToString(), new UTF8Encoding(false));
        Console.WriteLine($"[Estados] Bien formada {wellFormed}/{samples.Count}, coherente {coherent}/{samples.Count}. Informe: {Path.GetFullPath(ReportPath)}");
        return passed;
    }

    private static string Rate(int ok, int total)
    {
        return total == 0 ? "—" : $"{(double)ok / total:P0}";
    }
}
