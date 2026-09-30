using System.Collections.Generic;
using NUnit.Framework;

public class CaseRecordsTests
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
    public void SinJugarNoHayFinal()
    {
        Assert.IsNull(CaseRecords.Best("1"));
    }

    [Test]
    public void SeQuedaElMejorFinal()
    {
        CaseRecords.Record("2", Ending.Insufficient);
        CaseRecords.Record("2", Ending.Good);
        CaseRecords.Record("2", Ending.Bad);
        Assert.AreEqual(Ending.Good, CaseRecords.Best("2"));
        Assert.IsNull(CaseRecords.Best("3"), "cada historia va aparte");
    }
}
