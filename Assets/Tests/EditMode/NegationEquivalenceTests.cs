using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

/// <summary>
/// Mejora 3 reescribió IsNegated sin Substring ni Split. En la calibración con 10 intentos, 1A_papeles bajó de 80 % a
/// 60 % en la rama sin cambios en sus datos: este test descarta (o confirma) que el detector decida distinto. Compara
/// la detección actual con la implementación anterior, copiada aquí, para cada ancla de cada variante sobre todos los
/// textos de las historias, las respuestas de ejemplo y frases con negación.
/// </summary>
public class NegationEquivalenceTests
{
    private static readonly HashSet<string> Negations = new HashSet<string>
    {
        "no", "ni", "nunca", "nadie", "ningun", "ninguna", "ninguno", "nada", "tampoco", "jamas"
    };
    private static readonly char[] ClauseBreaks = { '.', ',', ';', ':', '!', '?' };

    // La versión anterior a la mejora 3, tal cual
    private static bool OldIsNegated(string text, int anchorIndex)
    {
        string before = text.Substring(0, anchorIndex);
        string clause = before.Substring(before.LastIndexOfAny(ClauseBreaks) + 1);
        string[] words = clause.Split(new[] { ' ', '¿', '¡' }, System.StringSplitOptions.RemoveEmptyEntries);
        return words.Skip(System.Math.Max(0, words.Length - 4)).Any(Negations.Contains);
    }

    private static bool OldOccurs(string text, string anchor)
    {
        if (anchor.Length == 0)
            return false;
        for (int i = text.IndexOf(anchor, System.StringComparison.Ordinal); i >= 0;
             i = text.IndexOf(anchor, i + 1, System.StringComparison.Ordinal))
        {
            if (!OldIsNegated(text, i))
                return true;
        }
        return false;
    }

    [Test]
    public void ElDetectorDecideIgualQueAntesEnTodosLosTextos()
    {
        var texts = new List<string>
        {
            "No, inspector, yo no vi el coche.", "¿Nunca le dijo nada? No sé, tío, la vi en la cocina.",
            "Nadie, nadie entró. Bueno, Daniel sí entró a las 22:35.", "no", "  no  vi  ", "¡No! ¿Ni siquiera la taza?"
        };
        int compared = 0;
        foreach (StoryData story in CaseLibrary.Stories)
            foreach (VariantData v in story.variants)
            {
                texts.AddRange(NarrativeValidator.AllTexts(story, v, includeEpilogue: true).Where(t => !string.IsNullOrEmpty(t)));
                foreach (ClueData c in v.clues)
                    texts.AddRange((c.sampleHits ?? new string[0]).Concat(c.sampleMisses ?? new string[0]));
            }
        var anchors = CaseLibrary.Stories.SelectMany(s => s.variants).SelectMany(v => v.clues)
            .SelectMany(c => c.anchors).SelectMany(g => g).Distinct().ToList();

        foreach (string raw in texts.Distinct())
        {
            string text = ClueDetector.Normalize(raw);
            foreach (string anchor in anchors)
            {
                string a = ClueDetector.Normalize(anchor);
                bool before = OldOccurs(text, a);
                bool now = ClueDetector.Evaluate(new[] { new[] { anchor } }, text).Matched;
                Assert.AreEqual(before, now, $"«{anchor}» en «{raw}»");
                compared++;
            }
        }
        Assert.Greater(compared, 10000, "se comparó de verdad");
    }
}
