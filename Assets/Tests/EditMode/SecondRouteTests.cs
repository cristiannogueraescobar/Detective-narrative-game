using System.Linq;
using NUnit.Framework;

/// <summary>
/// Sesión B, bloque 3: las dos pistas más débiles dependían solo de que Daniel confesara (1B_cena 33 %, 1C_llamada 56 %).
/// Cada una tiene ahora una segunda vía lógica: otro portador o un parte de la mañana que da la palanca, y Daniel admite
/// su secreto cuando el inspector la usa.
/// </summary>
public class SecondRouteTests
{
    private static (StoryData story, VariantData v) Get(string variantId)
    {
        Assert.IsTrue(CaseLibrary.TryFind(variantId, out StoryData story, out VariantData v), variantId);
        return (story, v);
    }

    // 1B ya tiene 6 pistas (el diseño aprobado es 5-6): la segunda vía no es una pista nueva sino un testigo. Amparo
    // cuenta lo que vio y Daniel confiesa la aventura cuando el inspector se lo echa en cara.
    [Test]
    public void AmparoVioConQuienVolvioDaniel()
    {
        var (_, v) = Get("1B");
        string seen = ClueDetector.Normalize(string.Join(" ", v.Role("vecina").knowledge));
        StringAssert.Contains("mujer", seen, "otro personaje sabe que no venía de una cena de clientes");
        StringAssert.Contains("mujer", v.Role("padre").admitsWhen, "Daniel confiesa la aventura si se lo echan en cara");
    }

    [Test]
    public void LaCalibracionDe1BPruebaLaSegundaVia()
    {
        var (_, v) = Get("1B");
        ClueData cena = v.clues.First(c => c.id == "1B_cena");
        Assert.IsTrue(cena.calibrationQuestions.Any(q => q.Contains("mujer")), "una pregunta usa lo que vio Amparo");
        Assert.LessOrEqual(v.clues.Count, 6, "sin pistas de más");
    }

    [Test]
    public void LaLlamadaDe1CTieneUnParteQueApuntaADaniel()
    {
        var (_, v) = Get("1C");
        string report = ClueDetector.Normalize(v.morningReports[3]); // Día 4
        StringAssert.Contains("daniel", report, "el parte del día 4 dice a quién preguntar");
        StringAssert.Contains("llamada", report);
        StringAssert.Contains("llamada", v.Role("padre").admitsWhen, "y Daniel cede si el inspector se lo dice");
    }
}
