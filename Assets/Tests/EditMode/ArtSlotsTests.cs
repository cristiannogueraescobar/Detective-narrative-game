using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class ArtSlotsTests
{
    private static string ArtNeeded => File.ReadAllText(Path.Combine(Application.dataPath, "..", "ART-NEEDED.md"));

    [Test]
    public void CadaHuecoDeArteEstaDocumentadoEnArtNeeded()
    {
        string doc = ArtNeeded;

        foreach (ArtSlot slot in ArtSlots.All())
            StringAssert.Contains(slot.path, doc, $"{slot.path} debe aparecer en ART-NEEDED.md");
    }

    [Test]
    public void RutasBajoAssetsArtEnPng()
    {
        foreach (ArtSlot slot in ArtSlots.All())
        {
            StringAssert.StartsWith("Assets/Art/", slot.path);
            StringAssert.EndsWith(".png", slot.path);
            Assert.Greater(slot.width, 0);
            Assert.Greater(slot.height, 0);
        }
    }

    [Test]
    public void CabeceraEIntroPorHistoria()
    {
        foreach (StoryData story in CaseLibrary.Stories)
        {
            Assert.IsTrue(ArtSlots.All().Any(s => s.path == ArtSlots.StoryHeader(story.id)), story.id);
            Assert.IsTrue(ArtSlots.All().Any(s => s.path == ArtSlots.StoryIntro(story.id)), story.id);
        }
    }

    [Test]
    public void SinArchivoSeUsaColorPlano()
    {
        Texture2D texture = ArtSlots.LoadOrPlaceholder("Assets/Art/no/existe.png", Color.red);

        Assert.IsNotNull(texture);
        Assert.AreEqual(Color.red, texture.GetPixel(0, 0));
    }
}
