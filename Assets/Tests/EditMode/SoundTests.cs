using System;
using System.Linq;
using NUnit.Framework;

public class SoundTests
{
    [Test]
    public void CadaEfectoTieneSuArchivoPropio()
    {
        var paths = Enum.GetValues(typeof(Sfx)).Cast<Sfx>().Select(SoundCatalog.PathOf).ToList();
        CollectionAssert.AllItemsAreUnique(paths);
        Assert.IsTrue(paths.All(p => p.StartsWith("Audio/sfx/")));
    }

    [Test]
    public void CadaMusicaTieneSuArchivoSalvoNinguna()
    {
        Assert.IsNull(SoundCatalog.PathOf(Music.None));
        var paths = Enum.GetValues(typeof(Music)).Cast<Music>().Where(m => m != Music.None).Select(SoundCatalog.PathOf).ToList();
        CollectionAssert.AllItemsAreUnique(paths);
        Assert.IsTrue(paths.All(p => p.StartsWith("Audio/musica/")));
    }

    [Test]
    public void CadaHistoriaYCadaFinalTienenSuSonido()
    {
        foreach (StoryData story in CaseLibrary.Stories)
            Assert.AreNotEqual(Music.Menu, SoundCatalog.ForStory(story.id), story.id);
        var endings = Enum.GetValues(typeof(Ending)).Cast<Ending>().Select(SoundCatalog.ForEnding).ToList();
        CollectionAssert.AllItemsAreUnique(endings);
    }

    [TestCase(1f, 1f, 1f)]
    [TestCase(0.5f, 0.5f, 0.25f)]
    [TestCase(0f, 1f, 0f)]
    [TestCase(2f, -1f, 0f)]
    public void VolumenEfectivo(float master, float channel, float expected)
    {
        Assert.AreEqual(expected, SoundCatalog.Effective(master, channel), 1e-5f);
    }

    [Test]
    public void SinArchivosNoFallaYSeAnota()
    {
        int before = SoundManager.Played.Count;
        Assert.DoesNotThrow(() => SoundManager.Play(Sfx.Clue));
        Assert.DoesNotThrow(() => SoundManager.PlayMusic(Music.Story2));
        Assert.AreEqual(Sfx.Clue, SoundManager.Played.Last());
        Assert.GreaterOrEqual(SoundManager.Played.Count, before + 1);
    }

    [Test]
    public void ElDocumentoDeAudioListaTodosLosArchivos()
    {
        string doc = System.IO.File.ReadAllText("AUDIO-NEEDED.md");
        foreach (Sfx sfx in Enum.GetValues(typeof(Sfx)))
            StringAssert.Contains(SoundCatalog.PathOf(sfx), doc, sfx.ToString());
        foreach (Music m in Enum.GetValues(typeof(Music)))
        {
            if (m != Music.None)
                StringAssert.Contains(SoundCatalog.PathOf(m), doc, m.ToString());
        }
    }
}
