using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

/// <summary>
/// Hallazgos de la revisión de código de la noche 2 (cada test reproduce uno).
/// </summary>
public class ReviewFixesTests
{
    private class CountingStore : ISettingsStore
    {
        public readonly Dictionary<string, float> values = new Dictionary<string, float>();
        public int reads;
        public float Get(string key, float fallback) { reads++; return values.TryGetValue(key, out float v) ? v : fallback; }
        public void Set(string key, float value) => values[key] = value;
    }

    [TearDown]
    public void TearDown() => GameSettings.UseStore(null);

    [Test]
    public void LosAjustesSeLeenUnaVezYLuegoDeMemoria()
    {
        var store = new CountingStore();
        GameSettings.UseStore(store);
        for (int i = 0; i < 100; i++)
        {
            bool _ = GameSettings.ReduceMotion;
            bool __ = GameSettings.HighContrast;
        }
        Assert.LessOrEqual(store.reads, 2, "cada ajuste se lee del almacén una sola vez");

        GameSettings.ReduceMotion = true;
        Assert.IsTrue(GameSettings.ReduceMotion, "lo escrito se ve al momento");
        Assert.AreEqual(1f, store.values[GameSettings.ReduceMotionKey], "y se guarda");
    }

    [Test]
    public void GuardadoConConversacionDeAlguienQueNoExisteSeRechaza()
    {
        var data = new SaveData
        {
            variantId = "1A", day = 2,
            conversations = new List<SaveData.Conversation> { new SaveData.Conversation { characterId = null } }
        };
        Assert.IsFalse(SaveSystem.TryDeserialize(SaveSystem.Serialize(data), out _));

        data.conversations[0].characterId = "fantasma";
        Assert.IsFalse(SaveSystem.TryDeserialize(SaveSystem.Serialize(data), out _));
    }

    [Test]
    public void UnaPartidaAntiguaNoEsUnChatVacio()
    {
        var store = new ConversationStore();
        store.ImportLegacy(new Dictionary<string, string> { { "madre", "TÚ: ¿Qué vio?" } }, "");
        Assert.IsFalse(store.IsEmpty, "el día 1 no se añade debajo de una conversación ya empezada");
        Assert.IsTrue(new ConversationStore().IsEmpty);
    }

    [Test]
    public void TrasCargarLosDiasCompartidosSonLaMismaEntrada()
    {
        var source = new ConversationStore();
        source.AppendToAll(ChatEntry.Day(1, "Parte"));
        source.Select("madre");
        source.Append("madre", ChatEntry.Player("hola", null, "09:00"));
        var data = new SaveData { version = SaveData.CurrentVersion };
        SaveSystem.StoreConversations(source, data);
        SaveData loaded = UnityEngine.JsonUtility.FromJson<SaveData>(SaveSystem.Serialize(data));

        var store = new ConversationStore();
        SaveSystem.RestoreConversations(loaded, store);

        Assert.AreSame(store.SharedEntries[0], store.EntriesOf("madre")[0], "así el chat no se rehace entero al cambiar de sospechoso");
    }
}
