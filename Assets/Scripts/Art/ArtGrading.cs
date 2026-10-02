using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gradación NO destructiva del arte existente para que case con el tema noir: desatura, tiñe y oscurece
/// en pantalla mediante un material (shader Detective/UI/Desaturate). Los archivos originales no se tocan.
/// Parámetros en el tema ("Gradación del arte"). El arte nuevo de Assets/Art/ se muestra tal cual.
/// </summary>
public static class ArtGrading
{
    public const string ShaderName = "Detective/UI/Desaturate";
    public const string LitShaderName = "Detective/UI/LitPortrait"; // Retratos 2.5D (tema: portraitLit)

    private static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();

    public enum Kind
    {
        LegacyPortrait, // Retratos antiguos (pixel art saturado)
        LegacyBust,     // El mismo arte, grande (busto o figura del interrogatorio): contraluz del tema bustRimStrength
        Background      // Fondos e ilustraciones de la escena
    }

    public static void Apply(Graphic graphic, Kind kind)
    {
        if (graphic == null)
            return;

        Theme t = ThemeManager.Current;
        bool portrait = kind != Kind.Background;
        float rim = kind == Kind.LegacyBust ? t.bustRimStrength : t.portraitRimStrength;
        float saturation = portrait ? t.legacyPortraitSaturation : t.backgroundSaturation;
        Color grade = portrait ? t.legacyPortraitGrade : t.backgroundGrade;
        float brightness = portrait ? t.legacyPortraitBrightness : t.backgroundBrightness;

        graphic.material = portrait && t.portraitLit
            ? LitMaterialFor(t, saturation, grade, brightness * t.portraitLitBoost, rim)
            : MaterialFor(saturation, grade, brightness);

        // Pixel art nítido: filtrado sin suavizar (propiedad en memoria, no cambia la importación)
        if (portrait && t.legacyPortraitPointFilter && graphic is RawImage raw && raw.texture != null)
            raw.texture.filterMode = FilterMode.Point;
    }

    /// <summary>
    /// Quita la gradación (arte nuevo hecho ya con la paleta del juego).
    /// </summary>
    public static void Clear(Graphic graphic)
    {
        if (graphic != null)
            graphic.material = null;
    }

    private static Material LitMaterialFor(Theme t, float saturation, Color grade, float brightness, float rim)
    {
        string key = $"lit|{saturation:F2}|{ColorUtility.ToHtmlStringRGB(grade)}|{brightness:F2}|{t.portraitLightDir}|" +
                     $"{ColorUtility.ToHtmlStringRGB(t.portraitLightColor)}|{t.portraitAmbient:F2}|{t.portraitRelief:F2}|" +
                     $"{t.portraitReliefRadius:F1}|{ColorUtility.ToHtmlStringRGB(t.portraitRimColor)}|{t.portraitLampFalloff:F2}|{rim:F2}";
        if (materials.TryGetValue(key, out Material cached) && cached != null)
            return cached;

        Shader shader = Shader.Find(LitShaderName);
        if (shader == null)
        {
            Debug.LogWarning($"[Arte] Falta el shader {LitShaderName}; el retrato se muestra plano.");
            return MaterialFor(saturation, grade, brightness);
        }

        var material = new Material(shader) { name = "Relieve " + key, hideFlags = HideFlags.DontSave };
        material.SetFloat("_Saturation", saturation);
        material.SetColor("_Grade", grade);
        material.SetFloat("_Brightness", brightness);
        material.SetVector("_LightDir", t.portraitLightDir);
        material.SetColor("_LightColor", t.portraitLightColor);
        material.SetFloat("_Ambient", t.portraitAmbient);
        material.SetFloat("_Relief", t.portraitRelief);
        material.SetFloat("_Radius", t.portraitReliefRadius);
        material.SetColor("_RimColor", t.portraitRimColor);
        material.SetFloat("_Falloff", t.portraitLampFalloff);
        material.SetFloat("_RimStrength", rim);
        materials[key] = material;
        return material;
    }

    private static Material MaterialFor(float saturation, Color grade, float brightness)
    {
        string key = $"{saturation:F2}|{ColorUtility.ToHtmlStringRGB(grade)}|{brightness:F2}";
        if (materials.TryGetValue(key, out Material cached) && cached != null)
            return cached;

        Shader shader = Shader.Find(ShaderName);
        if (shader == null)
        {
            Debug.LogWarning($"[Arte] Falta el shader {ShaderName}; el arte se muestra sin gradación.");
            return null;
        }

        var material = new Material(shader) { name = "Gradación " + key, hideFlags = HideFlags.DontSave };
        material.SetFloat("_Saturation", saturation);
        material.SetColor("_Grade", grade);
        material.SetFloat("_Brightness", brightness);
        materials[key] = material;
        return material;
    }
}
