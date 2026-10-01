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
        UIFactory.Toggle(list, "Filtro noir (grano, viñeta y color de cine)", GameSettings.NoirFilter, v => GameSettings.NoirFilter = v);
        UIFactory.Toggle(list, "Alto contraste", GameSettings.HighContrast, v => GameSettings.HighContrast = v);
        UIFactory.Toggle(list, "Vibración", GameSettings.Vibration, v => GameSettings.Vibration = v);

        Section(list, "JUEGO");
        UIFactory.Label(list, "Dificultad (para los casos nuevos)", theme.bodySize, theme.textPrimary);
        DifficultySelector(list);
        Button tutorial = null;
        tutorial = UIFactory.Button(list, "Repetir el tutorial", false, () =>
        {
            Tutorial.Reset();
            TMP_Text label = tutorial.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = "Tutorial activado";
        });
        tutorial.name = "RepetirTutorial";
        UIFactory.Button(list, GameTexts.RestartButton, false, AskRestart);
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

    // Historia / Detective / Veterano, con una línea que explica la elegida
    private readonly List<Button> difficultyButtons = new List<Button>();
    private TMP_Text difficultyHint;

    private void DifficultySelector(RectTransform list)
    {
        Theme theme = ThemeManager.Current;
        RectTransform row = UIFactory.Container(list, "Dificultad", Vector2.zero, Vector2.one);
        var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = theme.spacing;
        rowLayout.childControlWidth = rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = true;
        rowLayout.childForceExpandHeight = true;
        var rowElement = row.gameObject.AddComponent<LayoutElement>();
        rowElement.minHeight = rowElement.preferredHeight = Theme.MinTouchSize;

        foreach (DifficultyLevel level in new[] { DifficultyLevel.Historia, DifficultyLevel.Detective, DifficultyLevel.Veterano })
        {
            DifficultyLevel chosen = level;
            Button b = UIFactory.Button(row, Difficulty.Label(level), false, () => SelectDifficulty(chosen));
            b.name = "Dificultad " + Difficulty.Label(level);
            difficultyButtons.Add(b);
        }
        difficultyHint = UIFactory.Label(list, "", theme.secondarySize, theme.textSecondary);
        difficultyHint.name = "Dificultad (explicación)";
        MarkDifficulty();
    }

    private void SelectDifficulty(DifficultyLevel level)
    {
        GameSettings.Difficulty = level;
        MarkDifficulty();
    }

    private void MarkDifficulty()
    {
        for (int i = 0; i < difficultyButtons.Count; i++)
        {
            Button b = difficultyButtons[i];
            UIComponents.GetOrAdd<ThemeRole>(b.gameObject).role = i == (int)GameSettings.Difficulty ? UIRole.PrimaryButton : UIRole.SecondaryButton;
            ThemeApplier.Apply(b.transform);
        }
        if (difficultyHint != null)
            difficultyHint.text = Difficulty.Description(GameSettings.Difficulty);
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
        confirmation = ConfirmDialog.Show(transform, "ConfirmarReinicio",
            GameTexts.NewGameConfirm, GameTexts.NewGameYes, GameTexts.NewGameNo,
            () => onRestart?.Invoke());
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
