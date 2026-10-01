using UnityEditor;
using UnityEngine;

/// <summary>
/// Crea Assets/Resources/Theme.asset con la paleta por defecto. A partir de ahí, cambiar la paleta
/// es editar ese asset en el Inspector.
/// </summary>
public static class ThemeAssetCreator
{
    public const string AssetPath = "Assets/Resources/" + ThemeManager.ResourceName + ".asset";

    [MenuItem("Detective/Crear tema por defecto")]
    public static void Create()
    {
        if (AssetDatabase.LoadAssetAtPath<Theme>(AssetPath) != null)
        {
            Debug.Log($"[Tema] Ya existe {AssetPath}");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Theme>(AssetPath);
            return;
        }

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        Theme theme = ScriptableObject.CreateInstance<Theme>();
        AssetDatabase.CreateAsset(theme, AssetPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Tema] Creado {AssetPath}");
    }

    /// <summary>
    /// Batchmode: -executeMethod ThemeAssetCreator.CreateFromCommandLine (crea también el catálogo de arte).
    /// </summary>
    public static void CreateFromCommandLine()
    {
        Create();
        ArtCatalogBuilder.Rebuild();
        EditorApplication.Exit(0);
    }
}
