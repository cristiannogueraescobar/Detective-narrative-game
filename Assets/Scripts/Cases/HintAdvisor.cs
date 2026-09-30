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
/// Qué ayudas se han dado ya en esta partida (pista → nivel).
/// </summary>
public class HintMemory
{
    public readonly Dictionary<string, int> given = new Dictionary<string, int>();
    public int count;
}

public static class HintAdvisor
{
    public static Hint Next(StoryData story, InvestigationState state, IEnumerable<string> unlocked, HintMemory memory)
    {
        VariantData v = state.Variant;
        var available = new HashSet<string>(unlocked);
        List<ClueData> pending = v.clues.Where(c => !state.IsDiscovered(c.id)).ToList();

        // Orden: inocentes antes que el culpable, lo que no es decisivo antes que la ⚡, lo abierto antes que el secreto
        List<ClueData> candidates = pending.Where(c => available.Contains(c.holder))
            .OrderBy(c => c.holder == v.culpritId ? 1 : 0)
            .ThenBy(c => c.exposesLie ? 1 : 0)
            .ThenBy(c => c.isSecret ? 1 : 0)
            .ToList();

        memory.count++;
        if (candidates.Count == 0)
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
            text = question != null ? $"Prueba a preguntarle a {who}: «{question}»" : $"Insiste con {who}: hay algo que no te ha contado."
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
