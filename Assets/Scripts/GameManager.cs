using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Flujo de la partida: sorteo de variante, días y preguntas, elenco y desbloqueos, partes de la mañana y acusación.
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum CaseSelection
    {
        Aleatoria,
        Caso1A, Caso1B, Caso1C,
        Caso2A, Caso2B, Caso2C,
        Caso3A, Caso3B, Caso3C
    }

    public const int SafetyUnlockDay = 3; // Al empezar este día se desbloquea todo el elenco

    [Header("Referencias")]
    [SerializeField] private AIConversationManager conversationManager;
    [SerializeField] private InterrogationUI interrogationUI;

    [Header("Configuración")]
    [SerializeField] private int maxDays = 7;
    [SerializeField] private int questionsPerDay = 5;

    [Header("Debug (solo editor)")]
    [Tooltip("Fuerza historia y variante. Se ignora fuera del editor")]
    [SerializeField] private CaseSelection debugCase = CaseSelection.Aleatoria;

    private StoryData story;
    private VariantData variant;
    private int currentDay = 1;
    private int questionsUsedToday = 0;
    private bool accusationMade;

    private readonly List<string> unlocked = new List<string>();

    private InvestigationState State => conversationManager.State;

    private void Start()
    {
        conversationManager.OnClueRevealed += OnClueRevealed;
        conversationManager.OnContradictionDetected += OnContradictionDetected;
        conversationManager.OnCharacterMentioned += OnCharacterMentioned;

        if (interrogationUI != null)
            interrogationUI.Initialize(this);

        SelectCase();

        Debug.Log("[GameManager] Inicializado. Esperando menú principal...");
    }

    private void SelectCase()
    {
        string forcedId = Application.isEditor ? CaseIdFor(debugCase) : null;

        if (forcedId == null || !CaseLibrary.TryFind(forcedId, out story, out variant))
        {
            if (forcedId != null)
                Debug.LogWarning($"[GameManager] La variante {forcedId} aún no existe; se sortea entre las registradas.");

            var all = CaseLibrary.AllVariants().ToList();
            (story, variant) = all[Random.Range(0, all.Count)];
        }

        conversationManager.StartCase(story, variant);

        unlocked.Clear();
        unlocked.AddRange(story.cast.Where(c => c.startsUnlocked).Select(c => c.id));

        Debug.Log($"[GameManager] Caso: {story.title} ({variant.id}) · culpable: {variant.culpritId}");
    }

    private static string CaseIdFor(CaseSelection selection)
    {
        return selection == CaseSelection.Aleatoria ? null : selection.ToString().Substring("Caso".Length);
    }

    // ============================================
    // FLUJO
    // ============================================

    public void ShowCaseIntro()
    {
        interrogationUI?.ShowCaseIntro(story.title, story.intro);
    }

    /// <summary>
    /// Llamado por la UI al pulsar "Empezar".
    /// </summary>
    public void BeginInterrogation()
    {
        RefreshSuspects();
        interrogationUI?.SetEvidenceOptions(DiscoveredClues());
        interrogationUI?.UpdateCluesList(DiscoveredClues());
        interrogationUI?.UpdateContradictionsList(new List<string>());
        UpdateGameState();
    }

    public async void AskQuestion(string characterId, string question, string shownClueId)
    {
        if (questionsUsedToday >= questionsPerDay)
        {
            interrogationUI?.ShowError("No te quedan preguntas hoy.");
            return;
        }

        ClueData shownClue = string.IsNullOrEmpty(shownClueId) ? null : variant.Clue(shownClueId);
        string asked = TurnAnalyzer.BuildUserMessage(question, shownClue);

        interrogationUI?.ShowWaiting(true);

        LLMResult result = await conversationManager.AskSuspect(characterId, question, currentDay, shownClue);

        interrogationUI?.ShowWaiting(false);

        // Una petición fallida no gasta pregunta del día
        if (!result.Success)
        {
            interrogationUI?.ShowRequestFailed(result.ErrorMessage, question);
            return;
        }

        questionsUsedToday++;
        interrogationUI?.AddToConversation(characterId, story.Character(characterId).DisplayName, asked, result.Text);
        UpdateGameState();
    }

    public void EndDay()
    {
        currentDay++;
        questionsUsedToday = 0;

        if (currentDay > maxDays)
        {
            ShowAccusationPanel();
            return;
        }

        interrogationUI?.ShowDayTransition(currentDay, MorningReport(currentDay));

        if (currentDay >= SafetyUnlockDay)
        {
            foreach (CharacterData character in story.cast.Where(c => !unlocked.Contains(c.id)))
                Unlock(character.id, $"Un agente te informa: conviene hablar con {character.name}.");
        }

        UpdateGameState();
    }

    private string MorningReport(int day)
    {
        string report = day - 1 < variant.morningReports.Length ? variant.morningReports[day - 1] : "";

        if (day == maxDays)
            report += "\nÚltimo día: al terminarlo tendrás que acusar a alguien.";
        else if (day == maxDays - 1)
            report += "\nQuedan dos días de investigación.";

        return report.Trim();
    }

    public void ForceAccusationPanel()
    {
        Debug.Log("[GameManager] Acusación anticipada activada");
        ShowAccusationPanel();
    }

    // ============================================
    // SOSPECHOSOS
    // ============================================

    private void OnCharacterMentioned(string characterId)
    {
        Unlock(characterId, null);
    }

    private void Unlock(string characterId, string notice)
    {
        if (unlocked.Contains(characterId))
            return;

        unlocked.Add(characterId);
        CharacterData character = story.Character(characterId);
        Debug.Log($"[GameManager] Desbloqueado: {character.name}");

        if (!string.IsNullOrEmpty(notice))
            interrogationUI?.ShowNotice(notice);

        interrogationUI?.ShowSuspectUnlocked(character.DisplayName);
        RefreshSuspects();
    }

    private List<SuspectView> UnlockedSuspects()
    {
        // En el orden del elenco
        return story.cast.Where(c => unlocked.Contains(c.id)).Select(SuspectView.From).ToList();
    }

    private void RefreshSuspects()
    {
        interrogationUI?.SetSuspects(UnlockedSuspects());
    }

    // ============================================
    // PISTAS Y CONTRADICCIONES
    // ============================================

    private List<ClueData> DiscoveredClues()
    {
        return State.DiscoveredClueIds.Select(variant.Clue).ToList();
    }

    private void OnClueRevealed(ClueData clue)
    {
        Debug.Log($"[GameManager] Pista: {clue.playerName}");
        interrogationUI?.ShowClueNotification(clue.playerName);
        interrogationUI?.UpdateCluesList(DiscoveredClues());
        interrogationUI?.SetEvidenceOptions(DiscoveredClues());
    }

    private void OnContradictionDetected(string text)
    {
        Debug.Log($"[GameManager] Contradicción: {text}");
        interrogationUI?.ShowContradictionNotification(text);
        interrogationUI?.UpdateContradictionsList(
            State.ContradictionClueIds.Select(id => conversationManager.DescribeContradiction(variant.Clue(id))).ToList());
    }

    // ============================================
    // ACUSACIÓN
    // ============================================

    private void ShowAccusationPanel()
    {
        interrogationUI?.ShowAccusationPanel(UnlockedSuspects());
    }

    public void MakeAccusation(string accusedId)
    {
        if (accusationMade)
            return;

        accusationMade = true;
        AccusationResult result = State.Accuse(accusedId);

        Debug.Log($"[GameManager] Acusación: {accusedId} → {result.ending} (evidencia {result.evidence})");

        interrogationUI?.ShowAccusationResult(
            result,
            story.Character(accusedId).name,
            story.Character(variant.culpritId).name,
            InvestigationState.MaxEvidenceWithoutCulprit(variant),
            variant.epilogue);
    }

    public bool CanAskMoreQuestions()
    {
        return questionsUsedToday < questionsPerDay;
    }

    private void UpdateGameState()
    {
        interrogationUI?.UpdateGameState(currentDay, maxDays, questionsUsedToday, questionsPerDay);
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
