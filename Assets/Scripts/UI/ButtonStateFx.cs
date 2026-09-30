using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Estados de botón que se ven de un vistazo: pulsado se hunde un poco (salvo con "Reducir animaciones") y
/// desactivado se apaga (opacidad del tema). El color de cada estado lo pone el ColorBlock (ThemeApplier).
/// Solo actúa al pulsar y cuando cambia "interactable" (sin coste por fotograma mientras no cambie nada).
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonStateFx : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private Button button;
    private CanvasGroup group;
    private bool lastInteractable = true;
    private bool initialized;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (initialized)
            return;
        button = GetComponent<Button>();
        if (!TryGetComponent(out group))
            group = gameObject.AddComponent<CanvasGroup>();
        initialized = true;
        Refresh();
    }

    private void Update()
    {
        if (button.interactable != lastInteractable)
            Refresh();
    }

    public void Refresh()
    {
        Init();
        lastInteractable = button.interactable;
        group.alpha = lastInteractable ? 1f : ThemeManager.Current.disabledAlpha;
        if (!lastInteractable)
            transform.localScale = Vector3.one;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button.interactable && !GameSettings.ReduceMotion)
            transform.localScale = Vector3.one * ThemeManager.Current.pressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
    }
}
