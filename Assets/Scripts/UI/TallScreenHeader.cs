using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// En pantallas más alargadas que la de referencia (9:16, 1080 × 1920), la cabecera del interrogatorio (retrato y
/// controles) se lleva una parte del alto que sobra (Theme.tallScreenHeaderShare); el resto sigue yendo al chat.
/// Sin esto, en un 20:9 el chat recién empezado dejaba una franja vacía en el centro. Se mide dentro del área
/// segura (sin barra de estado ni muesca) y por proporción, porque el CanvasScaler (match 0,5) no da 1080 de ancho
/// en un móvil alargado. Se recalcula al cambiar el tamaño.
/// </summary>
[ExecuteAlways]
public class TallScreenHeader : MonoBehaviour
{
    public const float ReferenceHeight = 1920f;
    public const float ReferenceWidth = 1080f;

    public LayoutElement header;
    public LayoutElement portrait;
    public float baseHeight;
    public float portraitAspect = 0.75f;
    private float applied = -1f;

    public void Configure(LayoutElement headerElement, LayoutElement portraitElement, float height)
    {
        header = headerElement;
        portrait = portraitElement;
        baseHeight = height;
        applied = -1f;
        Apply();
    }

    private void OnRectTransformDimensionsChange()
    {
        Apply();
    }

    private void OnEnable()
    {
        Apply();
    }

    public void Apply()
    {
        if (header == null)
            return;
        // El área disponible (el padre de la columna: el área segura) comparada con una 9:16 del mismo ancho
        var area = transform.parent as RectTransform;
        if (area == null || area.rect.width <= 0f)
            return;
        float extra = area.rect.height - area.rect.width * ReferenceHeight / ReferenceWidth;
        // Sin busto (composición Figura) la cabecera son solo los controles: no crece
        bool bust = portrait != null && portrait.gameObject.activeSelf;
        float height = baseHeight + (bust ? Mathf.Max(0f, extra) * ThemeManager.Current.tallScreenHeaderShare : 0f);
        // El tope del busto depende del ancho: se recalcula si cambia cualquiera de los dos
        float key = height * 100000f + area.rect.width;
        if (Mathf.Approximately(key, applied))
            return;
        applied = key;
        header.minHeight = header.preferredHeight = height;
        if (portrait != null)
        {
            // Con un tope: sin él, en una pantalla muy alta el busto se llevaba el ancho y dejaba los controles en
            // 33 px (revisión de Cristian, sesión C: "una flecha y tres barras grises")
            float maxWidth = area.rect.width * ThemeManager.Current.headerPortraitMaxShare;
            portrait.minWidth = portrait.preferredWidth = Mathf.Min(height * portraitAspect, maxWidth);
        }
        if (transform is RectTransform rect)
            LayoutRebuilder.MarkLayoutForRebuild(rect);
    }
}
