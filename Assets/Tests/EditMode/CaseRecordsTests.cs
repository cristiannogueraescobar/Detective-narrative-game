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

    // Ronda 14: rejugar una historia "con otro culpable" no debe repetir la solución (al azar salía 1 de cada 3)
    [Test]
    public void AlRejugarSaleUnaVarianteQueNoHasJugado()
    {
        var ids = new[] { "1A", "1B", "1C" };
        CaseRecords.RecordPlayed("1", "1A");
        CaseRecords.RecordPlayed("1", "1B");
        for (int roll = 0; roll < 30; roll++)
            Assert.AreEqual("1C", ids[CaseRecords.PickVariant(ids, s => "1", roll)]);
    }

    [Test]
    public void ConTodasJugadasNoRepiteLaUltima()
    {
        var ids = new[] { "1A", "1B", "1C" };
        CaseRecords.RecordPlayed("1", "1A");
        CaseRecords.RecordPlayed("1", "1C");
        CaseRecords.RecordPlayed("1", "1B"); // La última
        var picked = new System.Collections.Generic.HashSet<string>();
        for (int roll = 0; roll < 30; roll++)
            picked.Add(ids[CaseRecords.PickVariant(ids, s => "1", roll)]);
        CollectionAssert.DoesNotContain(picked, "1B");
        CollectionAssert.AreEquivalent(new[] { "1A", "1C" }, picked, "y las demás siguen saliendo");
    }

    [Test]
    public void SinNadaJugadoCualquieraPuedeSalir()
    {
        var ids = new[] { "1A", "1B", "2A" };
        var picked = new System.Collections.Generic.HashSet<int>();
        for (int roll = 0; roll < 30; roll++)
            picked.Add(CaseRecords.PickVariant(ids, s => s.Substring(0, 1), roll));
        Assert.AreEqual(3, picked.Count);
    }

    // El final invita a rejugar con lo que queda por ver (y felicita al completarla)
    [Test]
    public void ElFinalDiceCuantosCulpablesQuedanPorVer()
    {
        StringAssert.Contains("otros dos culpables", GameTexts.ReplayLine(2));
        StringAssert.Contains("otro culpable", GameTexts.ReplayLine(1));
        StringAssert.Contains("todos", GameTexts.ReplayLine(0));

        var ids = new[] { "1A", "1B", "1C" };
        CaseRecords.RecordPlayed("1", "1B");
        Assert.AreEqual(2, CaseRecords.Unplayed(ids));
    }
}
