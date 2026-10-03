/// <summary>
/// Texto de una contradicción, el mismo para todas: quien miente, su cita y la pista que la rompe. Lo usan la libreta,
/// el aviso del chat y la tarjeta "TUS PRUEBAS". Sesión C: no distingue culpable de inocente (antes siempre era el
/// culpable, y la primera contradicción resolvía el caso).
/// </summary>
public static class Contradictions
{
    public static string Describe(StoryData story, VariantData variant, ClueData clue)
    {
        string liar = clue.LiarIn(variant);
        string quote = variant.Role(liar).lieQuote;
        return $"La versión de {story.Character(liar).shortName} («{quote}») choca con: {clue.playerName}";
    }
}
