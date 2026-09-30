using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// CAPA DE EFECTOS por encima de toda la UI (su propio Canvas, orden 50):
///   - Stamp: sello de tinta que golpea (contradicción, confidencial, finales);
///   - ClueCard: ficha de pista que cae girando, destella y vuela a la libreta;
///   - DayCard: hoja de calendario del nuevo día con el parte de la mañana (se cierra con un toque);
///   - Tension / Deliberation: viñeta que se cierra y latido en la acusación; pausa antes del veredicto;
///   - filtro noir (grano y viñeteado), opcional desde Ajustes.
/// Con "reducir animaciones" todo aparece en su estado final, sin movimiento, y dura menos.
/// </summary>
public class FxLayer : MonoBehaviour
{
    public const string LayerName = "Efectos (auto)";

    private RectTransform root;
    private readonly Queue<(string name, RectTransform target)> clueQueue = new Queue<(string, RectTransform)>();
    private bool clueRunning;
    private Image tension;
    private float tensionSince = -1f;
    private RectTransform heartbeatTarget;
    private RawImage grain;
    private Image vignette;
    private Texture2D grainTexture;
    private float nextGrain;

    private static Theme T => ThemeManager.Current;
    private static bool Instant => GameSettings.ReduceMotion || !Application.isPlaying;

    public static FxLayer Ensure(Canvas canvas)
    {
        if (canvas == null)
            return null;
        canvas = canvas.rootCanvas;

        Transform existing = canvas.transform.Find(LayerName);
        if (existing != null && existing.TryGetComponent(out FxLayer found))
            return found;

        var go = new GameObject(LayerName, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(canvas.transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.SetAsLastSibling();
        UIComponents.GetOrAdd<LayoutElement>(go).ignoreLayout = true;

        var own = go.AddComponent<Canvas>();
        own.overrideSorting = true;
        own.sortingOrder = 50;
        go.AddComponent<GraphicRaycaster>();

        var layer = go.AddComponent<FxLayer>();
        layer.root = rect;
        layer.BuildNoir();
        layer.RefreshNoir();
        GameSettings.Changed += layer.RefreshNoir;
        return layer;
    }

    private void OnDestroy()
    {
        GameSettings.Changed -= RefreshNoir;
        if (grainTexture != null)
            Destroy(grainTexture);
    }

    // ============================================
    // SELLO
    // ============================================

    /// <summary>
    /// Sello de tinta que baja y golpea. 'parent' null = en la capa de efectos, centrado; si no, dentro de
    /// ese panel (se queda con él). hold &lt; 0 = se queda; si no, se desvanece tras 'hold' segundos.
    /// </summary>
    public GameObject Stamp(string text, Color ink, float angle = -8f, float fontSize = 96f, RectTransform parent = null,
                            Vector2? anchor = null, float hold = 1.2f, Action onImpact = null)
    {
        RectTransform rect = NewRect(parent != null ? parent : root, "Sello (auto)");
        Vector2 a = anchor ?? new Vector2(0.5f, 0.55f);
        rect.anchorMin = rect.anchorMax = a;
        rect.localRotation = Quaternion.Euler(0f, 0f, angle);
        UIComponents.GetOrAdd<LayoutElement>(rect.gameObject).ignoreLayout = true;

        var frame = rect.gameObject.AddComponent<Image>();
        frame.sprite = UISprites.RoundedOutline(14, 6);
        frame.type = Image.Type.Sliced;
        frame.color = ink;
        frame.raycastTarget = false;

        TMP_Text label = NewText(rect, text, fontSize, ink);
        label.font = UIFactory.TitleFont(); // Los sellos, en letra de máquina
        label.fontStyle = FontStyles.Bold;
        label.characterSpacing = 10f;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.ForceMeshUpdate();
        Vector2 size = label.GetPreferredValues(text, float.PositiveInfinity, float.PositiveInfinity);

        // Nunca más ancho que el 90 % de donde se estampa (contando el giro)
        RectTransform space = parent != null ? parent : root;
        float maxWidth = Mathf.Max(200f, space.rect.width * 0.9f) - 72f;
        float rotated = size.x * Mathf.Cos(angle * Mathf.Deg2Rad) + size.y * Mathf.Abs(Mathf.Sin(angle * Mathf.Deg2Rad));
        if (rotated > maxWidth)
        {
            label.fontSize = fontSize * maxWidth / rotated;
            size = label.GetPreferredValues(text, float.PositiveInfinity, float.PositiveInfinity);
        }
        rect.sizeDelta = new Vector2(size.x + 72f, size.y + 36f);
        Stretch(label.rectTransform, 0f, 0f);

        var group = rect.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;

        Action impact = () =>
        {
            SoundManager.Play(Sfx.Stamp, 1f, 0.06f);
            onImpact?.Invoke();
        };
        StartCoroutine(StampRoutine(rect, group, hold, impact));
        return rect.gameObject;
    }

    private IEnumerator StampRoutine(RectTransform rect, CanvasGroup group, float hold, Action onImpact)
    {
        if (Instant)
        {
            rect.localScale = Vector3.one;
            group.alpha = 1f;
            onImpact?.Invoke();
        }
        else
        {
            const float fall = 0.32f;
            bool hit = false;
            for (float e = 0f; e < fall; e += Time.unscaledDeltaTime)
            {
                float t = e / fall;
                rect.localScale = Vector3.one * FxCurves.StampScale(t);
                group.alpha = FxCurves.StampAlpha(t);
                if (!hit && t >= 0.35f)
                {
                    hit = true;
                    onImpact?.Invoke();
                }
                yield return null;
            }
            rect.localScale = Vector3.one;
            group.alpha = 1f;
            if (!hit)
                onImpact?.Invoke();
        }

        if (hold < 0f)
            yield break;

        yield return new WaitForSecondsRealtime(Instant ? hold * 0.7f : hold);
        const float fade = 0.4f;
        for (float e = 0f; e < fade; e += Time.unscaledDeltaTime)
        {
            group.alpha = 1f - e / fade;
            yield return null;
        }
        Destroy(rect.gameObject);
    }

    // ============================================
    // DESTELLO Y SACUDIDA
    // ============================================

    public void Flash(Color color, float maxAlpha = 0.25f)
    {
        if (Instant)
            return;
        RectTransform rect = NewRect(root, "Destello (auto)");
        Stretch(rect, 0f, 0f);
        var image = rect.gameObject.AddComponent<Image>();
        image.raycastTarget = false;
        StartCoroutine(UIAnimations.Animate(T.contradictionAnimDuration,
            t => image.color = new Color(color.r, color.g, color.b, maxAlpha * Easing.Pulse(t)),
            () => Destroy(rect.gameObject)));
    }

    public void Shake(RectTransform target, float amplitude = 10f, float duration = 0.3f)
    {
        UIAnimations.Shake(this, target, amplitude, duration);
    }

    // ============================================
    // FICHA DE PISTA
    // ============================================

    /// <summary>
    /// Ficha de pista nueva: cae girando, destella, y vuela hasta 'flyTo' (el botón de la libreta).
    /// Varias seguidas hacen cola.
    /// </summary>
    public void ClueCard(string clueName, RectTransform flyTo)
    {
        clueQueue.Enqueue((clueName, flyTo));
        if (!clueRunning)
            StartCoroutine(RunClueQueue());
    }

    public bool ClueCardsPending => clueRunning || clueQueue.Count > 0;

    private IEnumerator RunClueQueue()
    {
        clueRunning = true;
        while (clueQueue.Count > 0)
        {
            var (name, target) = clueQueue.Dequeue();
            yield return ClueCardRoutine(name, target, clueQueue.Count > 0);
        }
        clueRunning = false;
    }

    private IEnumerator ClueCardRoutine(string clueName, RectTransform flyTo, bool hurry)
    {
        RectTransform card = NewRect(root, "Ficha de pista (auto)");
        card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.72f);
        card.sizeDelta = new Vector2(820f, 250f);
        var paper = card.gameObject.AddComponent<Image>();
        paper.sprite = UISprites.Rounded(10);
        paper.type = Image.Type.Sliced;
        paper.color = T.paper;
        paper.raycastTarget = false;
        card.gameObject.AddComponent<RectMask2D>().softness = new Vector2Int(2, 2);

        // Cinta adhesiva arriba
        RectTransform tape = NewRect(card, "Cinta");
        tape.anchorMin = tape.anchorMax = new Vector2(0.5f, 1f);
        tape.sizeDelta = new Vector2(180f, 40f);
        tape.localRotation = Quaternion.Euler(0f, 0f, -4f);
        var tapeImage = tape.gameObject.AddComponent<Image>();
        tapeImage.color = new Color(1f, 0.97f, 0.85f, 0.55f);
        tapeImage.raycastTarget = false;

        TMP_Text kicker = NewText(card, "PISTA NUEVA", T.secondarySize, T.paperInk);
        kicker.font = UIFactory.TitleFont();
        kicker.characterSpacing = 14f;
        kicker.fontStyle = FontStyles.Bold;
        var kr = kicker.rectTransform;
        kr.anchorMin = new Vector2(0f, 0.62f);
        kr.anchorMax = new Vector2(1f, 0.9f);
        kr.offsetMin = kr.offsetMax = Vector2.zero;

        TMP_Text title = NewText(card, clueName, T.headingSize, T.paperText);
        TextStyle.Set(title, TextStyle.Mode.MultiLine, T.headingSize);
        title.alignment = TextAlignmentOptions.Center;
        var tr = title.rectTransform;
        tr.anchorMin = new Vector2(0f, 0.08f);
        tr.anchorMax = new Vector2(1f, 0.62f);
        tr.offsetMin = new Vector2(40f, 0f);
        tr.offsetMax = new Vector2(-40f, 0f);

        // Banda de luz del destello
        RectTransform glint = NewRect(card, "Destello");
        glint.anchorMin = glint.anchorMax = new Vector2(0f, 0.5f);
        glint.sizeDelta = new Vector2(90f, 520f);
        glint.localRotation = Quaternion.Euler(0f, 0f, -22f);
        var glintImage = glint.gameObject.AddComponent<Image>();
        glintImage.sprite = UISprites.Radial();
        glintImage.color = new Color(1f, 1f, 1f, 0f);
        glintImage.raycastTarget = false;

        var group = card.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;

        Vector2 rest = card.anchoredPosition;
        if (Instant)
        {
            group.alpha = 1f;
            yield return new WaitForSecondsRealtime(hurry ? 0.8f : 1.6f);
        }
        else
        {
            float fall = T.clueAnimDuration;
            for (float e = 0f; e < fall; e += Time.unscaledDeltaTime)
            {
                float t = e / fall;
                card.localRotation = Quaternion.Euler(FxCurves.CardFallAngle(t), 0f, Mathf.Lerp(-6f, -1.5f, t));
                card.anchoredPosition = rest + new Vector2(0f, FxCurves.CardFallOffset(t));
                group.alpha = Mathf.Clamp01(t * 3f);
                yield return null;
            }
            card.localRotation = Quaternion.Euler(0f, 0f, -1.5f);
            card.anchoredPosition = rest;

            const float glintTime = 0.5f;
            for (float e = 0f; e < glintTime; e += Time.unscaledDeltaTime)
            {
                float t = e / glintTime;
                glint.anchoredPosition = new Vector2(FxCurves.GlintPosition(t) * card.rect.width, 0f);
                glintImage.color = new Color(1f, 1f, 1f, 0.55f * Mathf.Sin(t * Mathf.PI));
                yield return null;
            }
            glintImage.color = new Color(1f, 1f, 1f, 0f);

            yield return new WaitForSecondsRealtime(hurry ? 0.5f : 1.1f);

            // Vuela a la libreta
            if (flyTo != null && flyTo.gameObject.activeInHierarchy)
            {
                Vector3 from = card.position;
                Vector3 to = flyTo.position;
                const float fly = 0.45f;
                for (float e = 0f; e < fly; e += Time.unscaledDeltaTime)
                {
                    float t = Easing.InOutSine(e / fly);
                    card.position = Vector3.Lerp(from, to, t);
                    card.localScale = Vector3.one * Mathf.Lerp(1f, 0.12f, t);
                    group.alpha = 1f - t * 0.6f;
                    yield return null;
                }
                UIAnimations.Pop(this, flyTo);
            }
        }

        Destroy(card.gameObject);
    }

    // ============================================
    // CAMBIO DE DÍA
    // ============================================

    /// <summary>
    /// Hoja de calendario: pasa del día anterior al nuevo y escribe el parte de la mañana. Un toque la cierra
    /// (el primero completa el texto si aún se está escribiendo).
    /// </summary>
    public GameObject DayCard(int day, int maxDays, string report, Action onClosed = null)
    {
        RectTransform veil = NewRect(root, "Nuevo dia (auto)");
        Stretch(veil, 0f, 0f);
        var veilImage = veil.gameObject.AddComponent<Image>();
        veilImage.color = new Color(T.background.r, T.background.g, T.background.b, 0.94f);
        var group = veil.gameObject.AddComponent<CanvasGroup>();

        RectTransform page = NewRect(veil, "Hoja");
        page.anchorMin = page.anchorMax = new Vector2(0.5f, 0.62f);
        page.sizeDelta = new Vector2(440f, 480f);
        var pageImage = page.gameObject.AddComponent<Image>();
        pageImage.sprite = UISprites.Rounded(18);
        pageImage.type = Image.Type.Sliced;
        pageImage.color = T.paper;
        pageImage.raycastTarget = false;

        // Cabecera roja del calendario
        RectTransform header = NewRect(page, "Cabecera");
        header.anchorMin = new Vector2(0f, 0.8f);
        header.anchorMax = Vector2.one;
        header.offsetMin = header.offsetMax = Vector2.zero;
        var headerImage = header.gameObject.AddComponent<Image>();
        headerImage.sprite = UISprites.Rounded(18);
        headerImage.type = Image.Type.Sliced;
        headerImage.color = T.calendarRed;
        headerImage.raycastTarget = false;
        TMP_Text headerText = NewText(header, "DÍA", T.secondarySize, T.paper);
        headerText.characterSpacing = 18f;
        headerText.fontStyle = FontStyles.Bold;
        Stretch(headerText.rectTransform, 0f, 0f);

        TMP_Text number = NewText(page, (day - 1).ToString(), 220f, T.paperText);
        number.font = UIFactory.TitleFont();
        number.fontStyle = FontStyles.Bold;
        var nr = number.rectTransform;
        nr.anchorMin = new Vector2(0f, 0.18f);
        nr.anchorMax = new Vector2(1f, 0.8f);
        nr.offsetMin = nr.offsetMax = Vector2.zero;

        TMP_Text of = NewText(page, $"de {maxDays}", T.secondarySize, T.paperInk);
        var or = of.rectTransform;
        or.anchorMin = new Vector2(0f, 0.04f);
        or.anchorMax = new Vector2(1f, 0.2f);
        or.offsetMin = or.offsetMax = Vector2.zero;

        // Parte de la mañana
        TMP_Text body = NewText(veil, string.IsNullOrEmpty(report) ? "" : "Parte de la mañana:\n" + report, T.bodySize, T.textPrimary);
        body.fontStyle = FontStyles.Italic;
        TextStyle.Set(body, TextStyle.Mode.MultiLine, T.bodySize);
        var br = body.rectTransform;
        br.anchorMin = new Vector2(0f, 0.2f);
        br.anchorMax = new Vector2(1f, 0.4f);
        br.offsetMin = new Vector2(80f, 0f);
        br.offsetMax = new Vector2(-80f, 0f);
        Typewriter typewriter = body.gameObject.AddComponent<Typewriter>();
        if (!Instant)
            body.maxVisibleCharacters = 0; // Se escribe tras el cambio de hoja

        TMP_Text hint = NewText(veil, "Toca para continuar", T.secondarySize, T.textSecondary);
        var hr = hint.rectTransform;
        hr.anchorMin = new Vector2(0f, 0.08f);
        hr.anchorMax = new Vector2(1f, 0.13f);
        hr.offsetMin = hr.offsetMax = Vector2.zero;

        var closer = veil.gameObject.AddComponent<TapHandler>();
        bool closing = false;
        closer.onTap = () =>
        {
            if (typewriter.IsTyping)
            {
                typewriter.Complete();
                return;
            }
            if (closing)
                return;
            closing = true;
            StartCoroutine(FadeAndDestroy(veil.gameObject, group, 0.3f, onClosed));
        };

        StartCoroutine(DayCardRoutine(page, number, day, group, typewriter));
        return veil.gameObject;
    }

    private IEnumerator DayCardRoutine(RectTransform page, TMP_Text number, int day, CanvasGroup group, Typewriter typewriter)
    {
        if (Instant)
        {
            group.alpha = 1f;
            number.text = day.ToString();
            yield break;
        }

        yield return UIAnimations.Animate(0.25f, t => { if (group != null) group.alpha = t; });
        yield return new WaitForSecondsRealtime(0.25f);
        if (page == null)
            yield break; // Se cerró con un toque antes de tiempo

        // La hoja del día anterior se arranca hacia arriba y aparece la nueva
        const float flip = 0.5f;
        bool swapped = false;
        for (float e = 0f; e < flip; e += Time.unscaledDeltaTime)
        {
            float t = e / flip;
            float angle = t < 0.5f ? Mathf.Lerp(0f, 90f, t * 2f) : Mathf.Lerp(-90f, 0f, (t - 0.5f) * 2f);
            if (!swapped && t >= 0.5f)
            {
                swapped = true;
                number.text = day.ToString();
            }
            page.localRotation = Quaternion.Euler(angle, 0f, 0f);
            yield return null;
            if (page == null)
                yield break;
        }
        page.localRotation = Quaternion.identity;
        number.text = day.ToString();
        UIAnimations.Pop(this, page);

        typewriter.Reveal(0, 1f);
    }

    private IEnumerator FadeAndDestroy(GameObject target, CanvasGroup group, float duration, Action then)
    {
        if (!Instant)
        {
            for (float e = 0f; e < duration; e += Time.unscaledDeltaTime)
            {
                if (group != null)
                    group.alpha = 1f - e / duration;
                yield return null;
            }
        }
        Destroy(target);
        then?.Invoke();
    }

    // ============================================
    // ACUSACIÓN
    // ============================================

    /// <summary>
    /// Tensión de la acusación: la viñeta se cierra poco a poco y 'pulse' late como un corazón.
    /// </summary>
    public void SetTension(bool on, RectTransform pulse = null)
    {
        if (tension == null)
        {
            RectTransform rect = NewRect(root, "Tension (auto)");
            Stretch(rect, 0f, 0f);
            rect.SetAsFirstSibling();
            tension = rect.gameObject.AddComponent<Image>();
            tension.sprite = UISprites.Vignette();
            tension.raycastTarget = false;
        }

        if (heartbeatTarget != null)
            heartbeatTarget.localScale = Vector3.one;
        heartbeatTarget = on ? pulse : null;
        tensionSince = on ? Time.unscaledTime : -1f;
        tension.gameObject.SetActive(on);
        tension.color = new Color(0f, 0f, 0f, on && Instant ? 0.6f : 0f);
    }

    /// <summary>
    /// Pausa antes del veredicto: pantalla a negro, "El jurado delibera…" y latido. Después, 'then'.
    /// </summary>
    public void Deliberation(Action then)
    {
        RectTransform veil = NewRect(root, "Deliberacion (auto)");
        Stretch(veil, 0f, 0f);
        var image = veil.gameObject.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0f); // Bloquea toques mientras delibera
        TMP_Text text = NewText(veil, "El jurado delibera…", T.headingSize, T.textPrimary);
        text.fontStyle = FontStyles.Italic;
        Stretch(text.rectTransform, 40f, 0f);
        text.alpha = 0f;
        StartCoroutine(DeliberationRoutine(veil, image, text, then));
    }

    private IEnumerator DeliberationRoutine(RectTransform veil, Image image, TMP_Text text, Action then)
    {
        float fade = Instant ? 0f : 0.5f;
        float wait = Instant ? 0.5f : T.deliberationSeconds;

        for (float e = 0f; e < fade; e += Time.unscaledDeltaTime)
        {
            image.color = new Color(0f, 0f, 0f, e / fade);
            yield return null;
        }
        image.color = Color.black;

        for (float e = 0f; e < wait; e += Time.unscaledDeltaTime)
        {
            float beat = Instant ? 0f : FxCurves.Heartbeat(e / 0.8f);
            text.alpha = Mathf.Clamp01(e / 0.4f) * (0.75f + 0.25f * beat);
            text.rectTransform.localScale = Vector3.one * (1f + 0.02f * beat);
            yield return null;
        }

        SetTension(false);
        then?.Invoke();

        for (float e = 0f; e < fade; e += Time.unscaledDeltaTime)
        {
            float a = 1f - e / fade;
            image.color = new Color(0f, 0f, 0f, a);
            text.alpha = a;
            yield return null;
        }
        Destroy(veil.gameObject);
    }

    // ============================================
    // TUTORIAL
    // ============================================

    private GameObject currentHint;

    public bool HintVisible => currentHint != null;

    /// <summary>
    /// Retira la indicación abierta (p. ej. al abrir un aviso: esta capa va por encima de todo).
    /// </summary>
    public void CloseHint()
    {
        if (currentHint != null)
            Destroy(currentHint);
        currentHint = null;
    }

    /// <summary>
    /// Indicación junto a 'target' (encima si está en la mitad de abajo, debajo si no), con un aro que late
    /// alrededor. "Entendido" la cierra; "Saltar tutorial", también todas las siguientes.
    /// </summary>
    public GameObject Hint(string text, RectTransform target, Action onOk, Action onSkip)
    {
        if (currentHint != null)
            Destroy(currentHint);

        RectTransform holder = NewRect(root, "Indicacion (auto)");
        Stretch(holder, 0f, 0f);
        currentHint = holder.gameObject;
        Canvas.ForceUpdateCanvases();

        // Rectángulo del objetivo en el espacio de la capa
        Rect goal = new Rect(root.rect.center, Vector2.zero);
        if (target != null)
        {
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            Vector2 min = root.InverseTransformPoint(corners[0]);
            Vector2 max = root.InverseTransformPoint(corners[2]);
            goal = Rect.MinMaxRect(min.x, min.y, max.x, max.y);

            RectTransform ring = NewRect(holder, "Aro");
            ring.anchorMin = ring.anchorMax = new Vector2(0.5f, 0.5f);
            ring.anchoredPosition = goal.center - root.rect.center;
            ring.sizeDelta = goal.size + new Vector2(20f, 20f);
            var ringImage = ring.gameObject.AddComponent<Image>();
            ringImage.sprite = UISprites.RoundedOutline(18, 5);
            ringImage.type = Image.Type.Sliced;
            ringImage.color = T.accent;
            ringImage.raycastTarget = false;
            if (!Instant)
                StartCoroutine(PulseRing(ringImage, holder.gameObject));
        }

        RectTransform card = NewRect(holder, "Tarjeta");
        card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
        card.sizeDelta = new Vector2(Mathf.Min(900f, root.rect.width - 64f), 0f);
        var cardImage = card.gameObject.AddComponent<Image>();
        cardImage.sprite = UISprites.Rounded(20);
        cardImage.type = Image.Type.Sliced;
        cardImage.color = T.paper;
        var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(36, 36, 28, 24);
        layout.spacing = 16f;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        card.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        TMP_Text body = NewText(card, text, T.bodySize, T.paperText);
        body.alignment = TextAlignmentOptions.Left;
        body.textWrappingMode = TextWrappingModes.Normal;
        TextStyle.Set(body, TextStyle.Mode.Scrolling, T.bodySize);

        RectTransform row = NewRect(card, "Botones");
        var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 16f;
        rowLayout.childControlWidth = rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = true;
        var rowElement = row.gameObject.AddComponent<LayoutElement>();
        rowElement.minHeight = rowElement.preferredHeight = Theme.MinTouchSize;

        HintButton(row, "Saltar tutorial", T.paperInk, new Color(0f, 0f, 0f, 0.06f), () =>
        {
            Destroy(holder.gameObject);
            onSkip?.Invoke();
        });
        HintButton(row, "Entendido", T.buttonPrimaryText, T.buttonPrimary, () =>
        {
            Destroy(holder.gameObject);
            onOk?.Invoke();
        });

        // Encima del objetivo si está en la mitad de abajo; debajo si está arriba
        LayoutRebuilder.ForceRebuildLayoutImmediate(card);
        float height = card.rect.height;
        bool above = target == null || goal.center.y < root.rect.center.y;
        float y = above ? goal.yMax + 40f + height / 2f : goal.yMin - 40f - height / 2f;
        float half = root.rect.height / 2f;
        y = Mathf.Clamp(y, root.rect.yMin + height / 2f + 24f, root.rect.yMax - height / 2f - 24f);
        card.anchoredPosition = new Vector2(0f, y - root.rect.center.y);

        // Flecha hacia el objetivo
        if (target != null)
        {
            RectTransform arrow = NewRect(card, "Flecha");
            arrow.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            arrow.anchorMin = arrow.anchorMax = new Vector2(0.5f, above ? 0f : 1f);
            float x = Mathf.Clamp(goal.center.x - root.rect.center.x, -card.sizeDelta.x / 2f + 60f, card.sizeDelta.x / 2f - 60f);
            arrow.anchoredPosition = new Vector2(x, 0f);
            arrow.sizeDelta = new Vector2(34f, 34f);
            arrow.localRotation = Quaternion.Euler(0f, 0f, 45f);
            var arrowImage = arrow.gameObject.AddComponent<Image>();
            arrowImage.color = T.paper;
            arrowImage.raycastTarget = false;
            arrow.SetAsFirstSibling();
        }

        if (!Instant)
            UIAnimations.Pop(this, card);
        return holder.gameObject;
    }

    private void HintButton(RectTransform row, string label, Color textColor, Color background, Action onClick)
    {
        RectTransform rect = NewRect(row, label);
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = UISprites.Rounded(14);
        image.type = Image.Type.Sliced;
        image.color = background;
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => onClick());
        rect.gameObject.AddComponent<ClickSound>();
        TMP_Text text = NewText(rect, label, T.bodySize, textColor);
        TextStyle.Set(text, TextStyle.Mode.OneLine, T.bodySize);
        Stretch(text.rectTransform, 12f, 6f);
    }

    private IEnumerator PulseRing(Image ring, GameObject owner)
    {
        while (owner != null && ring != null)
        {
            float k = 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 4f);
            ring.color = new Color(T.accent.r, T.accent.g, T.accent.b, k);
            yield return null;
        }
    }

    // ============================================
    // FILTRO NOIR
    // ============================================

    private void BuildNoir()
    {
        RectTransform v = NewRect(root, "Viñeta noir (auto)");
        Stretch(v, 0f, 0f);
        vignette = v.gameObject.AddComponent<Image>();
        vignette.sprite = UISprites.Vignette();
        vignette.raycastTarget = false;

        RectTransform g = NewRect(root, "Grano (auto)");
        Stretch(g, 0f, 0f);
        grain = g.gameObject.AddComponent<RawImage>();
        grainTexture = NoiseTexture(128);
        grain.texture = grainTexture;
        grain.raycastTarget = false;
    }

    public void RefreshNoir()
    {
        if (grain == null)
            return;
        bool on = GameSettings.NoirFilter;
        grain.gameObject.SetActive(on && T.grainIntensity > 0f);
        vignette.gameObject.SetActive(on && T.vignetteIntensity > 0f);
        grain.color = new Color(1f, 1f, 1f, T.grainIntensity);
        vignette.color = new Color(0f, 0f, 0f, T.vignetteIntensity);
        grain.transform.SetAsLastSibling();
        vignette.transform.SetSiblingIndex(grain.transform.GetSiblingIndex());
    }

    private static Texture2D NoiseTexture(int size)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "Grano (auto)", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Point, hideFlags = HideFlags.DontSave
        };
        var random = new System.Random(5);
        var pixels = new Color32[size * size];
        for (int i = 0; i < pixels.Length; i++)
        {
            byte v = (byte)random.Next(0, 256);
            pixels[i] = new Color32(v, v, v, (byte)random.Next(60, 256));
        }
        texture.SetPixels32(pixels);
        texture.Apply(false, false);
        return texture;
    }

    private void Update()
    {
        // Grano a 24 imágenes por segundo, como el cine
        if (grain != null && grain.gameObject.activeSelf && !GameSettings.ReduceMotion && Time.unscaledTime >= nextGrain)
        {
            nextGrain = Time.unscaledTime + 1f / 24f;
            Vector2 tiles = root.rect.size / 256f;
            grain.uvRect = new Rect(UnityEngine.Random.value, UnityEngine.Random.value, tiles.x, tiles.y);
        }

        if (tension != null && tensionSince >= 0f && !Instant)
        {
            float seconds = Time.unscaledTime - tensionSince;
            float close = FxCurves.VignetteClose(seconds * 0.25f);
            float beat = FxCurves.Heartbeat(seconds / 0.85f);
            tension.color = new Color(0f, 0f, 0f, 0.15f + 0.35f * close + 0.08f * beat * close);
            if (heartbeatTarget != null)
                heartbeatTarget.localScale = Vector3.one * (1f + 0.012f * beat * close);
        }
    }

    // ============================================
    // UTILIDADES
    // ============================================

    private static RectTransform NewRect(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.SetAsLastSibling();
        go.AddComponent<ThemeRole>().role = UIRole.Ignore;
        return rect;
    }

    private static TMP_Text NewText(Transform parent, string text, float size, Color color)
    {
        RectTransform rect = NewRect(parent, "Texto");
        var tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.font = UIFactory.DefaultFont();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static void Stretch(RectTransform rect, float x, float y)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(x, y);
        rect.offsetMax = new Vector2(-x, -y);
    }
}

/// <summary>
/// Toque sobre una capa (la hoja del nuevo día).
/// </summary>
public class TapHandler : MonoBehaviour, IPointerClickHandler
{
    public Action onTap;

    public void OnPointerClick(PointerEventData eventData)
    {
        onTap?.Invoke();
    }
}
