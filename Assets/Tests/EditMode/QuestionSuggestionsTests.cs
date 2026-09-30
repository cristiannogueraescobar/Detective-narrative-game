using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class QuestionSuggestionsTests
{
    [Test]
    public void SeOfrecenMientrasNoSeHaPreguntadoNada()
    {
        var soloParte = new List<ChatEntry> { ChatEntry.Day(1, "Lo que se sabe…") };
        Assert.IsTrue(QuestionSuggestions.ShouldShow(soloParte));
        Assert.IsTrue(QuestionSuggestions.ShouldShow(new List<ChatEntry>()));
    }

    [Test]
    public void DesaparecenConLaPrimeraPregunta()
    {
        var entries = new List<ChatEntry> { ChatEntry.Day(1, "…"), ChatEntry.Player("¿Dónde estabas?", null, "09:00") };
        Assert.IsFalse(QuestionSuggestions.ShouldShow(entries));
        Assert.IsFalse(QuestionSuggestions.ShouldShow(null));
    }

    [Test]
    public void NombranALaVictima()
    {
        string[] all = QuestionSuggestions.For("Sofía");
        Assert.AreEqual(3, all.Length);
        Assert.IsTrue(all.Any(q => q.Contains("Sofía")));
        Assert.IsTrue(all.All(q => q.StartsWith("¿") && q.EndsWith("?")));
    }

    [Test]
    public void SinVictimaNoQuedaHueco()
    {
        foreach (string q in QuestionSuggestions.For(null))
        {
            StringAssert.DoesNotContain("{", q);
            StringAssert.DoesNotContain("  ", q);
        }
    }
}
