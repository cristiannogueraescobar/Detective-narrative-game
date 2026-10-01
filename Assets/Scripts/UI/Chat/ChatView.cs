using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// CHAT DEL INTERROGATORIO
/// Pinta las entradas de la conversación abierta dentro del ScrollRect:
///   - burbujas: el detective a la derecha, el sospechoso a la izquierda con su mini-retrato, hora del juego;
///   - avisos centrados (cambio de día, pistas, contradicciones, errores);
///   - "escribiendo…" mientras responde la IA y efecto de máquina de escribir (un toque lo completa);
///   - baja sola al último mensaje solo si el jugador ya estaba abajo; si ha subido a leer, se respeta
///     y aparece "Nuevos mensajes".
/// Las filas se reutilizan (pool): cambiar de sospechoso no crea ni destruye objetos.
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class ChatView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IScrollHandler
{
    public const float MaxBubbleFraction = 0.8f;   // Ancho máximo de una burbuja respecto a la fila
    public const float AvatarSize = 88f;

    private enum RowKind { Bubble, Notice, Day }

    private class Row
    {
        public RowKind kind;
        public GameObject root;
        public HorizontalLayoutGroup layout;
        public GameObject avatar;
        public RawImage face;
        public Image background;
        public TMP_Text meta;
        public TMP_Text evidence;
        public TMP_Text body;
        public TMP_Text heading;
        public Typewriter typewriter;
        public ChatEntry entry;
    }

    private ScrollRect scroll;
    private RectTransform content;
    private readonly List<ChatEntry> shown = new List<ChatEntry>();
    private readonly List<Row> rows = new List<Row>();
    private readonly Dictionary<RowKind, Stack<Row>> pool = new Dictionary<RowKind, Stack<Row>>();
    private Row typingRow;
    private Row typingTarget;
    private Coroutine typingDots;
    private Coroutine follow;
    private GameObject newMessages;
    private Texture avatarTexture;
    private Rect avatarFace = new Rect(0f, 0f, 1f, 1f);
    private bool avatarLegacy;
    private string typingSpeaker;
    private bool stick = true;

    public bool IsTyping => typingTarget != null && typingTarget.typewriter != null && typingTarget.typewriter.IsTyping;

    public int RowCount => rows.Count;

    /// <summary>
    /// Filas creadas en total (para comprobar que el pool reutiliza).
    /// </summary>
    public int CreatedRows { get; private set; }

    private static Theme T => ThemeManager.Current;

    public void Initialize()
    {
        if (scroll != null)
            return;

        scroll = GetComponent<ScrollRect>();
        content = scroll.content;
        scroll.onValueChanged.AddListener(_ => OnScrolled());
        BuildNewMessagesButton();
    }

    // ---------- Entradas ----------

    /// <summary>
    /// Muestra la conversación. Solo se añaden las entradas nuevas si la anterior es un prefijo
    /// (lo normal); si no (otro sospechoso), se rehace con filas del pool.
    /// 'typeLast': la última respuesta del sospechoso se escribe letra a letra.
    /// </summary>
    public void Show(IReadOnlyList<ChatEntry> entries, bool typeLast = false, float speed = 1f)
    {
        Initialize();

        int common = 0;
        while (common < shown.Count && common < entries.Count && ReferenceEquals(shown[common], entries[common]))
            common++;

        bool rebuilt = common < shown.Count;
        if (rebuilt)
        {
            Complete();
            for (int i = rows.Count - 1; i >= common; i--)
                Release(rows[i]);
            rows.RemoveRange(common, rows.Count - common);
            shown.RemoveRange(common, shown.Count - common);
            stick = true;
        }

        Row lastAdded = null;
        for (int i = common; i < entries.Count; i++)
        {
            lastAdded = Acquire(entries[i]);
            rows.Add(lastAdded);
            shown.Add(entries[i]);
            if (!rebuilt && Application.isPlaying)
                Appear(lastAdded);
        }

        if (typingRow != null)
            typingRow.root.transform.SetAsLastSibling();

        bool added = lastAdded != null;
        if (typeLast && lastAdded != null && lastAdded.entry.kind == ChatEntryKind.Suspect)
            StartTyping(lastAdded, speed);
        else if (added || rebuilt)
            AfterContentChanged(added && !rebuilt);
    }

    /// <summary>
    /// Descarta todas las filas (también las del pool) para que se vuelvan a crear con el tema actual.
    /// </summary>
    public void Restyle()
    {
        Complete();
        foreach (Row row in rows)
            Destroy(row.root);
        foreach (Stack<Row> free in pool.Values)
            foreach (Row row in free)
                Destroy(row.root);
        rows.Clear();
        pool.Clear();
        shown.Clear();
        if (typingRow != null)
        {
            Destroy(typingRow.root);
            typingRow = null;
        }
        if (newMessages != null)
        {
            Destroy(newMessages);
            BuildNewMessagesButton();
        }
    }

    private new static void Destroy(Object target)
    {
        if (Application.isPlaying)
            Object.Destroy(target);
        else
            Object.DestroyImmediate(target);
    }

    /// <summary>
    /// Mini-retrato de las respuestas (el del sospechoso de esta conversación). El arte antiguo lleva la misma
    /// gradación que el retrato grande (ArtGrading); el nuevo se ve tal cual.
    /// </summary>
    public void SetAvatar(Texture texture, Rect? face = null, bool legacyArt = false)
    {
        avatarTexture = texture;
        avatarLegacy = legacyArt && texture != null;
        avatarFace = face ?? UISprites.FaceCrop(texture);
        foreach (Row row in rows)
            ApplyAvatar(row);
        if (typingRow != null)
            ApplyAvatar(typingRow);
    }

    // ---------- Escribiendo… ----------

    public void ShowTyping(bool show, string speaker = null)
    {
        Initialize();
        typingSpeaker = speaker;

        if (!show)
        {
            if (typingDots != null)
                StopCoroutine(typingDots);
            typingDots = null;
            if (typingRow != null)
                typingRow.root.SetActive(false);
            return;
        }

        if (typingRow == null)
        {
            typingRow = CreateBubbleRow();
            typingRow.root.name = "Escribiendo (auto)";
        }

        typingRow.entry = ChatEntry.Suspect(speaker, "", null);
        ConfigureBubble(typingRow, typingRow.entry);
        typingRow.body.richText = true; // Los puntos animados son nuestros
        typingRow.meta.text = string.IsNullOrEmpty(speaker) ? "escribiendo…" : $"{speaker.ToUpperInvariant()} · escribiendo…";
        typingRow.body.text = Dots(1f);
        typingRow.root.SetActive(true);
        typingRow.root.transform.SetAsLastSibling();

        if (Application.isPlaying && isActiveAndEnabled)
        {
            if (typingDots != null)
                StopCoroutine(typingDots);
            typingDots = StartCoroutine(AnimateDots());
        }
        AfterContentChanged(true);
    }

    // Tres puntos que se encienden por turnos
    private static string Dots(float phase)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < 3; i++)
        {
            float a = GameSettings.ReduceMotion ? 0.8f : 0.3f + 0.7f * Mathf.Clamp01(1f - Mathf.Abs(Mathf.Repeat(phase - i * 0.33f, 1f) - 0.5f) * 2f);
            sb.Append($"<alpha=#{(int)(a * 255):X2}><size=160%>•</size> ");
        }
        return sb.ToString().TrimEnd();
    }

    private IEnumerator AnimateDots()
    {
        float phase = 0f;
        var wait = new WaitForSecondsRealtime(0.125f);
        while (typingRow != null && typingRow.root.activeSelf)
        {
            phase += 0.125f * 1.4f;
            typingRow.body.text = Dots(phase); // 8 veces por segundo basta y no rehace el chat en cada fotograma
            yield return wait;
        }
        typingDots = null;
    }

    // ---------- Máquina de escribir ----------

    private void StartTyping(Row row, float speed)
    {
        Complete();
        typingTarget = row;

        if (!Application.isPlaying || row.typewriter == null)
        {
            AfterContentChanged(true);
            return;
        }

        Canvas.ForceUpdateCanvases();
        row.typewriter.Reveal(0, speed);
        if (follow != null)
            StopCoroutine(follow);
        follow = StartCoroutine(FollowTyping(row));
        if (!stick)
            SetNewMessages(true);
    }

    /// <summary>
    /// Completa al momento la respuesta que se está escribiendo.
    /// </summary>
    public void Complete()
    {
        if (typingTarget != null && typingTarget.typewriter != null)
            typingTarget.typewriter.Complete();
        typingTarget = null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsTyping)
            Complete();
    }

    // El scroll sigue a la línea que se escribe (el final de la burbuja aún está en blanco)
    private IEnumerator FollowTyping(Row row)
    {
        yield return null;
        int lastTick = 0;
        while (row.typewriter != null && row.typewriter.IsTyping)
        {
            // Tecleo de la máquina cada pocas letras
            int visible = row.body.maxVisibleCharacters;
            if (visible - lastTick >= 6)
            {
                lastTick = visible;
                SoundManager.Play(Sfx.Typing, 0.25f, 0.08f);
            }
            if (stick)
                KeepVisible(row.body, visible);
            yield return null;
        }
        follow = null;
        if (stick)
            ScrollToBottom();
    }

    private void KeepVisible(TMP_Text text, int visibleCharacters)
    {
        TMP_TextInfo info = text.textInfo;
        if (info == null || info.characterCount == 0)
            return;

        int index = Mathf.Clamp(visibleCharacters - 1, 0, info.characterCount - 1);
        RectTransform viewport = scroll.viewport != null ? scroll.viewport : (RectTransform)scroll.transform;
        Vector3 world = text.rectTransform.TransformPoint(new Vector3(0f, info.characterInfo[index].descender, 0f));
        float fromTop = content.rect.yMax - content.InverseTransformPoint(world).y;

        float scrollable = content.rect.height - viewport.rect.height;
        if (scrollable <= 0f)
            return;

        float target = Mathf.Clamp(fromTop - viewport.rect.height + T.bodySize, 0f, scrollable);
        float current = (1f - scroll.verticalNormalizedPosition) * scrollable;
        if (target > current) // Nunca sube: solo acompaña hacia abajo
            scroll.verticalNormalizedPosition = 1f - target / scrollable;
    }

    // ---------- Scroll ----------

    public void OnBeginDrag(PointerEventData eventData)
    {
        stick = false; // El jugador toma el control; vuelve a pegarse si baja del todo
    }

    public void OnScroll(PointerEventData eventData)
    {
        stick = false;
    }

    private void OnScrolled()
    {
        if (content == null || scroll.viewport == null)
            return;
        if (ChatScrollPolicy.IsAtBottom(scroll.verticalNormalizedPosition, content.rect.height, scroll.viewport.rect.height) && !IsTyping)
        {
            stick = true;
            SetNewMessages(false);
        }
    }

    private void AfterContentChanged(bool newContent)
    {
        if (stick)
            ScrollToBottom();
        else if (newContent)
            SetNewMessages(true);
    }

    public void ScrollToBottom()
    {
        stick = true;
        SetNewMessages(false);

        if (Application.isPlaying && isActiveAndEnabled)
        {
            StartCoroutine(ScrollToBottomNextFrame());
            return;
        }

        if (content != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            scroll.verticalNormalizedPosition = 0f;
        }
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        if (stick && !IsTyping)
            scroll.verticalNormalizedPosition = 0f;
    }

    private void BuildNewMessagesButton()
    {
        // Zona táctil de 48 dp con una píldora visible más pequeña dentro
        RectTransform hit = UIFactory.Container(transform, "NuevosMensajes (auto)", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        hit.pivot = new Vector2(0.5f, 0f);
        hit.sizeDelta = new Vector2(420f, Theme.MinTouchSize);
        hit.anchoredPosition = new Vector2(0f, 8f);
        UIComponents.GetOrAdd<LayoutElement>(hit.gameObject).ignoreLayout = true;
        var hitImage = hit.gameObject.AddComponent<Image>();
        hitImage.color = Color.clear;

        RectTransform pill = UIFactory.Container(hit, "Pildora", new Vector2(0f, 0.5f), new Vector2(1f, 0.5f));
        pill.sizeDelta = new Vector2(0f, 72f);
        var pillImage = pill.gameObject.AddComponent<Image>();
        pillImage.sprite = UISprites.Rounded(ThemeManager.Current.RadiusPill);
        pillImage.type = Image.Type.Sliced;
        pillImage.color = T.accent;
        pillImage.raycastTarget = false;

        TMP_Text label = NewText(pill, "Texto", T.secondarySize, T.buttonPrimaryText, TextAlignmentOptions.Center);
        label.text = "Nuevos mensajes";
        Stretch(label.rectTransform, 16f, 0f);
        TextStyle.Set(label, TextStyle.Mode.OneLine, T.secondarySize);

        var button = hit.gameObject.AddComponent<Button>();
        button.targetGraphic = pillImage;
        button.onClick.AddListener(ScrollToBottom);
        UIComponents.GetOrAdd<ThemeRole>(hit.gameObject).role = UIRole.Ignore;

        newMessages = hit.gameObject;
        newMessages.SetActive(false);
    }

    private void SetNewMessages(bool visible)
    {
        if (newMessages != null && newMessages.activeSelf != visible)
        {
            newMessages.SetActive(visible);
            newMessages.transform.SetAsLastSibling();
        }
    }

    // ---------- Filas ----------

    private Row Acquire(ChatEntry entry)
    {
        RowKind kind = entry.IsBubble ? RowKind.Bubble : entry.kind == ChatEntryKind.Day ? RowKind.Day : RowKind.Notice;
        if (!pool.TryGetValue(kind, out Stack<Row> free))
            pool[kind] = free = new Stack<Row>();

        Row row = free.Count > 0 ? free.Pop() : Create(kind);
        row.root.SetActive(true);
        row.root.transform.SetAsLastSibling();
        row.root.transform.localScale = Vector3.one;
        if (row.root.TryGetComponent(out CanvasGroup group))
            group.alpha = 1f;
        row.entry = entry;

        if (kind == RowKind.Bubble)
            ConfigureBubble(row, entry);
        else if (kind == RowKind.Day)
            ConfigureDay(row, entry);
        else
            ConfigureNotice(row, entry);
        return row;
    }

    private void Release(Row row)
    {
        if (row.typewriter != null)
            row.typewriter.Complete();
        row.entry = null;
        row.root.SetActive(false);
        pool[row.kind].Push(row);
    }

    private Row Create(RowKind kind)
    {
        CreatedRows++;
        switch (kind)
        {
            case RowKind.Bubble: return CreateBubbleRow();
            case RowKind.Day: return CreateDayRow();
            default: return CreateNoticeRow();
        }
    }

    private Row CreateBubbleRow()
    {
        var row = new Row { kind = RowKind.Bubble };
        RectTransform root = NewRect(content, "Burbuja (auto)");
        row.root = root.gameObject;
        row.layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
        row.layout.spacing = 16f;
        row.layout.childControlWidth = row.layout.childControlHeight = true;
        row.layout.childForceExpandWidth = row.layout.childForceExpandHeight = false;

        // Mini-retrato redondo con aro
        RectTransform avatar = NewRect(root, "Avatar");
        var avatarElement = avatar.gameObject.AddComponent<LayoutElement>();
        avatarElement.minWidth = avatarElement.preferredWidth = AvatarSize;
        avatarElement.minHeight = avatarElement.preferredHeight = AvatarSize;
        var ring = avatar.gameObject.AddComponent<Image>();
        ring.sprite = UISprites.Circle();
        ring.color = T.suspectName;
        ring.raycastTarget = false;
        avatar.gameObject.AddComponent<Mask>().showMaskGraphic = true;
        RectTransform faceRect = NewRect(avatar, "Cara");
        Stretch(faceRect, 4f, 4f);
        row.face = faceRect.gameObject.AddComponent<RawImage>();
        row.face.raycastTarget = false;
        row.avatar = avatar.gameObject;

        // Burbuja: fondo redondeado que se ajusta al texto
        RectTransform bubble = NewRect(root, "Globo");
        row.background = bubble.gameObject.AddComponent<Image>();
        row.background.sprite = UISprites.Rounded(ThemeManager.Current.RadiusLarge);
        row.background.type = Image.Type.Sliced;
        var inner = bubble.gameObject.AddComponent<VerticalLayoutGroup>();
        inner.padding = new RectOffset(28, 28, 16, 20);
        inner.spacing = 4f;
        inner.childControlWidth = inner.childControlHeight = true;
        inner.childForceExpandWidth = true;
        inner.childForceExpandHeight = false;

        row.meta = NewText(bubble, "Meta", T.secondarySize, T.textSecondary, TextAlignmentOptions.Left);
        row.evidence = NewText(bubble, "Prueba", T.secondarySize, T.accent, TextAlignmentOptions.Left);
        row.evidence.fontStyle = FontStyles.Italic;
        row.body = NewText(bubble, "Texto", T.bodySize, T.textPrimary, TextAlignmentOptions.Left);
        row.body.richText = false; // Lo escribe el jugador o la IA: nada de etiquetas
        TextStyle.Set(row.meta, TextStyle.Mode.Scrolling, T.secondarySize);
        TextStyle.Set(row.evidence, TextStyle.Mode.Scrolling, T.secondarySize);
        TextStyle.Set(row.body, TextStyle.Mode.Scrolling, T.bodySize);
        row.meta.richText = true;

        var width = bubble.gameObject.AddComponent<BubbleWidth>();
        width.Configure(content, AvatarSize + row.layout.spacing, inner.padding.horizontal, row.meta, row.evidence, row.body);

        row.typewriter = row.body.gameObject.AddComponent<Typewriter>();
        return row;
    }

    private void ConfigureBubble(Row row, ChatEntry entry)
    {
        bool player = entry.kind == ChatEntryKind.Player;
        row.layout.childAlignment = player ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
        row.layout.padding = player ? new RectOffset(AvatarPad(), 0, 0, 0) : new RectOffset(0, 0, 0, 0);
        row.avatar.SetActive(!player);
        row.background.color = player ? T.playerBubble : T.suspectBubble;

        string who = player ? "TÚ" : (entry.speaker ?? "").ToUpperInvariant();
        string whoColor = Theme.Hex(player ? T.playerName : T.suspectName);
        row.meta.text = string.IsNullOrEmpty(entry.time) ? $"<color={whoColor}><b>{who}</b></color>" : $"<color={whoColor}><b>{who}</b></color> · {entry.time}";
        row.meta.alignment = player ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;

        bool hasEvidence = !string.IsNullOrEmpty(entry.evidence);
        row.evidence.gameObject.SetActive(hasEvidence);
        if (hasEvidence)
            row.evidence.text = "Prueba: " + entry.evidence;

        bool silent = player && string.IsNullOrEmpty(entry.text);
        row.body.gameObject.SetActive(!silent || !hasEvidence);
        row.body.text = silent ? "…" : entry.text;
        row.body.maxVisibleCharacters = 99999;
        ApplyAvatar(row);
    }

    // Las burbujas del jugador no llegan al borde izquierdo: se distinguen por el lado
    private static int AvatarPad() => (int)(AvatarSize + 16f);

    private void ApplyAvatar(Row row)
    {
        if (row.kind != RowKind.Bubble || row.face == null)
            return;
        row.face.texture = avatarTexture;
        row.face.color = avatarTexture != null ? Color.white : T.placeholder;
        row.face.uvRect = avatarFace;
        if (avatarLegacy)
            ArtGrading.Apply(row.face, ArtGrading.Kind.LegacyPortrait);
        else
            ArtGrading.Clear(row.face);
    }

    private Row CreateNoticeRow()
    {
        var row = new Row { kind = RowKind.Notice };
        RectTransform root = NewRect(content, "Aviso (auto)");
        row.root = root.gameObject;
        var layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(40, 40, 8, 8);
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        RectTransform card = NewRect(root, "Tarjeta");
        row.background = card.gameObject.AddComponent<Image>();
        row.background.sprite = UISprites.Rounded(ThemeManager.Current.RadiusLarge);
        row.background.type = Image.Type.Sliced;
        var inner = card.gameObject.AddComponent<VerticalLayoutGroup>();
        inner.padding = new RectOffset(24, 24, 14, 14);
        inner.childControlWidth = inner.childControlHeight = true;
        inner.childForceExpandWidth = true;
        inner.childForceExpandHeight = false;

        row.body = NewText(card, "Texto", T.secondarySize, T.systemText, TextAlignmentOptions.Center);
        TextStyle.Set(row.body, TextStyle.Mode.Scrolling, T.secondarySize);
        return row;
    }

    private void ConfigureNotice(Row row, ChatEntry entry)
    {
        Color color;
        Color card = Color.clear;
        FontStyles style = FontStyles.Normal;
        string text = entry.text;
        switch (entry.kind)
        {
            case ChatEntryKind.Unlock:
                color = T.success;
                text = "PUEDES INTERROGAR A: " + entry.text;
                break;
            case ChatEntryKind.Contradiction:
                color = T.contradiction;
                card = Color.Lerp(T.panel, T.contradiction, 0.12f); // Opaco: el contraste se mide contra él
                text = "<b>CONTRADICCIÓN</b>\n" + entry.text;
                break;
            case ChatEntryKind.Error:
                color = T.danger;
                break;
            case ChatEntryKind.Legacy:
                color = T.textPrimary;
                break;
            default:
                color = T.systemText;
                style = FontStyles.Italic;
                break;
        }

        bool legacy = entry.kind == ChatEntryKind.Legacy;
        row.background.color = card;
        row.body.richText = entry.kind == ChatEntryKind.Contradiction || legacy;
        row.body.color = color;
        row.body.fontStyle = style;
        row.body.alignment = legacy ? TextAlignmentOptions.Left : TextAlignmentOptions.Center;
        row.body.text = text;
    }

    private Row CreateDayRow()
    {
        var row = new Row { kind = RowKind.Day };
        RectTransform root = NewRect(content, "Dia (auto)");
        row.root = root.gameObject;
        var layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 24, 16);
        layout.spacing = 6f;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        RectTransform rule = NewRect(root, "Linea");
        var ruleElement = rule.gameObject.AddComponent<LayoutElement>();
        ruleElement.minHeight = ruleElement.preferredHeight = 2f;
        var ruleImage = rule.gameObject.AddComponent<Image>();
        ruleImage.color = new Color(T.accent.r, T.accent.g, T.accent.b, 0.35f);
        ruleImage.raycastTarget = false;
        row.background = ruleImage;

        row.heading = NewText(root, "Titulo", T.headingSize, T.accent, TextAlignmentOptions.Center);
        row.heading.font = UIFactory.TitleFont();
        row.heading.characterSpacing = 12f;
        row.heading.fontStyle = FontStyles.Bold;
        TextStyle.Set(row.heading, TextStyle.Mode.Scrolling, T.headingSize);

        // El parte es contenido del caso, no un aviso: recto, algo más grande y en el color del texto principal
        row.body = NewText(root, "Parte", T.reportSize, T.textPrimary, TextAlignmentOptions.Center);
        row.body.fontStyle = FontStyles.Normal;
        TextStyle.Set(row.body, TextStyle.Mode.Scrolling, T.reportSize);
        return row;
    }

    private void ConfigureDay(Row row, ChatEntry entry)
    {
        row.heading.text = $"DÍA {entry.day}";
        bool report = !string.IsNullOrEmpty(entry.text);
        row.body.gameObject.SetActive(report);
        row.body.text = !report ? "" : entry.day <= 1 ? "Lo que se sabe: " + entry.text : "Parte de la mañana: " + entry.text;
    }

    // Entrada suave de una fila nueva: sube un poco y aparece
    private void Appear(Row row)
    {
        if (GameSettings.ReduceMotion || !isActiveAndEnabled)
            return;

        CanvasGroup group = UIComponents.GetOrAdd<CanvasGroup>(row.root);
        group.alpha = 0f;
        Transform target = row.root.transform;
        StartCoroutine(UIAnimations.Animate(T.bubbleAppearDuration, t =>
        {
            float e = Easing.OutCubic(t);
            group.alpha = e;
            target.localScale = Vector3.one * Mathf.Lerp(0.96f, 1f, e);
        }));
    }

    private void OnDisable()
    {
        Complete();
        follow = null;
        typingDots = null;
    }

    // ---------- Utilidades ----------

    private static RectTransform NewRect(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        return rect;
    }

    private static TMP_Text NewText(Transform parent, string name, float size, Color color, TextAlignmentOptions alignment)
    {
        RectTransform rect = NewRect(parent, name);
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = UIFactory.DefaultFont();
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.Normal;
        rect.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore; // Lo estiliza el chat, no el tema general
        return text;
    }

    private static void Stretch(RectTransform rect, float x, float y)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(x, y);
        rect.offsetMax = new Vector2(-x, -y);
    }
}
