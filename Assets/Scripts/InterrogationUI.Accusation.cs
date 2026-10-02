using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Acusación, rueda de reconocimiento y presentación de los finales (ver InterrogationUI.cs).
/// </summary>
public partial class InterrogationUI
{
    // ============================================
    // ACUSACIÓN
    // ============================================

    private int accusationContradictions;
    private TMP_Dropdown keyClueDropdown; // "Prueba clave" (opcional): la pista que demuestra la acusación

    private void FillKeyClueOptions()
    {
        if (keyClueDropdown == null)
            return;
        var options = new List<string> { GameTexts.KeyClueNone };
        options.AddRange(evidenceOptions.ConvertAll(c => c.playerName));
        keyClueDropdown.ClearOptions();
        keyClueDropdown.AddOptions(options);
        keyClueDropdown.SetValueWithoutNotify(0);
        keyClueDropdown.gameObject.SetActive(evidenceOptions.Count > 0); // Sin pistas no hay nada que elegir
    }

    // Lo que llevas a la acusación, bajo la pregunta (sin decir qué pista incrimina a quién). Se rehace al
    // cambiar de tema: el color va en el texto
    private void RefreshAccusationPrompt()
    {
        if (accusationPromptText == null)
            return;
        accusationPromptText.text = $"{GameTexts.AccusationPrompt}\n<size=90%><color={Theme.Hex(T.textSecondary)}>" +
                                    $"{GameTexts.AccusationSummary(evidenceOptions.Count, accusationContradictions)}</color></size>";
    }

    public void ShowAccusationPanel(List<SuspectView> options, bool canGoBack, int contradictions = 0)
    {
        ShowPanel(accusationPanel);
        accusationOptions = options;
        accusationContradictions = contradictions;
        RefreshAccusationPrompt();
        FillKeyClueOptions();
        // El título está dentro de la columna de la distribución: se busca en todo el panel
        if (accusationPanel != null)
        {
            foreach (TMP_Text t in accusationPanel.GetComponentsInChildren<TMP_Text>(true))
            {
                if (t.name == "AccusationTitleText")
                    t.text = GameTexts.AccusationTitle(dayValue >= maxDaysValue || !canGoBack);
            }
        }
        fx?.SetTension(true, accusationPanel != null ? (RectTransform)accusationPanel.transform : null);
        SoundManager.Play(Sfx.Heartbeat, 0.8f);
        SoundManager.PlayMusic(Music.Tension);
        EnsureAccusationBackButton();
        if (accusationBackButton != null)
            accusationBackButton.gameObject.SetActive(canGoBack);

        if (accusationDropdown != null)
        {
            accusationDropdown.ClearOptions();
            accusationDropdown.AddOptions(options.ConvertAll(v => v.displayName));
            accusationDropdown.onValueChanged.RemoveListener(MarkLineup);
            accusationDropdown.onValueChanged.AddListener(MarkLineup);
        }
        else
        {
            Debug.LogError("[InterrogationUI] ¡accusationDropdown es NULL!");
        }
        FillLineup(options);
    }

    /// <summary>
    /// Vuelve al interrogatorio desde el panel de acusación.
    /// </summary>
    public void ShowInterrogation()
    {
        fx?.SetTension(false);
        ShowPanel(interrogationPanel);
        SetInputEnabled(true);
        RefreshConversationView();
    }

    /// <summary>
    /// Si la escena no tiene botón "Volver" en la acusación, lo crea clonando el de acusar, debajo de él.
    /// </summary>
    private void EnsureAccusationBackButton()
    {
        if (accusationBackButton != null || accuseButton == null)
            return;

        GameObject clone = Instantiate(accuseButton.gameObject, accuseButton.transform.parent);
        clone.name = "AccusationBackButton (auto)";
        var rect = (RectTransform)clone.transform;
        var source = (RectTransform)accuseButton.transform;
        rect.anchoredPosition = source.anchoredPosition - new Vector2(0f, source.rect.height + T.spacing);

        accusationBackButton = clone.GetComponent<Button>();
        UIComponents.SetOnlyListener(accusationBackButton, () => gameManager.CancelAccusation());

        TMP_Text label = clone.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
            label.text = "Volver";

        ThemeApplier.Apply(clone.transform);

        // En la columna de la acusación, debajo de "Acusar"
        if (clone.transform.parent.GetComponent<VerticalLayoutGroup>() != null)
        {
            LayoutKit.Put(clone.transform, (RectTransform)clone.transform.parent, height: 130f);
            LayoutKit.Label(accusationBackButton, "Volver");

            // Volver es la acción secundaria: no compite en color con "Acusar"
            UIComponents.GetOrAdd<ThemeRole>(clone).role = UIRole.SecondaryButton;
            ThemeApplier.Apply(clone.transform);
        }
    }

    public void OnAccuseClick()
    {
        if (accusationDropdown != null && accusationDropdown.value < accusationOptions.Count)
        {
            string accusedId = accusationOptions[accusationDropdown.value].id;
            // La prueba clave (opcional): la opción 0 es "ninguna"
            string keyClueId = keyClueDropdown != null && keyClueDropdown.value > 0 && keyClueDropdown.value <= evidenceOptions.Count
                ? evidenceOptions[keyClueDropdown.value - 1].id : null;
            if (accuseButton != null)
                accuseButton.interactable = false; // Un solo veredicto
            SoundManager.Play(Sfx.Accusation);
            SoundManager.PlayMusic(Music.None);
            if (fx != null)
                fx.Deliberation(() => gameManager.MakeAccusation(accusedId, keyClueId));
            else
                gameManager.MakeAccusation(accusedId, keyClueId);
        }
    }

    // ============================================
    // RESULTADO (MEJORADO)
    // ============================================

    private GameObject endingStamp;
    private RectTransform lineup;
    private readonly List<GameObject> lineupSelection = new List<GameObject>();

    /// <summary>
    /// Rellena la rueda de reconocimiento con los bustos de los sospechosos; tocar uno lo elige.
    /// </summary>
    // Descartado en la rueda: por una pista ya encontrada (byClue) o por la nota del propio jugador
    private bool IsCleared(string id, out bool byClue)
    {
        byClue = gameManager != null && gameManager.IsClearedByClue(id);
        return byClue || (gameManager != null && gameManager.Notes.TryGetValue(id, out SuspectNote note)
                          && note == SuspectNote.Descartado);
    }

    private void FillLineup(List<SuspectView> options)
    {
        if (lineup == null)
            return;

        for (int i = lineup.childCount - 1; i >= 0; i--)
            DestroyImmediateOrLater(lineup.GetChild(i).gameObject);
        lineupSelection.Clear();

        // Columnas y tamaño de celda según cuántos hay y el hueco disponible (y se reajusta si cambia)
        // Con alguien descartado, el pie lleva dos líneas (nombre tachado y motivo) a tamaño legible
        bool anyCleared = options.Exists(v => IsCleared(v.id, out _));
        float labelHeight = anyCleared ? 96f : 64f;
        bool singleRow = T.lineupSingleRow;
        // Una fila con alturas reales (sin cuadrícula) o la cuadrícula de bustos de antes, según el tema
        // Un solo LayoutGroup por objeto: se cambia uno por otro
        HeightLineup heights = null;
        if (singleRow)
        {
            var gridFit = lineup.GetComponent<GridFit>(); // Primero: depende de la cuadrícula
            if (gridFit != null)
                DestroyImmediate(gridFit);
            var grid = lineup.GetComponent<GridLayoutGroup>();
            if (grid != null)
                DestroyImmediate(grid);
            heights = UIComponents.GetOrAdd<HeightLineup>(lineup.gameObject);
            heights.labelBand = labelHeight;
            heights.marks = lineup.parent != null ? lineup.parent.Find("Alturas") as RectTransform : null;
        }
        else
        {
            var row = lineup.GetComponent<HeightLineup>();
            if (row != null)
                DestroyImmediate(row);
            if (lineup.GetComponent<GridLayoutGroup>() == null)
            {
                var grid = lineup.gameObject.AddComponent<GridLayoutGroup>();
                grid.spacing = new Vector2(T.spacing, T.spacing);
                grid.childAlignment = TextAnchor.MiddleCenter;
            }
            GridFit fit = UIComponents.GetOrAdd<GridFit>(lineup.gameObject);
            fit.count = options.Count;
            fit.labelHeight = labelHeight;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)lineup.parent);
            fit.Fit();
        }

        for (int i = 0; i < options.Count; i++)
        {
            int index = i;
            SuspectView view = options[i];
            RectTransform cell = UIFactory.Container(lineup, "Sospechoso " + view.shortName, Vector2.zero, Vector2.one);
            var card = cell.gameObject.AddComponent<Image>();
            card.sprite = UISprites.Rounded(ThemeManager.Current.RadiusMedium);
            card.type = Image.Type.Sliced;
            card.color = T.panelBorder;
            var button = cell.gameObject.AddComponent<Button>();
            button.targetGraphic = card;
            button.onClick.AddListener(() =>
            {
                if (accusationDropdown != null)
                    accusationDropdown.value = index;
                MarkLineup(index);
            });
            cell.gameObject.AddComponent<ClickSound>();
            cell.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;

            (Texture2D texture, bool legacy) = PortraitOf(view, EmotionOf(view.id));
            PortraitCrops.Crops crops = PortraitCrops.For(view.portraitKey, view.artId, legacy, texture);
            RectTransform frame = UIFactory.Container(cell, "Marco", new Vector2(0f, 0f), new Vector2(1f, 1f));
            frame.offsetMin = new Vector2(8f, labelHeight);
            frame.offsetMax = new Vector2(-8f, -8f);
            RectTransform face = UIFactory.Container(frame, singleRow ? "Figura" : "Busto", Vector2.zero, Vector2.one);
            var raw = face.gameObject.AddComponent<RawImage>();
            raw.texture = texture;
            raw.raycastTarget = false;
            if (legacy)
                ArtGrading.Apply(raw, ArtGrading.Kind.LegacyPortrait);
            if (singleRow)
            {
                // De cuerpo entero, de los pies a la cabeza: HeightLineup le da su altura real contra la pared
                Rect figure = crops.figure;
                raw.uvRect = figure;
                frame.offsetMin = frame.offsetMax = Vector2.zero;
                var item = cell.gameObject.AddComponent<HeightLineupItem>();
                item.heightCm = view.heightCm > 0 ? view.heightCm : 170;
                item.aspect = PortraitCrops.Aspect(texture, figure);
                item.figure = face;
                card.color = new Color(T.panelBorder.r, T.panelBorder.g, T.panelBorder.b, 0.35f); // La pared se ve detrás
            }
            else
            {
                raw.uvRect = crops.bust;
                face.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                face.GetComponent<AspectRatioFitter>().aspectRatio = 0.75f;
            }

            // Quien el jugador ha descartado en su libreta: atenuado (se puede elegir igual: es su nota, no una regla)
            // Y quien descarta una pista ya encontrada, con las mismas palabras que la libreta
            bool cleared = IsCleared(view.id, out bool byClue);
            if (cleared)
                frame.gameObject.AddComponent<CanvasGroup>().alpha = 0.45f;

            string reason = byClue ? "pista de descarte" : "tu descarte";
            TMP_Text name = UIFactory.Label(cell, cleared ? $"<s>{view.shortName}</s>\n<size=80%>({reason})</size>" : view.shortName, T.bodySize,
                                            cleared ? T.textSecondary : T.textPrimary);
            name.alignment = TextAlignmentOptions.Center;
            name.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
            var nameRect = name.rectTransform;
            nameRect.anchorMin = Vector2.zero;
            nameRect.anchorMax = new Vector2(1f, 0f);
            nameRect.pivot = new Vector2(0.5f, 0f);
            nameRect.offsetMin = new Vector2(8f, 0f);
            nameRect.offsetMax = new Vector2(-8f, labelHeight);
            // Descartado: el motivo en una segunda línea (en una sola, con nombres largos, se cortaba en 16:9)
            if (cleared)
                LayoutKit.MultiLine(name, T.bodySize);
            else
                LayoutKit.OneLine(name, T.bodySize);

            RectTransform ring = UIFactory.Container(cell, "Seleccion", Vector2.zero, Vector2.one);
            var ringImage = ring.gameObject.AddComponent<Image>();
            ringImage.sprite = UISprites.RoundedOutline(16, 6);
            ringImage.type = Image.Type.Sliced;
            ringImage.color = T.accent;
            ringImage.raycastTarget = false;
            ring.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
            lineupSelection.Add(ring.gameObject);
        }

        if (heights != null)
        {
            // Con el tamaño definitivo de la rueda (en el editor no hay LateUpdate que lo corrija después)
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)lineup.parent);
            heights.Relayout();
        }
        MarkLineup(accusationDropdown != null ? accusationDropdown.value : 0);
    }

    private void MarkLineup(int selected)
    {
        for (int i = 0; i < lineupSelection.Count; i++)
            lineupSelection[i].SetActive(i == selected);
    }

    private static void DestroyImmediateOrLater(GameObject target)
    {
        if (Application.isPlaying)
        {
            target.SetActive(false);
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }

    public void ShowAccusationResult(AccusationResult result, string accusedName, string culpritName,
                                     int maxEvidence, string epilogue, string stats = null, CaseSummary summary = null)
    {
        fx?.SetTension(false);
        ShowPanel(resultPanel);
        EndingStyle style = EndingStyle.For(result.ending, T);
        SoundManager.PlayMusic(Music.None);
        SoundManager.Play(SoundCatalog.ForEnding(result.ending));

        ScreenReader.Announce(style.title);
        if (resultTitleText != null)
        {
            resultTitleText.text = style.title.ToUpperInvariant();
            resultTitleText.color = style.ink;
        }

        // Cada final tiñe la escena con su color
        if (resultPanel != null)
        {
            Transform existing = resultPanel.transform.Find("Tinte final (auto)");
            RectTransform grade = existing != null ? (RectTransform)existing
                : UIFactory.Container(resultPanel.transform, "Tinte final (auto)", Vector2.zero, Vector2.one);
            if (existing == null)
            {
                grade.SetSiblingIndex(0);
                UIComponents.GetOrAdd<LayoutElement>(grade.gameObject).ignoreLayout = true;
                grade.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
                grade.gameObject.AddComponent<UnityEngine.UI.Image>().raycastTarget = false;
            }
            grade.GetComponent<UnityEngine.UI.Image>().color = style.grade;
        }

        if (resultDetailsText == null)
            return;

        resultDetailsText.text = EndingReport.Build(result, accusedName, culpritName, maxEvidence, epilogue, T, stats, summary);

        if (fx != null && resultPanel != null && resultTitleText != null)
        {
            // El sello del final hace de título; el informe se descubre línea a línea
            if (endingStamp != null)
                Destroy(endingStamp);
            resultTitleText.text = "";
            RectTransform shaken = (RectTransform)resultPanel.transform;
            endingStamp = fx.Stamp(style.stamp, style.ink, angle: -5f, fontSize: 80f, parent: resultTitleText.rectTransform,
                anchor: new Vector2(0.5f, 0.5f), hold: -1f,
                onImpact: () => fx.Shake(shaken, result.ending == Ending.Bad ? 16f : 8f, 0.3f));
            UIComponents.GetOrAdd<StepReveal>(resultDetailsText.gameObject).Play(0.9f);
        }

        // El expediente se cierra con la ficha policial del culpable de verdad (aparece al terminar el informe)
        if (summary != null && summary.culprit.id != null)
        {
            (Texture2D portrait, bool legacy) = PortraitOf(summary.culprit, Emotion.Tranquilo);
            bool placeholder = portrait == null || portrait.name.Contains("Placeholder");
            Mugshot.Show(resultDetailsText, placeholder ? null : portrait,
                PortraitCrops.For(summary.culprit.portraitKey, summary.culprit.artId, legacy, portrait).bust, legacy,
                $"CULPABLE\n{culpritName}");
        }
    }
}
