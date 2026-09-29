using NUnit.Framework;

public class MobilePanelLayoutTests
{
    [TestCase("CaseTitleText", false, true, false, false, false, PanelSlot.Top)]
    [TestCase("AccusationTitleText", false, true, false, false, false, PanelSlot.Top)]
    [TestCase("CaseDescrptionText", false, true, false, false, false, PanelSlot.Middle)]
    [TestCase("ResultDetailsText", false, true, false, false, false, PanelSlot.Middle)]
    [TestCase("StartButton", true, false, false, false, true, PanelSlot.Bottom)]
    [TestCase("CloseCluesButton", true, false, false, false, true, PanelSlot.Bottom)]
    [TestCase("AccusationDropdown", false, false, true, false, true, PanelSlot.Field)]
    [TestCase("ConversationScroll", false, false, false, true, true, PanelSlot.Middle)]
    [TestCase("SuspectsGroupImage", false, false, false, false, true, PanelSlot.Background)]
    public void ClasificaLosHijosDeUnPanel(string name, bool button, bool text, bool dropdown, bool scroll, bool image, PanelSlot expected)
    {
        Assert.AreEqual(expected, MobilePanelLayout.Classify(name, button, text, dropdown, scroll, image));
    }
}
