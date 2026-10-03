using System.Collections.Generic;
using System.Linq;

/// <summary>
/// La versión de cada personaje ("Dice: «…»" en la libreta) y su mentira. Decisión de Cristian (fase 1 de las
/// mentiras de inocentes): una mentira cuenta como dicha en cuanto su versión aparece en la libreta, igual para el
/// culpable y los inocentes. Lo que el jugador lee es lo que el personaje ha declarado; antes había que pillarle
/// diciéndola en el chat y la libreta la enseñaba sin que contase. La libreta y el registro usan esta misma regla.
/// </summary>
public static class HeardVersions
{
    /// <summary>
    /// La versión está en la libreta: ya ha contestado algo, y no nombra a nadie que el jugador aún no conoce.
    /// </summary>
    public static bool Heard(StoryData story, CharacterRole role, IEnumerable<string> interviewed, IEnumerable<string> unlocked)
    {
        if (role == null || string.IsNullOrEmpty(role.version) || interviewed == null || !interviewed.Contains(role.characterId))
            return false;
        var available = new HashSet<string>(unlocked ?? Enumerable.Empty<string>());
        return !story.cast.Any(o => !available.Contains(o.id) && !string.IsNullOrEmpty(o.shortName) && role.version.Contains(o.shortName));
    }

    /// <summary>
    /// Da por dicha la mentira de quien miente y ya tiene su versión en la libreta; devuelve las contradicciones nuevas.
    /// </summary>
    public static List<ClueData> RegisterLies(StoryData story, InvestigationState state, IEnumerable<string> interviewed, IEnumerable<string> unlocked)
    {
        List<string> asked = interviewed?.ToList() ?? new List<string>();
        List<string> available = unlocked?.ToList() ?? new List<string>();
        foreach (CharacterRole role in state.Variant.roles)
        {
            bool lies = !string.IsNullOrEmpty(role.lieQuote);
            if (lies && !state.LieTold(role.characterId) && Heard(story, role, asked, available))
                state.RegisterLieTold(role.characterId);
        }
        return state.UpdateContradictions();
    }
}
