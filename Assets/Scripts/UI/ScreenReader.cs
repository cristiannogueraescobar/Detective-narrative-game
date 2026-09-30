using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Lector de pantalla (TalkBack en Android, VoiceOver en iOS) con el módulo de accesibilidad de Unity 6.
/// Solo trabaja si el lector del sistema está activado: entonces describe lo que se ve (botones, desplegables,
/// casillas, deslizadores, campos, zonas desplazables y textos, en orden de lectura; lo tapado por otra pantalla o
/// transparente no cuenta) en un AccessibilityHierarchy. Cada nodo va atado a su objeto: si solo cambian textos o
/// valores se actualiza en el sitio (el lector no pierde el foco); si aparecen o desaparecen elementos se insertan
/// o se quitan solo esos. "Pantalla nueva" (el foco vuelve arriba) solo cuando cambia el primer control, no cuando
/// cambia un texto como el HUD. Los momentos importantes se anuncian con Announce.
/// Sin lector activado no hace nada ni cuesta nada. Requisito del módulo en Android: API 26.
/// </summary>
public class ScreenReader : MonoBehaviour
{
    public const float RefreshSeconds = 0.5f;
    private const int MaxChatTexts = 40;          // Un chat largo: los últimos mensajes
    private const float AnnouncementGrace = 1.5f; // Tras un anuncio no se manda el foco arriba (lo cortaría)

    private static ScreenReader instance;

    // Jerarquía viva: un nodo por objeto (id de instancia), en orden de lectura
    private readonly List<int> order = new List<int>();
    private readonly Dictionary<int, AccessibilityNode> nodes = new Dictionary<int, AccessibilityNode>();
    private readonly Dictionary<int, Item> bound = new Dictionary<int, Item>();
    private AccessibilityHierarchy hierarchy;
    private int firstControl;
    private float next;
    private static float lastAnnouncement = -10f;

    /// <summary>Veces que se ha avisado de "pantalla nueva" (para tests).</summary>
    public static int ScreenChanges { get; private set; }

    /// <summary>Último anuncio enviado (para tests).</summary>
    public static string LastAnnouncement { get; private set; }

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

    private void OnDestroy()
    {
        if (hierarchy != null && AssistiveSupport.activeHierarchy == hierarchy)
            AssistiveSupport.activeHierarchy = null;
        if (instance == this)
            instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetState();
        next = 0f;
    }

    private void ResetState()
    {
        order.Clear();
        nodes.Clear();
        bound.Clear();
        hierarchy = null;
        firstControl = 0;
    }

    /// <summary>
    /// Anuncia un momento importante (solo si el lector está activado).
    /// </summary>
    public static void Announce(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || !AssistiveSupport.isScreenReaderEnabled)
            return;
        LastAnnouncement = Plain(text);
        lastAnnouncement = Time.unscaledTime;
        AssistiveSupport.notificationDispatcher?.SendAnnouncement(LastAnnouncement);
    }

    /// <summary>
    /// Lo que hace el lector al activar un nodo (doble toque en TalkBack). Público para los tests.
    /// </summary>
    public static bool Invoke(AccessibilityNode node)
    {
        if (instance == null || node == null)
            return false;
        foreach (KeyValuePair<int, AccessibilityNode> pair in instance.nodes)
        {
            if (ReferenceEquals(pair.Value, node) && instance.bound.TryGetValue(pair.Key, out Item item))
                return item.link != null ? ActivateLink(item) : Activate(item.control);
        }
        return false;
    }

    /// <summary>
    /// Deslizar arriba/abajo sobre un deslizador (lo que hace el lector). Público para los tests.
    /// </summary>
    public static bool Step(AccessibilityNode node, int sign)
    {
        if (instance == null || node == null)
            return false;
        foreach (KeyValuePair<int, AccessibilityNode> pair in instance.nodes)
        {
            if (ReferenceEquals(pair.Value, node))
                return instance.Step(pair.Key, sign);
        }
        return false;
    }

    private void Update()
    {
        if (!AssistiveSupport.isScreenReaderEnabled)
        {
            if (hierarchy != null)
            {
                if (AssistiveSupport.activeHierarchy == hierarchy)
                    AssistiveSupport.activeHierarchy = null;
                ResetState();
            }
            return;
        }
        if (Time.unscaledTime < next)
            return;
        next = Time.unscaledTime + RefreshSeconds;
        Sync(Collect());
    }

    // ---------- Qué se ve ----------

    private struct Item
    {
        public int id;
        public RectTransform rect;
        public Camera camera;
        public float sortKey;
        public AccessibilityRole role;
        public string label;
        public string value;
        public bool disabled;
        public Selectable control;
        public ScrollRect scroll;
        public bool inChat;
        public TextLinkHandler link; // Enlace dentro de un texto (la libreta): un botón más para el lector
        public int linkIndex;
        public string linkId;
    }

    private static readonly List<Item> items = new List<Item>();
    private static readonly List<Item> texts = new List<Item>();
    private static readonly HashSet<int> usedLabels = new HashSet<int>();
    private static readonly List<Selectable> selectables = new List<Selectable>();
    private static readonly List<TMP_Text> tmpTexts = new List<TMP_Text>();
    private static readonly List<ScrollRect> scrolls = new List<ScrollRect>();

    private static List<Item> Collect()
    {
        items.Clear();
        texts.Clear();
        usedLabels.Clear();
        foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (!canvas.isRootCanvas || !canvas.isActiveAndEnabled)
                continue;
            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            canvas.GetComponentsInChildren(false, selectables);
            foreach (Selectable s in selectables)
            {
                if (s is Scrollbar)
                    continue;
                var rect = (RectTransform)s.transform;
                if (!Visible(rect, camera, 1f, true, out Rect frame))
                    continue;
                items.Add(new Item
                {
                    id = s.GetInstanceID(), rect = rect, camera = camera, sortKey = SortKey(frame),
                    role = RoleOf(s), label = LabelOf(s), value = ValueOf(s), disabled = !s.interactable, control = s
                });
            }

            // Zonas desplazables (el chat, el informe final): el lector las mueve con su gesto de desplazar
            canvas.GetComponentsInChildren(false, scrolls);
            foreach (ScrollRect scroll in scrolls)
            {
                var rect = (RectTransform)scroll.transform;
                if (!scroll.vertical || !Visible(rect, camera, 1f, false, out Rect frame))
                    continue;
                items.Add(new Item
                {
                    id = scroll.GetInstanceID(), rect = rect, camera = camera, sortKey = SortKey(frame) - 0.5f,
                    role = AccessibilityRole.ScrollView, label = "Zona desplazable", scroll = scroll
                });
            }

            canvas.GetComponentsInChildren(false, tmpTexts);
            foreach (TMP_Text t in tmpTexts)
            {
                if (usedLabels.Contains(t.GetInstanceID()) || t.GetComponentInParent<Selectable>() != null
                    || t.GetComponentInParent<Decorative>() != null)
                    continue;
                if (!Visible(t.rectTransform, camera, t.color.a, true, out Rect frame))
                    continue;
                string text = Plain(t.GetParsedText());
                if (text.Length == 0)
                    continue;
                texts.Add(new Item
                {
                    id = t.GetInstanceID(), rect = t.rectTransform, camera = camera, sortKey = SortKey(frame),
                    role = AccessibilityRole.StaticText, label = text, inChat = t.GetComponentInParent<ScrollRect>() != null
                });
                AddLinks(t, camera);
            }
        }

        items.AddRange(texts);
        items.Sort((a, b) => a.sortKey.CompareTo(b.sortKey));

        // Chat muy largo: fuera los mensajes más antiguos (nunca títulos ni el HUD)
        int chat = 0;
        foreach (Item i in items)
            if (i.inChat)
                chat++;
        for (int k = 0; k < items.Count && chat > MaxChatTexts; k++)
        {
            if (!items[k].inChat)
                continue;
            items.RemoveAt(k--);
            chat--;
        }
        return items;
    }

    // Cada enlace tocable del texto (ir a interrogar, tu nota) es un botón: con el dedo se toca la palabra; con el
    // lector, esto es lo único que lo hace posible
    private static void AddLinks(TMP_Text t, Camera camera)
    {
        if (!t.TryGetComponent(out TextLinkHandler handler) || handler.onLink == null)
            return;
        TMP_TextInfo info = t.textInfo;
        RectMask2D mask = t.GetComponentInParent<RectMask2D>();
        Rect view = mask != null ? ScreenRect(mask.rectTransform, camera) : new Rect(0, 0, Screen.width, Screen.height);
        for (int i = 0; i < info.linkCount; i++)
        {
            TMP_LinkInfo link = info.linkInfo[i];
            Rect frame = LinkRect(t, i, camera);
            if (frame.width < 1f || !frame.Overlaps(view) || !frame.Overlaps(new Rect(0, 0, Screen.width, Screen.height)))
                continue;
            string id = link.GetLinkID();
            texts.Add(new Item
            {
                id = (t.GetInstanceID() * 397) ^ id.GetHashCode(), rect = t.rectTransform, camera = camera,
                sortKey = SortKey(frame) + 0.25f, role = AccessibilityRole.Button,
                label = Plain(handler.Describe(id, link.GetLinkText())), link = handler, linkIndex = i, linkId = id
            });
        }
    }

    // Caja de pantalla (origen arriba) de los caracteres del enlace
    private static Rect LinkRect(TMP_Text t, int index, Camera camera)
    {
        TMP_TextInfo info = t.textInfo;
        if (index < 0 || index >= info.linkCount)
            return Rect.zero;
        TMP_LinkInfo link = info.linkInfo[index];
        float xMin = float.MaxValue, yMin = float.MaxValue, xMax = float.MinValue, yMax = float.MinValue;
        for (int c = link.linkTextfirstCharacterIndex; c < link.linkTextfirstCharacterIndex + link.linkTextLength && c < info.characterCount; c++)
        {
            TMP_CharacterInfo ch = info.characterInfo[c];
            if (!ch.isVisible)
                continue;
            Vector2 min = RectTransformUtility.WorldToScreenPoint(camera, t.transform.TransformPoint(new Vector3(ch.bottomLeft.x, ch.descender)));
            Vector2 max = RectTransformUtility.WorldToScreenPoint(camera, t.transform.TransformPoint(new Vector3(ch.topRight.x, ch.ascender)));
            xMin = Mathf.Min(xMin, min.x);
            yMin = Mathf.Min(yMin, min.y);
            xMax = Mathf.Max(xMax, max.x);
            yMax = Mathf.Max(yMax, max.y);
        }
        return xMax < xMin ? Rect.zero : new Rect(xMin, Screen.height - yMax, xMax - xMin, yMax - yMin);
    }

    private static bool ActivateLink(Item item)
    {
        if (item.link == null || !item.link.isActiveAndEnabled || item.link.onLink == null)
            return false;
        item.link.onLink(item.linkId);
        return true;
    }

    // De arriba abajo y de izquierda a derecha (filas de 24 px de tolerancia)
    private static float SortKey(Rect frame) => Mathf.Round(frame.y / 24f) * 100000f + frame.x;

    private static bool Visible(RectTransform rect, Camera camera, float ownAlpha, bool occlusion, out Rect frame)
    {
        frame = default;
        if (!rect.gameObject.activeInHierarchy || ownAlpha < 0.05f || GroupAlpha(rect) < 0.05f)
            return false;
        frame = ScreenRect(rect, camera);
        if (frame.width < 1f || frame.height < 1f || !frame.Overlaps(new Rect(0, 0, Screen.width, Screen.height)))
            return false;
        // Recortado por una máscara (texto del chat fuera de la vista)
        RectMask2D mask = rect.GetComponentInParent<RectMask2D>();
        if (mask != null && mask.transform != rect && !ScreenRect(mask.rectTransform, camera).Overlaps(frame))
            return false;
        return !occlusion || !Covered(rect, frame);
    }

    // Opacidad de todos los CanvasGroup de encima (el botón tiene el suyo; el panel que entra con fundido, otro)
    private static float GroupAlpha(Transform t)
    {
        float alpha = 1f;
        for (Transform p = t; p != null; p = p.parent)
        {
            if (!p.TryGetComponent(out CanvasGroup group) || !group.enabled)
                continue;
            alpha *= group.alpha;
            if (group.ignoreParentGroups)
                break;
        }
        return alpha;
    }

    private static readonly List<RaycastResult> hits = new List<RaycastResult>();
    private static PointerEventData probe;
    private static EventSystem probeSystem;

    // ¿Lo tapa otra cosa (el menú debajo de la selección de caso, la pantalla debajo de un aviso)? Lo de encima en
    // su centro tiene que ser él mismo, algo suyo o algo de lo que forma parte (su propio panel)
    private static bool Covered(RectTransform rect, Rect frame)
    {
        EventSystem events = EventSystem.current;
        if (events == null)
            return false;
        if (probe == null || probeSystem != events)
        {
            probe = new PointerEventData(events);
            probeSystem = events;
        }
        probe.position = new Vector2(frame.center.x, Screen.height - frame.center.y); // Vuelta a origen abajo
        hits.Clear();
        events.RaycastAll(probe, hits);
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
                // El rótulo va dentro (casilla) o al lado (fila de Ajustes); ese texto ya no se lee aparte
                TMP_Text label = s.GetComponentInChildren<TMP_Text>(true);
                // Fila de Ajustes: el rótulo es el texto justo encima (el hermano anterior más cercano)
                if (label == null && s.transform.parent != null)
                {
                    for (int k = s.transform.GetSiblingIndex() - 1; k >= 0 && label == null; k--)
                        s.transform.parent.GetChild(k).TryGetComponent(out label);
                }
                if (label == null)
                    return s.name;
                usedLabels.Add(label.GetInstanceID());
                return Plain(label.GetParsedText());
            default:
                TMP_Text caption = s.GetComponentInChildren<TMP_Text>(true);
                string text = caption != null ? Plain(caption.GetParsedText()) : "";
                return text.Length > 0 ? text : s.name.Replace("(auto)", "").Trim();
        }
    }

    private static string ValueOf(Selectable s)
    {
        switch (s)
        {
            case Slider slider: return Mathf.RoundToInt(slider.normalizedValue * 100f) + " %";
            case Toggle toggle: return toggle.isOn ? "activado" : "desactivado";
            default: return null;
        }
    }

    // ---------- Jerarquía ----------

    private readonly HashSet<int> alive = new HashSet<int>();

    private void Sync(List<Item> current)
    {
        bool structureChanged = hierarchy == null || current.Count != order.Count;
        for (int k = 0; !structureChanged && k < current.Count; k++)
            structureChanged = current[k].id != order[k];

        int newFirst = 0;
        foreach (Item i in current)
        {
            if (i.control != null)
            {
                newFirst = i.id;
                break;
            }
        }

        if (hierarchy == null)
            hierarchy = new AccessibilityHierarchy();
        if (AssistiveSupport.activeHierarchy != hierarchy)
            AssistiveSupport.activeHierarchy = hierarchy;

        if (structureChanged)
        {
            // Fuera lo que ya no está; dentro lo nuevo, en su sitio; lo demás se conserva (y con ello el foco)
            alive.Clear();
            foreach (Item i in current)
                alive.Add(i.id);
            for (int k = order.Count - 1; k >= 0; k--)
            {
                if (alive.Contains(order[k]))
                    continue;
                if (nodes.TryGetValue(order[k], out AccessibilityNode gone) && hierarchy.ContainsNode(gone))
                    hierarchy.RemoveNode(gone);
                nodes.Remove(order[k]);
                bound.Remove(order[k]);
                order.RemoveAt(k);
            }
            for (int index = 0; index < current.Count; index++)
            {
                Item item = current[index];
                if (nodes.ContainsKey(item.id))
                    continue;
                bound[item.id] = item;
                AccessibilityNode node = hierarchy.InsertNode(index, item.label, null);
                Wire(node, item);
                nodes[item.id] = node;
            }
            order.Clear();
            foreach (Item i in current)
                order.Add(i.id);
        }

        // Textos, valores y estados al día sin tocar la estructura
        foreach (Item item in current)
        {
            bound[item.id] = item;
            AccessibilityNode node = nodes[item.id];
            if (node.label != item.label)
                node.label = item.label;
            if (node.value != item.value)
                node.value = item.value;
            AccessibilityState state = item.disabled ? AccessibilityState.Disabled : AccessibilityState.None;
            if (node.state != state)
                node.state = state;
        }
        hierarchy.RefreshNodeFrames();

        bool newScreen = firstControl != 0 && newFirst != firstControl
                         && Time.unscaledTime - lastAnnouncement > AnnouncementGrace;
        firstControl = newFirst;
        if (newScreen)
        {
            ScreenChanges++;
            AssistiveSupport.notificationDispatcher?.SendScreenChanged(null);
        }
        else if (structureChanged)
        {
            AssistiveSupport.notificationDispatcher?.SendLayoutChanged(null);
        }
    }

    private void Wire(AccessibilityNode node, Item item)
    {
        node.role = item.role;
        int id = item.id;
        node.frameGetter = () => !bound.TryGetValue(id, out Item it) || it.rect == null ? Rect.zero
            : it.link != null ? LinkRect(it.link.GetComponent<TMP_Text>(), it.linkIndex, it.camera)
            : ScreenRect(it.rect, it.camera);
        if (item.link != null)
            node.invoked += () => bound.TryGetValue(id, out Item it) && ActivateLink(it);
        if (item.control != null)
            node.invoked += () => bound.TryGetValue(id, out Item it) && Activate(it.control);
        if (item.control is Slider)
        {
            node.incremented += () => Step(id, +1);
            node.decremented += () => Step(id, -1);
        }
        if (item.scroll != null)
            node.scrolled += direction => Scroll(id, direction);
    }

    private static bool Activate(Selectable control)
    {
        if (control == null || !control.IsActive() || !control.interactable)
            return false;
        switch (control)
        {
            case Button b: b.onClick.Invoke(); return true;
            case Toggle t: t.isOn = !t.isOn; return true;
            case TMP_Dropdown d: d.Show(); return true;
            case TMP_InputField f: f.ActivateInputField(); return true;
            default: return false;
        }
    }

    private bool Step(int id, int sign)
    {
        if (!bound.TryGetValue(id, out Item it) || !(it.control is Slider slider) || slider == null || !slider.interactable)
            return false;
        slider.normalizedValue = Mathf.Clamp01(slider.normalizedValue + 0.1f * sign);
        return true;
    }

    // Una pantalla por gesto: "adelante"/"abajo" muestra lo siguiente, "atrás"/"arriba" lo anterior
    private bool Scroll(int id, AccessibilityScrollDirection direction)
    {
        if (!bound.TryGetValue(id, out Item it) || it.scroll == null)
            return false;
        ScrollRect scroll = it.scroll;
        float content = scroll.content != null ? scroll.content.rect.height : 0f;
        float view = ((RectTransform)scroll.transform).rect.height;
        if (content <= view)
            return false;
        float page = view / (content - view);
        float delta;
        switch (direction)
        {
            case AccessibilityScrollDirection.Down:
            case AccessibilityScrollDirection.Forward: delta = -page; break;
            case AccessibilityScrollDirection.Up:
            case AccessibilityScrollDirection.Backward: delta = page; break;
            default: return false;
        }
        scroll.verticalNormalizedPosition = Mathf.Clamp01(scroll.verticalNormalizedPosition + delta);
        next = 0f; // Lo que entra en la vista, en la próxima pasada
        return true;
    }

    private static readonly Vector3[] corners = new Vector3[4];

    // Coordenadas de pantalla con el origen arriba a la izquierda (como worldBound de UI Toolkit en el manual)
    private static Rect ScreenRect(RectTransform rect, Camera camera)
    {
        rect.GetWorldCorners(corners);
        Vector2 min = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
        return new Rect(min.x, Screen.height - max.y, max.x - min.x, max.y - min.y);
    }

    private static readonly Regex Tags = new Regex("<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex Spaces = new Regex(@"\s+", RegexOptions.Compiled);

    // Sin etiquetas de formato ni saltos de línea: el lector lo lee de corrido
    private static string Plain(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";
        return Spaces.Replace(Tags.Replace(text, ""), " ").Trim();
    }
}
