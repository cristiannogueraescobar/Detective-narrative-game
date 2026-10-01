using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// Revisión 5, hallazgo 4 (sesión C): si el detective-bot ponía el nombre de una pista en vez de un sospechoso y ese
/// nombre contenía el de otro personaje ("El coche de Ana", que sabe Luis), se resolvía a Ana antes de mirar la pista.
/// </summary>
public class BotResolveTests
{
    private static (StoryData, VariantData) Story()
    {
        var story = new StoryData
        {
            cast = new List<CharacterData>
            {
                new CharacterData { id = "a", name = "Ana Pérez", shortName = "Ana" },
                new CharacterData { id = "b", name = "Luis Gil", shortName = "Luis" }
            }
        };
        var variant = new VariantData
        {
            clues = new List<ClueData> { new ClueData { id = "X_coche", playerName = "El coche de Ana", holder = "b" } }
        };
        return (story, variant);
    }

    [Test]
    public void UnaPistaQueNombraAOtroVaAQuienLaSabe()
    {
        var (story, variant) = Story();
        Assert.AreEqual("b", BotPlayer.ResolveSuspect(story, variant, "El coche de Ana"));
        Assert.AreEqual("b", BotPlayer.ResolveSuspect(story, variant, "X_coche"));
    }

    [Test]
    public void UnNombreSigueResolviendoseAlPersonaje()
    {
        var (story, variant) = Story();
        Assert.AreEqual("a", BotPlayer.ResolveSuspect(story, variant, "Ana"));
        Assert.AreEqual("b", BotPlayer.ResolveSuspect(story, variant, "Luis Gil"));
    }
}
