using UnityEngine;

/// <summary>
/// Textos de la interfaz en un solo sitio (la escena traía algunos con erratas o desactualizados).
/// </summary>
public static class GameTexts
{
    public const string GameName = "Detectives"; // Nombre visible del juego (menú, Acerca de); ver docs/REPORT-DIA3.md, "Decisiones para Cristian"
    public const string Tagline = "Interrogatorios · Tres casos";

    public const string InstructionsTitle = "CÓMO SE JUEGA";
    public const string AboutTitle = "ACERCA DE";
    public const string SettingsTitle = "AJUSTES";
    public const string NotebookTitle = "LIBRETA";
    public const string NoEvidence = "Mostrar prueba: ninguna";
    /// <summary>
    /// Opción "ninguna" del selector de pruebas; con pistas en la libreta recuerda que se pueden enseñar.
    /// </summary>
    public static string NoEvidenceWith(int available)
    {
        return available > 0 ? $"{NoEvidence} · {available} en la libreta" : NoEvidence;
    }

    /// <summary>
    /// Aviso antes de terminar el día con preguntas sin gastar (un toque por error no debe costar el día).
    /// </summary>
    public static string EndDayConfirm(int remaining)
    {
        return remaining == 1
            ? "¿Terminar el día? Te queda 1 pregunta y se perderá."
            : $"¿Terminar el día? Te quedan {remaining} preguntas y se perderán.";
    }

    /// <summary>
    /// Consejo en el parte de la mañana para quien lleva dos días sin ninguna pista (sin desvelar nada del caso).
    /// </summary>
    public static string StuckHint(int day, int cluesFound)
    {
        if (day < 3 || cluesFound > 0)
            return null;
        return "Aún no tienes ninguna pista. Pregunta cosas concretas: a qué hora, dónde, qué vio u oyó cada uno, " +
               "quién puede confirmarlo. Y habla con todos: cada uno sabe algo.";
    }

    /// <summary>
    /// Parte de la mañana: el del caso, el consejo si lo hay y el aviso de los últimos días, sin líneas vacías.
    /// </summary>
    public static string MorningReport(string caseReport, string hint, int day, int maxDays)
    {
        var parts = new System.Collections.Generic.List<string>();
        foreach (string part in new[] { caseReport, hint })
        {
            if (!string.IsNullOrWhiteSpace(part))
                parts.Add(part.Trim());
        }
        if (day == maxDays)
            parts.Add("Último día: al terminarlo tendrás que acusar a alguien.");
        else if (day == maxDays - 1)
            parts.Add("Quedan dos días de investigación.");
        return string.Join("\n", parts);
    }

    public const string NewGameConfirm = "¿Empezar un caso nuevo? Se perderá la investigación que tienes a medias.";
    public const string NewGameYes = "Empezar de nuevo";
    public const string NewGameNo = "Cancelar";

    public const string ThinkPrefix = "Piensas… ";

    /// <summary>
    /// Línea de gancho del expediente: quién era la víctima (la primera frase del parte, sin lo que pasó).
    /// </summary>
    public static string CaseHook(StoryData story)
    {
        string summary = story.victimSummary ?? "";
        int end = summary.IndexOf(". ", System.StringComparison.Ordinal);
        string first = end >= 0 ? summary.Substring(0, end + 1) : summary;
        return "Víctima: " + first.Trim();
    }

    /// <summary>
    /// Etiqueta del expediente en la selección de caso: el mejor final y el mejor rango conseguidos.
    /// </summary>
    public static string RecordLabel(Ending? best, string rank, Theme t)
    {
        if (best == null)
            return "SIN RESOLVER";
        string stamp = EndingStyle.For(best.Value, t).stamp;
        return string.IsNullOrEmpty(rank) ? stamp : $"{stamp} · {rank.ToUpperInvariant()}";
    }
    public const string KeyClueNone = "Prueba clave: ninguna (opcional)";

    /// <summary>
    /// Botón "Pensar" de la libreta con su coste en preguntas del día.
    /// </summary>
    public static string ThinkLabel(int cost)
    {
        return cost <= 0 ? "Pensar" : cost == 1 ? "Pensar (1 pregunta)" : $"Pensar ({cost} preguntas)";
    }

    public const string EndDayYes = "Terminar el día";
    public const string EndDayNo = "Seguir preguntando";

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
            "Cada día tienes cinco preguntas (siete en Historia, cuatro en Veterano; se elige en Ajustes). Cuando las gastes, pulsa «Fin del día»: por la mañana llega un parte con novedades. " +
            "No todos están disponibles al principio: aparecen cuando alguien los menciona, cuando preguntas por lo que ellos saben (la vecina, la curva, los caballos…) o cuando la policía los trae.\n\n" +
            h("LA LIBRETA") + "\n" +
            "Las pistas, las contradicciones y cómo está cada sospechoso se apuntan solos en la libreta. Algunas pistas descartan a alguien: léelas bien. " +
            "Toca una pista para enseñarla en tu próxima pregunta, o el nombre de un sospechoso para ir a interrogarle.\n\n" +
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

    /// <summary>
    /// Lo que el jugador lleva a la acusación (sin decir qué pistas incriminan a quién).
    /// </summary>
    public static string AccusationSummary(int clues, int contradictions)
    {
        if (clues <= 0 && contradictions <= 0)
            return "Tu libreta está vacía: acusar ahora es una apuesta.";
        string c = clues == 1 ? "1 pista" : $"{clues} pistas";
        string x = contradictions == 0 ? "ninguna contradicción"
                 : contradictions == 1 ? "1 contradicción" : $"{contradictions} contradicciones";
        return $"En tu libreta: {c} y {x}.";
    }
}
