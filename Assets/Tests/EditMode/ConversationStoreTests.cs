using NUnit.Framework;

public class ConversationStoreTests
{
    [Test]
    public void CadaSospechosoTieneSuConversacion()
    {
        var store = new ConversationStore();
        store.Select("padre");
        store.Append("padre", "P1 ");
        store.Select("madre");
        store.Append("madre", "M1 ");

        Assert.AreEqual("M1 ", store.CurrentText);
        store.Select("padre");
        Assert.AreEqual("P1 ", store.CurrentText);
    }

    [Test]
    public void RespuestaDeOtroSospechosoNoSeMezclaConElActual()
    {
        var store = new ConversationStore();
        store.Select("madre");
        store.Append("padre", "P1 ");

        Assert.AreEqual("", store.CurrentText);
        Assert.AreEqual("P1 ", store.TextOf("padre"));
    }

    [Test]
    public void AvisoVaAlActual()
    {
        var store = new ConversationStore();
        store.Select("padre");
        store.AppendToCurrent("✓ NUEVO SOSPECHOSO ");

        Assert.AreEqual("✓ NUEVO SOSPECHOSO ", store.TextOf("padre"));
        Assert.AreEqual("", store.TextOf("madre"));
    }

    [Test]
    public void CambioDeDiaSeAnotaEnTodasIncluidasLasQueAunNoEmpezaron()
    {
        var store = new ConversationStore();
        store.Select("padre");
        store.Append("padre", "P1 ");
        store.AppendToAll("DÍA 2 ");
        store.Select("vecina");

        Assert.AreEqual("P1 DÍA 2 ", store.TextOf("padre"));
        Assert.AreEqual("DÍA 2 ", store.CurrentText);
    }

    [Test]
    public void ReseleccionarNoDuplica()
    {
        var store = new ConversationStore();
        store.Select("padre");
        store.Append("padre", "P1 ");
        store.Select("padre");
        store.Select("padre");

        Assert.AreEqual("P1 ", store.CurrentText);
    }

    [Test]
    public void LimpiarVaciaTodo()
    {
        var store = new ConversationStore();
        store.Select("padre");
        store.Append("padre", "P1 ");
        store.Clear();

        Assert.IsNull(store.CurrentId);
        Assert.AreEqual("", store.TextOf("padre"));
    }
}
