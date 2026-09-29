using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum PanelSlot
{
    Ignore,
    Background, // Ilustración detrás del contenido
    Top,        // Título
    Field,      // Desplegable bajo el título
    Middle,     // Texto o lista que ocupa el centro
    Bottom      // Botones apilados abajo (zona del pulgar)
}

/// <summary>
/// Distribución vertical genérica para los paneles secundarios (intro, resultado, acusación, libreta, menús):
/// título arriba, desplegables debajo, contenido en el centro y botones apilados abajo a todo el ancho.
/// Se aplica por código sobre los hijos directos del panel; la escena no se modifica. Medidas en el tema.
/// </summary>
public static class MobilePanelLayout
{
    private const float TitleHeight = 220f;
    private const float FieldHeight = 130f;
    private const float ButtonHeight = 140f;

    public static PanelSlot Classify(string name, bool hasButton, bool hasText, bool hasDropdown, bool hasScroll, bool hasImage)
    {
        string n = name.ToLowerInvariant();

        if (hasButton)
            return PanelSlot.Bottom;
        if (hasDropdown)
            return PanelSlot.Field;
        if (hasScroll)
            return PanelSlot.Middle;
        if (hasText)
            return n.Contains("title") ? PanelSlot.Top : PanelSlot.Middle;
        if (hasImage)
            return PanelSlot.Background;
        return PanelSlot.Ignore;
    }

    public static void Apply(RectTransform panel)
    {
        if (panel == null)
            return;

        Theme t = ThemeManager.Current;
        float pad = t.padding;
        float gap = t.spacing;

        // El panel ocupa toda la pantalla
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.offsetMin = panel.offsetMax = Vector2.zero;

        var top = new List<RectTransform>();
        var fields = new List<RectTransform>();
        var middle = new List<RectTransform>();
        var bottom = new List<RectTransform>();
        var background = new List<RectTransform>();

        foreach (Transform child in panel)
        {
            if (!(child is RectTransform rect) || !child.gameObject.activeSelf || child.name.EndsWith("(auto)") && child.GetComponent<Button>() == null)
                continue;

            PanelSlot slot = Classify(child.name,
                child.GetComponent<Button>() != null,
                child.GetComponent<TMP_Text>() != null,
                child.GetComponent<TMP_Dropdown>() != null,
                child.GetComponent<ScrollRect>() != null,
                child.GetComponent<Graphic>() != null);

            switch (slot)
            {
                case PanelSlot.Top: top.Add(rect); break;
                case PanelSlot.Field: fields.Add(rect); break;
                case PanelSlot.Middle: middle.Add(rect); break;
                case PanelSlot.Bottom: bottom.Add(rect); break;
                case PanelSlot.Background: background.Add(rect); break;
            }
        }

        // De arriba abajo: títulos y desplegables
        float y = pad;
        foreach (RectTransform rect in top)
        {
            Band(rect, fromTop: true, y, TitleHeight, pad);
            y += TitleHeight + gap;
        }
        foreach (RectTransform rect in fields)
        {
            Band(rect, fromTop: true, y, FieldHeight, pad);
            y += FieldHeight + gap;
        }
        float topUsed = y;

        // De abajo arriba: botones apilados (el primero de la jerarquía queda más arriba)
        float b = pad;
        for (int i = bottom.Count - 1; i >= 0; i--)
        {
            Band(bottom[i], fromTop: false, b, ButtonHeight, pad);
            b += ButtonHeight + gap;
        }
        float bottomUsed = b;

        // Centro: el contenido reparte el hueco restante
        foreach (RectTransform rect in middle)
            Fill(rect, topUsed, bottomUsed, pad);

        foreach (RectTransform rect in background)
        {
            Fill(rect, topUsed, bottomUsed, pad);
            if (rect.TryGetComponent(out Image image))
                image.preserveAspect = true;
            rect.SetAsFirstSibling();
        }
    }

    private static void Band(RectTransform rect, bool fromTop, float offset, float height, float pad)
    {
        float anchorY = fromTop ? 1f : 0f;
        rect.anchorMin = new Vector2(0f, anchorY);
        rect.anchorMax = new Vector2(1f, anchorY);
        rect.pivot = new Vector2(0.5f, anchorY);
        rect.localScale = Vector3.one;

        if (fromTop)
        {
            rect.offsetMin = new Vector2(pad, -(offset + height));
            rect.offsetMax = new Vector2(-pad, -offset);
        }
        else
        {
            rect.offsetMin = new Vector2(pad, offset);
            rect.offsetMax = new Vector2(-pad, offset + height);
        }
    }

    private static void Fill(RectTransform rect, float fromTop, float fromBottom, float pad)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
        rect.offsetMin = new Vector2(pad, fromBottom);
        rect.offsetMax = new Vector2(-pad, -fromTop);
    }
}
