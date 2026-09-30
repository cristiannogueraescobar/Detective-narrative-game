using UnityEngine;

/// <summary>
/// Textos de la interfaz en un solo sitio (la escena traía algunos con erratas o desactualizados).
/// </summary>
public static class GameTexts
{
    public const string GameName = "Detectives";
    public const string Tagline = "Interrogatorios · Tres casos";

    public const string InstructionsTitle = "CÓMO SE JUEGA";
    public const string AboutTitle = "ACERCA DE";
    public const string SettingsTitle = "AJUSTES";
    public const string NotebookTitle = "LIBRETA";
    public const string NoEvidence = "Mostrar prueba: ninguna";
    public const string SuggestionsHint = "Puedes empezar por…";
    public const string PlayAgain = "Jugar otra vez";
    public const string MainMenu = "Menú principal";

    public static string Instructions(Theme t)
    {
        string h(string text) => $"<color={Theme.Hex(t.accent)}><b>{text}</b></color>";
        return
            h("TU TRABAJO") + "\n" +
            "Eres el inspector del caso. Tienes siete días para descubrir quién lo hizo y reunir pruebas suficientes para que no se libre.\n\n" +
            h("INTERROGAR") + "\n" +
            "Elige a un sospechoso y escríbele como hablarías tú. Funcionan mejor las preguntas concretas: horas, lugares, objetos, quién estaba con quién. Para romper el hielo, toca una de las preguntas de ejemplo: se escribe sola y puedes retocarla. " +
            "Si algo no te cuadra, insiste: a veces la segunda pregunta abre la puerta.\n\n" +
            h("MOSTRAR UNA PRUEBA") + "\n" +
            "Con «Mostrar prueba» enseñas una pista de tu libreta junto a la pregunta. Enséñasela a quien creas que miente y mira cómo reacciona: " +
            "si choca con su versión, es una contradicción.\n\n" +
            h("DÍAS Y PREGUNTAS") + "\n" +
            "Cada día tienes cinco preguntas. Cuando las gastes, pulsa «Fin del día»: por la mañana llega un parte con novedades. " +
            "Desde el tercer día puedes hablar con todos.\n\n" +
            h("LA LIBRETA") + "\n" +
            "Las pistas, las contradicciones y cómo está cada sospechoso se apuntan solos en la libreta. Algunas pistas descartan a alguien: léelas bien.\n\n" +
            h("ACUSAR Y FINALES") + "\n" +
            "Puedes acusar cuando quieras; el séptimo día es obligatorio. Solo hay una oportunidad:\n" +
            "•  Culpable y pruebas sólidas: caso cerrado.\n" +
            "•  Culpable con pocas pruebas: cerrado con dudas.\n" +
            "•  Culpable sin pruebas: sale libre.\n" +
            "•  Persona equivocada: caso fallido.";
    }

    public static string About(Theme t, string version)
    {
        string h(string text) => $"<color={Theme.Hex(t.accent)}><b>{text}</b></color>";
        return
            $"<size=140%><b>{GameName.ToUpperInvariant()}</b></size>\n" +
            $"<color={Theme.Hex(t.textSecondary)}>{Tagline} · versión {version}</color>\n\n" +
            "Un juego de detectives para móvil en el que interrogas a sospechosos que responden con inteligencia artificial. " +
            "Tres historias, tres culpables posibles en cada una: nunca sabes cuál te va a tocar.\n\n" +
            h("CREACIÓN") + "\n" +
            "Cristian Alexandre Noguera\n\n" +
            h("HECHO CON") + "\n" +
            "Unity · TextMeshPro\n" +
            "Sospechosos: qwen2.5 (Ollama) o Claude (Anthropic)\n" +
            "Desarrollo con ayuda de Claude\n\n" +
            h("GRACIAS") + "\n" +
            "A quien juegue, pregunte y dude. © 2026";
    }

    /// <summary>
    /// Barra superior: día y preguntas que quedan hoy.
    /// </summary>
    public static string Hud(int day, int maxDays, int questionsUsed, int questionsPerDay)
    {
        int left = Mathf.Max(0, questionsPerDay - questionsUsed);
        string questions = left == 0 ? "SIN PREGUNTAS HOY" : left == 1 ? "QUEDA 1 PREGUNTA" : $"QUEDAN {left} PREGUNTAS";
        return $"DÍA {day} DE {maxDays}  ·  {questions}";
    }

    /// <summary>
    /// La misma barra con los números resaltados (color de acento).
    /// </summary>
    public static string HudRich(int day, int maxDays, int questionsUsed, int questionsPerDay, Theme t)
    {
        string n(object v) => $"<color={Theme.Hex(t.accent)}><b>{v}</b></color>";
        int left = Mathf.Max(0, questionsPerDay - questionsUsed);
        string questions = left == 0 ? $"<color={Theme.Hex(t.accent)}><b>SIN PREGUNTAS HOY</b></color>"
            : left == 1 ? $"QUEDA {n(1)} PREGUNTA" : $"QUEDAN {n(left)} PREGUNTAS";
        return $"DÍA {n(day)} DE {maxDays}  ·  {questions}";
    }

    /// <summary>
    /// Balance de la partida en el informe final.
    /// </summary>
    public static string CaseStats(int day, int cluesFound, int cluesTotal)
    {
        return $"Día {day} de la investigación · pistas encontradas: {cluesFound} de {cluesTotal}";
    }

    /// <summary>
    /// Botón de continuar con el caso y el día en que se quedó.
    /// </summary>
    public static string Continue(string caseTitle, int day)
    {
        return string.IsNullOrEmpty(caseTitle) ? "Continuar" : $"Continuar · {caseTitle}, día {day}";
    }

    public static string AccusationTitle(bool lastDay)
    {
        return lastDay ? "HORA DE ACUSAR" : "ACUSACIÓN";
    }

    public const string AccusationPrompt = "¿Quién lo hizo? Solo tienes una oportunidad: tus pistas y las contradicciones serán las pruebas.";
}
