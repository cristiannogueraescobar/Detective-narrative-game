using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Problemas de una respuesta de sospechoso que el jugador bot anota (heurísticas; el informe muestra el texto
/// para revisarlo a mano):
///   - ruptura de personaje: dice que es una IA o un asistente;
///   - hora inventada: da una hora con cifras que no está en su ficha ni en el parte del caso;
///   - nombre inventado: nombra a alguien con mayúscula que no aparece en su ficha;
///   - confesión sin motivo: el culpable confiesa sin contradicciones ni pruebas; o confiesa un inocente;
///   - incoherente: vacía, en inglés, con la etiqueta de estado a la vista o repetida palabra por palabra.
/// </summary>
public static class PlaythroughChecks
{
    public enum Kind { AiBreak, InventedTime, InventedName, UnmotivatedConfession, FalseConfession, Incoherent }

    private static readonly Regex AiBreakPattern = new Regex(
        @"\b(soy|como) (una |un )?(ia|inteligencia artificial|modelo de lenguaje|asistente( virtual)?|chatbot|programa)\b|\bopenai\b|\bno puedo (ayudarte|ayudarle|proporcionar)\b|\b(mis|las) instrucciones\b|\bpersonaje (que interpreto|ficticio)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex ConfessionPattern = new Regex(
        @"\b(yo (la|lo) (mat[eé]|asesin[eé]|envenen[eé]|empuj[eé]|ahogu[eé])|(la|lo) mat[eé] yo|fui yo( quien| el que| la que)?|lo hice yo|confieso|soy (el|la) culpable|soy (el|la) asesin[oa])\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex DigitTime = new Regex(@"\b([01]?\d|2[0-3])[:.h]([0-5]\d)\b");

    private static readonly Regex Capitalized = new Regex(@"(?<![.!?¿¡«""\n]\s)(?<!^)\b(\p{Lu}\p{Ll}{2,})\b");

    private static readonly HashSet<string> CommonCapitalized = new HashSet<string>(
        new[]
        {
            "Dios", "Señor", "Señora", "Don", "Doña", "Usted", "Ustedes", "Inspector", "Inspectora", "Detective", "Agente",
            "Guardia", "Civil", "Policía", "Madrid", "España", "Galicia", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes",
            "Sábado", "Domingo", "Navidad", "Virgen", "WhatsApp", "Instagram", "Facebook", "Google", "Internet", "Mire",
            "Oiga", "Vale", "Bueno", "Sí", "No", "Pues", "Claro", "Hombre", "Mujer", "Mamá", "Papá", "Madre", "Padre"
        });

    private static readonly string[] EnglishWords = { " the ", " and ", " you ", " i'm ", " i am ", " what ", " with ", " this " };

    public class Finding
    {
        public Kind kind;
        public string detail;
    }

    /// <summary>
    /// 'sheet': todo lo que el personaje sabe (su ficha completa y el parte del caso).
    /// </summary>
    public static List<Finding> Check(string response, string sheet, bool speakerIsCulprit, int contradictions,
                                      int evidenceShownToSpeaker, string previousResponse, string question = null)
    {
        var findings = new List<Finding>();
        string text = response ?? "";
        string lower = " " + text.ToLowerInvariant() + " ";

        Match ai = AiBreakPattern.Match(text);
        if (ai.Success)
            findings.Add(new Finding { kind = Kind.AiBreak, detail = ai.Value });

        // Las horas que ya dijo el inspector en la pregunta no son inventadas
        HashSet<int> known = TimesIn(sheet);
        known.UnionWith(TimesIn(question));
        foreach (Match m in DigitTime.Matches(text))
        {
            int minutes = int.Parse(m.Groups[1].Value) * 60 + int.Parse(m.Groups[2].Value);
            if (!known.Contains(minutes))
                findings.Add(new Finding { kind = Kind.InventedTime, detail = m.Value });
        }

        string sheetFolded = Fold(sheet);
        foreach (Match m in Capitalized.Matches(text))
        {
            string word = m.Groups[1].Value;
            if (CommonCapitalized.Contains(word) || sheetFolded.Contains(Fold(word)))
                continue;
            findings.Add(new Finding { kind = Kind.InventedName, detail = word });
        }

        Match confession = ConfessionPattern.Match(text);
        if (confession.Success && !IsNegated(text, confession.Index))
        {
            if (!speakerIsCulprit)
                findings.Add(new Finding { kind = Kind.FalseConfession, detail = confession.Value });
            else if (contradictions == 0 && evidenceShownToSpeaker == 0)
                findings.Add(new Finding { kind = Kind.UnmotivatedConfession, detail = confession.Value });
        }

        string trimmed = text.Trim();
        if (trimmed.Length == 0 || trimmed == "…" || trimmed.Split(' ').Length < 2)
            findings.Add(new Finding { kind = Kind.Incoherent, detail = "respuesta vacía o de una palabra" });
        else if (EnglishWords.Count(w => lower.Contains(w)) >= 2)
            findings.Add(new Finding { kind = Kind.Incoherent, detail = "en inglés" });
        if (text.Contains("[ESTADO") || text.Contains("ESTADO:"))
            findings.Add(new Finding { kind = Kind.Incoherent, detail = "etiqueta de estado visible" });
        if (!string.IsNullOrEmpty(previousResponse) && trimmed.Length > 20 && trimmed == previousResponse.Trim())
            findings.Add(new Finding { kind = Kind.Incoherent, detail = "respuesta repetida" });

        return findings;
    }

    // "No, yo no la maté" / "jamás confieso..." no son confesiones
    private static bool IsNegated(string text, int index)
    {
        int start = System.Math.Max(0, index - 14);
        string before = text.Substring(start, index - start).ToLowerInvariant();
        return before.Contains("no ") || before.Contains("jamás") || before.Contains("nunca") || before.Contains("ni ");
    }

    public static HashSet<int> TimesIn(string text)
    {
        var result = new HashSet<int>();
        foreach (Match m in DigitTime.Matches(text ?? ""))
            result.Add(int.Parse(m.Groups[1].Value) * 60 + int.Parse(m.Groups[2].Value));
        return result;
    }

    // Sin tildes y en minúsculas, para comparar nombres
    public static string Fold(string text)
    {
        string decomposed = (text ?? "").ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (char c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString();
    }
}
