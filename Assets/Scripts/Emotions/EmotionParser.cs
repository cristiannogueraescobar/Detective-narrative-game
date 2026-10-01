using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum Emotion
{
    Tranquilo,
    Nervioso,
    Asustado,
    Enfadado,
    Triste
}

public struct EmotionParse
{
    public string text;       // Respuesta sin la etiqueta, lista para mostrar y analizar
    public Emotion? emotion;  // null si no hay etiqueta reconocible
    public bool wellFormed;   // true solo con "[ESTADO: x]" y x válido
}

/// <summary>
/// Lee y elimina la etiqueta oculta de estado emocional que el modelo añade al final de cada respuesta:
/// "[ESTADO: tranquilo|nervioso|asustado|enfadado|triste]". Tolera mayúsculas, espacios y femenino.
/// </summary>
public static class EmotionParser
{
    public const string TagInstruction =
        "Termina SIEMPRE con una línea aparte que diga cómo te sientes: [ESTADO: tranquilo], [ESTADO: nervioso], " +
        "[ESTADO: asustado], [ESTADO: enfadado] o [ESTADO: triste].";

    // Tolera erratas alrededor de "estado" ("[MESTADO: x]", "[ESTADOS: x]"): si no, la etiqueta se veía en el chat
    private static readonly Regex Bracketed = new Regex(@"\[\s*[a-záéíóú]{0,3}estado[a-z]{0,3}\s*:\s*([^\]]*?)\s*\]", RegexOptions.IgnoreCase);
    private static readonly Regex Exact = new Regex(@"\[\s*estado\s*:", RegexOptions.IgnoreCase);
    // Etiqueta cortada por el límite de tokens: "[ESTADO: nerv", "[EST"
    private static readonly Regex Truncated = new Regex(@"\[\s*[a-záéíóú]{0,3}es[^\]\n]*$", RegexOptions.IgnoreCase);
    private static readonly Regex Bare = new Regex(@"(^|\n)\s*estado\s*:\s*(\S+)\s*$", RegexOptions.IgnoreCase);
    private static readonly Regex Spaces = new Regex(@"[ \t]+");

    /// <summary>
    /// La respuesta con la etiqueta bien escrita al final (para el historial: el modelo copia lo que ve, erratas
    /// incluidas). Sin estado reconocible, solo el texto.
    /// </summary>
    public static string Canonical(string raw)
    {
        EmotionParse parse = Parse(raw);
        // Sin estado reconocible se guarda tal cual: quitar la etiqueta dejaría al modelo sin el ejemplo del formato
        return parse.emotion.HasValue ? $"{parse.text}\n[ESTADO: {parse.emotion.Value.ToString().ToLowerInvariant()}]" : raw.Trim();
    }

    // PERFORMANCE-AUDIT, mejora 3: el gestor analizaba la misma respuesta hasta 7 veces (idioma, primer contacto,
    // horas, final, repetición). Los últimos textos analizados se recuerdan; EmotionParse es un struct de strings
    // inmutables, así que devolver la copia guardada es seguro.
    private const int RecentLimit = 16;
    private static readonly Dictionary<string, EmotionParse> Recent = new Dictionary<string, EmotionParse>();
    private static readonly Queue<string> RecentOrder = new Queue<string>();
    private static readonly object RecentLock = new object();

    /// <summary>Análisis hechos de verdad (no servidos por la memoria): para los tests.</summary>
    public static int ParseWork { get; private set; }

    public static void ResetCache()
    {
        lock (RecentLock)
        {
            Recent.Clear();
            RecentOrder.Clear();
            ParseWork = 0;
        }
    }

    public static EmotionParse Parse(string raw)
    {
        string key = raw ?? "";
        lock (RecentLock)
        {
            if (Recent.TryGetValue(key, out EmotionParse hit))
                return hit;
        }
        EmotionParse parse = ParseUncached(raw);
        lock (RecentLock)
        {
            ParseWork++;
            if (!Recent.ContainsKey(key))
            {
                Recent[key] = parse;
                RecentOrder.Enqueue(key);
                if (RecentOrder.Count > RecentLimit)
                    Recent.Remove(RecentOrder.Dequeue());
            }
        }
        return parse;
    }

    private static EmotionParse ParseUncached(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            return new EmotionParse { text = "" };

        string word = null;
        bool bracketed = false;

        MatchCollection matches = Bracketed.Matches(raw);
        string text = raw;

        if (matches.Count > 0)
        {
            word = matches[matches.Count - 1].Groups[1].Value;
            bracketed = true;
            text = Bracketed.Replace(raw, "");
        }
        else
        {
            Match bare = Bare.Match(raw);
            if (bare.Success)
            {
                word = bare.Groups[2].Value;
                text = raw.Substring(0, bare.Index);
            }
        }

        text = Truncated.Replace(text, "");
        Emotion? emotion = word == null ? (Emotion?)null : MapWords(word);

        return new EmotionParse
        {
            text = Spaces.Replace(text, " ").Trim(),
            emotion = emotion,
            wellFormed = bracketed && emotion.HasValue && Exact.IsMatch(raw)
        };
    }

    // "muy nervioso" → nervioso: vale la última palabra reconocible
    private static Emotion? MapWords(string words)
    {
        string[] parts = words.Split(new[] { ' ', '\t', ',', '/' }, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = parts.Length - 1; i >= 0; i--)
        {
            Emotion? emotion = Map(parts[i]);
            if (emotion.HasValue)
                return emotion;
        }
        return null;
    }

    private static Emotion? Map(string word)
    {
        string w = ClueDetector.Normalize(word).Trim('.', ',', ';', ':', '!', '?');

        if (w.StartsWith("tranquil") || w.StartsWith("calmad"))
            return Emotion.Tranquilo;
        if (w.StartsWith("nervios"))
            return Emotion.Nervioso;
        if (w.StartsWith("asustad"))
            return Emotion.Asustado;
        if (w.StartsWith("enfadad") || w.StartsWith("enojad"))
            return Emotion.Enfadado;
        if (w.StartsWith("trist"))
            return Emotion.Triste;

        return null;
    }
}
