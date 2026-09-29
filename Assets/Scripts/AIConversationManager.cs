using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Orquesta el interrogatorio: ficha del personaje → LLM → análisis de la respuesta del portador.
/// El contenido de los casos vive en Cases/; aquí solo hay flujo y eventos.
/// </summary>
public class AIConversationManager : MonoBehaviour
{
    public const int DefaultMaxTokens = 250;
    public const float DefaultTemperature = 0.6f;
    public const int MaxHistoryMessages = 16; // Últimos 8 intercambios por sospechoso

    [Header("Proveedor LLM")]
    [SerializeField] private LLMProviderType provider = LLMProviderType.Ollama;
    [SerializeField] private OllamaSettings ollamaSettings = new OllamaSettings();
    [SerializeField] private AnthropicSettings anthropicSettings = new AnthropicSettings();

    [Header("Response Settings")]
    [SerializeField] private int maxTokens = DefaultMaxTokens;
    [SerializeField][Range(0f, 1f)] private float temperature = DefaultTemperature;

    [Header("Debug (solo editor)")]
    [Tooltip("Registra en consola cada evaluación de pista: anclas buscadas y encontradas")]
    [SerializeField] private bool debugLogClueEvaluation = true;

    // EVENTOS
    public event Action<ClueData> OnClueRevealed;
    public event Action<string> OnContradictionDetected; // Texto listo para mostrar
    public event Action<string> OnCharacterMentioned;    // Id del personaje mencionado
    public event Action<string, Emotion> OnEmotionChanged; // (personaje, estado leído de la etiqueta)

    private ILLMProvider llmProvider;
    private readonly Dictionary<string, List<ChatMessage>> conversationHistory =
        new Dictionary<string, List<ChatMessage>>();
    private readonly Dictionary<string, Emotion> emotions = new Dictionary<string, Emotion>();

    public StoryData Story { get; private set; }
    public InvestigationState State { get; private set; }

    private void Awake()
    {
        llmProvider = CreateProvider();
        Debug.Log($"[AIConversation] Proveedor: {llmProvider.DisplayName}");
    }

    private void Start()
    {
        // En segundo plano mientras el jugador está en el menú; no se espera ni bloquea la UI
        _ = llmProvider.WarmUpAsync();
    }

    private ILLMProvider CreateProvider()
    {
        switch (provider)
        {
            case LLMProviderType.Anthropic:
                return new AnthropicProvider(anthropicSettings);
            default:
                return new OllamaProvider(ollamaSettings);
        }
    }

    /// <summary>
    /// Sustituye el proveedor configurado (tests con un proveedor falso).
    /// </summary>
    public void UseProvider(ILLMProvider replacement)
    {
        llmProvider = replacement;
    }

    public void StartCase(StoryData story, VariantData variant)
    {
        Story = story;
        State = new InvestigationState(variant);
        conversationHistory.Clear();
        emotions.Clear();
    }

    /// <summary>
    /// Pregunta a un sospechoso, opcionalmente mostrándole una prueba ya descubierta.
    /// Si la petición falla, el historial y el estado quedan como estaban.
    /// </summary>
    public async Task<LLMResult> AskSuspect(string characterId, string question, int day, ClueData shownClue)
    {
        if (State == null)
            return LLMResult.Fail("No hay ningún caso en curso.");

        if (!conversationHistory.TryGetValue(characterId, out List<ChatMessage> history))
        {
            history = new List<ChatMessage>();
            conversationHistory[characterId] = history;
        }

        // La prueba se añade a "ya mostradas" solo si la petición sale bien; para esta ficha ya cuenta
        List<ClueData> shown = State.ShownTo(characterId).Select(State.Variant.Clue).ToList();
        if (shownClue != null && !shown.Contains(shownClue))
            shown.Add(shownClue);

        string systemPrompt = PromptBuilder.Build(Story, State.Variant, characterId, day, shown,
            TurnAnalyzer.RevealedSecrets(State, characterId));

        history.Add(new ChatMessage { role = "user", content = TurnAnalyzer.BuildUserMessage(question, shownClue) });

        LLMResult result = await llmProvider.SendAsync(systemPrompt,
            ConversationWindow.Last(history, MaxHistoryMessages), maxTokens, temperature);

        if (!result.Success)
        {
            // Quitar la pregunta para no dejar dos mensajes "user" seguidos al reintentar
            history.RemoveAt(history.Count - 1);
            Debug.LogWarning($"[AIConversation] Petición fallida ({llmProvider.DisplayName}): {result.ErrorMessage}");
            return result;
        }

        // El historial guarda la respuesta con su etiqueta para que el modelo mantenga el formato;
        // el análisis y la pantalla usan el texto limpio
        history.Add(new ChatMessage { role = "assistant", content = result.Text });
        EmotionParse parsed = EmotionParser.Parse(result.Text);
        string clean = string.IsNullOrWhiteSpace(parsed.text) ? "…" : parsed.text;

        if (Application.isEditor && debugLogClueEvaluation && !parsed.wellFormed)
            Debug.Log($"[Estado] {characterId}: etiqueta ausente o mal formada");

        TurnOutcome outcome = TurnAnalyzer.Analyze(Story, State, characterId, clean, shownClue);
        LogEvaluation(characterId, outcome);

        if (parsed.emotion.HasValue)
        {
            emotions[characterId] = parsed.emotion.Value;
            OnEmotionChanged?.Invoke(characterId, parsed.emotion.Value);
        }

        foreach (ClueData clue in outcome.newClues)
            OnClueRevealed?.Invoke(clue);

        foreach (string mentioned in outcome.mentionedCharacters)
            OnCharacterMentioned?.Invoke(mentioned);

        foreach (ClueData clue in outcome.newContradictions)
            OnContradictionDetected?.Invoke(DescribeContradiction(clue));

        return LLMResult.Ok(clean);
    }

    /// <summary>
    /// Último estado emocional de un sospechoso (tranquilo hasta que el modelo diga otra cosa).
    /// </summary>
    public IReadOnlyDictionary<string, Emotion> Emotions => emotions;

    /// <summary>
    /// Historial completo por sospechoso (con etiquetas de estado), para guardar la partida.
    /// </summary>
    public IReadOnlyDictionary<string, List<ChatMessage>> Histories => conversationHistory;

    /// <summary>
    /// Continúa una partida guardada: estado, historiales y estados emocionales.
    /// </summary>
    public void RestoreCase(StoryData story, InvestigationState state,
                            IDictionary<string, List<ChatMessage>> histories, IDictionary<string, Emotion> savedEmotions)
    {
        Story = story;
        State = state;
        conversationHistory.Clear();
        emotions.Clear();

        foreach (var pair in histories)
            conversationHistory[pair.Key] = new List<ChatMessage>(pair.Value);
        foreach (var pair in savedEmotions)
            emotions[pair.Key] = pair.Value;
    }

    public Emotion CurrentEmotion(string characterId)
    {
        return emotions.TryGetValue(characterId, out Emotion emotion) ? emotion : Emotion.Tranquilo;
    }

    public string DescribeContradiction(ClueData clue)
    {
        CharacterData culprit = Story.Character(State.Variant.culpritId);
        string quote = State.Variant.Role(culprit.id).lieQuote;
        return $"La versión de {culprit.shortName} («{quote}») choca con: {clue.playerName}";
    }

    private void LogEvaluation(string characterId, TurnOutcome outcome)
    {
        if (!Application.isEditor || !debugLogClueEvaluation)
            return;

        string holder = Story.Character(characterId).shortName;

        foreach (var (clue, trace) in outcome.traces)
            Debug.Log($"[Pista] {clue.id} ({holder}): {trace} ⇒ {(trace.Matched ? "DESCUBIERTA" : "no")}");

        if (outcome.lieTrace != null)
            Debug.Log($"[Mentira] {holder}: {outcome.lieTrace} ⇒ {(outcome.lieTold ? "DICHA" : "no")}");

        if (outcome.traces.Count == 0)
            Debug.Log($"[Pista] {holder} no tiene pistas pendientes");
    }
}
