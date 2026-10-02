using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Revisión de Cristian (sesión C) de las capturas de retratos: controles tapados o aplastados por el busto, el
/// sospechoso dos veces, el busto ampliado y borroso, el contraluz azul, la barra de arriba cortada y la rueda pequeña.
/// En el lienzo, 1440 × 3200 es igual que 1080 × 2400 (misma proporción, CanvasScaler con match 0,5); lo que cambia es
/// la densidad, que solo cuenta para la nitidez del busto (PixelScale, con su propio test). 1080 × 3840 (9:32) es la
/// proporción con la que salieron mal las capturas: ningún móvil la tiene, pero el layout no debe romperse.
/// </summary>
public class ArreglosInterfazTests
{
    private const float Tolerance = 1.5f;
    private LayoutPreview.Session session;

    private static IEnumerable<(string label, Vector2 size)> Screens()
    {
        yield return ("1080x1920", new Vector2(1080f, 1920f));
        yield return ("1080x2400", LayoutPreview.CanvasSize(1080f, 2400f));
        yield return ("1440x3200", LayoutPreview.CanvasSize(1440f, 3200f));
        yield return ("1080x3840 9-32", LayoutPreview.CanvasSize(1080f, 3840f));
    }

    private static IEnumerable<TestCaseData> PanelsAndScreens()
    {
        foreach (var screen in Screens())
            foreach (string panel in LayoutPreview.Panels)
                foreach (int textSize in new[] { 0, 2 })
                    yield return new TestCaseData(panel, screen.size, textSize).SetName($"Controles_{panel}_{screen.label}_texto{textSize}");
    }

    private static IEnumerable<TestCaseData> ScreensAndTextSizes()
    {
        foreach (var screen in Screens())
            foreach (int textSize in new[] { 0, 2 })
                yield return new TestCaseData(screen.size, textSize).SetName($"Hud_{screen.label}_texto{textSize}");
    }

    [SetUp]
    public void SetUp()
    {
        session = LayoutPreview.Open(new Vector2(1080f, 1920f));
    }

    [TearDown]
    public void TearDown()
    {
        GameSettings.TextSizeLevel = 0;
        ThemeManager.Override(null);
        LayoutPreview.Close();
    }

    // ---------- 1. Ningún control interactivo tapado, aplastado o fuera de pantalla ----------

    [TestCaseSource(nameof(PanelsAndScreens))]
    public void LosControlesSeVenYSeTocan(string panelName, Vector2 canvasSize, int textSize)
    {
        GameSettings.TextSizeLevel = textSize;
        LayoutPreview.SetSize(session, canvasSize);
        RectTransform panel = LayoutPreview.ShowOnly(session, panelName);

        var errors = new List<string>();
        CheckControls((RectTransform)session.canvas.transform, panel, errors);

        Assert.IsEmpty(errors, $"{panelName} a {canvasSize.x:F0}x{canvasSize.y:F0}:\n" + string.Join("\n", errors.Take(30)));
    }

    // La opción B (figura, sin busto): la cabecera cambia, los controles tienen que seguir libres
    [TestCase(1080f, 1920f)]
    [TestCase(1080f, 2400f)]
    [TestCase(1080f, 3840f)]
    public void ConLaFiguraLosControlesSeVenYSeTocan(float w, float h)
    {
        OpenWith(t => t.interrogationComposition = PortraitComposition.Figura);
        LayoutPreview.SetSize(session, LayoutPreview.CanvasSize(w, h));
        RectTransform panel = LayoutPreview.ShowOnly(session, "InterrogationPanel");
        var errors = new List<string>();
        CheckControls((RectTransform)session.canvas.transform, panel, errors);
        Assert.IsEmpty(errors, string.Join("; ", errors));
        // Y la etiqueta de estado no se sale de la zona de la figura
        Rect chip = WorldRect(LayoutPreview.Find(session, "Estado (auto)"));
        Rect stage = WorldRect(LayoutPreview.Find(session, InterrogationScene.StageName));
        Assert.LessOrEqual(chip.yMax, stage.yMax + Tolerance, "la etiqueta queda dentro, bajo la cabecera");
    }

    // El test tiene que cazar lo que se vio en las capturas: un control tapado por una imagen dibujada después
    [Test]
    public void ElTestCazaUnControlTapadoPorUnaImagen()
    {
        RectTransform panel = LayoutPreview.ShowOnly(session, "InterrogationPanel");
        var endDay = LayoutPreview.Find(session, "EndDayButton");
        var cover = new GameObject("Tapa (test)", typeof(RectTransform), typeof(RawImage)).GetComponent<RectTransform>();
        cover.SetParent(panel, false);
        cover.position = endDay.position;
        cover.sizeDelta = endDay.rect.size;
        cover.GetComponent<RawImage>().raycastTarget = false;
        LayoutPreview.Rebuild((RectTransform)session.canvas.transform);

        var errors = new List<string>();
        CheckControls((RectTransform)session.canvas.transform, panel, errors);

        Assert.IsTrue(errors.Any(e => e.Contains("EndDayButton") && e.Contains("Tapa (test)")), string.Join("\n", errors));
    }

    // ---------- 5. La barra de arriba (día y preguntas) cabe entera ----------

    [TestCaseSource(nameof(ScreensAndTextSizes))]
    public void LaBarraDeArribaCabeEntera(Vector2 canvasSize, int textSize)
    {
        GameSettings.TextSizeLevel = textSize;
        LayoutPreview.SetSize(session, canvasSize);
        foreach (var (day, used) in new[] { (1, 0), (7, 5), (7, 4) })
        {
            session.ui.UpdateGameState(day, 7, used, 5);
            LayoutPreview.ShowOnly(session, "InterrogationPanel");
            TMP_Text hud = LayoutPreview.Find(session, "HudText").GetComponent<TMP_Text>();
            hud.ForceMeshUpdate(true, true);
            Assert.IsFalse(hud.isTextTruncated, $"«{hud.GetParsedText()}» cortado a {canvasSize.x:F0}x{canvasSize.y:F0}, texto {textSize}");
            Assert.GreaterOrEqual(hud.fontSize, Theme.MinReadableSize - 0.01f);
        }
    }

    // ---------- 3. El busto no se amplía por encima de la resolución del retrato ----------

    [Test]
    public void UnRetratoMasPequeñoQueSuCajaSeQuedaASuTamaño()
    {
        // Busto de 450 × 600 téxeles en una caja de 1000 × 1300 a densidad 1: sin ampliar (máximo 1)
        Vector2 size = PixelScale.Fit(new Vector2(1000f, 1300f), new Vector2(450f, 600f), 1f, 1f);
        Assert.AreEqual(450f, size.x, 0.5f);
        Assert.AreEqual(600f, size.y, 0.5f);
    }

    [Test]
    public void UnRetratoMasGrandeQueSuCajaSeReduceYConservaLaProporcion()
    {
        Vector2 size = PixelScale.Fit(new Vector2(240f, 400f), new Vector2(450f, 600f), 1f, 1f);
        Assert.AreEqual(240f, size.x, 0.5f, "llena el ancho");
        Assert.AreEqual(320f, size.y, 0.5f, "3:4");
    }

    [Test]
    public void SiSePermiteAmpliarEsPorMultiplosEnteros()
    {
        // Cabría a 2,22×: con máximo 3, se queda en 2× (cada téxel, 2 × 2 píxeles: nítido)
        Vector2 size = PixelScale.Fit(new Vector2(1000f, 1400f), new Vector2(450f, 600f), 1f, 3f);
        Assert.AreEqual(900f, size.x, 0.5f);
        Assert.AreEqual(1200f, size.y, 0.5f);
    }

    [Test]
    public void LaDensidadDeLaPantallaCuenta()
    {
        // 1440 × 3200: cada unidad del lienzo son 1,49 píxeles. Una caja de 400 × 533 son 596 × 794 píxeles reales:
        // más que los 450 × 600 téxeles del busto, así que se queda a 1:1 (302 × 403 unidades)
        float density = 3200f / LayoutPreview.CanvasSize(1440f, 3200f).y;
        Vector2 size = PixelScale.Fit(new Vector2(400f, 533f), new Vector2(450f, 600f), density, 1f);
        Assert.AreEqual(450f / density, size.x, 0.5f);
        Assert.AreEqual(600f / density, size.y, 0.5f);
    }

    // Decisión de Cristian (sesión C): 2× (múltiplo entero) si cabe dentro del tope de la cabecera; si no, 1×
    [Test]
    public void ElBustoPuedeIrA2xSiCabeYSiNo1x()
    {
        float max = ThemeManager.Current.portraitMaxMagnification;
        Assert.AreEqual(2f, max, "el tema permite 2×");
        Vector2 big = PixelScale.Fit(new Vector2(1000f, 1400f), new Vector2(450f, 600f), 1f, max);
        Assert.AreEqual(new Vector2(900f, 1200f), big, "cabe a 2,2×: se queda en 2×");
        Vector2 middle = PixelScale.Fit(new Vector2(800f, 1100f), new Vector2(450f, 600f), 1f, max);
        Assert.AreEqual(new Vector2(450f, 600f), middle, "a 1,8× no cabe 2×: 1×, nunca 1,8×");
        Vector2 huge = PixelScale.Fit(new Vector2(2000f, 3000f), new Vector2(450f, 600f), 1f, max);
        Assert.AreEqual(new Vector2(900f, 1200f), huge, "nunca más de 2×");
    }

    // La caja del busto es la que limita el tope del 40 %: el busto, a cualquier escala, no se sale de ella
    [TestCase(1080f, 1920f)]
    [TestCase(1080f, 2400f)]
    [TestCase(1080f, 3840f)]
    public void ElBustoNoSeSaleDelTopeDeLaCabecera(float w, float h)
    {
        LayoutPreview.SetSize(session, LayoutPreview.CanvasSize(w, h));
        LayoutPreview.ShowOnly(session, "InterrogationPanel");
        Rect bust = WorldRect(BustImage().rectTransform);
        Rect header = WorldRect(LayoutPreview.Find(session, "Cabecera (auto)"));
        Assert.LessOrEqual(bust.width, header.width * ThemeManager.Current.headerPortraitMaxShare + Tolerance);
    }

    [TestCase(1080f, 2400f)]
    [TestCase(1080f, 3840f)]
    public void ElBustoDelInterrogatorioNoPasaDelMaximo(float w, float h)
    {
        LayoutPreview.SetSize(session, LayoutPreview.CanvasSize(w, h));
        LayoutPreview.ShowOnly(session, "InterrogationPanel");
        RawImage bust = BustImage();
        Assert.IsNotNull(bust.texture, "la vista previa enseña un sospechoso");
        Vector2 texels = new Vector2(bust.texture.width * bust.uvRect.width, bust.texture.height * bust.uvRect.height);
        Rect shown = bust.rectTransform.rect;
        // En la vista previa una unidad del lienzo es un píxel (densidad 1)
        Assert.LessOrEqual(shown.width, texels.x * ThemeManager.Current.portraitMaxMagnification + Tolerance, "ancho");
        Assert.LessOrEqual(shown.height, texels.y * ThemeManager.Current.portraitMaxMagnification + Tolerance, "alto");
    }

    // ---------- 4. Sin contraluz azul en el busto (y se puede volver a encender desde el tema) ----------

    [Test]
    // Decisión de Cristian (sesión C): tampoco en la rueda de acusación. Los dos se encienden desde el tema
    public void ElBustoYLaRuedaNoLlevanContraluz()
    {
        var raw = new GameObject("retrato", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
        ArtGrading.Apply(raw, ArtGrading.Kind.LegacyBust);
        Assert.AreEqual(ThemeManager.Current.bustRimStrength, raw.material.GetFloat("_RimStrength"), 1e-4f, "busto");
        Assert.AreEqual(0f, ThemeManager.Current.bustRimStrength, "busto: por defecto, apagado");
        ArtGrading.Apply(raw, ArtGrading.Kind.LegacyPortrait);
        Assert.AreEqual(ThemeManager.Current.portraitRimStrength, raw.material.GetFloat("_RimStrength"), 1e-4f, "rueda y ficha");
        Assert.AreEqual(0f, ThemeManager.Current.portraitRimStrength, "rueda: por defecto, apagado");
        Object.DestroyImmediate(raw.gameObject);
    }

    [Test]
    public void LasFigurasDeLaRuedaSalenSinContraluz()
    {
        List<SuspectView> three = CaseLibrary.Stories[1].cast.Take(3).Select(SuspectView.From).ToList();
        session.ui.ShowAccusationPanel(three, canGoBack: true, contradictions: 0);
        LayoutPreview.ShowOnly(session, "AccusatonPanel");
        var figures = LayoutPreview.Find(session, "Rueda (auto)").GetComponentsInChildren<HeightLineupItem>()
            .Select(i => i.figure.GetComponent<RawImage>()).Where(r => r != null && r.material != null && r.material.HasProperty("_RimStrength")).ToList();
        Assert.IsNotEmpty(figures, "la rueda usa el material con relieve");
        foreach (RawImage figure in figures)
            Assert.AreEqual(0f, figure.material.GetFloat("_RimStrength"), 1e-4f, figure.name);
    }

    [Test]
    public void ElContraluzDelBustoSeEnciendeDesdeElTema()
    {
        Theme theme = Object.Instantiate(ThemeManager.Current);
        theme.bustRimStrength = 0.7f;
        ThemeManager.Override(theme);
        var raw = new GameObject("retrato", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
        ArtGrading.Apply(raw, ArtGrading.Kind.LegacyBust);
        Assert.AreEqual(0.7f, raw.material.GetFloat("_RimStrength"), 1e-4f);
        Object.DestroyImmediate(raw.gameObject);
    }

    [Test]
    public void ElBustoDelInterrogatorioUsaElMaterialSinContraluz()
    {
        LayoutPreview.ShowOnly(session, "InterrogationPanel");
        RawImage bust = BustImage();
        Assert.IsNotNull(bust.material);
        Assert.AreEqual(0f, bust.material.GetFloat("_RimStrength"), 1e-4f);
    }

    // ---------- 2. El sospechoso sale una sola vez: o busto o figura ----------

    [TestCase(PortraitComposition.Busto)]
    [TestCase(PortraitComposition.Figura)]
    public void ElSospechosoSaleUnaSolaVez(PortraitComposition composition)
    {
        OpenWith(t => t.interrogationComposition = composition);
        RectTransform panel = LayoutPreview.ShowOnly(session, "InterrogationPanel");

        List<RawImage> shown = panel.GetComponentsInChildren<RawImage>(false)
            .Where(r => r.texture != null
                        && (r == BustImage() || r.name == "Figura")).ToList();
        Assert.AreEqual(1, shown.Count, string.Join(", ", shown.Select(r => r.name)));
        Assert.AreEqual(composition == PortraitComposition.Busto ? BustImage() : (Graphic)shown[0], shown[0]);
        if (composition == PortraitComposition.Figura)
            Assert.AreEqual("Figura", shown[0].name);

        // El estado emocional va con el retrato que se ve
        RectTransform chip = LayoutPreview.Find(session, "Estado (auto)");
        Assert.IsTrue(chip.IsChildOf(shown[0].transform.parent), $"la etiqueta de estado está en {chip.parent.name}");
    }

    [Test]
    public void ConLaFiguraLaCabeceraEsSoloControles()
    {
        OpenWith(t => t.interrogationComposition = PortraitComposition.Figura);
        LayoutPreview.ShowOnly(session, "InterrogationPanel");
        Rect dropdown = WorldRect(LayoutPreview.Find(session, "SuspectDropdown"));
        Rect header = WorldRect(LayoutPreview.Find(session, "Cabecera (auto)"));
        Assert.GreaterOrEqual(dropdown.width, header.width - 2f * Tolerance - 1f, "a todo lo ancho");
        RawImage figure = LayoutPreview.Find(session, "Figura").GetComponent<RawImage>();
        Assert.GreaterOrEqual(figure.color.a, 0.85f, "la figura es el retrato: opaca, no tenue");
    }

    // ---------- 6. La rueda de reconocimiento usa el alto disponible ----------

    [TestCase(1080f, 1920f)]
    [TestCase(1080f, 2400f)]
    public void LaRuedaOcupaElSitio(float w, float h)
    {
        LayoutPreview.SetSize(session, LayoutPreview.CanvasSize(w, h));
        // Historia 2 el primer día: tres sospechosos (el caso de la captura)
        List<SuspectView> three = CaseLibrary.Stories[1].cast.Take(3).Select(SuspectView.From).ToList();
        session.ui.ShowAccusationPanel(three, canGoBack: true, contradictions: 0);
        LayoutPreview.ShowOnly(session, "AccusatonPanel");

        var lineup = LayoutPreview.Find(session, "Rueda (auto)").GetComponent<HeightLineup>();
        lineup.Relayout();
        float available = ((RectTransform)lineup.transform).rect.height - lineup.labelBand;
        // A escala real: la pared medida (hasta 2 m) llena al menos el 65 % del alto (antes, a 1080 × 2400: 50 %). Con tres
        // figuras de cuerpo entero en fila, el ancho manda: más solo se logra solapándolas más o con dos filas
        float pxPerCm = lineup.GetComponentsInChildren<HeightLineupItem>().Max(i => i.figure.rect.height / i.heightCm);
        float wall = HeightLineup.WallTopCm * pxPerCm;
        Assert.GreaterOrEqual(wall, 0.65f * available, $"la pared de 2 m mide {wall:F0} de {available:F0} disponibles");
    }

    // ---------- Ayudas ----------

    private void OpenWith(System.Action<Theme> change)
    {
        LayoutPreview.Close();
        Theme theme = Object.Instantiate(ThemeManager.Current);
        theme.hideFlags = HideFlags.HideAndDontSave; // Si no, abrir la escena lo descarga y vuelve el tema del asset
        change(theme);
        ThemeManager.Override(theme);
        session = LayoutPreview.Open(new Vector2(1080f, 1920f));
    }

    private RawImage BustImage()
    {
        return LayoutPreview.Find(session, "Retrato (auto)").GetComponentsInChildren<RawImage>(true).First();
    }

    private static Rect WorldRect(RectTransform rect)
    {
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        return new Rect(corners[0].x, corners[0].y, corners[2].x - corners[0].x, corners[2].y - corners[0].y);
    }

    private static float GroupAlpha(Transform t)
    {
        float alpha = 1f;
        for (Transform p = t; p != null; p = p.parent)
        {
            if (p.TryGetComponent(out CanvasGroup group))
            {
                alpha *= group.alpha;
                if (group.ignoreParentGroups)
                    break;
            }
        }
        return alpha;
    }

    // Recortado por una máscara (chat desplazado): fuera de ella ni se ve ni recibe toques
    private static bool ClippedAt(Graphic g, Vector2 point)
    {
        for (Transform p = g.transform.parent; p != null; p = p.parent)
        {
            if ((p.TryGetComponent(out RectMask2D _) || p.TryGetComponent(out Mask _)) && !WorldRect((RectTransform)p).Contains(point))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Cada control interactivo: dentro de la pantalla, de 48 dp como mínimo y sin nada encima en cinco puntos (centro y
    /// cuatro interiores). "Encima" = un gráfico dibujado después (orden de la jerarquía), fuera del control, que recibe
    /// toques o que se ve (alfa ≥ 0,5). No cuentan las capas a pantalla completa transparentes (viñeta) ni lo recortado.
    /// </summary>
    private static void CheckControls(RectTransform canvas, RectTransform panel, List<string> errors)
    {
        Rect screen = WorldRect(canvas);
        List<Graphic> drawOrder = canvas.GetComponentsInChildren<Graphic>(false).ToList();

        foreach (Selectable control in panel.GetComponentsInChildren<Selectable>(false))
        {
            if (control is Scrollbar || !control.interactable)
                continue;
            ScrollRect scroll = control.GetComponentInParent<ScrollRect>();
            if (scroll != null && scroll.content != null && control.transform.IsChildOf(scroll.content))
                continue; // Dentro de un scroll se desplaza hasta verse
            var rect = (RectTransform)control.transform;
            Rect r = WorldRect(rect);
            string name = rect.name;

            if (r.xMin < screen.xMin - Tolerance || r.xMax > screen.xMax + Tolerance || r.yMin < screen.yMin - Tolerance || r.yMax > screen.yMax + Tolerance)
                errors.Add($"fuera de pantalla: {name} {r}");
            if (r.width < Theme.MinTouchSize - Tolerance || r.height < Theme.MinTouchSize - Tolerance)
                errors.Add($"aplastado: {name} {r.width:F0}x{r.height:F0}");

            Graphic own = control.targetGraphic != null ? control.targetGraphic : control.GetComponent<Graphic>();
            int ownIndex = own != null ? drawOrder.IndexOf(own) : -1;
            var points = new[] { r.center, new Vector2(r.xMin + r.width * 0.2f, r.yMin + r.height * 0.3f), new Vector2(r.xMax - r.width * 0.2f, r.yMin + r.height * 0.3f),
                                 new Vector2(r.xMin + r.width * 0.2f, r.yMax - r.height * 0.3f), new Vector2(r.xMax - r.width * 0.2f, r.yMax - r.height * 0.3f) };
            foreach (Vector2 point in points)
            {
                for (int i = ownIndex + 1; i < drawOrder.Count; i++)
                {
                    Graphic g = drawOrder[i];
                    if (g.transform.IsChildOf(control.transform) || !g.isActiveAndEnabled)
                        continue;
                    Rect gr = WorldRect(g.rectTransform);
                    if (!gr.Contains(point) || ClippedAt(g, point))
                        continue;
                    bool fullScreen = gr.xMin <= screen.xMin + 1f && gr.xMax >= screen.xMax - 1f && gr.yMin <= screen.yMin + 1f && gr.yMax >= screen.yMax - 1f;
                    float alpha = g.color.a * GroupAlpha(g.transform);
                    bool blocksTouch = g.raycastTarget && alpha > 0f;
                    // Las figuras de la rueda se solapan a propósito (lineupOverlap): se ven encima del hueco vecino,
                    // pero no reciben el toque, que es del hueco (los huecos no se solapan)
                    bool lineupNeighbour = g.GetComponentInParent<HeightLineupItem>() != null && control.GetComponent<HeightLineupItem>() != null;
                    bool covers = !(g is TMP_Text) && !fullScreen && alpha >= 0.5f && !lineupNeighbour;
                    if (blocksTouch || covers)
                    {
                        errors.Add($"tapado: {name} por {g.name} ({(blocksTouch ? "recibe el toque" : "se ve encima")})");
                        goto nextControl;
                    }
                }
            }
            nextControl: ;
        }
    }
}
