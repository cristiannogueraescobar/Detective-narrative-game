using System.Linq;
using NUnit.Framework;

public class PlaythroughChecksTests
{
    private const string Sheet = "Eres Carmen Vidal. A las 22:00 te tomaste tu pastilla. Rosario vive enfrente. Daniel es tu marido.";

    private static PlaythroughChecks.Kind[] Kinds(string response, bool culprit = false, int contradictions = 0, int shown = 0, string previous = null)
    {
        return PlaythroughChecks.Check(response, Sheet, culprit, contradictions, shown, previous).Select(f => f.kind).ToArray();
    }

    [Test]
    public void RespuestaNormalNoMarcaNada()
    {
        CollectionAssert.IsEmpty(Kinds("Me tomé la pastilla a las 22:00 y me dormí. Daniel se quedó abajo con Rosario, creo."));
    }

    [TestCase("Como IA, no puedo responder a eso.")]
    [TestCase("Soy un modelo de lenguaje y no tengo recuerdos.")]
    [TestCase("Lo siento, no puedo ayudarte con eso.")]
    public void RupturaDePersonaje(string response)
    {
        CollectionAssert.Contains(Kinds(response), PlaythroughChecks.Kind.AiBreak);
    }

    [Test]
    public void HoraQueNoEstaEnLaFicha()
    {
        CollectionAssert.Contains(Kinds("Llegué a casa a las 23:40, más o menos."), PlaythroughChecks.Kind.InventedTime);
        CollectionAssert.DoesNotContain(Kinds("A las 22.00 ya estaba dormida."), PlaythroughChecks.Kind.InventedTime);
    }

    [Test]
    public void NombreQueNoEstaEnLaFicha()
    {
        CollectionAssert.Contains(Kinds("Eso se lo contó a su amiga Verónica, pregúntele a ella."), PlaythroughChecks.Kind.InventedName);
        CollectionAssert.DoesNotContain(Kinds("Eso lo sabe Rosario, que vive enfrente."), PlaythroughChecks.Kind.InventedName);
        CollectionAssert.DoesNotContain(Kinds("Mire, inspector. Dios sabe que la quería."), PlaythroughChecks.Kind.InventedName);
    }

    [Test]
    public void ConfesionDelCulpableSinMotivo()
    {
        CollectionAssert.Contains(Kinds("Está bien, lo hice yo.", culprit: true), PlaythroughChecks.Kind.UnmotivatedConfession);
        CollectionAssert.DoesNotContain(Kinds("Está bien, lo hice yo.", culprit: true, contradictions: 1), PlaythroughChecks.Kind.UnmotivatedConfession);
    }

    [Test]
    public void ConfesionDeUnInocente()
    {
        CollectionAssert.Contains(Kinds("Fui yo, yo la maté."), PlaythroughChecks.Kind.FalseConfession);
    }

    [Test]
    public void NegarNoEsConfesar()
    {
        CollectionAssert.IsEmpty(Kinds("¡No! Yo no la maté, se lo juro por mis hijos.", culprit: true));
    }

    [Test]
    public void Incoherencias()
    {
        CollectionAssert.Contains(Kinds("…"), PlaythroughChecks.Kind.Incoherent);
        CollectionAssert.Contains(Kinds("I am sorry, what do you mean with this question?"), PlaythroughChecks.Kind.Incoherent);
        CollectionAssert.Contains(Kinds("Estaba en casa. [ESTADO: tranquilo]"), PlaythroughChecks.Kind.Incoherent);
        string repeated = "Estaba en casa toda la noche, ya se lo he dicho.";
        CollectionAssert.Contains(Kinds(repeated, previous: repeated), PlaythroughChecks.Kind.Incoherent);
    }
}
