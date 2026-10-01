using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// WCAG 2.4.7 (foco visible): con teclado o mando (build de PC) el botón enfocado se distingue; con el dedo, el
/// botón tocado no se queda "encendido" (se suelta la selección al levantar el dedo).
/// </summary>
public class FocusVisibleTests
{
    private GameObject root;

    [TearDown]
    public void TearDown()
    {
        if (root != null)
            Object.DestroyImmediate(root);
    }

    private Button MakeButton(UIRole role)
    {
        root = new GameObject("Raíz", typeof(RectTransform));
        var go = new GameObject("Boton", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(root.transform, false);
        go.AddComponent<ThemeRole>().role = role;
        ThemeApplier.Apply(root.transform);
        return go.GetComponent<Button>();
    }

    [TestCase(UIRole.PrimaryButton)]
    [TestCase(UIRole.SecondaryButton)]
    [TestCase(UIRole.DangerButton)]
    public void ElBotonEnfocadoSeDistingue(UIRole role)
    {
        ColorBlock colors = MakeButton(role).colors;
        Color normal = colors.normalColor, focused = colors.selectedColor;
        float change = Mathf.Abs(normal.r - focused.r) + Mathf.Abs(normal.g - focused.g) + Mathf.Abs(normal.b - focused.b);
        Assert.Greater(change, 0.3f, "el foco tiene que verse");
    }

    [Test]
    public void AlLevantarElDedoNoQuedaSeleccionado()
    {
        Button button = MakeButton(UIRole.PrimaryButton);
        var events = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
        try
        {
            events.SetSelectedGameObject(button.gameObject);
            var fx = button.GetComponent<ButtonStateFx>();
            fx.OnPointerUp(new PointerEventData(events) { pointerId = 0 });

            Assert.IsNull(events.currentSelectedGameObject);
        }
        finally
        {
            Object.DestroyImmediate(events.gameObject);
        }
    }
}
