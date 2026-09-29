using UnityEngine;

/// <summary>
/// Cómo se ve y se lee cada estado emocional: tinte del retrato, temblor y velocidad del texto.
/// Valores por defecto en código; el tema (ThemeManager) puede sustituirlos.
/// </summary>
public class EmotionStyle
{
    public Color tint = Color.white;
    public float shakeAmplitude;     // Píxeles de desplazamiento máximo del retrato
    public float shakeFrequency = 20f;
    public bool shakeOnce;           // Sacudida única (enfado) en lugar de temblor continuo
    public float textSpeed = 1f;     // Multiplicador de la velocidad de escritura

    public static EmotionStyle For(Emotion emotion)
    {
        switch (emotion)
        {
            case Emotion.Nervioso:
                return new EmotionStyle { tint = new Color(1f, 0.96f, 0.86f), shakeAmplitude = 1.5f, shakeFrequency = 18f, textSpeed = 1.25f };
            case Emotion.Asustado:
                return new EmotionStyle { tint = new Color(0.86f, 0.9f, 1f), shakeAmplitude = 3f, shakeFrequency = 26f, textSpeed = 1.4f };
            case Emotion.Enfadado:
                return new EmotionStyle { tint = new Color(1f, 0.84f, 0.8f), shakeAmplitude = 6f, shakeFrequency = 30f, shakeOnce = true, textSpeed = 1.15f };
            case Emotion.Triste:
                return new EmotionStyle { tint = new Color(0.82f, 0.86f, 0.95f), textSpeed = 0.75f };
            default:
                return new EmotionStyle();
        }
    }
}
