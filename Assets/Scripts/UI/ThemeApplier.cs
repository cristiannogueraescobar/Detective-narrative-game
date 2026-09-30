using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum UIRole
{
    Auto,            // Deducir del tipo y el nombre
    Ignore,
    Background,
    Panel,
    PrimaryButton,
    SecondaryButton,
    ButtonLabel,
    Title,
    Heading,
    Body,
    Secondary,
    Field,           // Fondo de desplegables y campos de texto (y de sus opciones)
    Accent           // Marcas de selección
}

/// <summary>
/// Aplica el tema activo a toda una jerarquía de UI por código: fondos, paneles, botones y textos.
/// El papel de cada elemento sale de ThemeRole si lo tiene; si no, de su tipo y su nombre.
/// </summary>
public static class ThemeApplier
{
    private static readonly string[] PrimaryButtonWords = { "ask", "accuse", "acuse", "start", "play", "confirm" };

    // Sprites por defecto de uGUI: los que sí se colorean con el tema
    private static readonly string[] BuiltInSprites =
        { "UISprite", "Background", "InputFieldBackground", "Knob", "Checkmark", "DropdownArrow", "UIMask" };

    /// <summary>
    /// ¿Es una ilustración (fondo, póster...) y no un sprite de interfaz? Las ilustraciones no se tiñen
    /// con el color del panel: reciben la gradación del arte (ArtGrading).
    /// </summary>
    public static bool IsArtworkSprite(string spriteName)
    {
        return !string.IsNullOrEmpty(spriteName) && System.Array.IndexOf(BuiltInSprites, spriteName) < 0;
    }

    public static UIRole RoleFor(string objectName, bool isButton, bool isText, bool insideButton)
    {
        string n = objectName.ToLowerInvariant();

        if (n.Contains("scrollbar") || n.Contains("handle") || n.Contains("viewport") || n.Contains("placeholder"))
            return UIRole.Ignore;

        if (isButton)
        {
            foreach (string word in PrimaryButtonWords)
            {
                if (n.StartsWith(word) || n.Contains("accus"))
                    return UIRole.PrimaryButton;
            }
            return UIRole.SecondaryButton;
        }

        if (isText)
        {
            if (insideButton)
                return UIRole.ButtonLabel;
            if (n.Contains("title"))
                return n.StartsWith("case") || n.StartsWith("game") || n.StartsWith("result") ? UIRole.Title : UIRole.Heading;
            if (n.Contains("hud") || n.Contains("waiting") || n.Contains("notification"))
                return UIRole.Secondary;
            return UIRole.Body;
        }

        if (n == "template")
            return UIRole.Panel;
        if (n.Contains("item background"))
            return UIRole.Field;
        if (n.Contains("checkmark"))
            return UIRole.Accent;
        if (n.Contains("panel") || n.Contains("notification"))
            return UIRole.Panel;
        if (n == "canvas" || n.Contains("background"))
            return UIRole.Background;

        return UIRole.Ignore;
    }

    public static void Apply(Transform root)
    {
        Theme theme = ThemeManager.Current;

        foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(true))
        {
            // Los retratos y el arte no se tiñen aquí
            if (graphic is RawImage)
                continue;

            if (graphic is Image image && IsArtworkSprite(image.sprite != null ? image.sprite.name : null))
            {
                ArtGrading.Apply(image, ArtGrading.Kind.Background);
                continue;
            }

            bool isText = graphic is TMP_Text;
            Button button = graphic.GetComponent<Button>();
            bool insideButton = !isText ? false : graphic.GetComponentInParent<Button>(true) != null;

            UIRole role = graphic.TryGetComponent(out ThemeRole themeRole) ? themeRole.role : UIRole.Auto;

            // El fondo de un desplegable o de un campo de texto es un control, no un panel
            if (role == UIRole.Auto && !isText && (graphic.TryGetComponent(out TMP_Dropdown _) || graphic.TryGetComponent(out TMP_InputField _)))
                role = UIRole.Field;

            // El fondo de un scroll (el chat) es un panel: sin esto conserva el gris claro de la escena
            if (role == UIRole.Auto && !isText && graphic.TryGetComponent(out ScrollRect _))
                role = UIRole.Panel;

            if (role == UIRole.Auto)
                role = RoleFor(graphic.gameObject.name, button != null && graphic.gameObject == button.gameObject, isText, insideButton);

            ApplyRole(graphic, role, theme, button);
        }
    }

    private static void ApplyRole(Graphic graphic, UIRole role, Theme theme, Button button)
    {
        switch (role)
        {
            case UIRole.Background:
                graphic.color = theme.background;
                break;
            case UIRole.Panel:
                graphic.color = theme.panel;
                break;
            case UIRole.PrimaryButton:
                graphic.color = Color.white; // El color lo pone el ColorBlock del botón
                if (button != null)
                    button.colors = ButtonColors(theme.buttonPrimary, theme.buttonDisabled);
                break;
            case UIRole.SecondaryButton:
                graphic.color = Color.white;
                if (button != null)
                    button.colors = ButtonColors(theme.buttonSecondary, theme.buttonDisabled);
                break;
            case UIRole.Field:
                Selectable owner = graphic.GetComponentInParent<Selectable>(true);
                if (owner != null && owner.targetGraphic == graphic)
                {
                    graphic.color = Color.white; // El color lo pone el ColorBlock del control
                    owner.colors = ButtonColors(theme.buttonSecondary, theme.buttonDisabled);
                }
                else
                {
                    graphic.color = theme.buttonSecondary;
                }
                break;
            case UIRole.Accent:
                graphic.color = theme.accent;
                break;
            case UIRole.ButtonLabel:
                Style((TMP_Text)graphic, theme.bodyFont, theme.bodySize, IsInPrimaryButton(graphic) ? theme.buttonPrimaryText : theme.buttonSecondaryText);
                break;
            case UIRole.Title:
                Style((TMP_Text)graphic, theme.titleFont, theme.titleSize, theme.accent);
                break;
            case UIRole.Heading:
                Style((TMP_Text)graphic, theme.titleFont, theme.headingSize, theme.textPrimary);
                break;
            case UIRole.Body:
                Style((TMP_Text)graphic, theme.bodyFont, theme.bodySize, theme.textPrimary);
                break;
            case UIRole.Secondary:
                Style((TMP_Text)graphic, theme.bodyFont, theme.secondarySize, theme.textSecondary);
                break;
        }
    }

    private static bool IsInPrimaryButton(Graphic label)
    {
        Button button = label.GetComponentInParent<Button>(true);
        if (button == null)
            return false;
        // Un rol puesto a mano en el botón manda sobre el nombre
        if (button.TryGetComponent(out ThemeRole role) && role.role != UIRole.Auto)
            return role.role == UIRole.PrimaryButton;
        return RoleFor(button.gameObject.name, true, false, false) == UIRole.PrimaryButton;
    }

    /// <summary>
    /// Política de texto: ningún texto puede desbordar su caja ni bajar del mínimo legible.
    /// - Dentro de un scroll: tamaño fijo, ajuste de línea, crece en alto.
    /// - Texto que se escribe en un campo: tamaño fijo (el campo desplaza su contenido).
    /// - Etiquetas de botón y desplegable, placeholder y textos secundarios (HUD): una línea, se reducen y ponen "…".
    /// - Títulos y cuerpos en caja fija: varias líneas, se reducen y ponen "…".
    /// </summary>
    private static void Style(TMP_Text text, TMP_FontAsset font, float size, Color color)
    {
        if (font != null)
            text.font = font;
        text.color = color;

        TMP_InputField field = text.GetComponentInParent<TMP_InputField>(true);
        if (field != null && field.textComponent == text)
        {
            text.enableAutoSizing = false;
            text.fontSize = Mathf.Max(size, Theme.MinReadableSize);
            return;
        }

        if (InsideScrollContent(text.transform))
        {
            LayoutKit.ScrollingText(text);
            text.fontSize = Mathf.Max(size, Theme.MinReadableSize);
            return;
        }

        bool oneLine = text.GetComponentInParent<TMP_Dropdown>(true) != null
                       || text.GetComponentInParent<Button>(true) != null
                       || (field != null && field.placeholder == text)
                       || size <= ThemeManager.Current.secondarySize;

        if (oneLine)
            LayoutKit.OneLine(text, size);
        else
            LayoutKit.MultiLine(text, size);
    }

    private static bool InsideScrollContent(Transform t)
    {
        ScrollRect scroll = t.GetComponentInParent<ScrollRect>(true);
        return scroll != null && scroll.content != null && t.IsChildOf(scroll.content)
               && scroll.GetComponentInParent<TMP_Dropdown>(true) == null;
    }

    private static ColorBlock ButtonColors(Color normal, Color disabled)
    {
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = normal;
        colors.highlightedColor = Color.Lerp(normal, Color.white, 0.15f);
        colors.pressedColor = Color.Lerp(normal, Color.black, 0.2f);
        colors.selectedColor = normal;
        colors.disabledColor = disabled;
        return colors;
    }
}
