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
}

/// <summary>
/// Estado de la investigación de una partida: pistas descubiertas, pruebas mostradas,
/// contradicciones del culpable y cálculo del final.
/// </summary>
public class InvestigationState
{
    public const int GoodThreshold = 5;
    public const int BittersweetThreshold = 3;
    public const int ContradictionWeight = 2;

    public VariantData Variant { get; }
    public bool CulpritToldLie { get; private set; }

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

    public bool Discover(string clueId)
    {
        if (discovered.Contains(clueId))
            return false;

        discovered.Add(clueId);
        return true;
    }

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

    public void RegisterLieTold()
    {
        CulpritToldLie = true;
    }

    /// <summary>
    /// Registra las contradicciones nuevas: pista que expone la mentira, ya descubierta, y el culpable
    /// ha contado su mentira o ha sido confrontado con esa pista. Devuelve solo las nuevas.
    /// </summary>
    public List<ClueData> UpdateContradictions()
    {
        var created = new List<ClueData>();
        List<string> shownToCulprit = ShownTo(Variant.culpritId).ToList();

        foreach (ClueData clue in Variant.clues)
        {
            if (!clue.exposesLie || !IsDiscovered(clue.id) || contradictions.Contains(clue.id))
                continue;

            if (CulpritToldLie || shownToCulprit.Contains(clue.id))
            {
                contradictions.Add(clue.id);
                created.Add(clue);
            }
        }

        return created;
    }

    public int IncriminatingFound => discovered.Count(id => Variant.Clue(id).kind == ClueKind.Incriminates);

    public int Evidence => IncriminatingFound + ContradictionWeight * contradictions.Count;

    /// <summary>
    /// Evidencia máxima alcanzable sin que el culpable cuente nada.
    /// </summary>
    public static int MaxEvidenceWithoutCulprit(VariantData variant)
    {
        var fromOthers = variant.clues.Where(c => c.holder != variant.culpritId).ToList();
        return fromOthers.Count(c => c.kind == ClueKind.Incriminates)
             + ContradictionWeight * fromOthers.Count(c => c.exposesLie);
    }

    public AccusationResult Accuse(string accusedId)
    {
        var result = new AccusationResult
        {
            accusedId = accusedId,
            correct = accusedId == Variant.culpritId,
            evidence = Evidence,
            incriminatingFound = IncriminatingFound,
            contradictions = contradictions.Count
        };

        if (!result.correct)
        {
            result.ending = Ending.Bad;
            result.ignoredClearingClue = discovered.Any(id =>
            {
                ClueData clue = Variant.Clue(id);
                return clue.kind == ClueKind.Clears && clue.clears == accusedId;
            });
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
