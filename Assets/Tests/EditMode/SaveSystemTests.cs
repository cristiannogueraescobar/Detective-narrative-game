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
            conversations = new List<SaveData.Conversation>
            {
                new SaveData.Conversation
                {
                    characterId = "madre",
                    entries = new List<ChatEntry> { ChatEntry.Player("¿Qué vio?", "La taza", "09:00"), ChatEntry.Suspect("CARMEN", "Una taza.", "09:00") }
                }
            },
            sharedEntries = new List<ChatEntry> { ChatEntry.Day(2, "Parte de la mañana") }
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
        Assert.AreEqual("Parte de la mañana", loaded.sharedEntries.Single().text);
        ChatEntry question = loaded.conversations.Single().entries[0];
        Assert.AreEqual(ChatEntryKind.Player, question.kind);
        Assert.AreEqual("La taza", question.evidence);
        Assert.AreEqual("09:00", question.time);
    }

    [Test]
    public void GuardadoDeLaVersionAnteriorSeCargaConSuTexto()
    {
        SaveData old = Sample();
        old.version = 1;
        old.conversations = new List<SaveData.Conversation> { new SaveData.Conversation { characterId = "madre", text = "TÚ: ¿Qué vio?" } };
        old.sharedEntries = new List<ChatEntry>();
        old.sharedConversation = "DÍA 2";

        Assert.IsTrue(SaveSystem.TryDeserialize(SaveSystem.Serialize(old), out SaveData loaded));

        var store = new ConversationStore();
        SaveSystem.RestoreConversations(loaded, store);
        store.Select("madre");
        Assert.AreEqual(ChatEntryKind.Legacy, store.CurrentEntries.Single().kind);
        StringAssert.Contains("¿Qué vio?", store.CurrentEntries.Single().text);
        Assert.AreEqual("DÍA 2", store.SharedEntries.Single().text);
    }

    [Test]
    public void GuardadoSinDificultadSeJuegaEnDetective()
    {
        // Un guardado de antes de la dificultad no trae el campo
        string json = SaveSystem.Serialize(Sample()).Replace("\"difficulty\":", "\"ignorado\":");
        Assert.IsTrue(SaveSystem.TryDeserialize(json, out SaveData loaded));
        Assert.AreEqual(-1, loaded.difficulty);
        Assert.AreEqual(DifficultyLevel.Detective, Difficulty.FromSave(loaded.difficulty));
    }

    [Test]
    public void LaDificultadYLasAyudasSeGuardan()
    {
        SaveData data = Sample();
        data.difficulty = (int)DifficultyLevel.Veterano;
        data.hintsUsed = 3;
        Assert.IsTrue(SaveSystem.TryDeserialize(SaveSystem.Serialize(data), out SaveData loaded));
        Assert.AreEqual(DifficultyLevel.Veterano, Difficulty.FromSave(loaded.difficulty));
        Assert.AreEqual(3, loaded.hintsUsed);
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
    public void PersonajesRepetidosSeRechazan()
    {
        SaveData data = Sample();
        data.histories.Add(new SaveData.History { characterId = "madre" });

        Assert.IsFalse(SaveSystem.TryDeserialize(SaveSystem.Serialize(data), out _));
    }

    [Test]
    public void DiaFueraDeRangoSeRechaza()
    {
        SaveData data = Sample();
        data.day = 42;

        Assert.IsFalse(SaveSystem.TryDeserialize(SaveSystem.Serialize(data), out _));
    }

    [Test]
    public void VersionesIlegiblesSeRechazan()
    {
        foreach (int version in new[] { 0, SaveData.OldestReadableVersion - 1, SaveData.CurrentVersion + 1 })
        {
            SaveData data = Sample();
            data.version = version;
            Assert.IsFalse(SaveSystem.TryDeserialize(SaveSystem.Serialize(data), out _), $"versión {version}");
        }
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
    public void AlmacenDeConversacionesSeGuardaYSeRecupera()
    {
        var store = new ConversationStore();
        store.AppendToAll(ChatEntry.Day(2, "Parte"));
        store.Select("madre");
        store.Append("madre", ChatEntry.Suspect("CARMEN", "M1", "09:00"));

        SaveData data = Sample();
        SaveSystem.StoreConversations(store, data);
        Assert.IsTrue(SaveSystem.TryDeserialize(SaveSystem.Serialize(data), out SaveData loaded));

        var copy = new ConversationStore();
        SaveSystem.RestoreConversations(loaded, copy);
        copy.Select("vecina");

        CollectionAssert.AreEqual(new[] { "Parte", "M1" }, copy.EntriesOf("madre").Select(e => e.text).ToArray());
        CollectionAssert.AreEqual(new[] { "Parte" }, copy.CurrentEntries.Select(e => e.text).ToArray());
    }

    [Test]
    public void GuardaElNivelDeCadaAyuda()
    {
        SaveData data = Sample();
        data.hintsGiven.Add("1A_puerta:2");

        Assert.IsTrue(SaveSystem.TryDeserialize(SaveSystem.Serialize(data), out SaveData loaded));

        CollectionAssert.AreEqual(new[] { "1A_puerta:2" }, loaded.hintsGiven);
    }

    // PERFORMANCE-AUDIT, mejora 4: guardado compacto y escritura fuera del hilo principal, sin cruzarse

    [Test]
    public void ElGuardadoEsCompacto()
    {
        SaveData data = Sample();
        string json = SaveSystem.Serialize(data);
        StringAssert.DoesNotContain(NewLine, json);
        Assert.Less(json.Length, UnityEngine.JsonUtility.ToJson(data, true).Length * 0.8, "al menos un 20 % menos");
        Assert.IsTrue(SaveSystem.TryDeserialize(json, out SaveData back));
        Assert.AreEqual(data.variantId, back.variantId);
    }

    [Test]
    public void UnGuardadoAntiguoConSangriaSigueCargando()
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(SaveSystem.FilePath, UnityEngine.JsonUtility.ToJson(Sample(), true));
        Assert.IsTrue(SaveSystem.TryLoad(out SaveData data));
        Assert.AreEqual(3, data.day);
    }

    [Test]
    public void VariosGuardadosSeguidosDejanElUltimo()
    {
        // 30 guardados; solo el último es del día 7 (la partida valida días 1-7): si uno antiguo pisara al último, no saldría 7
        for (int i = 1; i <= 30; i++)
        {
            SaveData data = Sample();
            data.day = i < 30 ? 1 + i % 6 : 7;
            SaveSystem.Save(data);
        }
        Assert.IsTrue(SaveSystem.TryLoad(out SaveData loaded), "leer espera a lo pendiente");
        Assert.AreEqual(7, loaded.day, "ninguno escrito fuera de orden");
        Assert.IsFalse(File.Exists(SaveSystem.FilePath + ".tmp"), "sin temporales a medias");
    }

    [Test]
    public void BorrarDespuesDeGuardarNoResucitaLaPartida()
    {
        SaveSystem.Save(Sample());
        SaveSystem.Delete();
        SaveSystem.Flush();
        Assert.IsFalse(SaveSystem.Exists, "una escritura pendiente no puede volver a crear el archivo borrado");
    }

    [Test]
    public void LaEscrituraNoSeHaceEnElHiloQueGuarda()
    {
        int caller = System.Threading.Thread.CurrentThread.ManagedThreadId;
        SaveSystem.Save(Sample());
        SaveSystem.Flush();
        Assert.AreNotEqual(caller, SaveSystem.LastWriterThread, "el disco se toca en otro hilo");
    }

    private static readonly string NewLine = System.Environment.NewLine.Substring(System.Environment.NewLine.Length - 1);
}
