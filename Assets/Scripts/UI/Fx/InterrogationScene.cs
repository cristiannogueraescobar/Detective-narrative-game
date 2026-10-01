using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cuánta tensión pinta cada estado en la escena del interrogatorio (parte pura, con tests).
/// </summary>
public static class SceneTension
{
    public static float Vignette(Emotion emotion)
    {
        switch (emotion)
        {
            case Emotion.Triste: return 0.18f;
            case Emotion.Nervioso: return 0.32f;
            case Emotion.Asustado: return 0.42f;
            case Emotion.Enfadado: return 0.36f;
            default: return 0f;
        }
    }

    // El enfado enrojece los bordes; el resto, sombra
    public static Color Tint(Emotion emotion) => emotion == Emotion.Enfadado ? new Color(0.36f, 0.03f, 0.02f) : Color.black;

    // px de temblor de la figura grande (la del busto ya tiembla con EmotionPresenter)
    public static float Tremble(Emotion emotion, bool reduceMotion)
    {
        if (reduceMotion)
            return 0f;
        switch (emotion)
        {
            case Emotion.Nervioso: return 1.6f;
            case Emotion.Asustado: return 2.4f;
            default: return 0f;
        }
    }
}

/// <summary>
/// Escena del interrogatorio (Sesión A, bloque 4): el hueco del centro, que con pocas preguntas quedaba vacío, muestra
/// al sospechoso de cuerpo entero, grande y tenue, como de pie al otro lado de la mesa; una viñeta oscurece los bordes
/// según la tensión de su estado; al cambiar de sospechoso, el nuevo entra con un fundido y un deslizamiento corto;
/// una pista nueva da un destello y una contradicción sacude el retrato. Con "reducir animaciones", solo color y
/// fundidos. Se apaga desde el tema (interrogationStage, tensionVignette).
/// </summary>
public class InterrogationScene : MonoBehaviour
{
    public const string StageName = "Escena (auto)";
    public const string VignetteName = "Viñeta de tensión (auto)";

    private RawImage stage;
    private AspectRatioFitter stageFit;
    private RectTransform stageRect;
    private Image vignette;
    private Image flash;
    private RectTransform portrait;
    private CanvasGroup portraitGroup;
    private Emotion emotion = Emotion.Tranquilo;
    private float vignetteAlpha;
    private float entry = 1f;        // 0 → 1 durante la entrada de un sospechoso
    private float entryFrom;         // px de deslizamiento
    private Coroutine pulse;

    private static Theme T => ThemeManager.Current;

    public static readonly Rect StageCrop = new Rect(0f, 0.34f, 1f, 0.66f);

    public RawImage Stage => stage;
    public Image Vignette => vignette;
    public float VignetteAlpha => vignette != null ? vignette.color.a : 0f;
    public float Entry => entry;

    /// <summary>
    /// Monta la escena: la figura dentro de la zona del chat (detrás de los mensajes, sin desplazarse con ellos) y la
    /// viñeta sobre todo el panel. 'portraitBox' es el busto de la cabecera (entra y se sacude con la escena).
    /// </summary>
    public static InterrogationScene Build(RectTransform panel, RectTransform chatArea, RectTransform portraitBox)
    {
        var scene = UIComponents.GetOrAdd<InterrogationScene>(panel.gameObject);
        scene.portrait = portraitBox;
        if (portraitBox != null)
            scene.portraitGroup = UIComponents.GetOrAdd<CanvasGroup>(portraitBox.gameObject);

        if (chatArea != null && chatArea.Find(StageName) == null)
        {
            RectTransform holder = UIFactory.Container(chatArea, StageName, new Vector2(0.04f, 0.16f), new Vector2(0.96f, 1f));
            holder.SetAsFirstSibling();
            UIComponents.GetOrAdd<LayoutElement>(holder.gameObject).ignoreLayout = true;
            RectTransform figure = UIFactory.Container(holder, "Figura", Vector2.zero, Vector2.one);
            scene.stage = figure.gameObject.AddComponent<RawImage>();
            scene.stage.raycastTarget = false;
            scene.stageRect = figure;
            scene.stageFit = figure.gameObject.AddComponent<AspectRatioFitter>();
            scene.stageFit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            scene.stage.gameObject.AddComponent<Decorative>();
            // Destello de pista: un velo ámbar sobre la figura
            RectTransform glow = UIFactory.Container(holder, "Destello", Vector2.zero, Vector2.one);
            scene.flash = glow.gameObject.AddComponent<Image>();
            scene.flash.raycastTarget = false;
            scene.flash.sprite = UISprites.Radial(); // Centro luminoso que se desvanece
            scene.flash.color = Color.clear;
            glow.gameObject.SetActive(false);
        }

        if (panel.Find(VignetteName) == null)
        {
            // Más grande que el panel: el centro transparente del sprite cubre la pantalla y solo se oscurecen los bordes
            RectTransform v = UIFactory.Container(panel, VignetteName, new Vector2(-0.2f, -0.12f), new Vector2(1.2f, 1.12f));
            UIComponents.GetOrAdd<LayoutElement>(v.gameObject).ignoreLayout = true;
            scene.vignette = v.gameObject.AddComponent<Image>();
            scene.vignette.sprite = UISprites.Vignette();
            scene.vignette.raycastTarget = false;
            scene.vignette.color = Color.clear;
            v.gameObject.AddComponent<Decorative>();
        }
        scene.ApplyTheme();
        return scene;
    }

    public void ApplyTheme()
    {
        if (stage != null)
            stage.transform.parent.gameObject.SetActive(T.interrogationStage);
        if (vignette != null)
            vignette.gameObject.SetActive(T.tensionVignette);
    }

    /// <summary>
    /// Sospechoso en pantalla (o cambio de sospechoso, con entrada si 'changed' y no es instantáneo).
    /// </summary>
    public void Show(Texture2D texture, bool legacyArt, Emotion state, bool changed, bool instant)
    {
        emotion = state;
        if (stage != null)
        {
            stage.texture = texture;
            // De la cabeza a los muslos: se ve la postura y la cara es grande (las piernas quedarían detrás de la mesa)
            stage.uvRect = StageCrop;
            if (texture != null)
                stageFit.aspectRatio = texture.width * StageCrop.width / (texture.height * StageCrop.height);
            if (legacyArt)
                ArtGrading.Apply(stage, ArtGrading.Kind.LegacyPortrait);
            else
                ArtGrading.Clear(stage);
        }
        if (changed && !instant && Application.isPlaying && isActiveAndEnabled)
        {
            entry = 0f;
            entryFrom = GameSettings.ReduceMotion ? 0f : 36f;
        }
        else
        {
            entry = 1f;
        }
        Render(0f);
    }

    public void SetEmotion(Emotion state)
    {
        emotion = state;
    }

    /// <summary>
    /// Pista nueva: destello ámbar sobre la figura (y un leve latido si no se reducen animaciones).
    /// </summary>
    public void ClueFlash()
    {
        if (flash == null || !isActiveAndEnabled || !T.interrogationStage)
            return;
        if (pulse != null)
            StopCoroutine(pulse);
        pulse = StartCoroutine(Glow());
    }

    private IEnumerator Glow()
    {
        flash.gameObject.SetActive(true);
        Color amber = T.accent;
        float duration = 0.9f;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float k = Easing.Pulse(t / duration);
            flash.color = new Color(amber.r, amber.g, amber.b, 0.35f * k);
            if (!GameSettings.ReduceMotion && stageRect != null)
                stageRect.localScale = Vector3.one * (1f + 0.035f * k);
            yield return null;
        }
        flash.color = Color.clear;
        flash.gameObject.SetActive(false);
        if (stageRect != null)
            stageRect.localScale = Vector3.one;
        pulse = null;
    }

    /// <summary>
    /// Contradicción: el busto se sacude (el HUD ya lo hace); con "reducir animaciones", nada se mueve.
    /// </summary>
    public void ContradictionShake()
    {
        if (portrait != null)
            UIAnimations.Shake(this, portrait, 10f, T.contradictionAnimDuration);
    }

    private void Update()
    {
        Render(Time.unscaledDeltaTime);
    }

    private void Render(float dt)
    {
        // Viñeta: se acerca a la tensión del estado en ~0,6 s
        if (vignette != null && vignette.gameObject.activeSelf)
        {
            float target = SceneTension.Vignette(emotion);
            vignetteAlpha = dt <= 0f ? target : Mathf.MoveTowards(vignetteAlpha, target, dt / 0.6f);
            Color tint = SceneTension.Tint(emotion);
            vignette.color = new Color(tint.r, tint.g, tint.b, vignetteAlpha);
        }

        if (entry < 1f)
            entry = Mathf.Min(1f, entry + dt / (GameSettings.ReduceMotion ? 0.2f : 0.4f));
        float eased = Easing.OutCubic(entry);

        if (stage != null)
        {
            stage.color = new Color(1f, 1f, 1f, T.stageAlpha * eased);
            float tremble = SceneTension.Tremble(emotion, GameSettings.ReduceMotion);
            float time = Time.unscaledTime;
            Vector2 shake = tremble > 0f
                ? new Vector2(Mathf.PerlinNoise(time * 9f, 0.3f) - 0.5f, Mathf.PerlinNoise(0.7f, time * 9f) - 0.5f) * (2f * tremble)
                : Vector2.zero;
            if (stageRect != null)
                stageRect.anchoredPosition = new Vector2(entryFrom * (1f - eased), 0f) + shake;
        }
        if (portraitGroup != null)
            portraitGroup.alpha = Mathf.Lerp(0.25f, 1f, eased);
    }
}
