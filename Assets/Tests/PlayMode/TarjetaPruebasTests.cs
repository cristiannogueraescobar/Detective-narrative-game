using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Tarjeta "TUS PRUEBAS" jugando de verdad (sesión C, punto 3d): las contradicciones que dice la tarjeta son las mismas
/// frases que la libreta, ni más ni menos.
/// </summary>
public class TarjetaPruebasTests
{
    private class Store : ISettingsStore
    {
        private readonly Dictionary<string, float> values = new Dictionary<string, float>();
        public float Get(string key, float fallback) => values.TryGetValue(key, out float v) ? v : fallback;
        public void Set(string key, float value) => values[key] = value;
    }

    private string saveDirectory;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        saveDirectory = Path.Combine(Path.GetTempPath(), "detective-tarjeta-" + System.Guid.NewGuid().ToString("N"));
        SaveSystem.DirectoryOverride = saveDirectory;
        GameSettings.UseStore(new Store());
        Tutorial.SkipAll();
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        SaveSystem.Flush();
        SaveSystem.DirectoryOverride = null;
        GameSettings.UseStore(null);
        if (Directory.Exists(saveDirectory))
            Directory.Delete(saveDirectory, true);
        yield return null;
    }

    [UnityTest]
    public IEnumerator LaTarjetaDiceLasContradiccionesComoLaLibreta()
    {
        // La variante sale al azar: se prueba hasta dar con una en la que el primer día se puede conseguir todo
        var provider = new ScriptedProvider();
        bool played = false;
        for (int attempt = 0; attempt < 12 && !played; attempt++)
        {
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            yield return null;
            yield return ScriptedPlay.Start(attempt % 3, provider);
            if (!ScriptedPlay.Feasible(2, true))
                continue;
            var found = new List<ClueData>();
            yield return ScriptedPlay.GatherEvidence(provider, 2, true, found);
            played = true;
        }
        Assert.IsTrue(played, "ninguna de 12 partidas permitía una contradicción el primer día");

        var manager = Object.FindFirstObjectByType<AIConversationManager>();
        List<string> expected = manager.State.ContradictionClueIds.Select(id => manager.DescribeContradiction(manager.State.Variant.Clue(id))).ToList();
        Assert.IsNotEmpty(expected);

        Object.FindFirstObjectByType<GameManager>().ForceAccusationPanel();
        yield return new WaitForSecondsRealtime(0.5f);
        var card = ScriptedPlay.Find(EvidenceCard.ObjectName).GetComponent<EvidenceCard>();
        if (card.Mode == EvidenceCardMode.Collapsed)
        {
            card.Toggle();
            yield return null;
        }

        var ui = Object.FindFirstObjectByType<InterrogationUI>();
        var notebook = (TMP_Text)typeof(InterrogationUI).GetField("cluesText", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ui);
        foreach (string line in expected)
        {
            StringAssert.Contains(line, notebook.text, "la libreta");
            StringAssert.Contains(line, card.VisibleText, "la tarjeta, con el mismo texto");
        }
        // Y nada que la libreta no diga: cada línea de contradicción de la tarjeta está en la libreta
        foreach (string line in card.VisibleText.Split('\n').Where(l => l.Contains("choca con")))
            StringAssert.Contains(line.Replace("≠", "").Trim(), notebook.text);
    }
}
