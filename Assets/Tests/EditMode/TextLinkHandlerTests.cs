using NUnit.Framework;
using TMPro;
using UnityEngine;

public class TextLinkHandlerTests
{
    private GameObject canvas;
    private GameObject go;
    private TextMeshProUGUI text;

    [SetUp]
    public void SetUp()
    {
        canvas = new GameObject("Lienzo", typeof(Canvas));
        go = new GameObject("Texto", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        text = go.AddComponent<TextMeshProUGUI>();
        text.font = UIFactory.DefaultFont();
        text.fontSize = 40f;
        ((RectTransform)go.transform).sizeDelta = new Vector2(900f, 600f);
        text.alignment = TextAlignmentOptions.TopLeft;
        text.text = "<link=\"a\">Primero</link>\n\n\n<link=\"b\">Segundo</link>";
        text.ForceMeshUpdate(true, true);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(canvas);
    }

    private Rect LinkRect(int index)
    {
        TMP_LinkInfo link = text.textInfo.linkInfo[index];
        TMP_CharacterInfo first = text.textInfo.characterInfo[link.linkTextfirstCharacterIndex];
        TMP_CharacterInfo last = text.textInfo.characterInfo[link.linkTextfirstCharacterIndex + link.linkTextLength - 1];
        return Rect.MinMaxRect(first.bottomLeft.x, first.descender, last.topRight.x, first.ascender);
    }

    [Test]
    public void UnToqueJustoEncimaDelEnlaceCuenta()
    {
        Rect a = LinkRect(0);
        Assert.AreEqual(0, TextLinkHandler.NearestLink(text, a.center, 60f));
    }

    [Test]
    public void UnToqueCercaCuentaComoElEnlaceMasProximo()
    {
        Rect a = LinkRect(0);
        Rect b = LinkRect(1);
        Assert.AreEqual(0, TextLinkHandler.NearestLink(text, new Vector2(a.center.x, a.yMin - 25f), 60f), "un dedo que cae un poco por debajo");
        Assert.AreEqual(1, TextLinkHandler.NearestLink(text, new Vector2(b.xMax + 30f, b.center.y), 60f), "o un poco a la derecha");
    }

    [Test]
    public void UnToqueLejosNoEsNingunEnlace()
    {
        Rect b = LinkRect(1);
        Assert.AreEqual(-1, TextLinkHandler.NearestLink(text, new Vector2(b.center.x, b.yMin - 200f), 60f));
    }
}
