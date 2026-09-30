using NUnit.Framework;
using TMPro;
using UnityEngine;

public class ThemeApplierTests
{
    [TestCase("AccusationPanel", false, false, false, UIRole.Panel)]
    [TestCase("IntroPanel", false, false, false, UIRole.Panel)]
    [TestCase("Canvas", false, false, false, UIRole.Background)]
    [TestCase("AskButton", true, false, false, UIRole.PrimaryButton)]
    [TestCase("Accusebutton", true, false, false, UIRole.PrimaryButton)]
    [TestCase("StartButton", true, false, false, UIRole.PrimaryButton)]
    [TestCase("BackFromAboutButton", true, false, false, UIRole.SecondaryButton)]
    [TestCase("EndDayButton", true, false, false, UIRole.SecondaryButton)]
    [TestCase("CaseTitleText", false, true, false, UIRole.Title)]
    [TestCase("GameTitleText", false, true, false, UIRole.Title)]
    [TestCase("CluesTitleText", false, true, false, UIRole.Heading)]
    [TestCase("HudText", false, true, false, UIRole.Secondary)]
    [TestCase("WaitingText", false, true, false, UIRole.Secondary)]
    [TestCase("ConversationText", false, true, false, UIRole.Body)]
    [TestCase("Text (TMP)", false, true, true, UIRole.ButtonLabel)]
    [TestCase("Scrollbar", false, false, false, UIRole.Ignore)]
    [TestCase("Template", false, false, false, UIRole.Panel)]
    [TestCase("Item Background", false, false, false, UIRole.Field)]
    [TestCase("Item Checkmark", false, false, false, UIRole.Accent)]
    public void RolPorNombreYTipo(string name, bool isButton, bool isText, bool insideButton, UIRole expected)
    {
        Assert.AreEqual(expected, ThemeApplier.RoleFor(name, isButton, isText, insideButton));
    }

    [TestCase("UISprite", false)]
    [TestCase("Background", false)]
    [TestCase("InputFieldBackground", false)]
    [TestCase("Knob", false)]
    [TestCase("menu_fondo.png_0", true)]
    [TestCase("sospechosos_imagen.png_0", true)]
    [TestCase(null, false)]
    public void IlustracionesNoSeTinenComoPaneles(string spriteName, bool isArtwork)
    {
        Assert.AreEqual(isArtwork, ThemeApplier.IsArtworkSprite(spriteName));
    }

    [Test]
    public void ReaplicarElTemaNoPisaElTamanoQueFijoLaDistribucion()
    {
        var canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
        try
        {
            var title = new GameObject("GameTitleText", typeof(RectTransform));
            title.transform.SetParent(canvas.transform, false);
            var text = title.AddComponent<TextMeshProUGUI>();
            text.text = "DETECTIVES";
            LayoutKit.MultiLine(text, 245f);

            ThemeApplier.Apply(canvas.transform);

            Assert.AreEqual(245f, text.fontSizeMax, 0.5f, "el tamaño de la distribución se conserva");
            Assert.AreEqual(ThemeManager.Current.accent, text.color, "el color sí lo pone el tema");
        }
        finally
        {
            Object.DestroyImmediate(canvas);
        }
    }
}
