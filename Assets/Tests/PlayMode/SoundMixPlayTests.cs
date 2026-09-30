using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// La mezcla en el SoundManager de verdad: una pista aparta la música y la música vuelve sola.
/// </summary>
public class SoundMixPlayTests
{
    [UnityTest]
    public IEnumerator UnaPistaApartaLaMusicaYLuegoVuelve()
    {
        SoundManager.PlayMusic(Music.Menu);
        yield return new WaitForSecondsRealtime(0.2f);
        Assert.AreEqual(1f, SoundManager.MusicDuck, 1e-4, "sin golpes, entera");

        SoundManager.Play(Sfx.Clue);
        yield return new WaitForSecondsRealtime(0.25f);
        Assert.AreEqual(SoundMix.Depth, SoundManager.MusicDuck, 0.02f, "apartada bajo la pista");

        yield return new WaitForSecondsRealtime(SoundMix.MaxHold + SoundMix.Release + 0.3f);
        Assert.AreEqual(1f, SoundManager.MusicDuck, 1e-4, "vuelve sola");
    }
}
