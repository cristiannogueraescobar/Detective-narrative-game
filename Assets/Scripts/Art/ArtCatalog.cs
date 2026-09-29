using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Catálogo de todas las imágenes de Assets/Art/ para poder cargarlas por ruta en una build.
/// Lo regenera automáticamente el editor (ArtCatalogBuilder) al añadir o quitar imágenes.
/// </summary>
public class ArtCatalog : ScriptableObject
{
    public const string ResourceName = "ArtCatalog"; // Assets/Resources/ArtCatalog.asset

    [Serializable]
    public class Entry
    {
        public string path;
        public Texture2D texture;
    }

    public List<Entry> entries = new List<Entry>();
}
