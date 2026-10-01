using UnityEngine;

/// <summary>
/// Cómo se ve y se lee cada estado emocional: tinte del retrato, temblor y velocidad del texto.
/// Los valores salen del tema activo (ThemeManager); aquí solo se reparten por estado.
/// </summary>
public class EmotionStyle
{
    public Color tint = Color.white;
    public float shakeAmplitude;     // Píxeles de desplazamiento máximo del retrato
    public float shakeFrequency = 20f;
    public bool shakeOnce;           // Sacudida única (enfado) en lugar de temblor continuo
    public float textSpeed = 1f;     // Multiplicador de la velocidad de escritura

    /// <summary>
    /// Color de la etiqueta del estado sobre el retrato (legible sobre fondo casi negro).
    /// </summary>
    public static Color LabelColor(Emotion emotion)
    {
        Theme t = ThemeManager.Current;
        switch (emotion)
        {
            case Emotion.Nervioso: return t.accent;
            case Emotion.Asustado: return new Color(0.72f, 0.82f, 1f);
            case Emotion.Enfadado: return t.danger;
            case Emotion.Triste: return new Color(0.7f, 0.76f, 0.9f);
            default: return t.textSecondary;
        }
    }

    public static EmotionStyle For(Emotion emotion)
    {
        Theme theme = ThemeManager.Current;
        var style = new EmotionStyle { tint = theme.EmotionTint(emotion) };

        switch (emotion)
        {
            case Emotion.Nervioso:
                style.shakeAmplitude = theme.nerviousShake;
                style.shakeFrequency = 18f;
                style.textSpeed = theme.nervousTextSpeed;
                break;
            case Emotion.Asustado:
                style.shakeAmplitude = theme.scaredShake;
                style.shakeFrequency = 26f;
                style.textSpeed = theme.scaredTextSpeed;
                break;
            case Emotion.Enfadado:
                style.shakeAmplitude = theme.angryShake;
                style.shakeFrequency = 30f;
                style.shakeOnce = true;
                style.textSpeed = theme.angryTextSpeed;
                break;
            case Emotion.Triste:
                style.textSpeed = theme.sadTextSpeed;
                break;
        }

        return style;
    }
}
