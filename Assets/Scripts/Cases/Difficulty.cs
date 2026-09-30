/// <summary>
/// Niveles de dificultad (Ajustes; se aplican al empezar un caso y se guardan con la partida).
///   Historia: 7 preguntas al día y "Pensar" gratis. Detective (por defecto): 5 preguntas y pensar cuesta una.
///   Veterano: 4 preguntas y sin ayudas.
/// </summary>
public enum DifficultyLevel
{
    Historia = 0,
    Detective = 1,
    Veterano = 2
}

public static class Difficulty
{
    public const DifficultyLevel Default = DifficultyLevel.Detective;

    public static int QuestionsPerDay(DifficultyLevel level)
    {
        switch (level)
        {
            case DifficultyLevel.Historia: return 7;
            case DifficultyLevel.Veterano: return 4;
            default: return 5;
        }
    }

    /// <summary>
    /// Preguntas que cuesta "Pensar" (-1: no hay ayudas).
    /// </summary>
    public static int HintCost(DifficultyLevel level)
    {
        switch (level)
        {
            case DifficultyLevel.Historia: return 0;
            case DifficultyLevel.Veterano: return -1;
            default: return 1;
        }
    }

    public static string Label(DifficultyLevel level)
    {
        switch (level)
        {
            case DifficultyLevel.Historia: return "Historia";
            case DifficultyLevel.Veterano: return "Veterano";
            default: return "Detective";
        }
    }

    public static string Description(DifficultyLevel level)
    {
        switch (level)
        {
            case DifficultyLevel.Historia: return "7 preguntas al día. Pensar es gratis.";
            case DifficultyLevel.Veterano: return "4 preguntas al día. Sin ayudas.";
            default: return "5 preguntas al día. Pensar cuesta una pregunta.";
        }
    }

    /// <summary>
    /// Nivel guardado en una partida (-1 en los guardados anteriores a la dificultad).
    /// </summary>
    public static DifficultyLevel FromSave(int saved)
    {
        return saved >= 0 && saved <= 2 ? (DifficultyLevel)saved : Default;
    }
}
