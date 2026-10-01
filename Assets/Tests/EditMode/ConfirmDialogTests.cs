using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmDialogTests
{
    private GameObject root;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Panel", typeof(RectTransform));
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
    }

    private static Button ButtonWithLabel(RectTransform dialog, string label)
    {
        foreach (Button b in dialog.GetComponentsInChildren<Button>(true))
        {
            if (b.GetComponentInChildren<TMP_Text>(true).text == label)
                return b;
        }
        return null;
    }

    [Test]
    public void SeReutilizaConOtrosTextosYOtraAccion()
    {
        int first = 0, second = 0;
        ConfirmDialog.Show(root.transform, "Aviso", "¿Uno?", "Sí/uno", "No", () => first++);
        RectTransform dialog = ConfirmDialog.Show(root.transform, "Aviso", "¿Dos?", "Vale", "Mejor no", () => second++);

        Assert.AreEqual(1, root.transform.childCount, "un solo aviso por nombre");
        StringAssert.Contains("¿Dos?", dialog.GetComponentInChildren<TMP_Text>().text);
        Button yes = ButtonWithLabel(dialog, "Vale");
        Assert.IsNotNull(yes, "el botón principal lleva el texto nuevo");
        Assert.IsNotNull(ButtonWithLabel(dialog, "Mejor no"));

        yes.onClick.Invoke();
        Assert.AreEqual(0, first, "la acción anterior ya no se dispara");
        Assert.AreEqual(1, second);
        Assert.IsFalse(dialog.gameObject.activeSelf, "se cierra al confirmar");
    }

    [Test]
    public void CancelarSoloCierra()
    {
        int yes = 0;
        RectTransform dialog = ConfirmDialog.Show(root.transform, "Aviso", "¿Seguro?", "Sí", "No", () => yes++);
        ButtonWithLabel(dialog, "No").onClick.Invoke();
        Assert.AreEqual(0, yes);
        Assert.IsFalse(dialog.gameObject.activeSelf);
        Assert.IsTrue(ConfirmDialog.Show(root.transform, "Aviso", "¿Seguro?", "Sí", "No", null).gameObject.activeSelf);
        Assert.IsTrue(ConfirmDialog.Hide(root.transform, "Aviso"));
        Assert.IsFalse(ConfirmDialog.Hide(root.transform, "Aviso"), "ya estaba cerrado");
    }
}
