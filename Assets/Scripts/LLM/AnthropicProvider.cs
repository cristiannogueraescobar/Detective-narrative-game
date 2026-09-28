using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class AnthropicSettings
{
    public string model = "claude-sonnet-4-20250514";
    public int timeoutSeconds = 60;
}

/// <summary>
/// API de Mensajes de Anthropic.
/// La key se lee de la variable de entorno ANTHROPIC_API_KEY o, si no existe, del archivo
/// anthropic_api_key.txt en la raíz del proyecto (junto al .exe en una build). Ese archivo está en .gitignore.
/// </summary>
public class AnthropicProvider : ILLMProvider
{
    private const string API_URL = "https://api.anthropic.com/v1/messages";
    private const string API_VERSION = "2023-06-01";
    public const string KEY_ENV_VAR = "ANTHROPIC_API_KEY";
    public const string KEY_FILE_NAME = "anthropic_api_key.txt";

    private readonly AnthropicSettings settings;
    private readonly string apiKey;

    public AnthropicProvider(AnthropicSettings settings)
    {
        this.settings = settings;
        apiKey = LoadApiKey();
    }

    public string DisplayName => $"Anthropic ({settings.model})";

    private static string KeyFilePath => Path.GetFullPath(Path.Combine(Application.dataPath, "..", KEY_FILE_NAME));

    private static string LoadApiKey()
    {
        string key = Environment.GetEnvironmentVariable(KEY_ENV_VAR);

        if (string.IsNullOrWhiteSpace(key) && File.Exists(KeyFilePath))
        {
            key = File.ReadAllText(KeyFilePath);
        }

        return string.IsNullOrWhiteSpace(key) ? null : key.Trim();
    }

    public async Task<LLMResult> SendAsync(string systemPrompt, IReadOnlyList<ChatMessage> history,
                                           int maxTokens, float temperature)
    {
        if (apiKey == null)
        {
            return LLMResult.Fail($"Falta la API key de Anthropic. Define {KEY_ENV_VAR} " +
                                  $"o crea {KEY_FILE_NAME} en la raíz del proyecto.");
        }

        var body = new AnthropicRequest
        {
            model = settings.model,
            max_tokens = maxTokens,
            temperature = temperature,
            system = systemPrompt,
            messages = new List<ChatMessage>(history).ToArray()
        };

        var headers = new Dictionary<string, string>
        {
            { "x-api-key", apiKey },
            { "anthropic-version", API_VERSION }
        };

        var response = await LLMHttp.PostJsonAsync(API_URL, JsonUtility.ToJson(body), settings.timeoutSeconds, headers);
        var parsed = LLMHttp.TryParse<AnthropicResponse>(response.Body);

        if (response.IsTimeout)
        {
            return LLMResult.Fail($"Anthropic no respondió en {settings.timeoutSeconds} s. Inténtalo de nuevo.");
        }

        if (response.Result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogWarning($"[Anthropic] Error de conexión: {response.Error}");
            return LLMResult.Fail("No se pudo conectar con la API de Anthropic. Comprueba tu conexión a internet.");
        }

        // JsonUtility siempre instancia las clases anidadas, así que se comprueba el mensaje, no el objeto
        string apiError = parsed?.error?.message;

        if (response.Result != UnityWebRequest.Result.Success || !string.IsNullOrEmpty(apiError))
        {
            Debug.LogWarning($"[Anthropic] HTTP {response.StatusCode} ({response.Error}). Cuerpo: {response.Body}");

            switch (response.StatusCode)
            {
                case 401:
                    return LLMResult.Fail("La API key de Anthropic no es válida.");
                case 429:
                    return LLMResult.Fail("Límite de peticiones de Anthropic alcanzado. Espera un momento.");
                case 529:
                    return LLMResult.Fail("La API de Anthropic está sobrecargada. Inténtalo en un momento.");
                default:
                    return LLMResult.Fail($"Anthropic devolvió un error: {apiError ?? response.Error}");
            }
        }

        string text = ExtractText(parsed);

        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.LogWarning($"[Anthropic] Respuesta sin texto. Cuerpo: {response.Body}");
            return LLMResult.Fail("Anthropic devolvió una respuesta vacía o con un formato inesperado.");
        }

        return LLMResult.Ok(text.Trim());
    }

    private static string ExtractText(AnthropicResponse parsed)
    {
        if (parsed?.content == null)
            return null;

        var text = new StringBuilder();

        foreach (var block in parsed.content)
        {
            if (block != null && block.type == "text")
                text.Append(block.text);
        }

        return text.ToString();
    }

    // ============================================
    // DTOs para JsonUtility
    // ============================================

    [Serializable]
    private class AnthropicRequest
    {
        public string model;
        public int max_tokens;
        public float temperature;
        public string system;
        public ChatMessage[] messages;
    }

    [Serializable]
    private class AnthropicResponse
    {
        public ContentBlock[] content;
        public ApiError error;
    }

    [Serializable]
    private class ContentBlock
    {
        public string type;
        public string text;
    }

    [Serializable]
    private class ApiError
    {
        public string type;
        public string message;
    }
}
