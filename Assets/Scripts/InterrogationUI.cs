using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI CONTROLLER FINAL - Con botón acusar anticipado y panel pistas arreglado
/// </summary>
public class InterrogationUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameManager gameManager;

    [Header("Paneles Principales")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private GameObject interrogationPanel;
    [SerializeField] private GameObject accusationPanel;
    [SerializeField] private GameObject resultPanel;

    [Header("Intro")]
    [SerializeField] private TMP_Text caseTitleText;
    [SerializeField] private TMP_Text caseDescriptionText;
    [SerializeField] private Button startButton;

    [Header("Interrogatorio - Input")]
    [SerializeField] private TMP_Dropdown suspectDropdown;
    [SerializeField] private TMP_InputField questionInput;
    [SerializeField] private Button askButton;
    [SerializeField] private Button endDayButton;
    [SerializeField] private Button accuseNowButton; // NUEVO: Botón acusar anticipado
    [Tooltip("Selector 'Mostrar prueba'. Si se deja vacío se crea clonando el desplegable de sospechosos")]
    [SerializeField] private TMP_Dropdown evidenceDropdown;

    [Header("Interrogatorio - Display")]
    [SerializeField] private TMP_Text conversationText;
    [SerializeField] private ScrollRect conversationScroll;
    [SerializeField] private TMP_Text hudText;
    [SerializeField] private TMP_Text waitingText;

    [Header("NUEVO - Imágenes de Sospechosos")]
    [SerializeField] private RawImage suspectImage;
    [SerializeField] private Texture2D padreGif;
    [SerializeField] private Texture2D madreGif;
    [SerializeField] private Texture2D hermanoGif;
    [SerializeField] private Texture2D vecinaGif;
    [SerializeField] private Texture2D detectiveGif;
    [SerializeField] private Texture2D carteroGif;
    [SerializeField] private Texture2D duenioBarGif;

    [Header("Pistas y Contradicciones")]
    [SerializeField] private TMP_Text cluesText;
    [SerializeField] private TMP_Text contradictionsText;
    [SerializeField] private GameObject clueNotification;
    [SerializeField] private TMP_Text clueNotificationText;

    [Header("Botones Panel Pistas")]
    [SerializeField] private Button viewCluesButton;
    [SerializeField] private Button closeCluesButton;
    [SerializeField] private GameObject cluesPanel;

    [Header("Acusacion")]
    [SerializeField] private TMP_Dropdown accusationDropdown;
    [SerializeField] private Button accuseButton;
    [Header("Móvil")]
    [Tooltip("Recoloca el interrogatorio para jugar en vertical con una mano (medidas en el tema)")]
    [SerializeField] private bool applyMobileLayout = true;

    [Tooltip("Botón para volver al interrogatorio. Si se deja vacío se crea bajo el de acusar")]
    [SerializeField] private Button accusationBackButton;

    [Header("Resultado")]
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private TMP_Text resultDetailsText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    private const string NoEvidenceOption = GameTexts.NoEvidence;

    private string currentSuspectId;
    private List<SuspectView> suspects = new List<SuspectView>();
    private List<SuspectView> accusationOptions = new List<SuspectView>();
    private List<ClueData> evidenceOptions = new List<ClueData>();
    private readonly ConversationStore conversations = new ConversationStore(); // Una conversación por sospechoso
    private Dictionary<string, Texture2D> suspectImages = new Dictionary<string, Texture2D>();
    private readonly List<string> visibleClueNames = new List<string>(); // Pistas del aviso en pantalla
    private readonly Dictionary<string, Emotion> emotionBySuspect = new Dictionary<string, Emotion>();
    private EmotionPresenter emotionPresenter;
    private ChatView chat;
    private TMP_Text emotionLabel;
    private FxLayer fx;                  // Efectos (solo en juego)
    private int unseenClues;             // Pistas nuevas desde la última vez que se abrió la libreta
    private TMP_Text clueBadge;
    private int maxDaysValue = 7;
    private int dayValue = 1;
    private ChatEntry pendingQuestion;   // Pregunta enviada que aún espera respuesta
    private string pendingSuspectId;
    private int questionsUsedToday;
    private int questionsPerDay = 5;
    private static Theme T => ThemeManager.Current; // Todos los colores y tamaños salen del tema

    public void Initialize(GameManager gm)
    {
        gameManager = gm;

        // Botones principales
        if (startButton != null)
            UIComponents.SetOnlyListener(startButton, StartInterrogation);

        if (askButton != null)
            UIComponents.SetOnlyListener(askButton, OnAskButtonClick);

        if (endDayButton != null)
            UIComponents.SetOnlyListener(endDayButton, OnEndDayClick);

        // NUEVO: Botón acusar anticipado
        if (accuseNowButton != null)
            UIComponents.SetOnlyListener(accuseNowButton, OnAccuseNowClick);

        if (accuseButton != null)
            UIComponents.SetOnlyListener(accuseButton, OnAccuseClick);

        if (restartButton != null)
            UIComponents.SetOnlyListener(restartButton, () => gameManager.RestartGame());

        if (menuButton != null)
            UIComponents.SetOnlyListener(menuButton, () => gameManager.BackToMenu());

        if (suspectDropdown != null)
            suspectDropdown.onValueChanged.AddListener(OnSuspectChanged);

        // Botones panel pistas (ARREGLADO)
        if (viewCluesButton != null)
        {
            UIComponents.SetOnlyListener(viewCluesButton, ShowCluesPanel);
        }

        if (closeCluesButton != null)
        {
            UIComponents.SetOnlyListener(closeCluesButton, HideCluesPanel);
        }

        BuildLayout();

        if (Application.isPlaying && interrogationPanel != null)
            fx = FxLayer.Ensure(interrogationPanel.GetComponentInParent<Canvas>());

        // Lo que se anima cada fotograma, en su propio Canvas (rendimiento en móvil)
        UIPerformance.IsolateInOwnCanvas(suspectImage);
        if (suspectImage != null && suspectImage.GetComponent<PortraitMotion>() == null)
            suspectImage.gameObject.AddComponent<PortraitMotion>(); // Respiración e inclinación al tocar
        UIPerformance.IsolateInOwnCanvas(conversationScroll != null ? conversationScroll.content : null);
        UIPerformance.IsolateInOwnCanvas(hudText);

        // Iconos (con sustituto si aún no hay arte)
        AddIcon(viewCluesButton != null ? viewCluesButton.transform : null, ArtSlots.IconNotebook);
        AddIcon(clueNotification != null ? clueNotification.transform : null, ArtSlots.IconClue);

        // Reacciones por código: no necesitan nada en la escena
        if (suspectImage != null)
            emotionPresenter = UIComponents.GetOrAdd<EmotionPresenter>(suspectImage.gameObject);

        if (clueNotification != null)
            clueNotification.SetActive(false);

        if (cluesPanel != null)
            cluesPanel.SetActive(false);

        // Ocultar todos los paneles al inicio
        HideAllPanels();

        Debug.Log("[InterrogationUI] Inicializado - Todos los paneles ocultos");
    }

    /// <summary>
    /// Distribución móvil (vertical, una mano) de todos los paneles del juego con LayoutGroups reales.
    /// Idempotente. La valida LayoutValidationTests a 1080 × 1920.
    /// </summary>
    public void BuildLayout()
    {
        // Imágenes de sospechosos (también en la vista previa del editor)
        suspectImages["Padre"] = padreGif;
        suspectImages["Madre"] = madreGif;
        suspectImages["Hermano"] = hermanoGif;
        suspectImages["Vecina"] = vecinaGif;
        suspectImages["Detective"] = detectiveGif;
        suspectImages["Cartero"] = carteroGif;
        suspectImages["Dueño del Bar"] = duenioBarGif;

        EnsureEvidenceDropdown();
        if (!applyMobileLayout)
            return;

        Transform root = interrogationPanel != null ? interrogationPanel.transform.root : transform.root;
        ThemeApplier.Apply(root);

        BuildInterrogationLayout();
        BuildNotebookLayout();
        BuildClueNoticeLayout();
        BuildIntroLayout();
        BuildAccusationLayout();
        BuildResultLayout();

        // Las capas se dibujan por encima del contenido del panel
        if (cluesPanel != null)
            cluesPanel.transform.SetAsLastSibling();
    }

    private void BuildInterrogationLayout()
    {
        if (interrogationPanel == null)
            return;

        RectTransform column = LayoutKit.Column((RectTransform)interrogationPanel.transform, out bool created);
        if (!created)
            return;

        // Fila 1: día y preguntas + libreta
        RectTransform hud = LayoutKit.Row(column, "HUD", Theme.MinTouchSize);
        if (hudText != null)
        {
            LayoutKit.Put(hudText, hud, flexibleWidth: 1f);
            hudText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutKit.OneLine(hudText, T.bodySize);
        }
        LayoutKit.Put(viewCluesButton, hud, width: 260f);
        LayoutKit.Label(viewCluesButton, "Libreta");

        // Cabecera: retrato en plano medio a la izquierda; a su derecha, a quién interrogas y las acciones del día
        // (cada una con su sitio). Así el chat se queda con la mayor parte de la pantalla.
        float headerHeight = 3f * Theme.MinTouchSize + 2f * T.spacing;
        RectTransform header = LayoutKit.Row(column, "Cabecera", headerHeight);
        header.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = false;

        if (suspectImage != null)
        {
            RectTransform portraitBox = UIFactory.Container(header, "Retrato (auto)", Vector2.zero, Vector2.one);
            LayoutKit.Size(portraitBox, width: headerHeight * 0.75f);
            var portrait = suspectImage.rectTransform;
            portrait.SetParent(portraitBox, false);
            portrait.anchorMin = Vector2.zero;
            portrait.anchorMax = Vector2.one;
            portrait.offsetMin = portrait.offsetMax = Vector2.zero;
            portrait.localScale = Vector3.one;
            var fitter = UIComponents.GetOrAdd<AspectRatioFitter>(suspectImage.gameObject);
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 0.75f;

            // Estado emocional a la vista: una etiqueta sobre el pie del retrato
            RectTransform chip = UIFactory.Container(portraitBox, "Estado (auto)", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            chip.pivot = new Vector2(0.5f, 0f);
            chip.anchoredPosition = new Vector2(0f, 8f);
            chip.sizeDelta = new Vector2(headerHeight * 0.72f, 50f);
            UIComponents.GetOrAdd<LayoutElement>(chip.gameObject).ignoreLayout = true;
            var chipImage = chip.gameObject.AddComponent<Image>();
            chipImage.sprite = UISprites.Rounded(25);
            chipImage.type = Image.Type.Sliced;
            chipImage.color = new Color(0f, 0f, 0f, 0.8f);
            chipImage.raycastTarget = false;
            chip.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
            emotionLabel = UIFactory.Label(chip, "", T.secondarySize, T.textPrimary);
            emotionLabel.name = "EstadoTexto";
            emotionLabel.alignment = TextAlignmentOptions.Center;
            emotionLabel.fontStyle = FontStyles.Bold;
            emotionLabel.characterSpacing = 4f;
            emotionLabel.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
            var labelRect = emotionLabel.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(8f, 0f);
            labelRect.offsetMax = new Vector2(-8f, 0f);
            LayoutKit.OneLine(emotionLabel, T.secondarySize);
        }

        RectTransform side = UIFactory.Container(header, "Controles (auto)", Vector2.zero, Vector2.one);
        LayoutKit.Size(side, flexibleWidth: 1f);
        var sideLayout = side.gameObject.AddComponent<VerticalLayoutGroup>();
        sideLayout.spacing = T.spacing;
        sideLayout.childControlWidth = sideLayout.childControlHeight = true;
        sideLayout.childForceExpandWidth = true;
        sideLayout.childForceExpandHeight = false;
        PutDropdown(suspectDropdown, side);
        LayoutKit.Put(endDayButton, side, height: Theme.MinTouchSize);
        LayoutKit.Label(endDayButton, "Fin del día");
        LayoutKit.Put(accuseNowButton, side, height: Theme.MinTouchSize);
        LayoutKit.Label(accuseNowButton, "Acusar");

        // Chat: todo el hueco que queda, solo desplazamiento vertical
        if (conversationScroll != null)
        {
            LayoutKit.Put(conversationScroll, column, height: 300f, flexibleHeight: 1f);
            ConfigureChatScroll();
        }

        // "Esperando respuesta" ahora es la burbuja de "escribiendo…" del chat
        if (waitingText != null)
            waitingText.gameObject.SetActive(false);

        // Abajo, al alcance del pulgar: la prueba que se muestra y la pregunta
        PutDropdown(evidenceDropdown, column);

        RectTransform ask = LayoutKit.Row(column, "Pregunta", 120f);
        if (questionInput != null)
        {
            LayoutKit.Put(questionInput, ask, flexibleWidth: 1f);
            ConfigureQuestionInput();
        }
        LayoutKit.Put(askButton, ask, width: 220f);
        LayoutKit.Label(askButton, "Enviar");
    }

    private void ConfigureChatScroll()
    {
        conversationScroll.horizontal = false;
        conversationScroll.vertical = true;
        if (conversationScroll.horizontalScrollbar != null)
        {
            conversationScroll.horizontalScrollbar.gameObject.SetActive(false);
            conversationScroll.horizontalScrollbar = null;
        }
        conversationScroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        LayoutKit.StyleScrollbar(conversationScroll.verticalScrollbar);

        RectTransform viewport = conversationScroll.viewport;
        if (viewport != null)
        {
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = viewport.offsetMax = Vector2.zero;
        }

        RectTransform content = conversationScroll.content;
        if (content != null)
        {
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = content.offsetMax = Vector2.zero;
            var layout = UIComponents.GetOrAdd<VerticalLayoutGroup>(content.gameObject);
            layout.padding = new RectOffset(16, 16, 8, 8);
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            UIComponents.GetOrAdd<ContentSizeFitter>(content.gameObject).verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        // Las burbujas sustituyen al texto único de la escena
        if (conversationText != null)
            conversationText.gameObject.SetActive(false);
        chat = UIComponents.GetOrAdd<ChatView>(conversationScroll.gameObject);
        chat.Initialize();
    }

    private void ConfigureQuestionInput()
    {
        // La escena traía la pregunta de ejemplo como texto escrito, no como placeholder
        questionInput.SetTextWithoutNotify(string.Empty);

        RectTransform area = questionInput.textViewport;
        if (area != null)
        {
            area.anchorMin = Vector2.zero;
            area.anchorMax = Vector2.one;
            area.offsetMin = new Vector2(20f, 8f);
            area.offsetMax = new Vector2(-20f, -8f);
        }

        if (questionInput.placeholder is TMP_Text placeholder)
        {
            placeholder.text = "Escribe tu pregunta…";
            placeholder.rectTransform.anchorMin = Vector2.zero;
            placeholder.rectTransform.anchorMax = Vector2.one;
            placeholder.rectTransform.offsetMin = placeholder.rectTransform.offsetMax = Vector2.zero;
            LayoutKit.OneLine(placeholder, T.bodySize);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            placeholder.color = T.textSecondary;
            placeholder.fontStyle = FontStyles.Italic;
        }

        if (questionInput.textComponent != null)
        {
            questionInput.textComponent.rectTransform.anchorMin = Vector2.zero;
            questionInput.textComponent.rectTransform.anchorMax = Vector2.one;
            questionInput.textComponent.rectTransform.offsetMin = questionInput.textComponent.rectTransform.offsetMax = Vector2.zero;
            questionInput.textComponent.alignment = TextAlignmentOptions.MidlineLeft;
        }
    }

    private static void PutDropdown(TMP_Dropdown dropdown, RectTransform column)
    {
        if (dropdown == null)
            return;

        LayoutKit.Put(dropdown, column, height: Theme.MinTouchSize);

        if (dropdown.captionText != null)
        {
            RectTransform label = dropdown.captionText.rectTransform;
            label.anchorMin = Vector2.zero;
            label.anchorMax = Vector2.one;
            label.offsetMin = new Vector2(24f, 6f);
            label.offsetMax = new Vector2(-72f, -6f); // Sitio para la flecha
            LayoutKit.OneLine(dropdown.captionText, T.bodySize);
            dropdown.captionText.alignment = TextAlignmentOptions.MidlineLeft;
        }

        if (dropdown.itemText != null)
            LayoutKit.OneLine(dropdown.itemText, T.bodySize);

        if (dropdown.template != null)
        {
            dropdown.template.anchorMin = new Vector2(0f, 0f);
            dropdown.template.anchorMax = new Vector2(1f, 0f);
            dropdown.template.pivot = new Vector2(0.5f, 1f);
            dropdown.template.sizeDelta = new Vector2(0f, 600f);
        }
    }

    private void BuildNotebookLayout()
    {
        if (cluesPanel == null)
            return;

        var panel = (RectTransform)cluesPanel.transform;
        LayoutKit.Overlay(panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        RectTransform column = LayoutKit.Column(panel, out bool created);
        if (!created)
            return;

        Transform title = panel.Find("CluesTitleText");
        if (title != null)
        {
            LayoutKit.Put(title, column, height: 110f);
            if (title.TryGetComponent(out TMP_Text titleText))
            {
                titleText.text = GameTexts.NotebookTitle;
                titleText.fontStyle = FontStyles.Bold;
                titleText.characterSpacing = 8f;
            }
        }

        if (contradictionsText != null)
            contradictionsText.gameObject.SetActive(false); // Las contradicciones van en la libreta

        ScrollRect notebookScroll = LayoutKit.Scrollable(cluesText, column);

        LayoutKit.Put(closeCluesButton, column, height: Theme.MinTouchSize);
        LayoutKit.Label(closeCluesButton, "Cerrar");

        // Aspecto de libreta: papel crema, tinta oscura y el margen rojo a la izquierda
        if (panel.TryGetComponent(out Image paper))
        {
            paper.sprite = null; // El sprite gris de la escena apagaba el crema
            paper.color = T.paper;
            UIComponents.GetOrAdd<ThemeRole>(paper.gameObject).role = UIRole.Ignore;
        }
        if (title != null && title.TryGetComponent(out TMP_Text paperTitle))
        {
            paperTitle.color = T.paperText;
            UIComponents.GetOrAdd<ThemeRole>(paperTitle.gameObject).role = UIRole.Ignore;
        }
        if (cluesText != null)
        {
            cluesText.color = T.paperText;
            UIComponents.GetOrAdd<ThemeRole>(cluesText.gameObject).role = UIRole.Ignore;
        }
        if (notebookScroll != null && notebookScroll.TryGetComponent(out Image scrollImage))
        {
            scrollImage.color = Color.clear;
            UIComponents.GetOrAdd<ThemeRole>(scrollImage.gameObject).role = UIRole.Ignore;
        }
        RectTransform margin = UIFactory.Container(panel, "Margen (auto)", new Vector2(0f, 0f), new Vector2(0f, 1f));
        margin.pivot = new Vector2(0f, 0.5f);
        margin.sizeDelta = new Vector2(3f, 0f);
        margin.anchoredPosition = new Vector2(T.padding * 0.55f, 0f);
        margin.SetSiblingIndex(0);
        UIComponents.GetOrAdd<LayoutElement>(margin.gameObject).ignoreLayout = true;
        var marginImage = margin.gameObject.AddComponent<Image>();
        marginImage.color = new Color(T.paperInk.r, T.paperInk.g, T.paperInk.b, 0.45f);
        marginImage.raycastTarget = false;
        margin.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
    }

    private void BuildClueNoticeLayout()
    {
        if (clueNotification == null)
            return;

        var notice = (RectTransform)clueNotification.transform;
        LayoutKit.Overlay(notice, new Vector2(0.05f, 1f), new Vector2(0.95f, 1f), new Vector2(0f, -470f), new Vector2(0f, -210f));

        if (clueNotificationText != null)
        {
            RectTransform text = clueNotificationText.rectTransform;
            text.anchorMin = Vector2.zero;
            text.anchorMax = Vector2.one;
            text.offsetMin = new Vector2(96f, 12f); // Sitio para el icono
            text.offsetMax = new Vector2(-24f, -12f);
            text.localScale = Vector3.one;
            LayoutKit.MultiLine(clueNotificationText, T.bodySize);
        }
    }

    private void BuildIntroLayout()
    {
        if (introPanel == null)
            return;

        RectTransform column = LayoutKit.Column((RectTransform)introPanel.transform, out bool created);
        if (!created)
            return;

        if (caseTitleText != null)
            LayoutKit.Put(caseTitleText, column, height: 150f);
        LayoutKit.Scrollable(caseDescriptionText, column);
        LayoutKit.Put(startButton, column, height: 130f);
        LayoutKit.Label(startButton, "Empezar");
    }

    private void BuildAccusationLayout()
    {
        if (accusationPanel == null)
            return;

        var panel = (RectTransform)accusationPanel.transform;
        RectTransform column = LayoutKit.Column(panel, out bool created);
        if (!created)
            return;

        Transform title = panel.Find("AccusationTitleText");
        if (title != null)
            LayoutKit.Put(title, column, height: 140f);

        Transform instructions = panel.Find("Text (TMP)");
        if (instructions != null)
        {
            LayoutKit.Put(instructions, column, height: 200f);
            if (instructions.TryGetComponent(out TMP_Text instructionsText))
            {
                instructionsText.text = GameTexts.AccusationPrompt;
                LayoutKit.MultiLine(instructionsText, T.bodySize);
            }
        }

        // Rueda de reconocimiento: los sospechosos de este caso (la foto de grupo era de la historia 1)
        Transform group = panel.Find("SuspectsGroupImage");
        if (group != null)
            group.gameObject.SetActive(false);
        lineup = UIFactory.Container(column, "Rueda (auto)", Vector2.zero, Vector2.one);
        LayoutKit.Size(lineup, height: 0f, flexibleHeight: 1f);
        UIComponents.GetOrAdd<LayoutElement>(lineup.gameObject).minHeight = 0f;
        var grid = lineup.gameObject.AddComponent<GridLayoutGroup>();
        grid.spacing = new Vector2(T.spacing, T.spacing);
        grid.childAlignment = TextAnchor.MiddleCenter;
        UIComponents.GetOrAdd<GridFit>(lineup.gameObject);

        PutDropdown(accusationDropdown, column);
        LayoutKit.Put(accuseButton, column, height: 130f);
        LayoutKit.Label(accuseButton, "Acusar");

        EnsureAccusationBackButton();
    }

    private void BuildResultLayout()
    {
        if (resultPanel == null)
            return;

        RectTransform column = LayoutKit.Column((RectTransform)resultPanel.transform, out bool created);
        if (!created)
            return;

        if (resultTitleText != null)
        {
            LayoutKit.Put(resultTitleText, column, height: 180f);
            LayoutKit.MultiLine(resultTitleText, T.headingSize);
        }
        LayoutKit.Scrollable(resultDetailsText, column);

        RectTransform buttons = LayoutKit.Row(column, "Botones", 130f);
        LayoutKit.Put(restartButton, buttons, flexibleWidth: 1f);
        LayoutKit.Label(restartButton, GameTexts.PlayAgain);
        LayoutKit.Put(menuButton, buttons, flexibleWidth: 1f);
        LayoutKit.Label(menuButton, GameTexts.MainMenu);
    }

    // ============================================
    // INTRO
    // ============================================

    public void ShowCaseIntro(string storyId, string title, string description)
    {
        ApplyIntroArt(storyId);

        // El título del caso ya encabeza el parte: arriba, el número de expediente
        if (caseTitleText != null)
        {
            caseTitleText.text = $"EXPEDIENTE Nº {storyId.PadLeft(3, '0')}";
            caseTitleText.color = T.textSecondary;
            caseTitleText.characterSpacing = 12f;
        }

        if (caseDescriptionText != null)
        {
            caseDescriptionText.text = description;

            // El parte del caso se escribe como el de la mañana; un toque lo completa
            Typewriter briefing = UIComponents.GetOrAdd<Typewriter>(caseDescriptionText.gameObject);
            if (briefing.isActiveAndEnabled)
            {
                briefing.OnFinished -= StampConfidential;
                briefing.OnFinished += StampConfidential;
                briefing.Reveal(0, 2.2f); // Un parte largo: se escribe deprisa
            }
        }

        Debug.Log("[InterrogationUI] Caso cargado: " + title);
    }

    public string CurrentSuspectId => currentSuspectId;

    private GameObject confidentialStamp;

    // Sello "CONFIDENCIAL" sobre el expediente, una vez escrito el parte
    private void StampConfidential()
    {
        if (fx == null || introPanel == null || !introPanel.activeInHierarchy || confidentialStamp != null)
            return;
        confidentialStamp = fx.Stamp("CONFIDENCIAL", T.danger, angle: -12f, fontSize: 64f,
            parent: (RectTransform)introPanel.transform, anchor: new Vector2(0.66f, 0.2f), hold: -1f);
    }

    private UnityEngine.UI.RawImage introBackground;
    private UnityEngine.UI.RawImage introHeader;

    /// <summary>
    /// Fondo y cabecera de la intro de cada historia (Assets/Art/Stories/...), o color plano si faltan.
    /// </summary>
    private void ApplyIntroArt(string storyId)
    {
        if (introPanel == null)
            return;

        if (introBackground == null)
        {
            RectTransform rect = UIFactory.Container(introPanel.transform, "IntroFondo (auto)", Vector2.zero, Vector2.one);
            rect.SetAsFirstSibling();
            UIComponents.GetOrAdd<LayoutElement>(rect.gameObject).ignoreLayout = true;
            introBackground = rect.gameObject.AddComponent<UnityEngine.UI.RawImage>();
            introBackground.raycastTarget = false;
            rect.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            UIComponents.GetOrAdd<RectMask2D>(introPanel);
            ParallaxLayer.AddTo(introBackground);
        }

        if (introHeader == null)
        {
            RectTransform rect = UIFactory.Container(introPanel.transform, "IntroCabecera (auto)", new Vector2(0f, 1f), Vector2.one);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 480f);
            rect.SetSiblingIndex(1);
            UIComponents.GetOrAdd<LayoutElement>(rect.gameObject).ignoreLayout = true;
            introHeader = rect.gameObject.AddComponent<UnityEngine.UI.RawImage>();
            introHeader.raycastTarget = false;
        }

        // Fondo de la historia o, si no hay, la sala de interrogatorios; oscurecido para leer el parte encima
        Texture2D background = ArtLibrary.Load(ArtSlots.StoryIntro(storyId)) ?? ArtLibrary.Load(ArtSlots.DefaultIntroBackground);
        introBackground.texture = background != null ? background : ArtLibrary.Placeholder(T.background);
        introBackground.color = new Color(0.4f, 0.4f, 0.4f, 1f);
        if (background != null)
        {
            introBackground.GetComponent<AspectRatioFitter>().aspectRatio = (float)background.width / background.height;
            ArtGrading.Apply(introBackground, ArtGrading.Kind.Background);
        }

        // La cabecera solo si existe su arte (un rectángulo liso partía la pantalla)
        Texture2D header = ArtLibrary.Load(ArtSlots.StoryHeader(storyId));
        introHeader.texture = header;
        introHeader.gameObject.SetActive(header != null);
    }

    /// <summary>
    /// Icono pequeño a la izquierda de un botón o aviso (Assets/Art/Icons/...), o un cuadro de color si falta.
    /// </summary>
    private void AddIcon(Transform parent, string path)
    {
        if (parent == null || parent.Find("Icono (auto)") != null)
            return;

        RectTransform rect = UIFactory.Container(parent, "Icono (auto)", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
        rect.pivot = new Vector2(0f, 0.5f);
        rect.sizeDelta = new Vector2(56f, 56f);
        rect.anchoredPosition = new Vector2(T.spacing, 0f);
        var icon = rect.gameObject.AddComponent<UnityEngine.UI.RawImage>();
        icon.raycastTarget = false;
        icon.texture = ArtSlots.LoadOrPlaceholder(path, new Color(T.accent.r, T.accent.g, T.accent.b, 0.35f));

        if (parent.GetComponent<Button>() != null)
        {
            TMP_Text label = parent.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                label.rectTransform.offsetMin = new Vector2(56f + 2f * T.spacing, label.rectTransform.offsetMin.y);
        }
    }

    /// <summary>
    /// Vuelve a la partida guardada: conversaciones, sospechoso abierto y estados emocionales.
    /// </summary>
    public void ContinueInterrogation(SaveData data, string suspectId, IReadOnlyDictionary<string, Emotion> emotions)
    {
        HideAllPanels();
        if (interrogationPanel != null)
        {
            interrogationPanel.SetActive(true);
            UIAnimations.FadeIn(this, interrogationPanel);
        }

        SaveSystem.RestoreConversations(data, conversations);
        emotionBySuspect.Clear();
        foreach (var pair in emotions)
            emotionBySuspect[pair.Key] = pair.Value;

        currentSuspectId = string.IsNullOrEmpty(suspectId) ? null : suspectId;
        if (currentSuspectId != null)
            conversations.Select(currentSuspectId);

        gameManager.BeginInterrogation();
        RefreshConversationView();
        if (currentSuspectId != null)
            UpdateSuspectImage(currentSuspectId, instant: true);
        SetInputEnabled(true);
    }

    public void StartInterrogation()
    {
        Debug.Log("[InterrogationUI] StartInterrogation llamado");

        HideAllPanels();

        if (interrogationPanel != null)
        {
            interrogationPanel.SetActive(true);
            UIAnimations.FadeIn(this, interrogationPanel);
        }

        conversations.Clear();
        currentSuspectId = null;
        RefreshConversationView();
        gameManager.BeginInterrogation();
        ShowTutorial(Tutorial.Ask, questionInput != null ? (RectTransform)questionInput.transform : null, 0.8f);
    }

    // ============================================
    // GESTIÓN DE PANELES
    // ============================================

    private void HideAllPanels()
    {
        if (introPanel != null) introPanel.SetActive(false);
        if (interrogationPanel != null) interrogationPanel.SetActive(false);
        if (accusationPanel != null) accusationPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);

        Debug.Log("[InterrogationUI] Todos los paneles ocultos");
    }

    private void ShowPanel(GameObject panel)
    {
        HideAllPanels();

        if (panel != null)
        {
            panel.SetActive(true);
            UIAnimations.FadeIn(this, panel);
            Debug.Log($"[InterrogationUI] Panel mostrado: {panel.name}");
        }
    }

    // ============================================
    // INTERROGATORIO
    // ============================================

    public void OnAskButtonClick()
    {
        string question = questionInput.text.Trim();
        string shownClueId = SelectedEvidenceId();

        if (string.IsNullOrEmpty(question) && shownClueId == null)
        {
            ShowError("Escribe una pregunta o elige una prueba para mostrar.");
            return;
        }

        if (!gameManager.CanAskMoreQuestions())
        {
            ShowError("No te quedan preguntas hoy.");
            return;
        }

        questionInput.text = "";
        SetInputEnabled(false);

        // La pregunta aparece al momento; si la petición falla, se retira y vuelve al campo
        string evidenceName = shownClueId != null ? evidenceOptions.Find(c => c.id == shownClueId)?.playerName : null;
        pendingQuestion = ChatEntry.Player(question, evidenceName, GameClock.TimeOf(questionsUsedToday, questionsPerDay));
        pendingSuspectId = currentSuspectId;
        SoundManager.Play(Sfx.Send);
        conversations.Append(currentSuspectId, pendingQuestion);
        RefreshConversationView();

        gameManager.AskQuestion(currentSuspectId, question, shownClueId);
    }

    public void OnEndDayClick()
    {
        gameManager.EndDay();
    }

    // NUEVO: Método para acusar antes del día 7
    public void OnAccuseNowClick()
    {
        Debug.Log("[InterrogationUI] Acusación anticipada solicitada");
        gameManager.ForceAccusationPanel();
    }

    private void OnSuspectChanged(int index)
    {
        if (index < 0 || index >= suspects.Count)
            return;

        SelectSuspect(suspects[index].id);
    }

    private void SelectSuspect(string suspectId)
    {
        if (suspectId == currentSuspectId)
            return;

        chat?.Complete();
        currentSuspectId = suspectId;
        conversations.Select(suspectId);

        UpdateSuspectImage(suspectId, instant: true);
        RefreshConversationView();
    }

    /// <summary>
    /// Estado emocional de un sospechoso (lo lee el gestor de conversación de la etiqueta oculta).
    /// </summary>
    public void SetEmotion(string suspectId, Emotion emotion)
    {
        emotionBySuspect[suspectId] = emotion;

        if (suspectId == currentSuspectId)
            UpdateSuspectImage(suspectId, instant: false);
    }

    private Emotion EmotionOf(string suspectId)
    {
        return suspectId != null && emotionBySuspect.TryGetValue(suspectId, out Emotion e) ? e : Emotion.Tranquilo;
    }

    /// <summary>
    /// Retrato por estado (Assets/Art/Portraits/&lt;artId&gt;_&lt;estado&gt;.png), si no el retrato antiguo del
    /// personaje, y si no un color plano. El tinte y el temblor del estado se aplican siempre.
    /// </summary>
    private void UpdateSuspectImage(string suspectId, bool instant)
    {
        if (suspectImage == null)
            return;

        SuspectView view = suspects.Find(v => v.id == suspectId);
        Emotion emotion = EmotionOf(suspectId);
        (Texture2D texture, bool legacyArt) = PortraitOf(view, emotion);

        suspectImage.texture = texture;
        // El pixel art antiguo es de cuerpo entero: en el interrogatorio, plano medio; en el chat, la cara
        string cropKey = legacyArt ? view.portraitKey : null;
        suspectImage.uvRect = PortraitCrops.Bust(cropKey);
        bool placeholder = texture == null || texture.name.Contains("Placeholder");
        chat?.SetAvatar(placeholder ? null : texture, PortraitCrops.Face(cropKey, texture));

        // El arte antiguo se gradúa para casar con el tema; el nuevo ya viene con la paleta del juego
        if (legacyArt)
            ArtGrading.Apply(suspectImage, ArtGrading.Kind.LegacyPortrait);
        else
            ArtGrading.Clear(suspectImage);
        suspectImage.gameObject.SetActive(true);
        emotionPresenter?.Apply(emotion, instant);

        if (emotionLabel != null)
        {
            emotionLabel.text = emotion.ToString().ToUpperInvariant();
            emotionLabel.color = EmotionStyle.LabelColor(emotion);
        }
    }

    /// <summary>
    /// Respuesta del sospechoso: burbuja a la izquierda que se escribe letra a letra (un toque la completa).
    /// </summary>
    private (Texture2D texture, bool legacy) PortraitOf(SuspectView view, Emotion emotion)
    {
        Texture2D texture = null;
        if (!string.IsNullOrEmpty(view.artId))
            texture = ArtLibrary.LoadFirst(PortraitPaths.Candidates(view.artId, emotion));
        if (texture != null)
            return (texture, false);
        if (view.portraitKey != null && suspectImages.TryGetValue(view.portraitKey, out Texture2D legacy) && legacy != null)
            return (legacy, true);
        return (ArtLibrary.Placeholder(T.placeholder), false);
    }

    public void AddAnswer(string suspectId, string speaker, string answer)
    {
        string time = pendingQuestion != null ? pendingQuestion.time : GameClock.TimeOf(questionsUsedToday, questionsPerDay);
        pendingQuestion = null;
        pendingSuspectId = null;

        conversations.Append(suspectId, ChatEntry.Suspect(speaker, answer, time));
        SoundManager.Play(Sfx.Answer);
        if (suspectId == currentSuspectId && chat != null)
            chat.Show(conversations.CurrentEntries, typeLast: true, speed: EmotionStyle.For(EmotionOf(suspectId)).textSpeed);

        if (evidenceDropdown != null)
            evidenceDropdown.value = 0;

        SetInputEnabled(true);
        ShowTutorial(Tutorial.Days, endDayButton != null ? (RectTransform)endDayButton.transform : null, 2.5f);
    }

    // ============================================
    // TUTORIAL
    // ============================================

    private void ShowTutorial(string id, RectTransform target, float delay)
    {
        if (fx == null || !Tutorial.ShouldShow(id))
            return;
        RunRoutine(TutorialAfter(id, target, delay));
    }

    private IEnumerator TutorialAfter(string id, RectTransform target, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        // Una cosa cada vez: espera a que acaben las fichas de pista y otras indicaciones
        while (fx.HintVisible || fx.ClueCardsPending)
            yield return null;
        if (!Tutorial.ShouldShow(id) || interrogationPanel == null || !interrogationPanel.activeInHierarchy)
            yield break;

        Tutorial.MarkSeen(id);
        fx.Hint(Tutorial.TextOf(id), target, null, Tutorial.SkipAll);
    }

    /// <summary>
    /// Aviso de sistema en la conversación abierta (o solo en pantalla si aún no hay sospechoso).
    /// </summary>
    private void AppendNotice(ChatEntry notice)
    {
        conversations.AppendToCurrent(notice);
        RefreshConversationView();
    }

    /// <summary>
    /// Conversaciones del chat (para guardar la partida).
    /// </summary>
    public ConversationStore Conversations => conversations;

    private UnityEngine.UI.Image contradictionOverlay;

    // Capa a pantalla completa para el destello de contradicción (se crea la primera vez)
    private UnityEngine.UI.Image ContradictionOverlay()
    {
        if (contradictionOverlay == null && interrogationPanel != null)
        {
            RectTransform rect = UIFactory.Container(interrogationPanel.transform, "ContradictionFlash (auto)", Vector2.zero, Vector2.one);
            contradictionOverlay = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            contradictionOverlay.raycastTarget = false;
            rect.gameObject.SetActive(false);
        }

        if (contradictionOverlay != null)
            contradictionOverlay.transform.SetAsLastSibling();
        return contradictionOverlay;
    }

    /// <summary>
    /// Primera página del chat: el día 1 con lo que se sabe del caso (el chat no empieza vacío).
    /// </summary>
    public void BeginCase(string situation)
    {
        if (!conversations.IsEmpty)
            return;
        conversations.AppendToAll(ChatEntry.Day(1, situation));
        RefreshConversationView();
    }

    /// <summary>
    /// El tema cambió (alto contraste): el chat se rehace con los colores nuevos.
    /// </summary>
    public void RestyleForTheme()
    {
        chat?.Restyle();
        RefreshConversationView();
        if (!string.IsNullOrEmpty(currentSuspectId))
            UpdateSuspectImage(currentSuspectId, instant: true);
    }

    /// <summary>
    /// Vuelve a pintar la conversación abierta (también la usa la vista previa del editor).
    /// </summary>
    public void RefreshConversationView()
    {
        chat?.Show(conversations.CurrentEntries);
    }

    // Las corrutinas solo existen en juego (el test de layout usa la UI en modo edición)
    private void RunRoutine(IEnumerator routine)
    {
        if (Application.isPlaying && isActiveAndEnabled)
            StartCoroutine(routine);
    }

    public void UpdateGameState(int day, int maxDays, int questionsUsed, int questionsMax)
    {
        questionsUsedToday = questionsUsed;
        questionsPerDay = questionsMax;
        maxDaysValue = maxDays;
        dayValue = day;

        if (hudText != null)
        {
            hudText.text = GameTexts.Hud(day, maxDays, questionsUsed, questionsMax);
        }
    }

    public void ShowWaiting(bool show)
    {
        SuspectView view = suspects.Find(v => v.id == currentSuspectId);
        chat?.ShowTyping(show, view.shortName);
    }

    private void SetInputEnabled(bool enabled)
    {
        if (questionInput != null)
            questionInput.interactable = enabled;

        if (askButton != null)
            askButton.interactable = enabled;

        if (endDayButton != null)
            endDayButton.interactable = enabled;

        if (suspectDropdown != null)
            suspectDropdown.interactable = enabled;

        if (accuseNowButton != null)
            accuseNowButton.interactable = enabled;

        if (evidenceDropdown != null)
            evidenceDropdown.interactable = enabled;
    }

    public void ShowError(string message)
    {
        AppendNotice(ChatEntry.System(ChatEntryKind.Error, message));
    }

    /// <summary>
    /// La petición al LLM falló: avisa, devuelve la pregunta al campo de texto y reactiva el input.
    /// </summary>
    public void ShowRequestFailed(string message, string question)
    {
        // La pregunta no llegó: su burbuja se retira y el texto vuelve al campo
        if (pendingQuestion != null)
        {
            conversations.Remove(pendingSuspectId, pendingQuestion);
            pendingQuestion = null;
            pendingSuspectId = null;
            RefreshConversationView();
        }

        ShowError($"{message}\nLa pregunta no se ha descontado.");

        if (questionInput != null && string.IsNullOrEmpty(questionInput.text))
            questionInput.text = question;

        SetInputEnabled(true);
    }

    // ============================================
    // PISTAS Y CONTRADICCIONES (ARREGLADO)
    // ============================================

    /// <summary>
    /// Libreta del detective (pistas, contradicciones y sospechosos), en el panel de pistas.
    /// </summary>
    public void UpdateNotebook(string notebook)
    {
        if (cluesText != null)
            cluesText.text = notebook;

        // Las contradicciones ya van en la libreta
        if (contradictionsText != null)
            contradictionsText.gameObject.SetActive(false);
    }

    public void ShowClueNotification(string clueName)
    {
        SetClueBadge(unseenClues + 1);
        SoundManager.Play(Sfx.Clue);
        Haptics.Pulse();
        ShowTutorial(Tutorial.Evidence, evidenceDropdown != null ? (RectTransform)evidenceDropdown.transform : null, 1f);
        if (fx != null)
        {
            fx.ClueCard(clueName, viewCluesButton != null ? (RectTransform)viewCluesButton.transform : null);
            return;
        }

        if (clueNotification != null && clueNotificationText != null)
        {
            // Varias pistas seguidas se acumulan en el mismo aviso y reinician el temporizador
            visibleClueNames.Add(clueName);
            clueNotificationText.text = (visibleClueNames.Count == 1 ? "PISTA DESCUBIERTA:\n" : "PISTAS DESCUBIERTAS:\n") +
                                        string.Join("\n", visibleClueNames);
            clueNotification.SetActive(true);
            UIAnimations.CardFlip(this, clueNotification.transform);
            CancelInvoke(nameof(HideClueNotification));
            Invoke(nameof(HideClueNotification), 3f + visibleClueNames.Count - 1);
        }
    }

    private void HideClueNotification()
    {
        visibleClueNames.Clear();

        if (clueNotification != null)
            clueNotification.SetActive(false);
    }

    public void ShowContradictionNotification(string text)
    {
        AppendNotice(ChatEntry.System(ChatEntryKind.Contradiction, text));

        if (fx != null)
        {
            // Sello de tinta que golpea: en el impacto, destello y sacudida de la pantalla
            RectTransform shaken = interrogationPanel != null ? (RectTransform)interrogationPanel.transform : null;
            SoundManager.Play(Sfx.Contradiction);
            ShowTutorial(Tutorial.Contradiction, viewCluesButton != null ? (RectTransform)viewCluesButton.transform : null, 2f);
            fx.Stamp("CONTRADICCIÓN", T.danger, angle: -9f, hold: 1.3f, onImpact: () =>
            {
                fx.Flash(T.contradiction, 0.12f);
                Haptics.Pulse();
                if (shaken != null)
                    fx.Shake(shaken, 14f, 0.35f);
            });
            return;
        }

        // Sin capa de efectos: destello del color de contradicción y sacudida del HUD
        UIAnimations.Flash(this, ContradictionOverlay(), T.contradiction, 0.25f);
        if (hudText != null)
            UIAnimations.Shake(this, hudText.rectTransform, 12f, T.contradictionAnimDuration);
    }

    /// <summary>
    /// Contador de pistas sin ver sobre el botón de la libreta (se crea la primera vez).
    /// </summary>
    private void SetClueBadge(int count)
    {
        unseenClues = count;
        if (viewCluesButton == null)
            return;

        if (clueBadge == null)
        {
            RectTransform badge = UIFactory.Container(viewCluesButton.transform, "Contador (auto)", Vector2.one, Vector2.one);
            badge.pivot = new Vector2(0.5f, 0.5f);
            badge.sizeDelta = new Vector2(52f, 52f);
            badge.anchoredPosition = new Vector2(-10f, -10f);
            var dot = badge.gameObject.AddComponent<UnityEngine.UI.Image>();
            dot.sprite = UISprites.Circle();
            dot.color = T.danger;
            dot.raycastTarget = false;
            badge.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;

            clueBadge = UIFactory.Label(badge, "", T.secondarySize, Color.white);
            clueBadge.alignment = TextAlignmentOptions.Center;
            clueBadge.fontStyle = FontStyles.Bold;
            clueBadge.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
            var rect = clueBadge.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            LayoutKit.OneLine(clueBadge, T.secondarySize);
        }

        clueBadge.text = count > 9 ? "9+" : count.ToString();
        clueBadge.transform.parent.gameObject.SetActive(count > 0);
    }

    // ARREGLADO: Panel de pistas
    private void ShowCluesPanel()
    {
        SetClueBadge(0);
        if (cluesPanel != null)
        {
            cluesPanel.SetActive(true);
            cluesPanel.transform.SetAsLastSibling(); // Por encima del retrato y los desplegables
            UIAnimations.CardFlip(this, cluesPanel.transform);
            Debug.Log("[InterrogationUI] Panel de pistas mostrado");
        }
    }

    private void HideCluesPanel()
    {
        if (cluesPanel != null)
        {
            cluesPanel.SetActive(false);
            Debug.Log("[InterrogationUI] Panel de pistas cerrado");
        }
    }

    // ============================================
    // SOSPECHOSOS
    // ============================================

    public void SetSuspects(List<SuspectView> unlocked)
    {
        suspects = unlocked;

        if (suspectDropdown == null || suspects.Count == 0)
            return;

        // Conserva el sospechoso actual si sigue en la lista
        int index = suspects.FindIndex(v => v.id == currentSuspectId);
        if (index < 0)
            index = 0;

        suspectDropdown.ClearOptions();
        suspectDropdown.AddOptions(suspects.ConvertAll(v => v.displayName));
        suspectDropdown.SetValueWithoutNotify(index);
        SelectSuspect(suspects[index].id);
    }

    public void SetEvidenceOptions(List<ClueData> discovered)
    {
        evidenceOptions = discovered;

        if (evidenceDropdown == null)
            return;

        var options = new List<string> { NoEvidenceOption };
        options.AddRange(discovered.ConvertAll(c => c.playerName));

        int previous = evidenceDropdown.value;
        evidenceDropdown.ClearOptions();
        evidenceDropdown.AddOptions(options);
        evidenceDropdown.SetValueWithoutNotify(previous < options.Count ? previous : 0);
    }

    private string SelectedEvidenceId()
    {
        if (evidenceDropdown == null || evidenceDropdown.value <= 0 || evidenceDropdown.value > evidenceOptions.Count)
            return null;

        return evidenceOptions[evidenceDropdown.value - 1].id;
    }

    /// <summary>
    /// Si la escena no tiene selector de pruebas, lo crea clonando el de sospechosos justo debajo.
    /// </summary>
    private void EnsureEvidenceDropdown()
    {
        if (evidenceDropdown != null || suspectDropdown == null)
            return;

        GameObject clone = Instantiate(suspectDropdown.gameObject, suspectDropdown.transform.parent);
        clone.name = "EvidenceDropdown (auto)";

        var rect = (RectTransform)clone.transform;
        var source = (RectTransform)suspectDropdown.transform;
        rect.anchoredPosition = source.anchoredPosition - new Vector2(0f, source.rect.height + 8f);

        evidenceDropdown = clone.GetComponent<TMP_Dropdown>();
        evidenceDropdown.onValueChanged.RemoveAllListeners();
        evidenceDropdown.ClearOptions();
        evidenceDropdown.AddOptions(new List<string> { NoEvidenceOption });

        Debug.LogWarning("[InterrogationUI] Selector de pruebas creado automáticamente bajo el de sospechosos. " +
                         "Para colocarlo a mano, asigna 'evidenceDropdown' en el Inspector.");
    }

    public void ShowSuspectUnlocked(string displayName)
    {
        AppendNotice(ChatEntry.System(ChatEntryKind.Unlock, displayName));
    }

    public void ShowNotice(string message)
    {
        AppendNotice(ChatEntry.System(ChatEntryKind.Notice, message));
    }

    public void ShowDayTransition(int newDay, string morningReport)
    {
        // El cambio de día se anota en todas las conversaciones, no solo en la abierta
        chat?.Complete();
        conversations.AppendToAll(ChatEntry.Day(newDay, morningReport));
        RefreshConversationView();

        SetInputEnabled(true);
        fx?.DayCard(newDay, maxDaysValue, morningReport);
        SoundManager.Play(Sfx.DayChange);
    }

    // ============================================
    // ACUSACIÓN
    // ============================================

    public void ShowAccusationPanel(List<SuspectView> options, bool canGoBack)
    {
        ShowPanel(accusationPanel);
        accusationOptions = options;
        // El título está dentro de la columna de la distribución: se busca en todo el panel
        if (accusationPanel != null)
        {
            foreach (TMP_Text t in accusationPanel.GetComponentsInChildren<TMP_Text>(true))
            {
                if (t.name == "AccusationTitleText")
                    t.text = GameTexts.AccusationTitle(dayValue >= maxDaysValue || !canGoBack);
            }
        }
        fx?.SetTension(true, accusationPanel != null ? (RectTransform)accusationPanel.transform : null);
        SoundManager.Play(Sfx.Heartbeat, 0.8f);
        SoundManager.PlayMusic(Music.Tension);
        EnsureAccusationBackButton();
        if (accusationBackButton != null)
            accusationBackButton.gameObject.SetActive(canGoBack);

        if (accusationDropdown != null)
        {
            accusationDropdown.ClearOptions();
            accusationDropdown.AddOptions(options.ConvertAll(v => v.displayName));
            accusationDropdown.onValueChanged.RemoveListener(MarkLineup);
            accusationDropdown.onValueChanged.AddListener(MarkLineup);
        }
        else
        {
            Debug.LogError("[InterrogationUI] ¡accusationDropdown es NULL!");
        }
        FillLineup(options);
    }

    /// <summary>
    /// Vuelve al interrogatorio desde el panel de acusación.
    /// </summary>
    public void ShowInterrogation()
    {
        fx?.SetTension(false);
        ShowPanel(interrogationPanel);
        SetInputEnabled(true);
        RefreshConversationView();
    }

    /// <summary>
    /// Si la escena no tiene botón "Volver" en la acusación, lo crea clonando el de acusar, debajo de él.
    /// </summary>
    private void EnsureAccusationBackButton()
    {
        if (accusationBackButton != null || accuseButton == null)
            return;

        GameObject clone = Instantiate(accuseButton.gameObject, accuseButton.transform.parent);
        clone.name = "AccusationBackButton (auto)";
        var rect = (RectTransform)clone.transform;
        var source = (RectTransform)accuseButton.transform;
        rect.anchoredPosition = source.anchoredPosition - new Vector2(0f, source.rect.height + T.spacing);

        accusationBackButton = clone.GetComponent<Button>();
        UIComponents.SetOnlyListener(accusationBackButton, () => gameManager.CancelAccusation());

        TMP_Text label = clone.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
            label.text = "Volver";

        ThemeApplier.Apply(clone.transform);

        // En la columna de la acusación, debajo de "Acusar"
        if (clone.transform.parent.GetComponent<VerticalLayoutGroup>() != null)
        {
            LayoutKit.Put(clone.transform, (RectTransform)clone.transform.parent, height: 130f);
            LayoutKit.Label(accusationBackButton, "Volver");

            // Volver es la acción secundaria: no compite en color con "Acusar"
            UIComponents.GetOrAdd<ThemeRole>(clone).role = UIRole.SecondaryButton;
            ThemeApplier.Apply(clone.transform);
        }
    }

    public void OnAccuseClick()
    {
        if (accusationDropdown != null && accusationDropdown.value < accusationOptions.Count)
        {
            string accusedId = accusationOptions[accusationDropdown.value].id;
            if (accuseButton != null)
                accuseButton.interactable = false; // Un solo veredicto
            SoundManager.Play(Sfx.Accusation);
            SoundManager.PlayMusic(Music.None);
            if (fx != null)
                fx.Deliberation(() => gameManager.MakeAccusation(accusedId));
            else
                gameManager.MakeAccusation(accusedId);
        }
    }

    // ============================================
    // RESULTADO (MEJORADO)
    // ============================================

    private GameObject endingStamp;
    private RectTransform lineup;
    private readonly List<GameObject> lineupSelection = new List<GameObject>();

    /// <summary>
    /// Rellena la rueda de reconocimiento con los bustos de los sospechosos; tocar uno lo elige.
    /// </summary>
    private void FillLineup(List<SuspectView> options)
    {
        if (lineup == null)
            return;

        for (int i = lineup.childCount - 1; i >= 0; i--)
            DestroyImmediateOrLater(lineup.GetChild(i).gameObject);
        lineupSelection.Clear();

        // Columnas y tamaño de celda según cuántos hay y el hueco disponible (y se reajusta si cambia)
        const float labelHeight = 64f;
        GridFit fit = UIComponents.GetOrAdd<GridFit>(lineup.gameObject);
        fit.count = options.Count;
        fit.labelHeight = labelHeight;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)lineup.parent);
        fit.Fit();

        for (int i = 0; i < options.Count; i++)
        {
            int index = i;
            SuspectView view = options[i];
            RectTransform cell = UIFactory.Container(lineup, "Sospechoso " + view.shortName, Vector2.zero, Vector2.one);
            var card = cell.gameObject.AddComponent<Image>();
            card.sprite = UISprites.Rounded(16);
            card.type = Image.Type.Sliced;
            card.color = T.panelBorder;
            var button = cell.gameObject.AddComponent<Button>();
            button.targetGraphic = card;
            button.onClick.AddListener(() =>
            {
                if (accusationDropdown != null)
                    accusationDropdown.value = index;
                MarkLineup(index);
            });
            cell.gameObject.AddComponent<ClickSound>();
            cell.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;

            (Texture2D texture, bool legacy) = PortraitOf(view, EmotionOf(view.id));
            RectTransform frame = UIFactory.Container(cell, "Marco", new Vector2(0f, 0f), new Vector2(1f, 1f));
            frame.offsetMin = new Vector2(8f, labelHeight);
            frame.offsetMax = new Vector2(-8f, -8f);
            RectTransform face = UIFactory.Container(frame, "Busto", Vector2.zero, Vector2.one);
            var raw = face.gameObject.AddComponent<RawImage>();
            raw.texture = texture;
            raw.uvRect = PortraitCrops.Bust(legacy ? view.portraitKey : null);
            raw.raycastTarget = false;
            if (legacy)
                ArtGrading.Apply(raw, ArtGrading.Kind.LegacyPortrait);
            face.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            face.GetComponent<AspectRatioFitter>().aspectRatio = 0.75f;

            TMP_Text name = UIFactory.Label(cell, view.shortName, T.bodySize, T.textPrimary);
            name.alignment = TextAlignmentOptions.Center;
            name.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
            var nameRect = name.rectTransform;
            nameRect.anchorMin = Vector2.zero;
            nameRect.anchorMax = new Vector2(1f, 0f);
            nameRect.pivot = new Vector2(0.5f, 0f);
            nameRect.offsetMin = new Vector2(8f, 0f);
            nameRect.offsetMax = new Vector2(-8f, labelHeight);
            LayoutKit.OneLine(name, T.bodySize);

            RectTransform ring = UIFactory.Container(cell, "Seleccion", Vector2.zero, Vector2.one);
            var ringImage = ring.gameObject.AddComponent<Image>();
            ringImage.sprite = UISprites.RoundedOutline(16, 6);
            ringImage.type = Image.Type.Sliced;
            ringImage.color = T.accent;
            ringImage.raycastTarget = false;
            ring.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
            lineupSelection.Add(ring.gameObject);
        }

        MarkLineup(accusationDropdown != null ? accusationDropdown.value : 0);
    }

    private void MarkLineup(int selected)
    {
        for (int i = 0; i < lineupSelection.Count; i++)
            lineupSelection[i].SetActive(i == selected);
    }

    private static void DestroyImmediateOrLater(GameObject target)
    {
        if (Application.isPlaying)
        {
            target.SetActive(false);
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }

    public void ShowAccusationResult(AccusationResult result, string accusedName, string culpritName,
                                     int maxEvidence, string epilogue)
    {
        fx?.SetTension(false);
        ShowPanel(resultPanel);
        EndingStyle style = EndingStyle.For(result.ending, T);
        SoundManager.PlayMusic(Music.None);
        SoundManager.Play(SoundCatalog.ForEnding(result.ending));

        if (resultTitleText != null)
        {
            resultTitleText.text = style.title.ToUpperInvariant();
            resultTitleText.color = style.ink;
        }

        // Cada final tiñe la escena con su color
        if (resultPanel != null)
        {
            Transform existing = resultPanel.transform.Find("Tinte final (auto)");
            RectTransform grade = existing != null ? (RectTransform)existing
                : UIFactory.Container(resultPanel.transform, "Tinte final (auto)", Vector2.zero, Vector2.one);
            if (existing == null)
            {
                grade.SetSiblingIndex(0);
                UIComponents.GetOrAdd<LayoutElement>(grade.gameObject).ignoreLayout = true;
                grade.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
                grade.gameObject.AddComponent<UnityEngine.UI.Image>().raycastTarget = false;
            }
            grade.GetComponent<UnityEngine.UI.Image>().color = style.grade;
        }

        if (resultDetailsText == null)
            return;

        resultDetailsText.text = EndingReport.Build(result, accusedName, culpritName, maxEvidence, epilogue, T);

        if (fx != null && resultPanel != null && resultTitleText != null)
        {
            // El sello del final hace de título; el informe se descubre línea a línea
            if (endingStamp != null)
                Destroy(endingStamp);
            resultTitleText.text = "";
            RectTransform shaken = (RectTransform)resultPanel.transform;
            endingStamp = fx.Stamp(style.stamp, style.ink, angle: -5f, fontSize: 80f, parent: resultTitleText.rectTransform,
                anchor: new Vector2(0.5f, 0.5f), hold: -1f,
                onImpact: () => fx.Shake(shaken, result.ending == Ending.Bad ? 16f : 8f, 0.3f));
            UIComponents.GetOrAdd<StepReveal>(resultDetailsText.gameObject).Play(0.9f);
        }
    }
}
