using System.Collections.Generic;

/// <summary>
/// Nota del jugador sobre un sospechoso: su propio razonamiento, no el del juego (a quién tacha, de quién sospecha).
/// </summary>
public enum SuspectNote
{
    Ninguna = 0,
    Sospechoso = 1,
    Descartado = 2
}

/// <summary>
/// Notas del jugador en la libreta (idea de Golden Idol y de las libretas de deducción: el jugador va tachando y el
/// caso se estrecha). Tocar la nota la cambia: sin nota → sospechoso → descartado → sin nota. Se guardan con la
/// partida ("id:n") y la rueda de reconocimiento atenúa a los descartados.
/// </summary>
public static class SuspectNotes
{
    public static SuspectNote Next(SuspectNote note)
    {
        return note == SuspectNote.Ninguna ? SuspectNote.Sospechoso
             : note == SuspectNote.Sospechoso ? SuspectNote.Descartado
             : SuspectNote.Ninguna;
    }

    public static string Label(SuspectNote note)
    {
        return note == SuspectNote.Sospechoso ? "sospechoso" : note == SuspectNote.Descartado ? "descartado" : "sin nota";
    }

    public static List<string> ToSave(IReadOnlyDictionary<string, SuspectNote> notes)
    {
        var list = new List<string>();
        if (notes == null)
            return list;
        foreach (KeyValuePair<string, SuspectNote> pair in notes)
        {
            if (pair.Value != SuspectNote.Ninguna)
                list.Add(pair.Key + ":" + (int)pair.Value);
        }
        return list;
    }

    public static Dictionary<string, SuspectNote> FromSave(IEnumerable<string> entries)
    {
        var notes = new Dictionary<string, SuspectNote>();
        if (entries == null)
            return notes;
        foreach (string entry in entries)
        {
            int colon = entry != null ? entry.LastIndexOf(':') : -1;
            if (colon > 0 && int.TryParse(entry.Substring(colon + 1), out int value) && value >= 1 && value <= 2)
                notes[entry.Substring(0, colon)] = (SuspectNote)value;
        }
        return notes;
    }
}
