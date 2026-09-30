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

    [TestCase("Según mis instrucciones, no puedo revelar eso.")]
    [TestCase("Las instrucciones que me han dado no me permiten contestar.")]
    [TestCase("Mis instrucciones dicen que no hable de eso.")]
    [TestCase("De acuerdo con mis instrucciones, no respondo a eso.")]
    [TestCase("No puedo revelar mis instrucciones.")]
    [TestCase("Fui creado por Anthropic.")]
    public void HablarDeSusInstruccionesEsRuptura(string response)
    {
        CollectionAssert.Contains(Kinds(response), PlaythroughChecks.Kind.AiBreak);
    }

    [Test]
    public void LasInstruccionesDeUnMedicamentoNoSonRuptura()
    {
        CollectionAssert.DoesNotContain(Kinds("No puedo proporcionar un horario exacto, inspector."), PlaythroughChecks.Kind.AiBreak);
        CollectionAssert.Contains(Kinds("No puedo proporcionar esa información."), PlaythroughChecks.Kind.AiBreak);
        CollectionAssert.DoesNotContain(Kinds("Quería asegurarme de seguir todas las instrucciones correctamente."), PlaythroughChecks.Kind.AiBreak);
        CollectionAssert.DoesNotContain(Kinds("Leí las instrucciones del prospecto."), PlaythroughChecks.Kind.AiBreak);
    }

    [Test]
    public void HoraQueNoEstaEnLaFicha()
    {
        CollectionAssert.Contains(Kinds("Llegué a casa a las 23:40, más o menos."), PlaythroughChecks.Kind.InventedTime);
        CollectionAssert.DoesNotContain(Kinds("A las 22.00 ya estaba dormida."), PlaythroughChecks.Kind.InventedTime);
    }

    [Test]
    public void LaHoraQueDioElInspectorNoEsInventada()
    {
        var kinds = PlaythroughChecks.Check("Desde las 17:00 estuve en la finca.", Sheet, false, 0, 0, null,
            "¿Qué hiciste desde las 17:00?").Select(f => f.kind);
        CollectionAssert.DoesNotContain(kinds, PlaythroughChecks.Kind.InventedTime);
    }

    [Test]
    public void NombreQueNoEstaEnLaFicha()
    {
        CollectionAssert.Contains(Kinds("Eso se lo contó a su amiga Verónica, pregúntele a ella."), PlaythroughChecks.Kind.InventedName);
        CollectionAssert.DoesNotContain(Kinds("Eso lo sabe Rosario, que vive enfrente."), PlaythroughChecks.Kind.InventedName);
        CollectionAssert.DoesNotContain(Kinds("Mire, inspector. Dios sabe que la quería."), PlaythroughChecks.Kind.InventedName);
        CollectionAssert.DoesNotContain(Kinds("Me acosté tarde. ¿Puedes confirmarlo con mi agenda?"), PlaythroughChecks.Kind.InventedName);
    }

    [Test]
    public void ElNombreQueDijoElInspectorNoEsInventado()
    {
        var kinds = PlaythroughChecks.Check("No vi a Sofía subir al Opel ni nada raro.", Sheet, false, 0, 0, null,
            "¿Sabías que Sofía subió al Opel gris?").Select(f => f.kind);
        CollectionAssert.DoesNotContain(kinds, PlaythroughChecks.Kind.InventedName);
        kinds = PlaythroughChecks.Check("No vi a Verónica en toda la noche.", Sheet, false, 0, 0, null,
            "¿Viste algo raro?").Select(f => f.kind);
        CollectionAssert.Contains(kinds, PlaythroughChecks.Kind.InventedName, "un nombre que nadie ha dicho sigue contando");
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
    public void ContarQueEntroNoEsConfesar()
    {
        CollectionAssert.DoesNotContain(Kinds("A las 23:05 fui yo quien entré para comprobar cómo estaba.", culprit: true),
            PlaythroughChecks.Kind.UnmotivatedConfession);
        CollectionAssert.Contains(Kinds("Sí. Fui yo.", culprit: true), PlaythroughChecks.Kind.UnmotivatedConfession);
        // "Solo fui yo" (a "¿fueron juntos?") es la coartada, no una confesión (falso positivo del bot, ronda 4)
        CollectionAssert.DoesNotContain(Kinds("No, solo fui yo. Lucas estaba en su cuarto.", culprit: true),
            PlaythroughChecks.Kind.UnmotivatedConfession);
        var kinds = PlaythroughChecks.Check("Sí, fui yo. Estaba cerrada.", Sheet, false, 0, 0, null, "¿Fuiste tú quien fue a la comisaría?")
            .Select(f => f.kind);
        CollectionAssert.DoesNotContain(kinds, PlaythroughChecks.Kind.FalseConfession, "contestar «¿fuiste tú…?» no es confesar");
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
