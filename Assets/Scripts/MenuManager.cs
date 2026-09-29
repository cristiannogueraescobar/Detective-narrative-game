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
    
    [Header("Referencia GameManager")]
    [SerializeField] private GameManager gameManager; // NUEVO
    
    [Header("Imágenes de Fondo (Opcional)")]
    [SerializeField] private RawImage mainMenuBackground;
    [SerializeField] private Texture2D mainMenuBackgroundTexture;
    
    private void Start()
    {
        // Configurar botones del menú principal
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);
        
        if (instructionsButton != null)
            instructionsButton.onClick.AddListener(ShowInstructions);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(ShowSettings);
        
        if (aboutButton != null)
            aboutButton.onClick.AddListener(ShowAbout);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
        
        // Configurar botones de retorno
        if (backFromInstructionsButton != null)
            backFromInstructionsButton.onClick.AddListener(ShowMainMenu);
        
        if (backFromSettingsButton != null)
            backFromSettingsButton.onClick.AddListener(ShowMainMenu);
        
        if (backFromAboutButton != null)
            backFromAboutButton.onClick.AddListener(ShowMainMenu);
        
        // Aplicar fondo si existe
        if (mainMenuBackground != null && mainMenuBackgroundTexture != null)
        {
            mainMenuBackground.texture = mainMenuBackgroundTexture;
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
    
    private void OnPlayClicked()
    {
        Debug.Log("[MenuManager] Botón JUGAR presionado");
        
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
            panelToShow.SetActive(true);
    }
}
