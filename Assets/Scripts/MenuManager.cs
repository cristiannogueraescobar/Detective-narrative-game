using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// MENU MANAGER CORREGIDO - Controla menú principal y navegación
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject aboutPanel;
    
    [Header("Panel de Juego")]
    [SerializeField] private GameObject introPanel; // NUEVO: Referencia directa
    
    [Header("Botones Menú Principal")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button instructionsButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button aboutButton;
    [SerializeField] private Button quitButton;
    
    [Header("Botones de Retorno")]
    [SerializeField] private Button backFromInstructionsButton;
    [SerializeField] private Button backFromSettingsButton;
    [SerializeField] private Button backFromAboutButton;
    
    [Header("Móvil")]
    [Tooltip("Recoloca los paneles del menú en vertical (título arriba, botones abajo). Medidas en el tema")]
    [SerializeField] private bool applyMobileLayout = true;

    [Header("Referencia GameManager")]
    [SerializeField] private GameManager gameManager; // NUEVO
    
    [Header("Imágenes de Fondo (Opcional)")]
    [SerializeField] private RawImage mainMenuBackground;
    [SerializeField] private Texture2D mainMenuBackgroundTexture;
    
    private void Start()
    {
        // Configurar botones del menú principal
        if (playButton != null)
            UIComponents.SetOnlyListener(playButton, OnPlayClicked);

        SetUpContinueButton();

        BuildLayout();
        BackButtonRouter.Ensure(gameObject);
        
        if (instructionsButton != null)
            UIComponents.SetOnlyListener(instructionsButton, ShowInstructions);
        
        if (settingsButton != null)
            UIComponents.SetOnlyListener(settingsButton, ShowSettings);
        
        if (aboutButton != null)
            UIComponents.SetOnlyListener(aboutButton, ShowAbout);
        
        if (quitButton != null)
            UIComponents.SetOnlyListener(quitButton, QuitGame);
        
        // Configurar botones de retorno
        if (backFromInstructionsButton != null)
            UIComponents.SetOnlyListener(backFromInstructionsButton, ShowMainMenu);
        
        if (backFromSettingsButton != null)
            UIComponents.SetOnlyListener(backFromSettingsButton, ShowMainMenu);
        
        if (backFromAboutButton != null)
            UIComponents.SetOnlyListener(backFromAboutButton, ShowMainMenu);
        
        // Fondo: arte nuevo (Assets/Art/Backgrounds/menu.png), si no el de la escena, si no color plano
        if (mainMenuBackground != null)
        {
            Texture2D newArt = ArtLibrary.Load(ArtSlots.MenuBackground);
            mainMenuBackground.texture = newArt
                                         ?? (mainMenuBackgroundTexture != null ? mainMenuBackgroundTexture : null)
                                         ?? ArtLibrary.Placeholder(ThemeManager.Current.background);

            ParallaxLayer.AddTo(mainMenuBackground);

            // Solo el fondo antiguo se gradúa
            if (newArt == null && mainMenuBackgroundTexture != null)
                ArtGrading.Apply(mainMenuBackground, ArtGrading.Kind.Background);
        }
        
        // Mostrar menú principal
        ShowMainMenu();

        // "Reiniciar" recarga la escena y salta el menú
        if (GameManager.StartNewGameOnLoad)
        {
            GameManager.StartNewGameOnLoad = false;
            StartCoroutine(PlayNextFrame());
        }
        
        Debug.Log("[MenuManager] Inicializado");
    }
    
    /// <summary>
    /// Si hay una partida guardada válida, añade "Continuar" (clonando el botón de jugar, encima de él)
    /// y el de jugar pasa a llamarse "Caso nuevo".
    /// </summary>
    private void SetUpContinueButton()
    {
        if (playButton == null || !SaveSystem.TryLoad(out SaveData saved))
            return;

        GameObject clone = Instantiate(playButton.gameObject, playButton.transform.parent);
        clone.name = "ContinueButton (auto)";
        clone.transform.SetSiblingIndex(playButton.transform.GetSiblingIndex()); // Encima de "Caso nuevo"
        var rect = (RectTransform)clone.transform;
        var source = (RectTransform)playButton.transform;
        rect.anchoredPosition = source.anchoredPosition + new Vector2(0f, source.rect.height + ThemeManager.Current.spacing);

        var continueButton = clone.GetComponent<Button>();
        UIComponents.SetOnlyListener(continueButton, OnContinueClicked);
        CaseLibrary.TryFind(saved.variantId, out StoryData savedStory, out _);
        SetLabel(clone, GameTexts.Continue(savedStory != null ? savedStory.title : null, saved.day));
        SetLabel(playButton.gameObject, GameTexts.NewCaseButton);
    }

    private static void SetLabel(GameObject button, string text)
    {
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
            label.text = text;
    }

    /// <summary>
    /// Distribución móvil de los paneles del menú. Idempotente; la usa también el test de layout.
    /// </summary>
    public void BuildLayout()
    {
        if (!applyMobileLayout)
            return;

        if (mainMenuPanel != null)
            ThemeApplier.Apply(mainMenuPanel.transform.root);

        if (settingsPanel != null)
            SettingsPanel.Build(settingsPanel, () => gameManager?.RestartGame());

        BuildMainMenu();
        BuildCaseSelect();
        Theme theme = ThemeManager.Current;
        SetText(instructionsPanel, "InstructionsTitleText", GameTexts.InstructionsTitle);
        SetText(instructionsPanel, "InstructionsText", GameTexts.Instructions(theme));
        SetText(aboutPanel, "AboutTitleText", GameTexts.AboutTitle);
        SetText(aboutPanel, "InstructionsText", GameTexts.About(theme, Application.version));
        SetText(settingsPanel, "SettingsTitleText", GameTexts.SettingsTitle);

        BuildPage(instructionsPanel, "InstructionsTitleText", "InstructionsText", backFromInstructionsButton);
        BuildPage(aboutPanel, "AboutTitleText", "InstructionsText", backFromAboutButton);
        BuildPage(settingsPanel, "SettingsTitleText", "AjustesControles", backFromSettingsButton);
    }

    private void BuildMainMenu()
    {
        if (mainMenuPanel == null)
            return;

        var panel = (RectTransform)mainMenuPanel.transform;
        if (panel.Find(LayoutKit.ColumnName) != null)
            return;

        // Ilustración sin deformar, con su ambiente, y sombras para leer el título y los botones
        RawImage backdrop = ArtBackdrop.Cover(panel);
        if (backdrop != null)
        {
            MenuAmbience.AddTo(backdrop);
            ParallaxLayer.AddTo(backdrop);
            UIPerformance.IsolateInOwnCanvas(backdrop);
        }
        ArtBackdrop.Shade(panel, "Sombra arriba (auto)", top: true, heightFraction: 0.5f, alpha: 0.96f);
        ArtBackdrop.Shade(panel, "Sombra abajo (auto)", top: false, heightFraction: 0.58f, alpha: 0.96f);

        RectTransform column = LayoutKit.Column(panel, out _);
        Theme t = ThemeManager.Current;
        var layout = column.GetComponent<VerticalLayoutGroup>();
        layout.padding.left = layout.padding.right = 96; // Botones más estrechos: el dibujo respira por los lados
        layout.spacing = t.spacing * 1.25f;

        Transform title = panel.Find("GameTitleText");
        if (title != null)
        {
            LayoutKit.Put(title, column, height: 340f); // La fuente tiene un interlineado enorme
            if (title.TryGetComponent(out TMP_Text titleText))
            {
                // La fuente del título dibuja los glifos pequeños para su tamaño (en la escena estaba a 242):
                // se deja crecer mucho y el autoajuste lo encaja en el ancho
                LayoutKit.MultiLine(titleText, t.titleSize * 3.4f);
                titleText.alignment = TextAlignmentOptions.Bottom; // Pegado al subtítulo
                titleText.text = GameTexts.GameName.ToUpperInvariant(); // El nombre sale de un solo sitio
                titleText.fontStyle = FontStyles.Normal; // Sin el subrayado de la escena
                titleText.color = t.accent;
                UIComponents.GetOrAdd<TitleIntro>(titleText.gameObject);
            }
        }

        // El subtítulo va sobre una banda oscura suave: el dibujo es muy cargado justo ahí
        RectTransform band = UIFactory.Container(column, "Subtitulo (banda)", Vector2.zero, Vector2.one);
        var bandImage = band.gameObject.AddComponent<Image>();
        bandImage.sprite = UISprites.Rounded(ThemeManager.Current.RadiusSmall);
        bandImage.type = Image.Type.Sliced;
        bandImage.color = new Color(t.background.r, t.background.g, t.background.b, t.titleBandAlpha);
        bandImage.raycastTarget = false;
        band.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
        LayoutKit.Put(band, column, height: 64f);

        TMP_Text subtitle = UIFactory.Label(band, "Interrogatorios · Tres casos", t.secondarySize, t.textPrimary);
        subtitle.name = "Subtitulo (auto)";
        subtitle.alignment = TextAlignmentOptions.Center;
        subtitle.characterSpacing = 6f;
        LayoutKit.Overlay(subtitle.rectTransform, Vector2.zero, Vector2.one, new Vector2(24f, 0f), new Vector2(-24f, 0f));
        LayoutKit.OneLine(subtitle, t.secondarySize);

        LayoutKit.Spacer(column, 1f);

        Transform continueButton = panel.Find("ContinueButton (auto)");
        if (continueButton != null)
        {
            PutMenuButton(continueButton.GetComponent<Button>(), column, null);
        }

        PutMenuButton(playButton, column, continueButton != null ? GameTexts.NewCaseButton : "Jugar");
        PutMenuButton(instructionsButton, column, "Instrucciones");
        PutMenuButton(settingsButton, column, "Ajustes");
        PutMenuButton(aboutButton, column, "Acerca de");
        PutMenuButton(quitButton, column, "Salir");

        LayoutKit.Spacer(column, 0.3f);
    }

    // Textos de la interfaz desde GameTexts (la escena traía erratas y datos viejos)
    private static void SetText(GameObject panel, string child, string text)
    {
        Transform t = panel != null ? panel.transform.Find(child) : null;
        if (t != null && t.TryGetComponent(out TMP_Text tmp))
        {
            tmp.text = text;
            tmp.richText = true;
        }
    }

    private static void PutMenuButton(Button button, RectTransform column, string label)
    {
        if (button == null)
            return;

        LayoutKit.Put(button, column, height: Theme.MinTouchSize);
        LayoutKit.Label(button, label);

        // La escena traía estilos sueltos (cursiva en "Acerca de")
        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            text.fontStyle = FontStyles.Normal;
            // La fuente de la escena solo trae ASCII: "Continuar · caso, día N" necesita la dinámica
            text.font = UIFactory.TitleFont();
            if (text.TryGetComponent(out TextStyle style))
                style.font = text.font;
        }
    }

    /// <summary>
    /// Página de menú: título arriba, contenido en el centro (con scroll si es texto) y "Volver" abajo.
    /// </summary>
    private static void BuildPage(GameObject pageObject, string titleName, string contentName, Button back)
    {
        if (pageObject == null)
            return;

        var panel = (RectTransform)pageObject.transform;
        RectTransform column = LayoutKit.Column(panel, out bool created);
        if (!created)
            return;

        Transform title = panel.Find(titleName);
        if (title != null)
        {
            LayoutKit.Put(title, column, height: 150f);
            if (title.TryGetComponent(out TMP_Text titleText))
                LayoutKit.MultiLine(titleText, ThemeManager.Current.titleSize);
        }

        Transform content = panel.Find(contentName);
        if (content != null && content.gameObject.activeSelf)
        {
            if (content.TryGetComponent(out TMP_Text text))
                LayoutKit.Scrollable(text, column);
            else
                LayoutKit.Put(content, column, height: 300f, flexibleHeight: 1f);
        }
        else
        {
            LayoutKit.Spacer(column, 1f);
        }

        if (back != null)
        {
            LayoutKit.Put(back, column, height: 130f);
            LayoutKit.Label(back, "Volver");
        }
    }

    private void OnContinueClicked()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        bool continued = false;
        try
        {
            continued = gameManager != null && gameManager.ContinueSavedGame();
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }

        if (!continued)
        {
            // El guardado ya no es válido: se empieza una partida nueva
            SaveSystem.Delete();
            OnPlayClicked();
        }
    }

    private RectTransform caseSelect;

    // "Jugar": primero se elige el caso
    private void OnPlayClicked()
    {
        ShowCaseSelect();
    }

    /// <summary>
    /// Selección de caso: un expediente por historia con su mejor final, y "al azar".
    /// </summary>
    private void ShowCaseSelect()
    {
        if (mainMenuPanel == null)
        {
            StartChosenCase(null);
            return;
        }
        ShowPanel(mainMenuPanel);
        BuildCaseSelect();
        CaseSelect.Refresh(caseSelect);
        caseSelect.gameObject.SetActive(true);
        caseSelect.SetAsLastSibling();
        UIAnimations.FadeIn(this, caseSelect.gameObject);
    }

    /// <summary>
    /// Botón Atrás (Android) en el menú: cierra la selección de caso o vuelve de una página. Nunca sale del juego.
    /// </summary>
    public bool HandleBack()
    {
        if (caseSelect != null && caseSelect.gameObject.activeInHierarchy)
        {
            if (!ConfirmDialog.Hide(caseSelect, NewGameDialog))
                caseSelect.gameObject.SetActive(false);
            return true;
        }
        foreach (GameObject page in new[] { instructionsPanel, settingsPanel, aboutPanel })
        {
            if (page != null && page.activeInHierarchy)
            {
                Transform confirm = page.transform.Find("ConfirmarReinicio");
                if (confirm != null && confirm.gameObject.activeSelf)
                    confirm.gameObject.SetActive(false);
                else
                    ShowMainMenu();
                return true;
            }
        }
        return false;
    }

    private Theme caseSelectTheme;

    private void BuildCaseSelect()
    {
        // Se rehace si cambió el tema (alto contraste): sus colores se ponen al construirla
        if (caseSelect != null && caseSelectTheme != ThemeManager.Current)
        {
            Destroy(caseSelect.gameObject);
            caseSelect = null;
        }
        if (caseSelect == null && mainMenuPanel != null)
        {
            caseSelect = CaseSelect.Build((RectTransform)mainMenuPanel.transform, OnCaseChosen, () => caseSelect.gameObject.SetActive(false));
            caseSelectTheme = ThemeManager.Current;
        }
    }

    private const string NewGameDialog = "ConfirmarNuevaPartida";

    // Con una investigación a medias, empezar otra la borra: se pregunta antes (Continuar sigue en el menú)
    private void OnCaseChosen(string storyId)
    {
        if (!SaveSystem.Exists || caseSelect == null)
        {
            StartChosenCase(storyId);
            return;
        }
        ConfirmDialog.Show(caseSelect, NewGameDialog, GameTexts.NewGameConfirm, GameTexts.NewGameYes, GameTexts.NewGameNo,
            () => StartChosenCase(storyId));
    }

    private void StartChosenCase(string storyId)
    {
        Debug.Log($"[MenuManager] Caso elegido: {storyId ?? "al azar"}");
        SaveSystem.Delete(); // "Caso nuevo" descarta la investigación anterior
        gameManager?.ChooseStory(storyId);
        if (caseSelect != null)
            caseSelect.gameObject.SetActive(false);
        
        // Ocultar menú principal
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
            Debug.Log("[MenuManager] MainMenu ocultado");
        }
        
        // Activar IntroPanel (referencia directa)
        if (introPanel != null)
        {
            introPanel.SetActive(true);
            UIAnimations.FadeIn(this, introPanel); // Del menú al expediente, con fundido
            Debug.Log("[MenuManager] IntroPanel activado (referencia directa)");
            
            // Llamar a GameManager para que actualice los textos del intro
            if (gameManager != null)
            {
                gameManager.ShowCaseIntro();
            }
        }
        else
        {
            Debug.LogError("[MenuManager] ¡IntroPanel no conectado en Inspector!");
        }
    }
    
    // Un fotograma de espera para que GameManager haya elegido ya la historia
    private System.Collections.IEnumerator PlayNextFrame()
    {
        yield return null;
        OnPlayClicked();
    }

    private void ShowMainMenu()
    {
        ShowPanel(mainMenuPanel);
        SoundManager.PlayMusic(Music.Menu);
    }
    
    private void ShowInstructions()
    {
        ShowPanel(instructionsPanel);
    }
    
    private void ShowSettings()
    {
        if (settingsPanel != null)
            SettingsPanel.Build(settingsPanel, () => gameManager?.RestartGame());

        ShowPanel(settingsPanel);
    }
    
    private void ShowAbout()
    {
        ShowPanel(aboutPanel);
    }
    
    private void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    private void ShowPanel(GameObject panelToShow)
    {
        // Ocultar todos los paneles del menú
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (aboutPanel != null) aboutPanel.SetActive(false);
        
        // Mostrar el seleccionado
        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
            UIAnimations.FadeIn(this, panelToShow);
        }
    }
}
