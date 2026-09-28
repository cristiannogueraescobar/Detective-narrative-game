using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Proveedor de LLM: recibe un system prompt + historial y devuelve la respuesta.
/// Nunca lanza excepciones por fallos de red o de formato: los devuelve en LLMResult.
/// </summary>
public interface ILLMProvider
{
    string DisplayName { get; }
    Task<LLMResult> SendAsync(string systemPrompt, IReadOnlyList<ChatMessage> history,
                              int maxTokens, float temperature);
}

public enum LLMProviderType
{
    Ollama,
    Anthropic
}

[Serializable]
public class ChatMessage
{
    public string role;
    public string content;
}

public struct LLMResult
{
    public bool Success;
    public string Text;
    public string ErrorMessage; // Mensaje ya redactado para mostrar al jugador

    public static LLMResult Ok(string text) => new LLMResult { Success = true, Text = text };
    public static LLMResult Fail(string errorMessage) => new LLMResult { Success = false, ErrorMessage = errorMessage };
}

/// <summary>
/// POST JSON compartido por los proveedores.
/// </summary>
public static class LLMHttp
{
    public struct Response
    {
        public UnityWebRequest.Result Result;
        public long StatusCode;
        public string Error;
        public string Body;

        public bool IsTimeout =>
            Result == UnityWebRequest.Result.ConnectionError &&
            Error != null && Error.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static async Task<Response> PostJsonAsync(string url, string jsonBody, int timeoutSeconds,
                                                     IDictionary<string, string> headers = null)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = timeoutSeconds;
            request.SetRequestHeader("Content-Type", "application/json");

            if (headers != null)
            {
                foreach (var header in headers)
                    request.SetRequestHeader(header.Key, header.Value);
            }

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            return new Response
            {
                Result = request.result,
                StatusCode = request.responseCode,
                Error = request.error,
                Body = request.downloadHandler?.text
            };
        }
    }

    /// <summary>
    /// JsonUtility.FromJson que devuelve null en vez de lanzar si el cuerpo no es JSON válido.
    /// </summary>
    public static T TryParse<T>(string json) where T : class
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonUtility.FromJson<T>(json);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
