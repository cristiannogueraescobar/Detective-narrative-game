using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Lector de pantalla (TalkBack en Android, VoiceOver en iOS) con el módulo de accesibilidad de Unity 6.
/// Solo trabaja si el lector del sistema está activado: entonces describe lo que se ve (botones, desplegables,
/// casillas, deslizadores, campos y textos, en orden de lectura) en un AccessibilityHierarchy y lo rehace cuando
/// cambia la pantalla. Los nodos se activan como un toque. Los momentos importantes (respuestas, pistas,
/// contradicciones, parte del día) se anuncian con Announce. Sin lector activado no hace nada ni cuesta nada.
/// Requisito del módulo en Android: API 26 (ver docs/RESEARCH.md).
/// </summary>
public class ScreenReader : MonoBehaviour
{
    public const float RefreshSeconds = 0.5f;
    private const int MaxTexts = 60; // Un chat largo no debe hacer una jerarquía enorme: los últimos mensajes

    private static ScreenReader instance;
    private readonly Dictionary<int, System.Action> actions = new Dictionary<int, System.Action>();
    private readonly Dictionary<int, (System.Action up, System.Action down)> steppers = new Dictionary<int, (System.Action, System.Action)>();
    private string signature = "";
    private float next;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (instance != null)
            return;
        var go = new GameObject("Lector de pantalla (auto)");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<ScreenReader>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        signature = "";
        next = 0f;
    }

    /// <summary>
    /// Anuncia un momento importante (solo si el lector está activado).
    /// </summary>
    public static void Announce(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || !AssistiveSupport.isScreenReaderEnabled)
            return;
        LastAnnouncement = Plain(text);
        AssistiveSupport.notificationDispatcher?.SendAnnouncement(LastAnnouncement);
    }

    /// <summary>
    /// Último anuncio enviado (para tests).
    /// </summary>
    public static string LastAnnouncement { get; private set; }

    /// <summary>
    /// Lo que hace el lector al activar un nodo (doble toque en TalkBack). Público para los tests.
    /// </summary>
    public static bool Invoke(AccessibilityNode node)
    {
        if (instance == null || node == null || !instance.actions.TryGetValue(node.id, out System.Action action))
            return false;
        action();
        return true;
    }

    private void Update()
    {
        if (!AssistiveSupport.isScreenReaderEnabled)
        {
            if (AssistiveSupport.activeHierarchy != null)
            {
                AssistiveSupport.activeHierarchy = null;
                signature = "";
            }
            return;
        }
        if (Time.unscaledTime < next)
            return;
        next = Time.unscaledTime + RefreshSeconds;

        List<Item> items = Collect();
        string current = Signature(items);
        if (current == signature && AssistiveSupport.activeHierarchy != null)
        {
            AssistiveSupport.activeHierarchy.RefreshNodeFrames(); // Lo mismo, quizá en otro sitio (scroll)
            return;
        }
        bool screenChanged = signature.Length > 0 && FirstLabel(items) != FirstLabelOf(signature);
        signature = current;
        AssistiveSupport.activeHierarchy = Build(items);
        if (screenChanged)
            AssistiveSupport.notificationDispatcher?.SendScreenChanged(null);
        else
            AssistiveSupport.notificationDispatcher?.SendLayoutChanged(null);
    }

    // ---------- Qué se ve ----------

    private struct Item
    {
        public RectTransform rect;
        public Camera camera;
        public AccessibilityRole role;
        public string label;
        public string value;
        public bool disabled;
        public Selectable control;
    }

    private static List<Item> Collect()
    {
        var items = new List<Item>();
        var texts = new List<Item>();
        foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (!canvas.isRootCanvas || !canvas.isActiveAndEnabled)
                continue;
            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            foreach (Selectable s in canvas.GetComponentsInChildren<Selectable>(false))
            {
                if (s is Scrollbar || !Visible((RectTransform)s.transform, camera))
                    continue;
                items.Add(new Item
                {
                    rect = (RectTransform)s.transform,
                    camera = camera,
                    role = RoleOf(s),
                    label = LabelOf(s),
                    value = ValueOf(s),
                    disabled = !s.interactable,
                    control = s
                });
            }

            foreach (TMP_Text t in canvas.GetComponentsInChildren<TMP_Text>(false))
            {
                if (t.GetComponentInParent<Selectable>() != null || !Visible(t.rectTransform, camera))
                    continue;
                string text = Plain(t.GetParsedText());
                if (text.Length == 0)
                    continue;
                texts.Add(new Item { rect = t.rectTransform, camera = camera, role = AccessibilityRole.StaticText, label = text });
            }
        }
        if (texts.Count > MaxTexts)
            texts.RemoveRange(0, texts.Count - MaxTexts);
        items.AddRange(texts);
        items.Sort((a, b) => ReadingOrder(a).CompareTo(ReadingOrder(b)));
        return items;
    }

    // De arriba abajo y de izquierda a derecha (filas de 24 px de tolerancia)
    private static float ReadingOrder(Item item)
    {
        Rect r = ScreenRect(item.rect, item.camera);
        return Mathf.Round(r.y / 24f) * 100000f + r.x;
    }

    private static bool Visible(RectTransform rect, Camera camera)
    {
        if (!rect.gameObject.activeInHierarchy)
            return false;
        CanvasGroup group = rect.GetComponentInParent<CanvasGroup>();
        if (group != null && group.alpha < 0.05f)
            return false;
        Rect r = ScreenRect(rect, camera);
        if (r.width < 1f || r.height < 1f || !r.Overlaps(new Rect(0, 0, Screen.width, Screen.height)))
            return false;
        // Recortado por una máscara (texto del chat fuera de la vista)
        RectMask2D mask = rect.GetComponentInParent<RectMask2D>();
        if (mask != null && !ScreenRect(mask.rectTransform, camera).Overlaps(r))
            return false;
        return !Covered(rect, camera);
    }

    private static readonly List<RaycastResult> hits = new List<RaycastResult>();

    // ¿Lo tapa otra cosa (el menú debajo de la selección de caso, la pantalla debajo de un aviso)? Lo de encima en
    // su centro tiene que ser él mismo, algo suyo o algo de lo que forma parte (su propio panel)
    private static bool Covered(RectTransform rect, Camera camera)
    {
        EventSystem events = EventSystem.current;
        if (events == null)
            return false;
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        Vector2 center = RectTransformUtility.WorldToScreenPoint(camera, (corners[0] + corners[2]) * 0.5f);
        hits.Clear();
        events.RaycastAll(new PointerEventData(events) { position = center }, hits);
        if (hits.Count == 0)
            return false;
        Transform top = hits[0].gameObject.transform;
        return !(top == rect || top.IsChildOf(rect) || rect.IsChildOf(top));
    }

    private static AccessibilityRole RoleOf(Selectable s)
    {
        switch (s)
        {
            case TMP_Dropdown _: return AccessibilityRole.Dropdown;
            case Toggle _: return AccessibilityRole.Toggle;
            case Slider _: return AccessibilityRole.Slider;
            case TMP_InputField _: return AccessibilityRole.TextField;
            default: return AccessibilityRole.Button;
        }
    }

    private static string LabelOf(Selectable s)
    {
        switch (s)
        {
            case TMP_Dropdown d:
                return d.captionText != null ? Plain(d.captionText.GetParsedText()) : s.name;
            case TMP_InputField f:
                string typed = f.text;
                return string.IsNullOrEmpty(typed) && f.placeholder is TMP_Text p ? Plain(p.text) : Plain(typed);
            case Slider _:
            case Toggle _:
                // El rótulo va al lado (fila de Ajustes): el texto hermano más cercano
                TMP_Text sibling = s.GetComponentInChildren<TMP_Text>(true) ?? s.transform.parent.GetComponentInChildren<TMP_Text>(true);
                return sibling != null ? Plain(sibling.GetParsedText()) : s.name;
            default:
                TMP_Text label = s.GetComponentInChildren<TMP_Text>(true);
                string text = label != null ? Plain(label.GetParsedText()) : "";
                return text.Length > 0 ? text : s.name.Replace("(auto)", "").Trim();
        }
    }

    private static string ValueOf(Selectable s)
    {
        switch (s)
        {
            case Slider slider: return Mathf.RoundToInt(Mathf.InverseLerp(slider.minValue, slider.maxValue, slider.value) * 100f) + " %";
            case Toggle toggle: return toggle.isOn ? "activado" : "desactivado";
            default: return null;
        }
    }

    // ---------- Jerarquía ----------

    private AccessibilityHierarchy Build(List<Item> items)
    {
        actions.Clear();
        steppers.Clear();
        var hierarchy = new AccessibilityHierarchy();
        foreach (Item item in items)
        {
            AccessibilityNode node = hierarchy.AddNode(item.label);
            node.role = item.role;
            node.value = item.value;
            node.state = item.disabled ? AccessibilityState.Disabled : AccessibilityState.None;
            RectTransform rect = item.rect;
            Camera camera = item.camera;
            node.frameGetter = () => rect != null ? ScreenRect(rect, camera) : Rect.zero;
            node.frame = ScreenRect(rect, camera);

            System.Action action = ActionFor(item.control);
            if (action != null)
            {
                actions[node.id] = action;
                node.invoked += () => { action(); return true; };
            }
            if (item.control is Slider slider)
            {
                float step = (slider.maxValue - slider.minValue) * 0.1f;
                node.incremented += () => slider.value = Mathf.Min(slider.maxValue, slider.value + step);
                node.decremented += () => slider.value = Mathf.Max(slider.minValue, slider.value - step);
            }
        }
        return hierarchy;
    }

    private static System.Action ActionFor(Selectable control)
    {
        switch (control)
        {
            case null: return null;
            case Button b: return () => { if (b != null && b.interactable) b.onClick.Invoke(); };
            case Toggle t: return () => { if (t != null && t.interactable) t.isOn = !t.isOn; };
            case TMP_Dropdown d: return () => { if (d != null && d.interactable) d.Show(); };
            case TMP_InputField f: return () => { if (f != null && f.interactable) f.ActivateInputField(); };
            default: return null;
        }
    }

    // Coordenadas de pantalla con el origen arriba a la izquierda (las del lector)
    private static Rect ScreenRect(RectTransform rect, Camera camera)
    {
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        Vector2 min = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
        return new Rect(min.x, Screen.height - max.y, max.x - min.x, max.y - min.y);
    }

    private static string Signature(List<Item> items)
    {
        var sb = new StringBuilder();
        foreach (Item i in items)
            sb.Append(i.role).Append('|').Append(i.label).Append('|').Append(i.value).Append('|').Append(i.disabled).Append('\n');
        return sb.ToString();
    }

    private static string FirstLabel(List<Item> items) => items.Count > 0 ? items[0].label : "";

    private static string FirstLabelOf(string signature)
    {
        int line = signature.IndexOf('\n');
        string first = line >= 0 ? signature.Substring(0, line) : signature;
        string[] parts = first.Split('|');
        return parts.Length > 1 ? parts[1] : "";
    }

    // Sin etiquetas de formato ni saltos de línea: el lector lo lee de corrido
    private static string Plain(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";
        string noTags = System.Text.RegularExpressions.Regex.Replace(text, "<[^>]+>", "");
        return System.Text.RegularExpressions.Regex.Replace(noTags, @"\s+", " ").Trim();
    }
}
