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

    // "las seis y media", "la una en punto", "las cinco menos cuarto": en los textos conocidos las horas a veces van en letra
    private static readonly string[] HourWords = { "doce", "una", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve", "diez", "once" };
    private static readonly Regex WordTime = new Regex(
        @"\bla(?:s)? (doce|una|dos|tres|cuatro|cinco|seis|siete|ocho|nueve|diez|once)(?: (y media|y cuarto|menos cuarto|en punto))?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static HashSet<int> MinutesIn(string text)
    {
        var result = new HashSet<int>();
        foreach (Match m in DigitTime.Matches(text ?? ""))
            result.Add(Minutes(m));
        foreach (Match m in WordTime.Matches(text ?? ""))
        {
            int hour = System.Array.IndexOf(HourWords, m.Groups[1].Value.ToLowerInvariant()); // 0..11
            int minutes = hour * 60;
            switch (m.Groups[2].Value.ToLowerInvariant())
            {
                case "y media": minutes += 30; break;
                case "y cuarto": minutes += 15; break;
                case "menos cuarto": minutes -= 15; break;
            }
            // Sin contexto no se sabe si es de día o de noche: valen las dos lecturas
            result.Add(((minutes % 1440) + 1440) % 1440);
            result.Add((minutes + 720) % 1440);
        }
        return result;
    }

    private static int Minutes(Match m)
    {
        return int.Parse(m.Groups[1].Value) * 60 + int.Parse(m.Groups[2].Value);
    }
}
