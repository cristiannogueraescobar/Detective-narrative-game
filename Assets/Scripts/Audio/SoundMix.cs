using UnityEngine;

/// <summary>
/// Mezcla en código (sin AudioMixer): bajo los golpes (pista, contradicción, acusación, día nuevo, finales) la
/// música se aparta unos 7 dB, aguanta mientras suena el golpe y vuelve sola con una curva suave. Así el golpe se
/// oye sin subir su volumen y la música nunca tapa un momento importante.
/// </summary>
public static class SoundMix
{
    public const float Depth = 0.45f;   // Ganancia de la música apartada (≈ −7 dB)
    public const float Attack = 0.08f;  // s hasta apartarse (rápido, sin chasquido)
    public const float Release = 1.5f;  // s para volver
    public const float MaxHold = 3f;    // Un golpe largo (finales) no deja la música apartada más de esto

    public static bool Ducks(Sfx sfx)
    {
        switch (sfx)
        {
            case Sfx.Clue:
            case Sfx.Contradiction:
            case Sfx.Accusation:
            case Sfx.DayChange:
            case Sfx.EndingGood:
            case Sfx.EndingBittersweet:
            case Sfx.EndingInsufficient:
            case Sfx.EndingBad:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Cuánto aguanta apartada la música para un golpe de 'clipSeconds'.
    /// </summary>
    public static float HoldFor(float clipSeconds) => Mathf.Clamp(clipSeconds, 0f, MaxHold);

    /// <summary>
    /// Ganancia de la música 'since' segundos después del golpe (negativo o infinito = no hay golpe).
    /// </summary>
    public static float DuckGain(float since, float hold)
    {
        if (since < 0f || float.IsInfinity(since) || float.IsNaN(since))
            return 1f;
        if (since < Attack)
            return Mathf.Lerp(1f, Depth, since / Attack);
        if (since <= hold)
            return Depth;
        float r = (since - hold) / Release;
        return r >= 1f ? 1f : Mathf.Lerp(Depth, 1f, Mathf.SmoothStep(0f, 1f, r));
    }
}
