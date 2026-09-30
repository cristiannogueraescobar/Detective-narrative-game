using UnityEngine;

/// <summary>
/// TEMA VISUAL: el único sitio donde se definen colores, tipografías, duraciones y medidas de la UI.
/// El asset está en Assets/Resources/Theme.asset (se crea con Detective → Crear tema por defecto).
/// Si no existe, se usan los valores de este fichero (paleta noir por defecto).
/// Tamaños en píxeles de la resolución de referencia (1080 × 1920, vertical).
/// </summary>
[CreateAssetMenu(fileName = "Theme", menuName = "Detective/Tema visual")]
public class Theme : ScriptableObject
{
    public const float MinReadableSize = 30f; // A 1080 de ancho, por debajo de esto no se lee bien en móvil
    public const float MinTouchSize = 120f;   // 48 dp en un móvil de 1080 px (≈ 411 dp) de ancho

    [Header("Fondos y paneles")]
    public Color background = new Color32(15, 16, 18, 255);
    public Color panel = new Color32(26, 27, 31, 255);
    public Color panelBorder = new Color32(43, 44, 49, 255);
    public Color overlay = new Color32(0, 0, 0, 180);           // Fondo de ventanas modales

    [Header("Texto")]
    public Color textPrimary = new Color32(232, 226, 214, 255);  // Blanco roto cálido
    public Color textSecondary = new Color32(156, 150, 138, 255);
    public Color accent = new Color32(217, 164, 65, 255);        // Ámbar: el único color vivo de la paleta

    [Header("Chat")]
    public Color playerBubble = new Color32(36, 48, 68, 255);
    public Color suspectBubble = new Color32(42, 38, 34, 255);
    public Color playerName = new Color32(127, 167, 217, 255);
    public Color suspectName = new Color32(217, 164, 65, 255);
    public Color systemText = new Color32(170, 170, 200, 255);   // Partes de la mañana, avisos

    [Header("Botones")]
    public Color buttonPrimary = new Color32(217, 164, 65, 255);
    public Color buttonPrimaryText = new Color32(17, 17, 17, 255);
    public Color buttonSecondary = new Color32(43, 44, 49, 255);
    public Color buttonSecondaryText = new Color32(232, 226, 214, 255);
    public Color buttonDisabled = new Color32(60, 60, 64, 255);

    [Header("Estados de la investigación")]
    public Color clue = new Color32(217, 164, 65, 255);
    public Color contradiction = new Color32(217, 140, 58, 255);
    public Color success = new Color32(110, 158, 106, 255);
    public Color danger = new Color32(224, 106, 94, 255); // ≥ 4.5:1 sobre el panel
    public Color placeholder = new Color32(46, 46, 51, 255);     // Arte que aún no existe

    [Header("Estados emocionales: tinte del retrato")]
    public Color calmTint = Color.white;
    public Color nervousTint = new Color(1f, 0.96f, 0.86f);
    public Color scaredTint = new Color(0.86f, 0.9f, 1f);
    public Color angryTint = new Color(1f, 0.84f, 0.8f);
    public Color sadTint = new Color(0.82f, 0.86f, 0.95f);

    [Header("Estados emocionales: movimiento y ritmo")]
    public float nerviousShake = 1.5f;
    public float scaredShake = 3f;
    public float angryShake = 6f;
    public float nervousTextSpeed = 1.25f;
    public float scaredTextSpeed = 1.4f;
    public float angryTextSpeed = 1.15f;
    public float sadTextSpeed = 0.75f;

    [Header("Estados emocionales: postura del retrato (EmotionPose)")]
    [Range(0f, 0.1f)] public float scaredRecoil = 0.04f;   // Se hace un 4 % más pequeño: da un paso atrás
    [Range(0f, 0.1f)] public float angryLean = 0.03f;      // Se acerca un 3 %
    public float sadDrop = 14f;                            // px que baja la cabeza
    [Range(0f, 1f)] public float sadSaturation = 0.45f;
    public Vector2 sweatBand = new Vector2(0.78f, 0.9f);   // Altura (0-1) de la frente en el retrato
    public float sweatInterval = 1.4f;                     // s entre gotas
    public float sweatDropLife = 1.6f;
    public float sweatDropFall = 36f;                      // px que resbala cada gota

    [Header("Gradación del arte existente (no destructiva, ver ArtGrading)")]
    [Range(0f, 1f)] public float legacyPortraitSaturation = 0.35f; // Retratos pixel art muy saturados
    public Color legacyPortraitGrade = new Color(0.95f, 0.88f, 0.78f); // Sepia suave
    [Range(0f, 2f)] public float legacyPortraitBrightness = 0.9f;
    public bool legacyPortraitPointFilter = true;
    [Range(0f, 1f)] public float backgroundSaturation = 0.7f;
    public Color backgroundGrade = new Color(0.92f, 0.9f, 0.88f);
    [Range(0f, 2f)] public float backgroundBrightness = 0.75f;          // Fondos más oscuros: el texto va encima

    [Header("Tipografía (px a 1080 × 1920)")]
    public TMPro.TMP_FontAsset titleFont;   // Vacío = la fuente que ya tenga cada texto
    public TMPro.TMP_FontAsset bodyFont;
    public float titleSize = 72f;
    public float headingSize = 52f;
    public float bodySize = 40f;
    public float secondarySize = 32f;

    [Header("Duraciones (s)")]
    public float panelFadeDuration = 0.25f;
    public float bubbleAppearDuration = 0.2f;
    public float tintDuration = 0.35f;
    public float clueAnimDuration = 0.6f;
    public float contradictionAnimDuration = 0.5f;
    public float typewriterCharsPerSecond = 45f;

    [Header("Papel (fichas de pista, calendario, expediente)")]
    public Color paper = new Color32(233, 223, 199, 255);
    public Color paperText = new Color32(38, 33, 28, 255);
    public Color paperInk = new Color32(128, 52, 40, 255);     // Tinta roja de sellos y rótulos sobre papel
    public Color calendarRed = new Color32(150, 45, 38, 255);

    [Header("Ambiente (menú, filtro noir)")]
    [Range(0f, 0.3f)] public float grainIntensity = 0.07f;
    [Range(0f, 1f)] public float vignetteIntensity = 0.45f;
    public float deliberationSeconds = 2.2f;                    // Pausa antes del veredicto
    public Color lampGlow = new Color(1f, 0.78f, 0.45f, 0.22f);  // Halo de la lámpara del menú
    public float titleIntroDuration = 1.6f;                      // Entrada del título del menú

    [Header("Profundidad (pseudo-3D). Sutil: mejor imperceptible que mareante")]
    [Range(0f, 0.03f)] public float breathAmplitude = 0.012f;   // 1,2 % de escala
    public float breathPeriod = 4.5f;                          // s por respiración
    [Range(0f, 20f)] public float tiltDegrees = 7f;            // Inclinación al tocar un retrato
    public float tiltDuration = 0.6f;
    [Range(0f, 60f)] public float parallaxPixels = 16f;        // Desplazamiento máximo de los fondos
    public float parallaxSmoothing = 4f;                       // Mayor = sigue antes al movimiento
    public float cardFlipDuration = 0.45f;                     // Giro de carta de pistas y libreta

    [Header("Distribución vertical (px a 1080 × 1920)")]
    public float hudHeight = 140f;
    public float portraitHeight = 380f;
    public float bottomAreaHeight = 440f;
    public float padding = 32f;
    public float spacing = 16f;

    public static Theme CreateDefault()
    {
        var theme = CreateInstance<Theme>();
        theme.name = "Tema por defecto (noir)";
        theme.hideFlags = HideFlags.DontSave;
        return theme;
    }

    public Color EmotionTint(Emotion emotion)
    {
        switch (emotion)
        {
            case Emotion.Nervioso: return nervousTint;
            case Emotion.Asustado: return scaredTint;
            case Emotion.Enfadado: return angryTint;
            case Emotion.Triste: return sadTint;
            default: return calmTint;
        }
    }

    /// <summary>
    /// Color en formato #RRGGBB para el texto enriquecido de TextMeshPro.
    /// </summary>
    public static string Hex(Color color)
    {
        return "#" + ColorUtility.ToHtmlStringRGB(color);
    }
}
