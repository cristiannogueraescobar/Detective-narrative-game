using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Parallax suave de un fondo: sigue la inclinación del móvil (acelerómetro) o, si no hay sensor, la posición
/// del dedo/ratón. La capa se amplía un poco para que al moverse no se vean los bordes. Va en su propio Canvas.
/// Con "reducir animaciones" el fondo queda quieto.
/// </summary>
public class ParallaxLayer : MonoBehaviour
{
    private static readonly ProfilerMarker UpdateMarker = new ProfilerMarker("Detective.ParallaxLayer");

    private RectTransform rect;
    private Vector2 rest;
    private Vector2 smoothedTilt;
    private Vector3? accelerometerBaseline;

    public static void AddTo(Component target)
    {
        if (target == null || target.GetComponent<ParallaxLayer>() != null)
            return;

        UIPerformance.IsolateInOwnCanvas(target);
        target.gameObject.AddComponent<ParallaxLayer>();
    }

    private void Awake()
    {
        rect = (RectTransform)transform;
        rest = rect.anchoredPosition;

        // Margen para que el desplazamiento no descubra los bordes
        float margin = 1f + 2f * ThemeManager.Current.parallaxPixels / 1080f;
        rect.localScale = new Vector3(margin, margin, 1f);

        if (Accelerometer.current != null && !Accelerometer.current.enabled)
            InputSystem.EnableDevice(Accelerometer.current);
    }

    private void Update()
    {
        using (UpdateMarker.Auto())
        {
            Theme t = ThemeManager.Current;
            if (GameSettings.ReduceMotion || t.parallaxPixels <= 0f)
            {
                rect.anchoredPosition = rest;
                return;
            }

            smoothedTilt = Vector2.Lerp(smoothedTilt, ReadTilt(), 1f - Mathf.Exp(-t.parallaxSmoothing * Time.unscaledDeltaTime));
            rect.anchoredPosition = rest + Motion3D.Parallax(smoothedTilt, t.parallaxPixels);
        }
    }

    // -1..1 en cada eje. Sin sensores ni puntero, 0 (fondo quieto).
    private Vector2 ReadTilt()
    {
        Accelerometer accelerometer = Accelerometer.current;
        if (accelerometer != null && accelerometer.enabled)
        {
            Vector3 value = accelerometer.acceleration.ReadValue();
            if (accelerometerBaseline == null && value.sqrMagnitude > 0.01f)
                accelerometerBaseline = value; // La postura con la que se sujeta el móvil al empezar es el "centro"

            if (accelerometerBaseline != null)
            {
                Vector3 delta = value - accelerometerBaseline.Value;
                return new Vector2(delta.x, delta.y) * 2.5f;
            }
        }

        Pointer pointer = Pointer.current;
        if (pointer != null && Screen.width > 0 && Screen.height > 0)
        {
            Vector2 position = pointer.position.ReadValue();
            return new Vector2(position.x / Screen.width * 2f - 1f, position.y / Screen.height * 2f - 1f);
        }

        return Vector2.zero;
    }

    private void OnDisable()
    {
        if (rect != null)
            rect.anchoredPosition = rest;
    }
}
