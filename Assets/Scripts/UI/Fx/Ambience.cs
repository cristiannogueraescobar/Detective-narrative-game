using UnityEngine;

/// <summary>
/// Matemáticas de la ambientación (sin estado; las usa MenuAmbience y se prueban aparte).
/// </summary>
public static class Ambience
{
    /// <summary>
    /// Intensidad de la lámpara (≈1): un vaivén casi imperceptible y, cada 7-13 s, un parpadeo breve
    /// de dos o tres caídas, como una bombilla vieja. Determinista para un 'seed' dado.
    /// </summary>
    public static float LampFlicker(float time, float seed)
    {
        float breathe = 1f + 0.025f * Mathf.Sin(time * 1.7f + seed) + 0.015f * (Mathf.PerlinNoise(time * 0.8f, seed) - 0.5f);

        // Ventanas de parpadeo: una por ciclo, en un momento pseudoaleatorio del ciclo
        const float cycle = 10f;
        float index = Mathf.Floor(time / cycle);
        float at = Hash(index + seed) * (cycle - 1.5f);
        float local = time - index * cycle - at;
        float dip = 0f;
        if (local >= 0f && local < 0.45f)
        {
            // Dos caídas rápidas
            dip = Mathf.Max(0f, Mathf.Sin(local / 0.45f * Mathf.PI * 2.5f)) * (0.3f + 0.15f * Hash(index * 1.7f + seed));
        }
        return Mathf.Clamp(breathe - dip, 0.5f, 1.06f);
    }

    /// <summary>
    /// Posición en la imagen (0-1, origen arriba a la izquierda) → ancla de Unity (origen abajo).
    /// </summary>
    public static Vector2 ImageToAnchor(Vector2 imagePosition)
    {
        return new Vector2(imagePosition.x, 1f - imagePosition.y);
    }

    /// <summary>
    /// Visibilidad de una mota de polvo según su distancia a la luz (1 cerca, 0 fuera del cono).
    /// </summary>
    public static float DustVisibility(Vector2 position, Vector2 light, float radius)
    {
        float d = Vector2.Distance(position, light) / Mathf.Max(0.0001f, radius);
        return Mathf.Clamp01(1f - d * d);
    }

    /// <summary>
    /// Tamaño que cubre toda la pantalla sin deformar la imagen (lo que sobra se recorta).
    /// </summary>
    public static Vector2 CoverSize(Vector2 image, Vector2 screen)
    {
        if (image.x <= 0f || image.y <= 0f)
            return screen;
        float scale = Mathf.Max(screen.x / image.x, screen.y / image.y);
        return image * scale;
    }

    private static float Hash(float x)
    {
        return Mathf.Repeat(Mathf.Sin(x * 127.1f + 311.7f) * 43758.5453f, 1f);
    }
}
