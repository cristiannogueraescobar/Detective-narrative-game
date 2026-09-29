using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reacción visual del retrato al estado emocional: tinte suave y temblor (continuo o sacudida única).
/// Se añade por código al RawImage del retrato; no necesita configuración en la escena.
/// </summary>
[RequireComponent(typeof(RawImage))]
public class EmotionPresenter : MonoBehaviour
{
    private const float ShakeOnceDuration = 0.45f;

    private RawImage image;
    private RectTransform rect;
    private Vector2 restPosition;
    private Coroutine tintRoutine;
    private Coroutine shakeRoutine;

    public Emotion Current { get; private set; } = Emotion.Tranquilo;

    private void Awake()
    {
        image = GetComponent<RawImage>();
        rect = (RectTransform)transform;
        restPosition = rect.anchoredPosition;
    }

    private void OnDisable()
    {
        // Al ocultarse, el retrato vuelve a su sitio y a su color
        StopAllCoroutines();
        tintRoutine = null;
        shakeRoutine = null;
        if (rect != null)
            rect.anchoredPosition = restPosition;
    }

    public void Apply(Emotion emotion, bool instant = false)
    {
        if (image == null)
            Awake();

        Current = emotion;
        EmotionStyle style = EmotionStyle.For(emotion);

        if (tintRoutine != null)
            StopCoroutine(tintRoutine);

        if (instant || !isActiveAndEnabled)
            image.color = style.tint;
        else
            tintRoutine = StartCoroutine(TintTo(style.tint));

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);
        rect.anchoredPosition = restPosition;

        if (style.shakeAmplitude > 0f && isActiveAndEnabled)
            shakeRoutine = StartCoroutine(style.shakeOnce ? ShakeOnce(style) : Tremble(style));
    }

    private IEnumerator TintTo(Color target)
    {
        Color start = image.color;
        float duration = ThemeManager.Current.tintDuration;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            image.color = Color.Lerp(start, target, t / duration);
            yield return null;
        }
        image.color = target;
        tintRoutine = null;
    }

    // Temblor continuo con ruido suave (nervioso, asustado) mientras dure el estado
    private IEnumerator Tremble(EmotionStyle style)
    {
        float seed = Random.value * 100f;
        while (true)
        {
            float t = Time.unscaledTime * style.shakeFrequency * 0.1f;
            var offset = new Vector2(Mathf.PerlinNoise(seed, t) - 0.5f, Mathf.PerlinNoise(t, seed) - 0.5f) * (2f * style.shakeAmplitude);
            rect.anchoredPosition = restPosition + offset;
            yield return null;
        }
    }

    // Sacudida única que se amortigua (enfado)
    private IEnumerator ShakeOnce(EmotionStyle style)
    {
        for (float t = 0f; t < ShakeOnceDuration; t += Time.unscaledDeltaTime)
        {
            float damping = 1f - t / ShakeOnceDuration;
            float x = Mathf.Sin(t * style.shakeFrequency) * style.shakeAmplitude * damping;
            rect.anchoredPosition = restPosition + new Vector2(x, 0f);
            yield return null;
        }
        rect.anchoredPosition = restPosition;
        shakeRoutine = null;
    }
}
