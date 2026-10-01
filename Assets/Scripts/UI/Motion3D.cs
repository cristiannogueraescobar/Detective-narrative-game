using UnityEngine;

/// <summary>
/// Curvas de los efectos de profundidad (pseudo-3D). Funciones puras y sin reservas de memoria:
/// se evalúan cada fotograma en móvil.
/// </summary>
public static class Motion3D
{
    /// <summary>
    /// Escala de respiración: oscila entre 1 y 1 + amplitud con el periodo dado (s).
    /// </summary>
    public static float Breath(float time, float period, float amplitude)
    {
        return 1f + amplitude * 0.5f * (1f - Mathf.Cos(2f * Mathf.PI * time / Mathf.Max(0.01f, period)));
    }

    /// <summary>
    /// Inclinación tipo tarjeta al tocar: sube rápido y vuelve con un pequeño rebote (grados).
    /// </summary>
    public static float Tilt(float progress, float maxDegrees)
    {
        progress = Mathf.Clamp01(progress);
        return maxDegrees * Mathf.Sin(Mathf.PI * progress) * (1f - progress * 0.5f);
    }

    /// <summary>
    /// Desplazamiento de una capa de fondo a partir de la inclinación (-1..1 en cada eje), en contra de ella.
    /// </summary>
    public static Vector2 Parallax(Vector2 tilt, float maxPixels)
    {
        return -Vector2.ClampMagnitude(tilt, 1f) * maxPixels;
    }

    /// <summary>
    /// Giro de carta al aparecer: de 90° (de canto) a 0° con un leve rebote.
    /// </summary>
    public static float CardFlip(float progress)
    {
        return 90f * (1f - Easing.OutBack(progress));
    }
}
