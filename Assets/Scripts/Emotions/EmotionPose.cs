using UnityEngine;

/// <summary>
/// Postura del retrato en cada estado emocional. El retrato se desliza hacia la postura del estado actual
/// (Step), así los cambios nunca son bruscos:
///   tranquilo: neutra · nervioso: temblor fino y sudor · asustado: retrocede (más pequeño) y tiembla ·
///   enfadado: se acerca (más grande) y enrojece · triste: baja la cabeza y pierde color.
/// Con "reducir animaciones" solo cambian el tinte y la saturación.
/// </summary>
public struct EmotionPose
{
    public Vector2 offset;     // px
    public float scale;
    public Color tint;
    public float saturation;   // 1 = color normal
    public float tremble;      // px de temblor continuo
    public float trembleFrequency;
    public bool sweat;

    public static EmotionPose For(Emotion emotion, Theme theme, bool reduceMotion)
    {
        var pose = new EmotionPose
        {
            offset = Vector2.zero,
            scale = 1f,
            tint = theme.EmotionTint(emotion),
            saturation = 1f,
            tremble = 0f,
            trembleFrequency = 20f,
            sweat = false
        };

        switch (emotion)
        {
            case Emotion.Nervioso:
                pose.tremble = theme.nerviousShake;
                pose.trembleFrequency = 18f;
                pose.sweat = true;
                pose.saturation = 0.95f;
                break;
            case Emotion.Asustado:
                pose.scale = 1f - theme.scaredRecoil;
                pose.offset = new Vector2(0f, -theme.scaredRecoil * 60f);
                pose.tremble = theme.scaredShake * 0.6f;
                pose.trembleFrequency = 26f;
                pose.saturation = 0.85f;
                break;
            case Emotion.Enfadado:
                pose.scale = 1f + theme.angryLean;
                pose.saturation = 1f;
                break;
            case Emotion.Triste:
                pose.offset = new Vector2(0f, -theme.sadDrop);
                pose.scale = 0.985f;
                pose.saturation = theme.sadSaturation;
                break;
        }

        if (reduceMotion)
        {
            pose.offset = Vector2.zero;
            pose.scale = 1f;
            pose.tremble = 0f;
            pose.sweat = false;
        }
        return pose;
    }

    /// <summary>
    /// Un paso de acercamiento exponencial hacia 'target' ('timeConstant' = segundos hasta ~63 %).
    /// </summary>
    public static EmotionPose Step(EmotionPose current, EmotionPose target, float deltaTime, float timeConstant)
    {
        float k = timeConstant <= 0f ? 1f : 1f - Mathf.Exp(-Mathf.Max(0f, deltaTime) / timeConstant);
        return new EmotionPose
        {
            offset = Vector2.Lerp(current.offset, target.offset, k),
            scale = Mathf.Lerp(current.scale, target.scale, k),
            tint = Color.Lerp(current.tint, target.tint, k),
            saturation = Mathf.Lerp(current.saturation, target.saturation, k),
            tremble = Mathf.Lerp(current.tremble, target.tremble, k),
            trembleFrequency = target.trembleFrequency,
            sweat = target.sweat
        };
    }
}
