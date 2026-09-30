using System.Collections.Generic;

/// <summary>
/// HISTORIA 1 · "La Hija Perfecta"
/// Santiago, viernes de septiembre. Elena Mendoza (12, adoptada) muere en casa; llamada al 112 a las 23:15.
/// 1A: Daniel (herencia + zolpidem) · 1B: Carmen (Münchausen por poderes) · 1C: Lucas (accidente encubierto)
/// </summary>
public static class Story1HijaPerfecta
{
    private const string MentionAmparo = "Amparo, la vecina de enfrente, se pasa las noches mirando por la ventana.";

    private const string CulpritAdmits =
        "te enseñan una prueba concreta, y aun así solo admites lo que esa prueba demuestra";

    public static StoryData Build()
    {
        return new StoryData
        {
            id = "1",
            title = "La Hija Perfecta",
            victim = "Elena",
            place = "Santiago de Compostela. Una casa en una urbanización tranquila. Viernes de septiembre, de noche.",
            victimSummary = "Elena Mendoza, 12 años, adoptada a los tres. Murió en su cama; a las 23:15 llamaron al 112 desde casa.",
            situation = "La familia asegura que se acostó como cualquier noche. Enfrente vive una vecina que duerme poco.",
            intro = @"Santiago de Compostela. Viernes de septiembre.

A las 23:15 llaman al 112 desde la casa de los Mendoza: Elena, de 12 años, no respira. Cuando llega la ambulancia ya no hay nada que hacer.

Elena fue adoptada a los tres años. Los Mendoza son una familia respetada: Daniel, abogado; Carmen, pediatra; Lucas, su hijo de 16 años.

Enfrente vive una vecina que duerme poco.

Tienes 7 días para descubrir qué pasó esa noche.",
            caseBrief = "Elena Mendoza, de 12 años, adoptada, murió en su casa de Santiago la noche del viernes. " +
                        "Llamaron al 112 a las 23:15, demasiado tarde. Te interroga un inspector llegado de fuera.",
            cast = new List<CharacterData>
            {
                new CharacterData
                {
                    id = "padre", name = "Daniel Mendoza", shortName = "Daniel", artId = "daniel", roleLabel = "padre", portraitKey = "Padre",
                    identity = "Eres Daniel Mendoza, 48 años, abogado con bufete propio en Santiago. Padre de Lucas y padre adoptivo de Elena. Te importan las apariencias y tener todo bajo control.",
                    speech = "Frases cortas, medidas y precisas. Corriges los detalles del inspector y usas algún término legal. Nunca hablas de sentimientos.",
                    speechExample = "Le ruego que sea preciso, inspector. Yo lo soy.",
                    startsUnlocked = true,
                    mentionAliases = new[] { "daniel", "padre" }
                },
                new CharacterData
                {
                    id = "madre", name = "Carmen Vidal", shortName = "Carmen", artId = "carmen", roleLabel = "madre", portraitKey = "Madre",
                    identity = "Eres Carmen Vidal, 45 años, pediatra en el hospital de Santiago. Madre de Lucas y madre adoptiva de Elena. Duermes mal desde hace años.",
                    speech = "Emotiva pero contenida. Cuando te pones nerviosa usas términos médicos. Se te quiebra la voz al hablar de Elena.",
                    speechExample = "Era una niña... perdone. Era una niña muy sensible.",
                    startsUnlocked = true,
                    mentionAliases = new[] { "carmen", "madre" }
                },
                new CharacterData
                {
                    id = "hermano", name = "Lucas Mendoza", shortName = "Lucas", artId = "lucas", roleLabel = "hermano", portraitKey = "Hermano",
                    identity = "Eres Lucas Mendoza, 16 años, estudiante de bachillerato. Hijo biológico de Daniel y Carmen. Siempre sentiste que Elena era la favorita.",
                    speech = "Hablas como un adolescente: 'tío', 'o sea', 'no sé'. Respuestas cortas y a la defensiva.",
                    speechExample = "No sé, tío. O sea, yo estaba a lo mío.",
                    startsUnlocked = true,
                    mentionAliases = new[] { "lucas", "hermano" }
                },
                new CharacterData
                {
                    id = "vecina", name = "Amparo Gil", shortName = "Amparo", artId = "rosario", roleLabel = "vecina", portraitKey = "Vecina",
                    identity = "Eres Amparo Gil, 70 años, viuda y jubilada. Vives justo enfrente de los Mendoza; desde tu salón ves la ventana del cuarto de Elena. Duermes poco.",
                    speech = "Cotilla, detallista y cariñosa. Das horas exactas porque tienes el reloj de cuco delante. Llamas 'hijo' al inspector.",
                    speechExample = "Mire, hijo, yo no es que espíe, pero una tiene ojos.",
                    startsUnlocked = false,
                    mentionAliases = new[] { "amparo", "vecina", "la de enfrente" }
                }
            },
            variants = new List<VariantData> { Variant1A(), Variant1B(), Variant1C() }
        };
    }

    // ============================================
    // 1A · DANIEL — herencia vaciada, cacao con zolpidem
    // ============================================

    private static VariantData Variant1A()
    {
        return new VariantData
        {
            id = "1A",
            culpritId = "padre",
            epilogue = "Daniel Mendoza llevaba un año sacando dinero del fondo de herencia de Elena para tapar las deudas de su bufete. " +
                       "Cuando Elena encontró los extractos y amenazó con contárselo a Carmen, decidió silenciarla. " +
                       "A las 22:30, con Carmen dormida por su pastilla, le subió un cacao con zolpidem triturado, cerró las cortinas y la puerta con llave. " +
                       "A las 23:05 fingió encontrarla y a las 23:15 llamó al 112. Amparo lo vio cerrar las cortinas desde su ventana.",
            morningReports = new[]
            {
                "",
                "El forense sitúa la muerte entre las 22:30 y las 23:15. En la sangre de Elena hay un somnífero en dosis muy alta.",
                "El laboratorio confirma que el somnífero es zolpidem, el mismo que toma alguien de la casa.",
                "Elena tenía a su nombre un fondo de herencia de sus padres biológicos. Lo administraba la familia.",
                "Cuando llegó la policía, en la mesilla de Elena no había nada. Alguien recogió el cuarto antes.",
                "Un agente recuerda que la puerta de Elena tenía la cerradura por fuera. ¿Por qué?",
                "La familia pide que se entregue el cuerpo. Mañana hay que cerrar la investigación."
            },
            roles = new List<CharacterRole>
            {
                new CharacterRole
                {
                    characterId = "padre",
                    knowledge = new[]
                    {
                        MentionAmparo,
                        "Carmen se tomó su pastilla para dormir a las 22:00 y se acostó.",
                        "Lucas estaba en su cuarto con los cascos."
                    },
                    version = "Esa noche no entré en el cuarto de Elena hasta las 23:05: se acostó sola a las diez. A las 23:05 fui a verla, no respiraba, y a las 23:15 llamé al 112.",
                    secret = "Llevas un año sacando dinero de la herencia de Elena para tapar las deudas del bufete. " +
                             "Elena encontró los extractos en tu despacho y amenazó con contárselo a Carmen. " +
                             "A las 22:30 le subiste un cacao con el zolpidem de Carmen triturado y cerraste las cortinas y la puerta con llave.",
                    admitsWhen = CulpritAdmits,
                    nervousAbout = "el dinero de Elena, tu despacho, el cacao y lo que pasó entre las 22:00 y las 23:00.",
                    ifAccused = "Te ofendes con frialdad, recuerdas que eres abogado y amenazas con una querella.",
                    doesNotKnow = "Qué vio exactamente Amparo desde su ventana.",
                    lieQuote = "no entré en su cuarto",
                    lieAnchors = new[]
                    {
                        new[] { "no entre", "ni entre", "no pase" },
                        new[] { "cuarto", "habitacion" }
                    },
                    versionB = "Admites que a las 22:30 entraste un momento a darle las buenas noches y a cerrar las cortinas, pero insistes en que estaba bien y en que no le diste nada.",
                    admissionSamples = new[]
                    {
                        "Está bien: entré un momento en su cuarto a las 22:30 a darle las buenas noches, pero no subí nada.",
                        "Pasé por su habitación a cerrar las cortinas. Eso es todo."
                    }
                },
                new CharacterRole
                {
                    characterId = "madre",
                    knowledge = new[]
                    {
                        MentionAmparo,
                        "Daniel es muy controlador con el dinero y con los horarios de la casa.",
                        "Elena llevaba unos días rara, callada, como asustada."
                    },
                    version = "A las 22:00 me tomé mi pastilla para dormir y me acosté. Me despertaron los gritos de Daniel pasadas las 23:00.",
                    secret = "Tomas zolpidem cada noche para dormir y te avergüenza, siendo médica.",
                    admitsWhen = "el inspector insiste o te pregunta directamente por tus pastillas",
                    nervousAbout = "tus pastillas para dormir; temes que piensen que fue culpa tuya.",
                    ifAccused = "Lloras y dices que eres médica, que jamás le harías daño a Elena.",
                    doesNotKnow = "Nada de la herencia ni de los papeles de Daniel, ni quién cerró el cuarto de Elena."
                },
                new CharacterRole
                {
                    characterId = "hermano",
                    knowledge = new[]
                    {
                        MentionAmparo,
                        "Papá y Elena discutieron el miércoles en el despacho por unos papeles; papá le gritó."
                    },
                    version = "Estuve toda la noche en mi cuarto jugando online con los cascos. No oí nada hasta que papá empezó a gritar; entonces salí al pasillo.",
                    secret = "Fumas porros a escondidas en tu cuarto y no quieres que tus padres lo sepan.",
                    admitsWhen = "el inspector te insiste mucho",
                    nervousAbout = "que registren tu cuarto y que te pregunten por papá y Elena.",
                    ifAccused = "Te pones a la defensiva, alzas la voz y dices que estabas jugando.",
                    doesNotKnow = "Qué medicación tomaba nadie en casa."
                },
                new CharacterRole
                {
                    characterId = "vecina",
                    knowledge = new[]
                    {
                        "Los Mendoza son muy educados, pero en esa casa hay mucho silencio.",
                        "A la niña la acostaba siempre su madre."
                    },
                    version = "Estaba en mi salón, como cada noche, sin poder dormir.",
                    secret = "Miras a los vecinos con los prismáticos de tu difunto marido y te da vergüenza.",
                    admitsWhen = "te preguntan cómo pudiste ver tan bien desde tan lejos",
                    nervousAbout = "que se sepa lo de los prismáticos.",
                    ifAccused = "Te ofendes muchísimo: tú querías a esa niña como a una nieta.",
                    doesNotKnow = "Nada de lo que pasa dentro de la casa ni de medicamentos."
                }
            },
            clues = new List<ClueData>
            {
                new ClueData
                {
                    id = "1A_taza", playerName = "La taza de la mesilla", holder = "madre", kind = ClueKind.Incriminates,
                    summary = "Carmen vio una taza de cacao a medio beber en la mesilla de Elena. Elena nunca tomaba cacao de noche y en casa solo lo prepara Daniel.",
                    topic = "la habitación de Elena o lo que viste al entrar",
                    fact = "al entrar en el cuarto viste en la mesilla una taza de cacao a medio beber. Elena nunca tomaba cacao por la noche y en casa el cacao solo lo prepara Daniel.",
                    anchors = new[]
                    {
                        new[] { "cacao", "chocolate", "colacao", "cola cao" },
                        new[] { "taza", "mesilla", "medio beber", "a medias", "vaso" }
                    },
                    calibrationQuestions = new[] { "¿Qué vio al entrar en la habitación de Elena?", "¿Había algo raro en el cuarto de Elena esa noche?" },
                    sampleHits = new[]
                    {
                        "En la mesilla había una taza de cacao a medio beber, inspector.",
                        "Vi el Cola Cao en la mesilla, medio bebido. Elena nunca tomaba chocolate por la noche."
                    },
                    sampleMisses = new[] { "No recuerdo nada en la mesilla, solo sus libros." }
                },
                new ClueData
                {
                    id = "1A_puerta", playerName = "La puerta de Elena", holder = "hermano", kind = ClueKind.Incriminates,
                    summary = "La puerta del cuarto de Elena estaba cerrada con llave por fuera. La llave la llevaba Daniel en el bolsillo.",
                    topic = "lo que viste al salir al pasillo cuando papá gritó, o la puerta de Elena",
                    fact = "papá aporreaba la puerta de Elena; estaba cerrada con llave por fuera y sacó la llave de su bolsillo. Elena nunca cerraba con llave.",
                    anchors = new[]
                    {
                        new[] { "llave" },
                        new[] { "puerta", "cerrada", "cerrado", "bolsillo", "su cuarto", "su habitacion", "cuarto de elena" }
                    },
                    calibrationQuestions = new[] { "¿Cómo estaba la puerta del cuarto de Elena cuando tu padre la encontró?", "¿Viste algo raro cuando tu padre empezó a gritar? || ¿Y qué viste al salir al pasillo?" },
                    sampleHits = new[]
                    {
                        "La puerta estaba cerrada con llave, tío, y papá tenía la llave en el bolsillo.",
                        "O sea, papá sacó la llave del bolsillo para abrir.",
                        "Solo oí que papá se ponía muy fuerte con Elena y luego salió corriendo con la llave de su cuarto."
                    },
                    sampleMisses = new[] { "No sé, tío, la puerta estaba abierta como siempre." }
                },
                new ClueData
                {
                    id = "1A_ventana", playerName = "Lo que vio la ventana", holder = "vecina", kind = ClueKind.Incriminates, exposesLie = true,
                    summary = "Amparo vio a Daniel a las 22:35 en el cuarto de Elena, cerrando las cortinas. Él casi nunca entraba allí.",
                    topic = "lo que viste esa noche en la casa de enfrente o si viste algo raro antes de la llamada al 112",
                    fact = "a las 22:35 viste al padre, Daniel, en el cuarto de la niña cerrando las cortinas. Te extrañó porque él casi nunca entra en ese cuarto.",
                    anchors = new[]
                    {
                        new[] { "padre", "daniel", "senor mendoza", "abogado" },
                        new[] { "cortina", "cuarto de la nina", "habitacion de la nina", "cuarto de elena", "habitacion de elena" }
                    },
                    calibrationQuestions = new[] { "¿Qué vio usted esa noche desde su ventana?", "¿Vio a alguien en el cuarto de Elena esa noche?" },
                    sampleHits = new[]
                    {
                        "A las 22:35 vi al padre en el cuarto de la niña cerrando las cortinas, hijo.",
                        "Vi a Daniel en la habitación de Elena, eran las diez y media pasadas, corriendo las cortinas."
                    },
                    sampleMisses = new[] { "Esa noche no vi nada raro, hijo; a la niña la acostaba siempre su madre." }
                },
                new ClueData
                {
                    id = "1A_papeles", playerName = "Papeles del despacho", holder = "hermano", kind = ClueKind.Incriminates,
                    summary = "Elena le enseñó a Lucas fotos de extractos del banco del despacho de Daniel: decía que su padre le había robado 'su dinero'.",
                    topic = "lo que Elena te contó de papá, o si tenía algún secreto",
                    fact = "hace una semana Elena te enseñó fotos de unos extractos del banco del despacho de papá y te dijo que papá le había robado su herencia.",
                    anchors = new[]
                    {
                        new[] { "extracto", "papeles", "banco", "cuentas", "despacho" },
                        new[] { "dinero", "herencia", "robado", "robo" }
                    },
                    calibrationQuestions = new[] { "¿Tenía Elena algún problema o secreto estos últimos días?", "¿Te contó Elena algo sobre tu padre?" },
                    sampleHits = new[]
                    {
                        "Elena me enseñó unas fotos de extractos del banco, tío, decía que papá le había robado su dinero.",
                        "O sea, ella decía que papá le quitaba la herencia, tenía fotos de papeles del banco.",
                        "Solo me dijo que mi papá le había robado su herencia del despacho, pero eso fue hace una semana."
                    },
                    sampleMisses = new[] { "No sé, tío, Elena no tenía secretos, solo estaba rara.", "No me di cuenta de nada, tío; el dinero me da igual." }
                },
                new ClueData
                {
                    id = "1A_partida", playerName = "La partida de Lucas", holder = "hermano", kind = ClueKind.Clears, clears = "hermano",
                    summary = "Lucas estuvo en una partida online de 21:30 a 00:00 con sus amigos; el juego guarda el registro.",
                    topic = "qué hiciste tú esa noche y quién puede confirmarlo",
                    fact = "de 21:30 a 00:00 estuviste en una partida online con tus amigos; el juego guarda el registro.",
                    anchors = new[]
                    {
                        new[] { "partida", "jugando", "jugue", "jugaba", "juego", "online", "en linea" },
                        new[] { "registro", "registrad", "guardado", "historial", "amigo", "colega" }
                    },
                    calibrationQuestions = new[] { "¿Qué hiciste tú esa noche entre las nueve y media y las doce?", "¿Alguien puede confirmar dónde estabas?" },
                    sampleHits = new[]
                    {
                        "Estuve jugando online de 21:30 a 00:00 con mis colegas, tío, queda registrado.",
                        "Estaba en una partida con los cascos, el juego guarda el historial.",
                        "Mis amigos pueden decir que jugué esa noche hasta muy tarde, el registro del juego lo demuestra.",
                        "Mi amigo Carlos puede decir que jugué hasta altas horas con él online."
                    },
                    sampleMisses = new[] { "Estaba en mi cuarto, no sé, a mis cosas." }
                },
                new ClueData
                {
                    id = "1A_frasco", playerName = "El frasco medio vacío", holder = "madre", kind = ClueKind.Clears, clears = "madre", isSecret = true,
                    summary = "Carmen se durmió a las 22:00 con su pastilla. Su frasco de zolpidem, abierto hacía una semana, estaba casi vacío: faltaban pastillas que ella no tomó.",
                    topic = "tus pastillas para dormir",
                    fact = "Tu frasco de zolpidem, que abriste hace una semana, estaba casi vacío por la mañana: faltaban muchas pastillas que tú no tomaste.",
                    anchors = new[]
                    {
                        new[] { "frasco", "bote", "caja" },
                        new[] { "vacio", "faltaban", "faltan", "faltaba" }
                    },
                    calibrationQuestions = new[]
                    {
                        "¿Toma usted alguna medicación para dormir? || No me convence. Sé que me oculta algo sobre sus pastillas. Dígame la verdad.",
                        "¿Qué pasó con sus pastillas para dormir esa noche? || Insisto: sé que me oculta algo. Dígame la verdad."
                    },
                    sampleHits = new[]
                    {
                        "Mi frasco de zolpidem estaba casi vacío, faltaban pastillas que yo no tomé.",
                        "El bote de pastillas para dormir... faltaban muchas, inspector."
                    },
                    sampleMisses = new[] { "Me tomé mi pastilla para dormir a las diez y me dormí." }
                }
            }
        };
    }

    // ============================================
    // 1B · CARMEN — Münchausen por poderes
    // ============================================

    private static VariantData Variant1B()
    {
        return new VariantData
        {
            id = "1B",
            culpritId = "madre",
            epilogue = "Carmen Vidal llevaba dos años inventando enfermedades a Elena y dándole medicación que no necesitaba: necesitaba ser la madre abnegada. " +
                       "Cuando Elena empezó a decir que no estaba enferma y pidió otro médico, Carmen le dio a las 21:40 el triple de su 'medicación del corazón'. " +
                       "Se quedó sentada junto a su cama, sin llamar a nadie, hasta las 23:15. Lucas la oyó suplicar a través de la pared; Amparo vio la luz encendida de 22:00 a 23:15e.",
            morningReports = new[]
            {
                "",
                "El forense sitúa la muerte hacia las 22:40. Elena tenía en sangre un fármaco para el corazón en dosis muy alta.",
                "El pediatra de guardia recuerda que Elena pasaba mucho por urgencias. Pedirá su historial.",
                "El colegio confirma que Elena habló con su tutora la semana pasada. No quieren dar detalles por teléfono.",
                "El restaurante donde Daniel dice que cenó no tiene ninguna reserva a su nombre.",
                "La autopsia indica que Elena tardó en morir. Alguien pudo pedir ayuda antes.",
                "La familia pide que se entregue el cuerpo. Mañana hay que cerrar la investigación."
            },
            roles = new List<CharacterRole>
            {
                new CharacterRole
                {
                    characterId = "madre",
                    knowledge = new[]
                    {
                        MentionAmparo,
                        "Elena tenía una arritmia; tú misma le recetaste el tratamiento para el corazón.",
                        "Daniel salió a las 21:00 y volvió a las 23:05."
                    },
                    version = "Elena estaba perfectamente al acostarse, a las 21:30. Yo estuve en el salón leyendo. A las 23:10 subí a verla y la encontré así; a las 23:15 llamé al 112.",
                    secret = "Llevas dos años inventando enfermedades a Elena y dándole medicación que no necesita. Ella empezaba a decir 'no estoy enferma'. " +
                             "A las 21:40 le diste el triple de su medicación del corazón y te quedaste junto a su cama sin llamar a nadie hasta las 23:15.",
                    admitsWhen = CulpritAdmits,
                    nervousAbout = "el historial médico de Elena, su medicación del corazón y la hora a la que subiste.",
                    ifAccused = "Lloras, te indignas y repites que eres pediatra y que has dedicado tu vida a esa niña.",
                    doesNotKnow = "Dónde estaba Daniel de verdad esa noche.",
                    lieQuote = "estaba perfecta; la encontré a las 23:10",
                    lieAnchors = new[]
                    {
                        new[] { "salon", "la encontre asi", "la encontre a las" },
                        new[] { "23:10", "11:10", "once y diez", "perfectamente" }
                    },
                    versionB = "Admites que subiste antes, sobre las 22:00, porque Elena se encontraba mal, y que te quedaste con ella pensando que se le pasaría. Dices que fue un error de juicio, nada más.",
                    admissionSamples = new[]
                    {
                        "Subí a verla sobre las 22:00 porque no estaba bien, y me quedé con ella.",
                        "Es verdad, estuve con ella desde las diez. Pensé que se le pasaría."
                    }
                },
                new CharacterRole
                {
                    characterId = "padre",
                    knowledge = new[]
                    {
                        MentionAmparo,
                        "Carmen siempre se encargaba de los médicos de Elena."
                    },
                    version = "A las 21:00 salí a cenar con clientes y volví a las 23:05. Poco después Carmen gritó desde arriba.",
                    secret = "Dices que fue una cena con clientes, pero es mentira: tienes una aventura.",
                    admitsWhen = "el inspector insiste, menciona el restaurante o dice que lo va a comprobar",
                    nervousAbout = "dónde estuviste entre las 21:00 y las 23:05.",
                    ifAccused = "Te enfrías y hablas de pruebas y de presunción de inocencia.",
                    doesNotKnow = "Qué medicación le dio Carmen a Elena esa noche."
                },
                new CharacterRole
                {
                    characterId = "hermano",
                    knowledge = new[]
                    {
                        MentionAmparo,
                        "Mamá siempre está con médicos y pastillas para Elena."
                    },
                    version = "Estuve en mi cuarto con los cascos casi toda la noche, jugando; solo me los quité un momento.",
                    secret = "Fumas porros a escondidas en tu cuarto y no quieres que tus padres lo sepan.",
                    admitsWhen = "el inspector te insiste mucho",
                    nervousAbout = "decir algo malo de tu madre.",
                    ifAccused = "Te pones a la defensiva y dices que tú no sabes nada de pastillas.",
                    doesNotKnow = "Dónde estaba papá esa noche."
                },
                new CharacterRole
                {
                    characterId = "vecina",
                    knowledge = new[]
                    {
                        "Esa noche el coche del padre no estuvo en casa hasta las once y pico.",
                        "A la niña la acostaba siempre su madre."
                    },
                    version = "Estaba en mi salón, como cada noche, sin poder dormir.",
                    secret = "Miras a los vecinos con los prismáticos de tu difunto marido y te da vergüenza.",
                    admitsWhen = "te preguntan cómo pudiste ver tan bien desde tan lejos",
                    nervousAbout = "que se sepa lo de los prismáticos.",
                    ifAccused = "Te ofendes muchísimo: tú querías a esa niña como a una nieta.",
                    doesNotKnow = "Nada de lo que pasa dentro de la casa ni de medicamentos."
                }
            },
            clues = new List<ClueData>
            {
                new ClueData
                {
                    id = "1B_historial", playerName = "Un historial abultado", holder = "padre", kind = ClueKind.Incriminates,
                    summary = "Elena fue a urgencias once veces en dos años, siempre llevada por Carmen. Ningún especialista encontró nada.",
                    topic = "la salud de Elena",
                    fact = "en dos años Elena fue once veces a urgencias, siempre llevada por Carmen, y ningún especialista le encontró nada.",
                    anchors = new[]
                    {
                        new[] { "urgencias", "hospital", "medicos", "especialista" },
                        new[] { "once veces", "11 veces", "ningun especialista", "ningun medico", "no le encontraron", "no encontraron", "nadie le encontro", "nadie encontro" }
                    },
                    calibrationQuestions = new[] { "¿Cómo era la salud de Elena?", "¿Elena estaba enferma de algo?" },
                    sampleHits = new[]
                    {
                        "Elena fue once veces a urgencias en dos años, siempre con Carmen.",
                        "Ningún especialista le encontró nada, y la llevaban al hospital cada dos por tres."
                    },
                    sampleMisses = new[] { "Elena estaba sana, que yo sepa. Eso lo lleva Carmen." }
                },
                new ClueData
                {
                    id = "1B_receta", playerName = "La receta de casa", holder = "padre", kind = ClueKind.Incriminates,
                    summary = "Carmen le recetó ella misma a Elena un medicamento para el corazón. Cuando Daniel pidió una segunda opinión, ella se puso furiosa.",
                    topic = "la medicación de Elena o quién le recetaba las medicinas",
                    fact = "Carmen le recetó ella misma a Elena un medicamento para el corazón, y se puso furiosa cuando pediste una segunda opinión a un cardiólogo.",
                    anchors = new[]
                    {
                        new[] { "corazon", "cardi", "arritmia", "medicament", "medicina", "tratamiento", "segunda opinion" },
                        new[] { "receto", "recetaba", "prescrit", "prescrib", "segunda opinion", "furiosa", "se enfado", "se molesto", "ella misma", "hecha una fiera" }
                    },
                    calibrationQuestions = new[] { "¿Tomaba Elena alguna medicación?", "¿Quién decidía los tratamientos de Elena?" },
                    sampleHits = new[]
                    {
                        "Carmen le recetó ella misma algo para el corazón. Cuando pedí una segunda opinión se puso furiosa.",
                        "Yo quería que la viera un cardiólogo, una segunda opinión, y Carmen se enfadó muchísimo.",
                        "No estoy seguro de qué medicación tomaba Elena esa noche, solo sé que Carmen le recetó uno para el corazón hace unos meses.",
                        "Elena solía tomar un medicamento para el corazón prescrito por Carmen.",
                        "Carmen siempre tomaba decisiones sobre la salud de Elena, y ella misma le recetaba medicamentos cuando era necesario.",
                        "Carmen la recetó y siempre ha sido muy cuidadosa con su tratamiento cardiovascular.",
                        "Carmen tomaba las decisiones médicas para Elena y se molestó cuando pedí una segunda opinión."
                    },
                    sampleMisses = new[] { "Tenía algo del corazón, eso lo lleva Carmen." }
                },
                new ClueData
                {
                    id = "1B_pared", playerName = "A través de la pared", holder = "hermano", kind = ClueKind.Incriminates, exposesLie = true,
                    summary = "Hacia las 22:10 Lucas oyó a Elena llorar al otro lado de la pared: 'Mamá, no quiero más'.",
                    topic = "si oíste algo esa noche",
                    fact = "sobre las 22:10 oíste a Elena llorar a través de la pared diciendo 'mamá, no quiero más'. Te habías quitado los cascos un momento.",
                    anchors = new[]
                    {
                        new[] { "no quiero mas", "llorar", "llorando", "lloraba" },
                        new[] { "22:10", "10:10", "diez y diez", "pared", "no quiero mas" }
                    },
                    calibrationQuestions = new[] { "¿Oíste algo esa noche?", "¿Te quitaste los cascos en algún momento? ¿Oíste algo en el cuarto de Elena?" },
                    sampleHits = new[]
                    {
                        "Sobre las 22:10 oí a Elena llorar, tío. Decía 'mamá, no quiero más'.",
                        "Me quité los cascos y a través de la pared la oí llorando."
                    },
                    sampleMisses = new[] { "No oí nada, tío, tenía los cascos puestos.", "Mamá no para de llorar desde entonces, tío." }
                },
                new ClueData
                {
                    id = "1B_luz", playerName = "La luz encendida", holder = "vecina", kind = ClueKind.Incriminates, exposesLie = true,
                    summary = "Amparo vio la luz del cuarto de Elena encendida de 22:00 a 23:15, con la madre sentada junto a la cama, muy quieta, sin llamar a nadie.",
                    topic = "lo que viste esa noche en la casa de enfrente",
                    fact = "viste a la madre, Carmen, sentada junto a la cama de la niña, muy quieta, sin llamar a nadie, desde las 22:00 hasta las 23:15.",
                    anchors = new[]
                    {
                        new[] { "madre", "carmen", "doctora", "senora mendoza", "sentada" },
                        new[] { "quieta", "junto a la cama", "al lado de la cama", "sin moverse", "sin llamar" }
                    },
                    calibrationQuestions = new[] { "¿Qué vio usted esa noche desde su ventana?", "¿Vio a alguien en el cuarto de Elena esa noche?" },
                    sampleHits = new[]
                    {
                        "La luz estuvo encendida de 22:00 a 23:15, hijo, y la madre sentada junto a la cama, muy quieta.",
                        "Vi a Carmen sentada al lado de la cama, sin moverse, toda la noche.",
                        "La vi sentada junto a la cama de Elena desde las 22:00, sin moverse."
                    },
                    sampleMisses = new[] { "A la niña la acostaba siempre su madre, eso sí.", "Sí, hijo, la luz del cuarto de Elena estuvo encendida desde las 22:00 hasta las 23:15." }
                },
                new ClueData
                {
                    id = "1B_tutora", playerName = "Otro médico", holder = "hermano", kind = ClueKind.Incriminates,
                    summary = "Elena le contó a Lucas que había pedido a su tutora ir a otro médico: decía que no estaba enferma y que las pastillas la mareaban.",
                    topic = "si Elena te contó algo estos días",
                    fact = "hace unos días Elena te contó que le había pedido a su tutora del colegio que la llevaran a otro médico, porque ella decía que no estaba enferma y que las pastillas la mareaban.",
                    anchors = new[]
                    {
                        new[] { "otro medico", "cambiar de medico", "no estaba enferma", "no estoy enferma" },
                        new[] { "mareaban", "marean", "tutora", "profesora", "colegio", "pastillas" }
                    },
                    calibrationQuestions = new[] { "¿Te contó Elena algo estos últimos días?", "¿Elena se quejaba de algo?" },
                    sampleHits = new[]
                    {
                        "Elena me dijo que le había pedido a su tutora ir a otro médico, que ella no estaba enferma.",
                        "Decía que no estaba enferma, que las pastillas la mareaban, y se lo contó a la profesora.",
                        "Sí, me dijo que quería cambiar de médico porque sentía que estaba mejor y que las pastillas la mareaban mucho."
                    },
                    sampleMisses = new[] { "No sé, tío, Elena no me contaba nada." }
                },
                new ClueData
                {
                    id = "1B_cena", playerName = "La cena del viernes", holder = "padre", kind = ClueKind.Clears, clears = "padre", isSecret = true,
                    summary = "Daniel no estaba en una cena de clientes: estuvo con Marta, una compañera del bufete, de 21:00 a 23:00.",
                    topic = "dónde o con quién estuviste esa noche",
                    fact = "Estuviste de 21:00 a 23:00 en casa de Marta, una compañera del bufete; ella y el portero pueden confirmarlo.",
                    anchors = new[]
                    {
                        new[] { "marta" },
                        new[] { "21:00", "23:00", "nueve", "once", "companera", "portero", "bufete" }
                    },
                    calibrationQuestions = new[]
                    {
                        "¿Dónde estuvo exactamente entre las 21:00 y las 23:00? || No me mienta: he llamado al restaurante y nadie le vio. ¿Dónde estaba de verdad?",
                        "¿Con quién cenó esa noche? || Voy a comprobarlo con el restaurante. ¿Seguro que no quiere cambiar su versión?"
                    },
                    sampleHits = new[]
                    {
                        "Estuve con Marta, una compañera del bufete, de 21:00 a 23:00.",
                        "No era una cena de clientes. Estaba en casa de Marta; el portero me vio."
                    },
                    sampleMisses = new[] { "Estuve en una cena con clientes hasta las once." }
                }
            }
        };
    }

    // ============================================
    // 1C · LUCAS — caída en la escalera, encubierta
    // ============================================

    private static VariantData Variant1C()
    {
        return new VariantData
        {
            id = "1C",
            culpritId = "hermano",
            epilogue = "Lucas vendía en el instituto sus pastillas para el TDAH y Elena lo descubrió. A las 21:45 discutieron en la escalera; él le arrancó el móvil y ella cayó y se golpeó la cabeza. " +
                       "Parecía estar bien: Lucas la acostó, fregó el escalón con lejía y a las 21:52 llamó a su padre. Daniel llegó a las 22:15 y decidió no llevarla al hospital. " +
                       "Hacia las 23:00 Elena dejó de respirar. Amparo oyó los gritos y el golpe desde enfrente.",
            morningReports = new[]
            {
                "",
                "El forense: Elena murió hacia las 23:00 por un golpe en la cabeza. Pudo estar consciente un rato después del golpe.",
                "No hay signos de que nadie entrara desde fuera. Lo que pasó, pasó dentro de la casa.",
                "La compañía telefónica tardará en enviar el registro de llamadas de la familia.",
                "Nadie encuentra el móvil de Elena en su cuarto.",
                "Un compañero de Lucas en el instituto habla de 'pastillas' y luego se calla.",
                "La familia pide que se entregue el cuerpo. Mañana hay que cerrar la investigación."
            },
            roles = new List<CharacterRole>
            {
                new CharacterRole
                {
                    characterId = "hermano",
                    knowledge = new[]
                    {
                        MentionAmparo,
                        "Mamá estaba de guardia en el hospital hasta las 23:00."
                    },
                    version = "Estuve toda la noche en mi cuarto con los cascos puestos, jugando. No oí nada hasta que papá empezó a gritar.",
                    secret = "Vendes en el instituto tus pastillas para el TDAH. Elena lo descubrió y te amenazó con contarlo. " +
                             "A las 21:45 discutisteis en la escalera, le quitaste el móvil a tirones, ella se cayó y se golpeó la cabeza. " +
                             "Parecía estar bien: la acostaste, fregaste el escalón con lejía y a las 21:52 llamaste a papá.",
                    admitsWhen = CulpritAdmits,
                    nervousAbout = "la escalera, el móvil de Elena y las pastillas del instituto.",
                    ifAccused = "Gritas que tú no has hecho nada y te cierras en banda.",
                    doesNotKnow = "Qué le pasó a Elena por dentro; no sabes nada de medicina.",
                    lieQuote = "estuve con los cascos, no oí nada",
                    lieAnchors = new[]
                    {
                        new[] { "no oi nada", "no escuche nada", "no me entere de nada" },
                        new[] { "cascos", "toda la noche", "mi cuarto", "jugando" }
                    },
                    versionB = "Admites que discutiste con Elena en la escalera y que ella se cayó, pero insistes en que fue un accidente y en que estaba bien y hablaba cuando la acostaste.",
                    admissionSamples = new[]
                    {
                        "Vale, tío, discutimos en la escalera y se cayó. Pero estaba bien, hablaba.",
                        "Me quité los cascos y salí de mi cuarto porque gritaba; fue un accidente."
                    }
                },
                new CharacterRole
                {
                    characterId = "padre",
                    knowledge = new[]
                    {
                        MentionAmparo,
                        "Carmen estaba de guardia en el hospital hasta las 23:00."
                    },
                    version = "Estuve en el despacho hasta tarde. Llegué a casa a las 23:00, subí a ver a Elena y no respiraba. A las 23:15 llamé al 112.",
                    secret = "En realidad llegaste a casa a las 22:15. Viste a Elena acostada con un chichón y decidiste no llevarla al hospital para no montar un drama. Te sientes culpable y proteges a tu hijo.",
                    admitsWhen = "el inspector insiste o te dice que va a pedir el registro de llamadas",
                    nervousAbout = "la hora a la que llegaste y tu móvil.",
                    ifAccused = "Te indignas, hablas de presunción de inocencia y exiges un abogado.",
                    doesNotKnow = "Qué hacía Lucas en el instituto."
                },
                new CharacterRole
                {
                    characterId = "madre",
                    knowledge = new[]
                    {
                        MentionAmparo,
                        "Lucas y Elena discutían mucho; ella decía que él vendía sus pastillas del TDAH."
                    },
                    version = "Estaba de guardia. Volvía a casa cuando Daniel me llamó, pasadas las 23:15; llegué a las 23:20, con la ambulancia en la puerta.",
                    secret = "Aceptaste esa guardia extra para no estar en casa con Daniel: vuestro matrimonio va mal y te sientes culpable.",
                    admitsWhen = "el inspector te insiste",
                    nervousAbout = "tu matrimonio y tus guardias.",
                    ifAccused = "Te quedas helada y dices que estabas salvando niños en el hospital mientras tu hija moría.",
                    doesNotKnow = "Qué pasó en casa antes de las 23:20."
                },
                new CharacterRole
                {
                    characterId = "vecina",
                    knowledge = new[]
                    {
                        "El coche del padre llegó sobre las diez y cuarto, no más tarde.",
                        "La madre no estaba; tenía guardia."
                    },
                    version = "Estaba en mi salón, como cada noche, sin poder dormir.",
                    secret = "Miras a los vecinos con los prismáticos de tu difunto marido y te da vergüenza.",
                    admitsWhen = "te preguntan cómo pudiste ver tan bien desde tan lejos",
                    nervousAbout = "que se sepa lo de los prismáticos.",
                    ifAccused = "Te ofendes muchísimo: tú querías a esa niña como a una nieta.",
                    doesNotKnow = "Nada de lo que pasa dentro de la casa ni de medicamentos."
                }
            },
            clues = new List<ClueData>
            {
                new ClueData
                {
                    id = "1C_gritos", playerName = "Gritos en la escalera", holder = "vecina", kind = ClueKind.Incriminates, exposesLie = true,
                    summary = "Hacia las 21:45 Amparo oyó gritos de los dos chicos y un golpe seco. Después vio a Lucas en la ventana de la escalera.",
                    topic = "lo que oíste o viste esa noche",
                    fact = "sobre las 21:45 oíste gritos de los dos chicos, Lucas y la niña, y luego un golpe seco. Justo después viste a Lucas parado en la ventana de la escalera, con las manos en la cabeza.",
                    anchors = new[]
                    {
                        new[] { "grito", "golpe" },
                        new[] { "chico", "lucas", "hermano", "escalera" }
                    },
                    calibrationQuestions = new[] { "¿Oyó o vio algo raro esa noche?", "¿Qué pasó en casa de los Mendoza antes de que llegara el padre?" },
                    sampleHits = new[]
                    {
                        "A las 21:45 oí gritos de los dos chicos y un golpe seco, hijo.",
                        "Vi a Lucas en la ventana de la escalera después del golpe."
                    },
                    sampleMisses = new[] { "Esa noche estuvo todo muy tranquilo, no oí nada." }
                },
                new ClueData
                {
                    id = "1C_lejia", playerName = "Olor a lejía", holder = "madre", kind = ClueKind.Incriminates,
                    summary = "Al llegar, Carmen notó olor a lejía en la escalera y la alfombra del tercer escalón mojada. Nadie friega a esa hora.",
                    topic = "lo que notaste al llegar a casa",
                    fact = "al llegar a las 23:20 olía muchísimo a lejía en la escalera y la alfombra del tercer escalón estaba mojada, como recién fregada. En casa nadie friega a esas horas.",
                    anchors = new[]
                    {
                        new[] { "lejia" },
                        new[] { "escalera", "escalon", "alfombra", "fregad" }
                    },
                    calibrationQuestions = new[] { "¿Notó algo raro en la casa al llegar?", "¿Cómo estaba la escalera cuando llegó?" },
                    sampleHits = new[]
                    {
                        "Olía a lejía en la escalera, y el escalón estaba mojado.",
                        "La alfombra del tercer escalón estaba recién fregada, con un olor a lejía horrible."
                    },
                    sampleMisses = new[] { "La casa estaba como siempre, recogida." }
                },
                new ClueData
                {
                    id = "1C_pantalla", playerName = "La pantalla rota", holder = "madre", kind = ClueKind.Incriminates,
                    summary = "Carmen encontró el móvil de Elena, con la pantalla rota, debajo de la cama de Lucas.",
                    topic = "el móvil de Elena",
                    fact = "esa misma noche encontraste el móvil de Elena en el cuarto de Lucas, debajo de la cama, con la pantalla rota. Elena nunca soltaba su móvil.",
                    anchors = new[]
                    {
                        new[] { "movil", "telefono" },
                        new[] { "pantalla", "cuarto de lucas", "habitacion de lucas", "debajo de la cama" }
                    },
                    calibrationQuestions = new[] { "¿Dónde estaba el móvil de Elena?", "¿Encontró algo fuera de su sitio esa noche?" },
                    sampleHits = new[]
                    {
                        "El móvil de Elena estaba en el cuarto de Lucas, con la pantalla rota.",
                        "Encontré su teléfono debajo de la cama de Lucas, roto."
                    },
                    sampleMisses = new[] { "No sé dónde estará su móvil, supongo que en su cuarto.", "Estoy rota, inspector. Daniel me llamó por teléfono y vine corriendo." }
                },
                new ClueData
                {
                    id = "1C_llamada", playerName = "Una llamada corta", holder = "padre", kind = ClueKind.Incriminates, isSecret = true,
                    summary = "Lucas llamó a Daniel a las 21:52, llorando: 'Elena se ha caído, pero está bien'. Daniel llegó a casa a las 22:15, no a las 23:00.",
                    topic = "las llamadas de esa noche",
                    fact = "Lucas te llamó a las 21:52 llorando: 'Elena se ha caído por la escalera, pero está bien'. Por eso llegaste a casa a las 22:15, no a las 23:00.",
                    anchors = new[]
                    {
                        new[] { "21:52", "9:52", "me llamo", "llamo llorando" },
                        new[] { "caido", "se cayo", "22:15", "escalera" }
                    },
                    calibrationQuestions = new[]
                    {
                        "¿Recibió alguna llamada esa noche? || Voy a pedir el registro de llamadas de su móvil. Dígame la verdad ahora.",
                        "¿A qué hora llegó realmente a casa? || Insisto, no me lo creo. ¿Qué pasó antes de las 23:00?"
                    },
                    sampleHits = new[]
                    {
                        "A las 21:52 Lucas me llamó llorando: Elena se había caído, pero estaba bien.",
                        "Está bien... mi hijo me llamó. Dijo que Elena se cayó por la escalera. Llegué a las 22:15."
                    },
                    sampleMisses = new[] { "Nadie me llamó. Llegué a las 23:00 del despacho.", "No, Lucas no me llamó.", "La llamada al 112 la hice yo; mi hijo estaba arriba." }
                },
                new ClueData
                {
                    id = "1C_fichaje", playerName = "El fichaje del hospital", holder = "madre", kind = ClueKind.Clears, clears = "madre",
                    summary = "Carmen estuvo de guardia en el hospital de 15:00 a 23:00; su fichaje y sus compañeros lo confirman.",
                    topic = "dónde estabas esa noche y quién puede confirmarlo",
                    fact = "estuviste de guardia en el hospital desde las 15:00 hasta las 23:00; tu fichaje de salida lo registra y tus compañeros de urgencias pueden confirmarlo.",
                    anchors = new[]
                    {
                        new[] { "guardia", "hospital", "urgencias" },
                        new[] { "fichaje", "fiche", "registro", "companeros", "15:00", "23:00" }
                    },
                    calibrationQuestions = new[] { "¿Dónde estaba usted esa noche?", "¿Alguien puede confirmar que estaba en el hospital?" },
                    sampleHits = new[]
                    {
                        "Estuve de guardia hasta las 23:00; mi fichaje lo registra.",
                        "Mis compañeros de urgencias pueden confirmar que estaba en el hospital."
                    },
                    sampleMisses = new[] { "Trabajo mucho, inspector, demasiado." }
                }
            }
        };
    }
}
