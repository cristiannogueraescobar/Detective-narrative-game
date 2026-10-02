using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CALIBRACIÓN DE PISTAS
/// Para cada pista, lanza sus preguntas típicas al portador (con su ficha real) contra Ollama y
/// mide cuántas respuestas detecta el mismo detector del juego. Una pista pasa con ≥ 2/3.
///
/// Menú: Detective/Calibrar pistas
/// Batchmode: Unity.exe -batchmode -nographics -projectPath . -executeMethod ClueCalibrator.RunFromCommandLine
///            [-variants 1A,1B] [-clues 1A_puerta,1B_cena] [-tries 3] [-ollama http://localhost:11434] [-model qwen2.5:7b-instruct]
/// Informe: Logs/clue-calibration.md
///
/// Sonda de precisión (desactivable con -noprecision): preguntas ajenas al caso a cada portador; cualquier
/// pista detectada ahí es una revelación espontánea o un falso positivo del detector, y se lista para revisarla.
/// </summary>
public static class ClueCalibrator
{
    public const string ReportPath = "Logs/clue-calibration.md";
    public const int CalibrationDay = 2;

    public static readonly string[] PrecisionQuestions =
    {
        "¿Cómo se encuentra hoy?",
        "Hábleme un poco de usted."
    };

    public class Options
    {
        public List<string> variantIds = new List<string>();
        public List<string> clueIds = new List<string>(); // Vacío = todas las pistas de las variantes
        // -seed N: la semilla de cada llamada sale de quién contesta (primera línea de su ficha), de la conversación y de
        // cuántas veces se ha hecho ya esa misma: dos versiones dan la misma semilla a la misma pregunta al mismo
        // personaje, aunque una tenga pistas de más antes (con el número de orden, una pista nueva desplazaba todas)
        public int seed = -1;
        public readonly Dictionary<string, int> seedOccurrences = new Dictionary<string, int>();
        public int tries = 3;
        public string ollamaUrl = "http://localhost:11434";
        public string model = new OllamaSettings().model;
        public bool precisionProbe = true;
        public float temperature = AIConversationManager.DefaultTemperature;
    }

    public class Attempt
    {
        public string question;
        public List<string> responses = new List<string>();
        public List<AnchorTrace> traces = new List<AnchorTrace>();
        public bool detected;
        public int firstMatchTurn; // 1 = primer turno; 0 = no detectada
        public string error;
    }

    public class PrecisionHit
    {
        public string variantId;
        public string characterId;
        public string question;
        public string response;
        public List<string> clueIds = new List<string>();
    }

    public class ClueResult
    {
        public string variantId;
        public ClueData clue;
        public List<Attempt> attempts = new List<Attempt>();
        public int Hits => attempts.Count(a => a.detected);
        public int FirstTurnHits => attempts.Count(a => a.firstMatchTurn == 1);
        public int Total => attempts.Count;
        public bool Passed => Passes(Hits, Total);
    }

    // ============================================
    // LÓGICA PURA
    // ============================================

    public static bool Passes(int hits, int total)
    {
        return total > 0 && hits * 3 >= total * 2;
    }

    public static string[] SplitTurns(string question)
    {
        return question.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(t => t.Trim())
                       .Where(t => t.Length > 0)
                       .ToArray();
    }

    public static Options ParseArgs(string[] args)
    {
        var options = new Options();

        for (int i = 0; i < args.Length - 1; i++)
        {
            switch (args[i])
            {
                case "-variants":
                    options.variantIds = args[i + 1].Split(',').Select(v => v.Trim()).Where(v => v.Length > 0).ToList();
                    break;
                case "-clues":
                    options.clueIds = args[i + 1].Split(',').Select(v => v.Trim()).Where(v => v.Length > 0).ToList();
                    break;
                case "-tries":
                    options.tries = int.Parse(args[i + 1]);
                    break;
                case "-ollama":
                    options.ollamaUrl = args[i + 1];
                    break;
                case "-temperature":
                    options.temperature = float.Parse(args[i + 1], System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "-seed":
                    options.seed = int.Parse(args[i + 1]);
                    break;
                case "-model":
                    options.model = args[i + 1];
                    break;
            }
        }

        options.precisionProbe = !args.Contains("-noprecision");

        if (options.variantIds.Count == 0)
            options.variantIds = CaseLibrary.AllVariants().Select(p => p.variant.id).ToList();

        return options;
    }

    // ============================================
    // PUNTOS DE ENTRADA
    // ============================================

    [MenuItem("Detective/Calibrar pistas")]
    public static void RunMenu()
    {
        Options options = ParseArgs(new string[0]);

        try
        {
            List<ClueResult> results = Run(options, (label, progress) =>
                EditorUtility.DisplayCancelableProgressBar("Calibrando pistas", label, progress));
            int failed = results.Count(r => !r.Passed);
            Debug.Log($"[Calibración] {results.Count - failed}/{results.Count} pistas ≥ 2/3. Informe: {Path.GetFullPath(ReportPath)}");
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
            Options options = ParseArgs(Environment.GetCommandLineArgs());
            List<ClueResult> results = Run(options, (label, progress) =>
            {
                Console.WriteLine($"[Calibración] {progress:P0} {label}");
                return false;
            });
            int failed = results.Count(r => !r.Passed);
            Console.WriteLine($"[Calibración] {results.Count - failed}/{results.Count} pistas ≥ 2/3. Informe: {Path.GetFullPath(ReportPath)}");
            exitCode = failed == 0 ? 0 : 1;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Calibración] Error: {e}");
            exitCode = 2;
        }

        EditorApplication.Exit(exitCode);
    }

    // ============================================
    // EJECUCIÓN
    // ============================================

    /// <summary>
    /// 'progress' devuelve true si el usuario cancela.
    /// </summary>
    public static List<ClueResult> Run(Options options, Func<string, float, bool> progress)
    {
        var results = new List<ClueResult>();

        var work = new List<(StoryData story, VariantData variant, ClueData clue)>();
        foreach (string id in options.variantIds)
        {
            if (!CaseLibrary.TryFind(id, out StoryData story, out VariantData variant))
            {
                Debug.LogWarning($"[Calibración] Variante {id} no registrada");
                continue;
            }
            // Una entrada por portador: con dos, cada uno se calibra con sus preguntas y sale en su fila
            work.AddRange(variant.clues
                .Where(c => options.clueIds.Count == 0 || options.clueIds.Contains(c.id))
                .SelectMany(c => c.Holders.Select(h => (story, variant, c.ForHolder(h)))));
        }

        var holders = work.Select(w => (w.story, w.variant, w.clue.holder)).Distinct().ToList();
        var precisionHits = new List<PrecisionHit>();
        int precisionTotal = options.precisionProbe ? holders.Count * PrecisionQuestions.Length * options.tries : 0;

        int totalAttempts = work.Sum(w => w.clue.calibrationQuestions.Length) * options.tries + precisionTotal;
        int done = 0;

        using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(180) })
        {
            foreach (var (story, variant, clue) in work)
            {
                var result = new ClueResult { variantId = variant.id, clue = clue };
                string systemPrompt = PromptBuilder.Build(story, variant, clue.holder, CalibrationDay,
                    new ClueData[0], new ClueData[0]);

                foreach (string question in clue.calibrationQuestions)
                {
                    for (int t = 0; t < options.tries; t++)
                    {
                        bool cancelled = progress($"{clue.id} · {question}", (float)done / Math.Max(1, totalAttempts));
                        if (cancelled)
                        {
                            WriteReport(options, results, precisionHits, precisionTotal);
                            return results;
                        }

                        result.attempts.Add(RunAttempt(client, options, systemPrompt, clue, question));
                        done++;
                    }
                }

                results.Add(result);
                Debug.Log($"[Calibración] {clue.id}: {result.Hits}/{result.Total} {(result.Passed ? "OK" : "FALLA")}");
            }

            if (options.precisionProbe)
            {
                foreach (var (story, variant, holder) in holders)
                {
                    string systemPrompt = PromptBuilder.Build(story, variant, holder, CalibrationDay,
                        new ClueData[0], new ClueData[0]);
                    List<ClueData> ownClues = variant.clues.Where(c => c.HeldBy(holder)).Select(c => c.ForHolder(holder)).ToList();

                    foreach (string question in PrecisionQuestions)
                    {
                        for (int t = 0; t < options.tries; t++)
                        {
                            if (progress($"precisión · {variant.id} {holder} · {question}", (float)done / Math.Max(1, totalAttempts)))
                            {
                                WriteReport(options, results, precisionHits, precisionTotal);
                                return results;
                            }

                            PrecisionHit hit = RunPrecision(client, options, systemPrompt, variant.id, holder, ownClues, question);
                            if (hit != null)
                                precisionHits.Add(hit);
                            done++;
                        }
                    }
                }

                Debug.Log($"[Calibración] Sonda de precisión: {precisionHits.Count}/{precisionTotal} respuestas revelaron alguna pista");
            }
        }

        WriteReport(options, results, precisionHits, precisionTotal);
        return results;
    }

    private static PrecisionHit RunPrecision(HttpClient client, Options options, string systemPrompt, string variantId,
                                             string holder, List<ClueData> ownClues, string question)
    {
        string response;
        try
        {
            response = Chat(client, options, systemPrompt,
                new List<ChatMessage> { new ChatMessage { role = "user", content = question } });
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Calibración] Sonda de precisión fallida: {e.GetBaseException().Message}");
            return null;
        }

        string normalized = ClueDetector.Normalize(response);
        List<string> matched = ownClues.Where(c => ClueDetector.Evaluate(c.anchors, normalized).Matched).Select(c => c.id).ToList();

        return matched.Count == 0 ? null : new PrecisionHit
        {
            variantId = variantId, characterId = holder, question = question, response = response, clueIds = matched
        };
    }

    private static Attempt RunAttempt(HttpClient client, Options options, string systemPrompt, ClueData clue, string question)
    {
        var attempt = new Attempt { question = question };
        var history = new List<ChatMessage>();

        foreach (string turn in SplitTurns(question))
        {
            history.Add(new ChatMessage { role = "user", content = turn });

            string response;
            try
            {
                response = Chat(client, options, systemPrompt, history);
            }
            catch (Exception e)
            {
                attempt.error = e.GetBaseException().Message;
                return attempt;
            }

            history.Add(new ChatMessage { role = "assistant", content = response });

            AnchorTrace trace = ClueDetector.Evaluate(clue.anchors, ClueDetector.Normalize(response));
            attempt.responses.Add(response);
            attempt.traces.Add(trace);

            if (trace.Matched && !attempt.detected)
                attempt.firstMatchTurn = attempt.responses.Count;
            attempt.detected |= trace.Matched;
        }

        return attempt;
    }

    // ============================================
    // OLLAMA (HttpClient: funciona en batchmode sin bucle de actualización del editor)
    // ============================================

    [Serializable]
    private class ChatRequest
    {
        public string model;
        public ChatMessage[] messages;
        public bool stream;
        public ChatOptions options;
    }

    [Serializable]
    private class ChatOptions
    {
        public float temperature;
        public int num_predict;
        public int num_ctx;
    }

    [Serializable]
    private class SeededChatRequest
    {
        public string model;
        public ChatMessage[] messages;
        public bool stream;
        public SeededChatOptions options;
    }

    [Serializable]
    private class SeededChatOptions
    {
        public float temperature;
        public int num_predict;
        public int num_ctx;
        public int seed;
    }

    [Serializable]
    private class ChatResponse
    {
        public ChatMessage message;
        public string error;
    }

    /// <summary>Semilla estable para una llamada (ver Options.seed).</summary>
    public static int SeedFor(Options options, string systemPrompt, IEnumerable<ChatMessage> history)
    {
        string who = (systemPrompt ?? "").Split('\n')[0];
        string key = who + "|" + string.Join("|", history.Select(m => m.role + ":" + m.content));
        options.seedOccurrences.TryGetValue(key, out int n);
        options.seedOccurrences[key] = n + 1;
        unchecked
        {
            uint hash = 2166136261; // FNV-1a: igual en cualquier proceso (string.GetHashCode no lo garantiza)
            foreach (char c in key)
                hash = (hash ^ c) * 16777619;
            return (int)((uint)options.seed + hash % 1000000u + (uint)n * 7919u) & int.MaxValue;
        }
    }

    public static string Chat(HttpClient client, Options options, string systemPrompt, List<ChatMessage> history)
    {
        var messages = new List<ChatMessage> { new ChatMessage { role = "system", content = systemPrompt } };
        messages.AddRange(history);

        var request = new ChatRequest
        {
            model = options.model,
            messages = messages.ToArray(),
            stream = false,
            options = new ChatOptions
            {
                temperature = options.temperature,
                num_predict = AIConversationManager.DefaultMaxTokens,
                num_ctx = OllamaSettings.DefaultNumCtx
            }
        };

        string json = JsonUtility.ToJson(request);
        if (options.seed >= 0)
        {
            json = JsonUtility.ToJson(new SeededChatRequest
            {
                model = request.model, messages = request.messages, stream = false,
                options = new SeededChatOptions
                {
                    temperature = request.options.temperature, num_predict = request.options.num_predict,
                    num_ctx = request.options.num_ctx, seed = SeedFor(options, systemPrompt, history)
                }
            });
        }
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage http = client.PostAsync(options.ollamaUrl.TrimEnd('/') + "/api/chat", content).GetAwaiter().GetResult();
        string body = http.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        ChatResponse parsed = JsonUtility.FromJson<ChatResponse>(body);

        if (!http.IsSuccessStatusCode || !string.IsNullOrEmpty(parsed?.error))
            throw new Exception($"HTTP {(int)http.StatusCode}: {parsed?.error ?? body}");

        return parsed?.message?.content?.Trim() ?? "";
    }

    // ============================================
    // INFORME
    // ============================================

    private static void WriteReport(Options options, List<ClueResult> results, List<PrecisionHit> precisionHits, int precisionTotal)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Calibración de pistas");
        sb.AppendLine();
        sb.AppendLine($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm} · Modelo: `{options.model}` · Temperatura: {options.temperature.ToString(System.Globalization.CultureInfo.InvariantCulture)} · Intentos por pregunta: {options.tries} · Umbral: 2/3");

        List<string> allResponses = results.SelectMany(r => r.attempts).SelectMany(a => a.responses)
            .Concat(precisionHits.Select(h => h.response)).ToList();
        Naturalness.Stats natural = Naturalness.Summarize(allResponses);
        int passed = results.Count(r => r.Passed);
        sb.AppendLine();
        sb.AppendLine($"**Resumen:** {passed}/{results.Count} pistas ≥ 2/3 · detección media {(results.Count == 0 ? 0 : results.Average(r => r.Total == 0 ? 0 : (double)r.Hits / r.Total)):P0}");
        sb.AppendLine($"**Naturalidad:** {natural.responses} respuestas · {natural.averageWords:F1} palabras de media · variedad {natural.distinctRatio:P0} · violaciones de estilo {natural.violations}");

        var violating = allResponses.Select(r => (r, v: Naturalness.Violations(r))).Where(x => x.v.Count > 0).Take(8).ToList();
        foreach (var (response, v) in violating)
            sb.AppendLine($"- ({string.Join(", ", v)}) {response.Replace("\n", " ")}");

        // Muestra de respuestas para juzgar el tono a mano
        sb.AppendLine();
        sb.AppendLine("**Muestra de respuestas (primer intento de cada pista):**");
        foreach (ClueResult r in results.Where(r => r.attempts.Count > 0 && r.attempts[0].responses.Count > 0))
            sb.AppendLine($"- {r.clue.id} ({r.clue.holder}): {r.attempts[0].responses[0].Replace("\n", " ")}");
        sb.AppendLine();
        sb.AppendLine("T1 = aciertos ya en el primer turno (en pistas secretas, revisar que sea una confesión y no una negación).");
        sb.AppendLine();
        sb.AppendLine("| Variante | Pista | Portador | Secreta | Aciertos | T1 | Tasa | Estado |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|");

        foreach (ClueResult r in results)
        {
            float rate = r.Total == 0 ? 0f : (float)r.Hits / r.Total;
            sb.AppendLine($"| {r.variantId} | {r.clue.id} | {r.clue.holder} | {(r.clue.isSecret ? "sí" : "")} | {r.Hits}/{r.Total} | {r.FirstTurnHits} | {rate:P0} | {(r.Passed ? "OK" : "**FALLA**")} |");
        }

        if (precisionTotal > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"## Sonda de precisión: {precisionHits.Count}/{precisionTotal} respuestas revelaron alguna pista");
            sb.AppendLine();
            sb.AppendLine($"Preguntas: {string.Join(" · ", PrecisionQuestions)}");

            foreach (PrecisionHit hit in precisionHits)
            {
                sb.AppendLine();
                sb.AppendLine($"- {hit.variantId} · {hit.characterId} · \"{hit.question}\" → {string.Join(", ", hit.clueIds)}");
                sb.AppendLine($"  - {hit.response.Replace("\n", " ")}");
            }
        }

        // Con -clues se detallan todas las pedidas (para ver también los fallos de una que pasa por poco)
        foreach (ClueResult r in results.Where(r => !r.Passed || (r.clue.isSecret && r.FirstTurnHits > 0) || options.clueIds.Count > 0))
        {
            sb.AppendLine();
            sb.AppendLine($"## {r.clue.id} — {r.Hits}/{r.Total}");
            sb.AppendLine();
            sb.AppendLine($"Anclas: `{string.Join(" & ", r.clue.anchors.Select(g => "[" + string.Join("|", g) + "]"))}`");

            // Fallidas y, en secretas, las detectadas en el primer turno (posibles negaciones)
            foreach (Attempt a in r.attempts.Where(a => !a.detected || (r.clue.isSecret && a.firstMatchTurn == 1)))
            {
                sb.AppendLine();
                sb.AppendLine($"**P:** {a.question}");
                if (a.error != null)
                    sb.AppendLine($"- ERROR: {a.error}");
                for (int i = 0; i < a.responses.Count; i++)
                    sb.AppendLine($"- R{i + 1}: {a.responses[i].Replace("\n", " ")}\n  - Traza: `{a.traces[i]}`");
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
        File.WriteAllText(ReportPath, sb.ToString(), new UTF8Encoding(false));
    }
}
