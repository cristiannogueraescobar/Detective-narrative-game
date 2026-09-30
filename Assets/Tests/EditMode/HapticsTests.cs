using System.Collections.Generic;
using NUnit.Framework;

public class HapticsTests
{
    private class MemoryStore : ISettingsStore
    {
        private readonly Dictionary<string, float> values = new Dictionary<string, float>();
        public float Get(string key, float fallback) => values.TryGetValue(key, out float v) ? v : fallback;
        public void Set(string key, float value) => values[key] = value;
    }

    [TearDown]
    public void TearDown() => GameSettings.UseStore(null);

    [Test]
    public void LaVibracionRespetaElAjuste()
    {
        GameSettings.UseStore(new MemoryStore());
        int before = Haptics.Pulses;
        Haptics.Pulse();
        Assert.AreEqual(before + 1, Haptics.Pulses, "activada por defecto");

        GameSettings.Vibration = false;
        Haptics.Pulse();
        Assert.AreEqual(before + 1, Haptics.Pulses, "apagada, no vibra");
    }
}
