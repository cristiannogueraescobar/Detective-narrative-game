using NUnit.Framework;

/// <summary>
/// Mentiras del culpable dichas con otras palabras en partidas reales del bot (día 3): deben contar como su mentira.
/// </summary>
public class LieDetectionTests
{
    private static bool Lie(string variantId, string answer)
    {
        CaseLibrary.TryFind(variantId, out _, out VariantData variant);
        return ClueDetector.Evaluate(variant.Role(variant.culpritId).lieAnchors, ClueDetector.Normalize(answer), negationGuard: false).Matched;
    }

    [TestCase("Yo estaba en Granada con Álex toda la jornada del sábado.")]
    [TestCase("Estaba en Granada todo el sábado, en casa de mi hermana.")]
    public void LaMentiraDeLuciaEn3B(string answer)
    {
        Assert.IsTrue(Lie("3B", answer));
    }

    [Test]
    public void DecirQueFueAGranadaSinMasNoEsLaMentira()
    {
        Assert.IsFalse(Lie("3B", "Álex se quedó en Granada con mi hermana."));
    }
}
