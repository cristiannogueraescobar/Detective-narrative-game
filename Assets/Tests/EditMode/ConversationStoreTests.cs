using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class ConversationStoreTests
{
    private static ChatEntry Q(string text) => ChatEntry.Player(text, null, "10:00");
    private static ChatEntry A(string text) => ChatEntry.Suspect("X", text, "10:00");

    private static string[] Texts(IEnumerable<ChatEntry> entries) => entries.Select(e => e.text).ToArray();

    [Test]
    public void CadaSospechosoTieneSuConversacion()
    {
        var store = new ConversationStore();
        store.Select("padre");
        store.Append("padre", A("P1"));
        store.Select("madre");
        store.Append("madre", A("M1"));

        CollectionAssert.AreEqual(new[] { "M1" }, Texts(store.CurrentEntries));
        store.Select("padre");
        CollectionAssert.AreEqual(new[] { "P1" }, Texts(store.CurrentEntries));
    }

    [Test]
    public void RespuestaDeOtroSospechosoNoSeMezclaConElActual()
    {
        var store = new ConversationStore();
        store.Select("madre");
        store.Append("padre", A("P1"));

        Assert.IsEmpty(store.CurrentEntries);
        CollectionAssert.AreEqual(new[] { "P1" }, Texts(store.EntriesOf("padre")));
    }

    [Test]
    public void AvisoVaAlActual()
    {
        var store = new ConversationStore();
        store.Select("padre");
        store.AppendToCurrent(ChatEntry.System(ChatEntryKind.Unlock, "NUEVO SOSPECHOSO"));

        CollectionAssert.AreEqual(new[] { "NUEVO SOSPECHOSO" }, Texts(store.EntriesOf("padre")));
        Assert.IsEmpty(store.EntriesOf("madre"));
    }

    [Test]
    public void CambioDeDiaSeAnotaEnTodasIncluidasLasQueAunNoEmpezaron()
    {
        var store = new ConversationStore();
        store.Select("padre");
        store.Append("padre", A("P1"));
        store.AppendToAll(ChatEntry.Day(2, "Parte"));
        store.Select("vecina");

        CollectionAssert.AreEqual(new[] { "P1", "Parte" }, Texts(store.EntriesOf("padre")));
        CollectionAssert.AreEqual(new[] { "Parte" }, Texts(store.CurrentEntries));
    }

    [Test]
    public void ReseleccionarNoDuplica()
    {
        var store = new ConversationStore();
        store.Select("padre");
        store.Append("padre", A("P1"));
        store.Select("padre");
        store.Select("padre");

        Assert.AreEqual(1, store.CurrentEntries.Count);
    }

    [Test]
    public void LimpiarVaciaTodo()
    {
        var store = new ConversationStore();
        store.Select("padre");
        store.Append("padre", A("P1"));
        store.Clear();

        Assert.IsNull(store.CurrentId);
        Assert.IsEmpty(store.EntriesOf("padre"));
    }

    [Test]
    public void PreguntaFallidaSeRetiraSinTocarElResto()
    {
        var store = new ConversationStore();
        store.Select("padre");
        store.Append("padre", A("P1"));
        ChatEntry pending = Q("¿Y el cacao?");
        store.Append("padre", pending);

        Assert.IsTrue(store.Remove("padre", pending));
        CollectionAssert.AreEqual(new[] { "P1" }, Texts(store.CurrentEntries));
        Assert.IsFalse(store.Remove("padre", pending), "retirar dos veces no hace nada");
    }

    [Test]
    public void SinSospechosoElegidoLosAvisosSeVenIgualmente()
    {
        var store = new ConversationStore();
        store.AppendToCurrent(ChatEntry.System(ChatEntryKind.Error, "Sin conexión"));

        CollectionAssert.AreEqual(new[] { "Sin conexión" }, Texts(store.CurrentEntries));
    }

    [Test]
    public void ExportarEImportarConservaEntradasYDias()
    {
        var store = new ConversationStore();
        store.AppendToAll(ChatEntry.Day(2, "Parte"));
        store.Select("madre");
        store.Append("madre", Q("¿Qué vio?"));
        store.Append("madre", A("Nada."));

        var copy = new ConversationStore();
        copy.Import(store.Export(), store.SharedEntries.ToList());
        copy.Select("vecina");

        CollectionAssert.AreEqual(new[] { "Parte", "¿Qué vio?", "Nada." }, Texts(copy.EntriesOf("madre")));
        CollectionAssert.AreEqual(new[] { "Parte" }, Texts(copy.CurrentEntries));
        Assert.AreEqual(ChatEntryKind.Player, copy.EntriesOf("madre")[1].kind);
    }

    [Test]
    public void ImportarConvierteElTextoAntiguoEnUnBloque()
    {
        var store = new ConversationStore();
        store.ImportLegacy(new Dictionary<string, string> { { "madre", "TÚ: ¿Qué vio?\n\nMADRE: Nada." } }, "DÍA 2");
        store.Select("madre");

        Assert.AreEqual(ChatEntryKind.Legacy, store.CurrentEntries.Single().kind);
        StringAssert.Contains("Nada.", store.CurrentEntries.Single().text);
        Assert.AreEqual("DÍA 2", store.SharedEntries.Single().text);
    }
}
