using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Comprobador de coherencia narrativa de una variante (A1). Reglas que miran varias fichas a la vez (las de una
/// sola pista o un solo personaje están en CaseDataValidationTests):
///   - la mentira del culpable la contradice al menos una pista que porta un inocente;
///   - las pistas de descarte descartan a alguien del reparto que no es el culpable;
///   - los partes de la mañana no regalan pistas ni señalan al culpable;
///   - el epílogo no trae horas que no estén en ninguna ficha;
///   - cada persona tiene una sola edad en toda la historia;
///   - la línea temporal (si la variante la tiene): nadie en dos sitios a la vez, y las horas de las pistas están
///     en ella.
/// </summary>
public static class NarrativeValidator
{
    public const string LieUncontradicted = "mentira-sin-contradecir";
    public const string ClearsUnknown = "descarta-a-nadie";
    public const string ClearsCulprit = "descarta-al-culpable";
    public const string ReportGivesClue = "parte-regala-pista";
    public const string ReportSpoils = "parte-destripa";
    public const string EpilogueTime = "epilogo-hora-nueva";
    public const string AgeMismatch = "edad-distinta";
    public const string TwoPlaces = "dos-sitios-a-la-vez";
    public const string TimeOffTimeline = "hora-fuera-de-la-linea-temporal";

    public struct Issue
    {
        public string variant;
        public string rule;
        public string detail;
        public override string ToString() => $"{variant} [{rule}] {detail}";
    }

    private static readonly Regex Sentence = new Regex(@"[^.!?\n]+");
    private static readonly Regex Age = new Regex(@"\b(\d{1,3}) años\b");
    private static readonly Regex Accusing = new Regex(@"\b(apunta a|culpable|mintió|miente|asesin|lo hizo|la mató|lo mató)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static List<Issue> Validate(StoryData story, VariantData v)
    {
        var issues = new List<Issue>();
        void Add(string rule, string detail) => issues.Add(new Issue { variant = v.id, rule = rule, detail = detail });

        // La mentira del culpable: al menos una pista que la expone, incriminatoria y en manos de otro
        if (!v.clues.Any(c => c.exposesLie && c.kind == ClueKind.Incriminates && c.holder != v.culpritId))
            Add(LieUncontradicted, $"ninguna pista de un inocente contradice «{v.Role(v.culpritId).lieQuote}»");

        // Descartes
        foreach (ClueData c in v.clues.Where(c => c.kind == ClueKind.Clears))
        {
            if (story.cast.All(ch => ch.id != c.clears))
                Add(ClearsUnknown, $"{c.id} descarta a «{c.clears}», que no está en el reparto");
            else if (c.clears == v.culpritId)
                Add(ClearsCulprit, $"{c.id} descarta al culpable");
        }

        // Partes de la mañana (desde el día 2): ni pistas gratis ni señalar al culpable
        CharacterData culprit = story.cast.First(ch => ch.id == v.culpritId);
        for (int day = 2; day <= v.morningReports.Length; day++)
        {
            string report = v.morningReports[day - 1] ?? "";
            if (report.Length == 0)
                continue;
            string normalized = ClueDetector.Normalize(report);
            foreach (ClueData c in v.clues)
            {
                if (ClueDetector.Evaluate(c.anchors, normalized).Matched)
                    Add(ReportGivesClue, $"día {day}: el parte destapa {c.id} sin preguntar («{report}»)");
            }
            foreach (Match s in Sentence.Matches(report))
            {
                bool namesCulprit = Mentions(s.Value, culprit);
                if (namesCulprit && Accusing.IsMatch(s.Value))
                    Add(ReportSpoils, $"día {day}: «{s.Value.Trim()}»");
            }
        }

        // Epílogo: sus horas tienen que estar en alguna parte del caso
        foreach (string t in TimeCheck.Unknown(v.epilogue, string.Join("\n", AllTexts(story, v, includeEpilogue: false))))
            Add(EpilogueTime, $"el epílogo dice {t}, que no aparece en ninguna ficha, pista ni parte");

        // Edades: una por persona en toda la historia (todas las variantes)
        var ages = new Dictionary<string, HashSet<string>>();
        foreach (string text in StoryTexts(story))
        {
            foreach (Match s in Sentence.Matches(text))
            {
                MatchCollection found = Age.Matches(s.Value);
                if (found.Count != 1)
                    continue;
                var who = story.cast.Where(ch => Mentions(s.Value, ch)).ToList();
                string person = who.Count == 1 ? who[0].id
                              : who.Count == 0 && !string.IsNullOrEmpty(story.victim) && Regex.IsMatch(s.Value, $@"\b{Regex.Escape(story.victim)}\b") ? "victima"
                              : null;
                if (person == null)
                    continue;
                if (!ages.TryGetValue(person, out HashSet<string> set))
                    ages[person] = set = new HashSet<string>();
                set.Add(found[0].Groups[1].Value);
            }
        }
        foreach (var pair in ages.Where(p => p.Value.Count > 1))
            Add(AgeMismatch, $"{pair.Key}: {string.Join(" / ", pair.Value)} años según la ficha");

        // Línea temporal
        if (v.timeline != null && v.timeline.Count > 0)
        {
            foreach (var group in v.timeline.Where(e => !e.approximate).GroupBy(e => (e.who, e.time)))
            {
                var places = group.Select(e => e.where).Distinct().ToList();
                if (places.Count > 1)
                    Add(TwoPlaces, $"{group.Key.who} a las {group.Key.time} está en {string.Join(" y en ", places)}");
            }
            string timelineText = string.Join("\n", v.timeline.Select(e => e.time));
            foreach (ClueData c in v.clues)
                foreach (string t in TimeCheck.Unknown(c.fact, timelineText))
                    Add(TimeOffTimeline, $"{c.id} dice {t}, que no está en la línea temporal");
        }

        return issues;
    }

    private static bool Mentions(string text, CharacterData character)
    {
        return new[] { character.shortName, character.name }.Where(n => !string.IsNullOrEmpty(n))
            .Any(n => Regex.IsMatch(text, $@"\b{Regex.Escape(n)}\b"));
    }

    // Todo el texto de una variante que el jugador o los personajes pueden decir
    public static IEnumerable<string> AllTexts(StoryData story, VariantData v, bool includeEpilogue)
    {
        yield return story.intro;
        yield return story.place;
        yield return story.victimSummary;
        yield return story.situation;
        yield return story.caseBrief;
        foreach (CharacterData ch in story.cast)
            yield return ch.identity;
        foreach (CharacterRole r in v.roles)
        {
            yield return r.version;
            yield return r.secret;
            yield return r.versionB;
            yield return r.doesNotKnow;
            foreach (string k in r.knowledge ?? new string[0])
                yield return k;
        }
        foreach (ClueData c in v.clues)
            yield return c.fact;
        foreach (string report in v.morningReports ?? new string[0])
            yield return report;
        if (includeEpilogue)
            yield return v.epilogue;
    }

    private static IEnumerable<string> StoryTexts(StoryData story)
    {
        foreach (VariantData v in story.variants)
            foreach (string t in AllTexts(story, v, includeEpilogue: true))
                if (!string.IsNullOrEmpty(t))
                    yield return t;
    }
}
