using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Ancho de una burbuja de chat: el de su texto más largo sin partir, con un máximo del 80 % de la fila.
/// Manda sobre el ancho que calcula su VerticalLayoutGroup (prioridad mayor); el alto lo sigue dando el grupo.
/// El máximo se mide sobre el contenido del chat (estable) y no sobre la fila (que aún no tiene ancho
/// la primera vez que se calcula).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class BubbleWidth : UIBehaviour, ILayoutElement
{
    private RectTransform widthSource;
    private float reserved;
    private float padding;
    private TMP_Text[] texts;
    private float preferred = -1f;
    private string cacheKey;

    public void Configure(RectTransform source, float reservedWidth, float horizontalPadding, params TMP_Text[] measured)
    {
        widthSource = source;
        reserved = reservedWidth;
        padding = horizontalPadding;
        texts = measured;
        SetDirty();
    }

    /// <summary>
    /// Ancho máximo de la burbuja con un ancho de contenido dado.
    /// </summary>
    public static float MaxWidth(float contentWidth, float reservedWidth)
    {
        return Mathf.Max(0f, contentWidth - reservedWidth) * ChatView.MaxBubbleFraction;
    }

    public void CalculateLayoutInputHorizontal()
    {
        if (widthSource == null || texts == null)
        {
            preferred = -1f;
            return;
        }

        float available = widthSource.rect.width;
        if (widthSource.TryGetComponent(out LayoutGroup group))
            available -= group.padding.horizontal;

        float max = Mathf.Min(MaxWidth(available, 0f), available - reserved);

        // El texto de medir no cambia casi nunca: sin esto, cada fotograma del "escribiendo…" remedía todo el chat
        var key = new System.Text.StringBuilder();
        key.Append(max);
        foreach (TMP_Text text in texts)
            key.Append('|').Append(text != null && text.gameObject.activeSelf ? text.text : "");
        string k = key.ToString();
        if (k == cacheKey && preferred >= 0f)
            return;
        cacheKey = k;

        float want = 0f;
        foreach (TMP_Text text in texts)
        {
            if (text == null || !text.gameObject.activeSelf || string.IsNullOrEmpty(text.text))
                continue;
            want = Mathf.Max(want, text.GetPreferredValues(text.text, float.PositiveInfinity, float.PositiveInfinity).x);
        }

        preferred = Mathf.Clamp(Mathf.Ceil(want) + padding + 2f, padding + 40f, Mathf.Max(max, padding + 40f));
    }

    public void CalculateLayoutInputVertical() { }

    public float minWidth => preferred;
    public float preferredWidth => preferred;
    public float flexibleWidth => 0f;
    public float minHeight => -1f;
    public float preferredHeight => -1f;
    public float flexibleHeight => -1f;
    public int layoutPriority => 2;

    private void SetDirty()
    {
        if (IsActive())
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SetDirty();
    }
}
