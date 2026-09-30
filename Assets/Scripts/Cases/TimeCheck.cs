using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Horas dichas en una respuesta que no aparecen en ningún texto conocido (la ficha del personaje, el caso y lo que
/// preguntó el inspector). qwen 7B a veces se inventa una hora ("la vi a las 21:45"): eso puede despistar al
/// jugador, así que la respuesta se pide otra vez (ver AIConversationManager).
/// </summary>
public static class TimeCheck
{
    private static readonly Regex DigitTime = new Regex(@"\b([01]?\d|2[0-3])[:.h]([0-5]\d)\b");

    public static List<string> Unknown(string answer, string known)
    {
        var result = new List<string>();
        if (string.IsNullOrEmpty(answer))
            return result;

        HashSet<int> knownMinutes = MinutesIn(known);
        foreach (Match m in DigitTime.Matches(answer))
        {
            if (!knownMinutes.Contains(Minutes(m)) && !result.Contains(m.Value))
                result.Add(m.Value);
        }
        return result;
    }

    public static HashSet<int> MinutesIn(string text)
    {
        var result = new HashSet<int>();
        foreach (Match m in DigitTime.Matches(text ?? ""))
            result.Add(Minutes(m));
        return result;
    }

    private static int Minutes(Match m)
    {
        return int.Parse(m.Groups[1].Value) * 60 + int.Parse(m.Groups[2].Value);
    }
}
