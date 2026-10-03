using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Partida en curso, en JSON. Las contradicciones no se guardan: se recalculan al restaurar.
/// </summary>
[Serializable]
public class SaveData
{
    public const int CurrentVersion = 2;  // 2: chat en entradas (burbujas); 1: chat en texto
    public const int OldestReadableVersion = 1;

    [Serializable] public class Shown { public string characterId; public string clueId; }
    [Serializable] public class History { public string characterId; public List<ChatMessage> messages = new List<ChatMessage>(); }
    [Serializable] public class EmotionEntry { public string characterId; public string emotion; }
    [Serializable] public class Conversation
    {
        public string characterId;
        public List<ChatEntry> entries = new List<ChatEntry>();
        public string text; // Solo versión 1
    }

    public int version = CurrentVersion;
    public string variantId;
    public int day = 1;
    public int questionsUsedToday;
    public int difficulty = -1; // DifficultyLevel; -1 en los guardados anteriores (= Detective)
    public int hintsUsed;
    public List<string> hintsGiven = new List<string>(); // "pista:nivel" (HintMemory); vacío en guardados anteriores
    public List<string> suspectNotes = new List<string>(); // "id:nota" (SuspectNotes); vacío en guardados anteriores
    public int dayStartClues = -1;          // Pistas y contradicciones al empezar el día ("Ayer: …"); -1 en guardados anteriores
    public int dayStartContradictions = -1;
    public string currentSuspect;
    public List<string> unlocked = new List<string>();
    public List<string> discovered = new List<string>();
    public List<string> discoveredBy = new List<string>(); // "pista:personaje"; vacío en guardados anteriores
    public List<Shown> shown = new List<Shown>();
    public bool culpritToldLie;
    public List<string> liesTold = new List<string>(); // Quién ha contado su mentira (sesión C); vacío en guardados anteriores
    public List<History> histories = new List<History>();
    public List<EmotionEntry> emotions = new List<EmotionEntry>();
    public List<Conversation> conversations = new List<Conversation>();
    public List<ChatEntry> sharedEntries = new List<ChatEntry>();
    public string sharedConversation = ""; // Solo versión 1
}

/// <summary>
/// Guardado y carga de la partida en Application.persistentDataPath/partida.json.
/// Un guardado ilegible o incoherente con los datos del juego se ignora (se empieza de cero), nunca lanza.
/// </summary>
public static class SaveSystem
{
    public const string FileName = "partida.json";
    public const int MaxDays = 7;

    /// <summary>
    /// Carpeta alternativa (tests). null = Application.persistentDataPath.
    /// </summary>
    public static string DirectoryOverride;

    public static string FilePath => Path.Combine(DirectoryOverride ?? Application.persistentDataPath, FileName);

    public static bool Exists
    {
        get
        {
            Flush();
            return File.Exists(FilePath);
        }
    }

    public static string Serialize(SaveData data)
    {
        return JsonUtility.ToJson(data, false); // Compacto: -42 % (las partidas antiguas con sangría se leen igual)
    }

    public static bool TryDeserialize(string json, out SaveData data)
    {
        data = null;
        if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{"))
            return false;

        try
        {
            data = JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception)
        {
            data = null;
            return false;
        }

        if (!IsValid(data))
        {
            data = null;
            return false;
        }

        return true;
    }

    /// <summary>
    /// El guardado debe encajar con los datos actuales del juego (una actualización pudo cambiarlos).
    /// </summary>
    private static bool IsValid(SaveData data)
    {
        if (data == null || data.version < SaveData.OldestReadableVersion || data.version > SaveData.CurrentVersion)
            return false;
        if (!CaseLibrary.TryFind(data.variantId, out StoryData story, out VariantData variant))
            return false;
        if (data.day < 1 || data.day > MaxDays || data.questionsUsedToday < 0)
            return false;

        var characters = new HashSet<string>(story.cast.Select(c => c.id));
        var clues = new HashSet<string>(variant.clues.Select(c => c.id));

        bool Known(string id) => id != null && characters.Contains(id);
        bool Unique<T>(List<T> list, System.Func<T, string> key) => list == null || list.Select(key).Distinct().Count() == list.Count;

        if (!Unique(data.histories, h => h.characterId) || !Unique(data.emotions, e => e.characterId) ||
            !Unique(data.conversations, c => c.characterId))
            return false;

        return (data.unlocked ?? new List<string>()).All(Known)
            && (data.discovered ?? new List<string>()).All(clues.Contains)
            && (data.shown ?? new List<SaveData.Shown>()).All(s => Known(s.characterId) && clues.Contains(s.clueId))
            && (data.histories ?? new List<SaveData.History>()).All(h => Known(h.characterId))
            && (data.conversations ?? new List<SaveData.Conversation>()).All(c => Known(c.characterId))
            && (data.emotions ?? new List<SaveData.EmotionEntry>()).All(e => Known(e.characterId) && Enum.TryParse(e.emotion, out Emotion _))
            && (data.currentSuspect == null || data.currentSuspect.Length == 0 || Known(data.currentSuspect));
    }

    /// <summary>
    /// Copia las conversaciones del chat a la partida.
    /// </summary>
    public static void StoreConversations(ConversationStore store, SaveData data)
    {
        data.conversations = new List<SaveData.Conversation>();
        foreach (var pair in store.Export())
            data.conversations.Add(new SaveData.Conversation { characterId = pair.Key, entries = pair.Value });
        data.sharedEntries = new List<ChatEntry>(store.SharedEntries);
        data.sharedConversation = "";
    }

    /// <summary>
    /// Recupera las conversaciones del chat (las de la versión 1 como bloques de texto).
    /// </summary>
    public static void RestoreConversations(SaveData data, ConversationStore store)
    {
        var conversations = data.conversations ?? new List<SaveData.Conversation>();
        if (data.version < 2)
        {
            store.ImportLegacy(conversations.ToDictionary(c => c.characterId, c => c.text), data.sharedConversation);
            return;
        }
        store.Import(conversations.ToDictionary(c => c.characterId, c => c.entries ?? new List<ChatEntry>()),
            data.sharedEntries ?? new List<ChatEntry>());
    }

    // PERFORMANCE-AUDIT, mejora 4: se guardaba ~40 veces por caso, síncrono en el hilo principal y con sangría
    // (41,6 KB frente a 24,3). Ahora el JSON se hace aquí (los datos no pueden cambiar mientras se leen) y el disco se
    // toca en otro hilo, en una cola de uno: las escrituras van en orden, una antigua nunca pisa a una nueva (número de
    // versión) y Delete, TryLoad y Exists esperan a lo pendiente (un borrado no lo resucita una escritura atrasada).
    private static readonly object WriteLock = new object();
    private static System.Threading.Tasks.Task pending = System.Threading.Tasks.Task.CompletedTask;
    private static int version;

    /// <summary>Hilo que hizo la última escritura en disco: para los tests.</summary>
    public static int LastWriterThread { get; private set; }

    // Ganchos de test: retraso al empezar cada escritura y número de escrituras hechas de verdad
    public static int WriteDelayMsForTests;
    public static int WritesForTests { get; private set; }

    public static void ResetCountersForTests()
    {
        LastWriterThread = 0;
        WritesForTests = 0;
    }

    public static void Save(SaveData data)
    {
        string json;
        try
        {
            json = Serialize(data);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Guardado] No se pudo preparar la partida: {e.Message}");
            return;
        }
        string path = FilePath; // Se fija ahora: DirectoryOverride puede cambiar antes de escribir
        lock (WriteLock)
        {
            int mine = ++version;
            pending = pending.ContinueWith(_ => Write(path, json, mine), System.Threading.Tasks.TaskScheduler.Default);
        }
    }

    private static void Write(string path, string json, int mine)
    {
        if (WriteDelayMsForTests > 0)
            System.Threading.Thread.Sleep(WriteDelayMsForTests);
        lock (WriteLock)
        {
            if (mine != version)
                return; // Ya hay un guardado más nuevo, o un borrado, detrás
        }
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temp = path + ".tmp";
            File.WriteAllText(temp, json);

            // Escritura atómica: nunca queda un guardado a medias. File.Replace cambia uno por otro de una vez; antes,
            // entre el Delete y el Move solo quedaba el temporal (auditoría finecomb). TryLoad lo recupera igualmente.
            if (File.Exists(path))
                File.Replace(temp, path, null);
            else
                File.Move(temp, path);
            LastWriterThread = System.Threading.Thread.CurrentThread.ManagedThreadId;
            WritesForTests++;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Guardado] No se pudo guardar la partida: {e.Message}");
        }
    }

    /// <summary>Espera a que se escriba lo pendiente (antes de leer, borrar o salir).</summary>
    public static void Flush()
    {
        System.Threading.Tasks.Task wait;
        lock (WriteLock)
            wait = pending;
        try
        {
            wait.Wait();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Guardado] Escritura pendiente fallida: {e.Message}");
        }
    }

    [RuntimeInitializeOnLoadMethod]
    private static void FlushOnQuit()
    {
        Application.quitting += Flush; // Que la última partida guardada llegue al disco al cerrar
    }

    public static bool TryLoad(out SaveData data)
    {
        data = null;
        Flush();
        try
        {
            if (File.Exists(FilePath))
                return TryDeserialize(File.ReadAllText(FilePath), out data);
            // Solo el temporal: la app cayó a mitad de una escritura antigua (sin File.Replace)
            return File.Exists(FilePath + ".tmp") && TryDeserialize(File.ReadAllText(FilePath + ".tmp"), out data);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Guardado] No se pudo leer la partida: {e.Message}");
            data = null;
            return false;
        }
    }

    public static string SetAsidePath => Path.Combine(DirectoryOverride ?? Application.persistentDataPath, "partida.rechazada.json");

    /// <summary>
    /// Aparta una partida que no se pudo restaurar (auditoría finecomb: "Continuar" la borraba ante cualquier
    /// excepción). Deja de ofrecerse, pero el archivo se conserva para recuperarla o diagnosticar el fallo.
    /// </summary>
    public static void SetAside()
    {
        lock (WriteLock)
            version++; // Lo que esté en cola ya no se escribe
        Flush();
        try
        {
            if (!File.Exists(FilePath))
                return;
            if (File.Exists(SetAsidePath))
                File.Delete(SetAsidePath);
            File.Move(FilePath, SetAsidePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Guardado] No se pudo apartar la partida: {e.Message}");
        }
    }

    public static void Delete()
    {
        lock (WriteLock)
            version++; // Lo que esté en cola ya no se escribe
        Flush();
        try
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Guardado] No se pudo borrar la partida: {e.Message}");
        }
    }

    /// <summary>
    /// Reconstruye el estado de la investigación a partir del guardado.
    /// </summary>
    public static InvestigationState RestoreState(VariantData variant, SaveData data)
    {
        var state = new InvestigationState(variant);

        var by = new Dictionary<string, string>();
        foreach (string entry in data.discoveredBy ?? new List<string>())
        {
            int colon = entry != null ? entry.IndexOf(':') : -1;
            if (colon > 0)
                by[entry.Substring(0, colon)] = entry.Substring(colon + 1);
        }
        foreach (string clueId in data.discovered)
            state.Discover(clueId, by.TryGetValue(clueId, out string who) && variant.Clue(clueId)?.HeldBy(who) == true ? who : null);
        foreach (SaveData.Shown shown in data.shown)
            state.RegisterShown(shown.characterId, shown.clueId);
        if (data.culpritToldLie) // Guardados anteriores: solo sabían de la mentira del culpable
            state.RegisterLieTold();
        foreach (string who in data.liesTold ?? new List<string>())
            if (variant.roles.Any(r => r.characterId == who))
                state.RegisterLieTold(who);

        state.UpdateContradictions();
        return state;
    }
}
