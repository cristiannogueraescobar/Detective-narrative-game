using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Ronda final 4: en los móviles alargados (1080 × 2400) el retrato gana parte del alto que sobra (antes todo iba
/// al chat y, con una conversación corta, quedaba una franja vacía); a 1080 × 1920 todo sigue igual.
/// </summary>
public class TallScreenLayoutTests
{
    [TearDown]
    public void TearDown()
    {
        LayoutPreview.Close();
    }

    // LayoutPreview.Close quita el tema forzado: se vuelve a poner en cada sesión
    private static float HeaderHeight(Vector2 size, Theme theme = null)
    {
        if (theme != null)
            ThemeManager.Override(theme);
        LayoutPreview.Session session = LayoutPreview.Open(size);
        LayoutPreview.ShowOnly(session, "InterrogationPanel");
        LayoutPreview.Rebuild((RectTransform)session.canvas.transform);
        float h = LayoutPreview.Find(session, "Retrato (auto)").rect.height;
        LayoutPreview.Close();
        return h;
    }

    [Test]
    public void ElRetratoCreceEnPantallasAlargadasYNoEnLasNormales()
    {
        // Lienzos de verdad (CanvasScaler con match 0,5): un 1080x2400 da ~966x2147, no 1080x2400
        Vector2 phone = LayoutPreview.CanvasSize(1080f, 2400f);
        float normal = HeaderHeight(LayoutPreview.CanvasSize(1080f, 1920f));
        float tall = HeaderHeight(phone);

        float expected = (phone.y - phone.x * 1920f / 1080f) * ThemeManager.Current.tallScreenHeaderShare;
        Assert.AreEqual(normal + expected, tall, 12f, "el retrato se lleva su parte del alto que sobra por la proporción");
        Assert.Greater(tall, normal + 100f);
    }

    [Test]
    public void ConLaParteACeroTodoComoAntes()
    {
        Theme theme = Object.Instantiate(ThemeManager.Current);
        theme.hideFlags = HideFlags.DontSave; // Abrir la escena descarga lo que no se referencia
        theme.tallScreenHeaderShare = 0f;
        try
        {
            Assert.AreEqual(HeaderHeight(LayoutPreview.CanvasSize(1080f, 1920f), theme), HeaderHeight(LayoutPreview.CanvasSize(1080f, 2400f), theme), 1f);
        }
        finally
        {
            ThemeManager.Override(null);
            Object.DestroyImmediate(theme);
        }
    }
}
