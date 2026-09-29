using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Carga de arte por ruta con sustituto: si la imagen no existe, el juego sigue funcionando
/// (quien llama decide el placeholder). Soltar la imagen en su carpeta basta para que aparezca.
/// </summary>
public static class ArtLibrary
{
#if !UNITY_EDITOR
    private static Dictionary<string, Texture2D> byPath;
#endif
    private static readonly Dictionary<Color, Texture2D> placeholders = new Dictionary<Color, Texture2D>();

    public static Texture2D Load(string path)
    {
#if UNITY_EDITOR
        // En el editor se lee directamente: no hace falta regenerar el catálogo para ver un cambio
        return UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
#else
        if (byPath == null)
        {
            byPath = new Dictionary<string, Texture2D>();
            ArtCatalog catalog = Resources.Load<ArtCatalog>(ArtCatalog.ResourceName);
            if (catalog != null)
            {
                foreach (ArtCatalog.Entry entry in catalog.entries)
                    byPath[entry.path] = entry.texture;
            }
        }

        return byPath.TryGetValue(path, out Texture2D texture) ? texture : null;
#endif
    }

    public static Texture2D LoadFirst(IEnumerable<string> paths)
    {
        foreach (string path in paths)
        {
            Texture2D texture = Load(path);
            if (texture != null)
                return texture;
        }

        return null;
    }

    /// <summary>
    /// Textura de color plano para cuando falta el arte.
    /// </summary>
    public static Texture2D Placeholder(Color color)
    {
        if (placeholders.TryGetValue(color, out Texture2D cached) && cached != null)
            return cached;

        var texture = new Texture2D(4, 4) { name = "Placeholder", hideFlags = HideFlags.DontSave };
        var pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        texture.SetPixels(pixels);
        texture.Apply();

        placeholders[color] = texture;
        return texture;
    }
}
