using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

/// <summary>
/// JUGADOR BOT
/// qwen hace de detective y juega partidas completas con la lógica real del juego (AIConversationManager,
/// detección de pistas, contradicciones, desbloqueos, días y preguntas, cálculo del final). Solo ve lo que ve
/// un jugador: el parte del caso y su libreta. Otra instancia del mismo modelo hace de sospechosos, como en juego.
///
/// Batchmode: Unity.exe -batchmode -nographics -projectPath . -executeMethod BotPlayer.RunFromCommandLine
///            [-variants 1A,2C] [-games 2] [-seed 7] [-ollama http://localhost:11434] [-model qwen2.5:7b-instruct]
/// Informe: Logs/bot-playthroughs.md · transcripciones: Logs/bot/&lt;variante&gt;_&lt;n&gt;.md · progreso: Logs/bot-progress.txt
/// </summary>
public static class BotPlayer
{
    public const string ReportPath = "Logs/bot-playthroughs.md";
    public const string TranscriptFolder = "Logs/bot";
    public const string ProgressPath = "Logs/bot-progress.txt";
    private const int QuestionsPerDay = 5;
    private const int MaxDays = 7;
    private const int EarliestAccusationDay = 3;
    private const float DetectiveTemperature = 0.8f;

    public class Options
    {
        public List<string> variantIds = new List<string>();
        public int games = 2;
        public int seed = 7;
        public string ollamaUrl = "http://localhost:11434";
        public string model = new OllamaSettings().model;
    }

    public class Turn
    {
        public int day;
        public string suspectId;
        public string question;
        public string evidenceId;
        public string answer;
        public Emotion emotion;
        public List<string> newClues = new List<string>();
        public List<PlaythroughChecks.Finding> findings = new List<PlaythroughChecks.Finding>();
    }

    public class Game
    {
        public string variantId;
        public int index;
        public Ending ending;
        public string accusedId;
        public string culpritId;
        public int questionsUsed;
        public int daysUsed;
        public int cluesFound;
        public int cluesTotal;
        public int contradictions;
        public int evidence;
        public string accusationReason;
        public string error;
        public List<Turn> turns = new List<Turn>();
        public Dictionary<Emotion, int> emotions = new Dictionary<Emotion, int>();
    }

    // ---------- Entrada ----------

    public static Options ParseArgs(string[] args)
    {
        var options = new Options();
        for (int i = 0; i < args.Length - 1; i++)
        {
            switch (args[i])
            {
                case "-variants": options.variantIds = args[i + 1].Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToList(); break;
                case "-games": options.games = Math.Max(1, int.Parse(args[i + 1])); break;
                case "-seed": options.seed = int.Parse(args[i + 1]); break;
                case "-ollama": options.ollamaUrl = args[i + 1]; break;
                case "-model": options.model = args[i + 1]; break;
            }
        }
        if (options.variantIds.Count == 0)
            options.variantIds = CaseLibrary.AllVariants().Select(p => p.variant.id).ToList();
        return options;
    }

    public static void RunFromCommandLine()
    {
        int code = 0;
        try
        {
            Options options = ParseArgs(Environment.GetCommandLineArgs());
            Run(options);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            code = 1;
        }
        EditorApplication.Exit(code);
    }

    public static List<Game> Run(Options options)
    {
        Directory.CreateDirectory(TranscriptFolder);
        var games = new List<Game>();
        var random = new System.Random(options.seed);

        using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(240) })
        {
            foreach (string variantId in options.variantIds)
            {
                for (int n = 1; n <= options.games; n++)
                {
                    Progress($"{DateTime.Now:HH:mm:ss} {variantId} partida {n}/{options.games} ...");
                    Game game;
                    try
                    {
                        game = Play(client, options, variantId, n, random);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        game = new Game { variantId = variantId, index = n, error = e.GetBaseException().Message };
                    }
                    games.Add(game);
                    WriteTranscript(game);
                    WriteReport(options, games);
                    Progress($"{DateTime.Now:HH:mm:ss} {variantId} partida {n}: {(game.error ?? game.ending.ToString())} · {game.questionsUsed} preguntas · pistas {game.cluesFound}/{game.cluesTotal}");
                }
            }
        }

        WriteReport(options, games);
        return games;
    }

    private static void Progress(string line)
    {
        Debug.Log("[Bot] " + line);
        File.AppendAllText(ProgressPath, line + "\n");
    }

    // ---------- Una partida ----------

    private class SyncProvider : ILLMProvider
    {
        private readonly HttpClient client;
        private readonly Options options;
        public SyncProvider(HttpClient client, Options options) { this.client = client; this.options = options; }
        public string DisplayName => "Ollama (bot)";
        public Task WarmUpAsync() => Task.CompletedTask;

        public Task<LLMResult> SendAsync(string systemPrompt, IReadOnlyList<ChatMessage> history, int maxTokens, float temperature)
        {
            try
            {
                string text = Chat(client, options, systemPrompt, history.ToList(), temperature, maxTokens, json: false);
                return Task.FromResult(LLMResult.Ok(text));
            }
            catch (Exception e)
            {
                return Task.FromResult(LLMResult.Fail(e.GetBaseException().Message));
            }
        }
    }

    private static Game Play(HttpClient client, Options options, string variantId, int index, System.Random random)
    {
        if (!CaseLibrary.TryFind(variantId, out StoryData story, out VariantData variant))
            throw new ArgumentException($"Variante desconocida: {variantId}");

        var host = new GameObject("BotPlayer") { hideFlags = HideFlags.HideAndDontSave };
        try
        {
            var manager = host.AddComponent<AIConversationManager>();
            manager.UseProvider(new SyncProvider(client, options));
            manager.StartCase(story, variant);

            var unlocked = new List<string>(story.cast.Where(c => c.startsUnlocked).Select(c => c.id));
            var game = new Game
            {
                variantId = variantId, index = index, culpritId = variant.culpritId, cluesTotal = variant.clues.Count
            };

            var turnClues = new List<string>();
            manager.OnClueRevealed += clue => turnClues.Add(clue.id);
            manager.OnCharacterMentioned += id => { if (!unlocked.Contains(id)) unlocked.Add(id); };

            var lastAnswer = new Dictionary<string, string>();
            string accused = null;

            for (int day = 1; day <= MaxDays && accused == null; day++)
            {
                game.daysUsed = day;
                if (day >= GameManager.SafetyUnlockDay)
                    foreach (CharacterData c in story.cast.Where(c => !unlocked.Contains(c.id)))
                        unlocked.Add(c.id);

                for (int q = 0; q < QuestionsPerDay && accused == null; q++)
                {
                    Decision decision = Decide(client, options, story, manager, unlocked, game, day, q, random);
                    if (decision.accuse != null && day >= EarliestAccusationDay)
                    {
                        accused = decision.accuse;
                        game.accusationReason = decision.reason;
                        break;
                    }

                    ClueData shown = decision.evidence != null ? variant.Clue(decision.evidence) : null;
                    turnClues.Clear();
                    LLMResult result = manager.AskSuspect(decision.suspect, decision.question, day, shown).GetAwaiter().GetResult();
                    if (!result.Success)
                        throw new Exception("Ollama: " + result.ErrorMessage);

                    Emotion emotion = manager.CurrentEmotion(decision.suspect);
                    game.emotions[emotion] = game.emotions.TryGetValue(emotion, out int count) ? count + 1 : 1;

                    string sheet = PromptBuilder.Build(story, variant, decision.suspect, day,
                        manager.State.ShownTo(decision.suspect).Select(variant.Clue).ToList(), new ClueData[0]) + "\n" + CaseBriefing.Format(story);
                    lastAnswer.TryGetValue(decision.suspect, out string previous);

                    game.turns.Add(new Turn
                    {
                        day = day, suspectId = decision.suspect, question = decision.question, evidenceId = decision.evidence,
                        answer = result.Text, emotion = emotion, newClues = new List<string>(turnClues),
                        findings = PlaythroughChecks.Check(result.Text, sheet, decision.suspect == variant.culpritId,
                            manager.State.ContradictionClueIds.Count, manager.State.ShownTo(decision.suspect).Count(), previous)
                    });
                    lastAnswer[decision.suspect] = result.Text;
                    game.questionsUsed++;
                }
            }

            if (accused == null)
            {
                Decision final = FinalAccusation(client, options, story, manager, unlocked, game);
                accused = final.accuse;
                game.accusationReason = final.reason;
            }

            AccusationResult outcome = manager.State.Accuse(accused);
            game.accusedId = accused;
            game.ending = outcome.ending;
            game.cluesFound = manager.State.DiscoveredClueIds.Count;
            game.contradictions = manager.State.ContradictionClueIds.Count;
            game.evidence = outcome.evidence;
            return game;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(host);
        }
    }

    // ---------- El detective ----------

    [Serializable]
    private class Decision
    {
        public string suspect;
        public string question;
        public string evidence;
        public string accuse;
        public string reason;
    }

    private static string DetectivePrompt(StoryData story, AIConversationManager manager, List<string> unlocked)
    {
        string notebook = StripTags(Notebook.Format(story, manager.State, unlocked, manager.Emotions, manager.DescribeContradiction));
        var sb = new StringBuilder();
        sb.AppendLine("Eres un jugador humano de un juego de detectives en español. Interrogas a sospechosos para descubrir al culpable.");
        sb.AppendLine("Haces preguntas cortas y naturales, como las escribiría una persona en el móvil: concretas (horas, lugares, objetos, relaciones).");
        sb.AppendLine("A veces muestras una pista ya descubierta a quien crees que miente para ver cómo reacciona.");
        sb.AppendLine();
        sb.AppendLine("PARTE DEL CASO:");
        sb.AppendLine(StripTags(CaseBriefing.Format(story)));
        sb.AppendLine();
        sb.AppendLine("TU LIBRETA:");
        sb.AppendLine(notebook);
        sb.AppendLine();
        sb.AppendLine("SOSPECHOSOS A LOS QUE PUEDES PREGUNTAR (id: nombre):");
        foreach (CharacterData c in story.cast.Where(c => unlocked.Contains(c.id)))
            sb.AppendLine($"- {c.id}: {c.DisplayName}");
        List<ClueData> clues = manager.State.DiscoveredClueIds.Select(manager.State.Variant.Clue).ToList();
        if (clues.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("PISTAS QUE PUEDES MOSTRAR (id: nombre):");
            foreach (ClueData c in clues)
                sb.AppendLine($"- {c.id}: {c.playerName}");
        }
        return sb.ToString();
    }

    private static Decision Decide(HttpClient client, Options options, StoryData story, AIConversationManager manager,
                                   List<string> unlocked, Game game, int day, int question, System.Random random)
    {
        string system = DetectivePrompt(story, manager, unlocked);
        var recent = game.turns.Skip(Math.Max(0, game.turns.Count - 8))
            .Select(t => $"- A {story.Character(t.suspectId).shortName}: «{t.question}» → «{Truncate(t.answer, 240)}»");

        string ask =
            $"Día {day} de {MaxDays}, pregunta {question + 1} de {QuestionsPerDay} de hoy.\n" +
            "Últimas preguntas y respuestas:\n" + (game.turns.Count == 0 ? "(ninguna)" : string.Join("\n", recent)) + "\n\n" +
            "Decide tu siguiente movimiento. Responde SOLO con un JSON:\n" +
            "{\"suspect\": \"id\", \"question\": \"tu pregunta\", \"evidence\": \"id de pista o null\"}\n" +
            (day >= EarliestAccusationDay
                ? "Si ya estás seguro de quién es el culpable y tienes pruebas, puedes acusar en su lugar: {\"accuse\": \"id\", \"reason\": \"por qué\"}\n"
                : "") +
            "No repitas preguntas ya hechas. Varía de sospechoso si alguien no aporta nada.";

        for (int attempt = 0; attempt < 3; attempt++)
        {
            string raw = Chat(client, options, system, new List<ChatMessage> { new ChatMessage { role = "user", content = ask } },
                DetectiveTemperature, 200, json: true);
            Decision d = ParseDecision(raw);
            if (d == null)
                continue;

            if (d.accuse != null && unlocked.Contains(d.accuse))
                return d;

            d.accuse = null;
            if (d.suspect == null || !unlocked.Contains(d.suspect) || string.IsNullOrWhiteSpace(d.question))
                continue;
            if (d.evidence != null && !manager.State.DiscoveredClueIds.Contains(d.evidence))
                d.evidence = null;
            return d;
        }

        // El modelo no dio un JSON válido: pregunta genérica a un sospechoso al azar (se anota en el informe)
        string fallback = unlocked[random.Next(unlocked.Count)];
        return new Decision { suspect = fallback, question = "¿Qué hizo usted aquella noche, paso a paso?" };
    }

    private static Decision FinalAccusation(HttpClient client, Options options, StoryData story, AIConversationManager manager,
                                            List<string> unlocked, Game game)
    {
        string system = DetectivePrompt(story, manager, unlocked);
        string summary = string.Join("\n", game.turns.Select(t => $"- {story.Character(t.suspectId).shortName}: «{Truncate(t.answer, 160)}»").TakeLast(20));
        string ask = "Se acabó el tiempo: tienes que acusar a uno de los sospechosos. Respuestas más recientes:\n" + summary +
                     "\n\nResponde SOLO con un JSON: {\"accuse\": \"id\", \"reason\": \"por qué\"}";

        for (int attempt = 0; attempt < 3; attempt++)
        {
            Decision d = ParseDecision(Chat(client, options, system,
                new List<ChatMessage> { new ChatMessage { role = "user", content = ask } }, 0.3f, 200, json: true));
            if (d?.accuse != null && story.cast.Any(c => c.id == d.accuse))
                return d;
        }
        return new Decision { accuse = unlocked[0], reason = "(sin decisión válida del modelo)" };
    }

    private static Decision ParseDecision(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        try
        {
            string json = raw.Trim();
            int start = json.IndexOf('{');
            int end = json.LastIndexOf('}');
            if (start < 0 || end <= start)
                return null;
            json = json.Substring(start, end - start + 1).Replace(": null", ": \"\"");
            Decision d = JsonUtility.FromJson<Decision>(json);
            if (d == null)
                return null;
            d.suspect = NullIfEmpty(d.suspect);
            d.evidence = NullIfEmpty(d.evidence);
            d.accuse = NullIfEmpty(d.accuse);
            d.question = NullIfEmpty(d.question);
            return d;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) || s == "null" ? null : s.Trim();

    private static string StripTags(string text) => System.Text.RegularExpressions.Regex.Replace(text ?? "", "<[^>]+>", "");

    private static string Truncate(string text, int max) => text == null ? "" : text.Length <= max ? text : text.Substring(0, max) + "…";

    // ---------- Ollama ----------

    [Serializable] private class ChatRequest { public string model; public ChatMessage[] messages; public bool stream; public string format; public ChatOptions options; }
    [Serializable] private class ChatRequestNoFormat { public string model; public ChatMessage[] messages; public bool stream; public ChatOptions options; }
    [Serializable] private class ChatOptions { public float temperature; public int num_predict; public int num_ctx; }
    [Serializable] private class ChatResponse { public ChatMessage message; public string error; }

    private static string Chat(HttpClient client, Options options, string systemPrompt, List<ChatMessage> history,
                               float temperature, int maxTokens, bool json)
    {
        var messages = new List<ChatMessage> { new ChatMessage { role = "system", content = systemPrompt } };
        messages.AddRange(history);
        var chatOptions = new ChatOptions { temperature = temperature, num_predict = maxTokens, num_ctx = OllamaSettings.DefaultNumCtx };

        string body = json
            ? JsonUtility.ToJson(new ChatRequest { model = options.model, messages = messages.ToArray(), stream = false, format = "json", options = chatOptions })
            : JsonUtility.ToJson(new ChatRequestNoFormat { model = options.model, messages = messages.ToArray(), stream = false, options = chatOptions });

        var content = new StringContent(body, Encoding.UTF8, "application/json");
        HttpResponseMessage http = client.PostAsync(options.ollamaUrl.TrimEnd('/') + "/api/chat", content).GetAwaiter().GetResult();
        string text = http.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        ChatResponse parsed = JsonUtility.FromJson<ChatResponse>(text);
        if (!http.IsSuccessStatusCode || !string.IsNullOrEmpty(parsed?.error))
            throw new Exception($"HTTP {(int)http.StatusCode}: {parsed?.error ?? text}");
        return parsed?.message?.content?.Trim() ?? "";
    }

    // ---------- Informes ----------

    private static void WriteTranscript(Game game)
    {
        CaseLibrary.TryFind(game.variantId, out StoryData story, out VariantData variant);
        var sb = new StringBuilder();
        sb.AppendLine($"# Bot · {game.variantId} · partida {game.index}");
        sb.AppendLine();
        if (game.error != null)
            sb.AppendLine($"**Error:** {game.error}");
        sb.AppendLine($"Final: **{game.ending}** · acusado: {game.accusedId} · culpable: {game.culpritId} · preguntas: {game.questionsUsed} · días: {game.daysUsed} · pistas {game.cluesFound}/{game.cluesTotal} · contradicciones {game.contradictions}");
        sb.AppendLine($"Motivo de la acusación: {game.accusationReason}");
        sb.AppendLine();
        foreach (Turn t in game.turns)
        {
            string who = story != null ? story.Character(t.suspectId).shortName : t.suspectId;
            string evidence = t.evidenceId != null ? $" [muestra {t.evidenceId}]" : "";
            sb.AppendLine($"**D{t.day} → {who}{evidence}:** {t.question}");
            sb.AppendLine($"> {t.answer.Replace("\n", " ")} *({t.emotion.ToString().ToLowerInvariant()})*");
            if (t.newClues.Count > 0)
                sb.AppendLine($"  - PISTAS: {string.Join(", ", t.newClues)}");
            foreach (var f in t.findings)
                sb.AppendLine($"  - ⚑ {f.kind}: {f.detail}");
            sb.AppendLine();
        }
        File.WriteAllText(Path.Combine(TranscriptFolder, $"{game.variantId}_{game.index}.md"), sb.ToString());
    }

    public static void WriteReport(Options options, List<Game> games)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Jugador bot: partidas completas");
        sb.AppendLine();
        sb.AppendLine($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm} · Modelo: `{options.model}` · Partidas por variante: {options.games} · Semilla: {options.seed}");
        sb.AppendLine("El detective es qwen con la ficha del caso y la libreta (lo que ve un jugador); los sospechosos, la lógica real del juego.");
        sb.AppendLine();
        sb.AppendLine("| Variante | Partidas | Finales (B/A/I/M) | Preguntas media | Pistas media | Contrad. | IA | Hora inv. | Nombre inv. | Confesión | Incoh. |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|");

        foreach (var group in games.GroupBy(g => g.variantId))
        {
            var ok = group.Where(g => g.error == null).ToList();
            int Count(PlaythroughChecks.Kind k) => ok.Sum(g => g.turns.Sum(t => t.findings.Count(f => f.kind == k)));
            string endings = $"{ok.Count(g => g.ending == Ending.Good)}/{ok.Count(g => g.ending == Ending.Bittersweet)}/{ok.Count(g => g.ending == Ending.Insufficient)}/{ok.Count(g => g.ending == Ending.Bad)}";
            sb.AppendLine($"| {group.Key} | {ok.Count}{(ok.Count < group.Count() ? $" (+{group.Count() - ok.Count} con error)" : "")} | {endings} | " +
                          $"{(ok.Count == 0 ? 0 : ok.Average(g => g.questionsUsed)):F1} | " +
                          $"{(ok.Count == 0 ? 0 : ok.Average(g => g.cluesFound)):F1}/{(ok.Count == 0 ? 0 : ok[0].cluesTotal)} | " +
                          $"{(ok.Count == 0 ? 0 : ok.Average(g => g.contradictions)):F1} | {Count(PlaythroughChecks.Kind.AiBreak)} | {Count(PlaythroughChecks.Kind.InventedTime)} | " +
                          $"{Count(PlaythroughChecks.Kind.InventedName)} | {Count(PlaythroughChecks.Kind.UnmotivatedConfession) + Count(PlaythroughChecks.Kind.FalseConfession)} | {Count(PlaythroughChecks.Kind.Incoherent)} |");
        }

        var all = games.Where(g => g.error == null).ToList();
        int answers = all.Sum(g => g.turns.Count);
        sb.AppendLine();
        sb.AppendLine($"**Total:** {all.Count} partidas · {answers} respuestas · aciertos del culpable {all.Count(g => g.accusedId == g.culpritId)}/{all.Count}");

        var emotionTotals = new Dictionary<Emotion, int>();
        foreach (Game g in all)
            foreach (var pair in g.emotions)
                emotionTotals[pair.Key] = (emotionTotals.TryGetValue(pair.Key, out int c) ? c : 0) + pair.Value;
        if (answers > 0)
            sb.AppendLine("**Estados emocionales:** " + string.Join(" · ", emotionTotals.OrderByDescending(p => p.Value)
                .Select(p => $"{p.Key.ToString().ToLowerInvariant()} {(double)p.Value / answers:P0}")));

        sb.AppendLine();
        sb.AppendLine("## Respuestas marcadas (para revisar a mano)");
        foreach (Game g in all)
        {
            CaseLibrary.TryFind(g.variantId, out StoryData story, out _);
            foreach (Turn t in g.turns.Where(t => t.findings.Count > 0))
            {
                string kinds = string.Join(", ", t.findings.Select(f => $"{f.kind}: {f.detail}"));
                sb.AppendLine($"- **{g.variantId}#{g.index} D{t.day} {story.Character(t.suspectId).shortName}** ({kinds}) — «{t.question}» → «{Truncate(t.answer.Replace("\n", " "), 300)}»");
            }
        }

        foreach (Game g in games.Where(g => g.error != null))
            sb.AppendLine($"- **{g.variantId}#{g.index} ERROR:** {g.error}");

        File.WriteAllText(ReportPath, sb.ToString());
    }
}
