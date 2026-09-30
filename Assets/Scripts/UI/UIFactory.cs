using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Creación de controles de UI por código, ya con el tema aplicado (paneles de ajustes, confirmaciones...).
/// </summary>
public static class UIFactory
{
    private static readonly DefaultControls.Resources NoSprites = new DefaultControls.Resources();

    public static RectTransform Container(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return rect;
    }

    public static VerticalLayoutGroup VerticalLayout(RectTransform rect, float spacing, float padding)
    {
        var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        layout.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        return layout;
    }

    public static TMP_Text Label(Transform parent, string text, float size, Color color)
    {
        var go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var label = go.AddComponent<TextMeshProUGUI>();
        label.font = DefaultFont();
        label.text = text;
        label.color = color;
        LayoutKit.MultiLine(label, size);
        go.AddComponent<LayoutElement>().minHeight = size * 1.4f;
        return label;
    }

    public static Slider Slider(Transform parent, float min, float max, float value, UnityAction<float> onChange)
    {
        Theme theme = ThemeManager.Current;
        GameObject go = DefaultControls.CreateSlider(NoSprites);
        go.transform.SetParent(parent, false);
        go.AddComponent<LayoutElement>().minHeight = Theme.MinTouchSize; // Área táctil de 48 dp; la pista es fina

        // Pista fina centrada y tirador grande dentro de toda la zona táctil
        Band(go.transform.Find("Background"), 16f, 0f);
        Band(go.transform.Find("Fill Area"), 16f, 24f);
        Band(go.transform.Find("Handle Slide Area"), 64f, 24f);
        if (go.transform.Find("Handle Slide Area/Handle") is RectTransform handle)
            handle.sizeDelta = new Vector2(48f, 0f);

        var slider = go.GetComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;
        slider.onValueChanged.AddListener(onChange);

        Tint(go.transform.Find("Background"), theme.panelBorder);
        Tint(go.transform.Find("Fill Area/Fill"), theme.accent);
        Tint(go.transform.Find("Handle Slide Area/Handle"), theme.textPrimary);
        return slider;
    }

    public static Toggle Toggle(Transform parent, string text, bool value, UnityAction<bool> onChange)
    {
        Theme theme = ThemeManager.Current;
        GameObject go = DefaultControls.CreateToggle(NoSprites);
        go.transform.SetParent(parent, false);
        go.AddComponent<LayoutElement>().minHeight = Theme.MinTouchSize;

        var background = go.transform.Find("Background") as RectTransform;
        if (background != null)
        {
            background.anchorMin = background.anchorMax = new Vector2(0f, 0.5f);
            background.pivot = new Vector2(0f, 0.5f);
            background.anchoredPosition = Vector2.zero;
            background.sizeDelta = new Vector2(56f, 56f);
        }

        TMP_Text toggleLabel = ReplaceLabel(go, text, new Vector2(56f + theme.spacing, 0f));
        toggleLabel.alignment = TextAlignmentOptions.MidlineLeft;
        // Casilla con borde claro: sin marcar también se ve
        if (background != null && background.TryGetComponent(out Image box))
        {
            box.sprite = UISprites.RoundedOutline(10, 4);
            box.type = Image.Type.Sliced;
            box.color = theme.textSecondary;
            box.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore; // Se llama "Background": el tema lo pintaría de negro
        }
        if (go.transform.Find("Background/Checkmark") is RectTransform check && check.TryGetComponent(out Image mark))
        {
            check.anchorMin = Vector2.zero;
            check.anchorMax = Vector2.one;
            check.offsetMin = new Vector2(10f, 10f);
            check.offsetMax = new Vector2(-10f, -10f);
            mark.sprite = UISprites.Rounded(6);
            mark.type = Image.Type.Sliced;
            mark.color = theme.accent;
            mark.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
        }

        var toggle = go.GetComponent<Toggle>();
        toggle.isOn = value;
        toggle.onValueChanged.AddListener(onChange);
        return toggle;
    }

    public static Button Button(Transform parent, string text, bool primary, Action onClick)
    {
        Theme theme = ThemeManager.Current;
        GameObject go = DefaultControls.CreateButton(NoSprites);
        go.name = primary ? "ConfirmButton" : "Button";
        go.transform.SetParent(parent, false);
        go.AddComponent<LayoutElement>().minHeight = Theme.MinTouchSize;

        ReplaceLabel(go, text, Vector2.zero);

        var button = go.GetComponent<Button>();
        button.onClick.AddListener(() => onClick());
        go.AddComponent<ThemeRole>().role = primary ? UIRole.PrimaryButton : UIRole.SecondaryButton;
        ThemeApplier.Apply(go.transform);
        LayoutKit.Label(button, null);
        return button;
    }

    /// <summary>
    /// Fuente de los textos creados por código: la del tema o, si no hay, la predeterminada de TextMeshPro.
    /// </summary>
    public static TMP_FontAsset DefaultFont()
    {
        return ThemeManager.Current.bodyFont != null ? ThemeManager.Current.bodyFont : TMP_Settings.defaultFontAsset;
    }

    /// <summary>
    /// Sustituye la etiqueta que crea DefaultControls (sea Text de uGUI o TMP sin fuente) por una TMP propia,
    /// estirada dentro del control a partir de 'leftInset'.
    /// </summary>
    private static TMP_Text ReplaceLabel(GameObject control, string text, Vector2 leftInset)
    {
        foreach (Text legacy in control.GetComponentsInChildren<Text>(true))
            UnityEngine.Object.DestroyImmediate(legacy.gameObject);
        foreach (TMP_Text old in control.GetComponentsInChildren<TMP_Text>(true))
            UnityEngine.Object.DestroyImmediate(old.gameObject);

        var labelObject = new GameObject("Label (auto)", typeof(RectTransform));
        var rect = (RectTransform)labelObject.transform;
        rect.SetParent(control.transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(16f + leftInset.x, 6f);
        rect.offsetMax = new Vector2(-16f, -6f);

        var label = labelObject.AddComponent<TextMeshProUGUI>();
        label.font = DefaultFont();
        label.text = text;
        label.color = ThemeManager.Current.textPrimary;
        label.alignment = TextAlignmentOptions.Center;
        LayoutKit.OneLine(label, ThemeManager.Current.bodySize);
        return label;
    }

    // Franja horizontal centrada de alto fijo, con margen lateral
    private static void Band(Transform target, float height, float sideInset)
    {
        if (!(target is RectTransform rect))
            return;
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.offsetMin = new Vector2(sideInset, -height / 2f);
        rect.offsetMax = new Vector2(-sideInset, height / 2f);
    }

    private static void Tint(Transform target, Color color)
    {
        Graphic graphic = target != null ? target.GetComponent<Graphic>() : null;
        if (graphic != null)
            graphic.color = color;
    }
}
