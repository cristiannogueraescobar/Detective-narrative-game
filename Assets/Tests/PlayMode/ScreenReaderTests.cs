using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Lector de pantalla (TalkBack / VoiceOver) con el módulo de accesibilidad de Unity 6: con el lector activado,
/// la interfaz visible se describe en un AccessibilityHierarchy y los nodos se pueden activar; sin lector, nada.
/// </summary>
public class ScreenReaderTests
{
    private class MemoryStore : ISettingsStore
    {
        private readonly Dictionary<string, float> values = new Dictionary<string, float>();
        public float Get(string key, float fallback) => values.TryGetValue(key, out float v) ? v : fallback;
        public void Set(string key, float value) => values[key] = value;
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SaveSystem.DirectoryOverride = Path.Combine(Path.GetTempPath(), "detective-sr-" + System.Guid.NewGuid().ToString("N"));
        GameSettings.UseStore(new MemoryStore());
        Tutorial.SkipAll();
        AssistiveSupport.screenReaderStatusOverride = AssistiveSupport.ScreenReaderStatusOverride.ForceEnabled;
        yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
        yield return new WaitForSecondsRealtime(ScreenReader.RefreshSeconds + 0.3f);
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        AssistiveSupport.screenReaderStatusOverride = AssistiveSupport.ScreenReaderStatusOverride.OSDriven;
        GameSettings.UseStore(null);
        SaveSystem.DirectoryOverride = null;
        yield return null;
    }

    private static IEnumerable<AccessibilityNode> Nodes()
    {
        var all = new List<AccessibilityNode>();
        void Walk(IEnumerable<AccessibilityNode> nodes)
        {
            foreach (AccessibilityNode n in nodes)
            {
                all.Add(n);
                Walk(n.children);
            }
        }
        if (AssistiveSupport.activeHierarchy != null)
            Walk(AssistiveSupport.activeHierarchy.rootNodes);
        return all;
    }

    [UnityTest]
    public IEnumerator ElMenuSeDescribeConSusBotones()
    {
        Assert.IsNotNull(AssistiveSupport.activeHierarchy, "con el lector activado hay jerarquía");
        AccessibilityNode play = Nodes().FirstOrDefault(n => n.label == "Jugar");
        Assert.IsNotNull(play, "el botón Jugar tiene nodo con su texto");
        Assert.AreEqual(AccessibilityRole.Button, play.role);
        Assert.Greater(play.frame.width, 0f, "con su sitio en pantalla");
        Assert.IsTrue(Nodes().Any(n => n.role == AccessibilityRole.StaticText && n.label.Contains("Tres casos")), "y los textos");
        yield return null;
    }

    [UnityTest]
    public IEnumerator ActivarUnNodoPulsaElBotonYLaPantallaNuevaSeDescribe()
    {
        AccessibilityNode play = Nodes().First(n => n.label == "Jugar");
        ScreenReader.Invoke(play);
        yield return new WaitForSecondsRealtime(ScreenReader.RefreshSeconds + 0.3f);

        Assert.IsTrue(Nodes().Any(n => n.label == "Caso al azar"), "la selección de caso ya está en la jerarquía");
        Assert.IsFalse(Nodes().Any(n => n.label == "Jugar"), "y el menú ya no");
    }

    [UnityTest]
    public IEnumerator SinLectorNoHayJerarquia()
    {
        AssistiveSupport.screenReaderStatusOverride = AssistiveSupport.ScreenReaderStatusOverride.ForceDisabled;
        yield return new WaitForSecondsRealtime(ScreenReader.RefreshSeconds + 0.3f);
        Assert.IsTrue(AssistiveSupport.activeHierarchy == null || !Nodes().Any());
    }

    [UnityTest]
    public IEnumerator LosMomentosImportantesSeAnuncian()
    {
        var ui = Object.FindFirstObjectByType<InterrogationUI>();
        ui.AddAnswer("x", "Carmen", "Estuve en casa toda la noche.");
        Assert.AreEqual("Carmen: Estuve en casa toda la noche.", ScreenReader.LastAnnouncement);

        ui.ShowClueNotification("La luz del pasillo");
        StringAssert.Contains("La luz del pasillo", ScreenReader.LastAnnouncement);
        StringAssert.Contains("Pista nueva", ScreenReader.LastAnnouncement);
        yield return null;
    }
}
