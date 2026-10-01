using System.Linq;
using NUnit.Framework;

/// <summary>
/// Sesión C: expresiones de los 12 personajes editando la cara de su retrato del juego (Tools/make_expressions.py).
/// Se usan como su base (arte antiguo: mismo encuadre y tratamiento); tranquilo es el retrato de hoy.
/// </summary>
public class LegacyExpressionsTests
{
    [Test]
    public void BuscaLaPropiaYDespuesSuSustituta()
    {
        CollectionAssert.AreEqual(new[] { LegacyExpressions.Folder + "/javier_triste.png" },
                                  LegacyExpressions.Candidates("javier", Emotion.Triste).ToArray());
        CollectionAssert.AreEqual(new[] { LegacyExpressions.Folder + "/ruiz_asustado.png", LegacyExpressions.Folder + "/ruiz_nervioso.png" },
                                  LegacyExpressions.Candidates("ruiz", Emotion.Asustado).ToArray());
        CollectionAssert.IsEmpty(LegacyExpressions.Candidates("javier", Emotion.Tranquilo).ToList(), "tranquilo es el retrato de hoy");
        CollectionAssert.IsEmpty(LegacyExpressions.Candidates(null, Emotion.Triste).ToList());
    }

    [Test]
    public void CadaPersonajeTieneSusDosExpresiones()
    {
        foreach (StoryData story in CaseLibrary.Stories)
            foreach (CharacterData c in story.cast)
            {
                int found = new[] { Emotion.Triste, Emotion.Nervioso, Emotion.Enfadado }
                    .Count(e => ArtLibrary.Load($"{LegacyExpressions.Folder}/{c.artId}_{e.ToString().ToLowerInvariant()}.png") != null);
                Assert.AreEqual(2, found, $"{c.shortName} ({c.artId})");
            }
    }

    [Test]
    public void LaExpresionTieneElTamanoDeSuRetrato()
    {
        // Mismo tamaño que la imagen de hoy: el encuadre (PortraitCrops) se reutiliza tal cual
        var daniel = ArtLibrary.Load("Assets/Images/Suspects/padre.gif.png");
        var enfadado = ArtLibrary.Load(LegacyExpressions.Folder + "/daniel_enfadado.png");
        Assert.IsNotNull(daniel);
        Assert.IsNotNull(enfadado);
        Assert.AreEqual(daniel.width, enfadado.width);
        Assert.AreEqual(daniel.height, enfadado.height);
    }
}
