using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class NaturalUnlocksTests
{
    private static StoryData StoryWithTrigger()
    {
        StoryData story = TestCases.Story(); // "c" (Carla, vecina) empieza bloqueada
        story.unlockTriggers.Add(new UnlockTrigger
        {
            id = "c",
            playerStems = new[] { "vecin", "ventana" },
            fallbackDay = 2,
            fallbackText = "Una vecina se presenta en comisaría."
        });
        return story;
    }

    [Test]
    public void PreguntarPorElTemaTraeAlPersonaje()
    {
        StoryData story = StoryWithTrigger();
        CollectionAssert.AreEqual(new[] { "c" }, NaturalUnlocks.TriggeredBy(story, new[] { "a", "b" }, "¿Alguna VECINA vio algo por la ventana?").ToArray());
        CollectionAssert.IsEmpty(NaturalUnlocks.TriggeredBy(story, new[] { "a", "b" }, "¿Dónde estaba usted?"));
        CollectionAssert.IsEmpty(NaturalUnlocks.TriggeredBy(story, new[] { "a", "b", "c" }, "¿Y la vecina?"), "si ya está, nada");
    }

    [Test]
    public void SiNadieLaTraeLlegaConElParteDeSuDia()
    {
        StoryData story = StoryWithTrigger();
        CollectionAssert.IsEmpty(NaturalUnlocks.DueOn(story, new[] { "a", "b" }, 1));
        var due = NaturalUnlocks.DueOn(story, new[] { "a", "b" }, 2).ToList();
        Assert.AreEqual("c", due.Single().id);
        Assert.AreEqual("Una vecina se presenta en comisaría.", due.Single().text);
    }

    [Test]
    public void SinDisparadorLlegaElDiaDeSeguridadConUnAvisoGenerico()
    {
        StoryData story = TestCases.Story();
        CollectionAssert.IsEmpty(NaturalUnlocks.DueOn(story, new[] { "a", "b" }, NaturalUnlocks.DefaultDay - 1));
        var due = NaturalUnlocks.DueOn(story, new[] { "a", "b" }, NaturalUnlocks.DefaultDay).Single();
        StringAssert.Contains("Carla Paz", due.text);
    }

    [Test]
    public void LaBaseDeDatosDaProfundidadATodosYDisparadoresALosBloqueados()
    {
        foreach (StoryData story in CaseLibrary.Stories)
        {
            foreach (CharacterData c in story.cast)
            {
                Assert.IsNotEmpty(c.personality, $"{story.id}/{c.id}: carácter");
                Assert.IsNotEmpty(c.tellsLying, $"{story.id}/{c.id}: cómo se le nota al mentir");
                Assert.IsNotEmpty(c.pressureArc, $"{story.id}/{c.id}: progresión bajo presión");
                if (!c.startsUnlocked)
                    Assert.IsTrue(story.unlockTriggers.Any(t => t.id == c.id && t.playerStems.Length > 0 && t.fallbackDay >= 2),
                        $"{story.id}/{c.id}: aparece por un tema de las preguntas y, si no, por un parte");
            }
            foreach (VariantData v in story.variants)
                Assert.IsNotEmpty(v.Role(v.culpritId).lieStrategy, $"{v.id}: cómo sostiene la mentira el culpable");
        }
    }

    [Test]
    public void LaFichaIncluyeElCaracterYComoSostieneLaMentira()
    {
        CaseLibrary.TryFind("1A", out StoryData story, out VariantData v);
        string culprit = PromptBuilder.Build(story, v, v.culpritId, 1, null, null);
        StringAssert.Contains("CARÁCTER:", culprit);
        StringAssert.Contains("CÓMO SOSTIENES LA MENTIRA:", culprit);
        string innocent = PromptBuilder.Build(story, v, "vecina", 1, null, null);
        StringAssert.Contains("BAJO PRESIÓN:", innocent);
        StringAssert.DoesNotContain("CÓMO SOSTIENES LA MENTIRA", innocent);
    }

    // Revisión D3 n.º 7 (medido con el bot: el hermano de la historia 3 aparecía el día 1 en 8 de 12 partidas):
    // las preguntas de ejemplo y las de ambiente no traen a nadie; hay que preguntar por lo que ese personaje sabe
    [Test]
    public void LasPreguntasDeEjemploNoDesbloqueanANadie()
    {
        foreach (StoryData story in CaseLibrary.Stories)
        {
            var start = story.cast.Where(c => c.startsUnlocked).Select(c => c.id).ToArray();
            var questions = QuestionSuggestions.For(story.victim).Concat(new[]
            {
                "¿Qué hiciste esa madrugada?", "¿A qué hora volviste en coche?", "¿Tienes vehículo?"
            });
            foreach (string q in questions)
                CollectionAssert.IsEmpty(NaturalUnlocks.TriggeredBy(story, start, q), $"historia {story.id}: «{q}»");
        }
    }
}
