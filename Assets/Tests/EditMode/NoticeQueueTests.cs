using System.Collections.Generic;
using NUnit.Framework;

public class NoticeQueueTests
{
    [Test]
    public void SinDiferirSeEjecutaAlMomento()
    {
        var log = new List<string>();
        var queue = new NoticeQueue();

        queue.Post(() => log.Add("pista"));

        CollectionAssert.AreEqual(new[] { "pista" }, log);
    }

    [Test]
    public void DiferidoEsperaAlFlushYRespetaElOrden()
    {
        var log = new List<string>();
        var queue = new NoticeQueue();

        queue.BeginDefer();
        queue.Post(() => log.Add("pista"));
        queue.Post(() => log.Add("contradicción"));
        log.Add("respuesta");
        queue.Flush();

        CollectionAssert.AreEqual(new[] { "respuesta", "pista", "contradicción" }, log);
    }

    [Test]
    public void FlushTerminaElModoDiferido()
    {
        var log = new List<string>();
        var queue = new NoticeQueue();

        queue.BeginDefer();
        queue.Flush();
        queue.Post(() => log.Add("después"));

        CollectionAssert.AreEqual(new[] { "después" }, log);
    }

    [Test]
    public void UnAvisoQueFallaNoImpideLosDemas()
    {
        var log = new List<string>();
        var queue = new NoticeQueue();

        queue.BeginDefer();
        queue.Post(() => throw new System.InvalidOperationException("roto"));
        queue.Post(() => log.Add("siguiente"));
        UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
        queue.Flush();
        UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
        queue.Post(() => log.Add("inmediato"));

        CollectionAssert.AreEqual(new[] { "siguiente", "inmediato" }, log);
    }

    [Test]
    public void DescartarVaciaSinEjecutar()
    {
        var log = new List<string>();
        var queue = new NoticeQueue();

        queue.BeginDefer();
        queue.Post(() => log.Add("pista"));
        queue.Discard();
        queue.Post(() => log.Add("ahora"));

        CollectionAssert.AreEqual(new[] { "ahora" }, log);
    }
}
