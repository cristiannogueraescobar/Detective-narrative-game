using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Optimizaciones de UI para móvil.
/// Todo lo que cambia cada fotograma (retrato que tiembla o respira, texto escribiéndose, HUD que se sacude)
/// va en su propio Canvas anidado: así solo se reconstruye su malla, no la del Canvas entero.
/// </summary>
public static class UIPerformance
{
    public static void IsolateInOwnCanvas(Component target)
    {
        if (target == null || target.GetComponent<Canvas>() != null)
            return;

        target.gameObject.AddComponent<Canvas>();

        // Un Canvas anidado necesita su propio GraphicRaycaster si contiene elementos pulsables
        if (target.GetComponentInChildren<Selectable>(true) != null || target.GetComponentInChildren<Graphic>(true)?.raycastTarget == true)
            target.gameObject.AddComponent<GraphicRaycaster>();
    }
}
