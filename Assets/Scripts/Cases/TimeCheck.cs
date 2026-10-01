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
        if (string.IsNullOrEmpty(answer))
            return new List<string>();
        MatchCollection times = DigitTime.Matches(answer);
        return times.Count == 0 ? new List<string>() : Collect(times, MinutesIn(known));
    }

    // PERFORMANCE-AUDIT, mejora 2: era lo más caro de cada respuesta (dos expresiones regulares sobre 6,6 KB de ficha e
    // historial, aunque la respuesta no tuviera horas). Ahora: sin horas en la respuesta no se mira nada; con horas, las
    // de la ficha (cambia por sospechoso y día) y las de cada mensaje salen de una caché por texto.
    // Sin lock: solo se usa desde el hilo principal (AskSuspect vuelve al contexto de Unity; el bot y los tests llaman de uno
    // en uno). Si algún día se llama desde otro hilo, hay que protegerla como las cachés de ClueDetector y EmotionParser.
    private static readonly Dictionary<string, HashSet<int>> KnownCache = new Dictionary<string, HashSet<int>>();
    private const int CacheLimit = 256;

    /// <summary>Textos conocidos analizados (no servidos por la caché): para los tests.</summary>
    public static int KnownScans { get; private set; }

    public static void ResetCache()
    {
        KnownCache.Clear();
        KnownScans = 0;
    }

    /// <summary>
    /// Igual que Unknown(answer, ficha + historial juntos), sin concatenar nada y con caché por texto.
    /// </summary>
    public static List<string> Unknown(string answer, string systemPrompt, IEnumerable<string> history)
    {
        if (string.IsNullOrEmpty(answer))
            return new List<string>();
        MatchCollection times = DigitTime.Matches(answer);
        if (times.Count == 0)
            return new List<string>(); // La mayoría de las respuestas: sin horas, nada que comprobar
        var known = new HashSet<int>(Cached(systemPrompt));
        foreach (string message in history)
            known.UnionWith(Cached(message));
        return Collect(times, known);
    }

    private static HashSet<int> Cached(string text)
    {
        text = text ?? "";
        if (!KnownCache.TryGetValue(text, out HashSet<int> minutes))
        {
            if (KnownCache.Count >= CacheLimit)
                KnownCache.Clear();
            KnownScans++;
            minutes = MinutesIn(text);
            KnownCache[text] = minutes;
        }
        return minutes;
    }

    private static List<string> Collect(MatchCollection times, HashSet<int> knownMinutes)
    {
        var result = new List<string>();
        foreach (Match m in times)
        {
            // "5:30" puede ser la de la mañana o la de la tarde: vale si alguna de las dos es conocida
            int minutes = Minutes(m);
            bool known12 = Hour(m) <= 12 && knownMinutes.Contains((minutes + 720) % 1440);
            if (!knownMinutes.Contains(minutes) && !known12 && !result.Contains(m.Value))
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
        {
            result.Add(Minutes(m));
            if (Hour(m) <= 12) // "5:30" en la ficha también cubre "17:30" en la respuesta
                result.Add((Minutes(m) + 720) % 1440);
        }
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

    private static int Hour(Match m) => int.Parse(m.Groups[1].Value);

    private static int Minutes(Match m)
    {
        return int.Parse(m.Groups[1].Value) * 60 + int.Parse(m.Groups[2].Value);
    }
}
