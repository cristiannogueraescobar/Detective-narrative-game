using System.Collections.Generic;
using NUnit.Framework;

public class GameSettingsTests
{
    private class MemoryStore : ISettingsStore
    {
        public readonly Dictionary<string, float> values = new Dictionary<string, float>();
        public float Get(string key, float fallback) => values.TryGetValue(key, out float v) ? v : fallback;
        public void Set(string key, float value) => values[key] = value;
    }

    private MemoryStore store;

    [SetUp]
    public void SetUp()
    {
        store = new MemoryStore();
        GameSettings.UseStore(store);
    }

    [TearDown]
    public void TearDown()
    {
        GameSettings.UseStore(null);
    }

    [Test]
    public void ValoresPorDefecto()
    {
        Assert.AreEqual(1f, GameSettings.Volume);
        Assert.AreEqual(1f, GameSettings.TextSpeed);
        Assert.IsFalse(GameSettings.ReduceMotion);
    }

    [Test]
    public void SeGuardanYSeLimitan()
    {
        GameSettings.Volume = 1.7f;
        GameSettings.TextSpeed = 0.1f;
        GameSettings.ReduceMotion = true;

        Assert.AreEqual(1f, GameSettings.Volume);
        Assert.AreEqual(GameSettings.MinTextSpeed, GameSettings.TextSpeed);
        Assert.IsTrue(GameSettings.ReduceMotion);
        Assert.AreEqual(1f, store.values[GameSettings.ReduceMotionKey]);
    }

    [Test]
    public void AvisaDeLosCambios()
    {
        int changes = 0;
        GameSettings.Changed += () => changes++;

        GameSettings.Volume = 0.5f;
        GameSettings.TextSpeed = 1.5f;

        Assert.AreEqual(2, changes);
    }
}
