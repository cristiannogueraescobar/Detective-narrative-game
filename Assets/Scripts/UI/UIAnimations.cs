using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Curvas de suavizado (0..1 → 0..1).
/// </summary>
public static class Easing
{
    public static float OutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        float u = 1f - t;
        return 1f - u * u * u;
    }

    public static float OutBack(float t)
    {
        t = Mathf.Clamp01(t);
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float u = t - 1f;
        return 1f + c3 * u * u * u + c1 * u * u;
    }

    public static float InOutSine(float t)
    {
        t = Mathf.Clamp01(t);
        return -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
    }

    /// <summary>
    /// Sube de 0 a 1 y vuelve a 0 (destellos).
    /// </summary>
    public static float Pulse(float t)
    {
        t = Mathf.Clamp01(t);
        return Mathf.Sin(Mathf.PI * t);
    }
}

/// <summary>
/// "Juice" de la UI por corrutinas (sin dependencias externas). Duraciones en el tema;
/// con "reducir animaciones" todo ocurre al instante.
/// </summary>
public static class UIAnimations
{
    private static bool Instant => GameSettings.ReduceMotion;

    /// <summary>
    /// Fundido de entrada de un panel (añade un CanvasGroup si no lo tiene).
    /// </summary>
    public static void FadeIn(MonoBehaviour host, GameObject panel)
    {
        if (panel == null || host == null)
            return;

        CanvasGroup group = UIComponents.GetOrAdd<CanvasGroup>(panel);
        if (Instant || !host.isActiveAndEnabled)
        {
            group.alpha = 1f;
            return;
        }

        host.StartCoroutine(Animate(ThemeManager.Current.panelFadeDuration, t => group.alpha = Easing.OutCubic(t)));
    }

    /// <summary>
    /// Aparición con rebote suave (aviso de pista).
    /// </summary>
    public static void Pop(MonoBehaviour host, Transform target)
    {
        if (target == null || host == null)
            return;

        if (Instant || !host.isActiveAndEnabled)
        {
            target.localScale = Vector3.one;
            return;
        }

        host.StartCoroutine(Animate(ThemeManager.Current.clueAnimDuration,
            t => { if (target != null) target.localScale = Vector3.one * Mathf.LerpUnclamped(0.85f, 1f, Easing.OutBack(t)); }));
    }

    /// <summary>
    /// Aparición como una carta que gira hacia el jugador (pistas, libreta).
    /// </summary>
    public static void CardFlip(MonoBehaviour host, Transform target)
    {
        if (target == null || host == null)
            return;

        if (Instant || !host.isActiveAndEnabled)
        {
            target.localRotation = Quaternion.identity;
            return;
        }

        host.StartCoroutine(Animate(ThemeManager.Current.cardFlipDuration,
            t => { if (target != null) target.localRotation = Quaternion.Euler(0f, Motion3D.CardFlip(t), 0f); },
            () => { if (target != null) target.localRotation = Quaternion.identity; }));
    }

    /// <summary>
    /// Destello de color sobre un gráfico (contradicción): sube y vuelve a transparente.
    /// </summary>
    public static void Flash(MonoBehaviour host, UnityEngine.UI.Graphic overlay, Color color, float maxAlpha)
    {
        if (overlay == null || host == null || Instant || !host.isActiveAndEnabled)
            return;

        overlay.gameObject.SetActive(true);
        overlay.raycastTarget = false;
        host.StartCoroutine(Animate(ThemeManager.Current.contradictionAnimDuration,
            t => { if (overlay != null) overlay.color = new Color(color.r, color.g, color.b, maxAlpha * Easing.Pulse(t)); },
            () => { if (overlay != null) overlay.gameObject.SetActive(false); }));
    }

    /// <summary>
    /// Sacudida horizontal breve que se amortigua.
    /// </summary>
    public static void Shake(MonoBehaviour host, RectTransform target, float amplitude, float duration)
    {
        if (target == null || host == null || Instant || !host.isActiveAndEnabled)
            return;

        Vector2 rest = target.anchoredPosition;
        host.StartCoroutine(Animate(duration,
            t => { if (target != null) target.anchoredPosition = rest + new Vector2(Mathf.Sin(t * 40f) * amplitude * (1f - t), 0f); },
            () => { if (target != null) target.anchoredPosition = rest; }));
    }

    public static IEnumerator Animate(float duration, Action<float> step, Action done = null)
    {
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            step(elapsed / duration);
            yield return null;
        }

        step(1f);
        done?.Invoke();
    }
}
