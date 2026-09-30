using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Va en la vista del chat: el contenido mide como mínimo lo que la vista. Con el grupo alineado abajo, una
/// conversación corta queda junto al campo de escribir (como en cualquier app de mensajes) en vez de arriba con un
/// hueco debajo. Reacciona solo cuando cambia el tamaño de la vista (sin coste por fotograma).
/// </summary>
[ExecuteAlways]
public class FillViewport : MonoBehaviour
{
    public LayoutElement content;
    private float applied = -1f;

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
        if (content == null)
            return;
        float height = ((RectTransform)transform).rect.height;
        if (Mathf.Approximately(height, applied))
            return;
        applied = height;
        content.minHeight = height;
        LayoutRebuilder.MarkLayoutForRebuild((RectTransform)content.transform);
    }
}
