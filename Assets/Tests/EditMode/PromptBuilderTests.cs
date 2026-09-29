using System.Linq;
using NUnit.Framework;

public class PromptBuilderTests
{
    private static string Build(string characterId, int day = 1, string[] shown = null, string[] told = null)
    {
        StoryData story = TestCases.Story();
        VariantData v = story.variants[0];
        return PromptBuilder.Build(story, v, characterId, day,
            (shown ?? new string[0]).Select(v.Clue), (told ?? new string[0]).Select(v.Clue));
    }

    [Test]
    public void IncluyeHechosDelPortadorYNoLosDeOtros()
    {
        string prompt = Build("b");

        StringAssert.Contains("Si te preguntan por tema i1: hecho i1 con taza", prompt);
        StringAssert.DoesNotContain("hecho x con cortina", prompt);
        StringAssert.Contains("Eres Bea Gil.", prompt);
        StringAssert.Contains("EL CASO: Alguien murió.", prompt);
        StringAssert.Contains("Ves a Ana a diario.", prompt);
    }

    [Test]
    public void PistaSecretaVaEnLoQueOcultas()
    {
        string prompt = Build("b");
        int secretSection = prompt.IndexOf("LO QUE OCULTAS:");

        Assert.Greater(secretSection, 0);
        Assert.Greater(prompt.IndexOf("hecho d con partida"), secretSection);
        StringAssert.Contains("Lo admites solo si te insisten.", prompt);
    }

    [Test]
    public void PistaSecretaIndicaCuandoYComoConfesarla()
    {
        string prompt = Build("b");

        StringAssert.Contains("- Si te preguntan por tema d: niégalo la primera vez; si el inspector insiste o dice que lo va a comprobar, confiésalo con tus palabras: hecho d con partida", prompt);
    }

    [Test]
    public void HechosAbiertosSeCuentanCompletosYSinProblema()
    {
        string prompt = Build("b");

        StringAssert.Contains("LO CUENTAS SIN PROBLEMA SI TE PREGUNTAN POR EL TEMA, con tus palabras, en primera persona, completo y con la hora:", prompt);
    }

    [Test]
    public void SoloElCulpableRecibeInstruccionesDeCulpable()
    {
        string culprit = Build("a");
        string innocent = Build("b");

        StringAssert.Contains("ERES EL CULPABLE", culprit);
        StringAssert.Contains("Salí un momento.", culprit);
        StringAssert.DoesNotContain("ERES EL CULPABLE", innocent);
        StringAssert.Contains("Eres inocente", innocent);
    }

    [Test]
    public void IncluyeDiaPruebasMostradasYLoYaContado()
    {
        string prompt = Build("b", day: 3, shown: new[] { "x" }, told: new[] { "i1" });

        StringAssert.Contains("HOY ES EL DÍA 3 DE LA INVESTIGACIÓN.", prompt);
        StringAssert.Contains("PRUEBAS QUE YA TE HAN MOSTRADO:", prompt);
        StringAssert.Contains("Resumen x", prompt);
        StringAssert.Contains("YA HAS CONTADO", prompt);
    }

    [Test]
    public void SinPruebasNiHistorialOmiteEsasSecciones()
    {
        string prompt = Build("c");

        StringAssert.DoesNotContain("PRUEBAS QUE YA TE HAN MOSTRADO", prompt);
        StringAssert.DoesNotContain("YA HAS CONTADO", prompt);
        StringAssert.DoesNotContain("LO QUE SABES:", prompt); // c no tiene knowledge
        StringAssert.Contains("REGLAS:", prompt);
    }
}
