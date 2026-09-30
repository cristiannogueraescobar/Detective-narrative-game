using NUnit.Framework;
using UnityEngine;

public class PortraitCropsTests
{
    [Test]
    public void LosEncuadresCaenDentroDeLaImagen()
    {
        foreach (string key in PortraitCrops.Keys)
        {
            foreach (Rect r in new[] { PortraitCrops.Bust(key), PortraitCrops.Face(key, null) })
            {
                Assert.GreaterOrEqual(r.xMin, 0f, key);
                Assert.GreaterOrEqual(r.yMin, 0f, key);
                Assert.LessOrEqual(r.xMax, 1.0001f, key);
                Assert.LessOrEqual(r.yMax, 1.0001f, key);
            }
            Assert.Greater(PortraitCrops.Face(key, null).yMin, PortraitCrops.Bust(key).yMin, $"{key}: la cara está en la parte alta del busto");
        }
    }

    [Test]
    public void CadaRetratoAntiguoDelElencoTieneEncuadre()
    {
        foreach (StoryData story in CaseLibrary.Stories)
            foreach (CharacterData c in story.cast)
                if (!string.IsNullOrEmpty(c.portraitKey))
                    CollectionAssert.Contains(PortraitCrops.Keys, c.portraitKey, c.id);
    }

    [Test]
    public void SinEncuadreSeMuestraEntero()
    {
        Assert.AreEqual(PortraitCrops.Full, PortraitCrops.Bust("desconocido"));
        Assert.AreEqual(PortraitCrops.Full, PortraitCrops.Bust(null));
    }
}
