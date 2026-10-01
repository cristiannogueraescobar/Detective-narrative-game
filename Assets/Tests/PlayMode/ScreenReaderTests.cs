using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using TMPro;
using UnityEngine.UI;

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
        // El menú entra con fundido: lo que aún es transparente no se lee (a propósito)
        yield return new WaitForSecondsRealtime(2.5f);
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

    // ---- Revisión de la tarde ----

    private static IEnumerator Refresh()
    {
        yield return new WaitForSecondsRealtime(ScreenReader.RefreshSeconds + 0.3f);
    }

    private static IEnumerator Activate(string label)
    {
        AccessibilityNode node = Nodes().First(n => n.label == label);
        Assert.IsTrue(ScreenReader.Invoke(node), label);
        yield return Refresh();
    }

    [UnityTest]
    public IEnumerator LosMarcosTienenElOrigenArriba()
    {
        // Como en UI Toolkit (worldBound): y crece hacia abajo. "Jugar" está en la mitad de abajo del menú
        AccessibilityNode play = Nodes().First(n => n.label == "Jugar");
        Assert.Greater(play.frame.y, Screen.height * 0.4f);
        yield return null;
    }

    [UnityTest]
    public IEnumerator CambiarElHudNoEsCambiarDePantalla()
    {
        yield return Activate("Jugar");
        yield return Activate("Caso al azar");
        yield return Activate("Empezar");
        int screens = ScreenReader.ScreenChanges;

        var hud = Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).First(t => t.name == "HudText");
        hud.text = "DÍA 1 DE 7  ·  QUEDAN 4 PREGUNTAS";
        yield return Refresh();

        Assert.AreEqual(screens, ScreenReader.ScreenChanges, "una respuesta no manda el foco arriba");
        Assert.IsTrue(Nodes().Any(n => n.label.Contains("QUEDAN 4")), "pero el texto nuevo sí está");
    }

    [UnityTest]
    public IEnumerator UnDeslizadorConservaSuNodoAlCambiarDeValor()
    {
        yield return Activate("Ajustes");
        AccessibilityNode volume = Nodes().First(n => n.role == AccessibilityRole.Slider && n.label == "Volumen general");
        string before = volume.value;
        // Deslizar hacia abajo sobre el nodo, como con TalkBack (el volumen empieza al 100 %)
        Assert.IsTrue(ScreenReader.Step(volume, before == "0 %" ? +1 : -1));
        yield return Refresh();

        Assert.IsTrue(Nodes().Any(n => ReferenceEquals(n, volume)), "el mismo nodo: el lector no pierde el foco");
        Assert.AreNotEqual(before, volume.value, "y su valor al día");
    }

    // En batchmode la "pantalla" es 640x480 apaisada y el chat se queda sin alto; el expediente sí se ve y se desplaza
    [UnityTest]
    public IEnumerator ElExpedienteEsUnaZonaDesplazable()
    {
        yield return Activate("Jugar");
        yield return Activate("Caso al azar");
        Assert.IsTrue(Nodes().Any(n => n.role == AccessibilityRole.ScrollView), "el lector puede desplazar el expediente");
    }

    [UnityTest]
    public IEnumerator UnTextoTransparenteNoSeLee()
    {
        var subtitle = Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).First(t => t.text.Contains("Tres casos"));
        Color c = subtitle.color;
        subtitle.color = new Color(c.r, c.g, c.b, 0f);
        yield return Refresh();
        Assert.IsFalse(Nodes().Any(n => n.label.Contains("Tres casos")));
    }

    // Lo decorativo (las cifras de la pared de alturas) no se lee
    [UnityTest]
    public IEnumerator LoDecorativoNoSeLee()
    {
        var subtitle = Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).First(t => t.text.Contains("Tres casos"));
        subtitle.gameObject.AddComponent<Decorative>();
        yield return Refresh();
        Assert.IsFalse(Nodes().Any(n => n.label.Contains("Tres casos")));
    }

    // Ronda 10: los enlaces de la libreta (ir a interrogar, tu nota) son botones para el lector, con contexto
    [UnityTest]
    public IEnumerator LosEnlacesDeLaLibretaSonBotonesParaElLector()
    {
        yield return Activate("Jugar");
        yield return Activate("Caso al azar");
        yield return Activate("Empezar");
        Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b => b.name == "ViewCluesButton").onClick.Invoke();
        yield return new WaitForSecondsRealtime(1f);
        yield return Refresh();

        AccessibilityNode note = Nodes().FirstOrDefault(n => n.role == AccessibilityRole.Button && n.label.EndsWith("añadir nota"));
        Assert.IsNotNull(note, "la nota es un botón");
        StringAssert.StartsWith("Nota sobre ", note.label, "con el nombre de a quién se refiere");
        string who = note.label.Substring("Nota sobre ".Length).Split(':')[0];
        Assert.IsTrue(Nodes().Any(n => n.role == AccessibilityRole.Button && n.label == $"Interrogar a {who}"), "el nombre también: ir a interrogar");

        Assert.IsTrue(ScreenReader.Invoke(note));
        yield return Refresh();
        Assert.IsTrue(Nodes().Any(n => n.label == $"Nota sobre {who}: sospecha"), "activarla cambia la nota");
        Assert.IsTrue(Nodes().Any(n => ReferenceEquals(n, note)), "el mismo nodo: el foco no se pierde");
    }
}
