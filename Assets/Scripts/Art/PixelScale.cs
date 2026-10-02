using UnityEngine;

/// <summary>
/// Tamaño en pantalla de un pixel art sin ampliarlo por encima de su resolución (revisión de Cristian, sesión C: el
/// busto de 450 × 600 téxeles salía a más de 2× con filtrado bilineal, en bloques enormes y borrosos). Reducir sí se
/// puede (se ve nítido); ampliar, solo hasta el máximo del tema y por múltiplos enteros (cada téxel, n × n píxeles).
/// </summary>
public static class PixelScale
{
    /// <param name="box">Caja disponible, en unidades del lienzo.</param>
    /// <param name="texels">Téxeles visibles del retrato (textura × uvRect).</param>
    /// <param name="density">Píxeles de pantalla por unidad del lienzo (Canvas.scaleFactor).</param>
    /// <param name="maxMagnification">Píxeles de pantalla por téxel como mucho (1 = nunca ampliar).</param>
    /// <returns>Tamaño en unidades del lienzo, con la proporción del retrato.</returns>
    public static Vector2 Fit(Vector2 box, Vector2 texels, float density, float maxMagnification)
    {
        if (box.x <= 0f || box.y <= 0f || texels.x <= 0f || texels.y <= 0f)
            return Vector2.zero;
        density = Mathf.Max(0.01f, density);
        float unitsPerTexel = Mathf.Min(box.x / texels.x, box.y / texels.y);
        float magnification = unitsPerTexel * density;
        if (magnification > 1f)
        {
            float allowed = Mathf.Floor(Mathf.Min(magnification, Mathf.Max(1f, maxMagnification)) + 1e-4f);
            unitsPerTexel = Mathf.Max(1f, allowed) / density;
        }
        return texels * unitsPerTexel;
    }
}
