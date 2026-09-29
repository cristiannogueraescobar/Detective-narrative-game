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
    private Dictionary<string, string> conversationsBySuspect = new Dictionary<string, string>();
    private Dictionary<string, Texture2D> suspectImages = new Dictionary<string, Texture2D>();
    private readonly List<string> visibleClueNames = new List<string>(); // Pistas del aviso en pantalla

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

    public void ShowCaseIntro(string title, string description)
    {
        if (caseTitleText != null)
            caseTitleText.text = title;

        if (caseDescriptionText != null)
            caseDescriptionText.text = description;

        Debug.Log("[InterrogationUI] Caso cargado: " + title);
    }

    public void StartInterrogation()
    {
        Debug.Log("[InterrogationUI] StartInterrogation llamado");

        HideAllPanels();

        if (interrogationPanel != null)
            interrogationPanel.SetActive(true);

        if (conversationText != null)
            conversationText.text = "";

        conversationsBySuspect.Clear();
        currentSuspectId = null;
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

        if (!string.IsNullOrEmpty(currentSuspectId) && conversationText != null)
            conversationsBySuspect[currentSuspectId] = conversationText.text;

        currentSuspectId = suspectId;

        if (conversationText != null)
        {
            conversationsBySuspect.TryGetValue(suspectId, out string saved);
            conversationText.text = saved ?? "";
        }

        SuspectView view = suspects.Find(v => v.id == suspectId);
        UpdateSuspectImage(view.portraitKey);
    }

    private void UpdateSuspectImage(string portraitKey)
    {
        if (suspectImage == null)
            return;

        if (portraitKey != null && suspectImages.ContainsKey(portraitKey) && suspectImages[portraitKey] != null)
        {
            suspectImage.texture = suspectImages[portraitKey];
            suspectImage.gameObject.SetActive(true);
        }
        else
        {
            suspectImage.gameObject.SetActive(false);
        }
    }

    public void AddToConversation(string suspectId, string displayName, string question, string response)
    {
        string entry = $"<color=#00AAFF><b>TÚ:</b></color> {question}\n\n" +
                       $"<color=#FFFFFF><b>{displayName.ToUpper()}:</b></color> {response}\n\n" +
                       "─────────────────\n\n";

        if (suspectId == currentSuspectId && conversationText != null)
        {
            conversationText.text += entry;
            conversationsBySuspect[suspectId] = conversationText.text;
        }
        else
        {
            conversationsBySuspect.TryGetValue(suspectId, out string saved);
            conversationsBySuspect[suspectId] = (saved ?? "") + entry;
        }

        if (evidenceDropdown != null)
            evidenceDropdown.value = 0;

        SetInputEnabled(true);
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
        if (conversationText != null)
        {
            conversationText.text += $"<color=#FF4444>⚠ {message}</color>\n\n";
        }

        StartCoroutine(ForceScrollToBottom());
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

    public void UpdateCluesList(List<ClueData> clues)
    {
        if (cluesText != null)
        {
            if (clues.Count == 0)
            {
                cluesText.text = "No hay pistas descubiertas aún.";
            }
            else
            {
                cluesText.text = "<b>PISTAS DESCUBIERTAS:</b>\n\n";
                foreach (ClueData clue in clues)
                {
                    cluesText.text += $"• <b>{clue.playerName}</b>: {clue.summary}\n\n";
                }
            }
        }
    }

    public void UpdateContradictionsList(List<string> contradictions)
    {
        if (contradictionsText != null)
        {
            if (contradictions.Count == 0)
            {
                contradictionsText.text = "No se han detectado contradicciones.";
            }
            else
            {
                contradictionsText.text = "<b>CONTRADICCIONES:</b>\n\n";
                foreach (string contra in contradictions)
                {
                    contradictionsText.text += $"• {contra}\n";
                }
            }
        }
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
        if (conversationText != null)
        {
            conversationText.text += $"<color=#FFAA00>⚠ CONTRADICCIÓN: {text}</color>\n\n";
            StartCoroutine(ForceScrollToBottom());
        }
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
        if (conversationText != null)
        {
            conversationText.text += $"<color=#00FF88>✓ NUEVO SOSPECHOSO: {displayName}</color>\n\n";
            StartCoroutine(ForceScrollToBottom());
        }
    }

    public void ShowNotice(string message)
    {
        if (conversationText != null)
        {
            conversationText.text += $"<color=#AAAAFF><i>{message}</i></color>\n\n";
            StartCoroutine(ForceScrollToBottom());
        }
    }

    public void ShowDayTransition(int newDay, string morningReport)
    {
        if (conversationText != null)
        {
            conversationText.text += $"\n<size=18><color=#FFFF00>═══════════════════</color></size>\n";
            conversationText.text += $"<size=16><b>DÍA {newDay}</b></size>\n";
            conversationText.text += $"<size=18><color=#FFFF00>═══════════════════</color></size>\n\n";

            if (!string.IsNullOrEmpty(morningReport))
                conversationText.text += $"<color=#CCCCCC><i>Parte de la mañana: {morningReport}</i></color>\n\n";

            StartCoroutine(ForceScrollToBottom());
        }

        SetInputEnabled(true);
    }

    // ============================================
    // ACUSACIÓN
    // ============================================

    public void ShowAccusationPanel(List<SuspectView> options)
    {
        ShowPanel(accusationPanel);
        accusationOptions = options;

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
                    resultTitleText.color = Color.green;
                    break;
                case Ending.Bittersweet:
                    resultTitleText.text = "⚠ ACERTASTE PERO SIN PRUEBAS";
                    resultTitleText.color = Color.yellow;
                    break;
                case Ending.Insufficient:
                    resultTitleText.text = "⚠ CULPABLE LIBRE POR FALTA DE PRUEBAS";
                    resultTitleText.color = new Color(1f, 0.5f, 0f); // Naranja
                    break;
                default:
                    resultTitleText.text = "✗ CASO NO RESUELTO - FINAL MALO";
                    resultTitleText.color = Color.red;
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
                text += "<color=green><b>¡EXCELENTE TRABAJO!</b></color>\n";
                text += "Las pruebas y las contradicciones no dejan lugar a dudas. El culpable es condenado.\n\n";
                break;
            case Ending.Bittersweet:
                text += "<color=yellow><b>ACERTASTE PERO...</b></color>\n";
                text += "Identificaste al culpable, pero la defensa encuentra huecos. El juicio será largo e incierto.\n\n";
                break;
            case Ending.Insufficient:
                text += "<color=orange><b>INSUFICIENTE EVIDENCIA</b></color>\n";
                text += "Tu intuición era correcta, pero sin pruebas el sospechoso queda en libertad.\n\n";
                break;
            default:
                text += "<color=red><b>INVESTIGACIÓN FALLIDA</b></color>\n";
                text += result.ignoredClearingClue
                    ? "Acusaste a alguien a quien tus propias pistas descartaban. Ignoraste pruebas.\n\n"
                    : "Acusaste a la persona equivocada. El verdadero culpable sigue libre.\n\n";
                break;
        }

        text += $"<b>LO QUE PASÓ DE VERDAD:</b>\n{epilogue}";
        resultDetailsText.text = text;
    }
}
