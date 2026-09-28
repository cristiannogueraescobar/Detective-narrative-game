using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// SISTEMA DE PISTAS LÓGICO Y DETERMINISTA
/// Las pistas aparecen 100% si haces la pregunta correcta
/// Desbloqueo de sospechosos narrativo
/// </summary>
public class AIConversationManager : MonoBehaviour
{
    [Header("Proveedor LLM")]
    [SerializeField] private LLMProviderType provider = LLMProviderType.Ollama;
    [SerializeField] private OllamaSettings ollamaSettings = new OllamaSettings();
    [SerializeField] private AnthropicSettings anthropicSettings = new AnthropicSettings();

    [Header("Response Settings")]
    [SerializeField] private int maxTokens = 400;
    [SerializeField][Range(0f, 1f)] private float temperature = 0.75f;
    
    // EVENTOS
    public event Action<string, string, string> OnClueRevealed; // (clueId, clueName, description)
    public event Action<string> OnContradictionDetected; // (contradiction)
    public event Action<string, string, string> OnResponseReceived; // (suspect, question, response)
    public event Action<string, string> OnSuspectMentioned; // (caseId, suspectName)

    // DATOS
    private ILLMProvider llmProvider;
    private Dictionary<string, CaseData> cases = new Dictionary<string, CaseData>();
    private Dictionary<string, string> personalityPrompts = new Dictionary<string, string>();
    private Dictionary<string, List<ChatMessage>> conversationHistory =
        new Dictionary<string, List<ChatMessage>>();
    private Dictionary<string, HashSet<string>> revealedClues = 
        new Dictionary<string, HashSet<string>>();
    
    private HashSet<string> mentionedSuspects = new HashSet<string>();
    
    private void Awake()
    {
        BuildCases();
        BuildPersonalities();
        llmProvider = CreateProvider();
        Debug.Log($"[AIConversation] Sistema cargado: {cases.Count} historias. Proveedor: {llmProvider.DisplayName}");
    }

    private void Start()
    {
        // En segundo plano mientras el jugador está en el menú; no se espera ni bloquea la UI
        _ = llmProvider.WarmUpAsync();
    }

    private ILLMProvider CreateProvider()
    {
        switch (provider)
        {
            case LLMProviderType.Anthropic:
                return new AnthropicProvider(anthropicSettings);
            default:
                return new OllamaProvider(ollamaSettings);
        }
    }
    
    // ============================================
    // CASOS COMPLETOS
    // ============================================
    
    private void BuildCases()
    {
        // CASO 1A: Padre culpable - Niña adoptada sedada
        cases["1A"] = new CaseData
        {
            id = "1A",
            title = "La Hija Perfecta",
            description = @"Santiago, una ciudad tranquila. Una familia respetada.

Elena Mendoza, 12 años, adoptada. Muere en casa una noche de septiembre.

Los padres llaman a emergencias. Tarde. Demasiado tarde.

Daniel (padre, abogado), Carmen (madre, doctora), Lucas (hermano, 16 años).

¿Qué pasó realmente?",
            culprit = "Padre",
            crimeTime = "22:30",
            reportedTime = "23:15",
            requiredClues = new List<string> { 
                "1A_hora_muerte",
                "1A_sedacion_habitual",
                "1A_puerta_cerrada",
                "1A_vecina_vio"
            }
        };
        
        // CASO 1B: Madre culpable - Síndrome de Münchausen
        cases["1B"] = new CaseData
        {
            id = "1B",
            title = "La Hija Perfecta",
            description = cases["1A"].description,
            culprit = "Madre",
            crimeTime = "22:00",
            reportedTime = "23:15",
            requiredClues = new List<string>
            {
                "1B_hora_muerte",
                "1B_medicacion_excesiva",
                "1B_sintomas_falsos",
                "1B_madre_esperó"
            }
        };
        
        // CASO 1C: Hermano culpable - Accidente cubierto
        cases["1C"] = new CaseData
        {
            id = "1C",
            title = "La Hija Perfecta",
            description = cases["1A"].description,
            culprit = "Hermano",
            crimeTime = "21:45",
            reportedTime = "23:15",
            requiredClues = new List<string>
            {
                "1C_hora_muerte",
                "1C_discusion",
                "1C_golpe_escalera",
                "1C_llamada_padre"
            }
        };

        // CASO 2A: Dueño bar culpable - Joven desaparecida
        cases["2A"] = new CaseData
        {
            id = "2A",
            title = "Noche de Verano",
            description = @"Costa gallega. Madrugada de agosto.

Sofía Vargas, 19 años, desaparece tras una fiesta.

Sale caminando sola a las 5:00 AM. 'Estoy cerca', dice por mensaje.

Marcos (dueño del bar), Andrés (cartero), Inspector Ruiz (detective).

Varios testigos. Versiones contradictorias. Un coche rondando.",
            culprit = "Dueño del Bar",
            crimeTime = "05:20",
            reportedTime = "08:30",
            requiredClues = new List<string>
            {
                "2A_hora_desaparicion",
                "2A_coche_rondando",
                "2A_cartero_vio",
                "2A_bar_cerro_tarde"
            }
        };
        
        // CASO 2B: Cartero culpable
        cases["2B"] = new CaseData
        {
            id = "2B",
            title = "Noche de Verano",
            description = cases["2A"].description,
            culprit = "Cartero",
            crimeTime = "05:15",
            reportedTime = "08:30",
            requiredClues = new List<string>
            {
                "2B_hora_desaparicion",
                "2B_ruta_cartero",
                "2B_furgoneta_limpia",
                "2B_conocia_horarios"
            }
        };
        
        // CASO 2C: Detective culpable
        cases["2C"] = new CaseData
        {
            id = "2C",
            title = "Noche de Verano",
            description = cases["2A"].description,
            culprit = "Detective",
            crimeTime = "05:10",
            reportedTime = "08:30",
            requiredClues = new List<string>
            {
                "2C_hora_desaparicion",
                "2C_caso_cerrado_rapido",
                "2C_evidencia_perdida",
                "2C_conflicto_interes"
            }
        };
        
        // CASO 3A: Padre culpable - Hoguera
        cases["3A"] = new CaseData
        {
            id = "3A",
            title = "Humo y Silencio",
            description = @"Otoño en Andalucía. Divorcio reciente.

Paula Navarro, 15 años, desaparece con su padre el fin de semana.

Javier (padre) reporta su desaparición el domingo por la noche.

Un vecino vio humo denso en la finca el sábado. Humo que olía raro.

Custodia disputada. Amenazas previas.",
            culprit = "Padre",
            crimeTime = "Sábado 21:00",
            reportedTime = "Domingo 22:30",
            requiredClues = new List<string>
            {
                "3A_hora_humo",
                "3A_vecino_olor",
                "3A_combustible",
                "3A_amenazas"
            }
        };
        
        // CASO 3B: Madre culpable
        cases["3B"] = new CaseData
        {
            id = "3B",
            title = "Humo y Silencio",
            description = cases["3A"].description,
            culprit = "Madre",
            crimeTime = "Sábado 19:00",
            reportedTime = "Domingo 22:30",
            requiredClues = new List<string>
            {
                "3B_hora_visita_finca",
                "3B_depresion",
                "3B_carta",
                "3B_medicacion_doble"
            }
        };
        
        // CASO 3C: Vecina culpable
        cases["3C"] = new CaseData
        {
            id = "3C",
            title = "Humo y Silencio",
            description = cases["3A"].description,
            culprit = "Vecina",
            crimeTime = "Sábado 20:00",
            reportedTime = "Domingo 22:30",
            requiredClues = new List<string>
            {
                "3C_hora_vecina_finca",
                "3C_obsesion",
                "3C_acceso_finca",
                "3C_celos"
            }
        };
    }

    // ============================================
    // PERSONALIDADES
    // ============================================
    
    private void BuildPersonalities()
    {
        personalityPrompts["padre_controlador"] = @"Eres Daniel Mendoza, abogado prestigioso.

RASGOS:
- Hablas de forma precisa y calculada
- Muy defensivo cuando te cuestionan
- Dificultad para mostrar emociones genuinas
- Controlador y meticuloso

COMPORTAMIENTO:
- Respondes con frases cortas y medidas
- Corriges constantemente pequeños detalles
- Evitas hablar de emociones
- Te molestas si detectas que dudan de ti

Ejemplo:
'Llegué a casa exactamente a las 22:10. No 22:15 ni 22:05. A las 22:10.'";

        personalityPrompts["madre_doctora"] = @"Eres Carmen Vidal, doctora pediatra.

RASGOS:
- Emotiva pero profesional
- Hablas con términos médicos cuando estás nerviosa
- Protectora de tu familia
- Genuinamente devastada

COMPORTAMIENTO:
- Lloras al hablar de Elena
- Explicas demasiado los detalles médicos
- Defiendes a tu familia automáticamente
- Tu dolor es visible y real

Ejemplo:
'Yo... yo revisé sus constantes esa noche. Todo parecía... normal. *solloza* Como doctora sé que debí...'";

        personalityPrompts["hermano_adolescente"] = @"Eres Lucas Mendoza, 16 años, estudiante.

RASGOS:
- Hablas como un adolescente normal
- Nervioso y asustado
- Sientes culpa aunque no hayas hecho nada
- Quieres proteger a tus padres

COMPORTAMIENTO:
- Usas muletillas: 'tío', 'o sea', 'no sé'
- Te contradices cuando estás nervioso
- Evitas el contacto visual (narras esto)
- Muestras más de lo que quieres

Ejemplo:
'Yo estaba en mi cuarto, tío. O sea, jugando a la Play. No sé... no escuché nada raro.'";

        personalityPrompts["dueno_bar"] = @"Eres Marcos Rial, dueño del bar 'La Marea'.

RASGOS:
- Hombre de 40 años, rudo pero simpático
- Conoce a todo el pueblo
- Habla con acento gallego suave
- Protector de su negocio y reputación

COMPORTAMIENTO:
- Haces comentarios sobre el pueblo
- Te quejas de la mala suerte
- Recuerdas detalles de clientes habituales
- Nervioso si hablan de cerrar tarde

Ejemplo:
'Mira, chaval, aquí cerramos cuando se va el último. Esa noche cerré sobre las cinco, como siempre en verano.'";

        personalityPrompts["cartero"] = @"Eres Andrés Souto, cartero hace 15 años.

RASGOS:
- Hombre metódico de 50 años
- Conoce cada rincón del pueblo
- Observador silencioso
- Rutinario hasta el extremo

COMPORTAMIENTO:
- Hablas de tu ruta con precisión
- Mencionas detalles que 'no tienen importancia'
- Te justificas diciendo que 'solo hacías tu trabajo'
- Incómodo con preguntas directas

Ejemplo:
'Paso por esa carretera cada mañana. A las 6:15 exactamente. Siempre a la misma hora. Es mi ruta.'";

        personalityPrompts["detective"] = @"Eres el Inspector Ruiz, 25 años en el cuerpo.

RASGOS:
- Profesional pero cansado del sistema
- Conoces todos los trucos
- Pragmático y algo cínico
- Leal a tus compañeros

COMPORTAMIENTO:
- Hablas con jerga policial
- Restas importancia a detalles
- Quieres cerrar casos rápido
- Te molestas si cuestionan tu trabajo

Ejemplo:
'Mira, llevo 25 años en esto. Sé reconocer un caso abierto y cerrado. Esta chica... fue mala suerte.'";

        personalityPrompts["padre_divorciado"] = @"Eres Javier Romero, padre recién divorciado.

RASGOS:
- Hombre de 38 años, amargado
- Obsesionado con la custodia
- Oscila entre ira y victimización
- Habla mal de tu ex constantemente

COMPORTAMIENTO:
- Culpas a tu ex de todo
- Justificas cada acción como 'por Paula'
- Agresivo cuando te presionan
- Alternas entre calma y explosiones

Ejemplo:
'Claro, ahora yo soy el malo. Como siempre. Su madre es la santa y yo el monstruo, ¿no?'";

        Debug.Log($"[AIConversation] {personalityPrompts.Count} personalidades cargadas");
    }

    // ============================================
    // API DE ENVÍO DE MENSAJES
    // ============================================

    /// <summary>
    /// Método principal usado por GameManager.
    /// Si la petición falla, el historial queda como estaba y no se detectan pistas.
    /// </summary>
    public async Task<LLMResult> AskSuspect(string suspectName, string question, string caseId, int currentDay)
    {
        if (!cases.ContainsKey(caseId))
        {
            return LLMResult.Fail("Caso no encontrado.");
        }

        string conversationKey = $"{caseId}_{suspectName}";
        
        if (!conversationHistory.ContainsKey(conversationKey))
        {
            conversationHistory[conversationKey] = new List<ChatMessage>();
        }
        
        if (!revealedClues.ContainsKey(conversationKey))
        {
            revealedClues[conversationKey] = new HashSet<string>();
        }

        List<ChatMessage> history = conversationHistory[conversationKey];
        history.Add(new ChatMessage { role = "user", content = question });

        LLMResult result = await llmProvider.SendAsync(
            BuildSystemPrompt(caseId, suspectName), history, maxTokens, temperature);

        if (!result.Success)
        {
            // Quitar la pregunta para no dejar dos mensajes "user" seguidos al reintentar
            history.RemoveAt(history.Count - 1);
            Debug.LogWarning($"[AIConversation] Petición fallida ({llmProvider.DisplayName}): {result.ErrorMessage}");
            return result;
        }

        history.Add(new ChatMessage { role = "assistant", content = result.Text });

        DetectSuspectMentions(question, caseId);
        DetectCluesInResponse(caseId, suspectName, question, result.Text);

        OnResponseReceived?.Invoke(suspectName, question, result.Text);
        return result;
    }

    // ============================================
    // CONSTRUCCIÓN DEL SYSTEM PROMPT
    // ============================================

    private string BuildSystemPrompt(string caseId, string suspect)
    {
        CaseData caseData = cases[caseId];
        string conversationKey = $"{caseId}_{suspect}";
        
        string personality = GetPersonalityForSuspect(suspect);
        
        HashSet<string> revealed = revealedClues.ContainsKey(conversationKey) 
            ? revealedClues[conversationKey] 
            : new HashSet<string>();

        bool isGuilty = caseData.culprit.ToLower().Contains(suspect.ToLower());

        StringBuilder prompt = new StringBuilder();
        
        prompt.AppendLine(personality);
        prompt.AppendLine();
        prompt.AppendLine("=== CONTEXTO DEL CASO ===");
        prompt.AppendLine(caseData.description);
        prompt.AppendLine();
        
        if (isGuilty)
        {
            prompt.AppendLine("=== TU SECRETO ===");
            prompt.AppendLine($"TÚ ERES EL CULPABLE. Cometiste el crimen a las {caseData.crimeTime}.");
            prompt.AppendLine($"Reportaste a las {caseData.reportedTime}.");
            prompt.AppendLine("ESTRATEGIA:");
            prompt.AppendLine("- Niega todo inicialmente");
            prompt.AppendLine("- Solo admite si te confrontan con evidencia específica");
            prompt.AppendLine("- Muestra nerviosismo cuando hablan de horarios");
            prompt.AppendLine("- Contradícete sutilmente bajo presión");
        }
        else
        {
            prompt.AppendLine("=== TU INOCENCIA ===");
            prompt.AppendLine("Eres INOCENTE. No cometiste este crimen.");
            prompt.AppendLine("ESTRATEGIA:");
            prompt.AppendLine("- Mantén tu historia consistente");
            prompt.AppendLine("- Muestra emociones genuinas");
            prompt.AppendLine("- Puedes tener sospechas de otros");
            prompt.AppendLine("- Colabora pero con límites personales");
        }
        
        prompt.AppendLine();
        prompt.AppendLine("=== PISTAS QUE PUEDES REVELAR ===");
        if (revealed.Count > 0)
        {
            foreach (string clue in revealed)
            {
                prompt.AppendLine($"✓ {clue}");
            }
        }
        else
        {
            prompt.AppendLine("Ninguna todavía. Sé cauteloso con lo que revelas.");
        }

        prompt.AppendLine();
        prompt.AppendLine("=== INSTRUCCIONES CRÍTICAS ===");
        prompt.AppendLine("1. Responde en 2-4 párrafos máximo");
        prompt.AppendLine("2. NO uses asteriscos para acciones");
        prompt.AppendLine("3. NO uses listas ni bullet points");
        prompt.AppendLine("4. Habla naturalmente, como en un interrogatorio real");
        prompt.AppendLine("5. Muestra emociones a través del diálogo");
        prompt.AppendLine("6. Si mencionan otros sospechosos, puedes dar opiniones");

        return prompt.ToString();
    }

    // ============================================
    // DETECCIÓN DE PISTAS
    // ============================================

    private void DetectCluesInResponse(string caseId, string suspect, string question, string response)
    {
        string conversationKey = $"{caseId}_{suspect}";
        string questionLower = question.ToLower();
        string responseLower = response.ToLower();

        var potentialClues = GetCluesForCase(caseId);
        
        foreach (var clue in potentialClues)
        {
            bool questionMatches = false;
            foreach (string keyword in clue.keywords)
            {
                if (questionLower.Contains(keyword.ToLower()))
                {
                    questionMatches = true;
                    break;
                }
            }

            if (questionMatches)
            {
                if (!revealedClues.ContainsKey(conversationKey))
                {
                    revealedClues[conversationKey] = new HashSet<string>();
                }

                if (!revealedClues[conversationKey].Contains(clue.name))
                {
                    revealedClues[conversationKey].Add(clue.name);
                    
                    // Disparar evento con firma correcta: (clueId, clueName, description)
                    OnClueRevealed?.Invoke(clue.name, clue.name, $"Pista descubierta: {clue.name}");
                    
                    Debug.Log($"[PISTA REVELADA] {clue.name}");

                    if (!string.IsNullOrEmpty(clue.suspectToUnlock))
                    {
                        OnSuspectMentioned?.Invoke(caseId, clue.suspectToUnlock);
                    }
                }
            }
        }
        
        // Detectar contradicciones básicas
        DetectContradictions(caseId, suspect, response);
    }

    private void DetectContradictions(string caseId, string suspect, string response)
    {
        string conversationKey = $"{caseId}_{suspect}";
        
        if (!conversationHistory.ContainsKey(conversationKey) || 
            conversationHistory[conversationKey].Count < 4)
        {
            return; // Necesitamos al menos 2 intercambios para detectar contradicciones
        }

        string responseLower = response.ToLower();
        
        // Buscar contradicciones simples en horarios
        if (responseLower.Contains("22:00") || responseLower.Contains("diez") || 
            responseLower.Contains("once") || responseLower.Contains("23:00"))
        {
            // Revisar mensajes anteriores
            for (int i = conversationHistory[conversationKey].Count - 3; i >= 0; i--)
            {
                var oldMessage = conversationHistory[conversationKey][i];
                if (oldMessage.role == "assistant")
                {
                    string oldLower = oldMessage.content.ToLower();
                    
                    // Ejemplo: dijo 22:00 antes pero ahora dice 23:00
                    if ((oldLower.Contains("22:00") && responseLower.Contains("23:00")) ||
                        (oldLower.Contains("23:00") && responseLower.Contains("22:00")))
                    {
                        OnContradictionDetected?.Invoke($"{suspect} contradijo su horario anterior");
                        break;
                    }
                }
            }
        }
    }

    private void DetectSuspectMentions(string message, string caseId)
    {
        string lower = message.ToLower();
        
        List<string> suspects = new List<string>();
        
        if (caseId.StartsWith("1"))
        {
            suspects.AddRange(new[] { "padre", "madre", "hermano", "daniel", "carmen", "lucas" });
        }
        else if (caseId.StartsWith("2"))
        {
            suspects.AddRange(new[] { "marcos", "andrés", "ruiz", "bar", "cartero", "inspector" });
        }
        else if (caseId.StartsWith("3"))
        {
            suspects.AddRange(new[] { "javier", "madre", "vecina", "padre" });
        }

        foreach (string suspect in suspects)
        {
            if (lower.Contains(suspect))
            {
                if (!mentionedSuspects.Contains(suspect))
                {
                    mentionedSuspects.Add(suspect);
                    OnSuspectMentioned?.Invoke(caseId, suspect);
                }
            }
        }
    }

    // ============================================
    // PISTAS POR CASO
    // ============================================

    private List<ClueData> GetCluesForCase(string caseId)
    {
        List<ClueData> clues = new List<ClueData>();

        if (caseId == "1A")
        {
            clues.Add(new ClueData
            {
                keywords = new[] { "hora", "tiempo", "cuándo", "qué hora" },
                name = "1A_hora_muerte",
                isTimeClue = true
            });
            clues.Add(new ClueData
            {
                keywords = new[] { "medicación", "pastillas", "sedantes", "dormida" },
                name = "1A_sedacion_habitual"
            });
            clues.Add(new ClueData
            {
                keywords = new[] { "puerta", "cerrada", "habitación", "acceso" },
                name = "1A_puerta_cerrada"
            });
            clues.Add(new ClueData
            {
                keywords = new[] { "vecina", "vio", "ventana", "testigo" },
                name = "1A_vecina_vio",
                suspectToUnlock = "vecina"
            });
        }
        else if (caseId == "1B")
        {
            clues.Add(new ClueData
            {
                keywords = new[] { "hora", "tiempo", "cuándo" },
                name = "1B_hora_muerte",
                isTimeClue = true
            });
            clues.Add(new ClueData
            {
                keywords = new[] { "medicación", "tratamiento", "doctora" },
                name = "1B_medicacion_excesiva"
            });
            clues.Add(new ClueData
            {
                keywords = new[] { "síntomas", "enfermedades", "médico" },
                name = "1B_sintomas_falsos"
            });
            clues.Add(new ClueData
            {
                keywords = new[] { "esperó", "tardó", "llamar", "emergencias" },
                name = "1B_madre_esperó"
            });
        }
        else if (caseId == "1C")
        {
            clues.Add(new ClueData
            {
                keywords = new[] { "hora", "tiempo", "cuándo" },
                name = "1C_hora_muerte",
                isTimeClue = true
            });
            clues.Add(new ClueData
            {
                keywords = new[] { "discusión", "pelea", "gritaron", "escuchó" },
                name = "1C_discusion"
            });
            clues.Add(new ClueData
            {
                keywords = new[] { "golpe", "escalera", "caída", "accidente" },
                name = "1C_golpe_escalera"
            });
            clues.Add(new ClueData
            {
                keywords = new[] { "llamada", "llamó", "teléfono", "padre" },
                name = "1C_llamada_padre"
            });
        }

        return clues;
    }

    // ============================================
    // UTILIDADES
    // ============================================

    private string GetPersonalityForSuspect(string suspect)
    {
        string lower = suspect.ToLower();
        
        if (lower.Contains("padre") || lower.Contains("daniel"))
            return personalityPrompts["padre_controlador"];
        if (lower.Contains("madre") || lower.Contains("carmen"))
            return personalityPrompts["madre_doctora"];
        if (lower.Contains("hermano") || lower.Contains("lucas"))
            return personalityPrompts["hermano_adolescente"];
        if (lower.Contains("marcos") || lower.Contains("bar"))
            return personalityPrompts["dueno_bar"];
        if (lower.Contains("andrés") || lower.Contains("cartero"))
            return personalityPrompts["cartero"];
        if (lower.Contains("ruiz") || lower.Contains("inspector"))
            return personalityPrompts["detective"];
        if (lower.Contains("javier"))
            return personalityPrompts["padre_divorciado"];

        return personalityPrompts["padre_controlador"];
    }

    // ============================================
    // API PÚBLICA
    // ============================================

    public List<string> GetRevealedClues(string caseId, string suspect)
    {
        string key = $"{caseId}_{suspect}";
        if (revealedClues.ContainsKey(key))
        {
            return new List<string>(revealedClues[key]);
        }
        return new List<string>();
    }

    public bool IsCaseSolved(string caseId)
    {
        if (!cases.ContainsKey(caseId))
            return false;

        CaseData caseData = cases[caseId];
        
        foreach (var kvp in revealedClues)
        {
            if (kvp.Key.StartsWith(caseId))
            {
                foreach (string requiredClue in caseData.requiredClues)
                {
                    if (!kvp.Value.Contains(requiredClue))
                        return false;
                }
                return true;
            }
        }
        
        return false;
    }

    public void ResetCase(string caseId)
    {
        List<string> keysToRemove = new List<string>();
        foreach (var key in conversationHistory.Keys)
        {
            if (key.StartsWith(caseId))
                keysToRemove.Add(key);
        }
        foreach (var key in keysToRemove)
        {
            conversationHistory.Remove(key);
            revealedClues.Remove(key);
        }
        
        mentionedSuspects.Clear();
        Debug.Log($"[AIConversation] Caso {caseId} reiniciado");
    }

    // ============================================
    // CLASES DE DATOS
    // ============================================

    [Serializable]
    public class CaseData
    {
        public string id;
        public string title;
        public string description;
        public string culprit;
        public string crimeTime;
        public string reportedTime;
        public List<string> requiredClues;
    }
    
    [Serializable]
    public class ClueData
    {
        public string[] keywords;
        public string name;
        public string suspectToUnlock;
        public bool isTimeClue;
    }
}
