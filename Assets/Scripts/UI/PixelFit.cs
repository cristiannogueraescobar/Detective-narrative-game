using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Va en la caja de un retrato (busto de la cabecera, figura del centro) y coloca dentro su RawImage con la proporción
/// del recorte y sin ampliarla por encima de Theme.portraitMaxMagnification (PixelScale). Sustituye al
/// AspectRatioFitter, que estiraba el pixel art a lo que midiera la caja. Apoyado abajo y centrado: la etiqueta de
/// estado queda sobre el pie del retrato. Se recalcula al cambiar la caja y al cambiar de retrato (Apply).
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class PixelFit : MonoBehaviour
{
    public RawImage target;
    public float fallbackAspect = 0.75f; // Sin textura (color plano): 3:4

    public static PixelFit For(RectTransform box, RawImage image)
    {
        var fit = UIComponents.GetOrAdd<PixelFit>(box.gameObject);
        fit.target = image;
        fit.Apply();
        return fit;
    }

    private void OnEnable()
    {
        Apply();
    }

    private void OnRectTransformDimensionsChange()
    {
        Apply();
    }

    public void Apply()
    {
        if (target == null)
            return;
        Rect box = ((RectTransform)transform).rect;
        Vector2 size;
        Texture texture = target.texture;
        if (texture != null)
        {
            var texels = new Vector2(texture.width * target.uvRect.width, texture.height * target.uvRect.height);
            Canvas canvas = target.canvas != null ? target.canvas.rootCanvas : null;
            float density = canvas != null && canvas.renderMode != RenderMode.WorldSpace ? canvas.scaleFactor : 1f;
            size = PixelScale.Fit(box.size, texels, density, ThemeManager.Current.portraitMaxMagnification);
        }
        else
        {
            float width = Mathf.Min(box.width, box.height * fallbackAspect);
            size = new Vector2(width, width / fallbackAspect);
        }

        RectTransform rect = target.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
    }
}
