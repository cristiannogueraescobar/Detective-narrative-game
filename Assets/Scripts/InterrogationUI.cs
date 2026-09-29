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

    private const string NoEvidenceOption = "— Sin prueba —";

    private string currentSuspectId;
    private List<SuspectView> suspects = new List<SuspectView>();
    private List<SuspectView> accusationOptions = new List<SuspectView>();
    private List<ClueData> evidenceOptions = new List<ClueData>();
    private readonly ConversationStore conversations = new ConversationStore(); // Una conversación por sospechoso
    private Dictionary<string, Texture2D> suspectImages = new Dictionary<string, Texture2D>();
    private readonly List<string> visibleClueNames = new List<string>(); // Pistas del aviso en pantalla
    private readonly Dictionary<string, Emotion> emotionBySuspect = new Dictionary<string, Emotion>();
    private EmotionPresenter emotionPresenter;
    private Typewriter typewriter;
    private static Theme T => ThemeManager.Current; // Todos los colores y tamaños salen del tema

    public void Initialize(GameManager gm)
    {
        gameManager = gm;

        // Botones principales
        if (startButton != null)
            startButton.onClick.AddListener(StartInterrogation);

        if (askButton != null)
            askButton.onClick.AddListener(OnAskButtonClick);

        if (endDayButton != null)
            endDayButton.onClick.AddListener(OnEndDayClick);

        // NUEVO: Botón acusar anticipado
        if (accuseNowButton != null)
            accuseNowButton.onClick.AddListener(OnAccuseNowClick);

        if (accuseButton != null)
            accuseButton.onClick.AddListener(OnAccuseClick);

        if (restartButton != null)
            restartButton.onClick.AddListener(() => gameManager.RestartGame());

        if (menuButton != null)
            menuButton.onClick.AddListener(() => gameManager.BackToMenu());

        if (suspectDropdown != null)
            suspectDropdown.onValueChanged.AddListener(OnSuspectChanged);

        // Botones panel pistas (ARREGLADO)
        if (viewCluesButton != null)
        {
            viewCluesButton.onClick.RemoveAllListeners(); // Limpia listeners viejos
            viewCluesButton.onClick.AddListener(ShowCluesPanel);
        }

        if (closeCluesButton != null)
        {
            closeCluesButton.onClick.RemoveAllListeners(); // Limpia listeners viejos
            closeCluesButton.onClick.AddListener(HideCluesPanel);
        }

        // Imágenes de sospechosos
        suspectImages["Padre"] = padreGif;
        suspectImages["Madre"] = madreGif;
        suspectImages["Hermano"] = hermanoGif;
        suspectImages["Vecina"] = vecinaGif;
        suspectImages["Detective"] = detectiveGif;
        suspectImages["Cartero"] = carteroGif;
        suspectImages["Dueño del Bar"] = duenioBarGif;

        EnsureEvidenceDropdown();

        if (applyMobileLayout)
            ApplyMobileLayout();

        // Reacciones por código: no necesitan nada en la escena
        if (suspectImage != null)
            emotionPresenter = suspectImage.GetComponent<EmotionPresenter>() ?? suspectImage.gameObject.AddComponent<EmotionPresenter>();
        if (conversationText != null)
            typewriter = conversationText.GetComponent<Typewriter>() ?? conversationText.gameObject.AddComponent<Typewriter>();

        if (clueNotification != null)
            clueNotification.SetActive(false);

        if (cluesPanel != null)
            cluesPanel.SetActive(false);

        // Ocultar todos los paneles al inicio
        HideAllPanels();

        Debug.Log("[InterrogationUI] Inicializado - Todos los paneles ocultos");
    }

    private void ApplyMobileLayout()
    {
        InterrogationLayout.Apply(new InterrogationLayout.Elements
        {
            panel = interrogationPanel != null ? (RectTransform)interrogationPanel.transform : null,
            hud = hudText != null ? hudText.rectTransform : null,
            endDay = endDayButton != null ? (RectTransform)endDayButton.transform : null,
            accuseNow = accuseNowButton != null ? (RectTransform)accuseNowButton.transform : null,
            notebook = viewCluesButton != null ? (RectTransform)viewCluesButton.transform : null,
            portrait = suspectImage != null ? suspectImage.rectTransform : null,
            chat = conversationScroll != null ? (RectTransform)conversationScroll.transform : null,
            waiting = waitingText != null ? waitingText.rectTransform : null,
            suspect = suspectDropdown != null ? (RectTransform)suspectDropdown.transform : null,
            evidence = evidenceDropdown != null ? (RectTransform)evidenceDropdown.transform : null,
            question = questionInput != null ? (RectTransform)questionInput.transform : null,
            send = askButton != null ? (RectTransform)askButton.transform : null,
            clueNotice = clueNotification != null ? (RectTransform)clueNotification.transform : null
        });
    }

    // ============================================
    // INTRO
    // ============================================

    public void ShowCaseIntro(string title, string description)
    {
        if (caseTitleText != null)
            caseTitleText.text = title;

        if (caseDescriptionText != null)
        {
            caseDescriptionText.text = description;

            // El parte del caso se escribe como el de la mañana; un toque lo completa
            Typewriter briefing = caseDescriptionText.GetComponent<Typewriter>() ?? caseDescriptionText.gameObject.AddComponent<Typewriter>();
            if (briefing.isActiveAndEnabled)
                briefing.Reveal(0, 1f);
        }

        Debug.Log("[InterrogationUI] Caso cargado: " + title);
    }

    public string CurrentSuspectId => currentSuspectId;

    public Dictionary<string, string> ExportConversations(out string shared)
    {
        shared = conversations.SharedText;
        return conversations.Export();
    }

    /// <summary>
    /// Vuelve a la partida guardada: conversaciones, sospechoso abierto y estados emocionales.
    /// </summary>
    public void ContinueInterrogation(IDictionary<string, string> texts, string shared, string suspectId,
                                      IReadOnlyDictionary<string, Emotion> emotions)
    {
        HideAllPanels();
        if (interrogationPanel != null)
        {
            interrogationPanel.SetActive(true);
            UIAnimations.FadeIn(this, interrogationPanel);
        }

        conversations.Import(texts, shared);
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
        StartCoroutine(ForceScrollToBottom());
    }

    private void SelectSuspect(string suspectId)
    {
        if (suspectId == currentSuspectId)
            return;

        typewriter?.Complete();
        currentSuspectId = suspectId;
        conversations.Select(suspectId);
        RefreshConversationView();

        UpdateSuspectImage(suspectId, instant: true);
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

        Texture2D texture = null;
        if (!string.IsNullOrEmpty(view.artId))
            texture = ArtLibrary.LoadFirst(PortraitPaths.Candidates(view.artId, emotion));
        if (texture == null && view.portraitKey != null && suspectImages.TryGetValue(view.portraitKey, out Texture2D legacy))
            texture = legacy;
        if (texture == null)
            texture = ArtLibrary.Placeholder(T.placeholder);

        suspectImage.texture = texture;
        suspectImage.gameObject.SetActive(true);
        emotionPresenter?.Apply(emotion, instant);
    }

    public void AddToConversation(string suspectId, string displayName, string question, string response)
    {
        string entry = $"<color={Theme.Hex(T.playerName)}><b>TÚ:</b></color> {question}\n\n" +
                       $"<color={Theme.Hex(T.suspectName)}><b>{displayName.ToUpper()}:</b></color> {response}\n\n" +
                       "─────────────────\n\n";

        if (suspectId == currentSuspectId && conversationText != null)
        {
            // La respuesta se escribe letra a letra, a la velocidad del estado emocional; un toque la completa
            int visibleBefore = typewriter != null ? typewriter.VisibleCount() : 0;
            conversations.Append(suspectId, entry);
            RefreshConversationView();
            typewriter?.Reveal(visibleBefore, EmotionStyle.For(EmotionOf(suspectId)).textSpeed);
        }
        else
        {
            // Respuesta de otro sospechoso (no debería ocurrir con la entrada bloqueada): a su conversación
            conversations.Append(suspectId, entry);
        }

        if (evidenceDropdown != null)
            evidenceDropdown.value = 0;

        SetInputEnabled(true);
        StartCoroutine(ForceScrollToBottom());
    }

    /// <summary>
    /// Aviso de sistema en la conversación abierta (o solo en pantalla si aún no hay sospechoso).
    /// </summary>
    private void AppendNotice(string notice)
    {
        if (currentSuspectId != null)
        {
            conversations.AppendToCurrent(notice);
            RefreshConversationView();
        }
        else if (conversationText != null)
        {
            conversationText.text += notice;
            StartCoroutine(ForceScrollToBottom());
        }
    }

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

    private void RefreshConversationView()
    {
        if (conversationText == null)
            return;

        conversationText.text = conversations.CurrentText;
        StartCoroutine(ForceScrollToBottom());
    }

    private IEnumerator ForceScrollToBottom()
    {
        yield return null;
        yield return null;

        if (conversationScroll != null)
        {
            Canvas.ForceUpdateCanvases();
            conversationScroll.verticalNormalizedPosition = 0f;
        }
    }

    public void UpdateGameState(int day, int maxDays, int questionsUsed, int questionsMax)
    {
        if (hudText != null)
        {
            hudText.text = $"DÍA {day}/{maxDays}  |  PREGUNTAS: {questionsUsed}/{questionsMax}";
        }
    }

    public void ShowWaiting(bool show)
    {
        if (waitingText != null)
        {
            waitingText.gameObject.SetActive(show);
            waitingText.text = show ? "Esperando respuesta..." : "";
        }
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
        AppendNotice($"<color={Theme.Hex(T.danger)}>⚠ {message}</color>\n\n");
    }

    /// <summary>
    /// La petición al LLM falló: avisa, devuelve la pregunta al campo de texto y reactiva el input.
    /// </summary>
    public void ShowRequestFailed(string message, string question)
    {
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
        if (clueNotification != null && clueNotificationText != null)
        {
            // Varias pistas seguidas se acumulan en el mismo aviso y reinician el temporizador
            visibleClueNames.Add(clueName);
            clueNotificationText.text = (visibleClueNames.Count == 1 ? "PISTA DESCUBIERTA:\n" : "PISTAS DESCUBIERTAS:\n") +
                                        string.Join("\n", visibleClueNames);
            clueNotification.SetActive(true);
            UIAnimations.Pop(this, clueNotification.transform);
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
        AppendNotice($"<color={Theme.Hex(T.contradiction)}>⚠ CONTRADICCIÓN: {text}</color>\n\n");

        // Destello del color de contradicción y sacudida del HUD
        UIAnimations.Flash(this, ContradictionOverlay(), T.contradiction, 0.25f);
        if (hudText != null)
            UIAnimations.Shake(this, hudText.rectTransform, 12f, T.contradictionAnimDuration);
    }

    // ARREGLADO: Panel de pistas
    private void ShowCluesPanel()
    {
        if (cluesPanel != null)
        {
            cluesPanel.SetActive(true);
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
        AppendNotice($"<color={Theme.Hex(T.success)}>✓ NUEVO SOSPECHOSO: {displayName}</color>\n\n");
    }

    public void ShowNotice(string message)
    {
        AppendNotice($"<color={Theme.Hex(T.systemText)}><i>{message}</i></color>\n\n");
    }

    public void ShowDayTransition(int newDay, string morningReport)
    {
        // El cambio de día se anota en todas las conversaciones, no solo en la abierta
        string header = $"\n<size={T.secondarySize}><color={Theme.Hex(T.accent)}>═══════════════════</color></size>\n" +
                        $"<size={T.headingSize}><b>DÍA {newDay}</b></size>\n" +
                        $"<size={T.secondarySize}><color={Theme.Hex(T.accent)}>═══════════════════</color></size>\n\n";

        if (!string.IsNullOrEmpty(morningReport))
            header += $"<color={Theme.Hex(T.systemText)}><i>Parte de la mañana: {morningReport}</i></color>\n\n";

        typewriter?.Complete();
        conversations.AppendToAll(header);
        RefreshConversationView();

        SetInputEnabled(true);
    }

    // ============================================
    // ACUSACIÓN
    // ============================================

    public void ShowAccusationPanel(List<SuspectView> options, bool canGoBack)
    {
        ShowPanel(accusationPanel);
        accusationOptions = options;
        EnsureAccusationBackButton();
        if (accusationBackButton != null)
            accusationBackButton.gameObject.SetActive(canGoBack);

        if (accusationDropdown != null)
        {
            accusationDropdown.ClearOptions();
            accusationDropdown.AddOptions(options.ConvertAll(v => v.displayName));
        }
        else
        {
            Debug.LogError("[InterrogationUI] ¡accusationDropdown es NULL!");
        }
    }

    /// <summary>
    /// Vuelve al interrogatorio desde el panel de acusación.
    /// </summary>
    public void ShowInterrogation()
    {
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
        accusationBackButton.onClick.RemoveAllListeners();
        accusationBackButton.onClick.AddListener(() => gameManager.CancelAccusation());

        TMP_Text label = clone.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
            label.text = "Volver";

        ThemeApplier.Apply(clone.transform);
    }

    public void OnAccuseClick()
    {
        if (accusationDropdown != null && accusationDropdown.value < accusationOptions.Count)
        {
            gameManager.MakeAccusation(accusationOptions[accusationDropdown.value].id);
        }
    }

    // ============================================
    // RESULTADO (MEJORADO)
    // ============================================

    public void ShowAccusationResult(AccusationResult result, string accusedName, string culpritName,
                                     int maxEvidence, string epilogue)
    {
        ShowPanel(resultPanel);

        if (resultTitleText != null)
        {
            switch (result.ending)
            {
                case Ending.Good:
                    resultTitleText.text = "✓ CASO RESUELTO - FINAL BUENO";
                    resultTitleText.color = T.success;
                    break;
                case Ending.Bittersweet:
                    resultTitleText.text = "⚠ ACERTASTE PERO SIN PRUEBAS";
                    resultTitleText.color = T.accent;
                    break;
                case Ending.Insufficient:
                    resultTitleText.text = "⚠ CULPABLE LIBRE POR FALTA DE PRUEBAS";
                    resultTitleText.color = T.contradiction;
                    break;
                default:
                    resultTitleText.text = "✗ CASO NO RESUELTO - FINAL MALO";
                    resultTitleText.color = T.danger;
                    break;
            }
        }

        if (resultDetailsText == null)
            return;

        string text = $"<b>TU ACUSACIÓN:</b> {accusedName}\n";
        text += $"<b>VERDADERO CULPABLE:</b> {culpritName}\n\n";
        text += $"<b>PISTAS INCRIMINATORIAS:</b> {result.incriminatingFound}\n";
        text += $"<b>CONTRADICCIONES DEL CULPABLE:</b> {result.contradictions}\n";
        text += $"<b>EVIDENCIA:</b> {result.evidence}/{maxEvidence} (hacen falta {InvestigationState.GoodThreshold} para una condena segura)\n\n";

        switch (result.ending)
        {
            case Ending.Good:
                text += $"<color={Theme.Hex(T.success)}><b>¡EXCELENTE TRABAJO!</b></color>\n";
                text += "Las pruebas y las contradicciones no dejan lugar a dudas. El culpable es condenado.\n\n";
                break;
            case Ending.Bittersweet:
                text += $"<color={Theme.Hex(T.accent)}><b>ACERTASTE PERO...</b></color>\n";
                text += "Identificaste al culpable, pero la defensa encuentra huecos. El juicio será largo e incierto.\n\n";
                break;
            case Ending.Insufficient:
                text += $"<color={Theme.Hex(T.contradiction)}><b>INSUFICIENTE EVIDENCIA</b></color>\n";
                text += "Tu intuición era correcta, pero sin pruebas el sospechoso queda en libertad.\n\n";
                break;
            default:
                text += $"<color={Theme.Hex(T.danger)}><b>INVESTIGACIÓN FALLIDA</b></color>\n";
                text += result.ignoredClearingClue
                    ? "Acusaste a alguien a quien tus propias pistas descartaban. Ignoraste pruebas.\n\n"
                    : "Acusaste a la persona equivocada. El verdadero culpable sigue libre.\n\n";
                break;
        }

        text += $"<b>LO QUE PASÓ DE VERDAD:</b>\n{epilogue}";
        resultDetailsText.text = text;
    }
}
