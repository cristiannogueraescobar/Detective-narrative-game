using System.Collections.Generic;

/// <summary>
/// Retratos propios de los personajes que compartían el de otro (Sesión A): salen de un original por código
/// (Tools/make_derived_portraits.py: recolor por zonas, canas, accesorios, espejo) y viven en archivos nuevos. Se usan
/// como los originales (mismo encuadre, mismo tratamiento de pixel art), con el encuadre del original reflejado si el
/// derivado está en espejo.
/// </summary>
public static class DerivedPortraits
{
    public const string Folder = "Assets/Art/Derived";

    public static readonly IReadOnlyDictionary<string, (string path, string baseKey, bool mirrored)> All =
        new Dictionary<string, (string, string, bool)>
        {
            { "Javier", (Folder + "/javier.png", "Padre", true) },    // Historia 3 (Daniel conserva el original)
            { "Lucía", (Folder + "/lucia.png", "Madre", true) },      // Historia 3 (Carmen conserva el original)
            { "Álex", (Folder + "/alex.png", "Hermano", true) },      // Historia 3 (Lucas conserva el original)
            { "Amparo", (Folder + "/amparo.png", "Vecina", false) },  // Historia 1: canas (70 años)
            { "Maruxa", (Folder + "/maruxa.png", "Vecina", false) },  // Historia 2
            { "Encarna", (Folder + "/encarna.png", "Vecina", true) }, // Historia 3: de luto
        };
}
