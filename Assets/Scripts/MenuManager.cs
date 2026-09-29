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

        if (applyMobileLayout)
        {
            foreach (GameObject panel in new[] { mainMenuPanel, instructionsPanel, settingsPanel, aboutPanel })
            {
                if (panel != null)
                    MobilePanelLayout.Apply((RectTransform)panel.transform);
            }
        }
        
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
    /// y el de jugar pasa a llamarse "Nueva partida".
    /// </summary>
    private void SetUpContinueButton()
    {
        if (playButton == null || !SaveSystem.TryLoad(out _))
            return;

        GameObject clone = Instantiate(playButton.gameObject, playButton.transform.parent);
        clone.name = "ContinueButton (auto)";
        clone.transform.SetSiblingIndex(playButton.transform.GetSiblingIndex()); // Encima de "Nueva partida"
        var rect = (RectTransform)clone.transform;
        var source = (RectTransform)playButton.transform;
        rect.anchoredPosition = source.anchoredPosition + new Vector2(0f, source.rect.height + ThemeManager.Current.spacing);

        var continueButton = clone.GetComponent<Button>();
        UIComponents.SetOnlyListener(continueButton, OnContinueClicked);
        SetLabel(clone, "Continuar");
        SetLabel(playButton.gameObject, "Nueva partida");
    }

    private static void SetLabel(GameObject button, string text)
    {
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
            label.text = text;
    }

    private void OnContinueClicked()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (gameManager == null || !gameManager.ContinueSavedGame())
        {
            // El guardado ya no es válido: se empieza una partida nueva
            SaveSystem.Delete();
            OnPlayClicked();
        }
    }

    private void OnPlayClicked()
    {
        Debug.Log("[MenuManager] Botón JUGAR presionado");
        SaveSystem.Delete(); // "Nueva partida" descarta la anterior
        
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
