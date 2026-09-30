using System;
using UnityEngine;

public enum ChatEntryKind
{
    Player,         // Pregunta del detective (burbuja a la derecha)
    Suspect,        // Respuesta del sospechoso (burbuja a la izquierda, con retrato)
    Day,            // Cambio de día con el parte de la mañana
    Notice,         // Aviso del juego
    Unlock,         // Nuevo sospechoso
    Contradiction,  // Contradicción detectada
    Error,          // Error (sin conexión, sin preguntas...)
    Legacy          // Texto de un guardado anterior a las burbujas
}

/// <summary>
/// Una línea del chat del interrogatorio. Se guarda tal cual en la partida.
/// </summary>
[Serializable]
public class ChatEntry
{
    public ChatEntryKind kind;
    public string speaker;   // Nombre del sospechoso (respuestas)
    public string text;
    public string time;      // Hora del juego ("10:40")
    public string evidence;  // Prueba mostrada con la pregunta
    public int day;          // Cambio de día

    public bool IsBubble => kind == ChatEntryKind.Player || kind == ChatEntryKind.Suspect;

    public static ChatEntry Player(string question, string evidence, string time)
    {
        return new ChatEntry { kind = ChatEntryKind.Player, text = question ?? "", evidence = evidence, time = time };
    }

    public static ChatEntry Suspect(string speaker, string answer, string time)
    {
        return new ChatEntry { kind = ChatEntryKind.Suspect, speaker = speaker, text = answer ?? "", time = time };
    }

    public static ChatEntry Day(int day, string morningReport)
    {
        return new ChatEntry { kind = ChatEntryKind.Day, day = day, text = morningReport ?? "" };
    }

    public static ChatEntry System(ChatEntryKind kind, string text)
    {
        return new ChatEntry { kind = kind, text = text ?? "" };
    }
}

/// <summary>
/// Hora del juego: la jornada del detective va de 09:00 a unas 19:00 y cada pregunta la hace avanzar.
/// </summary>
public static class GameClock
{
    public const int StartMinutes = 9 * 60;
    public const int WorkdayMinutes = 10 * 60;

    public static string TimeOf(int questionIndex, int questionsPerDay)
    {
        int perDay = Mathf.Max(1, questionsPerDay);
        int index = Mathf.Max(0, questionIndex);

        // Pasos regulares con un pequeño desfase para que no parezca un reloj de cuarzo
        int step = WorkdayMinutes / perDay;
        int minutes = StartMinutes + index * step + (index * 17) % 23;
        minutes = Mathf.Min(minutes, 23 * 60 + 59);
        return $"{minutes / 60:00}:{minutes % 60:00}";
    }
}

/// <summary>
/// Cuándo el chat baja solo: si el jugador ya estaba abajo. Si ha subido a leer, se respeta.
/// </summary>
public static class ChatScrollPolicy
{
    public const float BottomTolerance = 48f; // px de lienzo

    public static bool IsAtBottom(float verticalNormalizedPosition, float contentHeight, float viewportHeight)
    {
        float scrollable = contentHeight - viewportHeight;
        if (scrollable <= 0f)
            return true;
        return verticalNormalizedPosition * scrollable <= BottomTolerance;
    }
}
