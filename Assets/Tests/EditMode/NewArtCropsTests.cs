using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Encuadre del arte nuevo (Assets/Art/Portraits/&lt;artId&gt;_&lt;estado&gt;.png), docs/art/javier/BRIEF.md 6.5: cuando
/// llegue el arte basta con soltar los PNG. Mientras no llegue, todo sigue exactamente igual.
/// </summary>
public class NewArtCropsTests
{
    private Texture2D newArt;
    private Texture2D legacyArt;

    [SetUp]
    public void SetUp()
    {
        newArt = new Texture2D(768, 1024);    // Tamaño del encargo
        legacyArt = new Texture2D(760, 1158); // padre.gif.png, base del Javier derivado
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(newArt);
        Object.DestroyImmediate(legacyArt);
    }

    [Test]
    public void JavierTieneEncuadrePropioParaElArteNuevo()
    {
        Assert.IsTrue(PortraitCrops.HasNewArtCrops("javier"));
        PortraitCrops.Crops crops = PortraitCrops.For("Javier", "javier", legacy: false, newArt);
        Assert.AreNotEqual(PortraitCrops.Full, crops.figure, "la rueda no usa la imagen entera");
        Assert.AreNotEqual(PortraitCrops.Full, crops.bust, "el interrogatorio usa un plano medio, no la figura entera");
        Assert.AreEqual(PortraitCrops.NewArt("javier").figure, crops.figure);
        // Encargo: figura al 90 % del alto con los pies a 40 px del borde inferior de 1024
        Assert.AreEqual(40f / 1024f, crops.figure.y, 0.005f);
        Assert.AreEqual(0.90f, crops.figure.height, 0.01f);
        Assert.Greater(crops.bust.y, crops.figure.y, "el busto empieza por encima de los pies");
        Assert.Greater(crops.face.y, crops.bust.y, "la cara está dentro de la parte alta del busto");
    }

    [Test]
    public void EnLaRuedaJavierMide175cmConLaFiguraDeSuArteNuevo()
    {
        PortraitCrops.Crops crops = PortraitCrops.For("Javier", "javier", legacy: false, newArt);
        float aspect = PortraitCrops.Aspect(newArt, crops.figure);
        Assert.AreEqual(768f * crops.figure.width / (1024f * crops.figure.height), aspect, 1e-4f);
        var plan = HeightLineup.Plan(1000f, 900f, 64f, new[] { (178, 0.55f), (164, 0.4f), (175, aspect), (160, 0.45f) });
        Assert.AreEqual(175f * plan.pxPerCm, plan.figureHeights[2], 0.01f, "su figura (pies a cabeza) mide 175 cm");
        Assert.Greater(plan.figureHeights[2], plan.figureHeights[1], "más alto que Lucía (164)");
    }

    [Test]
    public void SinArteNuevoJavierSigueConElEncuadreDeHoy()
    {
        // legacy = true: el retrato derivado de padre.gif.png, como hasta ahora
        PortraitCrops.Crops crops = PortraitCrops.For("Javier", "javier", legacy: true, legacyArt);
        Assert.AreEqual(PortraitCrops.Figure("Javier"), crops.figure);
        Assert.AreEqual(PortraitCrops.Bust("Javier"), crops.bust);
        Assert.AreEqual(PortraitCrops.Face("Javier", legacyArt), crops.face);
        Assert.AreEqual(legacyArt.width * crops.figure.width / (legacyArt.height * crops.figure.height),
                        PortraitCrops.Aspect(legacyArt, crops.figure), 1e-4f);
    }

    [Test]
    public void ElArteNuevoSinEncuadreMedidoSigueComoAntes()
    {
        // Lo que hacía el juego con cualquier arte nuevo: imagen entera y cara con el recorte genérico
        Assert.IsFalse(PortraitCrops.HasNewArtCrops("daniel"));
        PortraitCrops.Crops crops = PortraitCrops.For("Padre", "daniel", legacy: false, newArt);
        Assert.AreEqual(PortraitCrops.Full, crops.figure);
        Assert.AreEqual(PortraitCrops.Full, crops.bust);
        Assert.AreEqual(UISprites.FaceCrop(newArt), crops.face);
    }
}
