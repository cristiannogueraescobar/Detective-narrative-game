using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sprites de interfaz generados por código (no hay arte para ellos): rectángulo redondeado en 9 partes,
/// círculo para máscaras y degradado de viñeta. Se crean una vez y se reutilizan.
/// </summary>
public static class UISprites
{
    private static readonly Dictionary<int, Sprite> rounded = new Dictionary<int, Sprite>();
    private static Sprite circle;

    /// <summary>
    /// Rectángulo con esquinas de 'radius' px (a pixelsPerUnitMultiplier = 1), borde suavizado, en 9 partes.
    /// </summary>
    public static Sprite Rounded(int radius)
    {
        radius = Mathf.Clamp(radius, 2, 64);
        if (rounded.TryGetValue(radius, out Sprite cached) && cached != null)
            return cached;

        int size = radius * 2 + 4;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = $"Redondeado {radius} (auto)",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.DontSave
        };

        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Distancia al rectángulo interior (las esquinas son cuartos de círculo)
                float cx = Mathf.Clamp(x + 0.5f, radius, size - radius);
                float cy = Mathf.Clamp(y + 0.5f, radius, size - radius);
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                byte a = (byte)(Mathf.Clamp01(radius - d + 0.5f) * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, a);
            }
        }
        texture.SetPixels32(pixels);
        texture.Apply(false, true);

        float b = radius + 1;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(b, b, b, b));
        sprite.name = texture.name;
        sprite.hideFlags = HideFlags.DontSave;
        rounded[radius] = sprite;
        return sprite;
    }

    /// <summary>
    /// Círculo blanco con borde suavizado (máscara de los mini-retratos).
    /// </summary>
    public static Sprite Circle()
    {
        if (circle != null)
            return circle;

        const int size = 128;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "Círculo (auto)",
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.DontSave
        };
        var pixels = new Color32[size * size];
        float r = size / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(r - d) * 255f));
            }
        }
        texture.SetPixels32(pixels);
        texture.Apply(false, true);

        circle = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        circle.name = texture.name;
        circle.hideFlags = HideFlags.DontSave;
        return circle;
    }

    private static readonly Dictionary<long, Sprite> outlines = new Dictionary<long, Sprite>();
    private static Sprite vignette;

    /// <summary>
    /// Contorno redondeado (marco de los sellos), en 9 partes.
    /// </summary>
    public static Sprite RoundedOutline(int radius, int thickness)
    {
        radius = Mathf.Clamp(radius, 4, 64);
        thickness = Mathf.Clamp(thickness, 1, radius);
        long key = radius * 1000L + thickness;
        if (outlines.TryGetValue(key, out Sprite cached) && cached != null)
            return cached;

        int size = radius * 2 + 4;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = $"Contorno {radius}-{thickness} (auto)", wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave
        };
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float cx = Mathf.Clamp(x + 0.5f, radius, size - radius);
                float cy = Mathf.Clamp(y + 0.5f, radius, size - radius);
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                float outer = Mathf.Clamp01(radius - d + 0.5f);
                float inner = Mathf.Clamp01(d - (radius - thickness) + 0.5f);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(outer * inner * 255f));
            }
        }
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        float b = radius + 1;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(b, b, b, b));
        sprite.name = texture.name;
        sprite.hideFlags = HideFlags.DontSave;
        outlines[key] = sprite;
        return sprite;
    }

    /// <summary>
    /// Viñeta: transparente en el centro, opaca en los bordes (se estira a la pantalla).
    /// </summary>
    public static Sprite Vignette()
    {
        if (vignette != null)
            return vignette;

        const int size = 128;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "Viñeta (auto)", wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave
        };
        var pixels = new Color32[size * size];
        float r = size / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r)) / r;
                float a = Mathf.Clamp01((d - 0.55f) / 0.6f);
                a = a * a * (3f - 2f * a);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
        }
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        vignette = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        vignette.name = texture.name;
        vignette.hideFlags = HideFlags.DontSave;
        return vignette;
    }

    private static Sprite radial;
    private static Sprite gradient;

    /// <summary>
    /// Punto de luz suave (centro opaco que se desvanece): halos, motas de polvo, vapor.
    /// </summary>
    public static Sprite Radial()
    {
        if (radial != null)
            return radial;

        const int size = 64;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "Radial (auto)", wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave
        };
        var pixels = new Color32[size * size];
        float r = size / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r)) / r;
                float a = Mathf.Clamp01(1f - d);
                a = a * a * (3f - 2f * a); // Suavizado
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
        }
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        radial = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        radial.name = texture.name;
        radial.hideFlags = HideFlags.DontSave;
        return radial;
    }

    /// <summary>
    /// Degradado vertical: opaco abajo, transparente arriba (se gira para el de arriba).
    /// </summary>
    public static Sprite Gradient()
    {
        if (gradient != null)
            return gradient;

        const int height = 128;
        var texture = new Texture2D(4, height, TextureFormat.RGBA32, false)
        {
            name = "Degradado (auto)", wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave
        };
        var pixels = new Color32[4 * height];
        for (int y = 0; y < height; y++)
        {
            float t = 1f - y / (float)(height - 1);
            byte a = (byte)(t * t * (3f - 2f * t) * 255f); // Curva en S: oscuro más tiempo, final suave
            for (int x = 0; x < 4; x++)
                pixels[y * 4 + x] = new Color32(255, 255, 255, a);
        }
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        gradient = Sprite.Create(texture, new Rect(0, 0, 4, height), new Vector2(0.5f, 0.5f), 100f);
        gradient.name = texture.name;
        gradient.hideFlags = HideFlags.DontSave;
        return gradient;
    }

    /// <summary>
    /// Recorte de un retrato para el mini-retrato: un cuadrado arriba y centrado (la cara).
    /// </summary>
    public static Rect FaceCrop(Texture texture, float widthFraction = 0.5f)
    {
        if (texture == null || texture.height == 0)
            return new Rect(0f, 0f, 1f, 1f);

        float w = Mathf.Clamp01(widthFraction);
        float h = Mathf.Clamp01(w * texture.width / texture.height);
        return new Rect((1f - w) / 2f, Mathf.Max(0f, 1f - h - 0.03f), w, h);
    }
}
