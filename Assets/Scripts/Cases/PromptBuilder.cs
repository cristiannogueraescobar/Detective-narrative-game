using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Construye la ficha (system prompt) de un personaje en una variante.
/// Pensada para modelos pequeños (qwen2.5:7b): secciones cortas, siempre en el mismo orden.
/// </summary>
public static class PromptBuilder
{
    // Pistas para que la etiqueta de estado sea coherente con lo que se le pregunta
    public static string EmotionGuide(string victim)
    {
        string who = string.IsNullOrEmpty(victim) ? "la víctima" : victim;
        return $"Tranquilo si te preguntan por ti, tu trabajo o tu rutina. Si te hablan de {who}: triste (o nervioso si ocultas algo), nunca tranquilo. Nervioso si tocan lo que te pone nervioso; enfadado o asustado si te acusan.";
    }

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
            sb.AppendLine("LO CUENTAS SI TE PREGUNTAN POR EL TEMA, con tus palabras, completo y con la hora:");
            foreach (ClueData clue in openFacts)
                sb.AppendLine($"- Si te preguntan por {clue.topic}: {clue.fact}");
        }

        sb.AppendLine($"TU VERSIÓN: {role.version}");

        sb.AppendLine($"LO QUE OCULTAS: {role.secret} Lo admites solo si {role.admitsWhen}.");
        foreach (ClueData clue in secretFacts)
            sb.AppendLine($"- Si te preguntan por {clue.topic}: niégalo la primera vez; si el inspector insiste o dice que lo va a comprobar, confiésalo con tus palabras: {clue.fact}");

        if (isCulprit)
        {
            sb.AppendLine("ERES EL CULPABLE, pero nunca lo confiesas. Mantén tu versión con calma y no des detalles de más.");
            sb.AppendLine($"SI EL INSPECTOR TE MUESTRA UNA PRUEBA QUE CONTRADICE TU VERSIÓN: {role.versionB}");
        }
        else
        {
            sb.AppendLine("Eres inocente del crimen, aunque ocultes cosas.");
        }

        sb.AppendLine($"TE PONE NERVIOSO: {role.nervousAbout}");
        sb.AppendLine($"SI TE ACUSAN: {role.ifAccused}");
        sb.AppendLine($"NO SABES: {role.doesNotKnow} Si te preguntan, di que no lo sabes.");

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
        sb.Append("REGLAS: español, primera persona, 2 a 4 frases, sin asteriscos ni listas. " +
                  "Nunca digas que eres una IA. No inventes nombres, hechos ni horas: si no sabes la hora, di que no te fijaste.\n" +
                  EmotionParser.TagInstruction + " " + EmotionGuide(story.victim));

        return sb.ToString();
    }
}
