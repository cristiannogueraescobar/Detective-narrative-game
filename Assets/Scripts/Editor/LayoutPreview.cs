using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Monta la UI de la escena real en modo edición, a un tamaño de lienzo concreto y con el contenido más largo
/// posible. La usan el test de layout (LayoutValidationTests) y las capturas (ScreenshotTool).
/// </summary>
public static class LayoutPreview
{
    public const string ScenePath = "Assets/Scenes/Game.unity";

    public static readonly string[] Panels =
        { "MainMenuPanel", "IntroPanel", "InterrogationPanel", "CluesPanel", "AccusatonPanel", "ResultPanel", "IntructionsPanel", "SettingsPanel", "AboutPanel", CaseSelect.PanelName };

    private class MemoryStore : ISettingsStore
    {
        private readonly Dictionary<string, float> values = new Dictionary<string, float>();
        public float Get(string key, float fallback) => values.TryGetValue(key, out float v) ? v : fallback;
        public void Set(string key, float value) => values[key] = value;
    }

    public class Session
    {
        public Canvas canvas;
        public InterrogationUI ui;
        public MenuManager menu;
    }

    public static Session Open(Vector2 canvasSize)
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        // Sin corrutinas ni animaciones en modo edición
        GameSettings.UseStore(new MemoryStore());
        GameSettings.ReduceMotion = true;

        var session = new Session
        {
            canvas = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None).First(c => c.isRootCanvas),
            ui = Object.FindFirstObjectByType<InterrogationUI>(FindObjectsInactive.Include),
            menu = Object.FindFirstObjectByType<MenuManager>(FindObjectsInactive.Include)
        };

        CanvasScaler scaler = session.canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
            scaler.enabled = false;
        session.canvas.renderMode = RenderMode.WorldSpace;
        SetSize(session, canvasSize);

        session.ui.BuildLayout();
        session.menu.BuildLayout();
        // Como en juego: GameManager vuelve a aplicar el tema cuando ya existen los controles creados por código
        ThemeApplier.Apply(session.canvas.transform);
        FillWorstCase(session.ui);

        // Como en juego: las plantillas de los desplegables empiezan ocultas
        foreach (TMP_Dropdown dropdown in session.canvas.GetComponentsInChildren<TMP_Dropdown>(true))
        {
            if (dropdown.template != null)
                dropdown.template.gameObject.SetActive(false);
        }
        return session;
    }

    public static void Close()
    {
        GameSettings.UseStore(null);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    }

    public static void SetSize(Session session, Vector2 size)
    {
        var rect = (RectTransform)session.canvas.transform;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
        rect.position = Vector3.zero;
    }

    /// <summary>
    /// Tamaño del lienzo que produce el CanvasScaler (1080 × 1920, match 0.5) en una pantalla dada.
    /// </summary>
    public static Vector2 CanvasSize(float screenWidth, float screenHeight)
    {
        float logW = Mathf.Log(screenWidth / 1080f, 2f);
        float logH = Mathf.Log(screenHeight / 1920f, 2f);
        float scale = Mathf.Pow(2f, Mathf.Lerp(logW, logH, 0.5f));
        return new Vector2(screenWidth / scale, screenHeight / scale);
    }

    // ---------- Contenido más largo posible ----------

    private static void FillWorstCase(InterrogationUI ui)
    {
        StoryData story = CaseLibrary.Stories.OrderByDescending(s => CaseBriefing.Format(s).Length).First();
        VariantData variant = CaseLibrary.AllVariants().Select(p => p.variant).OrderByDescending(v => v.clues.Count).First();
        CaseLibrary.TryFind(variant.id, out StoryData variantStory, out _);

        List<SuspectView> suspects = CaseLibrary.Stories.SelectMany(s => s.cast)
            .OrderByDescending(c => c.DisplayName.Length).Take(4).Select(SuspectView.From).ToList();

        ui.UpdateGameState(7, 7, 5, 5);
        ui.SetSuspects(suspects);
        ui.SetEvidenceOptions(CaseLibrary.AllVariants().SelectMany(p => p.variant.clues)
            .OrderByDescending(c => c.playerName.Length).Take(6).ToList());
        ui.ShowCaseIntro(story.id, story.title, CaseBriefing.Format(story));

        var state = new InvestigationState(variant);
        foreach (ClueData clue in variant.clues)
            state.Discover(clue.id);
        state.RegisterLieTold();
        state.UpdateContradictions();
        ui.UpdateNotebook(Notebook.Format(variantStory, state, variantStory.cast.Select(c => c.id),
            variantStory.cast.ToDictionary(c => c.id, c => Emotion.Enfadado),
            c => $"La versión de alguien («una cita larga de su mentira») choca con: {c.playerName}", onPaper: true));

        ShowEnding(ui, Ending.Insufficient);
        ui.ShowAccusationPanel(suspects, canGoBack: true);

        // Conversación larga (vive en el scroll), con todos los tipos de aviso que pueden aparecer en ella
        ui.ShowDayTransition(2, "El forense sitúa la muerte entre las 22:30 y las 23:15.");
        ui.ShowSuspectUnlocked(suspects[0].displayName);
        ui.ShowContradictionNotification("La versión de alguien («una cita») choca con: Lo que vio la ventana");
        ui.ShowNotice("Un agente te informa: conviene hablar con Rosario Gil.");
        ui.ShowError("No te quedan preguntas hoy.");
        // Conversación del sospechoso abierto: preguntas cortas y largas, una con prueba, respuestas de varios tamaños
        string id = suspects[0].id;
        string name = suspects[0].shortName;
        ui.Conversations.Append(id, ChatEntry.Player("¿Dónde estaba usted entre las diez y las once de la noche del sábado?", null, "09:00"));
        ui.Conversations.Append(id, ChatEntry.Suspect(name, string.Join(" ", Enumerable.Repeat("respuesta larga del sospechoso", 40)), "09:00"));
        ui.Conversations.Append(id, ChatEntry.Player("¿Y esto?", "La taza de la mesilla", "11:17"));
        ui.Conversations.Append(id, ChatEntry.Suspect(name, "No.", "11:17"));
        ui.Conversations.Append(id, ChatEntry.Player("", "Lo que vio la ventana", "13:11"));
        ui.Conversations.Append(id, ChatEntry.Suspect(name, "Eso no demuestra nada. Rosario ve lo que quiere ver desde esa ventana, siempre ha sido así.", "13:11"));
        ui.RefreshConversationView();
        ui.ShowWaiting(true);
    }

    /// <summary>
    /// Deja el panel de resultado con un final concreto y el epílogo más largo.
    /// </summary>
    public static void ShowEnding(Session session, Ending ending)
    {
        ShowEnding(session.ui, ending);
    }

    private static void ShowEnding(InterrogationUI ui, Ending ending)
    {
        VariantData variant = CaseLibrary.AllVariants().Select(p => p.variant).OrderByDescending(v => v.epilogue.Length).First();
        bool correct = ending != Ending.Bad;
        var result = new AccusationResult
        {
            ending = ending,
            correct = correct,
            evidence = ending == Ending.Good ? 6 : ending == Ending.Bittersweet ? 3 : 1,
            incriminatingFound = ending == Ending.Good ? 4 : 2,
            contradictions = ending == Ending.Good ? 2 : 0
        };
        ui.ShowAccusationResult(result, correct ? "Encarna Molina" : "Inspector Ruiz", "Encarna Molina", 7, variant.epilogue);
    }

    // ---------- Mostrar un panel ----------

    public static RectTransform ShowOnly(Session session, string panelName)
    {
        RectTransform target = Find(session, panelName);
        foreach (Transform child in session.canvas.transform)
            child.gameObject.SetActive(false);

        // La libreta vive dentro del interrogatorio
        if (target.parent != session.canvas.transform)
            target.parent.gameObject.SetActive(true);

        foreach (Transform sibling in target.parent)
        {
            if (sibling != target && sibling.TryGetComponent(out LayoutElement le) && le.ignoreLayout)
                sibling.gameObject.SetActive(false);
        }

        // Las capas (libreta, destellos) dentro del panel empiezan ocultas, como en juego
        foreach (Transform child in target)
        {
            if (child.TryGetComponent(out LayoutElement le) && le.ignoreLayout && child.name != "IntroFondo (auto)" && child.name != "IntroCabecera (auto)")
                child.gameObject.SetActive(false);
        }

        target.gameObject.SetActive(true);
        if (target.parent != session.canvas.transform)
            target.SetAsLastSibling(); // Una capa abierta se dibuja encima (como ShowCluesPanel)
        Rebuild((RectTransform)session.canvas.transform);

        // Como en juego: el chat abierto se ve por el final
        foreach (ChatView chat in target.GetComponentsInChildren<ChatView>(false))
            chat.ScrollToBottom();
        return target;
    }

    public static void Rebuild(RectTransform root)
    {
        for (int i = 0; i < 3; i++)
        {
            Canvas.ForceUpdateCanvases();
            foreach (LayoutGroup group in root.GetComponentsInChildren<LayoutGroup>(false).Reverse())
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)group.transform);
            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(false))
                text.ForceMeshUpdate(true, true);
        }
    }

    public static RectTransform Find(Session session, string name)
    {
        return session.canvas.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(r => r.name == name);
    }
}
