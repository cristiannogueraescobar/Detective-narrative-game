using NUnit.Framework;

public class NoOllamaPreloadTests
{
    [Test]
    public void LaSuitePlayModeNoPrecargaElModelo()
    {
        Assert.IsTrue(OllamaProvider.PreloadDisabled, "NoOllamaPreloadSetup apaga la precarga en toda la suite");
    }
}
