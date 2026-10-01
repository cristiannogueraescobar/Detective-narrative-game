using UnityEngine;

/// <summary>
/// En escritorio (la build de Windows) el juego se abre como una ventana vertical 9:16 al 85 % del alto de la
/// pantalla, como un móvil: a pantalla completa en un monitor apaisado la interfaz vertical quedaba estirada.
/// En móvil, en el editor y en batchmode (prueba de humo) no hace nada.
/// </summary>
public static class DesktopWindow
{
    private const float ScreenShare = 0.85f; // Deja sitio a la barra de título y a la de tareas (portátil de 768 px)

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Apply()
    {
        if (Application.isEditor || Application.isMobilePlatform || Application.isBatchMode)
            return;

        Resolution display = Screen.currentResolution;
        Vector2Int size = SizeFor(display.width, display.height);
        Screen.SetResolution(size.x, size.y, FullScreenMode.Windowed);
        Debug.Log($"[DesktopWindow] Pantalla {display.width}x{display.height} → ventana {size.x}x{size.y}");
    }

    /// <summary>
    /// Tamaño de la ventana para una pantalla de 'width' × 'height' píxeles.
    /// </summary>
    public static Vector2Int SizeFor(int width, int height)
    {
        if (width <= 0 || height <= 0)
            return new Vector2Int(540, 960);

        int h = Mathf.RoundToInt(height * ScreenShare);
        int w = Mathf.RoundToInt(h * 9f / 16f);
        int maxWidth = Mathf.FloorToInt(width * ScreenShare);
        if (w > maxWidth)
        {
            w = maxWidth;
            h = Mathf.RoundToInt(w * 16f / 9f);
        }
        return new Vector2Int(w, h);
    }
}
