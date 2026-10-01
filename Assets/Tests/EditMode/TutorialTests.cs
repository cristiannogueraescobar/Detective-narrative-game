using System.Collections.Generic;
using NUnit.Framework;

public class TutorialTests
{
    private class MemoryStore : ISettingsStore
    {
        private readonly Dictionary<string, float> values = new Dictionary<string, float>();
        public float Get(string key, float fallback) => values.TryGetValue(key, out float v) ? v : fallback;
        public void Set(string key, float value) => values[key] = value;
    }

    [SetUp]
    public void SetUp() => GameSettings.UseStore(new MemoryStore());

    [TearDown]
    public void TearDown() => GameSettings.UseStore(null);

    [Test]
    public void CadaIndicacionSaleUnaSolaVez()
    {
        Assert.IsTrue(Tutorial.ShouldShow(Tutorial.Ask));
        Tutorial.MarkSeen(Tutorial.Ask);
        Assert.IsFalse(Tutorial.ShouldShow(Tutorial.Ask));
        Assert.IsTrue(Tutorial.ShouldShow(Tutorial.Days), "las demás siguen pendientes");
        Assert.IsFalse(Tutorial.Finished);
    }

    [Test]
    public void SaltarLasApagaTodas()
    {
        Tutorial.SkipAll();
        foreach (string id in Tutorial.All)
            Assert.IsFalse(Tutorial.ShouldShow(id), id);
        Assert.IsTrue(Tutorial.Finished);
    }

    [Test]
    public void ReiniciarLasVuelveAMostrar()
    {
        Tutorial.SkipAll();
        Tutorial.Reset();
        Assert.IsTrue(Tutorial.ShouldShow(Tutorial.Contradiction));
    }

    [Test]
    public void TodasTienenTextoYEsCorto()
    {
        foreach (string id in Tutorial.All)
        {
            string text = Tutorial.TextOf(id);
            Assert.IsNotEmpty(text);
            Assert.LessOrEqual(text.Split(' ').Length, 25, id + ": una indicación se lee de un vistazo");
        }
    }
}
