using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// Crea una versión DINÁMICA de la fuente de rótulos (Special Elite) a partir de su TTF, como asset nuevo: el
/// atlas original es estático y solo trae ASCII, así que "DÍA", "CONTRADICCIÓN" o "Nº" sacaban esas letras de
/// otra fuente. La original no se toca.
///   Unity -batchmode -nographics -projectPath . -executeMethod FontAssetBuilder.BuildTitleFont -quit
/// </summary>
public static class FontAssetBuilder
{
    public const string SourcePath = "Assets/Fonts/Special_Elite/SpecialElite-Regular.ttf";
    public const string OutputPath = "Assets/Fonts/Special_Elite/SpecialElite-Dinamica SDF.asset";

    // Latín-1 imprimible y los signos tipográficos que usa el juego
    public static string Characters()
    {
        var sb = new System.Text.StringBuilder();
        for (int c = 0x20; c <= 0x7E; c++)
            sb.Append((char)c);
        for (int c = 0xA0; c <= 0xFF; c++)
            sb.Append((char)c);
        sb.Append("·…—–«»“”‘’•ºª¿¡€");
        return sb.ToString();
    }

    [MenuItem("Detective/Crear fuente de rótulos dinámica")]
    public static void BuildTitleFont()
    {
        var ttf = AssetDatabase.LoadAssetAtPath<Font>(SourcePath);
        if (ttf == null)
        {
            Debug.LogError($"[Fuente] No está {SourcePath}");
            return;
        }

        TMP_FontAsset font = TMP_FontAsset.CreateFontAsset(ttf, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
        font.name = "SpecialElite-Dinamica SDF";
        font.TryAddCharacters(Characters(), out string missing);
        if (!string.IsNullOrEmpty(missing))
            Debug.LogWarning($"[Fuente] La fuente no trae: {missing}");

        // Con todos los caracteres ya dentro, el atlas se congela: no crece en juego ni ensucia el repositorio
        font.atlasPopulationMode = AtlasPopulationMode.Static;
        AssetDatabase.CreateAsset(font, OutputPath);
        font.material.name = font.name + " Material";
        font.atlasTexture.name = font.name + " Atlas";
        AssetDatabase.AddObjectToAsset(font.material, font);
        AssetDatabase.AddObjectToAsset(font.atlasTexture, font);
        EditorUtility.SetDirty(font);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Fuente] Creada {OutputPath} ({font.characterTable.Count} caracteres)");
    }

    /// <summary>
    /// Congela el atlas del asset ya creado (mismo GUID: el tema sigue apuntando a él).
    ///   Unity -batchmode -nographics -projectPath . -executeMethod FontAssetBuilder.FreezeTitleFont -quit
    /// </summary>
    public static void FreezeTitleFont()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(OutputPath);
        if (font == null)
            return;
        font.TryAddCharacters(Characters(), out _);
        font.atlasPopulationMode = AtlasPopulationMode.Static;
        EditorUtility.SetDirty(font);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Fuente] {OutputPath} congelada con {font.characterTable.Count} caracteres");
    }
}
