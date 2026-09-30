using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Botón Atrás del sistema (Android lo envía como Escape): lo reparte entre la partida y el menú, de dentro
/// hacia fuera. Si nadie lo usa, no hace nada (no se sale del juego por accidente).
/// </summary>
public class BackButtonRouter : MonoBehaviour
{
    private InterrogationUI game;
    private MenuManager menu;

    public static BackButtonRouter Ensure(GameObject host)
    {
        return host.TryGetComponent(out BackButtonRouter existing) ? existing : host.AddComponent<BackButtonRouter>();
    }

    private void Start()
    {
        game = FindFirstObjectByType<InterrogationUI>();
        menu = FindFirstObjectByType<MenuManager>();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Back();
    }

    /// <summary>
    /// Devuelve true si alguien usó el Atrás.
    /// </summary>
    public bool Back()
    {
        if (game != null && game.HandleBack())
            return true;
        return menu != null && menu.HandleBack();
    }
}
