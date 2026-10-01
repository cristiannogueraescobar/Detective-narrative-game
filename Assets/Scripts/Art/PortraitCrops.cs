using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Encuadres de los retratos antiguos (pixel art de cuerpo entero): un plano medio 3:4 para el interrogatorio y
/// la cara para el mini-retrato del chat. Medidos sobre la parte opaca de cada imagen (Assets/Images/Suspects);
/// las imágenes no se tocan, solo se recortan al mostrarlas (uvRect).
/// </summary>
public static class PortraitCrops
{
    private static readonly Dictionary<string, (Rect bust, Rect face)> ByKey = new Dictionary<string, (Rect, Rect)>
    {
        { "Cartero", (new Rect(0.225f, 0.458f, 0.552f, 0.507f), new Rect(0.376f, 0.716f, 0.289f, 0.199f)) },
        { "Detective", (new Rect(0.248f, 0.450f, 0.565f, 0.502f), new Rect(0.346f, 0.706f, 0.296f, 0.197f)) },
        { "Dueño del Bar", (new Rect(0.182f, 0.443f, 0.603f, 0.497f), new Rect(0.299f, 0.696f, 0.316f, 0.195f)) },
        { "Hermano", (new Rect(0.134f, 0.456f, 0.746f, 0.511f), new Rect(0.314f, 0.716f, 0.391f, 0.201f)) },
        { "Madre", (new Rect(0.108f, 0.477f, 0.794f, 0.514f), new Rect(0.307f, 0.739f, 0.416f, 0.202f)) },
        { "Padre", (new Rect(0.207f, 0.460f, 0.589f, 0.516f), new Rect(0.370f, 0.722f, 0.309f, 0.203f)) },
        { "Vecina", (new Rect(0.175f, 0.525f, 0.519f, 0.461f), new Rect(0.290f, 0.759f, 0.272f, 0.181f)) },
    };

    // Los retratos derivados (DerivedPortraits) tienen la misma pose que su original: su encuadre, reflejado si van en espejo
    static PortraitCrops()
    {
        foreach (var pair in DerivedPortraits.All)
        {
            var (bust, face) = ByKey[pair.Value.baseKey];
            ByKey[pair.Key] = pair.Value.mirrored ? (Mirror(bust), Mirror(face)) : (bust, face);
        }
    }

    private static Rect Mirror(Rect r) => new Rect(1f - r.xMax, r.y, r.width, r.height);

    public static readonly Rect Full = new Rect(0f, 0f, 1f, 1f);

    public static Rect Bust(string portraitKey)
    {
        return portraitKey != null && ByKey.TryGetValue(portraitKey, out var c) ? c.bust : Full;
    }

    /// <summary>
    /// Cara para el mini-retrato; sin encuadre conocido, un cuadrado arriba y centrado.
    /// </summary>
    public static Rect Face(string portraitKey, Texture texture)
    {
        return portraitKey != null && ByKey.TryGetValue(portraitKey, out var c) ? c.face : UISprites.FaceCrop(texture);
    }

    public static IEnumerable<string> Keys => ByKey.Keys;
}
