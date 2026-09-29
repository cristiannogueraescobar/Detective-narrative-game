using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contenido del panel de Ajustes, creado por código dentro del panel que ya existe en la escena:
/// volumen, velocidad del texto, reducir animaciones y reiniciar partida (con confirmación).
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    private Action onRestart;
    private RectTransform confirmation;

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

        // Deja arriba sitio para el título y abajo para el botón "Volver" que ya tiene el panel
        RectTransform list = UIFactory.Container(transform, "AjustesControles", new Vector2(0.08f, 0.2f), new Vector2(0.92f, 0.82f));
        UIFactory.VerticalLayout(list, theme.spacing * 2f, theme.padding);

        UIFactory.Label(list, "Volumen", theme.bodySize, theme.textPrimary);
        UIFactory.Slider(list, 0f, 1f, GameSettings.Volume, v => GameSettings.Volume = v);

        UIFactory.Label(list, "Velocidad del texto", theme.bodySize, theme.textPrimary);
        UIFactory.Slider(list, GameSettings.MinTextSpeed, GameSettings.MaxTextSpeed, GameSettings.TextSpeed, v => GameSettings.TextSpeed = v);

        UIFactory.Toggle(list, "Reducir animaciones", GameSettings.ReduceMotion, v => GameSettings.ReduceMotion = v);

        UIFactory.Button(list, "Reiniciar partida", false, AskRestart);
    }

    private void AskRestart()
    {
        if (confirmation != null)
        {
            confirmation.gameObject.SetActive(true);
            return;
        }

        Theme theme = ThemeManager.Current;
        confirmation = UIFactory.Container(transform, "ConfirmarReinicio", Vector2.zero, Vector2.one);
        confirmation.gameObject.AddComponent<Image>().color = theme.overlay;

        RectTransform box = UIFactory.Container(confirmation, "Caja", new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.65f));
        box.gameObject.AddComponent<Image>().color = theme.panel;
        UIFactory.VerticalLayout(box, theme.spacing, theme.padding);

        UIFactory.Label(box, "¿Empezar una partida nueva? Se perderá la investigación actual.", theme.bodySize, theme.textPrimary);
        UIFactory.Button(box, "Sí, reiniciar", true, () => onRestart?.Invoke());
        UIFactory.Button(box, "Cancelar", false, () => confirmation.gameObject.SetActive(false));
    }
}
