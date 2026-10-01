using System.Collections.Generic;
using System.Linq;

// ============================================
// DATOS DE HISTORIAS, VARIANTES, PERSONAJES Y PISTAS
// Todo el contenido narrativo vive en los ficheros StoryN*.cs; aquí solo están los tipos.
// ============================================

public enum ClueKind
{
    Incriminates, // Apunta al culpable: suma evidencia
    Clears,       // Descarta a un inocente (campo 'clears')
    Context       // Ambientación: no puntúa
}

/// <summary>
/// Un hecho concreto que solo conoce su portador. Se descubre cuando la RESPUESTA del portador
/// contiene al menos un ancla de cada grupo.
/// </summary>
public class ClueData
{
    public string id;                 // Interno, nunca se muestra al jugador
    public string playerName;         // Nombre neutro visible en la libreta
    public string summary;            // Texto de la libreta y de la confrontación
    public string holder;             // Id del personaje que la conoce
    public string topic;              // "Si te preguntan por {topic}..."
    public string fact;               // "...cuenta que {fact}"
    public bool isSecret;             // Va en "LO QUE OCULTAS" en vez de en los hechos que cuenta
    public string[][] anchors;        // AND entre grupos, OR dentro de cada grupo
    public ClueKind kind;
    public string clears;             // Id del personaje descartado (solo kind == Clears)
    public bool exposesLie;           // Choca con la mentira del culpable
    public string[] calibrationQuestions; // Turnos separados por "||"
    public string[] sampleHits;       // Respuestas de ejemplo que DEBEN detectarse
    public string[] sampleMisses;     // Respuestas de ejemplo que NO deben detectarse
}

/// <summary>
/// Personaje de una historia: lo que no cambia entre variantes.
/// </summary>
public class CharacterData
{
    public string id;             // "padre", "madre"...
    public string name;           // "Daniel Mendoza"
    public string shortName;      // "Daniel"
    public string roleLabel;      // "padre"
    public string portraitKey;    // Clave del retrato en InterrogationUI ("Padre", "Vecina"...)
    public string artId;          // Nombre de los retratos por estado: Assets/Art/Portraits/<artId>_<estado>.png
    public int heightCm = 170;    // Altura real: la rueda de reconocimiento los mide contra la pared (Sesión A)
    public string identity;       // "Eres Daniel Mendoza, 48 años, abogado. ..."
    public string speech;         // Cómo habla
    public string speechExample;  // Una frase de ejemplo
    public bool startsUnlocked;
    public string[] mentionAliases; // Palabras que, en una respuesta, lo desbloquean

    // Profundidad (Resources/Stories/StoriesDatabase.json)
    public string personality;    // Carácter
    public string wound;          // Herida o presión vital
    public string tellsLying;     // Cómo se le nota al mentir
    public string pressureArc;    // Cómo reacciona según aprieta el interrogatorio

    public string DisplayName => $"{shortName} ({roleLabel})";
}

/// <summary>
/// Papel de un personaje en una variante concreta.
/// </summary>
public class CharacterRole
{
    public string characterId;
    public string[] knowledge;    // Hechos que sabe y cuenta sin problema (no son pistas)
    public string version;        // Lo que dice que hizo
    public string secret;         // Secreto menor (inocente) o la verdad del crimen (culpable)
    public string admitsWhen;     // Condición para admitir el secreto
    public string nervousAbout;
    public string ifAccused;
    public string doesNotKnow;

    // Solo el culpable
    public string lieQuote;       // Cita corta de su mentira, para el texto de la contradicción
    public string[][] lieAnchors; // Detecta que ha contado su mentira
    public string versionB;       // Verdad parcial cuando le muestran una prueba que le contradice
    public string lieStrategy;    // Culpable: cómo sostiene la mentira (StoriesDatabase.json)
    public string[] admissionSamples; // Frases de la versión B que NO deben contar como la mentira
}

public class VariantData
{
    public string id;             // "1A"
    public string culpritId;
    public string epilogue;       // Lo que pasó de verdad (pantalla final)
    public string[] morningReports; // Índice = día - 1; el día 1 va vacío
    public List<CharacterRole> roles = new List<CharacterRole>();
    public List<ClueData> clues = new List<ClueData>();
    public List<TimelineEvent> timeline = new List<TimelineEvent>(); // Lo que pasó de verdad, hora a hora (validador)

    public CharacterRole Role(string characterId) => roles.First(r => r.characterId == characterId);
    public ClueData Clue(string clueId) => clues.First(c => c.id == clueId);
}

/// <summary>
/// Cómo aparece un personaje bloqueado: por un tema de las preguntas del jugador o, si nadie lo trae, con un parte
/// de la mañana el día indicado (NaturalUnlocks).
/// </summary>
[System.Serializable]
public class UnlockTrigger
{
    public string id;
    public string[] playerStems;
    public int fallbackDay;
    public string fallbackText;
}

/// <summary>
/// Un momento de la línea temporal real de una variante: quién estaba dónde y haciendo qué. "~22:30" = aproximado
/// (no cuenta para "dos sitios a la vez"). Solo la usa el validador narrativo; no llega a las fichas.
/// </summary>
public class TimelineEvent
{
    public string time;
    public bool approximate;
    public string who;
    public string where;
    public string what;

    public TimelineEvent(string time, string who, string where, string what)
    {
        approximate = time.StartsWith("~");
        this.time = time.TrimStart('~');
        this.who = who;
        this.where = where;
        this.what = what;
    }
}

public class StoryData
{
    public string id;             // "1"
    public string title;
    public string victim;         // Nombre de pila de la víctima
    public string intro;          // Texto de la pantalla de introducción
    public string place;          // Parte del caso: lugar y momento
    public string victimSummary;  // Parte del caso: quién es la víctima
    public string situation;      // Parte del caso: qué se sabe al empezar
    public string caseBrief;      // "EL CASO" en las fichas de personaje
    public List<CharacterData> cast = new List<CharacterData>();
    public List<VariantData> variants = new List<VariantData>();
    public List<UnlockTrigger> unlockTriggers = new List<UnlockTrigger>(); // Desbloqueos naturales (StoriesDatabase.json)

    public CharacterData Character(string characterId) => cast.First(c => c.id == characterId);
}

/// <summary>
/// Lo que la UI necesita para mostrar a un sospechoso.
/// </summary>
public struct SuspectView
{
    public string id;
    public string displayName;
    public string shortName;
    public string portraitKey;
    public string artId;
    public int heightCm;

    public static SuspectView From(CharacterData character) => new SuspectView
    {
        id = character.id,
        displayName = character.DisplayName,
        shortName = character.shortName,
        portraitKey = character.portraitKey,
        artId = character.artId,
        heightCm = character.heightCm
    };
}
