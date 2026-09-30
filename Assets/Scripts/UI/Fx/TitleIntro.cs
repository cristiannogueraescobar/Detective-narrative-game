using TMPro;
using UnityEngine;

/// <summary>
/// Entrada del título del menú: se enciende como un rótulo viejo (sube, parpadea, se queda) mientras las letras
/// se juntan. Una vez por aparición del menú. Con "reducir animaciones" aparece sin más.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class TitleIntro : MonoBehaviour
{
    public float restSpacing = 8f;
    private TMP_Text text;
    private float elapsed = -1f; // < 0: sin animación en curso

    /// <summary>
    /// Opacidad a lo largo de la entrada (t de 0 a 1).
    /// </summary>
    public static float Alpha(float t)
    {
        if (t <= 0f)
            return 0f;
        if (t >= 1f)
            return 1f;

        float rise = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.35f));
        // Dos parpadeos: uno fuerte hacia el 45 % y otro corto hacia el 62 %
        float dip = Bump(t, 0.45f, 0.05f) * 0.75f + Bump(t, 0.62f, 0.03f) * 0.4f;
        return Mathf.Clamp01(rise - dip);
    }

    /// <summary>
    /// Espaciado entre letras: empieza abierto y se cierra hasta el de reposo.
    /// </summary>
    public static float Spacing(float t, float rest)
    {
        return Mathf.Lerp(rest + 40f, rest, Easing.OutCubic(Mathf.Clamp01(t)));
    }

    private static float Bump(float t, float center, float width)
    {
        float d = Mathf.Abs(t - center) / width;
        return d >= 1f ? 0f : 1f - d * d;
    }

    private void OnEnable()
    {
        text = GetComponent<TMP_Text>();
        elapsed = Application.isPlaying && !GameSettings.ReduceMotion ? 0f : -1f;
        Apply(elapsed < 0f ? 1f : 0f);
    }

    private void Update()
    {
        if (elapsed < 0f)
            return;
        // Un tirón (el primer fotograma compila shaders) no se come la entrada
        elapsed += Mathf.Min(Time.unscaledDeltaTime, 1f / 30f);
        float t = elapsed / Mathf.Max(0.1f, ThemeManager.Current.titleIntroDuration);
        Apply(t);
        if (t >= 1f)
            elapsed = -1f;
    }

    private void Apply(float t)
    {
        text.alpha = Alpha(t);
        text.characterSpacing = Spacing(t, restSpacing);
    }
}
