using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Detección determinista de pistas: normaliza el texto y comprueba grupos de anclas.
/// Un grupo se cumple si aparece cualquiera de sus anclas; la pista se detecta si se cumplen todos los grupos.
/// Guardia de negación: no cuenta un ancla precedida de cerca por "no", "nunca", "nadie"... en la misma frase
/// ("no vi ninguna taza de cacao" no revela la taza).
/// </summary>
public static class ClueDetector
{
    // 22.35 / 22,35 / 22h35 / 22 : 35 → 22:35
    private static readonly Regex TimePattern = new Regex(@"\b(\d{1,2})\s*[.,h:]\s*(\d{2})\b");
    private static readonly Regex Spaces = new Regex(@"\s+");

    private static readonly HashSet<string> Negations = new HashSet<string>
    {
        "no", "ni", "nunca", "nadie", "ningun", "ninguna", "ninguno", "nada", "tampoco", "jamas"
    };
    private const int NegationWindow = 4; // Palabras antes del ancla
    private static readonly char[] ClauseBreaks = { '.', ',', ';', ':', '!', '?' };

    public static string Normalize(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        string decomposed = text.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);

        foreach (char c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        string s = TimePattern.Replace(sb.ToString().Normalize(NormalizationForm.FormC), "$1:$2");
        return Spaces.Replace(s, " ").Trim();
    }

    public static AnchorTrace Evaluate(string[][] groups, string normalizedText, bool negationGuard = true)
    {
        var trace = new AnchorTrace();

        if (groups == null || groups.Length == 0)
            return trace;

        trace.Matched = true;

        foreach (string[] group in groups)
        {
            List<string> found = group.Where(anchor => Occurs(normalizedText, NormalizedAnchor(anchor), negationGuard)).ToList();
            trace.Groups.Add(new GroupTrace { Anchors = group, Found = found });

            if (found.Count == 0)
                trace.Matched = false;
        }

        return trace;
    }

    /// <summary>
    /// ¿Aparece el ancla al menos una vez sin negación delante?
    /// </summary>
    private static bool Occurs(string text, string anchor, bool negationGuard)
    {
        if (anchor.Length == 0)
            return false;

        for (int index = text.IndexOf(anchor, System.StringComparison.Ordinal); index >= 0;
             index = text.IndexOf(anchor, index + 1, System.StringComparison.Ordinal))
        {
            if (!negationGuard || !IsNegated(text, index))
                return true;
        }

        return false;
    }

    // Las NegationWindow palabras antes del ancla, dentro de su frase (hasta el último signo de ClauseBreaks).
    // Antes: dos Substring y un Split por cada aparición; ahora se recorren hacia atrás sin copiar la frase.
    private static bool IsNegated(string text, int anchorIndex)
    {
        int start = anchorIndex > 0 ? text.LastIndexOfAny(ClauseBreaks, anchorIndex - 1) + 1 : 0;
        int i = anchorIndex - 1;
        for (int seen = 0; seen < NegationWindow; seen++)
        {
            while (i >= start && IsWordBreak(text[i]))
                i--;
            if (i < start)
                return false;
            int end = i + 1;
            while (i >= start && !IsWordBreak(text[i]))
                i--;
            if (Negations.Contains(text.Substring(i + 1, end - i - 1)))
                return true;
        }
        return false;
    }

    private static bool IsWordBreak(char c) => c == ' ' || c == '¿' || c == '¡';

    public static bool MentionsAny(string normalizedText, IEnumerable<string> aliases)
    {
        return aliases != null && aliases.Any(alias => normalizedText.Contains(NormalizedAnchor(alias)));
    }

    // PERFORMANCE-AUDIT, mejora 3: las anclas y los alias son datos fijos de la historia, pero se normalizaban en cada
    // llamada (~60 por respuesta, 65 µs cada una). Se normalizan una vez y se recuerdan.
    private static readonly Dictionary<string, string> AnchorCache = new Dictionary<string, string>();
    private static readonly object AnchorLock = new object();

    /// <summary>Anclas normalizadas de verdad (no servidas por la caché): para los tests.</summary>
    public static int AnchorNormalizations { get; private set; }

    public static void ResetAnchorCache()
    {
        lock (AnchorLock)
        {
            AnchorCache.Clear();
            AnchorNormalizations = 0;
        }
    }

    private static string NormalizedAnchor(string anchor)
    {
        string key = anchor ?? "";
        lock (AnchorLock)
        {
            if (AnchorCache.TryGetValue(key, out string cached))
                return cached;
        }
        string normalized = Normalize(key);
        lock (AnchorLock)
        {
            if (!AnchorCache.ContainsKey(key))
            {
                if (AnchorCache.Count >= 4096)
                    AnchorCache.Clear();
                AnchorCache[key] = normalized;
                AnchorNormalizations++;
            }
        }
        return normalized;
    }
}

public class AnchorTrace
{
    public bool Matched;
    public List<GroupTrace> Groups = new List<GroupTrace>();

    public override string ToString()
    {
        return string.Join(" & ", Groups.Select(g =>
            $"[{string.Join("|", g.Anchors)}]→{(g.Found.Count > 0 ? string.Join(",", g.Found) : "∅")}"));
    }
}

public class GroupTrace
{
    public string[] Anchors;
    public List<string> Found;
}
