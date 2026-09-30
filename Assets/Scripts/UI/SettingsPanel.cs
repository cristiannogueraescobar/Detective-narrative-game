using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contenido del panel de Ajustes, creado por código dentro del panel que ya existe en la escena, en una lista
/// con desplazamiento: sonido (general, música, efectos), texto (tamaño, velocidad), imagen (reducir animaciones,
/// filtro noir, alto contraste) y partida (reiniciar, con confirmación). Todo se guarda y se aplica al momento.
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    public const string ListName = "AjustesControles";

    private Action onRestart;
    private RectTransform confirmation;
    private readonly List<Button> sizeButtons = new List<Button>();

    public static SettingsPanel Build(GameObject panel, Action onRestart)
    {
        SettingsPanel existing = panel.GetComponent<SettingsPanel>();
        if (existing != null)
            return existing;

        // El panel de la escena traía un texto de relleno: los controles ocupan su sitio
        Transform filler = panel.transform.Find("InstructionsText");
        if (filler != null)
            filler.gameObject.SetActive(false);

        var settings = panel.AddComponent<SettingsPanel>();
        settings.onRestart = onRestart;
        settings.CreateControls();
        return settings;
    }

    private void CreateControls()
    {
        Theme theme = ThemeManager.Current;

        ScrollRect scroll = LayoutKit.ScrollArea(transform, ListName, theme.spacing);
        RectTransform list = scroll.content;
        var layout = list.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset((int)theme.padding, (int)theme.padding, 0, (int)theme.padding);

        Section(list, "SONIDO");
        LabeledSlider(list, "Volumen general", 0f, 1f, GameSettings.Volume, v => GameSettings.Volume = v);
        LabeledSlider(list, "Música", 0f, 1f, GameSettings.MusicVolume, v => GameSettings.MusicVolume = v);
        LabeledSlider(list, "Efectos", 0f, 1f, GameSettings.SfxVolume, v => GameSettings.SfxVolume = v, () => SoundManager.Play(Sfx.Clue));

        Section(list, "TEXTO");
        UIFactory.Label(list, "Tamaño del texto", theme.bodySize, theme.textPrimary);
        TextSizeSelector(list);
        LabeledSlider(list, "Velocidad del texto", GameSettings.MinTextSpeed, GameSettings.MaxTextSpeed, GameSettings.TextSpeed, v => GameSettings.TextSpeed = v);

        Section(list, "IMAGEN");
        UIFactory.Toggle(list, "Reducir animaciones", GameSettings.ReduceMotion, v => GameSettings.ReduceMotion = v);
        UIFactory.Toggle(list, "Filtro noir (grano y viñeta)", GameSettings.NoirFilter, v => GameSettings.NoirFilter = v);
        UIFactory.Toggle(list, "Alto contraste", GameSettings.HighContrast, v => GameSettings.HighContrast = v);
        UIFactory.Toggle(list, "Vibración", GameSettings.Vibration, v => GameSettings.Vibration = v);

        Section(list, "PARTIDA");
        Button tutorial = null;
        tutorial = UIFactory.Button(list, "Repetir el tutorial", false, () =>
        {
            Tutorial.Reset();
            TMP_Text label = tutorial.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = "Tutorial activado: sale en la próxima partida";
        });
        tutorial.name = "RepetirTutorial";
        UIFactory.Button(list, "Reiniciar partida", false, AskRestart);
    }

    private static void Section(RectTransform list, string title)
    {
        Theme theme = ThemeManager.Current;
        TMP_Text heading = UIFactory.Label(list, title, theme.secondarySize, theme.accent);
        heading.name = "Seccion " + title;
        heading.characterSpacing = 10f;
        heading.fontStyle = FontStyles.Bold;
        heading.margin = new Vector4(0f, theme.spacing * 1.5f, 0f, 0f);
        LayoutKit.OneLine(heading, theme.secondarySize);
        heading.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
        UIComponents.GetOrAdd<LayoutElement>(heading.gameObject).minHeight = theme.secondarySize * 2.4f;
    }

    private static void LabeledSlider(RectTransform list, string label, float min, float max, float value, Action<float> onChange,
                                      Action onRelease = null)
    {
        Theme theme = ThemeManager.Current;
        UIFactory.Label(list, label, theme.bodySize, theme.textPrimary);
        Slider slider = UIFactory.Slider(list, min, max, value, v => onChange(v));
        if (onRelease != null)
            slider.gameObject.AddComponent<SliderRelease>().onRelease = onRelease; // Suena al soltar, para oír el nivel
    }

    // Tres botones (normal, grande, muy grande): el elegido va resaltado
    private void TextSizeSelector(RectTransform list)
    {
        Theme theme = ThemeManager.Current;
        RectTransform row = UIFactory.Container(list, "TamanoTexto", Vector2.zero, Vector2.one);
        var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = theme.spacing;
        rowLayout.childControlWidth = rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = true;
        rowLayout.childForceExpandHeight = true;
        var rowElement = row.gameObject.AddComponent<LayoutElement>();
        rowElement.minHeight = rowElement.preferredHeight = Theme.MinTouchSize;

        string[] names = { "Normal", "Grande", "Muy grande" };
        for (int i = 0; i < names.Length; i++)
        {
            int level = i;
            Button b = UIFactory.Button(row, names[i], false, () => SelectTextSize(level));
            b.name = "Tamano " + names[i];
            sizeButtons.Add(b);
        }
        MarkTextSize();
    }

    private void SelectTextSize(int level)
    {
        GameSettings.TextSizeLevel = level;
        MarkTextSize();
    }

    private void MarkTextSize()
    {
        for (int i = 0; i < sizeButtons.Count; i++)
        {
            Button b = sizeButtons[i];
            UIComponents.GetOrAdd<ThemeRole>(b.gameObject).role = i == GameSettings.TextSizeLevel ? UIRole.PrimaryButton : UIRole.SecondaryButton;
            ThemeApplier.Apply(b.transform);
        }
    }

    private void AskRestart()
    {
        if (confirmation != null)
        {
            confirmation.gameObject.SetActive(true);
            confirmation.SetAsLastSibling();
            return;
        }

        Theme theme = ThemeManager.Current;
        confirmation = UIFactory.Container(transform, "ConfirmarReinicio", Vector2.zero, Vector2.one);
        confirmation.gameObject.AddComponent<Image>().color = theme.overlay;
        UIComponents.GetOrAdd<LayoutElement>(confirmation.gameObject).ignoreLayout = true;

        // Caja centrada que mide lo que su contenido (antes quedaba media caja vacía)
        RectTransform box = UIFactory.Container(confirmation, "Caja", new Vector2(0.06f, 0.5f), new Vector2(0.94f, 0.5f));
        box.pivot = new Vector2(0.5f, 0.5f);
        var boxImage = box.gameObject.AddComponent<Image>();
        boxImage.color = theme.panel; // Más oscura que el botón secundario: "Cancelar" se ve como botón
        boxImage.sprite = UISprites.Rounded(20);
        boxImage.type = Image.Type.Sliced;
        boxImage.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
        UIFactory.VerticalLayout(box, theme.spacing, theme.padding);
        box.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        TMP_Text question = UIFactory.Label(box, "¿Empezar una partida nueva? Se perderá la investigación actual.", theme.bodySize, theme.textPrimary);
        question.alignment = TextAlignmentOptions.Center;
        question.GetComponent<LayoutElement>().minHeight = theme.bodySize * 3.2f; // Dos líneas holgadas
        UIFactory.Button(box, "Sí, reiniciar", true, () => onRestart?.Invoke());
        UIFactory.Button(box, "Cancelar", false, () => confirmation.gameObject.SetActive(false));
    }
}

/// <summary>
/// Aviso al soltar un deslizador (el de efectos suena para oír el volumen elegido).
/// </summary>
public class SliderRelease : MonoBehaviour, UnityEngine.EventSystems.IPointerUpHandler
{
    public Action onRelease;

    public void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
    {
        onRelease?.Invoke();
    }
}
