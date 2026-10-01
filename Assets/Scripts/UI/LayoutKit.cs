using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Construcción de layouts reales (LayoutGroup + LayoutElement) sobre los objetos de la escena.
/// Nada de posiciones absolutas: cada panel es una columna; las filas reparten el ancho; lo largo va en scroll.
/// Todo lo creado lleva "(auto)" en el nombre.
/// </summary>
public static class LayoutKit
{
    public const string ColumnName = "Layout (auto)";

    /// <summary>
    /// Columna a pantalla completa dentro del panel (con márgenes del tema). Devuelve null si ya existía:
    /// el layout se construye una sola vez.
    /// </summary>
    public static RectTransform Column(RectTransform panel, out bool created)
    {
        Transform existing = panel.Find(ColumnName);
        created = existing == null;
        if (!created)
            return (RectTransform)existing;

        Theme t = ThemeManager.Current;
        RectTransform column = UIFactory.Container(panel, ColumnName, Vector2.zero, Vector2.one);
        var layout = column.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset((int)t.padding, (int)t.padding, (int)t.padding, (int)t.padding);
        layout.spacing = t.spacing;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        column.gameObject.AddComponent<SafeAreaFitter>(); // El fondo llega al borde; los controles no
        return column;
    }

    /// <summary>
    /// Fila horizontal de altura fija; sus hijos se reparten el ancho.
    /// </summary>
    public static RectTransform Row(RectTransform column, string name, float height)
    {
        RectTransform row = UIFactory.Container(column, name + " (auto)", Vector2.zero, Vector2.one);
        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = ThemeManager.Current.spacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;
        Size(row, height: height);
        return row;
    }

    /// <summary>
    /// Mete un elemento existente en un contenedor de layout con los tamaños indicados.
    /// preferred = -1: sin preferencia; flexible = 0: no crece.
    /// </summary>
    public static RectTransform Put(Component element, RectTransform container, float height = -1f, float flexibleHeight = 0f,
                                    float width = -1f, float flexibleWidth = 0f)
    {
        if (element == null)
            return null;

        var rect = (RectTransform)element.transform;
        rect.SetParent(container, false);
        rect.SetAsLastSibling();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        Size(rect, height, flexibleHeight, width, flexibleWidth);
        return rect;
    }

    public static void Size(RectTransform rect, float height = -1f, float flexibleHeight = 0f, float width = -1f, float flexibleWidth = 0f)
    {
        LayoutElement element = UIComponents.GetOrAdd<LayoutElement>(rect.gameObject);
        element.ignoreLayout = false;
        element.minHeight = height >= 0f && flexibleHeight <= 0f ? height : -1f;
        element.preferredHeight = height;
        element.flexibleHeight = flexibleHeight;
        element.minWidth = width >= 0f && flexibleWidth <= 0f ? width : -1f;
        element.preferredWidth = width;
        element.flexibleWidth = flexibleWidth;
    }

    public static void Spacer(RectTransform column, float flexible)
    {
        RectTransform spacer = UIFactory.Container(column, "Espacio (auto)", Vector2.zero, Vector2.one);
        Size(spacer, height: 0f, flexibleHeight: flexible);
        UIComponents.GetOrAdd<LayoutElement>(spacer.gameObject).minHeight = 0f;
    }

    /// <summary>
    /// Elemento superpuesto (avisos, libreta): fuera del layout, a pantalla completa o con anclas dadas.
    /// </summary>
    public static void Overlay(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        UIComponents.GetOrAdd<LayoutElement>(rect.gameObject).ignoreLayout = true;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// Mete un texto largo en un área con scroll vertical que ocupa el hueco disponible de la columna.
    /// El texto crece hacia abajo y nunca desborda.
    /// </summary>
    public static ScrollRect Scrollable(TMP_Text text, RectTransform column, float flexibleHeight = 1f, float minHeight = 200f)
    {
        if (text == null)
            return null;

        ScrollRect scroll = ScrollArea(column, text.name + " Scroll (auto)", 0f);
        Size((RectTransform)scroll.transform, height: minHeight, flexibleHeight: flexibleHeight);
        UIComponents.GetOrAdd<LayoutElement>(scroll.gameObject).minHeight = minHeight;

        // Un poco de aire a los lados: la cursiva sobresale y la máscara del scroll la cortaba
        scroll.content.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(8, 8, 0, 0);

        var textRect = (RectTransform)text.transform;
        textRect.SetParent(scroll.content, false);
        textRect.localScale = Vector3.one;
        textRect.localRotation = Quaternion.identity;
        ScrollingText(text);
        return scroll;
    }

    /// <summary>
    /// Zona con desplazamiento solo vertical: lo que se añada a su 'content' se apila y crece hacia abajo.
    /// </summary>
    public static ScrollRect ScrollArea(Transform parent, string name, float spacing)
    {
        RectTransform area = UIFactory.Container(parent, name, Vector2.zero, Vector2.one);

        RectTransform viewport = UIFactory.Container(area, "Viewport (auto)", Vector2.zero, Vector2.one);
        viewport.gameObject.AddComponent<RectMask2D>();

        RectTransform content = UIFactory.Container(viewport, "Content (auto)", new Vector2(0f, 1f), Vector2.one);
        content.pivot = new Vector2(0.5f, 1f);
        var contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        contentLayout.childControlWidth = contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.spacing = spacing;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var scroll = area.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;
        // Un Image transparente recoge el arrastre del dedo en toda el área
        area.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        return scroll;
    }

    /// <summary>
    /// Barra de desplazamiento discreta: fina, pista casi invisible y tirador del color del borde.
    /// </summary>
    public static void StyleScrollbar(Scrollbar bar)
    {
        if (bar == null)
            return;

        Theme t = ThemeManager.Current;
        var rect = (RectTransform)bar.transform;
        rect.sizeDelta = new Vector2(12f, rect.sizeDelta.y);
        if (bar.TryGetComponent(out Image track))
            track.color = new Color(t.panelBorder.r, t.panelBorder.g, t.panelBorder.b, 0.35f);
        if (bar.handleRect != null && bar.handleRect.TryGetComponent(out Image handle))
            handle.color = t.textSecondary;
        bar.colors = ColorBlock.defaultColorBlock;
    }

    /// <summary>
    /// Texto que vive en un scroll: tamaño fijo del tema, ajuste de línea, crece en alto.
    /// </summary>
    public static void ScrollingText(TMP_Text text)
    {
        TextStyle.Set(text, TextStyle.Mode.Scrolling, ThemeManager.Current.bodySize);
    }

    /// <summary>
    /// Texto de una línea que nunca desborda: se reduce hasta el mínimo legible y después pone "…".
    /// </summary>
    public static void OneLine(TMP_Text text, float maxSize)
    {
        TextStyle.Set(text, TextStyle.Mode.OneLine, maxSize);
    }

    /// <summary>
    /// Texto de varias líneas en caja fija: se reduce hasta el mínimo legible y después pone "…".
    /// </summary>
    public static void MultiLine(TMP_Text text, float maxSize)
    {
        TextStyle.Set(text, TextStyle.Mode.MultiLine, maxSize);
    }

    /// <summary>
    /// Botón con etiqueta corta, centrada y ocupando el botón con margen interior.
    /// </summary>
    public static void Label(Button button, string text)
    {
        if (button == null)
            return;

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label == null)
            return;

        if (text != null)
            label.text = text;
        var rect = label.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(16f, 6f);
        rect.offsetMax = new Vector2(-16f, -6f);
        rect.localScale = Vector3.one;
        label.alignment = TextAlignmentOptions.Center;
        OneLine(label, ThemeManager.Current.bodySize);
    }
}
