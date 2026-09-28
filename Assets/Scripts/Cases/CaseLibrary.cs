using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Registro de historias jugables. Solo las variantes registradas aquí entran en el sorteo.
/// </summary>
public static class CaseLibrary
{
    private static List<StoryData> stories;

    public static IReadOnlyList<StoryData> Stories
    {
        get
        {
            if (stories == null)
            {
                stories = new List<StoryData>
                {
                    Story1HijaPerfecta.Build()
                };
            }
            return stories;
        }
    }

    public static IEnumerable<(StoryData story, VariantData variant)> AllVariants()
    {
        return Stories.SelectMany(story => story.variants.Select(variant => (story, variant)));
    }

    public static bool TryFind(string variantId, out StoryData story, out VariantData variant)
    {
        foreach (var pair in AllVariants())
        {
            if (pair.variant.id == variantId)
            {
                story = pair.story;
                variant = pair.variant;
                return true;
            }
        }

        story = null;
        variant = null;
        return false;
    }
}
