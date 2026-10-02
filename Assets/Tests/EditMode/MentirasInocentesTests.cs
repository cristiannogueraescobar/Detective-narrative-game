using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

/// <summary>
/// Fase 1 de DISENO-MENTIRAS-INOCENTES.md (decisiones de Cristian, 02-10-2026): los inocentes también mienten, sobre su
/// propio secreto. Una contradicción demuestra que alguien miente, no que sea culpable.
/// Caso sintético (TestCases): culpable "a" (miente con «estuve en casa»; la ⚡ "x" la rompe). Aquí "b" (Bea, inocente)
/// miente sobre su secreto, el tabaco, y la pista de descarte "d" rompe su mentira.
/// </summary>
public class MentirasInocentesTests
{
    private static StoryData StoryWithInnocentLiar()
    {
        StoryData story = TestCases.Story();
        VariantData v = story.variants[0];
        CharacterRole bea = v.Role("b");
        bea.lieQuote = "trabajé toda la tarde";
        bea.lieAnchors = new[] { new[] { "trabaje toda la tarde" } };
        bea.lieAbout = "el tabaco";
        bea.versionB = "Vale, salí un rato a fumar; no quería que se supiera.";
        ClueData d = v.Clue("d");
        d.exposesLie = true;
        d.exposesLieOf = "b";
        return story;
    }

    // ---------- Contradicciones ----------

    [Test]
    public void LaMentiraDeUnInocenteContadaYRotaEsUnaContradiccion()
    {
        StoryData story = StoryWithInnocentLiar();
        var state = new InvestigationState(story.variants[0]);
        state.RegisterLieTold("b");
        state.Discover("d");
        List<ClueData> created = state.UpdateContradictions();
        CollectionAssert.AreEqual(new[] { "d" }, created.Select(c => c.id).ToArray());
        Assert.IsTrue(state.LieTold("b"));
        Assert.IsFalse(state.CulpritToldLie, "la del culpable sigue sin contarse");
    }

    // Fuga 1: enseñar una prueba ya no reacciona solo con el culpable
    [Test]
    public void EnsenarLaPruebaAlInocenteQueMienteTambienLeContradice()
    {
        StoryData story = StoryWithInnocentLiar();
        var state = new InvestigationState(story.variants[0]);
        state.Discover("d");
        state.Discover("x");
        state.RegisterShown("b", "d");
        state.RegisterShown("b", "x");
        CollectionAssert.AreEqual(new[] { "d" }, state.UpdateContradictions().Select(c => c.id).ToArray(),
            "su pista le contradice; la ⚡ del culpable, enseñada a Bea, no");
    }

    [Test]
    public void LaDelCulpableSigueIgual()
    {
        StoryData story = StoryWithInnocentLiar();
        var state = new InvestigationState(story.variants[0]);
        state.Discover("x");
        state.RegisterLieTold(); // Sin quién: el culpable (partidas y llamadas de antes)
        CollectionAssert.AreEqual(new[] { "x" }, state.UpdateContradictions().Select(c => c.id).ToArray());
        Assert.IsTrue(state.CulpritToldLie);
    }

    // Mismo formato para todas, con quien miente y su cita
    [Test]
    public void ElTextoNombraAQuienMienteConSuCita()
    {
        StoryData story = StoryWithInnocentLiar();
        VariantData v = story.variants[0];
        Assert.AreEqual("La versión de Bea («trabajé toda la tarde») choca con: Nombre d", Contradictions.Describe(story, v, v.Clue("d")));
        Assert.AreEqual("La versión de Ana («estuve en casa») choca con: Nombre x", Contradictions.Describe(story, v, v.Clue("x")));
    }

    // ---------- Puntuación ----------

    [Test]
    public void SoloCuentanLasContradiccionesDelCulpable()
    {
        StoryData story = StoryWithInnocentLiar();
        var state = new InvestigationState(story.variants[0]);
        state.RegisterLieTold("b");
        state.Discover("d");
        state.UpdateContradictions();
        Assert.AreEqual(1, state.ContradictionClueIds.Count, "la libreta la enseña");
        Assert.AreEqual(0, state.CulpritContradictions, "no puntúa");
        Assert.AreEqual(state.IncriminatingFound, state.Evidence);

        state.Discover("x");
        state.RegisterLieTold();
        state.UpdateContradictions();
        Assert.AreEqual(1, state.CulpritContradictions);
        Assert.AreEqual(state.IncriminatingFound + InvestigationState.ContradictionWeight, state.Evidence);
        Assert.AreEqual(1, state.Accuse("a").contradictions, "el informe cuenta las que puntúan");
    }

    [Test]
    public void LaEvidenciaMaximaNoCuentaLasMentirasDeInocentes()
    {
        StoryData story = StoryWithInnocentLiar();
        VariantData v = story.variants[0];
        int incriminating = v.clues.Count(c => c.kind == ClueKind.Incriminates && c.Holders.Any(h => h != v.culpritId));
        Assert.AreEqual(incriminating + InvestigationState.ContradictionWeight * 1, InvestigationState.MaxEvidenceWithoutCulprit(v),
            "solo la ⚡ del culpable (x); la de Bea (d) no");
    }

    [Test]
    public void AcusarAlInocenteQueMintioEsFinalMaloYDiceSobreQueMentia()
    {
        StoryData story = StoryWithInnocentLiar();
        var state = new InvestigationState(story.variants[0]);
        AccusationResult result = state.Accuse("b");
        Assert.AreEqual(Ending.Bad, result.ending);
        Assert.AreEqual("el tabaco", result.innocentLieAbout);
        Assert.IsNull(state.Accuse("c").innocentLieAbout, "Carla no miente: sin línea");

        string report = EndingReport.Build(result, "Bea (hija)", "Ana (madre)", 5, "Lo hizo A.", ThemeManagerTheme());
        StringAssert.Contains("Bea mentía, sí: sobre el tabaco, no sobre el crimen.", report);
    }

    // ---------- Análisis del turno ----------

    [Test]
    public void SeDetectaLaMentiraDelInocenteEnSuRespuesta()
    {
        StoryData story = StoryWithInnocentLiar();
        var state = new InvestigationState(story.variants[0]);
        TurnOutcome outcome = TurnAnalyzer.Analyze(story, state, "b", "Trabajé toda la tarde en la tienda, inspector.", null);
        Assert.IsTrue(outcome.lieTold);
        Assert.IsTrue(state.LieTold("b"));
        Assert.IsFalse(state.CulpritToldLie);
    }

    // ---------- Ficha ----------

    [Test]
    public void ElInocenteQueMienteSabeQueHacerSiLeEnsenanLaPrueba()
    {
        StoryData story = StoryWithInnocentLiar();
        string prompt = PromptBuilder.Build(story, story.variants[0], "b", 1, null, null);
        StringAssert.Contains("SI EL INSPECTOR TE MUESTRA UNA PRUEBA QUE CONTRADICE TU VERSIÓN: Vale, salí un rato a fumar", prompt);
        StringAssert.DoesNotContain("ERES EL CULPABLE", prompt);
        StringAssert.Contains("Eres inocente del crimen", prompt);
    }

    // ---------- Guardado ----------

    [Test]
    public void SeGuardaQuienHaMentidoYUnaPartidaAntiguaSeSigueLeyendo()
    {
        StoryData story = StoryWithInnocentLiar();
        VariantData v = story.variants[0];
        var data = new SaveData { discovered = new List<string> { "d" }, liesTold = new List<string> { "b" } };
        InvestigationState restored = SaveSystem.RestoreState(v, data);
        Assert.IsTrue(restored.LieTold("b"));
        CollectionAssert.Contains(restored.ContradictionClueIds.ToList(), "d");

        var old = new SaveData { discovered = new List<string> { "x" }, culpritToldLie = true };
        InvestigationState fromOld = SaveSystem.RestoreState(v, old);
        Assert.IsTrue(fromOld.CulpritToldLie, "una partida guardada antes solo sabía de la mentira del culpable");
        CollectionAssert.Contains(fromOld.ContradictionClueIds.ToList(), "x");
    }

    // ---------- Decisión de Cristian (fase 1): la mentira cuenta en cuanto su versión está en la libreta ----------

    [Test]
    public void LaVersionEstaEnLaLibretaEnCuantoContestaSalvoSiNombraAAlguienAunNoDisponible()
    {
        StoryData story = StoryWithInnocentLiar();
        CharacterRole bea = story.variants[0].Role("b");
        Assert.IsFalse(HeardVersions.Heard(story, bea, new string[0], new[] { "a", "b" }), "aún no ha contestado");
        Assert.IsTrue(HeardVersions.Heard(story, bea, new[] { "b" }, new[] { "a", "b" }));
        bea.version = "Trabajé toda la tarde con Carla.";
        Assert.IsFalse(HeardVersions.Heard(story, bea, new[] { "b" }, new[] { "a", "b" }), "nombra a Carla, que aún no ha aparecido");
        Assert.IsTrue(HeardVersions.Heard(story, bea, new[] { "b" }, new[] { "a", "b", "c" }));
    }

    [Test]
    public void LaMentiraDelInocenteCuentaEnCuantoSuVersionEstaEnLaLibreta()
    {
        StoryData story = StoryWithInnocentLiar();
        var state = new InvestigationState(story.variants[0]);
        state.Discover("d");
        Assert.IsEmpty(HeardVersions.RegisterLies(story, state, new string[0], new[] { "a", "b" }), "sin contestar, nada");
        List<ClueData> created = HeardVersions.RegisterLies(story, state, new[] { "b" }, new[] { "a", "b" });
        CollectionAssert.AreEqual(new[] { "d" }, created.Select(c => c.id).ToArray(), "ha contestado y la pista ya está: contradicción");
        Assert.IsTrue(state.LieTold("b"));
        Assert.IsEmpty(HeardVersions.RegisterLies(story, state, new[] { "b" }, new[] { "a", "b" }), "una sola vez");
    }

    [Test]
    public void YLaDelCulpableIgual()
    {
        StoryData story = StoryWithInnocentLiar();
        var state = new InvestigationState(story.variants[0]);
        state.Discover("x");
        CollectionAssert.AreEqual(new[] { "x" }, HeardVersions.RegisterLies(story, state, new[] { "a" }, new[] { "a", "b" }).Select(c => c.id).ToArray());
        Assert.IsTrue(state.CulpritToldLie);
    }

    [Test]
    public void QuienNoMienteNoRegistraNada()
    {
        StoryData story = TestCases.Story(); // Bea no miente aquí
        var state = new InvestigationState(story.variants[0]);
        HeardVersions.RegisterLies(story, state, new[] { "b" }, new[] { "a", "b" });
        Assert.IsFalse(state.LieTold("b"));
        Assert.IsEmpty(state.LiesTold);
    }

    // ---------- Datos de las historias ----------

    private static IEnumerable<(StoryData story, VariantData variant)> All()
    {
        foreach (StoryData story in CaseLibrary.Stories)
            foreach (VariantData variant in story.variants)
                yield return (story, variant);
    }

    private static IEnumerable<CharacterRole> InnocentLiars(VariantData v) =>
        v.roles.Where(r => r.characterId != v.culpritId && !string.IsNullOrEmpty(r.lieQuote));

    [Test]
    public void UnMentirosoInocenteComoMuchoPorVariante()
    {
        foreach (var (_, v) in All())
            Assert.LessOrEqual(InnocentLiars(v).Count(), 1, v.id);
    }

    [Test]
    public void CadaMentiraDeInocenteEstaCompletaYSeDetectaEnSuVersion()
    {
        foreach (var (_, v) in All())
        {
            foreach (CharacterRole liar in InnocentLiars(v))
            {
                string who = $"{v.id} {liar.characterId}";
                Assert.IsNotEmpty(liar.lieAnchors, who + ": anclas de la mentira");
                Assert.IsFalse(string.IsNullOrEmpty(liar.versionB), who + ": qué hace si le enseñan la prueba");
                Assert.IsFalse(string.IsNullOrEmpty(liar.lieAbout), who + ": sobre qué miente (informe)");
                Assert.IsTrue(ClueDetector.Evaluate(liar.lieAnchors, ClueDetector.Normalize(liar.version), negationGuard: false).Matched,
                    who + ": su versión cuenta como su mentira");
                Assert.IsFalse(ClueDetector.Evaluate(liar.lieAnchors, ClueDetector.Normalize(liar.versionB), negationGuard: false).Matched,
                    who + ": admitirlo no cuenta como mentir");
                Assert.IsTrue(v.clues.Any(c => c.exposesLie && c.exposesLieOf == liar.characterId), who + ": alguna pista rompe su mentira");
            }
        }
    }

    // Decisión de Cristian: la pista que rompe la mentira de un inocente nunca le incrimina. En el modelo, "Incriminates"
    // siempre es contra el culpable: la regla es que quien miente no sea el culpable y que, si la pista descarta a alguien,
    // no descarte al culpable (sería un descarte falso)
    [Test]
    public void LaPistaQueRompeLaMentiraDeUnInocenteNuncaLoIncrimina()
    {
        foreach (var (story, v) in All())
        {
            foreach (ClueData clue in v.clues.Where(c => !string.IsNullOrEmpty(c.exposesLieOf)))
            {
                string who = $"{v.id} {clue.id}";
                Assert.IsTrue(clue.exposesLie, who + ": exposesLieOf sin exposesLie");
                Assert.AreNotEqual(v.culpritId, clue.exposesLieOf, who + ": para el culpable, exposesLieOf va vacío");
                Assert.IsTrue(story.cast.Any(c => c.id == clue.exposesLieOf), who);
                Assert.IsFalse(string.IsNullOrEmpty(v.Role(clue.exposesLieOf).lieQuote), who + ": quien miente tiene su cita");
                Assert.IsFalse(clue.kind == ClueKind.Incriminates && v.culpritId == clue.exposesLieOf, who + ": no le incrimina");
                if (clue.kind == ClueKind.Clears)
                    Assert.AreNotEqual(v.culpritId, clue.clears, who);
                // Quien la cuenta no puede ser el culpable si es la que rompe la mentira del inocente con su testimonio
                // (un culpable no "ayuda"): basta con que la tenga algún inocente
                Assert.IsTrue(clue.Holders.Any(h => h != v.culpritId), who + ": la cuenta algún inocente");
            }
        }
    }

    // Fase 1: Daniel miente en 1B (la cena con clientes; su secreto, la aventura) y en 1C (la hora de llegada; su secreto,
    // que vio el golpe y no la llevó al hospital)
    [TestCase("1B", "1B_cena")]
    [TestCase("1C", "1C_llamada")]
    public void EnLaFase1DanielMienteSobreSuSecreto(string variantId, string clueId)
    {
        CaseLibrary.TryFind(variantId, out StoryData story, out VariantData v);
        CollectionAssert.AreEqual(new[] { "padre" }, InnocentLiars(v).Select(r => r.characterId).ToArray());
        ClueData clue = v.Clue(clueId);
        Assert.IsTrue(clue.exposesLie);
        Assert.AreEqual("padre", clue.exposesLieOf);
        StringAssert.StartsWith("La versión de Daniel («", Contradictions.Describe(story, v, clue));
    }

    // Calibración de la fase 1: "hasta cerca de las 23:00. Llegué a casa a esa hora" también es su mentira
    [Test]
    public void LaMentiraDeDanielEn1CSeDetectaComoLaDice()
    {
        CaseLibrary.TryFind("1C", out _, out VariantData v);
        string[][] anchors = v.Role("padre").lieAnchors;
        foreach (string said in new[]
                 {
                     "Estuve en el despacho hasta cerca de las 23:00. Llegué a casa a esa hora y subí a ver a Elena.",
                     "A las 23:00 llegué a casa y subí a ver a Elena."
                 })
            Assert.IsTrue(ClueDetector.Evaluate(anchors, ClueDetector.Normalize(said), negationGuard: false).Matched, said);
    }

    // Decisión de Cristian (fase 1): 1B_receta al 80 %. Daniel la suelta poco y varía mucho entre pasadas; Lucas, que ya
    // sabía lo de la segunda opinión, la cuenta también (segundo portador, como Amparo con 1B_cena)
    [Test]
    public void LaRecetaDe1BLaSabenDanielYLucas()
    {
        CaseLibrary.TryFind("1B", out _, out VariantData v);
        CollectionAssert.AreEquivalent(new[] { "padre", "hermano" }, v.Clue("1B_receta").Holders.ToArray());
    }

    // Calibración de la fase 1: con una frase hecha como reacción, Daniel la repetía tal cual ("estuve con otra persona")
    // en vez de confesar con los detalles de su secreto, y las anclas de su pista no la veían (1B_cena por Daniel: 10 %)
    [TestCase("1B")]
    [TestCase("1C")]
    public void LaReaccionDelInocenteRemiteASuSecretoSinFraseHecha(string variantId)
    {
        CaseLibrary.TryFind(variantId, out _, out VariantData v);
        StringAssert.Contains("lo que ocultas", v.Role("padre").versionB);
    }

    // El resto de variantes, sin mentiroso inocente hasta la fase 2
    [TestCase("1A")]
    [TestCase("2A")]
    [TestCase("2B")]
    [TestCase("2C")]
    [TestCase("3A")]
    [TestCase("3B")]
    [TestCase("3C")]
    public void FueraDeLaFase1NadieMasMiente(string variantId)
    {
        CaseLibrary.TryFind(variantId, out _, out VariantData v);
        Assert.IsEmpty(InnocentLiars(v).Select(r => r.characterId).ToArray());
    }

    private static Theme ThemeManagerTheme() => ThemeManager.Current;
}
