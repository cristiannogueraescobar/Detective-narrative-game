using NUnit.Framework;

public class DifficultyTests
{
    [TestCase(DifficultyLevel.Historia, 7, 0)]
    [TestCase(DifficultyLevel.Detective, 5, 1)]
    [TestCase(DifficultyLevel.Veterano, 4, -1)]
    public void PreguntasYCosteDePensar(DifficultyLevel level, int questions, int hintCost)
    {
        Assert.AreEqual(questions, Difficulty.QuestionsPerDay(level));
        Assert.AreEqual(hintCost, Difficulty.HintCost(level), "-1 = sin ayudas");
    }

    [Test]
    public void PorDefectoEsDetectiveYSeGuarda()
    {
        var store = new MemorySettingsStore();
        GameSettings.UseStore(store);
        try
        {
            Assert.AreEqual(DifficultyLevel.Detective, GameSettings.Difficulty);
            GameSettings.Difficulty = DifficultyLevel.Veterano;
            Assert.AreEqual(DifficultyLevel.Veterano, GameSettings.Difficulty);
        }
        finally
        {
            GameSettings.UseStore(null);
        }
    }

    [Test]
    public void UnGuardadoAntiguoSinDificultadEsDetective()
    {
        Assert.AreEqual(DifficultyLevel.Detective, Difficulty.FromSave(-1));
        Assert.AreEqual(DifficultyLevel.Historia, Difficulty.FromSave(0));
        Assert.AreEqual(DifficultyLevel.Detective, Difficulty.FromSave(99), "un valor raro no rompe la partida");
    }

    private class MemorySettingsStore : ISettingsStore
    {
        private readonly System.Collections.Generic.Dictionary<string, float> values = new System.Collections.Generic.Dictionary<string, float>();
        public float Get(string key, float fallback) => values.TryGetValue(key, out float v) ? v : fallback;
        public void Set(string key, float value) => values[key] = value;
    }
}
