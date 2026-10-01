using NUnit.Framework;

/// <summary>
/// Sesión C, punto 5 (excepción autorizada en la capa de proveedores): la suite PlayMode abría la escena y
/// AIConversationManager.Start llamaba a WarmUpAsync, que cargaba qwen en Ollama (5,6 GB de GPU) aunque los tests
/// usan un proveedor falso. Con OllamaProvider.PreloadDisabled los tests lo apagan; el juego sigue precargando.
/// </summary>
public class OllamaPreloadTests
{
    [TearDown]
    public void TearDown()
    {
        OllamaProvider.PreloadDisabled = false;
    }

    [Test]
    public void ElJuegoSiguePrecargandoPorDefecto()
    {
        Assert.IsFalse(OllamaProvider.PreloadDisabled, "nada lo apaga fuera de los tests");
        Assert.IsTrue(new OllamaSettings().preloadOnStart);
    }

    [Test]
    public void ConLaPrecargaApagadaNoSeLlamaAOllama()
    {
        OllamaProvider.PreloadDisabled = true;
        int before = OllamaProvider.PreloadRequests;
        var provider = new OllamaProvider(new OllamaSettings { baseUrl = "http://127.0.0.1:9" });
        System.Threading.Tasks.Task warm = provider.WarmUpAsync();
        Assert.IsTrue(warm.IsCompleted, "vuelve en el acto");
        Assert.AreEqual(before, OllamaProvider.PreloadRequests, "ninguna petición");
    }
}
