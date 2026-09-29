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

    public static Theme Current
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
    /// Fuerza un tema (tests o pruebas de paleta). null vuelve al asset o al tema por defecto.
    /// </summary>
    public static void Override(Theme theme)
    {
        overrideTheme = theme;
    }
}
