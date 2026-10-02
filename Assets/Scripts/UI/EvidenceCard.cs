using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum EvidenceCardMode { Empty, Full, Collapsed, Expanded }

/// <summary>
/// Tarjeta "TUS PRUEBAS" de la acusación (propuesta adoptada por Cristian, sesión C): entre la pregunta y la rueda, lo
/// que el jugador lleva. Las pistas por su nombre y las contradicciones con el mismo texto que la libreta (el juego se lo
/// pasa hecho: AIConversationManager.DescribeContradiction), sin decir más que ella.
///   - Libreta vacía: dos líneas (título y "Libreta vacía: …"); el resto, para la rueda.
///   - Entera, si cabe en la pared que la rueda no usa (pantallas 20:9: la rueda la limita el ancho). La rueda no encoge.
///   - Si no cabe (16:9): una línea, "TUS PRUEBAS · 4 pistas · 1 contradicción", que al tocarla despliega la lista.
///     Desplegada empuja la rueda hacia abajo (la encoge mientras está abierta); nunca la tapa.
/// Es un elemento del layout de la columna: no puede pisar ni la rueda ni los botones.
/// </summary>
[ExecuteAlways]
public class EvidenceCard : MonoBehaviour
{
    public const string ObjectName = "Tarjeta de pruebas (auto)";
    public const float Padding = 20f;
    private const float MaxExpandedShare = 0.6f; // Desplegada, como mucho el 60 % del alto de tarjeta + rueda

    private RectTransform wall;
    private HeightLineup lineup;
    private LayoutElement element;
    private Button header;
    private TMP_Text headerLabel;
    private RectTransform arrow;
    private TMP_Text body;
    private readonly List<string> clues = new List<string>();
    private readonly List<string> contradictions = new List<string>();
    private bool expanded;
    private EvidenceCardMode mode = EvidenceCardMode.Empty;

    private static Theme T => ThemeManager.Current;

    public EvidenceCardMode Mode => mode;
    public float LastSpare, LastFull, LastTotal; // Medidas de la última decisión (tests y depuración)
    public TMP_Text Body => body;

    /// <summary>Lo que se ve ahora (cabecera y lista, según la forma), sin etiquetas de formato.</summary>
    public string VisibleText
    {
        get
        {
            var parts = new List<string>();
            if (header != null && header.gameObject.activeSelf)
                parts.Add(StripTags(headerLabel.text));
            if (body != null && body.gameObject.activeSelf)
                parts.Add(StripTags(body.text));
            return string.Join("\n", parts);
        }
    }

    /// <summary>
    /// Forma de la tarjeta: vacía si no hay pruebas; entera si cabe en el sitio que le sobra a la rueda; si no, plegada.
    /// </summary>
    public static EvidenceCardMode Choose(float spare, float fullHeight, bool empty)
    {
        if (empty)
            return EvidenceCardMode.Empty;
        return fullHeight <= spare ? EvidenceCardMode.Full : EvidenceCardMode.Collapsed;
    }

    /// <summary>
    /// Monta la tarjeta en la columna de la acusación ('wallRect' es la pared de la rueda, su hermana de abajo).
    /// </summary>
    public static EvidenceCard Build(RectTransform column, RectTransform wallRect, Sprite arrowSprite)
    {
        RectTransform root = UIFactory.Container(column, ObjectName, Vector2.zero, Vector2.one);
        var card = root.gameObject.AddComponent<EvidenceCard>();
        card.wall = wallRect;
        root.SetSiblingIndex(wallRect.GetSiblingIndex()); // Justo encima de la pared
        card.element = UIComponents.GetOrAdd<LayoutElement>(root.gameObject);
        card.element.flexibleHeight = 0f;

        var background = root.gameObject.AddComponent<Image>();
        background.sprite = UISprites.Rounded(T.RadiusPill / 3);
        background.type = Image.Type.Sliced;
        background.color = new Color(T.buttonSecondary.r, T.buttonSecondary.g, T.buttonSecondary.b, 0.9f);
        background.raycastTarget = false;
        root.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;

        // Cabecera tocable (solo plegada o desplegada): una línea con el resumen y la flecha
        card.header = UIFactory.Button(root, "", false, card.Toggle);
        card.header.name = "Cabecera de pruebas (auto)";
        UIComponents.GetOrAdd<LayoutElement>(card.header.gameObject).ignoreLayout = true;
        var h = (RectTransform)card.header.transform;
        h.anchorMin = new Vector2(0f, 1f);
        h.anchorMax = Vector2.one;
        h.pivot = new Vector2(0.5f, 1f);
        h.sizeDelta = new Vector2(0f, Theme.MinTouchSize);
        h.anchoredPosition = Vector2.zero;
        card.headerLabel = card.header.GetComponentInChildren<TMP_Text>(true);
        card.headerLabel.alignment = TextAlignmentOptions.MidlineLeft;
        card.headerLabel.margin = new Vector4(Padding, 0f, Theme.MinTouchSize, 0f);
        LayoutKit.OneLine(card.headerLabel, T.secondarySize);
        if (arrowSprite != null)
        {
            card.arrow = UIFactory.Container(h, "Flecha", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f));
            card.arrow.sizeDelta = new Vector2(40f, 40f);
            card.arrow.anchoredPosition = new Vector2(-Theme.MinTouchSize * 0.5f, 0f);
            var a = card.arrow.gameObject.AddComponent<Image>();
            a.sprite = arrowSprite;
            a.color = T.textPrimary;
            a.raycastTarget = false;
        }

        // La lista (o, vacía, el título y una línea)
        RectTransform b = UIFactory.Container(root, "Lista de pruebas", Vector2.zero, Vector2.one);
        card.body = b.gameObject.AddComponent<TextMeshProUGUI>();
        card.body.font = UIFactory.DefaultFont();
        card.body.color = T.textPrimary;
        card.body.raycastTarget = false;
        card.body.alignment = TextAlignmentOptions.TopLeft;
        card.body.textWrappingMode = TextWrappingModes.Normal;
        card.body.richText = true;
        b.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
        card.Apply();
        return card;
    }

    /// <summary>Contenido: nombres de las pistas de la libreta y textos de las contradicciones (los de la libreta).</summary>
    public void Set(IEnumerable<string> clueNames, IEnumerable<string> contradictionTexts)
    {
        clues.Clear();
        clues.AddRange(clueNames);
        contradictions.Clear();
        contradictions.AddRange(contradictionTexts);
        expanded = false;
        Apply();
    }

    public void Toggle()
    {
        if (mode != EvidenceCardMode.Collapsed && mode != EvidenceCardMode.Expanded)
            return;
        expanded = !expanded;
        Apply();
    }

    private void OnEnable()
    {
        Apply();
    }

    // Un cambio de tamaño llega en pleno rebuild del layout: ahí no se puede encender ni apagar nada (TextMeshPro se
    // queja). Se decide en el siguiente LateUpdate; al abrir la acusación y al tocarla se decide en el acto
    private bool dirty;

    private void OnRectTransformDimensionsChange()
    {
        dirty = true;
    }

    private void LateUpdate()
    {
        if (!dirty)
            return;
        dirty = false;
        Apply();
    }

    private string Title => $"<color={Theme.Hex(T.accent)}><b>{GameTexts.EvidenceCardTitle}</b></color>";

    // "TUS PRUEBAS · 4 pistas · 1 contradicción": el mismo resumen entera, plegada y desplegada
    private string SummaryTitle => $"{Title}  ·  {GameTexts.EvidenceLine(clues.Count, contradictions.Count)}";

    private string ListText()
    {
        // Pistas con un punto; contradicciones con ≠ y el texto de la libreta
        var lines = new List<string>();
        foreach (string c in clues)
            lines.Add($"  ·  {c}");
        foreach (string c in contradictions)
            lines.Add($"  <color={Theme.Hex(T.accent)}><b>≠</b></color>  {c}");
        return string.Join("\n", lines);
    }

    private float PreferredHeight(string text, float width)
    {
        if (width <= 0f)
            return 0f;
        body.enableAutoSizing = false;
        body.fontSize = Mathf.Max(T.secondarySize * GameSettings.TextScale, Theme.MinReadableSize);
        return body.GetPreferredValues(text, width, float.PositiveInfinity).y;
    }

    /// <summary>
    /// Elige la forma con el sitio de ahora (tarjeta + pared, que se reparten la parte flexible de la columna) y fija el
    /// alto de la tarjeta. Si no cambia nada, no toca el layout (se llama desde los cambios de tamaño).
    /// </summary>
    public void Apply()
    {
        if (body == null || element == null)
            return;
        var rect = (RectTransform)transform;
        float width = rect.rect.width - 2f * Padding;
        float total = rect.rect.height + (wall != null ? wall.rect.height : 0f);
        bool empty = clues.Count == 0 && contradictions.Count == 0;

        string full = empty ? Title + "\n" + GameTexts.EvidenceCardEmpty : SummaryTitle + "\n" + ListText();
        float fullHeight = PreferredHeight(full, width) + 2f * Padding;
        float spare = total - NeededLineupHeight() - T.spacing;
        EvidenceCardMode next = Choose(spare, fullHeight, empty);
        if (next == EvidenceCardMode.Collapsed && expanded)
            next = EvidenceCardMode.Expanded;
        mode = next;
        LastSpare = spare; LastFull = fullHeight; LastTotal = total;

        float height;
        bool showHeader = mode == EvidenceCardMode.Collapsed || mode == EvidenceCardMode.Expanded;
        // Solo lo que cambia: esto también corre dentro de un cambio de tamaño (en pleno rebuild del layout)
        if (header.gameObject.activeSelf != showHeader)
            header.gameObject.SetActive(showHeader);
        if (body.gameObject.activeSelf != (mode != EvidenceCardMode.Collapsed))
            body.gameObject.SetActive(mode != EvidenceCardMode.Collapsed);
        var b = body.rectTransform;
        b.offsetMin = new Vector2(Padding, Padding);
        b.offsetMax = new Vector2(-Padding, showHeader ? -Theme.MinTouchSize : -Padding);
        switch (mode)
        {
            case EvidenceCardMode.Collapsed:
                height = Theme.MinTouchSize;
                break;
            case EvidenceCardMode.Expanded:
                string list = ListText();
                height = Theme.MinTouchSize + PreferredHeight(list, width) + Padding;
                height = Mathf.Min(height, Mathf.Max(Theme.MinTouchSize * 2f, total * MaxExpandedShare));
                if (body.text != list)
                    body.text = list;
                break;
            default:
                height = fullHeight;
                if (body.text != full)
                    body.text = full;
                break;
        }
        // Si la lista desplegada no cabe en su tope, se reduce hasta el mínimo legible y después "…"
        body.enableAutoSizing = mode == EvidenceCardMode.Expanded;
        body.fontSizeMax = Mathf.Max(T.secondarySize * GameSettings.TextScale, Theme.MinReadableSize);
        body.fontSizeMin = Theme.MinReadableSize;
        body.overflowMode = TextOverflowModes.Ellipsis;

        if (showHeader)
        {
            string line = SummaryTitle;
            if (headerLabel.text != line)
                headerLabel.text = line;
            if (arrow != null)
                arrow.localEulerAngles = new Vector3(0f, 0f, mode == EvidenceCardMode.Expanded ? 180f : 0f);
        }

        if (!Mathf.Approximately(element.preferredHeight, height))
        {
            element.minHeight = element.preferredHeight = height;
            if (transform.parent is RectTransform column)
                LayoutRebuilder.MarkLayoutForRebuild(column);
        }
    }

    // Alto que necesita la rueda a su escala: la limita el ancho (con 20:9 sobra alto) o todo el alto que tenga
    private float NeededLineupHeight()
    {
        if (lineup == null && wall != null)
            lineup = wall.GetComponentInChildren<HeightLineup>();
        if (lineup == null)
            return 0f;
        var people = new List<(int cm, float aspect)>();
        foreach (HeightLineupItem item in lineup.GetComponentsInChildren<HeightLineupItem>())
            people.Add((item.heightCm, item.aspect));
        if (people.Count == 0)
            return 0f;
        return HeightLineup.NeededHeight(((RectTransform)lineup.transform).rect.width, lineup.labelBand, people, T.lineupOverlap);
    }

    private static string StripTags(string s)
    {
        return System.Text.RegularExpressions.Regex.Replace(s ?? "", "<[^>]+>", "");
    }
}
