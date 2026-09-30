using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Curvas de los efectos (t de 0 a 1). Sin estado: se prueban aparte y las usan FxLayer y compañía.
/// </summary>
public static class FxCurves
{
    // Sello: baja desde 2,4× y golpea al 35 %; después, quieto
    public static float StampScale(float t)
    {
        float k = Mathf.Clamp01(t / 0.35f);
        return Mathf.Lerp(2.4f, 1f, k * k); // Acelera hasta el impacto
    }

    public static float StampAlpha(float t)
    {
        return Mathf.Clamp01(t / 0.2f);
    }

    // Ficha de pista: cae girando sobre el eje X y se endereza con un pequeño rebote
    public static float CardFallAngle(float t)
    {
        return Mathf.LerpUnclamped(-75f, 0f, Easing.OutBack(Mathf.Clamp01(t)));
    }

    public static float CardFallOffset(float t)
    {
        return Mathf.Lerp(260f, 0f, Easing.OutCubic(Mathf.Clamp01(t)));
    }

    // Destello: una banda de luz que cruza la ficha de izquierda a derecha (posición normalizada)
    public static float GlintPosition(float t)
    {
        return Mathf.Lerp(-0.3f, 1.3f, Easing.InOutSine(Mathf.Clamp01(t)));
    }

    // Viñeta de la acusación: se cierra deprisa al principio y luego muy despacio
    public static float VignetteClose(float seconds)
    {
        return Mathf.Clamp01(1f - Mathf.Exp(-Mathf.Max(0f, seconds) * 1.4f));
    }

    // Latido (lub-dub) en un ciclo de 0 a 1
    public static float Heartbeat(float t)
    {
        t = Mathf.Repeat(t, 1f);
        return Pulse(t, 0.12f, 0.07f) + 0.7f * Pulse(t, 0.3f, 0.07f);
    }

    private static float Pulse(float t, float center, float width)
    {
        float d = (t - center) / width;
        return Mathf.Exp(-d * d * 2f);
    }
}

/// <summary>
/// La verdad del caso como línea temporal: una frase por paso; si la frase da una hora, la hora es la etiqueta.
/// </summary>
public static class Timeline
{
    public struct Step
    {
        public string time;   // null si la frase no da hora
        public string text;
    }

    private static readonly Regex Time = new Regex(@"\b([01]?\d|2[0-3]):[0-5]\d\b");
    private static readonly Regex LeadingTime = new Regex(@"^(?:A|Hacia|Sobre) las (\d{1,2}:\d{2}),?\s+");
    private static readonly Regex SentenceEnd = new Regex(@"(?<=[.!?…])\s+(?=[\p{Lu}¿¡«])");
    private static readonly string[] Abbreviations = { "Sr.", "Sra.", "Dr.", "Dra.", "D.", "Dña.", "St." };

    public static IEnumerable<Step> FromEpilogue(string epilogue)
    {
        if (string.IsNullOrWhiteSpace(epilogue))
            yield break;

        var sentences = new List<string>();
        foreach (string part in SentenceEnd.Split(epilogue.Trim()))
        {
            // "El Sr. Gil": no se parte tras una abreviatura
            if (sentences.Count > 0 && EndsWithAbbreviation(sentences[sentences.Count - 1]))
                sentences[sentences.Count - 1] += " " + part;
            else
                sentences.Add(part);
        }

        foreach (string sentence in sentences)
        {
            Match m = Time.Match(sentence);
            string text = sentence.Trim();
            // La hora ya va en el margen: "A las 22:30, con…" → "Con…"
            Match lead = LeadingTime.Match(text);
            if (m.Success && lead.Success && lead.Groups[1].Value == m.Value && lead.Length < text.Length)
                text = char.ToUpper(text[lead.Length]) + text.Substring(lead.Length + 1);
            yield return new Step { time = m.Success ? m.Value : null, text = text };
        }
    }

    private static bool EndsWithAbbreviation(string sentence)
    {
        foreach (string a in Abbreviations)
        {
            if (sentence.EndsWith(" " + a) || sentence == a)
                return true;
        }
        return false;
    }
}

/// <summary>
/// Cómo se presenta cada final: sello, color de la escena y frase de cierre.
/// </summary>
public class EndingStyle
{
    public string stamp;
    public Color ink;       // Color del sello
    public Color grade;     // Tinte de la pantalla
    public string title;
    public string verdict;

    public static EndingStyle For(Ending ending, Theme t)
    {
        switch (ending)
        {
            case Ending.Good:
                return new EndingStyle
                {
                    stamp = "CASO CERRADO", ink = t.success, grade = new Color(0.95f, 0.78f, 0.45f, 0.06f),
                    title = "Caso resuelto", verdict = "Las pruebas y las contradicciones no dejan lugar a dudas. El culpable es condenado."
                };
            case Ending.Bittersweet:
                return new EndingStyle
                {
                    stamp = "CERRADO CON DUDAS", ink = t.accent, grade = new Color(0.85f, 0.55f, 0.35f, 0.08f),
                    title = "Culpable, pero con dudas", verdict = "Señalaste al culpable, pero la defensa encuentra huecos. El juicio será largo e incierto."
                };
            case Ending.Insufficient:
                return new EndingStyle
                {
                    stamp = "SOBRESEÍDO", ink = t.contradiction, grade = new Color(0.45f, 0.55f, 0.75f, 0.12f),
                    title = "Libre por falta de pruebas", verdict = "Tu intuición era buena, pero sin pruebas el caso se archiva y el culpable sale por la puerta."
                };
            default:
                return new EndingStyle
                {
                    stamp = "CASO FALLIDO", ink = t.danger, grade = new Color(0.7f, 0.12f, 0.1f, 0.14f),
                    title = "Caso no resuelto", verdict = "Acusaste a la persona equivocada. El verdadero culpable sigue libre."
                };
        }
    }
}

/// <summary>
/// Resumen del caso para el informe final: rango, veredicto sobre la prueba clave y pistas que se escaparon.
/// </summary>
public class CaseSummary
{
    public string rank;
    public string keyClueLine;                 // null si no se eligió prueba clave
    public string notesLine;                   // Lo que apuntó el jugador sobre el culpable (null: nada)
    public SuspectView culprit;                // Para la ficha policial del final (id null = sin ficha)
    public System.Collections.Generic.List<string> missed = new System.Collections.Generic.List<string>();
}

/// <summary>
/// Texto del informe final: acusación, evidencia, veredicto y la verdad como línea temporal (una línea por paso,
/// para que StepReveal la descubra poco a poco).
/// </summary>
public static class EndingReport
{
    public static string Build(AccusationResult result, string accusedName, string culpritName, int maxEvidence,
                               string epilogue, Theme t, string stats = null, CaseSummary summary = null)
    {
        EndingStyle style = EndingStyle.For(result.ending, t);
        var sb = new System.Text.StringBuilder();
        string label(string text) => $"<color={Theme.Hex(t.textSecondary)}>{text}</color>";

        sb.AppendLine($"{label("TU ACUSACIÓN")}  <b>{accusedName}</b>");
        sb.AppendLine($"{label("CULPABLE")}  <b>{culpritName}</b>");
        // Un dato por línea: dos en la misma partían la cifra a la línea siguiente en pantallas estrechas
        sb.AppendLine($"{label("PISTAS INCRIMINATORIAS")}  {result.incriminatingFound}");
        sb.AppendLine($"{label("CONTRADICCIONES")}  {result.contradictions}");
        // Puntuación (pistas incriminatorias + contradicciones, que pesan más), no un recuento de pistas: con "0/6"
        // junto a "pistas encontradas: 0 de 5" parecían dos cuentas de lo mismo
        sb.AppendLine($"{label("SOLIDEZ DE LAS PRUEBAS")}  <b>{result.evidence} de {maxEvidence}</b>");
        // Cuánto faltó, solo si acertaste sin pruebas suficientes (ganando o acusando a otro no aporta nada)
        int missing = InvestigationState.GoodThreshold - result.evidence;
        if ((result.ending == Ending.Bittersweet || result.ending == Ending.Insufficient) && missing > 0)
            sb.AppendLine($"<size=80%>{label($"Con {missing} {(missing == 1 ? "punto" : "puntos")} más de solidez habría sido una condena segura.")}</size>");
        if (!string.IsNullOrEmpty(stats))
            sb.AppendLine(label(stats));
        sb.AppendLine();

        if (summary != null && !string.IsNullOrEmpty(summary.rank))
            sb.AppendLine($"{label("RANGO")}  <b>{summary.rank}</b>");
        if (summary != null && !string.IsNullOrEmpty(summary.keyClueLine))
            sb.AppendLine(summary.keyClueLine);
        if (summary != null && !string.IsNullOrEmpty(summary.notesLine))
            sb.AppendLine(summary.notesLine);
        if (summary != null && (!string.IsNullOrEmpty(summary.rank) || !string.IsNullOrEmpty(summary.keyClueLine)
                                || !string.IsNullOrEmpty(summary.notesLine)))
            sb.AppendLine();

        sb.AppendLine($"<color={Theme.Hex(style.ink)}><b>{style.title.ToUpperInvariant()}</b></color>");
        sb.AppendLine(result.ending == Ending.Bad && result.ignoredClearingClue
            ? "Acusaste a alguien a quien tus propias pistas descartaban. El verdadero culpable sigue libre."
            : style.verdict);
        sb.AppendLine();

        sb.AppendLine($"<color={Theme.Hex(t.accent)}><b>LO QUE PASÓ DE VERDAD</b></color>");
        foreach (Timeline.Step step in Timeline.FromEpilogue(epilogue))
        {
            string marker = step.time != null ? $"<color={Theme.Hex(t.accent)}><b>{step.time}</b></color>" : $"<color={Theme.Hex(t.accent)}>•</color>";
            sb.AppendLine($"{marker}  {step.text}");
        }

        // Lo que se escapó: aprender a preguntar (y ganas de rejugar)
        if (summary != null && summary.missed != null && summary.missed.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"<color={Theme.Hex(t.accent)}><b>LO QUE SE TE ESCAPÓ</b></color>");
            foreach (string m in summary.missed)
                sb.AppendLine($"<color={Theme.Hex(t.accent)}>•</color>  {m}");
        }
        return sb.ToString().TrimEnd();
    }
}
