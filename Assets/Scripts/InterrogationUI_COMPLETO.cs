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
    
    private string currentSuspect = "Padre";
    private List<string> allSuspects = new List<string>();
    private HashSet<string> unlockedSuspects = new HashSet<string>();
    private Dictionary<string, string> conversationsBySuspect = new Dictionary<string, string>();
    private Dictionary<string, Texture2D> suspectImages = new Dictionary<string, Texture2D>();
    
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
        {
            interrogationPanel.SetActive(true);
            Debug.Log("[InterrogationUI] InterrogationPanel activado");
        }
        
        if (conversationText != null)
            conversationText.text = "";
        
        conversationsBySuspect["Padre"] = "";
        conversationsBySuspect["Madre"] = "";
        conversationsBySuspect["Hermano"] = "";
        
        // IMPORTANTE: Inicializar sospechosos desbloqueados por defecto
        unlockedSuspects.Clear();
        unlockedSuspects.Add("Padre");
        unlockedSuspects.Add("Madre");
        unlockedSuspects.Add("Hermano");
        
        Debug.Log($"[InterrogationUI] Sospechosos desbloqueados iniciales: {string.Join(", ", unlockedSuspects)}");
        
        UpdateSuspectImage("Padre");
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
        
        if (string.IsNullOrEmpty(question))
        {
            ShowError("Escribe una pregunta primero.");
            return;
        }
        
        if (!gameManager.CanAskMoreQuestions())
        {
            ShowError("No te quedan preguntas hoy.");
            return;
        }
        
        questionInput.text = "";
        SetInputEnabled(false);
        gameManager.AskQuestion(currentSuspect, question);
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
        if (suspectDropdown != null && suspectDropdown.options.Count > index)
        {
            if (!string.IsNullOrEmpty(currentSuspect) && conversationText != null)
            {
                conversationsBySuspect[currentSuspect] = conversationText.text;
            }
            
            currentSuspect = suspectDropdown.options[index].text;
            
            if (conversationText != null)
            {
                if (conversationsBySuspect.ContainsKey(currentSuspect))
                {
                    conversationText.text = conversationsBySuspect[currentSuspect];
                }
                else
                {
                    conversationText.text = "";
                    conversationsBySuspect[currentSuspect] = "";
                }
            }
            
            UpdateSuspectImage(currentSuspect);
            StartCoroutine(ForceScrollToBottom());
        }
    }
    
    private void UpdateSuspectImage(string suspectName)
    {
        if (suspectImage == null)
            return;
        
        if (suspectImages.ContainsKey(suspectName) && suspectImages[suspectName] != null)
        {
            suspectImage.texture = suspectImages[suspectName];
            suspectImage.gameObject.SetActive(true);
        }
        else
        {
            suspectImage.gameObject.SetActive(false);
        }
    }
    
    public void AddToConversation(string suspect, string question, string response)
    {
        if (conversationText != null)
        {
            conversationText.text += $"<color=#00AAFF><b>TÚ:</b></color> {question}\n\n";
            conversationText.text += $"<color=#FFFFFF><b>{suspect.ToUpper()}:</b></color> {response}\n\n";
            conversationText.text += "─────────────────\n\n";
            
            conversationsBySuspect[suspect] = conversationText.text;
        }
        
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
    
    public void UpdateGameState(int day, int questionsUsed, int questionsMax)
    {
        if (hudText != null)
        {
            hudText.text = $"DÍA {day}/7  |  PREGUNTAS: {questionsUsed}/{questionsMax}";
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
    }
    
    public void ShowError(string message)
    {
        if (conversationText != null)
        {
            conversationText.text += $"<color=#FF4444>⚠ {message}</color>\n\n";
        }
        
        StartCoroutine(ForceScrollToBottom());
    }
    
    // ============================================
    // PISTAS Y CONTRADICCIONES (ARREGLADO)
    // ============================================
    
    public void UpdateCluesList(List<string> clues)
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
                foreach (string clue in clues)
                {
                    cluesText.text += $"• {clue}\n";
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
            clueNotificationText.text = $"PISTA DESCUBIERTA:\n{clueName}";
            clueNotification.SetActive(true);
            Invoke(nameof(HideClueNotification), 3f);
        }
    }
    
    private void HideClueNotification()
    {
        if (clueNotification != null)
            clueNotification.SetActive(false);
    }
    
    public void ShowContradictionNotification(string suspectName)
    {
        if (conversationText != null)
        {
            conversationText.text += $"<color=#FFAA00>⚠ {suspectName} cambió su versión!</color>\n\n";
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
    
    public void UpdateSuspectList(List<string> all, HashSet<string> unlocked)
    {
        Debug.Log($"[InterrogationUI] UpdateSuspectList llamado");
        Debug.Log($"[InterrogationUI] All suspects: {string.Join(", ", all)}");
        Debug.Log($"[InterrogationUI] Unlocked suspects: {string.Join(", ", unlocked)}");
        
        allSuspects = all;
        unlockedSuspects = unlocked;
        
        if (suspectDropdown != null)
        {
            suspectDropdown.ClearOptions();
            
            List<string> options = new List<string>();
            foreach (string suspect in all)
            {
                if (unlocked.Contains(suspect))
                {
                    options.Add(suspect);
                    Debug.Log($"[InterrogationUI] Dropdown option added: {suspect}");
                }
            }
            
            if (options.Count == 0)
            {
                Debug.LogWarning("[InterrogationUI] No unlocked suspects! Using defaults.");
                options.Add("Padre");
                options.Add("Madre");
                options.Add("Hermano");
            }
            
            suspectDropdown.AddOptions(options);
            
            if (options.Count > 0)
            {
                currentSuspect = options[0];
                UpdateSuspectImage(currentSuspect);
                Debug.Log($"[InterrogationUI] Current suspect set to: {currentSuspect}");
            }
        }
    }
    
    public void ShowSuspectUnlocked(string suspectName)
    {
        if (conversationText != null)
        {
            conversationText.text += $"<color=#00FF88>✓ NUEVO SOSPECHOSO: {suspectName}</color>\n\n";
            StartCoroutine(ForceScrollToBottom());
        }
    }
    
    public void ShowDayTransition(int newDay)
    {
        if (conversationText != null)
        {
            conversationText.text += $"\n<size=18><color=#FFFF00>═══════════════════</color></size>\n";
            conversationText.text += $"<size=16><b>DÍA {newDay}</b></size>\n";
            conversationText.text += $"<size=18><color=#FFFF00>═══════════════════</color></size>\n\n";
            StartCoroutine(ForceScrollToBottom());
        }
        
        SetInputEnabled(true);
    }
    
    // ============================================
    // ACUSACIÓN
    // ============================================
    
    public void ShowAccusationPanel(List<string> all, HashSet<string> unlocked)
    {
        Debug.Log($"[InterrogationUI] ShowAccusationPanel llamado");
        Debug.Log($"[InterrogationUI] Total sospechosos recibidos: {all.Count}");
        Debug.Log($"[InterrogationUI] Sospechosos desbloqueados: {unlocked.Count}");
        
        ShowPanel(accusationPanel);
        
        if (accusationDropdown != null)
        {
            accusationDropdown.ClearOptions();
            
            List<string> options = new List<string>();
            
            // Construir lista de opciones con TODOS los sospechosos desbloqueados
            foreach (string suspect in all)
            {
                if (unlocked.Contains(suspect))
                {
                    options.Add(suspect);
                    Debug.Log($"[InterrogationUI] ✓ Agregado al dropdown: {suspect}");
                }
                else
                {
                    Debug.Log($"[InterrogationUI] ✗ NO agregado (bloqueado): {suspect}");
                }
            }
            
            // Verificación de seguridad
            if (options.Count == 0)
            {
                Debug.LogWarning("[InterrogationUI] ¡WARNING! No hay opciones en el dropdown. Usando sospechosos por defecto.");
                options.Add("Padre");
                options.Add("Madre");
                options.Add("Hermano");
            }
            
            accusationDropdown.AddOptions(options);
            
            Debug.Log($"[InterrogationUI] Total opciones en dropdown: {options.Count}");
            Debug.Log($"[InterrogationUI] Opciones: {string.Join(", ", options)}");
        }
        else
        {
            Debug.LogError("[InterrogationUI] ¡accusationDropdown es NULL!");
        }
    }
    
    public void OnAccuseClick()
    {
        if (accusationDropdown != null && accusationDropdown.options.Count > 0)
        {
            string accused = accusationDropdown.options[accusationDropdown.value].text;
            gameManager.MakeAccusation(accused);
        }
    }
    
    // ============================================
    // RESULTADO (MEJORADO)
    // ============================================
    
    public void ShowAccusationResult(bool correct, string accused, string realCulprit, 
                                      string ending, int cluesFound, int cluesTotal, int contradictions)
    {
        ShowPanel(resultPanel);
        
        if (resultTitleText != null)
        {
            if (ending == "GOOD")
            {
                resultTitleText.text = "✓ CASO RESUELTO - FINAL BUENO";
                resultTitleText.color = Color.green;
            }
            else if (ending == "BITTERSWEET")
            {
                resultTitleText.text = "⚠ ACERTASTE PERO SIN PRUEBAS";
                resultTitleText.color = Color.yellow;
            }
            else if (ending == "BAD")
            {
                resultTitleText.text = "✗ CASO NO RESUELTO - FINAL MALO";
                resultTitleText.color = Color.red;
            }
            else if (ending == "INSUFFICIENT")
            {
                resultTitleText.text = "⚠ CULPABLE LIBRE POR FALTA DE PRUEBAS";
                resultTitleText.color = new Color(1f, 0.5f, 0f); // Naranja
            }
        }
        
        if (resultDetailsText != null)
        {
            resultDetailsText.text = $"<b>TU ACUSACIÓN:</b> {accused}\n";
            resultDetailsText.text += $"<b>VERDADERO CULPABLE:</b> {realCulprit}\n\n";
            resultDetailsText.text += $"<b>PISTAS DESCUBIERTAS:</b> {cluesFound}/{cluesTotal}\n";
            resultDetailsText.text += $"<b>CONTRADICCIONES DETECTADAS:</b> {contradictions}\n\n";
            
            float percentage = (float)cluesFound / cluesTotal * 100f;
            resultDetailsText.text += $"<b>EVIDENCIA RECOPILADA:</b> {percentage:F0}%\n\n";
            
            // Mensajes según el ending
            if (ending == "GOOD")
            {
                resultDetailsText.text += "<color=green><b>¡EXCELENTE TRABAJO!</b></color>\n\n";
                resultDetailsText.text += "Resolviste el caso con todas las pruebas necesarias. ";
                resultDetailsText.text += "El culpable fue condenado y la justicia prevalece.\n\n";
                resultDetailsText.text += "Tu investigación fue meticulosa y exhaustiva.";
            }
            else if (ending == "BITTERSWEET")
            {
                resultDetailsText.text += "<color=yellow><b>ACERTASTE PERO...</b></color>\n\n";
                resultDetailsText.text += "Identificaste correctamente al culpable, pero sin pruebas suficientes ";
                resultDetailsText.text += "para asegurar una condena. El caso puede reabrirse si aparecen más evidencias.\n\n";
                resultDetailsText.text += $"Necesitabas al menos {Mathf.Ceil(cluesTotal * 0.75f)} pistas para un caso sólido.";
            }
            else if (ending == "INSUFFICIENT")
            {
                resultDetailsText.text += "<color=orange><b>INSUFICIENTE EVIDENCIA</b></color>\n\n";
                resultDetailsText.text += "Aunque acusaste correctamente, NO tenías pruebas suficientes. ";
                resultDetailsText.text += "El sospechoso queda en libertad por falta de evidencia contundente.\n\n";
                resultDetailsText.text += "Tu intuición era correcta, pero en el sistema judicial las pruebas son esenciales.";
            }
            else // BAD
            {
                resultDetailsText.text += "<color=red><b>INVESTIGACIÓN FALLIDA</b></color>\n\n";
                resultDetailsText.text += "Acusaste a la persona equivocada. El verdadero culpable sigue libre ";
                resultDetailsText.text += "y la justicia no se ha cumplido.\n\n";
                resultDetailsText.text += "Una investigación más profunda habría revelado la verdad.";
            }
        }
    }
}
