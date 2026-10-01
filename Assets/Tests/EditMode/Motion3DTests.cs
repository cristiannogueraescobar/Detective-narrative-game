using System;
using NUnit.Framework;
using UnityEngine;

public class Motion3DTests
{
    [Test]
    public void RespiracionEntreUnoYUnoMasAmplitudYPeriodica()
    {
        Assert.AreEqual(1f, Motion3D.Breath(0f, 4f, 0.015f), 1e-5);

        for (float t = 0f; t < 8f; t += 0.1f)
        {
            float s = Motion3D.Breath(t, 4f, 0.015f);
            Assert.GreaterOrEqual(s, 1f - 1e-5f);
            Assert.LessOrEqual(s, 1.015f + 1e-5f);
            Assert.AreEqual(s, Motion3D.Breath(t + 4f, 4f, 0.015f), 1e-4);
        }
    }

    [Test]
    public void InclinacionEmpiezaYAcabaPlana()
    {
        Assert.AreEqual(0f, Motion3D.Tilt(0f, 8f), 1e-4);
        Assert.AreEqual(0f, Motion3D.Tilt(1f, 8f), 1e-4);
        Assert.Greater(Motion3D.Tilt(0.3f, 8f), 0f);
        Assert.LessOrEqual(Mathf.Abs(Motion3D.Tilt(0.3f, 8f)), 8f + 1e-4f);
    }

    [Test]
    public void ParallaxLimitadoAlMaximo()
    {
        Assert.AreEqual(Vector2.zero, Motion3D.Parallax(Vector2.zero, 20f));
        Vector2 far = Motion3D.Parallax(new Vector2(5f, -5f), 20f);
        Assert.LessOrEqual(far.magnitude, 20f + 1e-4f);
        Assert.Less(far.x, 0f, "la capa se mueve en contra de la inclinación");
    }

    [Test]
    public void GiroDeCartaDeNoventaACero()
    {
        Assert.AreEqual(90f, Motion3D.CardFlip(0f), 1e-3);
        Assert.AreEqual(0f, Motion3D.CardFlip(1f), 1e-3);
        Assert.Less(Motion3D.CardFlip(0.5f), 90f);
    }

    [Test]
    public void SinReservasDeMemoriaPorFotograma()
    {
        // Calentamiento (JIT)
        Motion3D.Breath(0.1f, 4f, 0.015f);
        Motion3D.Tilt(0.1f, 8f);
        Motion3D.Parallax(Vector2.one, 20f);
        Motion3D.CardFlip(0.1f);

        long before = GC.GetAllocatedBytesForCurrentThread();
        float sink = 0f;
        for (int i = 0; i < 10000; i++)
        {
            float t = i * 0.016f;
            sink += Motion3D.Breath(t, 4f, 0.015f) + Motion3D.Tilt(t % 1f, 8f) + Motion3D.CardFlip(t % 1f);
            sink += Motion3D.Parallax(new Vector2(Mathf.Sin(t), Mathf.Cos(t)), 20f).x;
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.AreEqual(0, allocated, $"reservas por 10 000 fotogramas simulados (sink {sink})");
    }
}
