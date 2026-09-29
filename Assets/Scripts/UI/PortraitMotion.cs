using System.Collections;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Retrato "vivo": respiración sutil (escala) e inclinación tipo tarjeta al tocarlo (rotación).
/// El temblor de los estados emocionales (EmotionPresenter) mueve la posición, así que se suma por encima.
/// Con "reducir animaciones" el retrato queda quieto.
/// </summary>
public class PortraitMotion : MonoBehaviour, IPointerClickHandler
{
    private static readonly ProfilerMarker UpdateMarker = new ProfilerMarker("Detective.PortraitMotion");

    private Coroutine tiltRoutine;
    private float tiltSign = 1f;

    private void Update()
    {
        using (UpdateMarker.Auto())
        {
            Theme t = ThemeManager.Current;
            float scale = GameSettings.ReduceMotion ? 1f : Motion3D.Breath(Time.unscaledTime, t.breathPeriod, t.breathAmplitude);
            transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameSettings.ReduceMotion)
            return;

        if (tiltRoutine != null)
            StopCoroutine(tiltRoutine);

        // Se inclina hacia el lado contrario al que se toca, como una tarjeta
        var rect = (RectTransform)transform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out Vector2 local);
        tiltSign = local.x >= 0f ? 1f : -1f;
        tiltRoutine = StartCoroutine(Tilt());
    }

    private IEnumerator Tilt()
    {
        Theme t = ThemeManager.Current;
        yield return UIAnimations.Animate(t.tiltDuration, p =>
        {
            float angle = Motion3D.Tilt(p, t.tiltDegrees);
            transform.localRotation = Quaternion.Euler(angle * 0.4f, angle * tiltSign, 0f);
        });
        transform.localRotation = Quaternion.identity;
        tiltRoutine = null;
    }

    private void OnDisable()
    {
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        tiltRoutine = null;
    }
}
