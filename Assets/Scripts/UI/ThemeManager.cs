using UnityEngine;

/// <summary>
/// Acceso al tema activo: Assets/Resources/Theme.asset si existe; si no, el tema por defecto en código.
/// </summary>
public static class ThemeManager
{
    public const string ResourceName = "Theme";

    private static Theme overrideTheme;
    private static Theme loaded;
    private static Theme fallback;
    private static Theme highContrast;
    private static Theme highContrastSource;

    /// <summary>
    /// Tema activo; con "alto contraste" en Ajustes, su variante de alto contraste.
    /// </summary>
    public static Theme Current
    {
        get
        {
            Theme theme = Base;
            return GameSettings.HighContrast ? HighContrastOf(theme) : theme;
        }
    }

    private static Theme Base
    {
        get
        {
            if (overrideTheme != null)
                return overrideTheme;

            if (loaded == null)
                loaded = Resources.Load<Theme>(ResourceName);
            if (loaded != null)
                return loaded;

            if (fallback == null)
                fallback = Theme.CreateDefault();
            return fallback;
        }
    }

    /// <summary>
    /// Variante de alto contraste: fondos negros, texto blanco, acentos más vivos, sin grano.
    /// </summary>
    public static Theme HighContrastOf(Theme source)
    {
        if (highContrast != null && highContrastSource == source)
            return highContrast;

        Theme t = Object.Instantiate(source);
        t.name = source.name + " (alto contraste)";
        t.hideFlags = HideFlags.DontSave;
        t.background = Color.black;
        t.panel = new Color32(8, 8, 10, 255);
        t.panelBorder = new Color32(90, 90, 96, 255);
        t.textPrimary = Color.white;
        t.textSecondary = new Color32(222, 218, 210, 255);
        t.systemText = new Color32(225, 225, 245, 255);
        t.accent = new Color32(255, 196, 84, 255);
        t.buttonPrimary = t.accent;
        t.buttonPrimaryText = Color.black;
        t.buttonSecondary = new Color32(46, 47, 54, 255);
        t.buttonSecondaryText = Color.white;
        t.playerBubble = new Color32(24, 44, 82, 255);
        t.suspectBubble = new Color32(52, 44, 36, 255);
        t.playerName = new Color32(170, 205, 255, 255);
        t.suspectName = t.accent;
        t.success = new Color32(140, 210, 130, 255);
        t.contradiction = new Color32(255, 170, 80, 255);
        t.danger = new Color32(255, 120, 108, 255);
        t.dangerOnButton = new Color32(255, 170, 160, 255);
        t.grainIntensity = 0f;
        t.vignetteIntensity = Mathf.Min(source.vignetteIntensity, 0.2f);

        highContrast = t;
        highContrastSource = source;
        return t;
    }

    /// <summary>
    /// Fuerza un tema (tests o pruebas de paleta). null vuelve al asset o al tema por defecto.
    /// </summary>
    public static void Override(Theme theme)
    {
        overrideTheme = theme;
    }
}
