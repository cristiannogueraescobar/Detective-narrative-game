using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Construcción y distribución de los paneles de la partida (ver InterrogationUI.cs).
/// </summary>
public partial class InterrogationUI
{
    /// <summary>
    /// Distribución móvil (vertical, una mano) de todos los paneles del juego con LayoutGroups reales.
    /// Idempotente. La valida LayoutValidationTests a 1080 × 1920.
    /// </summary>
    public void BuildLayout()
    {
        // Imágenes de sospechosos (también en la vista previa del editor)
        suspectImages["Padre"] = padreGif;
        suspectImages["Madre"] = madreGif;
        suspectImages["Hermano"] = hermanoGif;
        suspectImages["Vecina"] = vecinaGif;
        suspectImages["Detective"] = detectiveGif;
        suspectImages["Cartero"] = carteroGif;
        suspectImages["Dueño del Bar"] = duenioBarGif;
        // Retratos propios de quienes compartían el de otro (Sesión A), en archivos nuevos
        foreach (var pair in DerivedPortraits.All)
        {
            Texture2D derived = ArtLibrary.Load(pair.Value.path);
            if (derived != null)
                suspectImages[pair.Key] = derived;
        }

        EnsureEvidenceDropdown();
        if (!applyMobileLayout)
            return;

        Transform root = interrogationPanel != null ? interrogationPanel.transform.root : transform.root;
        ThemeApplier.Apply(root);

        BuildInterrogationLayout();
        BuildNotebookLayout();
        BuildClueNoticeLayout();
        BuildIntroLayout();
        BuildAccusationLayout();
        BuildResultLayout();

        // Las capas se dibujan por encima del contenido del panel
        if (cluesPanel != null)
            cluesPanel.transform.SetAsLastSibling();
    }

    private void BuildInterrogationLayout()
    {
        if (interrogationPanel == null)
            return;

        RectTransform column = LayoutKit.Column((RectTransform)interrogationPanel.transform, out bool created);
        if (!created)
            return;

        // Ambiente: la sala de interrogatorios, muy oscura, detrás de todo (sin deformar)
        Texture2D room = ArtLibrary.Load(ArtSlots.DefaultIntroBackground);
        if (room != null)
        {
            RectTransform roomRect = UIFactory.Container(interrogationPanel.transform, "Sala (auto)", Vector2.zero, Vector2.one);
            roomRect.SetAsFirstSibling();
            UIComponents.GetOrAdd<LayoutElement>(roomRect.gameObject).ignoreLayout = true;
            var roomImage = roomRect.gameObject.AddComponent<RawImage>();
            roomImage.texture = room;
            roomImage.raycastTarget = false;
            ArtGrading.Apply(roomImage, ArtGrading.Kind.Background);
            roomImage.color = ScaleRgb(storyTint, T.roomBrightness);
            roomBackdrop = roomImage;
            var roomFit = roomRect.gameObject.AddComponent<AspectRatioFitter>();
            roomFit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            roomFit.aspectRatio = (float)room.width / room.height;
            UIComponents.GetOrAdd<RectMask2D>(interrogationPanel);
            UIPerformance.IsolateInOwnCanvas(roomImage);
        }

        // Fila 1: día y preguntas + libreta
        RectTransform hud = LayoutKit.Row(column, "HUD", Theme.MinTouchSize);
        if (hudText != null)
        {
            LayoutKit.Put(hudText, hud, flexibleWidth: 1f);
            hudText.alignment = TextAlignmentOptions.MidlineLeft;
            hudText.font = UIFactory.TitleFont(); // Como un rótulo de expediente
            TextStyle.Set(hudText, TextStyle.Mode.OneLineOrTwo, T.bodySize); // Si no cabe ni al mínimo, en dos líneas
        }
        LayoutKit.Put(viewCluesButton, hud, width: 260f);
        LayoutKit.Label(viewCluesButton, "Libreta");

        // Cabecera: retrato en plano medio a la izquierda; a su derecha, a quién interrogas y las acciones del día
        // (cada una con su sitio). Así el chat se queda con la mayor parte de la pantalla.
        float headerHeight = 3f * Theme.MinTouchSize + 2f * T.spacing;
        RectTransform header = LayoutKit.Row(column, "Cabecera", headerHeight);
        header.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = false;

        if (suspectImage != null)
        {
            RectTransform portraitBox = UIFactory.Container(header, "Retrato (auto)", Vector2.zero, Vector2.one);
            LayoutKit.Size(portraitBox, width: headerHeight * 0.75f);
            var portrait = suspectImage.rectTransform;
            portrait.SetParent(portraitBox, false);
            portrait.anchorMin = Vector2.zero;
            portrait.anchorMax = Vector2.one;
            portrait.offsetMin = portrait.offsetMax = Vector2.zero;
            portrait.localScale = Vector3.one;
            // Sin AspectRatioFitter: estiraba el pixel art a lo que midiera la caja (bloques borrosos en pantallas altas)
            if (suspectImage.TryGetComponent(out AspectRatioFitter oldFitter))
                DestroyImmediate(oldFitter);
            PixelFit.For(portraitBox, suspectImage);

            // Estado emocional a la vista: una etiqueta sobre el pie del retrato
            RectTransform chip = UIFactory.Container(portraitBox, "Estado (auto)", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            chip.pivot = new Vector2(0.5f, 0f);
            chip.anchoredPosition = new Vector2(0f, 8f);
            chip.sizeDelta = new Vector2(headerHeight * 0.72f, 50f);
            UIComponents.GetOrAdd<LayoutElement>(chip.gameObject).ignoreLayout = true;
            var chipImage = chip.gameObject.AddComponent<Image>();
            chipImage.sprite = UISprites.Rounded(ThemeManager.Current.RadiusPill);
            chipImage.type = Image.Type.Sliced;
            chipImage.color = new Color(0f, 0f, 0f, 0.8f);
            chipImage.raycastTarget = false;
            chip.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
            emotionLabel = UIFactory.Label(chip, "", T.secondarySize, T.textPrimary);
            emotionLabel.name = "EstadoTexto";
            emotionLabel.alignment = TextAlignmentOptions.Center;
            emotionLabel.fontStyle = FontStyles.Bold;
            emotionLabel.characterSpacing = 4f;
            emotionLabel.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
            var labelRect = emotionLabel.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(8f, 0f);
            labelRect.offsetMax = new Vector2(-8f, 0f);
            LayoutKit.OneLine(emotionLabel, T.secondarySize);
            chip.gameObject.SetActive(false); // Se enseña cuando hay un sospechoso
        }

        RectTransform side = UIFactory.Container(header, "Controles (auto)", Vector2.zero, Vector2.one);
        LayoutKit.Size(side, flexibleWidth: 1f);
        var sideLayout = side.gameObject.AddComponent<VerticalLayoutGroup>();
        sideLayout.childAlignment = TextAnchor.MiddleCenter; // Con un retrato más alto (pantallas 20:9), centrados a su lado
        sideLayout.spacing = T.spacing;
        sideLayout.childControlWidth = sideLayout.childControlHeight = true;
        sideLayout.childForceExpandWidth = true;
        sideLayout.childForceExpandHeight = false;
        PutDropdown(suspectDropdown, side);
        LayoutKit.Put(endDayButton, side, height: Theme.MinTouchSize);
        LayoutKit.Label(endDayButton, "Fin del día");
        LayoutKit.Put(accuseNowButton, side, height: Theme.MinTouchSize);
        LayoutKit.Label(accuseNowButton, "Acusar");
        // Acusar es la acción más cara: aviso (texto de alerta), no reclamo; el dorado queda para Enviar
        if (accuseNowButton != null)
        {
            UIComponents.GetOrAdd<ThemeRole>(accuseNowButton.gameObject).role = UIRole.DangerButton;
            ThemeApplier.Apply(accuseNowButton.transform);
        }

        // Pantallas alargadas: el retrato se lleva una parte del alto extra; el resto, el chat
        Transform portraitBoxT = header.Find("Retrato (auto)");
        UIComponents.GetOrAdd<TallScreenHeader>(column.gameObject).Configure(
            header.GetComponent<LayoutElement>(),
            portraitBoxT != null ? portraitBoxT.GetComponent<LayoutElement>() : null,
            headerHeight);

        // Chat: todo el hueco que queda, solo desplazamiento vertical
        if (conversationScroll != null)
        {
            LayoutKit.Put(conversationScroll, column, height: 300f, flexibleHeight: 1f);
            ConfigureChatScroll();
            BuildSuggestions(column);
            // El hueco del centro: el sospechoso de cuerpo entero detrás del chat, viñeta de tensión, entradas
            Transform portraitBoxForScene = suspectImage != null ? suspectImage.rectTransform.parent : null;
            scene = InterrogationScene.Build((RectTransform)interrogationPanel.transform, (RectTransform)conversationScroll.transform,
                                             portraitBoxForScene as RectTransform);
        }

        // "Esperando respuesta" ahora es la burbuja de "escribiendo…" del chat
        if (waitingText != null)
            waitingText.gameObject.SetActive(false);

        // Abajo, al alcance del pulgar: la prueba que se muestra y la pregunta
        PutDropdown(evidenceDropdown, column);

        RectTransform ask = LayoutKit.Row(column, "Pregunta", 120f);
        if (questionInput != null)
        {
            LayoutKit.Put(questionInput, ask, flexibleWidth: 1f);
            ConfigureQuestionInput();
        }
        LayoutKit.Put(askButton, ask, width: 220f);
        LayoutKit.Label(askButton, "Enviar");
    }

    private void ConfigureChatScroll()
    {
        conversationScroll.horizontal = false;
        conversationScroll.vertical = true;
        if (conversationScroll.horizontalScrollbar != null)
        {
            conversationScroll.horizontalScrollbar.gameObject.SetActive(false);
            conversationScroll.horizontalScrollbar = null;
        }
        conversationScroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

        // El chat deja ver la sala detrás (las burbujas llevan su propio fondo)
        if (conversationScroll.TryGetComponent(out Image chatBackground))
        {
            chatBackground.color = new Color(0f, 0f, 0f, 0.2f);
            UIComponents.GetOrAdd<ThemeRole>(chatBackground.gameObject).role = UIRole.Ignore;
        }
        LayoutKit.StyleScrollbar(conversationScroll.verticalScrollbar);

        RectTransform viewport = conversationScroll.viewport;
        if (viewport != null)
        {
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = viewport.offsetMax = Vector2.zero;
        }

        RectTransform content = conversationScroll.content;
        if (content != null)
        {
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = content.offsetMax = Vector2.zero;
            var layout = UIComponents.GetOrAdd<VerticalLayoutGroup>(content.gameObject);
            layout.padding = new RectOffset(16, 16, 8, 8);
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.LowerCenter; // Conversación corta: abajo, junto al campo
            UIComponents.GetOrAdd<ContentSizeFitter>(content.gameObject).verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            if (viewport != null)
            {
                var fill = UIComponents.GetOrAdd<FillViewport>(viewport.gameObject);
                fill.content = UIComponents.GetOrAdd<LayoutElement>(content.gameObject);
                fill.Apply();
            }
        }

        // Las burbujas sustituyen al texto único de la escena
        if (conversationText != null)
            conversationText.gameObject.SetActive(false);
        chat = UIComponents.GetOrAdd<ChatView>(conversationScroll.gameObject);
        chat.Initialize();
    }

    /// <summary>
    /// Preguntas de ejemplo bajo un chat sin empezar, alineadas a la derecha como las del detective. Van en la
    /// columna (no encima del chat): mientras se ven, el chat se encoge y nunca tapan el parte del día.
    /// </summary>
    private void BuildSuggestions(RectTransform column)
    {
        if (column.Find("Sugerencias (auto)") is RectTransform existing)
        {
            suggestionsBox = existing;
            return;
        }

        suggestionsBox = UIFactory.Container(column, "Sugerencias (auto)", Vector2.zero, Vector2.one);
        suggestionsBox.SetSiblingIndex(conversationScroll.transform.GetSiblingIndex() + 1);
        var layout = UIFactory.VerticalLayout(suggestionsBox, T.spacing * 0.5f, 0f);
        layout.padding = new RectOffset(Mathf.RoundToInt(T.spacing * 6f), 0, 0, 0);
        layout.childAlignment = TextAnchor.LowerRight;

        TMP_Text hint = UIFactory.Label(suggestionsBox, GameTexts.SuggestionsHint, T.secondarySize, T.textSecondary);
        hint.fontStyle = FontStyles.Italic;
        hint.gameObject.AddComponent<ThemeRole>().role = UIRole.Secondary;
        hint.alignment = TextAlignmentOptions.BottomRight;
        LayoutKit.OneLine(hint, T.secondarySize);
        UIComponents.GetOrAdd<LayoutElement>(hint.gameObject).minHeight = T.secondarySize * 1.5f;

        suggestionLabels.Clear();
        string[] texts = QuestionSuggestions.For(victimName);
        for (int i = 0; i < texts.Length; i++)
        {
            int index = i;
            Button chip = UIFactory.Button(suggestionsBox, texts[i], false, () => OnSuggestion(index));
            chip.name = $"Sugerencia {i + 1}";
            suggestionLabels.Add(chip.GetComponentInChildren<TMP_Text>(true));
        }
        suggestionsBox.gameObject.SetActive(false);
    }

    private const string QuestionPlaceholder = "Escribe tu pregunta…";
    private const string EvidencePlaceholder = "Pregunta sobre la prueba (opcional)…";

    // Con una prueba elegida, la pregunta es opcional: se puede enseñar sin decir nada
    private void OnEvidenceChanged(int index)
    {
        if (questionInput != null && questionInput.placeholder is TMP_Text placeholder)
            placeholder.text = index > 0 ? EvidencePlaceholder : QuestionPlaceholder;
    }

    private void ConfigureQuestionInput()
    {
        if (evidenceDropdown != null)
        {
            evidenceDropdown.onValueChanged.RemoveListener(OnEvidenceChanged);
            evidenceDropdown.onValueChanged.AddListener(OnEvidenceChanged);
        }
        // La escena traía la pregunta de ejemplo como texto escrito, no como placeholder
        questionInput.SetTextWithoutNotify(string.Empty);

        RectTransform area = questionInput.textViewport;
        if (area != null)
        {
            area.anchorMin = Vector2.zero;
            area.anchorMax = Vector2.one;
            area.offsetMin = new Vector2(20f, 8f);
            area.offsetMax = new Vector2(-20f, -8f);
        }

        if (questionInput.placeholder is TMP_Text placeholder)
        {
            placeholder.text = QuestionPlaceholder;
            placeholder.rectTransform.anchorMin = Vector2.zero;
            placeholder.rectTransform.anchorMax = Vector2.one;
            placeholder.rectTransform.offsetMin = placeholder.rectTransform.offsetMax = Vector2.zero;
            LayoutKit.OneLine(placeholder, T.bodySize);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            placeholder.color = T.textSecondary;
            placeholder.fontStyle = FontStyles.Italic;
        }

        if (questionInput.textComponent != null)
        {
            questionInput.textComponent.rectTransform.anchorMin = Vector2.zero;
            questionInput.textComponent.rectTransform.anchorMax = Vector2.one;
            questionInput.textComponent.rectTransform.offsetMin = questionInput.textComponent.rectTransform.offsetMax = Vector2.zero;
            questionInput.textComponent.alignment = TextAlignmentOptions.MidlineLeft;
        }

        // Con algo escrito las preguntas de ejemplo se apartan: tocar una no pisa lo que ya hay
        questionInput.onValueChanged.RemoveListener(OnQuestionTextChanged);
        questionInput.onValueChanged.AddListener(OnQuestionTextChanged);
    }

    private static void PutDropdown(TMP_Dropdown dropdown, RectTransform column)
    {
        if (dropdown == null)
            return;

        LayoutKit.Put(dropdown, column, height: Theme.MinTouchSize);

        if (dropdown.captionText != null)
        {
            RectTransform label = dropdown.captionText.rectTransform;
            label.anchorMin = Vector2.zero;
            label.anchorMax = Vector2.one;
            label.offsetMin = new Vector2(24f, 6f);
            label.offsetMax = new Vector2(-72f, -6f); // Sitio para la flecha
            LayoutKit.OneLine(dropdown.captionText, T.bodySize);
            dropdown.captionText.alignment = TextAlignmentOptions.MidlineLeft;
        }

        if (dropdown.itemText != null)
            LayoutKit.OneLine(dropdown.itemText, T.bodySize);

        if (dropdown.template != null)
        {
            UIComponents.GetOrAdd<DropdownFit>(dropdown.gameObject).Fit(); // Lista del ancho del desplegable

            // Cada opción de la lista abierta, de 48 dp como el resto de controles
            Toggle item = dropdown.template.GetComponentInChildren<Toggle>(true);
            if (item != null)
            {
                var itemRect = (RectTransform)item.transform;
                itemRect.sizeDelta = new Vector2(itemRect.sizeDelta.x, Theme.MinTouchSize);
                // El contenido de la plantilla mide una opción: uGUI calcula el hueco extra con él y, si es más bajo,
                // la lista se abría desplazada con la primera opción cortada
                if (dropdown.template.TryGetComponent(out ScrollRect list) && list.content != null)
                    list.content.sizeDelta = new Vector2(list.content.sizeDelta.x, itemRect.rect.height);
                if (dropdown.itemText != null)
                {
                    RectTransform text = dropdown.itemText.rectTransform;
                    text.anchorMin = Vector2.zero;
                    text.anchorMax = Vector2.one;
                    text.offsetMin = new Vector2(72f, 6f);
                    text.offsetMax = new Vector2(-24f, -6f);
                    dropdown.itemText.alignment = TextAlignmentOptions.MidlineLeft;
                }
            }
        }
    }

    private void BuildNotebookLayout()
    {
        if (cluesPanel == null)
            return;

        var panel = (RectTransform)cluesPanel.transform;
        LayoutKit.Overlay(panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        RectTransform column = LayoutKit.Column(panel, out bool created);
        if (!created)
            return;

        Transform title = panel.Find("CluesTitleText");
        if (title != null)
        {
            LayoutKit.Put(title, column, height: 110f);
            if (title.TryGetComponent(out TMP_Text titleText))
            {
                titleText.text = GameTexts.NotebookTitle;
                titleText.fontStyle = FontStyles.Bold;
                titleText.characterSpacing = 8f;
            }
        }

        if (contradictionsText != null)
            contradictionsText.gameObject.SetActive(false); // Las contradicciones van en la libreta

        ScrollRect notebookScroll = LayoutKit.Scrollable(cluesText, column);

        // Abajo: "Pensar" (ayuda por niveles, según la dificultad) y "Cerrar"
        RectTransform actions = LayoutKit.Row(column, "Acciones de la libreta", Theme.MinTouchSize);
        thinkButton = UIFactory.Button(actions, GameTexts.ThinkLabel(1), false, OnThinkClick);
        thinkButton.name = "PensarButton";
        LayoutKit.Put(thinkButton, actions, flexibleWidth: 1.4f);
        LayoutKit.Put(closeCluesButton, actions, flexibleWidth: 1f);
        LayoutKit.Label(closeCluesButton, "Cerrar");

        // Aspecto de libreta: papel crema, tinta oscura y el margen rojo a la izquierda
        if (panel.TryGetComponent(out Image paper))
        {
            paper.sprite = null; // El sprite gris de la escena apagaba el crema
            paper.color = T.paper;
            UIComponents.GetOrAdd<ThemeRole>(paper.gameObject).role = UIRole.Ignore;
        }
        if (title != null && title.TryGetComponent(out TMP_Text paperTitle))
        {
            paperTitle.color = T.paperText;
            UIComponents.GetOrAdd<ThemeRole>(paperTitle.gameObject).role = UIRole.Ignore;
        }
        if (cluesText != null)
        {
            cluesText.color = T.paperText;
            UIComponents.GetOrAdd<ThemeRole>(cluesText.gameObject).role = UIRole.Ignore;
            // Tocar una pista en la libreta la prepara como prueba
            cluesText.raycastTarget = true;
            TextLinkHandler links = UIComponents.GetOrAdd<TextLinkHandler>(cluesText.gameObject);
            links.onLink = OnNotebookLink;
            links.describe = DescribeNotebookLink;
        }
        if (notebookScroll != null && notebookScroll.TryGetComponent(out Image scrollImage))
        {
            scrollImage.color = Color.clear;
            UIComponents.GetOrAdd<ThemeRole>(scrollImage.gameObject).role = UIRole.Ignore;
        }
        RectTransform margin = UIFactory.Container(panel, "Margen (auto)", new Vector2(0f, 0f), new Vector2(0f, 1f));
        margin.pivot = new Vector2(0f, 0.5f);
        margin.sizeDelta = new Vector2(3f, 0f);
        margin.anchoredPosition = new Vector2(T.padding * 0.55f, 0f);
        margin.SetSiblingIndex(0);
        UIComponents.GetOrAdd<LayoutElement>(margin.gameObject).ignoreLayout = true;
        var marginImage = margin.gameObject.AddComponent<Image>();
        marginImage.color = new Color(T.paperInk.r, T.paperInk.g, T.paperInk.b, 0.45f);
        marginImage.raycastTarget = false;
        margin.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
    }

    private void BuildClueNoticeLayout()
    {
        if (clueNotification == null)
            return;

        var notice = (RectTransform)clueNotification.transform;
        LayoutKit.Overlay(notice, new Vector2(0.05f, 1f), new Vector2(0.95f, 1f), new Vector2(0f, -470f), new Vector2(0f, -210f));

        if (clueNotificationText != null)
        {
            RectTransform text = clueNotificationText.rectTransform;
            text.anchorMin = Vector2.zero;
            text.anchorMax = Vector2.one;
            text.offsetMin = new Vector2(96f, 12f); // Sitio para el icono
            text.offsetMax = new Vector2(-24f, -12f);
            text.localScale = Vector3.one;
            LayoutKit.MultiLine(clueNotificationText, T.bodySize);
        }
    }

    private void BuildIntroLayout()
    {
        if (introPanel == null)
            return;

        RectTransform column = LayoutKit.Column((RectTransform)introPanel.transform, out bool created);
        if (!created)
            return;

        if (caseTitleText != null)
            LayoutKit.Put(caseTitleText, column, height: 150f);
        LayoutKit.Scrollable(caseDescriptionText, column);
        LayoutKit.Put(startButton, column, height: 130f);
        LayoutKit.Label(startButton, "Empezar");
    }

    private void BuildAccusationLayout()
    {
        if (accusationPanel == null)
            return;

        var panel = (RectTransform)accusationPanel.transform;
        RectTransform column = LayoutKit.Column(panel, out bool created);
        if (!created)
            return;

        Transform title = panel.Find("AccusationTitleText");
        if (title != null)
            LayoutKit.Put(title, column, height: 140f);

        Transform instructions = panel.Find("Text (TMP)");
        if (instructions != null)
        {
            LayoutKit.Put(instructions, column, height: 240f); // Pregunta + resumen de la libreta
            if (instructions.TryGetComponent(out TMP_Text instructionsText))
            {
                instructionsText.text = GameTexts.AccusationPrompt;
                LayoutKit.MultiLine(instructionsText, T.bodySize);
                accusationPromptText = instructionsText;
            }
        }

        // Rueda de reconocimiento: los sospechosos de este caso (la foto de grupo era de la historia 1)
        Transform group = panel.Find("SuspectsGroupImage");
        if (group != null)
            group.gameObject.SetActive(false);
        // La rueda va sobre una pared de alturas de comisaría (la rueda se vacía y se rellena sin tocar la pared)
        RectTransform wall = UIFactory.Container(column, LineupWallName, Vector2.zero, Vector2.one);
        LayoutKit.Size(wall, height: 0f, flexibleHeight: 1f);
        UIComponents.GetOrAdd<LayoutElement>(wall.gameObject).minHeight = 0f;
        BuildLineupWall(wall);
        // Encima de la pared: lo que llevas a la acusación (tarjeta "TUS PRUEBAS")
        Transform dropdownArrow = accusationDropdown != null ? accusationDropdown.transform.Find("Arrow") : null;
        evidenceCard = EvidenceCard.Build(column, wall, dropdownArrow != null && dropdownArrow.TryGetComponent(out Image arrowImage) ? arrowImage.sprite : null);
        lineup = UIFactory.Container(wall, "Rueda (auto)", Vector2.zero, Vector2.one);
        lineup.offsetMin = new Vector2(LineupWallMargin, 0f); // Sitio para las cifras de la izquierda
        var grid = lineup.gameObject.AddComponent<GridLayoutGroup>();
        grid.spacing = new Vector2(T.spacing, T.spacing);
        grid.childAlignment = TextAnchor.MiddleCenter;
        UIComponents.GetOrAdd<GridFit>(lineup.gameObject);

        PutDropdown(accusationDropdown, column);
        // "Prueba clave": un clon del selector de sospechoso (mismo aspecto y tamaño táctil)
        if (accusationDropdown != null && keyClueDropdown == null)
        {
            GameObject clone = Instantiate(accusationDropdown.gameObject, column);
            clone.name = "PruebaClaveDropdown (auto)";
            keyClueDropdown = clone.GetComponent<TMP_Dropdown>();
            keyClueDropdown.onValueChanged.RemoveAllListeners();
            keyClueDropdown.ClearOptions();
            keyClueDropdown.AddOptions(new List<string> { GameTexts.KeyClueNone });
            PutDropdown(keyClueDropdown, column);
        }
        LayoutKit.Put(accuseButton, column, height: 130f);
        LayoutKit.Label(accuseButton, "Acusar");

        EnsureAccusationBackButton();
    }

    public const string LineupWallName = "Pared de la rueda (auto)";
    private const float LineupWallMargin = 72f;

    // Líneas horizontales cada 10 cm, de 190 (arriba) a 130 (abajo), con la cifra a la izquierda
    private void BuildLineupWall(RectTransform wall)
    {
        RectTransform marks = UIFactory.Container(wall, "Alturas", Vector2.zero, Vector2.one);
        marks.gameObject.SetActive(T.lineupWall);
        marks.gameObject.AddComponent<Decorative>(); // El lector de pantalla no lee las cifras
        UIComponents.GetOrAdd<LayoutElement>(marks.gameObject).ignoreLayout = true;
        // Con alturas reales la rueda recoloca las rayas a su escala (HeightLineup); hasta 2 m
        const int top = HeightLineup.WallTopCm, bottom = 110, step = 10;
        int count = (top - bottom) / step;
        for (int k = 0; k <= count; k++)
        {
            float y = 1f - (float)k / count;
            RectTransform line = UIFactory.Container(marks, "Linea " + (top - k * step), new Vector2(0f, y), new Vector2(1f, y));
            line.sizeDelta = new Vector2(0f, k % 2 == 0 ? 3f : 2f);
            var image = line.gameObject.AddComponent<Image>();
            image.color = new Color(T.textSecondary.r, T.textSecondary.g, T.textSecondary.b, k % 2 == 0 ? 0.35f : 0.18f);
            image.raycastTarget = false;
            line.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;

            RectTransform label = UIFactory.Container(marks, "Cifra " + (top - k * step), new Vector2(0f, y), new Vector2(0f, y));
            label.pivot = new Vector2(0f, 0f);
            label.sizeDelta = new Vector2(LineupWallMargin, 40f);
            TMP_Text text = UIFactory.Label(label, (top - k * step).ToString(), T.secondarySize * 0.8f, T.textSecondary);
            text.font = UIFactory.TitleFont();
            text.alignment = TextAlignmentOptions.BottomLeft;
            text.raycastTarget = false;
            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = textRect.offsetMax = Vector2.zero;
            UIComponents.GetOrAdd<LayoutElement>(text.gameObject).ignoreLayout = true;
            text.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
        }
    }

    private void BuildResultLayout()
    {
        if (resultPanel == null)
            return;

        RectTransform column = LayoutKit.Column((RectTransform)resultPanel.transform, out bool created);
        if (!created)
            return;

        if (resultTitleText != null)
        {
            LayoutKit.Put(resultTitleText, column, height: 180f);
            LayoutKit.MultiLine(resultTitleText, T.headingSize);
        }
        LayoutKit.Scrollable(resultDetailsText, column);

        RectTransform buttons = LayoutKit.Row(column, "Botones", 130f);
        LayoutKit.Put(restartButton, buttons, flexibleWidth: 1f);
        LayoutKit.Label(restartButton, GameTexts.PlayAgain);
        LayoutKit.Put(menuButton, buttons, flexibleWidth: 1f);
        LayoutKit.Label(menuButton, GameTexts.MainMenu);
    }

    /// <summary>
    /// Icono pequeño a la izquierda de un botón o aviso (Assets/Art/Icons/...), o un cuadro de color si falta.
    /// </summary>
    private void AddIcon(Transform parent, string path)
    {
        if (parent == null || parent.Find("Icono (auto)") != null)
            return;

        RectTransform rect = UIFactory.Container(parent, "Icono (auto)", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
        rect.pivot = new Vector2(0f, 0.5f);
        rect.sizeDelta = new Vector2(56f, 56f);
        rect.anchoredPosition = new Vector2(T.spacing, 0f);
        var icon = rect.gameObject.AddComponent<UnityEngine.UI.RawImage>();
        icon.raycastTarget = false;
        icon.texture = ArtSlots.LoadOrPlaceholder(path, new Color(T.accent.r, T.accent.g, T.accent.b, 0.35f));

        if (parent.GetComponent<Button>() != null)
        {
            TMP_Text label = parent.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                label.rectTransform.offsetMin = new Vector2(56f + 2f * T.spacing, label.rectTransform.offsetMin.y);
        }
    }
}
