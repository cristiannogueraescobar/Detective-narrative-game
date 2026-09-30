using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Post-proceso noir de URP (gradación suave, virado frío/cálido y bloom solo en los brillos) sobre toda la
/// interfaz: con el ajuste "Filtro noir" los lienzos raíz se dibujan con la cámara principal y un Volume global
/// creado en memoria. El umbral del bloom queda por encima del color más claro del tema: el texto no brilla.
/// Sin filtro (o con Theme.postFx = false) todo vuelve al lienzo superpuesto de siempre, sin coste.
/// El grano y la viñeta siguen en FxLayer (respetan "Reducir animaciones").
/// </summary>
public static class NoirPostFx
{
    public const string VolumeName = "Post-proceso noir (auto)";
    private const float PlaneDistance = 10f;

    public static Volume Volume { get; private set; }

    /// <summary>
    /// Cámara de la escena (la principal, o la primera que dibuja en pantalla: la de Game no lleva etiqueta).
    /// </summary>
    public static Camera TargetCamera
    {
        get
        {
            if (Camera.main != null)
                return Camera.main;
            foreach (Camera c in Object.FindObjectsByType<Camera>(FindObjectsSortMode.InstanceID))
                if (c.targetTexture == null && c.isActiveAndEnabled)
                    return c;
            return null;
        }
    }

    public static bool Enabled => ThemeManager.Current.postFx && GameSettings.NoirFilter;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // GameSettings.UseStore (tests) vacía los suscriptores: se vuelve a enganchar en cada escena
        GameSettings.Changed -= Refresh;
        GameSettings.Changed += Refresh;
        Refresh();
    }

    public static void Refresh()
    {
        Camera camera = TargetCamera;
        if (camera == null)
            return;

        bool on = Enabled;
        if (camera.TryGetComponent(out UniversalAdditionalCameraData data) || on)
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = on;

        foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (!canvas.isRootCanvas)
                continue;
            if (on && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = Mathf.Min(PlaneDistance, camera.farClipPlane * 0.5f);
            }
            else if (!on && canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == camera)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
        }

        if (on && Volume == null)
            Volume = CreateVolume();
        if (Volume != null)
        {
            Volume.enabled = on;
            if (on)
                Configure(Volume.profile, ThemeManager.Current);
        }
    }

    private static Volume CreateVolume()
    {
        var go = new GameObject(VolumeName);
        var volume = go.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10f;
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.name = "Noir (memoria)";
        profile.hideFlags = HideFlags.DontSave;
        profile.Add<Bloom>();
        profile.Add<ColorAdjustments>();
        profile.Add<SplitToning>();
        volume.sharedProfile = profile;
        return volume;
    }

    private static void Configure(VolumeProfile profile, Theme t)
    {
        profile.TryGet(out Bloom bloom);
        bloom.active = t.postBloomIntensity > 0f;
        bloom.threshold.Override(t.postBloomThreshold);
        bloom.intensity.Override(t.postBloomIntensity);
        bloom.scatter.Override(0.6f);
        bloom.highQualityFiltering.Override(false); // Móvil: filtrado barato

        profile.TryGet(out ColorAdjustments grading);
        grading.active = true;
        grading.contrast.Override(t.postContrast);
        grading.saturation.Override(t.postSaturation);

        profile.TryGet(out SplitToning toning);
        toning.active = true;
        toning.shadows.Override(t.postShadowTone);
        toning.highlights.Override(t.postHighlightTone);
        toning.balance.Override(0f);
    }
}
