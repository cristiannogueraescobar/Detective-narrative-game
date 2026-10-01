using NUnit.Framework;
using UnityEngine;

public class EmotionPoseTests
{
    private static EmotionPose Pose(Emotion e, bool reduce = false) => EmotionPose.For(e, ThemeManager.Current, reduce);

    [Test]
    public void TranquiloEsLaPoseNeutra()
    {
        EmotionPose calm = Pose(Emotion.Tranquilo);
        Assert.AreEqual(Vector2.zero, calm.offset);
        Assert.AreEqual(1f, calm.scale, 1e-4f);
        Assert.AreEqual(1f, calm.saturation, 1e-4f);
        Assert.AreEqual(0f, calm.tremble);
        Assert.IsFalse(calm.sweat);
    }

    [Test]
    public void NerviosoTiemblaYSuda()
    {
        EmotionPose p = Pose(Emotion.Nervioso);
        Assert.Greater(p.tremble, 0f);
        Assert.IsTrue(p.sweat);
    }

    [Test]
    public void AsustadoRetrocede()
    {
        EmotionPose p = Pose(Emotion.Asustado);
        Assert.Less(p.scale, 1f, "se hace pequeño, como si diera un paso atrás");
        Assert.Greater(p.tremble, 0f);
    }

    [Test]
    public void EnfadadoSeAcercaYEnrojece()
    {
        EmotionPose p = Pose(Emotion.Enfadado);
        Assert.Greater(p.scale, 1f, "se inclina hacia el detective");
        Assert.Greater(p.tint.r, p.tint.g, "tinte rojizo");
        Assert.Greater(p.tint.r, p.tint.b);
    }

    [Test]
    public void TristeBajaYSeDesatura()
    {
        EmotionPose p = Pose(Emotion.Triste);
        Assert.Less(p.offset.y, 0f, "baja la cabeza");
        Assert.Less(p.saturation, 0.7f);
    }

    [Test]
    public void ReducirAnimacionesQuitaElMovimientoPeroNoElColor()
    {
        foreach (Emotion e in System.Enum.GetValues(typeof(Emotion)))
        {
            EmotionPose moving = Pose(e);
            EmotionPose still = Pose(e, reduce: true);
            Assert.AreEqual(Vector2.zero, still.offset, e.ToString());
            Assert.AreEqual(1f, still.scale, 1e-4f, e.ToString());
            Assert.AreEqual(0f, still.tremble, e.ToString());
            Assert.IsFalse(still.sweat, e.ToString());
            Assert.AreEqual(moving.tint, still.tint, e.ToString());
            Assert.AreEqual(moving.saturation, still.saturation, 1e-4f, e.ToString());
        }
    }

    [Test]
    public void LaTransicionEsSuaveYConverge()
    {
        EmotionPose from = Pose(Emotion.Tranquilo);
        EmotionPose to = Pose(Emotion.Triste);

        EmotionPose first = EmotionPose.Step(from, to, 1f / 60f, 0.35f);
        Assert.Greater(first.offset.y, to.offset.y, "no salta de golpe");
        Assert.Less(first.offset.y, 0f, "pero ya se mueve");

        EmotionPose current = from;
        for (int i = 0; i < 180; i++)
            current = EmotionPose.Step(current, to, 1f / 60f, 0.35f);
        Assert.AreEqual(to.offset.y, current.offset.y, 0.05f);
        Assert.AreEqual(to.saturation, current.saturation, 0.01f);
    }

    [Test]
    public void DeltaCeroNoMueve()
    {
        EmotionPose from = Pose(Emotion.Tranquilo);
        EmotionPose same = EmotionPose.Step(from, Pose(Emotion.Enfadado), 0f, 0.35f);
        Assert.AreEqual(from.scale, same.scale, 1e-5f);
    }
}
