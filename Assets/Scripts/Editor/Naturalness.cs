using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Métricas baratas de naturalidad para comparar configuraciones del modelo:
/// longitud, variedad entre respuestas y violaciones de las reglas de estilo de la ficha.
/// </summary>
public static class Naturalness
{
    public const int MaxSentences = 5; // La ficha pide 2-4 frases; se tolera una de margen

    private static readonly Regex BulletLine = new Regex(@"(^|\n)\s*([-•]|\d+[.)])\s+");
    private static readonly Regex SentenceEnd = new Regex(@"[.!?…]+");
    private static readonly string[] AiMentions = { "inteligencia artificial", "como ia", "soy una ia", "modelo de lenguaje", "asistente virtual" };
    private static readonly string[] MetaLeaks = { "la primera vez lo niego", "si insiste", "si el inspector insiste", "sin rodeos", "esta ficha", "mi ficha" };
    private static readonly Regex English = new Regex(@"\b(the|and|because|she|her|his|was|with|didn't|don't)\b", RegexOptions.IgnoreCase);
    // Hecho copiado tal cual de la ficha, en segunda persona: "Tu madre te dejó..."
    private static readonly Regex CopiedSecondPerson = new Regex(@"\btu \w+ te \w+", RegexOptions.IgnoreCase);

    public class Stats
    {
        public int responses;
        public float averageWords;
        public float distinctRatio;
        public int violations;
    }

    public static List<string> Violations(string response)
    {
        var found = new List<string>();
        string lower = ClueDetector.Normalize(response);

        if (response.Contains("*"))
            found.Add("asteriscos");
        if (BulletLine.IsMatch(response))
            found.Add("lista");
        if (AiMentions.Any(lower.Contains))
            found.Add("menciona IA");
        if (SentenceEnd.Matches(response).Count > MaxSentences)
            found.Add("demasiadas frases");
        if (English.IsMatch(response))
            found.Add("inglés");
        if (MetaLeaks.Any(lower.Contains))
            found.Add("recita la ficha");
        if (CopiedSecondPerson.IsMatch(response))
            found.Add("segunda persona copiada");

        return found;
    }

    public static Stats Summarize(IEnumerable<string> responses)
    {
        List<string> list = responses.ToList();
        if (list.Count == 0)
            return new Stats();

        return new Stats
        {
            responses = list.Count,
            averageWords = (float)list.Average(r => r.Split(new[] { ' ', '\n' }, System.StringSplitOptions.RemoveEmptyEntries).Length),
            distinctRatio = (float)list.Select(ClueDetector.Normalize).Distinct().Count() / list.Count,
            violations = list.Sum(r => Violations(r).Count)
        };
    }
}
