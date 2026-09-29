using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

/// <summary>
/// Resolubilidad: un jugador que interroga con sentido debe poder llegar al final BUENO en cada variante
/// dentro del presupuesto de preguntas (7 días × 5), usando el mismo análisis de respuestas que el juego
/// (TurnAnalyzer) y las reglas de desbloqueo (menciones y red de seguridad del día 3).
/// Las respuestas del modelo se sustituyen por textos de los datos: su versión y conocimiento, y el
/// primer ejemplo positivo de cada pista (los secretos cuestan una negación previa).
/// </summary>
public class ResolvabilityTests
{
    private const int QuestionsPerDay = 5;
    private const int MaxQuestions = 7 * QuestionsPerDay;
    private const int SafetyUnlockQuestion = (GameManager.SafetyUnlockDay - 1) * QuestionsPerDay;

    private static IEnumerable<TestCaseData> Variants()
    {
        return CaseLibrary.AllVariants().Select(pair => new TestCaseData(pair.variant.id).SetName($"Resoluble_{pair.variant.id}"));
    }

    private class Playthrough
    {
        public StoryData story;
        public InvestigationState state;
        public HashSet<string> unlocked;
        public int questions;
        public List<string> log = new List<string>();

        public void Ask(string characterId, string response, string label)
        {
            questions++;
            if (questions == SafetyUnlockQuestion + 1)
                UnlockAll("día 3");

            TurnOutcome outcome = TurnAnalyzer.Analyze(story, state, characterId, response, null);
            foreach (string mentioned in outcome.mentionedCharacters)
            {
                if (unlocked.Add(mentioned))
                    log.Add($"  desbloqueado {mentioned} (mención de {characterId})");
            }

            log.Add($"{questions,2}. {characterId}: {label} → {string.Join(", ", outcome.newClues.Select(c => c.id))}" +
                    (outcome.lieTold ? " [mentira]" : "") +
                    string.Concat(outcome.newContradictions.Select(c => $" [contradicción {c.id}]")));
        }

        public void UnlockAll(string reason)
        {
            foreach (CharacterData c in story.cast)
            {
                if (unlocked.Add(c.id))
                    log.Add($"  desbloqueado {c.id} ({reason})");
            }
        }
    }

    [TestCaseSource(nameof(Variants))]
    public void JugadorSensatoLlegaAlFinalBueno(string variantId)
    {
        Assert.IsTrue(CaseLibrary.TryFind(variantId, out StoryData story, out VariantData variant));

        var play = new Playthrough
        {
            story = story,
            state = new InvestigationState(variant),
            unlocked = new HashSet<string>(story.cast.Where(c => c.startsUnlocked).Select(c => c.id))
        };

        var introduced = new HashSet<string>();
        bool progress = true;

        while (progress && play.questions < MaxQuestions)
        {
            progress = false;

            foreach (CharacterData character in story.cast.Where(c => play.unlocked.Contains(c.id)).ToList())
            {
                CharacterRole role = variant.Role(character.id);

                // Primera pregunta a cada personaje: qué hizo y qué sabe
                if (introduced.Add(character.id))
                {
                    play.Ask(character.id, string.Join(" ", role.knowledge.Append(role.version)), "versión");
                    progress = true;
                }

                foreach (ClueData clue in variant.clues.Where(c => c.holder == character.id && !play.state.IsDiscovered(c.id)))
                {
                    if (play.questions >= MaxQuestions)
                        break;

                    if (clue.isSecret)
                        play.Ask(character.id, "No sé de qué me habla, inspector.", $"{clue.id} (niega)");

                    play.Ask(character.id, clue.sampleHits[0], clue.id);
                    progress = true;
                }
            }

            // Si nadie más tiene nada que contar y quedan bloqueados, el jugador espera al día 3
            if (!progress && play.unlocked.Count < story.cast.Count && play.questions < SafetyUnlockQuestion)
            {
                play.questions = SafetyUnlockQuestion;
                play.UnlockAll("día 3 (espera)");
                progress = true;
            }
        }

        string trace = string.Join("\n", play.log);
        Assert.IsTrue(play.unlocked.Contains(variant.culpritId), $"el culpable debe estar desbloqueado para acusarle\n{trace}");
        Assert.LessOrEqual(play.questions, MaxQuestions, trace);

        AccusationResult result = play.state.Accuse(variant.culpritId);
        Assert.AreEqual(Ending.Good, result.ending, $"evidencia {result.evidence}\n{trace}");
        Assert.AreEqual(variant.clues.Count, play.state.DiscoveredClueIds.Count, $"todas las pistas descubiertas\n{trace}");
    }

    [TestCaseSource(nameof(Variants))]
    public void AcusarAUnInocenteNuncaDaFinalBueno(string variantId)
    {
        Assert.IsTrue(CaseLibrary.TryFind(variantId, out StoryData story, out VariantData variant));
        var state = new InvestigationState(variant);
        foreach (ClueData clue in variant.clues)
            state.Discover(clue.id);
        state.RegisterLieTold();
        state.UpdateContradictions();

        foreach (CharacterData innocent in story.cast.Where(c => c.id != variant.culpritId))
            Assert.AreEqual(Ending.Bad, state.Accuse(innocent.id).ending, innocent.id);
    }
}
