using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Lo que el detective "piensa" cuando el jugador se atasca (botón Pensar de la libreta). Sale de los datos, no del
/// modelo: una pista aún sin encontrar de alguien ya disponible. Primero una idea vaga (quién), después la pregunta
/// concreta (una de las preguntas de calibración de esa pista). Nunca empieza por la pista decisiva (⚡) ni por
/// las que tiene el culpable, para no regalar el caso.
/// </summary>
public class Hint
{
    public string clueId;   // null si no hay pista a la que apuntar
    public string holderId; // A quién preguntar
    public int level;       // 1 vaga, 2 concreta
    public string text;     // Lo que se muestra
    public string question; // Nivel 2: pregunta sugerida (se puede escribir en el campo)
}

/// <summary>
/// Qué ayudas se han dado ya en esta partida (pista → nivel) y cuántas contaban (las que apuntan a una pista).
/// Se guarda con la partida: tras "Continuar", la siguiente ayuda sigue donde se quedó.
/// </summary>
public class HintMemory
{
    public readonly Dictionary<string, int> given = new Dictionary<string, int>();
    public int count;

    /// <summary>
    /// "pista:nivel" por cada ayuda dada (formato del guardado).
    /// </summary>
    public List<string> ToSave()
    {
        return given.Select(kv => kv.Key + ":" + kv.Value).ToList();
    }

    public static HintMemory FromSave(int count, IEnumerable<string> entries)
    {
        var memory = new HintMemory { count = System.Math.Max(0, count) };
        foreach (string entry in entries ?? Enumerable.Empty<string>())
        {
            int colon = entry != null ? entry.LastIndexOf(':') : -1;
            if (colon > 0 && int.TryParse(entry.Substring(colon + 1), out int level))
                memory.given[entry.Substring(0, colon)] = System.Math.Max(1, System.Math.Min(2, level));
        }
        return memory;
    }
}

public static class HintAdvisor
{
    public static Hint Next(StoryData story, InvestigationState state, IEnumerable<string> unlocked, HintMemory memory)
    {
        VariantData v = state.Variant;
        var available = new HashSet<string>(unlocked);
        List<ClueData> pending = v.clues.Where(c => !state.IsDiscovered(c.id)).ToList();

        // Orden: inocentes antes que el culpable, lo que no es decisivo antes que la ⚡, lo abierto antes que el secreto
        // Con dos portadores, se sugiere el que esté disponible (el que no es el culpable, si lo están los dos)
        List<ClueData> candidates = pending.Where(c => c.Holders.Any(available.Contains))
            .Select(c => c.ForHolder(c.Holders.Where(available.Contains)
                .OrderBy(h => h == v.culpritId ? 1 : 0)
                .ThenBy(h => c.ForHolder(h).isSecret ? 1 : 0) // La versión abierta antes que la secreta
                .First()))
            .OrderBy(c => c.holder == v.culpritId ? 1 : 0)
            .ThenBy(c => c.exposesLie ? 1 : 0)
            .ThenBy(c => c.isSecret ? 1 : 0)
            .ToList();

        if (candidates.Count == 0) // Un aviso, no una ayuda: no cuenta para el rango ni se cobra
        {
            return new Hint
            {
                text = pending.Count == 0
                    ? "Ya tienes todo lo que se puede averiguar. Repasa la libreta: es hora de acusar."
                    : "Alguien que aún no conoces sabe algo. Pregunta a los demás quién estaba cerca esa noche."
            };
        }

        // Si ya se dio la idea vaga de una pista que sigue sin salir, ahora la concreta; si no, la vaga de la siguiente
        ClueData clue = candidates.FirstOrDefault(c => memory.given.TryGetValue(c.id, out int level) && level == 1)
                        ?? candidates.FirstOrDefault(c => !memory.given.ContainsKey(c.id))
                        ?? candidates[0];
        int next = memory.given.TryGetValue(clue.id, out int previous) ? System.Math.Min(2, previous + 1) : 1;
        memory.given[clue.id] = next;
        memory.count++;

        string who = story.cast.First(c => c.id == clue.holder).shortName;
        if (next == 1)
            return new Hint { clueId = clue.id, holderId = clue.holder, level = 1, text = $"Quizá {who} sabe más de lo que ha contado." };

        string question = FirstQuestion(clue);
        return new Hint
        {
            clueId = clue.id,
            holderId = clue.holder,
            level = 2,
            question = question,
            text = question != null ? $"Pregúntale a {who}: «{question}»" : $"Insiste con {who}: hay algo que no te ha contado."
        };
    }

    // "¿Qué vio al salir? || ¿Y después?" → "¿Qué vio al salir?"
    private static string FirstQuestion(ClueData clue)
    {
        string raw = clue.calibrationQuestions?.FirstOrDefault(q => !string.IsNullOrWhiteSpace(q));
        if (raw == null)
            return null;
        int cut = raw.IndexOf("||", System.StringComparison.Ordinal);
        return (cut >= 0 ? raw.Substring(0, cut) : raw).Trim();
    }
}
