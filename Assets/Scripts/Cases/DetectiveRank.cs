using System;

/// <summary>
/// Rango del detective al cerrar un caso: el final manda; la prueba clave y acusar con días de sobra suben, y
/// muchas ayudas bajan. Acusar a la persona equivocada nunca pasa de "Novato".
/// </summary>
public static class DetectiveRank
{
    public static readonly string[] Ranks = { "Novato", "Agente", "Sabueso", "Inspector", "Inspector jefe" };

    public static string For(Ending ending, bool keyClueRight, int hintsUsed, int daysLeft)
    {
        if (ending == Ending.Bad)
            return Ranks[0];

        int points = ending == Ending.Good ? 3 : ending == Ending.Bittersweet ? 2 : 1;
        if (keyClueRight)
            points++;
        if (daysLeft >= 2)
            points++;
        if (hintsUsed >= 3)
            points--;
        // Sin la prueba clave no se llega a lo más alto: el rango premia razonar, no solo acumular
        int max = keyClueRight ? Ranks.Length - 1 : Ranks.Length - 2;
        return Ranks[Math.Max(1, Math.Min(max, points))];
    }

    public static int Order(string rank)
    {
        return rank == null ? -1 : Array.IndexOf(Ranks, rank);
    }
}
