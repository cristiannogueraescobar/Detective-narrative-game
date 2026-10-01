using System.Collections.Generic;

/// <summary>
/// Tipos de pregunta con los que se calibra la etiqueta de estado y los estados que se consideran coherentes con cada uno.
/// </summary>
public enum EmotionProbe
{
    Neutra,        // "¿A qué se dedica?"
    Victima,       // "¿Cómo era {víctima}?"
    TemaSensible,  // Lo que le pone nervioso según su ficha
    Acusacion      // "Creo que usted lo hizo."
}

public static class EmotionCoherence
{
    private static readonly Dictionary<EmotionProbe, Emotion[]> Expected = new Dictionary<EmotionProbe, Emotion[]>
    {
        { EmotionProbe.Neutra, new[] { Emotion.Tranquilo, Emotion.Nervioso, Emotion.Triste } },
        { EmotionProbe.Victima, new[] { Emotion.Triste, Emotion.Nervioso } },
        { EmotionProbe.TemaSensible, new[] { Emotion.Nervioso, Emotion.Asustado, Emotion.Enfadado } },
        { EmotionProbe.Acusacion, new[] { Emotion.Enfadado, Emotion.Nervioso, Emotion.Asustado } }
    };

    public static bool IsCoherent(EmotionProbe probe, Emotion emotion)
    {
        return System.Array.IndexOf(Expected[probe], emotion) >= 0;
    }
}
