using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Mantiene Assets/Resources/ArtCatalog.asset sincronizado con las imágenes de Assets/Art/.
/// </summary>
public class ArtCatalogBuilder : AssetPostprocessor
{
    public const string ArtFolder = "Assets/Art";
    public const string CatalogPath = "Assets/Resources/" + ArtCatalog.ResourceName + ".asset";

    private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        bool touchesArt = imported.Concat(deleted).Concat(moved).Concat(movedFrom)
            .Any(p => p.StartsWith(ArtFolder + "/") && !p.EndsWith(".meta"));

        if (touchesArt)
            EditorApplication.delayCall += Rebuild;
    }

    [MenuItem("Detective/Reconstruir catálogo de arte")]
    public static void Rebuild()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        var catalog = AssetDatabase.LoadAssetAtPath<ArtCatalog>(CatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<ArtCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }

        catalog.entries.Clear();

        if (AssetDatabase.IsValidFolder(ArtFolder))
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { ArtFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                catalog.entries.Add(new ArtCatalog.Entry { path = path, texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path) });
            }
        }

        catalog.entries.Sort((a, b) => string.CompareOrdinal(a.path, b.path));
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Arte] Catálogo actualizado: {catalog.entries.Count} imágenes en {ArtFolder}");
    }

    /// <summary>
    /// Para batchmode: -executeMethod ArtCatalogBuilder.RebuildFromCommandLine
    /// </summary>
    public static void RebuildFromCommandLine()
    {
        Rebuild();
        EditorApplication.Exit(0);
    }
}
