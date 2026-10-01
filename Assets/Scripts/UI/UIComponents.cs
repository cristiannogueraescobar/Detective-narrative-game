using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Utilidades seguras para componentes de UI.
/// </summary>
public static class UIComponents
{
    /// <summary>
    /// Devuelve el componente o lo añade. No usar "GetComponent() ?? AddComponent()": en el editor
    /// GetComponent devuelve un null falso que "??" no detecta.
    /// </summary>
    public static T GetOrAdd<T>(GameObject go) where T : Component
    {
        return go.TryGetComponent(out T existing) ? existing : go.AddComponent<T>();
    }

    /// <summary>
    /// Deja el botón con una sola acción: apaga las llamadas persistentes puestas en el Inspector
    /// (evita que un clic ejecute dos cosas) y quita las añadidas por código.
    /// </summary>
    public static void SetOnlyListener(Button button, UnityAction action)
    {
        if (button == null)
            return;

        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            button.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }
}
