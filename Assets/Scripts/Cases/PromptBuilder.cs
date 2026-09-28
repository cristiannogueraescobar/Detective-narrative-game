using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Construye la ficha (system prompt) de un personaje en una variante.
/// Pensada para modelos pequeños (qwen2.5:7b): secciones cortas, siempre en el mismo orden.
/// </summary>
public static class PromptBuilder
{
    public static string Build(StoryData story, VariantData variant, string characterId, int day,
                               IEnumerable<ClueData> shownToCharacter, IEnumerable<ClueData> alreadyTold)
    {
        CharacterData character = story.Character(characterId);
        CharacterRole role = variant.Role(characterId);
        bool isCulprit = variant.culpritId == characterId;

        List<ClueData> ownClues = variant.clues.Where(c => c.holder == characterId).ToList();
        List<ClueData> openFacts = ownClues.Where(c => !c.isSecret).ToList();
        List<ClueData> secretFacts = ownClues.Where(c => c.isSecret).ToList();
        List<ClueData> shown = shownToCharacter?.ToList() ?? new List<ClueData>();
        List<ClueData> told = alreadyTold?.ToList() ?? new List<ClueData>();

        var sb = new StringBuilder();

        sb.AppendLine(character.identity);
        sb.AppendLine($"CÓMO HABLAS: {character.speech} Ejemplo: \"{character.speechExample}\"");
        sb.AppendLine($"EL CASO: {story.caseBrief}");

        if (role.knowledge != null && role.knowledge.Length > 0)
        {
            sb.AppendLine("LO QUE SABES:");
            foreach (string line in role.knowledge)
                sb.AppendLine($"- {line}");
        }

        if (openFacts.Count > 0)
        {
            sb.AppendLine("SI TE PREGUNTAN, CUÉNTALO CON LA HORA Y LOS DETALLES EXACTOS:");
            foreach (ClueData clue in openFacts)
                sb.AppendLine($"- Si te preguntan por {clue.topic}: {clue.fact}");
        }

        sb.AppendLine($"TU VERSIÓN: {role.version}");

        sb.Append($"LO QUE OCULTAS: {role.secret}");
        foreach (ClueData clue in secretFacts)
            sb.Append($" {clue.fact}");
        sb.AppendLine($" Lo admites solo si {role.admitsWhen}.");

        if (isCulprit)
        {
            sb.AppendLine("ERES EL CULPABLE, pero nunca lo confiesas. Mantén tu versión con calma y no des detalles de más.");
            sb.AppendLine($"SI EL INSPECTOR TE MUESTRA UNA PRUEBA QUE CONTRADICE TU VERSIÓN: {role.versionB}");
        }
        else
        {
            sb.AppendLine("Eres inocente del crimen, aunque tengas cosas que ocultar.");
        }

        sb.AppendLine($"TE PONE NERVIOSO: {role.nervousAbout}");
        sb.AppendLine($"SI TE ACUSAN: {role.ifAccused}");
        sb.AppendLine($"NO SABES: {role.doesNotKnow} Si te preguntan por eso, di que no lo sabes.");

        if (shown.Count > 0)
        {
            sb.AppendLine("PRUEBAS QUE YA TE HAN MOSTRADO:");
            foreach (ClueData clue in shown)
                sb.AppendLine($"- {clue.summary}");
        }

        if (told.Count > 0)
        {
            sb.AppendLine("YA HAS CONTADO ESTO (no te contradigas):");
            foreach (ClueData clue in told)
                sb.AppendLine($"- {clue.fact}");
        }

        sb.AppendLine($"HOY ES EL DÍA {day} DE LA INVESTIGACIÓN.");
        sb.Append("REGLAS: Responde en español, en primera persona, con 2 a 4 frases. " +
                  "Sin asteriscos, sin listas, sin acotaciones. Nunca digas que eres una IA. " +
                  "No inventes horas, nombres ni hechos que no estén en esta ficha.");

        return sb.ToString();
    }
}
