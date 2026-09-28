using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

/// <summary>
/// Reglas que toda variante registrada debe cumplir para ser jugable y terminable con el final BUENO.
/// </summary>
public class CaseDataValidationTests
{
    // Ficha base (sin pruebas mostradas); el spec pide 250-350 palabras, con margen para la historia 3
    private const int MaxPromptWords = 400;

    private static IEnumerable<TestCaseData> Variants()
    {
        return CaseLibrary.AllVariants().Select(pair => new TestCaseData(pair.story.id, pair.variant.id).SetName($"Variante_{pair.variant.id}"));
    }

    private static (StoryData story, VariantData variant) Get(string variantId)
    {
        Assert.IsTrue(CaseLibrary.TryFind(variantId, out StoryData story, out VariantData variant), variantId);
        return (story, variant);
    }

    [Test]
    public void HayAlMenosUnaHistoriaConTresVariantes()
    {
        Assert.IsTrue(CaseLibrary.Stories.Count > 0);
        Assert.IsTrue(CaseLibrary.Stories.All(s => s.variants.Count == 3));
        Assert.IsFalse(CaseLibrary.TryFind("9Z", out _, out _));
    }

    [TestCaseSource(nameof(Variants))]
    public void EstructuraDePistas(string storyId, string variantId)
    {
        var (_, v) = Get(variantId);

        Assert.That(v.clues.Count, Is.InRange(4, 6), "4-6 pistas");
        Assert.IsTrue(v.clues.Any(c => c.exposesLie), "al menos una pista que exponga la mentira");
        Assert.IsTrue(v.clues.Any(c => c.kind == ClueKind.Clears), "al menos una pista de descarte");
        Assert.AreEqual(v.clues.Count, v.clues.Select(c => c.id).Distinct().Count(), "ids únicos");
        Assert.IsTrue(v.clues.All(c => c.id.StartsWith(variantId + "_")), "ids con prefijo de variante");
        Assert.AreEqual(v.clues.Count, v.clues.Select(c => c.playerName).Distinct().Count(), "nombres visibles únicos");
    }

    [TestCaseSource(nameof(Variants))]
    public void PortadoresYRolesEnElElenco(string storyId, string variantId)
    {
        var (story, v) = Get(variantId);
        var castIds = story.cast.Select(c => c.id).ToList();

        Assert.AreEqual(4, story.cast.Count, "elenco de 4");
        CollectionAssert.AreEquivalent(castIds, v.roles.Select(r => r.characterId), "un rol por personaje");
        CollectionAssert.Contains(castIds, v.culpritId);

        foreach (ClueData clue in v.clues)
        {
            CollectionAssert.Contains(castIds, clue.holder, clue.id);
            if (clue.kind == ClueKind.Clears)
            {
                CollectionAssert.Contains(castIds, clue.clears, clue.id);
                Assert.AreNotEqual(v.culpritId, clue.clears, $"{clue.id} no puede descartar al culpable");
            }
            if (clue.exposesLie)
                Assert.AreNotEqual(v.culpritId, clue.holder, $"{clue.id}: el culpable no puede exponer su propia mentira");
        }

        CharacterRole culprit = v.Role(v.culpritId);
        Assert.IsNotEmpty(culprit.lieQuote);
        Assert.IsNotEmpty(culprit.versionB);
        Assert.IsTrue(culprit.lieAnchors != null && culprit.lieAnchors.Length > 0);
    }

    [TestCaseSource(nameof(Variants))]
    public void FinalBuenoAlcanzableSinElCulpable(string storyId, string variantId)
    {
        var (_, v) = Get(variantId);

        Assert.GreaterOrEqual(InvestigationState.MaxEvidenceWithoutCulprit(v), InvestigationState.GoodThreshold);
    }

    [TestCaseSource(nameof(Variants))]
    public void AnclasYEjemplosDeCadaPista(string storyId, string variantId)
    {
        var (_, v) = Get(variantId);

        foreach (ClueData clue in v.clues)
        {
            Assert.IsNotEmpty(clue.playerName, clue.id);
            Assert.IsNotEmpty(clue.summary, clue.id);
            Assert.IsNotEmpty(clue.topic, clue.id);
            Assert.IsNotEmpty(clue.fact, clue.id);
            Assert.GreaterOrEqual(clue.anchors.Length, 2, $"{clue.id}: al menos 2 grupos de anclas");
            Assert.IsTrue(clue.anchors.All(g => g.Length > 0), $"{clue.id}: grupos no vacíos");
            Assert.GreaterOrEqual(clue.calibrationQuestions.Length, 2, $"{clue.id}: preguntas de calibración");
            Assert.GreaterOrEqual(clue.sampleHits.Length, 2, $"{clue.id}: ejemplos positivos");
            Assert.GreaterOrEqual(clue.sampleMisses.Length, 1, $"{clue.id}: ejemplos negativos");

            Assert.IsTrue(ClueDetector.Evaluate(clue.anchors, ClueDetector.Normalize(clue.fact)).Matched,
                $"{clue.id}: el propio hecho debe detectarse");

            foreach (string hit in clue.sampleHits)
                Assert.IsTrue(ClueDetector.Evaluate(clue.anchors, ClueDetector.Normalize(hit)).Matched,
                    $"{clue.id} debería detectar: {hit}");

            foreach (string miss in clue.sampleMisses)
                Assert.IsFalse(ClueDetector.Evaluate(clue.anchors, ClueDetector.Normalize(miss)).Matched,
                    $"{clue.id} NO debería detectar: {miss}");
        }
    }

    [TestCaseSource(nameof(Variants))]
    public void NegacionesDeCadaPistaNoSeDetectan(string storyId, string variantId)
    {
        var (_, v) = Get(variantId);

        foreach (ClueData clue in v.clues)
        {
            string denial = $"No, no {clue.anchors[0][0]} ni {clue.anchors[1][0]}.";
            Assert.IsFalse(ClueDetector.Evaluate(clue.anchors, ClueDetector.Normalize(denial)).Matched,
                $"{clue.id} no debería detectar la negación: {denial}");
        }
    }

    [TestCaseSource(nameof(Variants))]
    public void LaFichaDelPortadorNoDisparaSusPropiasPistas(string storyId, string variantId)
    {
        var (story, v) = Get(variantId);

        foreach (CharacterData character in story.cast)
        {
            CharacterRole role = v.Role(character.id);
            // Todo lo que el personaje dirá con naturalidad, salvo los hechos de sus pistas
            var ownText = new List<string> { character.identity, character.speechExample, role.version, role.secret, role.nervousAbout, role.ifAccused };
            ownText.AddRange(role.knowledge);
            string normalized = ClueDetector.Normalize(string.Join(" ", ownText));

            foreach (ClueData clue in v.clues.Where(c => c.holder == character.id))
            {
                AnchorTrace trace = ClueDetector.Evaluate(clue.anchors, normalized);
                Assert.IsFalse(trace.Matched, $"{clue.id} se dispara con la propia ficha de {character.id}: {trace}");
            }
        }
    }

    [TestCaseSource(nameof(Variants))]
    public void LaMentiraDelCulpableSeDetectaEnSuVersion(string storyId, string variantId)
    {
        var (_, v) = Get(variantId);
        CharacterRole culprit = v.Role(v.culpritId);

        Assert.IsTrue(ClueDetector.Evaluate(culprit.lieAnchors, ClueDetector.Normalize(culprit.version), negationGuard: false).Matched,
            "la versión del culpable debe contener su mentira");
    }

    [TestCaseSource(nameof(Variants))]
    public void PartesDeLaMananaYEpilogo(string storyId, string variantId)
    {
        var (_, v) = Get(variantId);

        Assert.AreEqual(7, v.morningReports.Length);
        Assert.IsTrue(v.morningReports.Skip(1).All(r => !string.IsNullOrWhiteSpace(r)), "partes de los días 2-7");
        Assert.IsNotEmpty(v.epilogue);
    }

    [TestCaseSource(nameof(Variants))]
    public void PersonajesBloqueadosSonMencionados(string storyId, string variantId)
    {
        var (story, v) = Get(variantId);
        var unlocked = story.cast.Where(c => c.startsUnlocked).Select(c => c.id).ToList();

        foreach (CharacterData locked in story.cast.Where(c => !c.startsUnlocked))
        {
            bool mentioned = v.roles
                .Where(r => unlocked.Contains(r.characterId))
                .SelectMany(r => r.knowledge)
                .Any(line => ClueDetector.MentionsAny(ClueDetector.Normalize(line), locked.mentionAliases));

            Assert.IsTrue(mentioned, $"{locked.id} debe ser mencionado por algún personaje desbloqueado");
        }
    }

    [TestCaseSource(nameof(Variants))]
    public void FichasDentroDelLimiteDePalabras(string storyId, string variantId)
    {
        var (story, v) = Get(variantId);

        foreach (CharacterData character in story.cast)
        {
            string prompt = PromptBuilder.Build(story, v, character.id, 1, new ClueData[0], new ClueData[0]);
            int words = prompt.Split(new[] { ' ', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries).Length;

            Assert.LessOrEqual(words, MaxPromptWords, $"{character.id}: {words} palabras");
        }
    }
}
