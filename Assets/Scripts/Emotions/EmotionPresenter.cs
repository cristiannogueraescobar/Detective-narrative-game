using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reacción visual del retrato al estado emocional (ver EmotionPose): el retrato se desliza hacia la postura
/// del estado (posición, tamaño, tinte, saturación, temblor) y, al cambiar, reacciona una vez:
///   asustado: sobresalto hacia atrás · enfadado: sacudida y golpe de rojo · nervioso: gotas de sudor.
/// El tamaño lo aplica PortraitMotion (que suma la respiración); aquí solo se calcula (PoseScale).
/// Se añade por código al RawImage del retrato; no necesita nada en la escena.
/// </summary>
[RequireComponent(typeof(RawImage))]
public class EmotionPresenter : MonoBehaviour
{
    private const float ShakeOnceDuration = 0.45f;
    private const float RecoilDuration = 0.35f;
    private const float FlashDuration = 0.6f;
    private const int MaxDrops = 3;

    private static readonly int SaturationId = Shader.PropertyToID("_Saturation");
    private static readonly int GradeId = Shader.PropertyToID("_Grade");
    private static readonly int BrightnessId = Shader.PropertyToID("_Brightness");

    private RawImage image;
    private RectTransform rect;
    private Vector2 restPosition;
    private EmotionPose pose;
    private EmotionPose target;
    private float impulseTime = -1f;
    private Emotion impulseKind;
    private float trembleSeed;
    private float nextDrop;

    // Material propio para la saturación (copia la gradación del arte antiguo si la tiene)
    private Material ownMaterial;
    private float baseSaturation = 1f;

    private readonly List<(RectTransform rect, Image image, float born)> drops = new List<(RectTransform, Image, float)>();

    public Emotion Current { get; private set; } = Emotion.Tranquilo;

    /// <summary>
    /// Escala de la postura (la multiplica PortraitMotion con la respiración).
    /// </summary>
    public float PoseScale { get; private set; } = 1f;

    private static Theme T => ThemeManager.Current;

    private void Awake()
    {
        image = GetComponent<RawImage>();
        rect = (RectTransform)transform;
        restPosition = rect.anchoredPosition;
        trembleSeed = Random.value * 100f;
        pose = target = EmotionPose.For(Emotion.Tranquilo, T, GameSettings.ReduceMotion);
    }

    private void OnDisable()
    {
        // Al ocultarse, el retrato vuelve a su sitio
        if (rect != null)
            rect.anchoredPosition = restPosition;
        impulseTime = -1f;
        ClearDrops();
    }

    private void OnDestroy()
    {
        if (ownMaterial == null)
            return;
        if (Application.isPlaying)
            Destroy(ownMaterial);
        else
            DestroyImmediate(ownMaterial);
    }

    public void Apply(Emotion emotion, bool instant = false)
    {
        if (image == null)
            Awake();

        bool changed = emotion != Current;
        Current = emotion;
        target = EmotionPose.For(emotion, T, GameSettings.ReduceMotion);
        AdoptMaterial();

        if (instant || !isActiveAndEnabled || !Application.isPlaying)
        {
            pose = target;
            impulseTime = -1f;
            Render(0f);
            return;
        }

        // Reacción de un momento al entrar en el estado
        if (changed && !GameSettings.ReduceMotion && (emotion == Emotion.Asustado || emotion == Emotion.Enfadado))
        {
            impulseKind = emotion;
            impulseTime = 0f;
        }
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;
        pose = EmotionPose.Step(pose, target, dt, T.tintDuration);
        if (impulseTime >= 0f)
            impulseTime += dt;
        Render(dt);
        UpdateSweat(dt);
    }

    private void Render(float dt)
    {
        Vector2 offset = pose.offset;
        float scale = pose.scale;
        Color tint = pose.tint;

        // Temblor continuo (ruido suave, no aleatorio por fotograma)
        if (pose.tremble > 0.01f)
        {
            float t = Time.unscaledTime * pose.trembleFrequency * 0.1f;
            offset += new Vector2(Mathf.PerlinNoise(trembleSeed, t) - 0.5f, Mathf.PerlinNoise(t, trembleSeed) - 0.5f) * (2f * pose.tremble);
        }

        // Reacción al entrar en el estado
        if (impulseTime >= 0f)
        {
            if (impulseKind == Emotion.Asustado && impulseTime < RecoilDuration)
            {
                float k = Mathf.Sin(impulseTime / RecoilDuration * Mathf.PI); // Sube y vuelve
                scale -= 0.06f * k;
                offset.y -= 12f * k;
            }
            else if (impulseKind == Emotion.Enfadado)
            {
                if (impulseTime < ShakeOnceDuration)
                {
                    float damping = 1f - impulseTime / ShakeOnceDuration;
                    offset.x += Mathf.Sin(impulseTime * 60f) * T.angryShake * damping;
                }
                if (impulseTime < FlashDuration)
                    tint = Color.Lerp(tint, new Color(1f, 0.55f, 0.5f), Easing.Pulse(impulseTime / FlashDuration) * 0.6f);
            }

            if (impulseTime > Mathf.Max(RecoilDuration, FlashDuration, ShakeOnceDuration))
                impulseTime = -1f;
        }

        rect.anchoredPosition = restPosition + offset;
        PoseScale = scale;
        image.color = tint;
        if (ownMaterial != null)
            ownMaterial.SetFloat(SaturationId, baseSaturation * pose.saturation);
    }

    // ---------- Saturación ----------

    private void AdoptMaterial()
    {
        Material current = image.material;
        if (current == ownMaterial && ownMaterial != null)
            return;

        // El arte antiguo trae su gradación (y quizá el relieve 2.5D): se conserva y la emoción se aplica encima
        bool graded = current != null && current != image.defaultMaterial && current.HasProperty(SaturationId);
        Shader shader = graded ? current.shader : Shader.Find(ArtGrading.ShaderName);
        if (shader == null)
            return;

        if (ownMaterial == null || ownMaterial.shader != shader)
        {
            if (ownMaterial != null) // Cambio plano ↔ relieve (o alto contraste): el material anterior sobra
            {
                if (Application.isPlaying)
                    Destroy(ownMaterial);
                else
                    DestroyImmediate(ownMaterial);
            }
            ownMaterial = new Material(shader) { name = "Retrato (emoción)", hideFlags = HideFlags.DontSave };
        }
        if (graded)
            ownMaterial.CopyPropertiesFromMaterial(current); // Luz y relieve del tema

        baseSaturation = graded ? current.GetFloat(SaturationId) : 1f;
        ownMaterial.SetColor(GradeId, graded && current.HasProperty(GradeId) ? current.GetColor(GradeId) : Color.white);
        ownMaterial.SetFloat(BrightnessId, graded && current.HasProperty(BrightnessId) ? current.GetFloat(BrightnessId) : 1f);
        image.material = ownMaterial;
    }

    // ---------- Sudor ----------

    private void UpdateSweat(float dt)
    {
        for (int i = drops.Count - 1; i >= 0; i--)
        {
            var (dropRect, dropImage, born) = drops[i];
            float age = Time.unscaledTime - born;
            float life = T.sweatDropLife;
            if (age >= life || dropRect == null)
            {
                if (dropRect != null)
                    Destroy(dropRect.gameObject);
                drops.RemoveAt(i);
                continue;
            }

            float p = age / life;
            dropRect.anchoredPosition += new Vector2(0f, -T.sweatDropFall * dt / life);
            Color c = dropImage.color;
            c.a = 0.75f * Mathf.Clamp01(p * 6f) * (1f - p);
            dropImage.color = c;
        }

        if (!pose.sweat || !Application.isPlaying || Time.unscaledTime < nextDrop || drops.Count >= MaxDrops)
            return;

        nextDrop = Time.unscaledTime + T.sweatInterval * Random.Range(0.7f, 1.3f);
        var go = new GameObject("Gota (auto)", typeof(RectTransform));
        var dr = (RectTransform)go.transform;
        dr.SetParent(transform, false);
        Vector2 band = T.sweatBand;
        dr.anchorMin = dr.anchorMax = new Vector2(Random.Range(0.36f, 0.64f), Random.Range(band.x, band.y));
        dr.sizeDelta = new Vector2(9f, 14f);
        var di = go.AddComponent<Image>();
        di.sprite = UISprites.Rounded(ThemeManager.Current.RadiusTiny);
        di.type = Image.Type.Sliced;
        di.color = new Color(0.85f, 0.93f, 1f, 0f);
        di.raycastTarget = false;
        drops.Add((dr, di, Time.unscaledTime));
    }

    private void ClearDrops()
    {
        foreach (var (dropRect, _, _) in drops)
        {
            if (dropRect != null)
                Destroy(dropRect.gameObject);
        }
        drops.Clear();
    }
}
