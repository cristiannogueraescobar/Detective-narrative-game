using NUnit.Framework;

/// <summary>
/// Comprobación de memoria entre partidas del bot (Sesión A): una respuesta de la partida anterior que llega al modelo
/// es una fuga, salvo que la partida actual la haya vuelto a dar ella misma (misma variante, misma pregunta: el modelo
/// repite su ficha palabra por palabra).
/// </summary>
public class BotMemoryCheckTests
{
    private const string Previous = "Encarna está obsesionada con Paula: tiene fotos suyas junto a las de su hija muerta.";

    [Test]
    public void UnaRespuestaDeLaPartidaAnteriorEnElPromptEsUnaFuga()
    {
        int leaks = BotPlayer.CountLeaks(new[] { Previous }, new[] { "ficha\nuser: hola\nassistant: " + Previous }, new string[0]);
        Assert.AreEqual(1, leaks);
    }

    [Test]
    public void SiLaPartidaActualLaRepiteNoEsFuga()
    {
        int leaks = BotPlayer.CountLeaks(new[] { Previous }, new[] { "ficha\nassistant: " + Previous }, new[] { Previous + " *(nervioso)*" });
        Assert.AreEqual(0, leaks, "la misma pregunta en la misma variante da la misma respuesta: no es memoria");
    }

    [Test]
    public void SinRastroNoHayFuga()
    {
        Assert.AreEqual(0, BotPlayer.CountLeaks(new[] { Previous }, new[] { "ficha\nuser: otra cosa" }, new string[0]));
    }
}
