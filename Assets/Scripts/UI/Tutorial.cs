/// <summary>
/// Tutorial ligero de la primera partida: cuatro indicaciones en su momento, cada una una sola vez.
/// "Saltar tutorial" las apaga todas. Se guarda con los ajustes.
/// </summary>
public static class Tutorial
{
    public const string Ask = "preguntar";
    public const string Days = "dias";
    public const string Evidence = "pruebas";
    public const string Contradiction = "contradiccion";

    public static readonly string[] All = { Ask, Days, Evidence, Contradiction };

    private const string Prefix = "tutorial.";
    private const string SkippedKey = "tutorial.saltado";

    public static bool ShouldShow(string id)
    {
        return !GameSettings.GetFlag(SkippedKey) && !GameSettings.GetFlag(Prefix + id);
    }

    public static void MarkSeen(string id)
    {
        GameSettings.SetFlag(Prefix + id, true);
    }

    public static void SkipAll()
    {
        GameSettings.SetFlag(SkippedKey, true);
    }

    public static bool Finished
    {
        get
        {
            if (GameSettings.GetFlag(SkippedKey))
                return true;
            foreach (string id in All)
            {
                if (!GameSettings.GetFlag(Prefix + id))
                    return false;
            }
            return true;
        }
    }

    public static void Reset()
    {
        GameSettings.SetFlag(SkippedKey, false);
        foreach (string id in All)
            GameSettings.SetFlag(Prefix + id, false);
    }

    public static string TextOf(string id, int questionsPerDay = 5)
    {
        switch (id)
        {
            case Ask:
                return "Elige a quién interrogar y escribe tu pregunta (o toca una de ejemplo). Funcionan las concretas: horas, lugares, objetos.";
            case Days:
                return $"Tienes {GameTexts.NumberWord(questionsPerDay)} preguntas al día. Cuando se acaben, pulsa «Fin del día» y a la mañana siguiente llegará un parte.";
            case Evidence:
                return "Las pistas se guardan en tu libreta. Con «Mostrar prueba» puedes enseñárselas a un sospechoso junto a tu pregunta.";
            default:
                return "¡Lo has pillado en una mentira! Las contradicciones cuentan como pruebas cuando acuses.";
        }
    }
}
