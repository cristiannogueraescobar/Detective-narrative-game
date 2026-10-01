using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class OllamaSettings
{
    public string baseUrl = "http://localhost:11434";
    public string model = "qwen2.5:7b-instruct";
    [Tooltip("La primera petición carga el modelo en memoria y puede tardar")]
    public int timeoutSeconds = 180;
    [Tooltip("Cargar el modelo en segundo plano al arrancar la escena")]
    public bool preloadOnStart = true;
    [Tooltip("Tiempo que Ollama mantiene el modelo en memoria tras cada petición (\"60m\", \"2h\", \"-1m\" = siempre). Un número sin unidad se interpreta en minutos")]
    public string keepAlive = DefaultKeepAlive;
    [Tooltip("Tamaño de contexto (num_ctx). Debe ser igual en la precarga y en las preguntas, o Ollama recarga el modelo")]
    public int numCtx = DefaultNumCtx;

    public const string DefaultKeepAlive = "60m";
    public const int DefaultNumCtx = 8192;
}

/// <summary>
/// Ollama local vía /api/chat, sin streaming.
/// </summary>
public class OllamaProvider : ILLMProvider
{
    private readonly OllamaSettings settings;

    /// <summary>
    /// Solo para tests: la suite PlayMode abre la escena con el proveedor real antes de cambiarlo por uno falso, y la
    /// precarga cargaba el modelo en la GPU (sesión C). El juego nunca lo cambia: sigue precargando con preloadOnStart.
    /// </summary>
    public static bool PreloadDisabled;

    /// <summary>Precargas enviadas a Ollama: para los tests.</summary>
    public static int PreloadRequests { get; private set; }

    public OllamaProvider(OllamaSettings settings)
    {
        this.settings = settings;
    }

    public string DisplayName => $"Ollama ({settings.model})";

    private string ChatUrl => settings.baseUrl.TrimEnd('/') + "/api/chat";

    // Número sin unidad ("60", "-1", "1.5") y duración de Go válida ("60m", "1h30m", "-1m")
    private static readonly Regex BareNumber = new Regex(@"^-?\d+(\.\d+)?$");
    private static readonly Regex GoDuration = new Regex(@"^-?(\d+(\.\d+)?(ns|us|µs|ms|s|m|h))+$");

    private string lastWarnedKeepAlive;

    /// <summary>
    /// keepAlive del Inspector listo para enviar. Ollama rechaza con HTTP 400 un texto sin unidad como "-1".
    /// </summary>
    private string KeepAlive
    {
        get
        {
            string value = NormalizeKeepAlive(settings.keepAlive, out string warning);

            // Avisar una vez por valor, no en cada petición
            if (warning != null && settings.keepAlive != lastWarnedKeepAlive)
            {
                lastWarnedKeepAlive = settings.keepAlive;
                Debug.LogWarning($"[Ollama] keepAlive: {warning}");
            }

            return value;
        }
    }

    /// <summary>
    /// Vacío → valor por defecto; número sin unidad → minutos ("-1" → "-1m");
    /// duración válida → sin cambios; cualquier otra cosa → valor por defecto.
    /// </summary>
    public static string NormalizeKeepAlive(string raw, out string warning)
    {
        warning = null;
        string value = raw?.Trim();

        if (string.IsNullOrEmpty(value))
            return OllamaSettings.DefaultKeepAlive;

        if (BareNumber.IsMatch(value))
        {
            warning = $"'{raw}' no tiene unidad; se usa '{value}m'.";
            return value + "m";
        }

        if (GoDuration.IsMatch(value))
            return value;

        warning = $"'{raw}' no es una duración válida (ej. \"60m\", \"2h\", \"-1m\"); se usa '{OllamaSettings.DefaultKeepAlive}'.";
        return OllamaSettings.DefaultKeepAlive;
    }

    /// <summary>
    /// Con "messages" vacío, Ollama solo carga el modelo en memoria (done_reason: "load") sin generar nada.
    /// Si falla (Ollama no arrancado, etc.) solo se registra: el jugador verá el aviso normal al preguntar.
    /// </summary>
    public async Task WarmUpAsync()
    {
        if (!settings.preloadOnStart || PreloadDisabled)
            return;

        try
        {
            var body = new OllamaChatRequest
            {
                model = settings.model,
                messages = new ChatMessage[0],
                stream = false,
                keep_alive = KeepAlive,
                options = new OllamaOptions { num_ctx = settings.numCtx }
            };

            float start = Time.realtimeSinceStartup;
            PreloadRequests++;
            var response = await LLMHttp.PostJsonAsync(ChatUrl, JsonUtility.ToJson(body), settings.timeoutSeconds);

            if (response.Result == UnityWebRequest.Result.Success)
                Debug.Log($"[Ollama] Modelo '{settings.model}' precargado en {Time.realtimeSinceStartup - start:F1} s (keep_alive {KeepAlive})");
            else
                Debug.Log($"[Ollama] Precarga no completada ({response.StatusCode} {response.Error}). Se reintentará con la primera pregunta.");
        }
        catch (Exception e)
        {
            Debug.Log($"[Ollama] Precarga no completada: {e.Message}");
        }
    }

    public async Task<LLMResult> SendAsync(string systemPrompt, IReadOnlyList<ChatMessage> history,
                                           int maxTokens, float temperature)
    {
        var messages = new List<ChatMessage>(history.Count + 1);
        messages.Add(new ChatMessage { role = "system", content = systemPrompt });
        messages.AddRange(history);

        var body = new OllamaChatRequest
        {
            model = settings.model,
            messages = messages.ToArray(),
            stream = false,
            keep_alive = KeepAlive,
            options = new OllamaOptions { temperature = temperature, num_predict = maxTokens, num_ctx = settings.numCtx }
        };

        var response = await LLMHttp.PostJsonAsync(ChatUrl, JsonUtility.ToJson(body), settings.timeoutSeconds);
        var parsed = LLMHttp.TryParse<OllamaChatResponse>(response.Body);

        if (response.IsTimeout)
        {
            return LLMResult.Fail($"Ollama no respondió en {settings.timeoutSeconds} s. " +
                                  "Si es la primera pregunta, el modelo puede estar cargándose: inténtalo de nuevo.");
        }

        if (response.Result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogWarning($"[Ollama] Error de conexión: {response.Error}");
            return LLMResult.Fail($"No se pudo conectar con Ollama en {settings.baseUrl}. " +
                                  "¿Está arrancado? (ejecuta 'ollama serve')");
        }

        string apiError = parsed?.error;

        if (response.StatusCode == 404 && !string.IsNullOrEmpty(apiError) &&
            apiError.IndexOf("not found", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return LLMResult.Fail($"El modelo '{settings.model}' no está descargado. " +
                                  $"Ejecuta 'ollama pull {settings.model}'.");
        }

        if (response.Result != UnityWebRequest.Result.Success || !string.IsNullOrEmpty(apiError))
        {
            Debug.LogWarning($"[Ollama] HTTP {response.StatusCode} ({response.Error}). Cuerpo: {response.Body}");
            return LLMResult.Fail($"Ollama devolvió un error: {apiError ?? response.Error}");
        }

        string text = parsed?.message?.content;

        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.LogWarning($"[Ollama] Respuesta sin texto. Cuerpo: {response.Body}");
            return LLMResult.Fail("Ollama devolvió una respuesta vacía o con un formato inesperado.");
        }

        return LLMResult.Ok(text.Trim());
    }

    // ============================================
    // DTOs para JsonUtility
    // ============================================

    [Serializable]
    private class OllamaChatRequest
    {
        public string model;
        public ChatMessage[] messages;
        public bool stream;
        public string keep_alive;
        public OllamaOptions options;
    }

    [Serializable]
    private class OllamaOptions
    {
        public float temperature;
        public int num_predict;
        public int num_ctx;
    }

    [Serializable]
    private class OllamaChatResponse
    {
        public ChatMessage message;
        public string error;
    }
}
