using NUnit.Framework;

/// <summary>
/// Ninguna clave de Anthropic en archivos versionados: dos claves llegaron a main en las escenas del commit inicial
/// (revocadas el 01-10-2026). El prefijo se monta por partes para que este archivo no se detecte a sí mismo.
/// </summary>
public class SecretScanTests
{
    private static readonly string Prefix = "sk" + "-ant-";

    [Test]
    public void DetectaUnaClaveDeAnthropicEnUnTexto()
    {
        Assert.AreEqual(1, SecretScan.Find($"m_Text: {Prefix}api03-abcdef\napiKey: ''").Count);
        Assert.AreEqual(2, SecretScan.Find($"{Prefix}a {Prefix}b").Count);
        Assert.AreEqual(0, SecretScan.Find("anthropic_api_key.txt, sk-otra-cosa").Count);
    }

    [Test]
    public void NingunArchivoVersionadoContieneUnaClaveDeAnthropic()
    {
        var hits = SecretScan.ScanTrackedFiles();
        Assert.IsEmpty(hits, "Claves en archivos versionados (quítalas y revócalas):\n" + string.Join("\n", hits));
    }
}
