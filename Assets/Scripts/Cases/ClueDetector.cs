using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Detección determinista de pistas: normaliza el texto y comprueba grupos de anclas.
/// Un grupo se cumple si aparece cualquiera de sus anclas; la pista se detecta si se cumplen todos los grupos.
/// </summary>
public static class ClueDetector
{
    // 22.35 / 22,35 / 22h35 / 22 : 35 → 22:35
    private static readonly Regex TimePattern = new Regex(@"\b(\d{1,2})\s*[.,h:]\s*(\d{2})\b");
    private static readonly Regex Spaces = new Regex(@"\s+");

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

    public static AnchorTrace Evaluate(string[][] groups, string normalizedText)
    {
        var trace = new AnchorTrace();

        if (groups == null || groups.Length == 0)
            return trace;

        trace.Matched = true;

        foreach (string[] group in groups)
        {
            List<string> found = group.Where(anchor => normalizedText.Contains(Normalize(anchor))).ToList();
            trace.Groups.Add(new GroupTrace { Anchors = group, Found = found });

            if (found.Count == 0)
                trace.Matched = false;
        }

        return trace;
    }

    public static bool MentionsAny(string normalizedText, IEnumerable<string> aliases)
    {
        return aliases != null && aliases.Any(alias => normalizedText.Contains(Normalize(alias)));
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
