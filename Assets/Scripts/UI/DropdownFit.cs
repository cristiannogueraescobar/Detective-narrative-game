using TMPro;
using UnityEngine;

/// <summary>
/// La lista de un desplegable se abre con el ancho exacto del desplegable y justo debajo (o encima, si no cabe).
/// Con anclas estiradas, TMP_Dropdown la recolocaba mal al sacarla fuera del control: aquí el ancho es explícito
/// y se actualiza cuando cambia el tamaño del desplegable.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(TMP_Dropdown))]
public class DropdownFit : MonoBehaviour
{
    public float listHeight = 600f;

    public void Fit()
    {
        var dropdown = GetComponent<TMP_Dropdown>();
        if (dropdown.template == null)
            return;
        RectTransform template = dropdown.template;
        template.anchorMin = template.anchorMax = new Vector2(0.5f, 0f);
        template.pivot = new Vector2(0.5f, 1f);
        template.anchoredPosition = Vector2.zero;
        template.sizeDelta = new Vector2(((RectTransform)transform).rect.width, listHeight);
    }

    private void OnEnable()
    {
        Fit();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (isActiveAndEnabled)
            Fit();
    }
}
