using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// VALIDACIÓN DEL LAYOUT (vertical)
/// Abre la escena real (LayoutPreview), construye la distribución móvil, la rellena con el contenido más largo
/// que puede aparecer y recorre cada panel, en tres proporciones de pantalla, comprobando que:
///   1. ningún elemento con texto o control se sale de su padre,
///   2. no se solapa con sus hermanos,
///   3. ningún texto desborda su caja,
///   4. ningún texto queda por debajo del tamaño legible del tema.
/// Lo que vive dentro de un scroll puede crecer (se desplaza); los overlays (ignoreLayout) pueden tapar.
/// </summary>
public class LayoutValidationTests
{
    private const float Tolerance = 1.5f;

    private LayoutPreview.Session session;

    [SetUp]
    public void SetUp()
    {
        session = LayoutPreview.Open(new Vector2(1080f, 1920f));
    }

    [TearDown]
    public void TearDown()
    {
        LayoutPreview.Close();
    }

    // Tamaño del lienzo que produce el CanvasScaler (1080 × 1920, match 0.5) en cada pantalla
    private static IEnumerable<(string label, Vector2 size)> Screens()
    {
        yield return ("1080x1920 referencia 16-9", new Vector2(1080f, 1920f));
        yield return ("1080x2340 movil alargado 19.5-9", LayoutPreview.CanvasSize(1080f, 2340f));
        yield return ("1536x2048 tableta 4-3", LayoutPreview.CanvasSize(1536f, 2048f));
    }

    private static IEnumerable<TestCaseData> PanelsAndScreens()
    {
        foreach (var screen in Screens())
            foreach (string panel in LayoutPreview.Panels)
                yield return new TestCaseData(panel, screen.size, 0).SetName($"Layout_{panel}_{screen.label}");

        // Alto contraste (Ajustes): todos los paneles, con sus textos, contrastes y zonas táctiles
        foreach (string panel in LayoutPreview.Panels)
            yield return new TestCaseData(panel, new Vector2(1080f, 1920f), -1).SetName($"Layout_{panel}_alto contraste");

        // Texto "muy grande" (Ajustes) en las pantallas de móvil
        foreach (var screen in Screens().Take(2))
            foreach (string panel in LayoutPreview.Panels)
                yield return new TestCaseData(panel, screen.size, 2).SetName($"Layout_{panel}_{screen.label}_texto muy grande");
    }

    [TestCaseSource(nameof(PanelsAndScreens))]
    public void PanelSinDesbordesNiSolapes(string panelName, Vector2 canvasSize, int textSize)
    {
        // textSize -1: tamaño normal con alto contraste (la vista previa se construye ya con ese tema)
        if (textSize < 0)
        {
            LayoutPreview.Close();
            session = LayoutPreview.Open(canvasSize, highContrast: true);
            Assert.IsTrue(GameSettings.HighContrast);
            textSize = 0;
        }
        GameSettings.TextSizeLevel = textSize;
        LayoutPreview.SetSize(session, canvasSize);
        RectTransform panel = LayoutPreview.ShowOnly(session, panelName);
        Assert.IsNotNull(panel, $"no existe {panelName}");

        var errors = new List<string>();
        Validate(panel, errors);

        Assert.IsEmpty(errors, $"{panelName} a {canvasSize.x:F0}x{canvasSize.y:F0}:\n" + string.Join("\n", errors.Take(40)));
    }

    [Test]
    public void AvisoDePistaCabeEnPantalla()
    {
        RectTransform notice = LayoutPreview.Find(session, "ClueNotification");
        LayoutPreview.ShowOnly(session, "InterrogationPanel");
        notice.gameObject.SetActive(true);
        notice.GetComponentInChildren<TMP_Text>(true).text = "PISTAS DESCUBIERTAS:\nEl armario medio vacío\nAlgo gordo en el puerto";

        var errors = new List<string>();
        LayoutPreview.Rebuild(notice);
        Validate(notice, errors);

        Assert.IsEmpty(errors, string.Join("\n", errors));
    }

    [Test]
    public void LasOpcionesDeLosDesplegablesSeTocanBien()
    {
        foreach (TMP_Dropdown dropdown in session.canvas.GetComponentsInChildren<TMP_Dropdown>(true))
        {
            Assert.IsNotNull(dropdown.template, dropdown.name);
            Transform item = dropdown.template.GetComponentInChildren<Toggle>(true)?.transform;
            Assert.IsNotNull(item, $"{dropdown.name}: sin opción de plantilla");
            Assert.GreaterOrEqual(((RectTransform)item).rect.height, Theme.MinTouchSize - Tolerance,
                $"{dropdown.name}: cada opción de la lista debe medir 48 dp");
        }
    }

    [Test]
    public void PreguntasDeEjemploCabenSinTaparElChat([Values(0, 2)] int textSize, [Values(1920f, 2400f, 1440f)] float height)
    {
        GameSettings.TextSizeLevel = textSize;
        LayoutPreview.SetSize(session, LayoutPreview.CanvasSize(1080f, height));
        RectTransform panel = LayoutPreview.ShowOnly(session, "InterrogationPanel");

        // Un sospechoso aún sin preguntas, con el parte del día más largo de los tres casos
        string situation = CaseLibrary.Stories.Select(s => s.situation).OrderByDescending(s => s.Length).First();
        session.ui.ShowWaiting(false);
        session.ui.UpdateGameState(1, 7, 0, 5); // La vista previa llega con el día gastado
        session.ui.Conversations.Append("nuevo", ChatEntry.Day(1, situation));
        session.ui.Conversations.Select("nuevo");
        session.ui.SetVictim("Sofía");
        LayoutPreview.Rebuild(panel);

        RectTransform box = LayoutPreview.Find(session, "Sugerencias (auto)");
        Assert.IsNotNull(box);
        Assert.IsTrue(box.gameObject.activeInHierarchy, "un interrogatorio sin empezar ofrece preguntas");

        var errors = new List<string>();
        Validate(panel, errors);
        Assert.IsEmpty(errors, string.Join("\n", errors));

        Button[] chips = box.GetComponentsInChildren<Button>();
        Assert.AreEqual(3, chips.Length);
        foreach (Button chip in chips)
            Assert.GreaterOrEqual(((RectTransform)chip.transform).rect.height, Theme.MinTouchSize - Tolerance, chip.name);
        StringAssert.Contains("Sofía", string.Join(" ", chips.Select(c => c.GetComponentInChildren<TMP_Text>().text)));

        ScrollRect chat = LayoutPreview.Find(session, "ConversationScroll").GetComponent<ScrollRect>();
        Assert.GreaterOrEqual(WorldRect(chat.viewport).yMin, WorldRect(box).yMax - Tolerance, "debajo del chat, sin taparlo");
        Assert.GreaterOrEqual(WorldRect(chat.viewport).height, WorldRect(box).height * 0.5f, "el chat sigue teniendo sitio");

        TMP_Text hint = box.GetComponentInChildren<TMP_Text>();
        Assert.AreEqual(UIRole.Secondary, hint.GetComponent<ThemeRole>()?.role, "el rótulo sigue gris al cambiar de tema");

        // Sin preguntas hoy no se invita a preguntar
        session.ui.UpdateGameState(1, 7, 5, 5);
        Assert.IsFalse(box.gameObject.activeSelf, "sin preguntas hoy");
        session.ui.UpdateGameState(1, 7, 0, 5);
        Assert.IsTrue(box.gameObject.activeSelf);

        // Con algo escrito (o una pregunta devuelta tras un fallo) no se ofrece sustituirlo
        TMP_InputField input = LayoutPreview.Find(session, "QuestionInput")?.GetComponent<TMP_InputField>()
                               ?? panel.GetComponentInChildren<TMP_InputField>(true);
        input.text = "¿Qué hacía usted allí?";
        Assert.IsFalse(box.gameObject.activeSelf, "con el campo escrito");
        input.text = "";
        Assert.IsTrue(box.gameObject.activeSelf);

        session.ui.Conversations.Append("nuevo", ChatEntry.Player("¿Dónde estabas?", null, "09:00"));
        session.ui.RefreshConversationView();
        Assert.IsFalse(box.gameObject.activeSelf, "desaparecen con la primera pregunta");
    }

    [Test]
    public void ElResumenDeLaAcusacionSigueAlTema()
    {
        RectTransform panel = LayoutPreview.ShowOnly(session, "AccusatonPanel");
        TMP_Text prompt = panel.GetComponentsInChildren<TMP_Text>(true).First(t => t.name == "Text (TMP)");
        StringAssert.Contains("libreta", prompt.text);
        string before = prompt.text;

        GameSettings.HighContrast = !GameSettings.HighContrast;
        session.ui.RestyleForTheme();
        Assert.AreNotEqual(before, prompt.text, "el color del resumen se rehace con el tema nuevo");
        StringAssert.Contains(Theme.Hex(ThemeManager.Current.textSecondary), prompt.text);
    }

    [Test]
    public void ChatSoloDesplazamientoVertical()
    {
        ScrollRect chat = LayoutPreview.Find(session, "ConversationScroll").GetComponent<ScrollRect>();

        Assert.IsFalse(chat.horizontal, "sin desplazamiento horizontal");
        Assert.IsTrue(chat.vertical);
        Assert.IsTrue(chat.horizontalScrollbar == null || !chat.horizontalScrollbar.gameObject.activeSelf, "sin barra horizontal");
    }

    [Test]
    public void BarraSuperiorConAccionesSeparadas()
    {
        LayoutPreview.ShowOnly(session, "InterrogationPanel");
        Rect endDay = WorldRect(LayoutPreview.Find(session, "EndDayButton"));
        Rect accuse = WorldRect(LayoutPreview.Find(session, "AcuseNowButton"));

        Assert.IsFalse(endDay.Overlaps(new Rect(accuse.x + Tolerance, accuse.y + Tolerance, accuse.width - 2 * Tolerance, accuse.height - 2 * Tolerance)),
            "Fin del día y Acusar no comparten sitio");
        Assert.GreaterOrEqual(endDay.height, Theme.MinTouchSize - Tolerance);
        Assert.GreaterOrEqual(accuse.height, Theme.MinTouchSize - Tolerance);
    }

    // ---------- Recorrido ----------

    private void Validate(RectTransform root, List<string> errors)
    {
        CheckInside(root, (RectTransform)session.canvas.transform, errors);

        // Glifos, fuente, contraste y política de tamaño en todos los textos visibles (también en scroll)
        foreach (TMP_Text any in root.GetComponentsInChildren<TMP_Text>(false))
        {
            if (!any.enabled || string.IsNullOrEmpty(any.text))
                continue;
            CheckGlyphs(any, errors);
            CheckContrast(any, errors);
            CheckStyleKept(any, errors);
        }

        foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(false))
        {
            if (rect == root || InsideInputField(rect))
                continue;

            // El contenido de un scroll puede ser más alto que su ventana (para eso se desplaza)
            if (IsRelevant(rect) && rect.parent is RectTransform parent && !IsClippedCover(rect) && !IsScrollContent(rect))
                CheckInside(rect, parent, errors);

            TMP_Text text = rect.GetComponent<TMP_Text>();
            if (text != null && text.enabled && !string.IsNullOrEmpty(text.text))
            {
                text.ForceMeshUpdate(true, true);
                if (text.isTextOverflowing)
                    errors.Add($"texto desbordado: {Path(rect)} («{Short(text.text)}», {text.fontSize:F0} px)");
                if (text.fontSize < Theme.MinReadableSize - 0.5f)
                    errors.Add($"texto ilegible: {Path(rect)} a {text.fontSize:F0} px (mínimo {Theme.MinReadableSize})");
            }

            CheckSiblings(rect, errors);
            CheckOverlaysOnTop(rect, errors);
        }

        // Zonas táctiles: 48 dp de lado como mínimo (las barras de scroll se arrastran con el contenido)
        foreach (Selectable control in root.GetComponentsInChildren<Selectable>(false))
        {
            if (control is Scrollbar || !control.interactable)
                continue;
            Rect r = ((RectTransform)control.transform).rect;
            if (r.width < Theme.MinTouchSize - Tolerance || r.height < Theme.MinTouchSize - Tolerance)
                errors.Add($"zona táctil pequeña: {Path((RectTransform)control.transform)} {r.width:F0}x{r.height:F0} (mínimo {Theme.MinTouchSize})");
        }

        // Casillas: su borde tiene que verse sobre el fondo (WCAG 1.4.11, 3:1 para componentes)
        foreach (Toggle toggle in root.GetComponentsInChildren<Toggle>(false))
        {
            if (!(toggle.targetGraphic is Graphic box))
                continue;
            Color? background = BackgroundOf(box.transform);
            if (background == null)
                continue;
            float ratio = Contrast(box.color, background.Value);
            if (ratio < 3f)
                errors.Add($"casilla invisible: {Path((RectTransform)toggle.transform)} contraste {ratio:F1}:1");
        }

        // Placeholder de los campos de texto: no debe desbordar
        foreach (TMP_InputField field in root.GetComponentsInChildren<TMP_InputField>(false))
        {
            if (field.placeholder is TMP_Text placeholder && placeholder.gameObject.activeInHierarchy)
            {
                placeholder.ForceMeshUpdate(true, true);
                if (placeholder.isTextOverflowing)
                    errors.Add($"placeholder desbordado: {Path((RectTransform)placeholder.transform)}");
            }
        }
    }

    private static void CheckSiblings(RectTransform parent, List<string> errors)
    {
        List<RectTransform> content = parent.Cast<Transform>().OfType<RectTransform>()
            .Where(r => r.gameObject.activeSelf && IsRelevant(r) && !IsOverlay(r)).ToList();

        for (int i = 0; i < content.Count; i++)
        {
            for (int j = i + 1; j < content.Count; j++)
            {
                Rect a = WorldRect(content[i]);
                Rect b = WorldRect(content[j]);
                float w = Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin);
                float h = Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin);
                if (w > Tolerance && h > Tolerance)
                    errors.Add($"solape: {Path(content[i])} ↔ {content[j].name} ({w:F0}×{h:F0})");
            }
        }
    }

    // Una capa activa (libreta, aviso) debe dibujarse por encima del contenido de su padre
    private static void CheckOverlaysOnTop(RectTransform parent, List<string> errors)
    {
        int lastContent = -1;
        foreach (Transform child in parent)
        {
            if (child.gameObject.activeSelf && !IsOverlay((RectTransform)child) && IsRelevant((RectTransform)child))
                lastContent = Mathf.Max(lastContent, child.GetSiblingIndex());
        }

        foreach (Transform child in parent)
        {
            if (child.gameObject.activeSelf && IsOverlay((RectTransform)child) && child.GetComponent<Graphic>() is Graphic g
                && g.raycastTarget && child.GetSiblingIndex() < lastContent)
                errors.Add($"capa por debajo del contenido: {Path((RectTransform)child)}");
        }
    }

    // La política de texto (TextStyle) debe sobrevivir a la activación del panel (TMP reinicia los textos nuevos)
    private static void CheckStyleKept(TMP_Text text, List<string> errors)
    {
        if (!text.TryGetComponent(out TextStyle style))
            return;
        bool auto = style.mode == TextStyle.Mode.OneLine || style.mode == TextStyle.Mode.MultiLine;
        float actual = auto ? text.fontSizeMax : text.fontSize;
        float expected = Mathf.Max(style.maxSize * GameSettings.TextScale, Theme.MinReadableSize);
        if (auto != text.enableAutoSizing || Mathf.Abs(actual - expected) > 0.5f)
            errors.Add($"estilo perdido: {Path((RectTransform)text.transform)} {actual:F0} px en vez de {expected:F0}");
    }

    // Contraste WCAG entre cada color del texto (incluidas las etiquetas <color>) y su fondo real
    private static readonly System.Text.RegularExpressions.Regex ColorTag =
        new System.Text.RegularExpressions.Regex("<color=(#[0-9A-Fa-f]{6})");

    private static void CheckContrast(TMP_Text text, List<string> errors)
    {
        Color? background = BackgroundOf(text.transform);
        if (background == null)
            return; // Encima de una ilustración: no se puede medir con un color

        var colors = new List<Color> { text.color };
        foreach (System.Text.RegularExpressions.Match m in ColorTag.Matches(text.text))
        {
            if (ColorUtility.TryParseHtmlString(m.Groups[1].Value, out Color c))
                colors.Add(c);
        }

        float required = text.fontSize >= 48f ? 3f : 4.5f;
        foreach (Color c in colors)
        {
            float ratio = Contrast(c, background.Value);
            if (ratio < required)
            {
                errors.Add($"contraste {ratio:F1}:1 (mínimo {required}) en {Path((RectTransform)text.transform)}: #{ColorUtility.ToHtmlStringRGB(c)} sobre #{ColorUtility.ToHtmlStringRGB(background.Value)}");
                return;
            }
        }
    }

    // Primer fondo opaco por encima del texto (el control que lo contiene o el panel)
    private static Color? BackgroundOf(Transform t)
    {
        for (Transform p = t.parent; p != null; p = p.parent)
        {
            if (p.GetComponent<RawImage>() is RawImage raw && raw.enabled && raw.color.a > 0.5f)
                return null;
            if (!(p.GetComponent<Image>() is Image image) || !image.enabled)
                continue;
            if (p.TryGetComponent(out Mask mask) && mask.enabled && !mask.showMaskGraphic)
                continue; // Máscara invisible: no pinta nada
            if (image.sprite != null && ThemeApplier.IsArtworkSprite(image.sprite.name))
                return null;

            Color color = image.color;
            Selectable owner = p.GetComponent<Selectable>();
            if (owner != null && owner.targetGraphic == image && owner.transition == Selectable.Transition.ColorTint)
                color *= owner.colors.normalColor;
            if (color.a > 0.5f)
                return color;
        }
        return null;
    }

    private static float Contrast(Color a, Color b)
    {
        float la = Luminance(a), lb = Luminance(b);
        return (Mathf.Max(la, lb) + 0.05f) / (Mathf.Min(la, lb) + 0.05f);
    }

    private static float Luminance(Color c)
    {
        float Channel(float v) => v <= 0.03928f ? v / 12.92f : Mathf.Pow((v + 0.055f) / 1.055f, 2.4f);
        return 0.2126f * Channel(c.r) + 0.7152f * Channel(c.g) + 0.0722f * Channel(c.b);
    }

    // Texto sin fuente, o con caracteres que la fuente no tiene (TMP los sustituye por el carácter de relleno)
    private static void CheckGlyphs(TMP_Text text, List<string> errors)
    {
        if (text.font == null)
        {
            errors.Add($"texto sin fuente: {Path((RectTransform)text.transform)}");
            return;
        }

        text.ForceMeshUpdate(true, true);
        char replacement = (char)TMP_Settings.missingGlyphCharacter;
        if (replacement == 0)
            replacement = (char)0x25A1;
        bool intended = text.text.IndexOf(replacement) >= 0;

        string family = text.font.faceInfo.familyName;
        for (int i = 0; i < text.textInfo.characterCount; i++)
        {
            TMP_CharacterInfo ci = text.textInfo.characterInfo[i];
            if (ci.character == replacement && !intended)
            {
                errors.Add($"carácter sin glifo en {Path((RectTransform)text.transform)} ({text.font.name}): «{Short(text.text)}»");
                return;
            }
            // Una letra sacada de otra familia (la de reserva) se nota: "DÍA" con la Í en otra fuente
            if (ci.isVisible && ci.fontAsset != null && ci.fontAsset.faceInfo.familyName != family)
            {
                errors.Add($"letra «{ci.character}» en otra fuente ({ci.fontAsset.faceInfo.familyName}) en {Path((RectTransform)text.transform)} ({family})");
                return;
            }
        }
    }

    private static void CheckInside(RectTransform child, RectTransform parent, List<string> errors)
    {
        Rect c = WorldRect(child);
        Rect p = WorldRect(parent);
        if (c.xMin < p.xMin - Tolerance || c.xMax > p.xMax + Tolerance || c.yMin < p.yMin - Tolerance || c.yMax > p.yMax + Tolerance)
            errors.Add($"se sale de su padre: {Path(child)} {Fmt(c)} fuera de {parent.name} {Fmt(p)}");
        if (c.width < 1f || c.height < 1f)
            errors.Add($"tamaño nulo: {Path(child)} {Fmt(c)}");
    }

    // Texto, controles, imágenes y contenedores de layout; no la decoración interna de un control
    private static bool IsRelevant(RectTransform rect)
    {
        if (rect.GetComponentInParent<Selectable>(true) is Selectable owner && owner.transform != rect && !(owner is Scrollbar))
            return rect.GetComponent<TMP_Text>() != null && owner.GetComponent<TMP_Dropdown>() == null; // Etiqueta de un botón
        return rect.GetComponent<TMP_Text>() != null || rect.GetComponent<Selectable>() != null
            || rect.GetComponent<LayoutGroup>() != null || rect.GetComponent<RawImage>() != null
            || rect.GetComponent<ScrollRect>() != null;
    }

    private static bool IsScrollContent(RectTransform rect)
    {
        return rect.parent != null && rect.GetComponentInParent<ScrollRect>(true) is ScrollRect scroll && scroll.content == rect;
    }

    // Fondo que cubre la pantalla sin deformarse: desborda a propósito y su padre lo recorta
    private static bool IsClippedCover(RectTransform rect)
    {
        return rect.TryGetComponent(out AspectRatioFitter fitter) && fitter.aspectMode == AspectRatioFitter.AspectMode.EnvelopeParent
               && rect.parent != null && rect.parent.GetComponent<RectMask2D>() != null;
    }

    private static bool IsOverlay(RectTransform rect)
    {
        return rect.TryGetComponent(out LayoutElement le) && le.ignoreLayout;
    }

    private static bool InsideScrollContent(RectTransform rect)
    {
        foreach (ScrollRect scroll in rect.GetComponentsInParent<ScrollRect>(true))
        {
            if (scroll.content != null && (rect == scroll.content || rect.IsChildOf(scroll.content)))
                return true;
        }
        return false;
    }

    private static bool InsideInputField(RectTransform rect)
    {
        TMP_InputField field = rect.GetComponentInParent<TMP_InputField>(true);
        return field != null && field.transform != rect;
    }

    private static Rect WorldRect(RectTransform rect)
    {
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
    }

    private static string Path(RectTransform rect)
    {
        return rect.parent != null ? $"{rect.parent.name}/{rect.name}" : rect.name;
    }

    private static string Fmt(Rect r) => $"[{r.xMin:F0},{r.yMin:F0}–{r.xMax:F0},{r.yMax:F0}]";

    private static string Short(string s) => s.Length > 30 ? s.Substring(0, 30) + "…" : s;
}
