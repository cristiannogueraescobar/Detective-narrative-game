using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fondo ilustrado de un panel a pantalla completa SIN deformarlo: la imagen cubre la pantalla y se recorta lo
/// que sobra (antes el sprite del panel se estiraba al 9:16). El arte original no se toca; solo cambia cómo
/// se muestra. También pone sombras de degradado para que el texto se lea encima.
/// </summary>
public static class ArtBackdrop
{
    public const string BackdropName = "Fondo (auto)";

    /// <summary>
    /// Pasa el sprite ilustrado del panel a un RawImage que cubre la pantalla con su proporción.
    /// Devuelve el RawImage (o null si el panel no tiene arte). Idempotente.
    /// </summary>
    public static RawImage Cover(RectTransform panel)
    {
        if (panel == null)
            return null;

        Transform existing = panel.Find(BackdropName);
        if (existing != null)
            return existing.GetComponent<RawImage>();

        if (!panel.TryGetComponent(out Image image) || image.sprite == null || !ThemeApplier.IsArtworkSprite(image.sprite.name))
            return null;

        Sprite sprite = image.sprite;
        var go = new GameObject(BackdropName, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(panel, false);
        rect.SetAsFirstSibling();
        UIComponents.GetOrAdd<LayoutElement>(go).ignoreLayout = true;

        var raw = go.AddComponent<RawImage>();
        raw.texture = sprite.texture;
        Rect r = sprite.textureRect;
        raw.uvRect = new Rect(r.x / sprite.texture.width, r.y / sprite.texture.height, r.width / sprite.texture.width, r.height / sprite.texture.height);
        raw.raycastTarget = false;
        ArtGrading.Apply(raw, ArtGrading.Kind.Background);

        var fitter = go.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = r.width / r.height;

        // El panel queda de color liso por detrás (lo que la imagen no cubra, que es nada)
        image.sprite = null;
        image.color = ThemeManager.Current.background;
        UIComponents.GetOrAdd<ThemeRole>(panel.gameObject).role = UIRole.Background;

        // La imagen desborda por los lados: el panel recorta
        UIComponents.GetOrAdd<RectMask2D>(panel.gameObject);
        return raw;
    }

    /// <summary>
    /// Sombra de degradado (negro → transparente) pegada arriba o abajo del panel, detrás de los controles.
    /// </summary>
    public static Image Shade(RectTransform panel, string name, bool top, float heightFraction, float alpha)
    {
        Transform existing = panel.Find(name);
        if (existing != null)
            return existing.GetComponent<Image>();

        var go = new GameObject(name, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(panel, false);
        Transform backdrop = panel.Find(BackdropName);
        rect.SetSiblingIndex(backdrop != null ? backdrop.GetSiblingIndex() + 1 : 0);
        UIComponents.GetOrAdd<LayoutElement>(go).ignoreLayout = true;

        rect.anchorMin = new Vector2(0f, top ? 1f - heightFraction : 0f);
        rect.anchorMax = new Vector2(1f, top ? 1f : heightFraction);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        if (top)
            rect.localScale = new Vector3(1f, -1f, 1f); // El degradado es opaco abajo: se voltea

        var shade = go.AddComponent<Image>();
        shade.sprite = UISprites.Gradient();
        shade.color = new Color(0f, 0f, 0f, alpha);
        shade.raycastTarget = false;
        go.AddComponent<ThemeRole>().role = UIRole.Ignore;
        return shade;
    }
}
