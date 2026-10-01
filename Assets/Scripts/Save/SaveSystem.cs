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
    public List<Shown> shown = new List<Shown>();
    public bool culpritToldLie;
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

    public static bool Exists => File.Exists(FilePath);

    public static string Serialize(SaveData data)
    {
        return JsonUtility.ToJson(data, true);
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

    public static void Save(SaveData data)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            string temp = FilePath + ".tmp";
            File.WriteAllText(temp, Serialize(data));

            // Escritura atómica: nunca queda un guardado a medias
            if (File.Exists(FilePath))
                File.Delete(FilePath);
            File.Move(temp, FilePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Guardado] No se pudo guardar la partida: {e.Message}");
        }
    }

    public static bool TryLoad(out SaveData data)
    {
        data = null;
        try
        {
            return File.Exists(FilePath) && TryDeserialize(File.ReadAllText(FilePath), out data);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Guardado] No se pudo leer la partida: {e.Message}");
            data = null;
            return false;
        }
    }

    public static void Delete()
    {
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

        foreach (string clueId in data.discovered)
            state.Discover(clueId);
        foreach (SaveData.Shown shown in data.shown)
            state.RegisterShown(shown.characterId, shown.clueId);
        if (data.culpritToldLie)
            state.RegisterLieTold();

        state.UpdateContradictions();
        return state;
    }
}
