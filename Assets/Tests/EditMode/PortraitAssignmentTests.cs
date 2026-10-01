using System.IO;
using System.Linq;
using NUnit.Framework;

/// <summary>
/// Sesión A, bloque 3: con 7 retratos para 12 personajes, cinco compartían cara (el padre de la historia 1 y el de la
/// 3, las tres vecinas…). Cada personaje tiene ahora el suyo; los nuevos salen de los originales por código
/// (Tools/make_derived_portraits.py) en archivos nuevos.
/// </summary>
public class PortraitAssignmentTests
{
    [Test]
    public void NingunPersonajeComparteRetratoConOtro()
    {
        var all = CaseLibrary.Stories.SelectMany(s => s.cast.Select(c => (story: s.id, c.name, c.portraitKey))).ToList();
        var shared = all.GroupBy(c => c.portraitKey).Where(g => g.Count() > 1)
                        .Select(g => g.Key + ": " + string.Join(", ", g.Select(c => c.name))).ToList();
        CollectionAssert.IsEmpty(shared, "retratos compartidos");
    }

    [Test]
    public void CadaRetratoDerivadoTieneSuArchivoYSuEncuadre()
    {
        foreach (var pair in DerivedPortraits.All)
        {
            Assert.IsTrue(File.Exists(pair.Value.path), pair.Value.path);
            CollectionAssert.Contains(PortraitCrops.Keys, pair.Key, pair.Key + ": encuadre del original (en espejo si lo está)");
            CollectionAssert.Contains(PortraitCrops.Keys, pair.Value.baseKey);
        }
    }

    [Test]
    public void ElEncuadreDeUnRetratoEnEspejoEsElDelOriginalReflejado()
    {
        var javier = DerivedPortraits.All["Javier"];
        Assert.IsTrue(javier.mirrored);
        UnityEngine.Rect b = PortraitCrops.Bust("Padre"), m = PortraitCrops.Bust("Javier");
        Assert.AreEqual(1f - b.xMax, m.xMin, 1e-4f);
        Assert.AreEqual(b.yMin, m.yMin, 1e-4f);
        Assert.AreEqual(b.width, m.width, 1e-4f);
    }
}
