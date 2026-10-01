using NUnit.Framework;

/// <summary>
/// Para toda la suite PlayMode: los tests usan proveedores falsos y no deben cargar el modelo real en la GPU al abrir
/// la escena (sesión C, punto 5). Sin namespace: NUnit lo aplica a todo el ensamblado.
/// </summary>
[SetUpFixture]
public class NoOllamaPreloadSetup
{
    [OneTimeSetUp]
    public void DisablePreload()
    {
        OllamaProvider.PreloadDisabled = true;
    }

    [OneTimeTearDown]
    public void RestorePreload()
    {
        OllamaProvider.PreloadDisabled = false;
    }
}
