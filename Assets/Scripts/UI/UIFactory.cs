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
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.enableWordWrapping = true;
        go.AddComponent<LayoutElement>().minHeight = size * 1.4f;
        return label;
    }

    public static Slider Slider(Transform parent, float min, float max, float value, UnityAction<float> onChange)
    {
        Theme theme = ThemeManager.Current;
        GameObject go = DefaultControls.CreateSlider(NoSprites);
        go.transform.SetParent(parent, false);
        go.AddComponent<LayoutElement>().minHeight = 72f; // Área táctil cómoda

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
        go.AddComponent<LayoutElement>().minHeight = 72f;

        // Sustituir el Text de uGUI por TextMeshPro
        Text legacy = go.GetComponentInChildren<Text>(true);
        if (legacy != null)
        {
            GameObject labelGo = legacy.gameObject;
            UnityEngine.Object.DestroyImmediate(legacy);
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = theme.bodySize;
            label.color = theme.textPrimary;
        }

        var background = go.transform.Find("Background") as RectTransform;
        if (background != null)
            background.sizeDelta = new Vector2(56f, 56f);
        Tint(background, theme.panelBorder);
        Tint(go.transform.Find("Background/Checkmark"), theme.accent);

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
        go.AddComponent<LayoutElement>().minHeight = 110f;

        Text legacy = go.GetComponentInChildren<Text>(true);
        if (legacy != null)
        {
            GameObject labelGo = legacy.gameObject;
            UnityEngine.Object.DestroyImmediate(legacy);
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.alignment = TextAlignmentOptions.Center;
        }

        var button = go.GetComponent<Button>();
        button.onClick.AddListener(() => onClick());
        go.AddComponent<ThemeRole>().role = primary ? UIRole.PrimaryButton : UIRole.SecondaryButton;
        ThemeApplier.Apply(go.transform);
        return button;
    }

    private static void Tint(Transform target, Color color)
    {
        Graphic graphic = target != null ? target.GetComponent<Graphic>() : null;
        if (graphic != null)
            graphic.color = color;
    }
}
