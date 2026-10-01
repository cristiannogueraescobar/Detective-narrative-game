using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Sesión A, bloque 4: la escena del interrogatorio (figura grande y tenue en el hueco del centro, viñeta de
/// tensión). La parte pura: cuánta tensión pinta cada estado y qué se mueve con "reducir animaciones".
/// </summary>
public class InterrogationSceneTests
{
    [Test]
    public void LaTensionSubeConElEstado()
    {
        float calm = SceneTension.Vignette(Emotion.Tranquilo);
        Assert.AreEqual(0f, calm, 1e-4f, "tranquilo: sin viñeta");
        Assert.Greater(SceneTension.Vignette(Emotion.Triste), calm);
        Assert.Greater(SceneTension.Vignette(Emotion.Nervioso), SceneTension.Vignette(Emotion.Triste));
        Assert.GreaterOrEqual(SceneTension.Vignette(Emotion.Asustado), SceneTension.Vignette(Emotion.Nervioso));
        Assert.LessOrEqual(SceneTension.Vignette(Emotion.Enfadado), 0.6f, "nunca tapa la pantalla");
    }

    [Test]
    public void ElEnfadoTiñeDeRojoYElRestoEsNegro()
    {
        Color angry = SceneTension.Tint(Emotion.Enfadado);
        Assert.Greater(angry.r, angry.g + 0.1f);
        Color nervous = SceneTension.Tint(Emotion.Nervioso);
        Assert.AreEqual(nervous.r, nervous.g, 0.05f);
    }

    [Test]
    public void LaFiguraTiemblaSoloSiHayTensionYNoSeReducenAnimaciones()
    {
        Assert.AreEqual(0f, SceneTension.Tremble(Emotion.Tranquilo, reduceMotion: false));
        Assert.Greater(SceneTension.Tremble(Emotion.Nervioso, reduceMotion: false), 0f);
        Assert.AreEqual(0f, SceneTension.Tremble(Emotion.Nervioso, reduceMotion: true), "reducir animaciones: quieta");
    }
}
