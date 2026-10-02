using TMPro;
using UnityEngine;

/// <summary>
/// Política de tamaño de un texto (LayoutKit). Se reaplica al activarse: TextMeshPro reinicia a sus valores
/// por defecto (autoajuste hasta 72 px) los textos creados por código dentro de un panel inactivo la primera
/// vez que el panel se activa.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class TextStyle : MonoBehaviour
{
    public enum Mode
    {
        OneLine,    // Una línea: se reduce hasta el mínimo legible y después "…"
        MultiLine,  // Varias líneas en caja fija: se reduce y después "…"
        Scrolling,  // Dentro de un scroll: tamaño fijo, crece en alto
        Fixed,      // Tamaño fijo (texto que se escribe en un campo)
        OneLineOrTwo // Una línea que se reduce; si al mínimo legible aún no cabe, dos líneas (barra de arriba)
    }

    public Mode mode;
    public float maxSize;
    public TMP_FontAsset font;

    private float appliedScale = -1f;

    private void OnEnable()
    {
        Apply();
        GameSettings.Changed += OnSettingsChanged; // Tamaño del texto en caliente
    }

    private void OnDisable()
    {
        GameSettings.Changed -= OnSettingsChanged;
    }

    private void OnSettingsChanged()
    {
        if (!Mathf.Approximately(appliedScale, GameSettings.TextScale))
            Apply();
    }

    public void Apply()
    {
        TMP_Text text = GetComponent<TMP_Text>();
        if (text == null)
            return;

        if (font != null)
            text.font = font;
        else if (text.font == null)
            text.font = UIFactory.DefaultFont();

        appliedScale = GameSettings.TextScale;
        float max = Mathf.Max(maxSize * appliedScale, Theme.MinReadableSize);
        switch (mode)
        {
            case Mode.OneLine:
                text.enableAutoSizing = true;
                text.fontSizeMax = max;
                text.fontSizeMin = Theme.MinReadableSize;
                text.textWrappingMode = TextWrappingModes.NoWrap;
                text.overflowMode = TextOverflowModes.Ellipsis;
                break;
            case Mode.OneLineOrTwo:
                text.enableAutoSizing = true;
                text.fontSizeMax = max;
                text.fontSizeMin = Theme.MinReadableSize;
                text.overflowMode = TextOverflowModes.Ellipsis;
                FitLines(text);
                break;
            case Mode.MultiLine:
                text.enableAutoSizing = true;
                text.fontSizeMax = max;
                text.fontSizeMin = Theme.MinReadableSize;
                text.textWrappingMode = TextWrappingModes.Normal;
                text.overflowMode = TextOverflowModes.Ellipsis;
                break;
            case Mode.Scrolling:
                text.enableAutoSizing = false;
                text.fontSize = max;
                text.textWrappingMode = TextWrappingModes.Normal;
                text.overflowMode = TextOverflowModes.Overflow;
                break;
            case Mode.Fixed:
                text.enableAutoSizing = false;
                text.fontSize = max;
                break;
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        if (mode == Mode.OneLineOrTwo && TryGetComponent(out TMP_Text text))
            FitLines(text);
    }

    /// <summary>
    /// Tras cambiar el texto de uno en modo OneLineOrTwo: vuelve a decidir si cabe en una línea.
    /// </summary>
    public static void Refit(TMP_Text text)
    {
        if (text != null && text.TryGetComponent(out TextStyle style) && style.mode == Mode.OneLineOrTwo)
            FitLines(text);
    }

    private static void FitLines(TMP_Text text)
    {
        if (!text.isActiveAndEnabled || ((RectTransform)text.transform).rect.width <= 0f)
            return;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.ForceMeshUpdate(true, true);
        if (text.isTextTruncated)
        {
            text.textWrappingMode = TextWrappingModes.Normal;
            text.ForceMeshUpdate(true, true);
        }
    }

    public static void Set(TMP_Text text, Mode mode, float maxSize)
    {
        if (text == null)
            return;

        TextStyle style = UIComponents.GetOrAdd<TextStyle>(text.gameObject);
        style.mode = mode;
        style.maxSize = maxSize;
        style.font = text.font;
        style.Apply();
    }
}
