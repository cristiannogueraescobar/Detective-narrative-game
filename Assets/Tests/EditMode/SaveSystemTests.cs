using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

public class SaveSystemTests
{
    private string directory;

    [SetUp]
    public void SetUp()
    {
        directory = Path.Combine(Path.GetTempPath(), "detective-save-tests-" + System.Guid.NewGuid().ToString("N"));
        SaveSystem.DirectoryOverride = directory;
    }

    [TearDown]
    public void TearDown()
    {
        SaveSystem.DirectoryOverride = null;
        if (Directory.Exists(directory))
            Directory.Delete(directory, true);
    }

    private static SaveData Sample()
    {
        return new SaveData
        {
            variantId = "1A",
            day = 3,
            questionsUsedToday = 2,
            currentSuspect = "madre",
            unlocked = new List<string> { "padre", "madre", "hermano", "vecina" },
            discovered = new List<string> { "1A_taza", "1A_ventana" },
            shown = new List<SaveData.Shown> { new SaveData.Shown { characterId = "padre", clueId = "1A_ventana" } },
            culpritToldLie = true,
            histories = new List<SaveData.History>
            {
                new SaveData.History
                {
                    characterId = "madre",
                    messages = new List<ChatMessage>
                    {
                        new ChatMessage { role = "user", content = "¿Qué vio?" },
                        new ChatMessage { role = "assistant", content = "Una taza de cacao. [ESTADO: triste]" }
                    }
                }
            },
            emotions = new List<SaveData.EmotionEntry> { new SaveData.EmotionEntry { characterId = "madre", emotion = "Triste" } },
            conversations = new List<SaveData.Conversation> { new SaveData.Conversation { characterId = "madre", text = "TÚ: ¿Qué vio?" } },
            sharedConversation = "DÍA 2"
        };
    }

    [Test]
    public void IdaYVueltaConservaTodo()
    {
        string json = SaveSystem.Serialize(Sample());

        Assert.IsTrue(SaveSystem.TryDeserialize(json, out SaveData loaded));
        Assert.AreEqual("1A", loaded.variantId);
        Assert.AreEqual(3, loaded.day);
        Assert.AreEqual(2, loaded.questionsUsedToday);
        CollectionAssert.AreEqual(Sample().discovered, loaded.discovered);
        Assert.AreEqual("1A_ventana", loaded.shown.Single().clueId);
        Assert.IsTrue(loaded.culpritToldLie);
        Assert.AreEqual("Una taza de cacao. [ESTADO: triste]", loaded.histories[0].messages[1].content);
        Assert.AreEqual("Triste", loaded.emotions[0].emotion);
        Assert.AreEqual("DÍA 2", loaded.sharedConversation);
    }

    [TestCase("")]
    [TestCase("{ esto no es json")]
    [TestCase("{\"variantId\": \"1A\", \"day\": ")]
    [TestCase("null")]
    [TestCase("[1,2,3]")]
    public void GuardadoCorruptoSeRechazaSinExcepcion(string json)
    {
        Assert.DoesNotThrow(() => SaveSystem.TryDeserialize(json, out _));
        Assert.IsFalse(SaveSystem.TryDeserialize(json, out _));
    }

    [Test]
    public void VarianteDesconocidaSeRechaza()
    {
        SaveData data = Sample();
        data.variantId = "9Z";

        Assert.IsFalse(SaveSystem.TryDeserialize(SaveSystem.Serialize(data), out _));
    }

    [Test]
    public void PistaOPersonajeQueNoExistenSeRechazan()
    {
        SaveData badClue = Sample();
        badClue.discovered.Add("1A_inventada");
        Assert.IsFalse(SaveSystem.TryDeserialize(SaveSystem.Serialize(badClue), out _));

        SaveData badCharacter = Sample();
        badCharacter.unlocked.Add("fantasma");
        Assert.IsFalse(SaveSystem.TryDeserialize(SaveSystem.Serialize(badCharacter), out _));
    }

    [Test]
    public void DiaFueraDeRangoSeRechaza()
    {
        SaveData data = Sample();
        data.day = 42;

        Assert.IsFalse(SaveSystem.TryDeserialize(SaveSystem.Serialize(data), out _));
    }

    [Test]
    public void VersionAntiguaSeRechaza()
    {
        SaveData data = Sample();
        data.version = SaveData.CurrentVersion - 1;

        Assert.IsFalse(SaveSystem.TryDeserialize(SaveSystem.Serialize(data), out _));
    }

    [Test]
    public void GuardarCargarYBorrarEnDisco()
    {
        Assert.IsFalse(SaveSystem.Exists);
        Assert.IsFalse(SaveSystem.TryLoad(out _));

        SaveSystem.Save(Sample());
        Assert.IsTrue(SaveSystem.Exists);
        Assert.IsTrue(SaveSystem.TryLoad(out SaveData loaded));
        Assert.AreEqual("madre", loaded.currentSuspect);

        SaveSystem.Delete();
        Assert.IsFalse(SaveSystem.Exists);
    }

    [Test]
    public void ArchivoCorruptoEnDiscoNoLanzaYSeIgnora()
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(SaveSystem.FilePath, "\u0000\u0001basura");

        Assert.DoesNotThrow(() => SaveSystem.TryLoad(out _));
        Assert.IsFalse(SaveSystem.TryLoad(out _));
    }

    [Test]
    public void RestaurarEstadoRecalculaContradicciones()
    {
        Assert.IsTrue(CaseLibrary.TryFind("1A", out _, out VariantData variant));

        InvestigationState state = SaveSystem.RestoreState(variant, Sample());

        CollectionAssert.AreEquivalent(new[] { "1A_taza", "1A_ventana" }, state.DiscoveredClueIds);
        CollectionAssert.AreEqual(new[] { "1A_ventana" }, state.ShownTo("padre"));
        CollectionAssert.AreEqual(new[] { "1A_ventana" }, state.ContradictionClueIds);
    }

    [Test]
    public void AlmacenDeConversacionesSeExportaEImporta()
    {
        var store = new ConversationStore();
        store.AppendToAll("DÍA 2 ");
        store.Select("madre");
        store.Append("madre", "M1 ");

        var copy = new ConversationStore();
        copy.Import(store.Export(), store.SharedText);
        copy.Select("vecina");

        Assert.AreEqual("DÍA 2 M1 ", copy.TextOf("madre"));
        Assert.AreEqual("DÍA 2 ", copy.CurrentText);
    }
}
