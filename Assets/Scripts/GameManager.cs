using System;
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

    // Los avisos que provoca una respuesta se muestran después de la respuesta, no antes
    private readonly NoticeQueue notices = new NoticeQueue();

    private InvestigationState State => conversationManager.State;

    private void Start()
    {
        conversationManager.OnClueRevealed += OnClueRevealed;
        conversationManager.OnContradictionDetected += OnContradictionDetected;
        conversationManager.OnCharacterMentioned += OnCharacterMentioned;

        AudioListener.volume = GameSettings.Volume;

        if (interrogationUI != null)
        {
            // El tema se aplica a toda la UI antes de que se creen controles por código (que lo heredan)
            ThemeApplier.Apply(interrogationUI.transform.root);
            interrogationUI.Initialize(this);
        }

        SelectCase();

        highContrastApplied = GameSettings.HighContrast;
        GameSettings.Changed += OnSettingsChanged;

        Debug.Log("[GameManager] Inicializado. Esperando menú principal...");
    }

    private bool highContrastApplied;

    // El alto contraste cambia los colores del tema: se vuelven a aplicar a toda la UI al momento
    private void OnSettingsChanged()
    {
        if (GameSettings.HighContrast == highContrastApplied || interrogationUI == null)
            return;
        highContrastApplied = GameSettings.HighContrast;
        ThemeApplier.Apply(interrogationUI.transform.root);
        interrogationUI.RestyleForTheme();
    }

    private void OnDestroy()
    {
        GameSettings.Changed -= OnSettingsChanged;

        if (conversationManager == null)
            return;

        conversationManager.OnClueRevealed -= OnClueRevealed;
        conversationManager.OnContradictionDetected -= OnContradictionDetected;
        conversationManager.OnCharacterMentioned -= OnCharacterMentioned;
    }

    // ============================================
    // GUARDADO
    // ============================================

    private void SaveGame()
    {
        if (accusationMade || State == null || currentDay > maxDays)
            return;

        var data = new SaveData
        {
            variantId = variant.id,
            day = currentDay,
            questionsUsedToday = questionsUsedToday,
            currentSuspect = interrogationUI != null ? interrogationUI.CurrentSuspectId : null,
            unlocked = new List<string>(unlocked),
            discovered = new List<string>(State.DiscoveredClueIds),
            culpritToldLie = State.CulpritToldLie
        };

        foreach (CharacterData character in story.cast)
        {
            foreach (string clueId in State.ShownTo(character.id))
                data.shown.Add(new SaveData.Shown { characterId = character.id, clueId = clueId });
        }

        foreach (var pair in conversationManager.Histories)
            data.histories.Add(new SaveData.History { characterId = pair.Key, messages = new List<ChatMessage>(pair.Value) });
        foreach (var pair in conversationManager.Emotions)
            data.emotions.Add(new SaveData.EmotionEntry { characterId = pair.Key, emotion = pair.Value.ToString() });

        if (interrogationUI != null)
            SaveSystem.StoreConversations(interrogationUI.Conversations, data);

        SaveSystem.Save(data);
    }

    /// <summary>
    /// Continúa la partida guardada. Devuelve false si no hay guardado válido (se empieza de cero).
    /// </summary>
    public bool ContinueSavedGame()
    {
        if (!SaveSystem.TryLoad(out SaveData data) || !CaseLibrary.TryFind(data.variantId, out story, out variant))
            return false;

        currentDay = data.day;
        questionsUsedToday = data.questionsUsedToday;
        accusationMade = false;
        unlocked.Clear();
        unlocked.AddRange(data.unlocked);

        conversationManager.RestoreCase(story, SaveSystem.RestoreState(variant, data),
            data.histories.ToDictionary(h => h.characterId, h => h.messages),
            data.emotions.ToDictionary(e => e.characterId, e => (Emotion)Enum.Parse(typeof(Emotion), e.emotion)));

        interrogationUI?.ContinueInterrogation(data, data.currentSuspect, conversationManager.Emotions);

        Debug.Log($"[GameManager] Partida continuada: {variant.id}, día {currentDay}");
        return true;
    }

    private void SelectCase()
    {
        string forcedId = Application.isEditor ? CaseIdFor(debugCase) : null;

        if (forcedId == null || !CaseLibrary.TryFind(forcedId, out story, out variant))
        {
            if (forcedId != null)
                Debug.LogWarning($"[GameManager] La variante {forcedId} aún no existe; se sortea entre las registradas.");

            var all = CaseLibrary.AllVariants().ToList();
            (story, variant) = all[UnityEngine.Random.Range(0, all.Count)];
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
        interrogationUI?.ShowCaseIntro(story.id, story.title, CaseBriefing.Format(story));
    }

    /// <summary>
    /// Llamado por la UI al pulsar "Empezar".
    /// </summary>
    public void BeginInterrogation()
    {
        SoundManager.PlayMusic(SoundCatalog.ForStory(story.id));
        RefreshSuspects();
        interrogationUI?.SetEvidenceOptions(DiscoveredClues());
        RefreshNotebook();
        UpdateGameState();
    }

    private bool requestInFlight;

    public async void AskQuestion(string characterId, string question, string shownClueId)
    {
        // Un doble clic o un segundo listener no deben lanzar dos peticiones a la vez
        if (requestInFlight)
            return;

        if (questionsUsedToday >= questionsPerDay)
        {
            interrogationUI?.ShowRequestFailed("No te quedan preguntas hoy.", question);
            return;
        }

        requestInFlight = true;
        notices.BeginDefer();
        bool answered = false;

        try
        {
            interrogationUI?.ShowWaiting(true);

            LLMResult result;

            try
            {
                ClueData shownClue = string.IsNullOrEmpty(shownClueId) ? null : variant.Clue(shownClueId);
                result = await conversationManager.AskSuspect(characterId, question, currentDay, shownClue);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                result = LLMResult.Fail("Error inesperado al procesar la pregunta.");
            }

            interrogationUI?.ShowWaiting(false);

            // Una petición fallida no gasta pregunta del día
            if (!result.Success)
            {
                interrogationUI?.ShowRequestFailed(result.ErrorMessage, question);
                return;
            }

            questionsUsedToday++;
            answered = true;
            // El estado va antes que la respuesta: marca la velocidad de escritura y el retrato
            interrogationUI?.SetEmotion(characterId, conversationManager.CurrentEmotion(characterId));
            RefreshNotebook();
            interrogationUI?.AddAnswer(characterId, story.Character(characterId).shortName, result.Text);
        }
        catch (Exception e)
        {
            // Cualquier fallo al mostrar la respuesta: nunca dejar la entrada bloqueada
            Debug.LogException(e);
            interrogationUI?.ShowRequestFailed("Error inesperado al mostrar la respuesta.", answered ? "" : question);
        }
        finally
        {
            requestInFlight = false;

            if (answered)
                notices.Flush();
            else
                notices.Discard();

            UpdateGameState();
            if (answered)
                SaveGame();
        }
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
        SaveGame();
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

        notices.Post(() =>
        {
            if (!string.IsNullOrEmpty(notice))
                interrogationUI?.ShowNotice(notice);

            interrogationUI?.ShowSuspectUnlocked(character.DisplayName);
            RefreshSuspects();
            RefreshNotebook();
        });
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

    /// <summary>
    /// Libreta: pistas, contradicciones y sospechosos desbloqueados con su estado.
    /// </summary>
    private void RefreshNotebook()
    {
        interrogationUI?.UpdateNotebook(Notebook.Format(story, State, unlocked,
            conversationManager.Emotions, conversationManager.DescribeContradiction));
    }

    private void OnClueRevealed(ClueData clue)
    {
        Debug.Log($"[GameManager] Pista: {clue.playerName}");
        notices.Post(() =>
        {
            interrogationUI?.ShowClueNotification(clue.playerName);
            interrogationUI?.SetEvidenceOptions(DiscoveredClues());
            RefreshNotebook();
        });
    }

    private void OnContradictionDetected(string text)
    {
        Debug.Log($"[GameManager] Contradicción: {text}");
        notices.Post(() =>
        {
            interrogationUI?.ShowContradictionNotification(text);
            RefreshNotebook();
        });
    }

    // ============================================
    // ACUSACIÓN
    // ============================================

    /// <summary>
    /// Se puede volver a interrogar desde la acusación mientras queden días y no se haya acusado.
    /// </summary>
    public bool CanCancelAccusation => !accusationMade && currentDay <= maxDays;

    private void ShowAccusationPanel()
    {
        interrogationUI?.ShowAccusationPanel(UnlockedSuspects(), CanCancelAccusation);
    }

    public void CancelAccusation()
    {
        if (CanCancelAccusation)
            interrogationUI?.ShowInterrogation();
    }

    public void MakeAccusation(string accusedId)
    {
        if (accusationMade)
            return;

        accusationMade = true;
        SaveSystem.Delete(); // La partida ha terminado
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

    /// <summary>
    /// Tras recargar la escena, el menú empieza directamente una partida nueva (Reiniciar).
    /// </summary>
    public static bool StartNewGameOnLoad;

    public void RestartGame()
    {
        StartNewGameOnLoad = true;
        SaveSystem.Delete();
        ReloadScene();
    }

    public void BackToMenu()
    {
        StartNewGameOnLoad = false;
        ReloadScene();
    }

    private static void ReloadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
