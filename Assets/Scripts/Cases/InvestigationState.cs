using System.Collections.Generic;
using System.Linq;

public enum Ending
{
    Good,         // Acierto con evidencia sólida
    Bittersweet,  // Acierto con evidencia parcial
    Insufficient, // Acierto sin pruebas: queda libre
    Bad           // Acusación equivocada
}

public class AccusationResult
{
    public Ending ending;
    public bool correct;
    public string accusedId;
    public int evidence;
    public int incriminatingFound;
    public int contradictions;
    public bool ignoredClearingClue; // Acusó a alguien cuya pista de descarte ya tenía
    public string innocentLieAbout;  // Acusó a un inocente que mentía: sobre qué (su secreto, no el crimen)
}

/// <summary>
/// Estado de la investigación de una partida: pistas descubiertas, pruebas mostradas,
/// contradicciones del culpable y cálculo del final.
/// </summary>
public class InvestigationState
{
    public const int GoodThreshold = 6; // Antes 5; fase 2 de las mentiras de inocentes (decisión de Cristian)
    public const int BittersweetThreshold = 3;
    public const int ContradictionWeight = 2;

    public VariantData Variant { get; }
    // Quién ha contado ya su mentira (sesión C: también los inocentes, sobre su secreto)
    private readonly HashSet<string> liesTold = new HashSet<string>();
    public bool CulpritToldLie => liesTold.Contains(Variant.culpritId);
    public IReadOnlyCollection<string> LiesTold => liesTold;

    private readonly List<string> discovered = new List<string>();
    private readonly List<string> contradictions = new List<string>();
    private readonly Dictionary<string, List<string>> shown = new Dictionary<string, List<string>>();

    public InvestigationState(VariantData variant)
    {
        Variant = variant;
    }

    public IReadOnlyList<string> DiscoveredClueIds => discovered;
    public IReadOnlyList<string> ContradictionClueIds => contradictions;

    public bool IsDiscovered(string clueId) => discovered.Contains(clueId);

    /// <summary>
    /// ¿Una pista ya encontrada descarta a este personaje? (libreta, rueda y final malo "tus propias pistas lo
    /// descartaban")
    /// </summary>
    public bool IsClearedByClue(string characterId)
    {
        return discovered.Any(id =>
        {
            ClueData clue = Variant.Clue(id);
            return clue != null && clue.kind == ClueKind.Clears && clue.clears == characterId;
        });
    }

    // Quién contó cada pista (con dos portadores no tiene por qué ser el principal). Sesión C: si la contaba Amparo,
    // la ficha de Daniel le hacía "recordar" una confesión que no hizo
    private readonly Dictionary<string, string> revealedBy = new Dictionary<string, string>();

    public bool Discover(string clueId, string by = null)
    {
        if (discovered.Contains(clueId))
            return false;

        discovered.Add(clueId);
        if (by != null)
            revealedBy[clueId] = by;
        return true;
    }

    /// <summary>Quién la contó; si no se sabe (guardados anteriores, pistas del día), su portador principal.</summary>
    public string RevealedBy(string clueId) =>
        revealedBy.TryGetValue(clueId, out string by) ? by : Variant.Clue(clueId)?.holder;

    /// <summary>La pista tal como la contó quien la contó (su resumen, su versión).</summary>
    public ClueData ClueAsFound(string clueId) => Variant.Clue(clueId)?.ForHolder(RevealedBy(clueId));

    public void RegisterShown(string characterId, string clueId)
    {
        if (!shown.TryGetValue(characterId, out List<string> list))
        {
            list = new List<string>();
            shown[characterId] = list;
        }

        if (!list.Contains(clueId))
            list.Add(clueId);
    }

    public IEnumerable<string> ShownTo(string characterId)
    {
        return shown.TryGetValue(characterId, out List<string> list) ? list : Enumerable.Empty<string>();
    }

    /// <summary>Ha contado su mentira; sin quién, el culpable.</summary>
    public void RegisterLieTold(string who = null)
    {
        liesTold.Add(who ?? Variant.culpritId);
    }

    public bool LieTold(string who) => liesTold.Contains(who);

    /// <summary>Las contradicciones que puntúan: solo las de la mentira del culpable.</summary>
    public int CulpritContradictions => contradictions.Count(id => Variant.Clue(id).LiarIn(Variant) == Variant.culpritId);

    /// <summary>
    /// Registra las contradicciones nuevas: pista que expone la mentira, ya descubierta, y el culpable
    /// ha contado su mentira o ha sido confrontado con esa pista. Devuelve solo las nuevas.
    /// </summary>
    public List<ClueData> UpdateContradictions()
    {
        var created = new List<ClueData>();

        foreach (ClueData clue in Variant.clues)
        {
            if (!clue.exposesLie || !IsDiscovered(clue.id) || contradictions.Contains(clue.id))
                continue;

            // Cada pista ⚡ rompe la mentira de alguien: cuenta si esa persona la ha dicho o si se le ha enseñado la pista
            string liar = clue.LiarIn(Variant);
            if (LieTold(liar) || ShownTo(liar).Contains(clue.id))
            {
                contradictions.Add(clue.id);
                created.Add(clue);
            }
        }

        return created;
    }

    public int IncriminatingFound => discovered.Count(id => Variant.Clue(id).kind == ClueKind.Incriminates);

    public int Evidence => IncriminatingFound + ContradictionWeight * CulpritContradictions;

    /// <summary>
    /// Evidencia máxima alcanzable sin que el culpable cuente nada.
    /// </summary>
    public static int MaxEvidenceWithoutCulprit(VariantData variant)
    {
        var fromOthers = variant.clues.Where(c => c.Holders.Any(h => h != variant.culpritId)).ToList();
        return fromOthers.Count(c => c.kind == ClueKind.Incriminates)
             + ContradictionWeight * fromOthers.Count(c => c.exposesLie && c.LiarIn(variant) == variant.culpritId);
    }

    public AccusationResult Accuse(string accusedId)
    {
        var result = new AccusationResult
        {
            accusedId = accusedId,
            correct = accusedId == Variant.culpritId,
            evidence = Evidence,
            incriminatingFound = IncriminatingFound,
            contradictions = CulpritContradictions
        };

        if (!result.correct)
        {
            result.ending = Ending.Bad;
            result.ignoredClearingClue = IsClearedByClue(accusedId);
            string lieAbout = Variant.roles.FirstOrDefault(r => r.characterId == accusedId)?.lieAbout;
            result.innocentLieAbout = string.IsNullOrEmpty(lieAbout) ? null : lieAbout;
        }
        else if (result.evidence >= GoodThreshold)
        {
            result.ending = Ending.Good;
        }
        else if (result.evidence >= BittersweetThreshold)
        {
            result.ending = Ending.Bittersweet;
        }
        else
        {
            result.ending = Ending.Insufficient;
        }

        return result;
    }
}
