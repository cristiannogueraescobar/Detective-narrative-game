using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Modelo con guion para jugar de verdad sin Ollama: devuelve, en orden, las respuestas que se le encolan.
/// </summary>
public class ScriptedProvider : ILLMProvider
{
    public readonly Queue<string> answers = new Queue<string>();
    public int calls;
    public string DisplayName => "Guion";

    public async Task<LLMResult> SendAsync(string systemPrompt, IReadOnlyList<ChatMessage> history, int maxTokens, float temperature)
    {
        calls++;
        await Task.Delay(100);
        return LLMResult.Ok(answers.Count > 0 ? answers.Dequeue() : "No sé nada más. [ESTADO: tranquilo]");
    }

    public Task WarmUpAsync() => Task.CompletedTask;
}

/// <summary>
/// Partida jugada por el flujo normal (sesión C, tarjeta "TUS PRUEBAS"): se elige al sospechoso en el desplegable, se
/// escribe la pregunta y se pulsa Enviar. Lo único con guion es el modelo, que contesta con la frase real de cada pista
/// (sus sampleHits, los que el detector reconoce por contrato). La contradicción sale como la saca un jugador: enseñando
/// al culpable, con "Mostrar prueba", la pista que choca con su versión. Pistas, libreta, desplegable de pruebas y
/// textos de la acusación son los del juego.
/// </summary>
public static class ScriptedPlay
{
    public static GameObject Find(string name)
    {
        return Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == name && g.scene.IsValid());
    }

    private static IEnumerator Tap(string name)
    {
        GameObject go = Find(name);
        Assert.IsNotNull(go, name);
        ExecuteEvents.Execute(go, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
        yield return null;
        yield return null;
    }

    private static bool CanAsk => Find("AskButton").GetComponent<Button>().interactable;

    /// <summary>Empieza la historia 'storyIndex' (0-2) con el modelo con guion.</summary>
    public static IEnumerator Start(int storyIndex, ScriptedProvider provider)
    {
        Tutorial.SkipAll();
        yield return Tap("PlayButton");
        yield return Tap("Caso " + CaseLibrary.Stories[storyIndex].id);
        yield return Tap("StartButton");
        Object.FindFirstObjectByType<AIConversationManager>().UseProvider(provider);
        yield return new WaitForSecondsRealtime(0.5f);
    }

    private static List<CharacterData> Unlocked(StoryData story)
    {
        var dropdown = Find("SuspectDropdown").GetComponent<TMP_Dropdown>();
        return dropdown.options.Select(o => story.cast.FirstOrDefault(c => SuspectView.From(c).displayName == o.text))
            .Where(c => c != null).ToList();
    }

    private static IEnumerator Ask(ScriptedProvider provider, CharacterData who, string question, string answer, ClueData showing = null)
    {
        var story = Object.FindFirstObjectByType<AIConversationManager>().Story;
        var dropdown = Find("SuspectDropdown").GetComponent<TMP_Dropdown>();
        int index = Unlocked(story).IndexOf(who);
        Assert.GreaterOrEqual(index, 0, who.id + " desbloqueado");
        if (dropdown.value != index)
        {
            dropdown.value = index;
            yield return new WaitForSecondsRealtime(0.3f);
        }
        // El desplegable de "Mostrar prueba" (su primera opción es "Mostrar prueba: ninguna")
        var evidence = Object.FindObjectsByType<TMP_Dropdown>(FindObjectsSortMode.None)
            .First(d => d.options.Count > 0 && d.options[0].text.StartsWith("Mostrar prueba"));
        evidence.value = showing == null ? 0 : evidence.options.FindIndex(o => o.text.Contains(showing.playerName));
        Assert.GreaterOrEqual(evidence.value, 0, "la prueba está en el desplegable");

        provider.answers.Enqueue(answer);
        int before = provider.calls;
        Find("QuestionInput").GetComponent<TMP_InputField>().text = question;
        yield return Tap("AskButton");
        float end = Time.realtimeSinceStartup + 10f;
        while (!(provider.calls > before && CanAsk))
        {
            Assert.Less(Time.realtimeSinceStartup, end, "respuesta a " + question);
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.4f);
    }

    /// <summary>
    /// ¿Se pueden conseguir así el primer día? (la variante sale al azar: el culpable o los portadores pueden estar aún
    /// bloqueados)
    /// </summary>
    public static bool Feasible(int clueCount, bool contradiction)
    {
        var manager = Object.FindFirstObjectByType<AIConversationManager>();
        List<CharacterData> unlocked = Unlocked(manager.Story);
        string culprit = manager.State.Variant.culpritId;
        var reachable = manager.State.Variant.clues.Where(c => unlocked.Any(u => u.id != culprit && c.HeldBy(u.id))).ToList();
        if (reachable.Count < clueCount)
            return false;
        return !contradiction || (unlocked.Any(u => u.id == culprit) && reachable.Any(c => c.exposesLie));
    }

    /// <summary>
    /// Consigue 'clueCount' pistas preguntando a quien las tiene (de los desbloqueados) y, si se pide, una contradicción
    /// enseñando al culpable la primera que destapa su mentira. Devuelve las pistas conseguidas.
    /// </summary>
    public static IEnumerator GatherEvidence(ScriptedProvider provider, int clueCount, bool contradiction, List<ClueData> found)
    {
        var manager = Object.FindFirstObjectByType<AIConversationManager>();
        StoryData story = manager.Story;
        InvestigationState state = manager.State;
        List<CharacterData> unlocked = Unlocked(story);
        CharacterData culprit = story.Character(state.Variant.culpritId);

        // Pistas al alcance: algún portador desbloqueado que no sea el culpable (con la que destapa la mentira, primero)
        var reachable = new List<(ClueData clue, CharacterData holder)>();
        foreach (ClueData clue in state.Variant.clues.OrderByDescending(c => contradiction && c.exposesLie))
        {
            CharacterData holder = unlocked.FirstOrDefault(c => c.id != culprit.id && clue.HeldBy(c.id));
            if (holder != null)
                reachable.Add((clue, holder));
        }
        Assert.GreaterOrEqual(reachable.Count, clueCount, "pistas al alcance el primer día");

        foreach (var (clue, holder) in reachable.Take(clueCount))
        {
            ClueData view = clue.ForHolder(holder.id);
            yield return Ask(provider, holder, view.calibrationQuestions[0].Split('|')[0].Trim(), view.sampleHits[0] + " [ESTADO: nervioso]");
            Assert.IsTrue(state.IsDiscovered(clue.id), $"la pista {clue.id} sale por el flujo normal");
            found.Add(clue);
        }

        if (contradiction)
        {
            ClueData lie = found.FirstOrDefault(c => c.exposesLie);
            Assert.IsNotNull(lie, "una pista que destapa la mentira");
            Assert.IsTrue(unlocked.Contains(culprit), "el culpable está desbloqueado");
            yield return Ask(provider, culprit, "¿Y esto, cómo lo explica?", "Eso no demuestra nada. [ESTADO: nervioso]", lie);
            Assert.IsTrue(state.ContradictionClueIds.Contains(lie.id), "la contradicción sale enseñando la prueba");
        }
    }
}
