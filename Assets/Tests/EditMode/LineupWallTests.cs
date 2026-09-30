using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Ronda final: la rueda de reconocimiento sobre una pared de alturas de comisaría (como la ficha policial del
/// final), con un interruptor del tema. La rueda sigue siendo la misma (se rellena sin tocar la pared).
/// </summary>
public class LineupWallTests
{
    [TearDown]
    public void TearDown()
    {
        LayoutPreview.Close();
    }

    [Test]
    public void LaRuedaTieneSuParedDeAlturas()
    {
        LayoutPreview.Session session = LayoutPreview.Open(new Vector2(1080f, 1920f));
        RectTransform wall = LayoutPreview.Find(session, InterrogationUI.LineupWallName);
        RectTransform lineup = LayoutPreview.Find(session, "Rueda (auto)");

        Assert.IsNotNull(wall, "pared detrás de la rueda");
        Assert.AreEqual(wall, lineup.parent, "la rueda va sobre la pared (y se rellena sin borrarla)");
        Assert.IsTrue(wall.Find("Alturas").gameObject.activeSelf);
        StringAssert.Contains("180", string.Join(" ", System.Array.ConvertAll(
            wall.GetComponentsInChildren<TMPro.TMP_Text>(true), t => t.text)));
    }

    [Test]
    public void ElTemaQuitaLaPared()
    {
        Theme theme = Object.Instantiate(ThemeManager.Current);
        theme.hideFlags = HideFlags.DontSave;
        theme.lineupWall = false;
        ThemeManager.Override(theme);
        try
        {
            LayoutPreview.Session session = LayoutPreview.Open(new Vector2(1080f, 1920f));
            RectTransform wall = LayoutPreview.Find(session, InterrogationUI.LineupWallName);
            Assert.IsFalse(wall.Find("Alturas").gameObject.activeSelf);
        }
        finally
        {
            ThemeManager.Override(null);
            Object.DestroyImmediate(theme);
        }
    }
}
