using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// Notas del jugador sobre cada sospechoso (ronda 9, idea de Golden Idol / libretas de deducción): el jugador
/// tacha a quien descarta y marca a quien sospecha; la libreta y la rueda lo reflejan, y se guarda con la partida.
/// </summary>
public class SuspectNotesTests
{
    [Test]
    public void TocarLaNotaVaSinNotaSospechosoDescartadoYVuelta()
    {
        Assert.AreEqual(SuspectNote.Sospechoso, SuspectNotes.Next(SuspectNote.Ninguna));
        Assert.AreEqual(SuspectNote.Descartado, SuspectNotes.Next(SuspectNote.Sospechoso));
        Assert.AreEqual(SuspectNote.Ninguna, SuspectNotes.Next(SuspectNote.Descartado));
    }

    [Test]
    public void LasNotasSeGuardanYSeRecuperan()
    {
        var notes = new Dictionary<string, SuspectNote> { { "padre", SuspectNote.Descartado }, { "madre", SuspectNote.Sospechoso } };

        Dictionary<string, SuspectNote> loaded = SuspectNotes.FromSave(SuspectNotes.ToSave(notes));

        Assert.AreEqual(SuspectNote.Descartado, loaded["padre"]);
        Assert.AreEqual(SuspectNote.Sospechoso, loaded["madre"]);
        Assert.AreEqual(0, SuspectNotes.FromSave(null).Count, "guardados anteriores: sin notas");
    }

    [Test]
    public void LaLibretaEnseñaLaNotaComoEnlace()
    {
        StoryData story = TestCases.Story();
        var state = new InvestigationState(story.variants[0]);
        var notes = new Dictionary<string, SuspectNote> { { "b", SuspectNote.Descartado } };

        string notebook = Notebook.Format(story, state, new[] { "a", "b" }, new Dictionary<string, Emotion>(), c => "x",
            onPaper: true, notes: notes);

        StringAssert.Contains($"<link=\"{Notebook.NoteLinkPrefix}b\">", notebook);
        StringAssert.Contains("tu nota: descartado", notebook);
        StringAssert.Contains($"<link=\"{Notebook.NoteLinkPrefix}a\">", notebook, "también a quien aún no tiene nota");
        StringAssert.Contains("añadir nota", notebook, "sin nota todavía: invita a poner una");
        StringAssert.DoesNotContain("tu nota: sin nota", notebook);
        StringAssert.Contains($"<nobr><link=\"{Notebook.NoteLinkPrefix}b\">", notebook, "la nota no se parte en dos líneas (en 20:9 lo hacía)");
    }

    [Test]
    public void LasNotasSeGuardanConLaPartida()
    {
        var data = new SaveData { variantId = "1A" };
        data.suspectNotes.Add("padre:2");
        Assert.IsTrue(SaveSystem.TryDeserialize(SaveSystem.Serialize(data), out SaveData loaded));
        CollectionAssert.AreEqual(new[] { "padre:2" }, loaded.suspectNotes);
    }
}
