using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// GAME MANAGER MEJORADO - Con acusación anticipada y 4 finales
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private AIConversationManager conversationManager;
    [SerializeField] private InterrogationUI interrogationUI;
    
    [Header("Configuración")]
    [SerializeField] private int maxDays = 7;
    [SerializeField] private int questionsPerDay = 5;
    
    private AIConversationManager.CaseData currentCase;
    private int currentDay = 1;
    private int questionsUsedToday = 0;
    
    private List<string> allSuspects = new List<string> { "Padre", "Madre", "Hermano", "Vecina", "Detective", "Cartero", "Dueño del Bar" };
    private HashSet<string> unlockedSuspects = new HashSet<string>();
    
    private HashSet<string> discoveredClues = new HashSet<string>();
    private Dictionary<string, string> clueNames = new Dictionary<string, string>();
    private List<string> contradictions = new List<string>();
    
    private void Start()
    {
        conversationManager.OnClueRevealed += OnClueRevealed;
        conversationManager.OnContradictionDetected += OnContradictionDetected;
        conversationManager.OnResponseReceived += OnResponseReceived;
        conversationManager.OnError += OnError;
        
        if (interrogationUI != null)
        {
            interrogationUI.Initialize(this);
        }
        
        SelectRandomCase();
        
        unlockedSuspects.Add("Padre");
        unlockedSuspects.Add("Madre");
        unlockedSuspects.Add("Hermano");
        
        Debug.Log("[GameManager] Inicializado. Esperando menú principal...");
    }
    
    private void SelectRandomCase()
    {
        string[] caseIds = { "1A", "1B", "1C", "2A", "2B", "2C", "3A", "3B", "3C" };
        string randomId = caseIds[Random.Range(0, caseIds.Length)];
        
        var casesField = typeof(AIConversationManager).GetField("cases", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var cases = (Dictionary<string, AIConversationManager.CaseData>)casesField.GetValue(conversationManager);
        
        currentCase = cases[randomId];
        
        Debug.Log($"[GameManager] Caso: {currentCase.title} ({currentCase.id})");
        Debug.Log($"[GameManager] Culpable: {currentCase.culprit}");
    }
    
    public void ShowCaseIntro()
    {
        if (interrogationUI != null)
        {
            interrogationUI.ShowCaseIntro(currentCase.title, currentCase.description);
        }
    }
    
    public async void AskQuestion(string suspectName, string question)
    {
        if (questionsUsedToday >= questionsPerDay)
        {
            interrogationUI?.ShowError("No te quedan preguntas hoy.");
            return;
        }
        
        questionsUsedToday++;
        UpdateGameState();
        
        interrogationUI?.ShowWaiting(true);
        
        string response = await conversationManager.AskSuspect(suspectName, question, currentCase.id, currentDay);
        
        interrogationUI?.ShowWaiting(false);
        interrogationUI?.AddToConversation(suspectName, question, response);
        UpdateGameState();
    }
    
    public void EndDay()
    {
        currentDay++;
        questionsUsedToday = 0;
        
        if (currentDay > maxDays)
        {
            ShowAccusationPanel();
        }
        else
        {
            interrogationUI?.ShowDayTransition(currentDay);
            UpdateGameState();
            CheckSuspectUnlocks();
        }
    }
    
    // NUEVO: Forzar panel de acusación antes del día 7
    public void ForceAccusationPanel()
    {
        Debug.Log("[GameManager] Acusación anticipada activada");
        ShowAccusationPanel();
    }
    
    private void CheckSuspectUnlocks()
    {
        if (!unlockedSuspects.Contains("Vecina") && discoveredClues.Count >= 1)
        {
            UnlockSuspect("Vecina");
        }
        
        if (!unlockedSuspects.Contains("Cartero") && discoveredClues.Count >= 2)
        {
            UnlockSuspect("Cartero");
        }
        
        if (!unlockedSuspects.Contains("Detective") && discoveredClues.Count >= 3)
        {
            UnlockSuspect("Detective");
        }
        
        if (currentCase.id.StartsWith("2") && !unlockedSuspects.Contains("Dueño del Bar") && discoveredClues.Count >= 2)
        {
            UnlockSuspect("Dueño del Bar");
        }
    }
    
    private void UnlockSuspect(string suspectName)
    {
        if (unlockedSuspects.Add(suspectName))
        {
            Debug.Log($"[GameManager] Desbloqueado: {suspectName}");
            interrogationUI?.ShowSuspectUnlocked(suspectName);
            interrogationUI?.UpdateSuspectList(allSuspects, unlockedSuspects);
        }
    }
    
    private void OnClueRevealed(string clueId, string clueName, string description)
    {
        if (discoveredClues.Add(clueId))
        {
            clueNames[clueId] = clueName;
            Debug.Log($"[GameManager] Pista: {clueName}");
            interrogationUI?.ShowClueNotification(clueName);
            interrogationUI?.UpdateCluesList(GetDiscoveredCluesNames());
            CheckSuspectUnlocks();
        }
    }
    
    private void OnContradictionDetected(string contradiction)
    {
        contradictions.Add(contradiction);
        Debug.Log($"[GameManager] Contradicción: {contradiction}");
        interrogationUI?.ShowContradictionNotification(contradiction);
        interrogationUI?.UpdateContradictionsList(contradictions);
    }
    
    private void OnResponseReceived(string suspect, string question, string response)
    {
        Debug.Log($"[GameManager] {suspect} respondió");
    }
    
    private void OnError(string error)
    {
        Debug.LogError($"[GameManager] Error: {error}");
        interrogationUI?.ShowError(error);
    }
    
    private void ShowAccusationPanel()
    {
        interrogationUI?.ShowAccusationPanel(allSuspects, unlockedSuspects);
    }
    
    // MEJORADO: Sistema de 4 finales
    public void MakeAccusation(string accused)
    {
        bool correct = accused == currentCase.culprit;
        int cluesFound = discoveredClues.Count;
        int totalClues = currentCase.requiredClues.Count;
        float cluePercentage = (float)cluesFound / totalClues;
        
        string ending;
        
        // 4 FINALES POSIBLES:
        if (correct && cluePercentage >= 0.75f)
        {
            // FINAL 1: GOOD - Acertaste + 75%+ pruebas
            ending = "GOOD";
            Debug.Log("[GameManager] FINAL BUENO - Culpable condenado con pruebas");
        }
        else if (correct && cluePercentage >= 0.5f && cluePercentage < 0.75f)
        {
            // FINAL 2: BITTERSWEET - Acertaste + 50-74% pruebas
            ending = "BITTERSWEET";
            Debug.Log("[GameManager] FINAL AGRIDULCE - Acertaste pero pocas pruebas");
        }
        else if (correct && cluePercentage < 0.5f)
        {
            // FINAL 3: INSUFFICIENT - Acertaste pero <50% pruebas (QUEDA LIBRE)
            ending = "INSUFFICIENT";
            Debug.Log("[GameManager] FINAL INSUFICIENTE - Culpable libre por falta de pruebas");
        }
        else
        {
            // FINAL 4: BAD - Acusación incorrecta
            ending = "BAD";
            Debug.Log("[GameManager] FINAL MALO - Acusación incorrecta");
        }
        
        interrogationUI?.ShowAccusationResult(
            correct, 
            accused, 
            currentCase.culprit, 
            ending, 
            cluesFound, 
            totalClues, 
            contradictions.Count
        );
    }
    
    public bool CanAskMoreQuestions()
    {
        return questionsUsedToday < questionsPerDay;
    }
    
    private void UpdateGameState()
    {
        interrogationUI?.UpdateGameState(currentDay, questionsUsedToday, questionsPerDay);
    }
    
    private List<string> GetDiscoveredCluesNames()
    {
        return discoveredClues.Select(id => clueNames.ContainsKey(id) ? clueNames[id] : id).ToList();
    }
    
    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
    
    public void BackToMenu()
    {
        Debug.Log("[GameManager] Menú principal");
    }
}
