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
    /// Apunta un final; solo se queda si es mejor que el que había.
    /// </summary>
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

    public static void Record(string storyId, Ending ending)
    {
        Ending? best = Best(storyId);
        if (best == null || Rank(ending) > Rank(best.Value))
        {
            GameSettings.SetValue(Prefix + storyId, Rank(ending));
            GameSettings.Flush(); // A disco ya: la partida guardada se acaba de borrar
        }
    }
}
