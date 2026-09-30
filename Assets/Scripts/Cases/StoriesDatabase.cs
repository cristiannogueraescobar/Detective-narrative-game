using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Base de datos de las historias (Resources/Stories/StoriesDatabase.json): la profundidad de cada personaje
/// (carácter, herida, cómo se le nota al mentir, cómo reacciona bajo presión; y, si es culpable, cómo sostiene la
/// mentira) y los disparadores naturales de los desbloqueos. Se aplica sobre las historias al construirlas; si el
/// archivo falta, las historias funcionan igual, sin esa capa.
/// </summary>
public static class StoriesDatabase
{
    public const string ResourcePath = "Stories/StoriesDatabase";

    [Serializable] private class Root { public StoryEntry[] stories; }
    [Serializable] private class StoryEntry { public string id; public CharacterEntry[] characters; public UnlockTrigger[] unlocks; }
    [Serializable] private class CharacterEntry
    {
        public string id, personality, wound, tellsLying, pressureArc;
        public LieStrategy[] lieStrategies;
    }
    [Serializable] private class LieStrategy { public string variant, text; }

    public static void Apply(System.Collections.Generic.IEnumerable<StoryData> stories)
    {
        TextAsset json = Resources.Load<TextAsset>(ResourcePath);
        if (json == null)
            return;
        Apply(stories, json.text);
    }

    public static void Apply(System.Collections.Generic.IEnumerable<StoryData> stories, string json)
    {
        Root root = JsonUtility.FromJson<Root>(json);
        if (root?.stories == null)
            return;

        foreach (StoryData story in stories)
        {
            StoryEntry entry = root.stories.FirstOrDefault(s => s.id == story.id);
            if (entry == null)
                continue;

            foreach (CharacterEntry c in entry.characters ?? new CharacterEntry[0])
            {
                CharacterData character = story.cast.FirstOrDefault(x => x.id == c.id);
                if (character == null)
                    continue;
                character.personality = c.personality;
                character.wound = c.wound;
                character.tellsLying = c.tellsLying;
                character.pressureArc = c.pressureArc;
                foreach (LieStrategy lie in c.lieStrategies ?? new LieStrategy[0])
                {
                    VariantData v = story.variants.FirstOrDefault(x => x.id == lie.variant);
                    if (v != null && v.culpritId == c.id)
                        v.Role(c.id).lieStrategy = lie.text;
                }
            }

            story.unlockTriggers.Clear();
            story.unlockTriggers.AddRange((entry.unlocks ?? new UnlockTrigger[0]).Where(u => story.cast.Any(x => x.id == u.id)));
        }
    }
}
