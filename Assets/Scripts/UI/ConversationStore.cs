using System.Collections.Generic;
using System.Text;

/// <summary>
/// Conversaciones por sospechoso: la UI solo muestra la del seleccionado.
/// Las respuestas van a su sospechoso (aunque no esté seleccionado); los avisos, al actual;
/// el cambio de día, a todos (también a los que aún no tienen conversación).
/// </summary>
public class ConversationStore
{
    private readonly Dictionary<string, StringBuilder> bySuspect = new Dictionary<string, StringBuilder>();
    private readonly StringBuilder shared = new StringBuilder(); // Lo que verá quien empiece a hablar más tarde

    public string CurrentId { get; private set; }

    public string CurrentText => CurrentId == null ? "" : TextOf(CurrentId);

    public string TextOf(string suspectId)
    {
        return bySuspect.TryGetValue(suspectId, out StringBuilder sb) ? sb.ToString() : shared.ToString();
    }

    public void Select(string suspectId)
    {
        CurrentId = suspectId;
    }

    public void Append(string suspectId, string entry)
    {
        Get(suspectId).Append(entry);
    }

    public void AppendToCurrent(string entry)
    {
        if (CurrentId != null)
            Append(CurrentId, entry);
    }

    public void AppendToAll(string entry)
    {
        shared.Append(entry);
        foreach (StringBuilder sb in bySuspect.Values)
            sb.Append(entry);
    }

    public string SharedText => shared.ToString();

    /// <summary>
    /// Conversaciones por sospechoso (para guardar la partida).
    /// </summary>
    public Dictionary<string, string> Export()
    {
        var result = new Dictionary<string, string>();
        foreach (var pair in bySuspect)
            result[pair.Key] = pair.Value.ToString();
        return result;
    }

    public void Import(IDictionary<string, string> texts, string sharedText)
    {
        Clear();
        shared.Append(sharedText ?? "");
        foreach (var pair in texts)
            bySuspect[pair.Key] = new StringBuilder(pair.Value ?? "");
    }

    public void Clear()
    {
        bySuspect.Clear();
        shared.Clear();
        CurrentId = null;
    }

    private StringBuilder Get(string suspectId)
    {
        if (!bySuspect.TryGetValue(suspectId, out StringBuilder sb))
        {
            // Quien empieza a hablar hereda los separadores de día ya ocurridos
            sb = new StringBuilder(shared.ToString());
            bySuspect[suspectId] = sb;
        }
        return sb;
    }
}
