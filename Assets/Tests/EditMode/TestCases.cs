using System.Collections.Generic;

/// <summary>
/// Historia sintética mínima para los tests: culpable "a"; pistas i1..i3 (incriminan), x (incrimina y
/// expone la mentira), d (descarta a "b"), ctx (contexto).
/// </summary>
public static class TestCases
{
    public static VariantData Variant()
    {
        return Story().variants[0];
    }

    public static StoryData Story()
    {
        var variant = new VariantData
        {
            id = "T1",
            culpritId = "a",
            epilogue = "Lo hizo A.",
            morningReports = new[] { "", "d2", "d3", "d4", "d5", "d6", "d7" },
            roles = new List<CharacterRole>
            {
                new CharacterRole
                {
                    characterId = "a", knowledge = new[] { "Conoces a Bea." }, version = "Estuve en casa.",
                    secret = "Lo hiciste tú.", admitsWhen = "nunca", nervousAbout = "la hora",
                    ifAccused = "Te indignas.", doesNotKnow = "Nada de coches.",
                    lieQuote = "estuve en casa", lieAnchors = new[] { new[] { "en casa" } },
                    versionB = "Salí un momento."
                },
                new CharacterRole
                {
                    characterId = "b", knowledge = new[] { "Ves a Ana a diario." }, version = "Trabajé.",
                    secret = "Fumas a escondidas.", admitsWhen = "te insisten", nervousAbout = "el tabaco",
                    ifAccused = "Lloras.", doesNotKnow = "Nada de la llave."
                },
                new CharacterRole
                {
                    characterId = "c", knowledge = new string[0], version = "Miraba por la ventana.",
                    secret = "Espías a los vecinos.", admitsWhen = "te insisten", nervousAbout = "los prismáticos",
                    ifAccused = "Te ofendes.", doesNotKnow = "Nada del dinero."
                }
            },
            clues = new List<ClueData>
            {
                Clue("i1", "b", ClueKind.Incriminates, "taza"),
                Clue("i2", "b", ClueKind.Incriminates, "llave"),
                Clue("i3", "c", ClueKind.Incriminates, "extracto"),
                Clue("x", "c", ClueKind.Incriminates, "cortina", exposesLie: true),
                Clue("d", "b", ClueKind.Clears, "partida", clears: "b", isSecret: true),
                Clue("ctx", "b", ClueKind.Context, "lluvia")
            }
        };

        return new StoryData
        {
            id = "T",
            title = "Historia de prueba",
            intro = "Intro.",
            caseBrief = "Alguien murió.",
            cast = new List<CharacterData>
            {
                Character("a", "Ana Gil", "Ana", "madre", true, "ana"),
                Character("b", "Bea Gil", "Bea", "hija", true, "bea"),
                Character("c", "Carla Paz", "Carla", "vecina", false, "carla")
            },
            variants = new List<VariantData> { variant }
        };
    }

    private static ClueData Clue(string id, string holder, ClueKind kind, string anchor,
                                 bool exposesLie = false, string clears = null, bool isSecret = false)
    {
        return new ClueData
        {
            id = id,
            playerName = $"Nombre {id}",
            summary = $"Resumen {id}",
            holder = holder,
            topic = $"tema {id}",
            fact = $"hecho {id} con {anchor}",
            isSecret = isSecret,
            anchors = new[] { new[] { anchor }, new[] { id } },
            kind = kind,
            clears = clears,
            exposesLie = exposesLie,
            calibrationQuestions = new[] { $"¿Qué sabes de {anchor}?" }
        };
    }

    private static CharacterData Character(string id, string name, string shortName, string role,
                                           bool unlocked, string alias)
    {
        return new CharacterData
        {
            id = id, name = name, shortName = shortName, roleLabel = role, portraitKey = role,
            identity = $"Eres {name}.", speech = "Hablas claro.", speechExample = "Hola.",
            startsUnlocked = unlocked, mentionAliases = new[] { alias }
        };
    }
}
