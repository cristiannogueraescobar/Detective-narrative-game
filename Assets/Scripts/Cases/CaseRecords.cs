/// <summary>
/// El mejor final conseguido en cada historia (se guarda con los ajustes). Lo enseña la selección de caso.
/// </summary>
public static class CaseRecords
{
    private const string Prefix = "casos.mejorFinal.";

    // Orden de mejor a peor: bueno > agridulce > insuficiente > malo
    public static int Rank(Ending ending)
    {
        switch (ending)
        {
            case Ending.Good: return 4;
            case Ending.Bittersweet: return 3;
            case Ending.Insufficient: return 2;
            default: return 1;
        }
    }

    public static Ending? Best(string storyId)
    {
        int rank = (int)GameSettings.GetValue(Prefix + storyId, 0f);
        switch (rank)
        {
            case 4: return Ending.Good;
            case 3: return Ending.Bittersweet;
            case 2: return Ending.Insufficient;
            case 1: return Ending.Bad;
            default: return null;
        }
    }

    /// <summary>
    /// Mejor rango de detective en una historia (null si aún no se ha cerrado).
    /// </summary>
    public static string BestRank(string storyId)
    {
        int saved = (int)GameSettings.GetValue(RankKey(storyId), -1f);
        return saved >= 0 && saved < DetectiveRank.Ranks.Length ? DetectiveRank.Ranks[saved] : null;
    }

    public static void RecordRank(string storyId, string rank)
    {
        int order = DetectiveRank.Order(rank);
        if (order > DetectiveRank.Order(BestRank(storyId)))
        {
            GameSettings.SetValue(RankKey(storyId), order);
            GameSettings.Flush();
        }
    }

    private static string RankKey(string storyId) => $"casos.rango.{storyId}";

    /// <summary>
    /// Apunta un final; solo se queda si es mejor que el que había.
    /// </summary>
    public static void Record(string storyId, Ending ending)
    {
        Ending? best = Best(storyId);
        if (best == null || Rank(ending) > Rank(best.Value))
        {
            GameSettings.SetValue(Prefix + storyId, Rank(ending));
            GameSettings.Flush(); // A disco ya: la partida guardada se acaba de borrar
        }
    }

    // ---- Variantes jugadas: rejugar una historia trae otro culpable ----

    private static string PlayedKey(string variantId) => $"casos.jugada.{variantId}";
    private static string LastKey(string storyId) => $"casos.ultima.{storyId}";

    /// <summary>
    /// Apunta una variante terminada (ya se sabe su solución) y que es la última de su historia.
    /// </summary>
    public static void RecordPlayed(string storyId, string variantId)
    {
        GameSettings.SetValue(PlayedKey(variantId), 1f);
        // Solo hay números en los ajustes: la última se guarda como el código de su id
        GameSettings.SetValue(LastKey(storyId), Code(variantId));
        GameSettings.Flush();
    }

    public static bool Played(string variantId) => GameSettings.GetValue(PlayedKey(variantId), 0f) > 0f;

    /// <summary>
    /// Índice de la variante que toca entre 'variantIds': una aún no jugada; si ya se jugaron todas, cualquiera
    /// menos la última de su historia. 'roll' es el azar (Random.Range en el juego, fijo en los tests).
    /// </summary>
    public static int PickVariant(System.Collections.Generic.IReadOnlyList<string> variantIds, System.Func<string, string> storyOf, int roll)
    {
        var candidates = new System.Collections.Generic.List<int>();
        for (int k = 0; k < variantIds.Count; k++)
            if (!Played(variantIds[k]))
                candidates.Add(k);
        if (candidates.Count == 0)
        {
            for (int k = 0; k < variantIds.Count; k++)
                if (GameSettings.GetValue(LastKey(storyOf(variantIds[k])), -1f) != Code(variantIds[k]))
                    candidates.Add(k);
        }
        if (candidates.Count == 0)
            return variantIds.Count == 0 ? -1 : System.Math.Abs(roll) % variantIds.Count;
        return candidates[System.Math.Abs(roll) % candidates.Count];
    }

    // "2B" → número estable (los ajustes solo guardan números); basta con que distinga las de una historia
    private static float Code(string variantId)
    {
        int code = 0;
        foreach (char c in variantId ?? "")
            code = code * 31 + c;
        return code & 0xFFFF;
    }
}
