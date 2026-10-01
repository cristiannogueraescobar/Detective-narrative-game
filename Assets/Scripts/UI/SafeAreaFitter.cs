using UnityEngine;

/// <summary>
/// Ajusta el contenido de un panel al área segura de la pantalla (muesca, barra de gestos). El fondo del panel
/// sigue ocupando toda la pantalla; solo se encoge la columna con los controles.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private Rect applied;
    private Vector2 appliedScreen;

    /// <summary>
    /// Anclas (mínimo, máximo) normalizadas del área segura dentro de la pantalla.
    /// </summary>
    public static (Vector2 min, Vector2 max) Anchors(Rect safeArea, Vector2 screen)
    {
        if (screen.x <= 0f || screen.y <= 0f || safeArea.width <= 0f || safeArea.height <= 0f)
            return (Vector2.zero, Vector2.one);

        var min = new Vector2(Mathf.Clamp01(safeArea.xMin / screen.x), Mathf.Clamp01(safeArea.yMin / screen.y));
        var max = new Vector2(Mathf.Clamp01(safeArea.xMax / screen.x), Mathf.Clamp01(safeArea.yMax / screen.y));
        return (min, max);
    }

    private void OnEnable()
    {
        Apply();
    }

    private void Update()
    {
        // La orientación y el área segura pueden cambiar en caliente (rotación, ventana dividida)
        if (Screen.safeArea != applied || new Vector2(Screen.width, Screen.height) != appliedScreen)
            Apply();
    }

    private void Apply()
    {
        applied = Screen.safeArea;
        appliedScreen = new Vector2(Screen.width, Screen.height);
        var (min, max) = Anchors(applied, appliedScreen);

        var rect = (RectTransform)transform;
        rect.anchorMin = min;
        rect.anchorMax = max;
    }
}
