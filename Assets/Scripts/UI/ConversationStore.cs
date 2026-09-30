using System.Collections.Generic;

/// <summary>
/// Conversaciones por sospechoso: la UI solo muestra la del seleccionado.
/// Las respuestas van a su sospechoso (aunque no esté seleccionado); los avisos, al actual;
/// el cambio de día, a todos (también a los que aún no tienen conversación).
/// </summary>
public class ConversationStore
{
    private readonly Dictionary<string, List<ChatEntry>> bySuspect = new Dictionary<string, List<ChatEntry>>();
    private readonly List<ChatEntry> shared = new List<ChatEntry>();   // Lo que verá quien empiece a hablar más tarde
    private readonly List<ChatEntry> loose = new List<ChatEntry>();    // Avisos antes de elegir sospechoso

    public string CurrentId { get; private set; }

    public IReadOnlyList<ChatEntry> CurrentEntries => CurrentId == null ? (IReadOnlyList<ChatEntry>)loose : EntriesOf(CurrentId);

    public IReadOnlyList<ChatEntry> SharedEntries => shared;

    public IReadOnlyList<ChatEntry> EntriesOf(string suspectId)
    {
        return suspectId != null && bySuspect.TryGetValue(suspectId, out List<ChatEntry> list) ? list : shared;
    }

    public void Select(string suspectId)
    {
        CurrentId = suspectId;
    }

    public void Append(string suspectId, ChatEntry entry)
    {
        Get(suspectId).Add(entry);
    }

    public void AppendToCurrent(ChatEntry entry)
    {
        if (CurrentId != null)
            Append(CurrentId, entry);
        else
            loose.Add(entry);
    }

    public void AppendToAll(ChatEntry entry)
    {
        shared.Add(entry);
        foreach (List<ChatEntry> list in bySuspect.Values)
            list.Add(entry);
    }

    /// <summary>
    /// Retira una entrada (la pregunta de una petición que falló).
    /// </summary>
    public bool Remove(string suspectId, ChatEntry entry)
    {
        if (suspectId != null && bySuspect.TryGetValue(suspectId, out List<ChatEntry> list))
            return list.Remove(entry);
        return loose.Remove(entry);
    }

    /// <summary>
    /// Conversaciones por sospechoso (para guardar la partida).
    /// </summary>
    public Dictionary<string, List<ChatEntry>> Export()
    {
        var result = new Dictionary<string, List<ChatEntry>>();
        foreach (var pair in bySuspect)
            result[pair.Key] = new List<ChatEntry>(pair.Value);
        return result;
    }

    public void Import(IDictionary<string, List<ChatEntry>> entries, List<ChatEntry> sharedEntries)
    {
        Clear();
        if (sharedEntries != null)
            shared.AddRange(sharedEntries);
        foreach (var pair in entries)
            bySuspect[pair.Key] = new List<ChatEntry>(pair.Value ?? new List<ChatEntry>());
    }

    /// <summary>
    /// Guardados de antes de las burbujas: cada conversación era un texto con formato.
    /// </summary>
    public void ImportLegacy(IDictionary<string, string> texts, string sharedText)
    {
        var entries = new Dictionary<string, List<ChatEntry>>();
        foreach (var pair in texts)
        {
            var list = new List<ChatEntry>();
            if (!string.IsNullOrEmpty(pair.Value))
                list.Add(ChatEntry.System(ChatEntryKind.Legacy, pair.Value));
            entries[pair.Key] = list;
        }

        var sharedList = new List<ChatEntry>();
        if (!string.IsNullOrEmpty(sharedText))
            sharedList.Add(ChatEntry.System(ChatEntryKind.Legacy, sharedText));
        Import(entries, sharedList);
    }

    public void Clear()
    {
        bySuspect.Clear();
        shared.Clear();
        loose.Clear();
        CurrentId = null;
    }

    private List<ChatEntry> Get(string suspectId)
    {
        if (!bySuspect.TryGetValue(suspectId, out List<ChatEntry> list))
        {
            // Quien empieza a hablar hereda los cambios de día ya ocurridos
            list = new List<ChatEntry>(shared);
            bySuspect[suspectId] = list;
        }
        return list;
    }
}
