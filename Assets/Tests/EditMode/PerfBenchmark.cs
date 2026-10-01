using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

/// <summary>
/// Antes y después de las mejoras 2-4 de PERFORMANCE-AUDIT, en la misma ejecución y sobre datos reales (1A, Lucía,
/// día 3, 16 mensajes). "Antes" fuerza el camino antiguo: sin cachés, contexto entero concatenado y escritura síncrona.
/// No es un test de aprobado: escribe Logs/perf-mejoras.md. Explicit: solo se ejecuta si se pide por nombre.
/// </summary>
[Explicit]
public class PerfBenchmark
{
    private const int N = 400;

    private static double Micro(System.Action action, int n = N)
    {
        action(); // Calentar (JIT)
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < n; i++)
            action();
        return sw.Elapsed.TotalMilliseconds * 1000.0 / n;
    }

    [Test]
    public void MedirAntesYDespues()
    {
        Assert.IsTrue(CaseLibrary.TryFind("1A", out StoryData story, out VariantData v));
        string suspect = v.roles[1].characterId;
        string prompt = PromptBuilder.Build(story, v, suspect, 3, new ClueData[0], new ClueData[0]);
        var history = new List<string>();
        for (int i = 0; i < 8; i++)
        {
            history.Add($"¿Dónde estaba usted a las 22:{10 + i}? ¿Vio algo raro esa noche, la pregunta número {i}?");
            history.Add($"Estaba en casa, inspector, como siempre. Me acosté sobre las once y no oí nada raro. [ESTADO: nervioso] {i}");
        }
        string withTime = "Llegué a casa a las 22:15 y me fui a dormir. [ESTADO: tranquilo]";
        string noTime = "No sé de qué me habla, inspector. Yo no vi nada. [ESTADO: tranquilo]";
        string context = prompt + "\n" + string.Join("\n", history);
        var rows = new List<string>();

        // Mejora 2: TimeCheck
        double tcBefore = Micro(() => TimeCheck.Unknown(EmotionParser.Parse(withTime).text, context));
        double tcBeforeNoTime = Micro(() => { TimeCheck.MinutesIn(context); TimeCheck.Unknown(noTime, context); });
        TimeCheck.ResetCache();
        double tcAfter = Micro(() => TimeCheck.Unknown(withTime, prompt, history));
        double tcAfterNoTime = Micro(() => TimeCheck.Unknown(noTime, prompt, history));
        rows.Add($"| `TimeCheck.Unknown`, respuesta con hora ({context.Length} caracteres de contexto) | {tcBefore:F1} µs | {tcAfter:F1} µs |");
        rows.Add($"| `TimeCheck.Unknown`, respuesta sin hora (la mayoría) | {tcBeforeNoTime:F1} µs | {tcAfterNoTime:F1} µs |");

        // Mejora 3: EmotionParser (4 análisis del mismo texto por respuesta) y anclas
        string raw = "Esa noche estuve en casa, se lo juro. Llegué a las 22:15 y me acosté. [ESTADO: nervioso]";
        double epBefore = Micro(() => { for (int k = 0; k < 4; k++) { EmotionParser.ResetCache(); EmotionParser.Parse(raw); } });
        EmotionParser.ResetCache();
        double epAfter = Micro(() => { for (int k = 0; k < 4; k++) EmotionParser.Parse(raw); });
        rows.Add($"| `EmotionParser.Parse` × 4 sobre la misma respuesta | {epBefore:F1} µs | {epAfter:F1} µs |");

        string answer = ClueDetector.Normalize("Vi a la madre sentada junto a la cama, muy quieta, sin llamar a nadie hasta las 23:15.");
        double cdBefore = Micro(() => { ClueDetector.ResetAnchorCache(); foreach (ClueData c in v.clues) ClueDetector.Evaluate(c.anchors, answer); });
        ClueDetector.ResetAnchorCache();
        double cdAfter = Micro(() => { foreach (ClueData c in v.clues) ClueDetector.Evaluate(c.anchors, answer); });
        rows.Add($"| `ClueDetector.Evaluate` × {v.clues.Count} pistas | {cdBefore:F1} µs | {cdAfter:F1} µs |");

        // Mejora 4: guardado
        SaveData data = BigSave(v);
        string pretty = UnityEngine.JsonUtility.ToJson(data, true);
        string compact = SaveSystem.Serialize(data);
        string dir = Path.Combine(Path.GetTempPath(), "detective-perf-" + System.Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, "antes.json");
        double saveBefore = Micro(() =>
        {
            File.WriteAllText(path + ".tmp", UnityEngine.JsonUtility.ToJson(data, true));
            if (File.Exists(path)) File.Delete(path);
            File.Move(path + ".tmp", path);
        }, 100);
        SaveSystem.DirectoryOverride = dir;
        double saveAfter = Micro(() => SaveSystem.Save(data), 100);
        SaveSystem.Flush();
        SaveSystem.DirectoryOverride = null;
        Directory.Delete(dir, true);
        rows.Add($"| Guardar: JSON | {pretty.Length / 1024.0:F1} KB | {compact.Length / 1024.0:F1} KB |");
        rows.Add($"| Guardar: tiempo en el hilo principal (serializar + escribir) | {saveBefore:F0} µs | {saveAfter:F0} µs |");

        var sb = new StringBuilder();
        sb.AppendLine("# Mejoras 2-4: antes y después (misma ejecución, EditMode, PC de desarrollo)");
        sb.AppendLine();
        sb.AppendLine("| Operación | Antes | Después |");
        sb.AppendLine("|---|---|---|");
        rows.ForEach(r => sb.AppendLine(r));
        Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/perf-mejoras.md", sb.ToString());
        UnityEngine.Debug.Log(sb.ToString());
    }

    // Una partida grande, como la de la auditoría (35 turnos)
    private static SaveData BigSave(VariantData v)
    {
        var data = new SaveData { variantId = v.id, day = 5, questionsUsedToday = 2, currentSuspect = v.roles[0].characterId };
        foreach (CharacterRole r in v.roles)
        {
            data.unlocked.Add(r.characterId);
            var h = new SaveData.History { characterId = r.characterId };
            var conv = new SaveData.Conversation { characterId = r.characterId };
            for (int i = 0; i < 9; i++)
            {
                h.messages.Add(new ChatMessage { role = "user", content = $"Pregunta {i}: ¿dónde estaba usted esa noche entre las diez y las once, y quién puede confirmarlo?" });
                h.messages.Add(new ChatMessage { role = "assistant", content = $"Respuesta {i}: estaba en casa, inspector, viendo la televisión con la puerta cerrada. No oí nada raro hasta que gritaron. [ESTADO: nervioso]" });
                conv.entries.Add(new ChatEntry { kind = ChatEntryKind.Player, text = h.messages[h.messages.Count - 2].content });
                conv.entries.Add(new ChatEntry { kind = ChatEntryKind.Suspect, text = h.messages[h.messages.Count - 1].content });
            }
            data.histories.Add(h);
            data.conversations.Add(conv);
        }
        data.discovered.AddRange(v.clues.Take(3).Select(c => c.id));
        return data;
    }
}
