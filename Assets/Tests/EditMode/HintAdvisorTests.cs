using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class HintAdvisorTests
{
    // Historia de prueba: culpable "a"; i1, i2, d, ctx los tiene "b"; i3 y x (⚡) los tiene "c" (bloqueada al empezar)
    private StoryData story;
    private InvestigationState state;
    private HintMemory memory;

    [SetUp]
    public void SetUp()
    {
        story = TestCases.Story();
        state = new InvestigationState(story.variants[0]);
        memory = new HintMemory();
    }

    private Hint Next(params string[] unlocked)
    {
        return HintAdvisor.Next(story, state, unlocked, memory);
    }

    [Test]
    public void ApuntaAUnaPistaSinEncontrarDeAlguienDisponible()
    {
        Hint hint = Next("a", "b");
        Assert.IsNotNull(hint.clueId);
        Assert.AreEqual("b", story.variants[0].Clue(hint.clueId).holder, "c aún no está disponible");
        Assert.AreEqual("b", hint.holderId, "la ayuda dice a quién preguntar");
        Assert.IsFalse(state.IsDiscovered(hint.clueId));
    }

    [Test]
    public void PrimeroUnaIdeaVagaYLuegoLaPreguntaConcreta()
    {
        Hint first = Next("a", "b");
        Assert.AreEqual(1, first.level);
        StringAssert.Contains("Bea", first.text);
        Assert.IsNull(first.question, "el nivel 1 no da la pregunta");

        Hint second = Next("a", "b");
        Assert.AreEqual(first.clueId, second.clueId, "insistir sobre la misma pista la concreta");
        Assert.AreEqual(2, second.level);
        StringAssert.StartsWith("¿", second.question);
        StringAssert.Contains(second.question, second.text);

        Hint third = Next("a", "b");
        Assert.AreNotEqual(first.clueId, third.clueId, "después pasa a otra pista");
        Assert.AreEqual(1, third.level);
    }

    [Test]
    public void NoEmpiezaPorLaPistaDecisiva()
    {
        Hint hint = Next("a", "b", "c");
        Assert.IsFalse(story.variants[0].Clue(hint.clueId).exposesLie);
    }

    // Sesión C, mentiras de inocentes (fuga 3): "Pensar" dejaba al culpable para el final, y a quien nunca te mandaba
    // era el culpable. El orden de las pistas sugeridas no puede depender de quién lo es
    [Test]
    public void ElOrdenNoDependeDeQuienEsElCulpable()
    {
        List<string> Sequence(string culprit)
        {
            StoryData s = TestCases.Story();
            s.variants[0].culpritId = culprit;
            s.variants[0].Clue("ctx").holder = "a"; // Que el culpable "a" tenga una pista que se pueda sugerir
            var st = new InvestigationState(s.variants[0]);
            var memory = new HintMemory();
            var ids = new List<string>();
            for (int i = 0; i < 12; i++)
            {
                Hint h = HintAdvisor.Next(s, st, new[] { "a", "b", "c" }, memory);
                if (h.clueId == null)
                    break;
                if (h.level == 2)
                {
                    ids.Add(h.clueId);
                    st.Discover(h.clueId);
                }
            }
            return ids;
        }

        CollectionAssert.AreEqual(Sequence("a"), Sequence("b"));
        CollectionAssert.AreEqual(Sequence("a"), Sequence("c"));
    }

    [Test]
    public void LaDecisivaLlegaCuandoNoQuedanOtras()
    {
        foreach (string id in new[] { "i1", "i2", "i3", "d", "ctx" })
            state.Discover(id);
        Assert.AreEqual("x", Next("a", "b", "c").clueId);
    }

    [Test]
    public void SiTodoEstaEncontradoSugiereAcusar()
    {
        foreach (ClueData c in story.variants[0].clues)
            state.Discover(c.id);
        Hint hint = Next("a", "b", "c");
        Assert.IsNull(hint.clueId);
        StringAssert.Contains("acusar", hint.text);
    }

    [Test]
    public void SiLoQueFaltaLoSabeAlguienQueAunNoConocesLoDice()
    {
        foreach (string id in new[] { "i1", "i2", "d", "ctx" })
            state.Discover(id);
        Hint hint = Next("a", "b"); // Lo que falta lo tiene "c", aún bloqueada
        Assert.IsNull(hint.clueId);
        StringAssert.Contains("aún no", hint.text);
    }

    [Test]
    public void LaPreguntaSugeridaEsLaPrimeraPartedeUnaPreguntaEncadenada()
    {
        story.variants[0].Clue("i1").calibrationQuestions = new[] { "¿Qué vio al salir? || ¿Y después?" };
        memory.given["i1"] = 1;
        Hint hint = HintAdvisor.Next(story, state, new[] { "a", "b" }, memory);
        Assert.AreEqual("i1", hint.clueId);
        Assert.AreEqual("¿Qué vio al salir?", hint.question);
    }

    // Revisión D3 n.º 4: solo cuentan (y bajan el rango) las ayudas que apuntan a una pista
    [Test]
    public void SinPistaALaQueApuntarNoCuentaComoAyuda()
    {
        foreach (string id in new[] { "i1", "i2", "i3", "d", "ctx", "x" })
            state.Discover(id);
        foreach (ClueData c in story.variants[0].clues)
            state.Discover(c.id);

        Hint hint = Next("a", "b", "c");

        Assert.IsNull(hint.clueId);
        Assert.AreEqual(0, memory.count);
    }

    [Test]
    public void LaMemoriaDeAyudasSobreviveAlGuardado()
    {
        Hint first = Next("a", "b");
        HintMemory restored = HintMemory.FromSave(memory.count, memory.ToSave());

        Hint second = HintAdvisor.Next(story, state, new[] { "a", "b" }, restored);

        Assert.AreEqual(first.clueId, second.clueId);
        Assert.AreEqual(2, second.level, "tras Continuar, la siguiente es la concreta, no otra vez la vaga");
        Assert.AreEqual(2, restored.count);
    }
}
