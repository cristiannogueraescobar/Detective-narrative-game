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
/// Decisión de Cristian (fase 1 de las mentiras de inocentes), jugando de verdad: la mentira cuenta en cuanto la versión
/// está en la libreta. En 1B, Daniel contesta sin decir su mentira; Amparo cuenta lo de la cena (1B_cena, en su versión);
/// la contradicción tiene que salir en la libreta igual.
/// </summary>
public class MentirasInocentesPlayTests
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
        saveDirectory = Path.Combine(Path.GetTempPath(), "detective-mentiras-" + System.Guid.NewGuid().ToString("N"));
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
    public IEnumerator LaMentiraDeDanielCuentaAunqueNoLaDigaEnElChat()
    {
        var provider = new ScriptedProvider();
        AIConversationManager manager = null;
        for (int attempt = 0; attempt < 25; attempt++)
        {
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            yield return null;
            yield return ScriptedPlay.Start(0, provider);
            manager = Object.FindFirstObjectByType<AIConversationManager>();
            if (manager.State.Variant.id == "1B")
                break;
        }
        Assert.AreEqual("1B", manager.State.Variant.id, "en 25 partidas de la historia 1 no salió la variante 1B");

        StoryData story = manager.Story;
        CharacterData daniel = story.Character("padre");
        CharacterData amparo = story.Character("vecina");
        // Daniel contesta sin decir que cenó con clientes (y nombra a Amparo, como en su ficha: así aparece)
        yield return ScriptedPlay.AskWith(provider, daniel, "¿Cómo está usted?", "Destrozado, inspector. Pregunte a Amparo, la vecina de enfrente: lo ve todo. [ESTADO: triste]");
        yield return new WaitForSecondsRealtime(1f);

        // Amparo cuenta lo de la cena (su versión de 1B_cena): la frase real que el detector reconoce
        Assert.IsTrue(ScriptedPlay.Unlocked(story).Contains(amparo), "nombrarla la trae");
        ClueData cena = manager.State.Variant.Clue("1B_cena").ForHolder("vecina");
        yield return ScriptedPlay.AskWith(provider, amparo, cena.calibrationQuestions[0].Split('|')[0].Trim(), cena.sampleHits[0] + " [ESTADO: tranquilo]");
        Assert.IsTrue(manager.State.IsDiscovered("1B_cena"), "Amparo lo ha contado");

        string expected = manager.DescribeContradiction(manager.State.Variant.Clue("1B_cena"));
        Assert.IsTrue(manager.State.ContradictionClueIds.Contains("1B_cena"), "la contradicción cuenta sin que Daniel diga su mentira");
        var ui = Object.FindFirstObjectByType<InterrogationUI>();
        var notebook = (TMP_Text)typeof(InterrogationUI).GetField("cluesText", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ui);
        yield return new WaitForSecondsRealtime(0.5f);
        StringAssert.Contains(expected, notebook.text, "y la libreta la enseña");
        StringAssert.Contains("Dice: «" + manager.State.Variant.Role("padre").version, notebook.text, "con su versión");
    }
}
