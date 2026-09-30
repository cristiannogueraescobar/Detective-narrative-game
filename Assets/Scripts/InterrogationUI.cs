using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI DE LA PARTIDA: intro del caso, interrogatorio (chat, retrato, pruebas, libreta, avisos), acusación y final.
/// Repartida en tres archivos (clase parcial):
///   InterrogationUI.cs            flujo de la partida (preguntas, sospechosos, avisos, tutorial, HUD);
///   InterrogationUI.Layout.cs     construcción y distribución de todos los paneles (LayoutKit);
///   InterrogationUI.Accusation.cs acusación, rueda de reconocimiento y presentación de los finales.
/// </summary>
public partial class InterrogationUI : MonoBehaviour
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
            emotionLabel.transform.parent.gameObject.SetActive(true);
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

    /// <summary>
    /// Botón Atrás (Android) en la partida: cierra la libreta o vuelve de la acusación. En el interrogatorio no
    /// hace nada (salir por error perdería el hilo; el guardado es automático).
    /// </summary>
    public bool HandleBack()
    {
        if (cluesPanel != null && cluesPanel.activeInHierarchy)
        {
            HideCluesPanel();
            return true;
        }
        if (accusationPanel != null && accusationPanel.activeInHierarchy && gameManager != null && gameManager.CanCancelAccusation
            && (accuseButton == null || accuseButton.interactable))
        {
            gameManager.CancelAccusation();
            return true;
        }
        return false;
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
    /// Partida nueva sin recargar la escena (selección de caso tras un "Continuar" fallido): sin restos del caso
    /// anterior en el chat, los estados, el sospechoso abierto ni el contador de pistas.
    /// </summary>
    public void ResetForNewCase()
    {
        chat?.Complete();
        conversations.Clear();
        emotionBySuspect.Clear();
        currentSuspectId = null;
        pendingQuestion = null;
        pendingSuspectId = null;
        SetClueBadge(0);
        RefreshConversationView();
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
            string previous = hudText.text;
            hudText.richText = true;
            hudText.text = GameTexts.HudRich(day, maxDays, questionsUsed, questionsMax, T);
            if (previous != hudText.text && Application.isPlaying)
                UIAnimations.Pop(this, hudText.transform);
        }

        // Sin preguntas, lo siguiente es terminar el día: el botón pasa a ser el principal y late
        if (endDayButton != null)
        {
            bool dayDone = questionsUsed >= questionsMax;
            UIRole wanted = dayDone ? UIRole.PrimaryButton : UIRole.SecondaryButton;
            ThemeRole role = UIComponents.GetOrAdd<ThemeRole>(endDayButton.gameObject);
            if (role.role != wanted)
            {
                role.role = wanted;
                ThemeApplier.Apply(endDayButton.transform);
            }
            if (dayDone && Application.isPlaying)
                UIAnimations.Pop(this, endDayButton.transform);
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

    private void OnNotebookLink(string link)
    {
        if (!link.StartsWith(Notebook.ClueLinkPrefix) || evidenceDropdown == null)
            return;
        string clueId = link.Substring(Notebook.ClueLinkPrefix.Length);
        int index = evidenceOptions.FindIndex(c => c.id == clueId);
        if (index < 0)
            return;
        evidenceDropdown.value = index + 1; // La opción 0 es "ninguna"
        HideCluesPanel();
        if (questionInput != null && questionInput.interactable)
            questionInput.ActivateInputField();
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
}
