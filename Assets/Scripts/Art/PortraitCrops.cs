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

    // Figura entera (de los pies a la cabeza, sin márgenes ni el humo del cigarro) para la rueda con alturas reales.
    // Medido sobre la parte opaca de cada imagen (la región conectada más grande y lo que la toca: botella, manos)
    private static readonly Dictionary<string, Rect> FigureByKey = new Dictionary<string, Rect>
    {
        { "Padre", new Rect(0.079f, 0.036f, 0.845f, 0.920f) },
        { "Madre", new Rect(0.108f, 0.056f, 0.794f, 0.916f) },
        { "Hermano", new Rect(0.131f, 0.038f, 0.750f, 0.910f) },
        { "Vecina", new Rect(0.194f, 0.146f, 0.614f, 0.781f) },
        { "Cartero", new Rect(0.033f, 0.043f, 0.938f, 0.903f) },
        { "Dueño del Bar", new Rect(0.083f, 0.035f, 0.842f, 0.886f) },
        { "Detective", new Rect(0.233f, 0.038f, 0.595f, 0.897f) },
    };

    public static Rect Figure(string portraitKey)
    {
        if (portraitKey == null)
            return Full;
        if (FigureByKey.TryGetValue(portraitKey, out Rect r))
            return r;
        if (DerivedPortraits.All.TryGetValue(portraitKey, out var d) && FigureByKey.TryGetValue(d.baseKey, out Rect b))
            return d.mirrored ? Mirror(b) : b;
        return Full;
    }

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

    // ---------- Arte nuevo (Assets/Art/Portraits/<artId>_<estado>.png) ----------

    public struct Crops
    {
        public Rect figure; // Pies a cabeza: rueda con alturas reales
        public Rect bust;   // Plano medio: interrogatorio y ficha del culpable
        public Rect face;   // Mini-retrato del chat
    }

    // Encuadre del encargo (docs/art/javier/BRIEF.md): 768x1024, figura al 90 % del alto con los pies a 40 px y la
    // cabeza centrada; busto y cara con las proporciones medianas de los 7 retratos antiguos. Es la previsión hasta
    // que llegue el arte: entonces se sustituye por lo que mida Tools/measure_portrait.py (BRIEF 6.3).
    private static readonly Crops BriefFraming = new Crops
    {
        figure = new Rect(0.200f, 0.039f, 0.600f, 0.900f),
        bust = new Rect(0.248f, 0.453f, 0.504f, 0.504f),
        face = new Rect(0.368f, 0.714f, 0.264f, 0.198f),
    };

    private static readonly Dictionary<string, Crops> NewArtByArtId = new Dictionary<string, Crops>
    {
        { "javier", BriefFraming },
    };

    public static bool HasNewArtCrops(string artId) => artId != null && NewArtByArtId.ContainsKey(artId);

    public static Crops NewArt(string artId) => NewArtByArtId[artId];

    /// <summary>
    /// Encuadres de un retrato: los del arte antiguo por su portraitKey; los del arte nuevo por su artId si están
    /// medidos; si no, la imagen entera y la cara genérica (lo que hacía el juego antes).
    /// </summary>
    public static Crops For(string portraitKey, string artId, bool legacy, Texture texture)
    {
        if (legacy)
            return new Crops { figure = Figure(portraitKey), bust = Bust(portraitKey), face = Face(portraitKey, texture) };
        if (HasNewArtCrops(artId))
            return NewArtByArtId[artId];
        return new Crops { figure = Full, bust = Full, face = UISprites.FaceCrop(texture) };
    }

    /// <summary>
    /// Proporción ancho/alto en pantalla de la figura recortada (la rueda reparte el sitio con ella).
    /// </summary>
    public static float Aspect(Texture texture, Rect figure)
    {
        return texture != null ? texture.width * figure.width / (texture.height * figure.height) : 0.5f;
    }
}
