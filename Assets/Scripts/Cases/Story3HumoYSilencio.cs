using System.Collections.Generic;

/// <summary>
/// HISTORIA 3 · "Humo y Silencio"
/// Finca Los Olivares, Jaén, octubre. Paula Romero Navarro (15) desaparece el fin de semana que pasa con su padre.
/// Divorcio reciente y vista de custodia el lunes. Humo negro el sábado; denuncia el domingo a las 22:30.
/// 3A: Javier (la golpea tras discutir) · 3B: Lucía (se la lleva viva a Portugal y simula un crimen) · 3C: Encarna (obsesión)
/// </summary>
public static class Story3HumoYSilencio
{
    private const string MentionAlex = "Álex, el hermano de Paula, es con quien ella más habla.";
    private const string MentionEncarna = "Encarna, la vecina de la finca de al lado, lo ve todo desde su casa.";

    private const string CulpritAdmits =
        "te enseñan una prueba concreta, y aun así solo admites lo que esa prueba demuestra";

    private const string FirstReport = "La Guardia Civil encuentra en el quemadero restos de una mochila y de unas zapatillas de chica.";
    private const string LastDay = "La jueza aplaza la vista de custodia. Mañana tienes que acusar a alguien.";

    public static StoryData Build()
    {
        return new StoryData
        {
            id = "3",
            title = "Humo y Silencio",
            victim = "Paula",
            place = "Finca Los Olivares, Jaén. Octubre.",
            victimSummary = "Paula Romero Navarro, 15 años. Pasaba el fin de semana con su padre en la finca.",
            situation = "Divorcio reciente y vista de custodia el lunes. El sábado salió humo negro del quemadero; el domingo a las 22:30 su padre denunció la desaparición.",
            intro = @"Finca Los Olivares, Jaén. Octubre.

Paula Romero Navarro, 15 años, pasa el fin de semana con su padre en la finca. Sus padres se divorciaron hace tres meses y el lunes hay vista de custodia.

El sábado por la noche, desde la finca de al lado, se ve salir humo negro del quemadero.

El domingo a las 22:30, Javier denuncia que Paula ha desaparecido.

Tienes 7 días para descubrir qué pasó.",
            caseBrief = "Paula Romero Navarro, 15 años, desapareció el fin de semana que pasaba con su padre en la finca Los Olivares (Jaén). " +
                        "El sábado salió humo negro del quemadero; el domingo a las 22:30 su padre denunció la desaparición. " +
                        "El lunes había vista de custodia.",
            cast = new List<CharacterData>
            {
                new CharacterData
                {
                    id = "padre", name = "Javier Romero", shortName = "Javier", artId = "javier", roleLabel = "padre", portraitKey = "Padre",
                    identity = "Eres Javier Romero, 44 años, olivarero, dueño de la finca Los Olivares. Divorciado de Lucía hace tres meses; el lunes hay vista de custodia de Paula.",
                    speech = "Amargado y a la defensiva; pasas de hacerte la víctima a enfadarte. Hablas mal de tu ex.",
                    speechExample = "Claro, ahora el malo soy yo. Como siempre.",
                    startsUnlocked = true,
                    mentionAliases = new[] { "javier", "padre" }
                },
                new CharacterData
                {
                    id = "madre", name = "Lucía Navarro", shortName = "Lucía", artId = "lucia", roleLabel = "madre", portraitKey = "Madre",
                    identity = "Eres Lucía Navarro, 41 años, profesora de instituto en Granada. Madre de Paula y de Álex. Estás en tratamiento por depresión.",
                    speech = "Contenida y firme, con frases cortas. Cuando te rompes, hablas muy bajo.",
                    speechExample = "Paula es lo único que me importa. Lo único.",
                    startsUnlocked = true,
                    mentionAliases = new[] { "lucia", "madre" }
                },
                new CharacterData
                {
                    id = "hermano", name = "Álex Romero", shortName = "Álex", artId = "alex", roleLabel = "hermano", portraitKey = "Hermano",
                    identity = "Eres Álex Romero, 17 años, hermano de Paula. Vives con tu madre en Granada; Paula te lo cuenta todo.",
                    speech = "Seco y protector; respuestas cortas, a veces cortantes. Eres serio para tu edad.",
                    speechExample = "Pregúntame lo que quieras. Pero rápido.",
                    startsUnlocked = false,
                    mentionAliases = new[] { "alex", "hermano" }
                },
                new CharacterData
                {
                    id = "vecina", name = "Encarna Molina", shortName = "Encarna", artId = "encarna", roleLabel = "vecina", portraitKey = "Vecina",
                    identity = "Eres Encarna Molina, 63 años, viuda, dueña de la finca de al lado de Los Olivares. Perdiste a tu hija Rocío en 1998, con 15 años. Tienes caballos.",
                    speech = "Andaluza, piadosa y dulce; hablas de la Virgen y de tu Rocío en presente. Llamas 'hijo' al inspector.",
                    speechExample = "Ay, hijo, qué cosas más malas pasan. Que la Virgen la proteja.",
                    startsUnlocked = false,
                    mentionAliases = new[] { "encarna", "vecina", "finca de al lado" }
                }
            },
            variants = new List<VariantData> { Variant3A(), Variant3B(), Variant3C() }
        };
    }

    private static CharacterRole InnocentEncarna(string version, params string[] knowledge)
    {
        return new CharacterRole
        {
            characterId = "vecina",
            knowledge = knowledge,
            version = version,
            secret = "Tienes un pleito con Javier por el agua del pozo y le guardas rencor; temes que parezca que le acusas por eso.",
            admitsWhen = "el inspector insiste",
            nervousAbout = "el pleito del pozo.",
            ifAccused = "Te santiguas y dices que tú no harías daño ni a una mosca.",
            doesNotKnow = "Qué pasó dentro de la casa de Javier."
        };
    }

    // ============================================
    // 3A · JAVIER — la discusión del sábado
    // ============================================

    private static VariantData Variant3A()
    {
        return new VariantData
        {
            id = "3A",
            culpritId = "padre",
            epilogue = "Paula iba a contarle a la jueza el lunes que su padre bebía y la amenazaba. El sábado a las 20:30 discutieron; a las 20:40 ella escribió a Álex " +
                       "pidiéndole que fuera a por ella. A las 20:50 Javier la golpeó y no volvió a levantarse. A las 21:00 quemó sus cosas en el quemadero con neumáticos " +
                       "y a las 21:30 salió a por garrafas de gasoil. Encarna lo vio todo desde su casa.",
            morningReports = new[]
            {
                "",
                FirstReport,
                "El laboratorio encuentra restos de neumático y gasoil en la hoguera.",
                "La gasolinera del pueblo vendió gasoil en garrafas el sábado a las 21:40.",
                "El juzgado confirma que Paula iba a declarar en la vista de custodia del lunes.",
                "Los perros de la Guardia Civil marcan un punto en el olivar, cerca del quemadero.",
                LastDay
            },
            roles = new List<CharacterRole>
            {
                new CharacterRole
                {
                    characterId = "padre",
                    knowledge = new[] { MentionAlex, MentionEncarna },
                    version = "Paula cenó conmigo tan tranquila y se acostó a las diez. El domingo por la mañana ya no estaba.",
                    secret = "Paula iba a contarle a la jueza que bebes y la amenazas. A las 20:30 discutisteis y a las 20:50 la golpeaste; no volvió a levantarse. " +
                             "Luego quemaste sus cosas y saliste a por gasoil.",
                    admitsWhen = CulpritAdmits,
                    nervousAbout = "la bebida, los audios a tu ex, el quemadero y el sábado por la noche.",
                    ifAccused = "Estallas: gritas que es tu hija y que su madre os ha puesto a todos en contra.",
                    doesNotKnow = "Qué vio Encarna desde su casa.",
                    lieQuote = "cenó tranquila y se acostó a las diez",
                    lieAnchors = new[]
                    {
                        new[] { "tranquila", "se acosto a las diez", "se fue a dormir a las diez" },
                        new[] { "ceno", "cenamos", "a las diez", "22:00" }
                    },
                    versionB = "Admites que discutisteis a las 20:30 porque ella quería vivir con su madre, pero juras que luego se calmó y se encerró en su cuarto.",
                    admissionSamples = new[]
                    {
                        "Discutimos un poco en la cena, sí. Estaba enfadada conmigo.",
                        "Vale, a las ocho y media tuvimos una bronca, pero luego se le pasó y se encerró en su cuarto."
                    }
                },
                new CharacterRole
                {
                    characterId = "madre",
                    knowledge = new[] { MentionAlex, MentionEncarna, "Paula quiere vivir contigo y lo iba a decir en la vista del lunes." },
                    version = "El sábado estuve todo el día en Granada, en casa de mi hermana, con Álex.",
                    secret = "Pensabas mudarte a Madrid con Paula después de la vista sin decírselo al juzgado.",
                    admitsWhen = "el inspector insiste",
                    nervousAbout = "Madrid y tu medicación.",
                    ifAccused = "Te quedas helada y dices, muy bajo, que eres su madre.",
                    doesNotKnow = "Qué pasó en la finca el sábado."
                },
                new CharacterRole
                {
                    characterId = "hermano",
                    knowledge = new[] { "Paula no quería ir a la finca ese fin de semana." },
                    version = "El sábado no pisé la finca. Paula estaba con mi padre.",
                    secret = "Paula te pidió ayuda y no fuiste porque habías bebido en una fiesta; no te lo perdonas.",
                    admitsWhen = "el inspector insiste",
                    nervousAbout = "por qué no fuiste a por Paula.",
                    ifAccused = "Aprietas los puños y te callas.",
                    doesNotKnow = "Qué pasó en la finca después de las nueve."
                },
                InnocentEncarna("El sábado estuve en casa toda la tarde; desde mi ventana se ve la finca de Javier.",
                    "Javier bebe mucho desde el divorcio.", "Paula venía a ver tus caballos cuando estaba en la finca.")
            },
            clues = new List<ClueData>
            {
                new ClueData
                {
                    id = "3A_mensaje", playerName = "El último mensaje", holder = "hermano", kind = ClueKind.Incriminates, exposesLie = true,
                    summary = "A las 20:40 Paula le escribió a Álex: 'Papá está fatal, ha bebido, ven a por mí'.",
                    topic = "la última vez que supiste de Paula",
                    fact = "a las 20:40 Paula te escribió por WhatsApp: 'Papá está fatal, ha bebido, ven a por mí'. Fue su último mensaje.",
                    anchors = new[]
                    {
                        new[] { "ven a por mi", "esta fatal", "ha bebido", "habia bebido", "estaba mal", "recogiera", "a buscarla", "a por ella" },
                        new[] { "20:40", "8:40", "mensaje", "whatsapp", "me escribio" }
                    },
                    calibrationQuestions = new[] { "¿Cuándo supiste de Paula por última vez?", "¿Te escribió Paula el sábado?" },
                    sampleHits = new[]
                    {
                        "A las 20:40 me escribió: papá está fatal, ha bebido, ven a por mí.",
                        "Su último mensaje fue por WhatsApp: que papá había bebido y que fuera a por ella.",
                        "A las 20:40 me envió un mensaje diciendo que su padre estaba mal y que yo debía ir a buscarla."
                    },
                    sampleMisses = new[] { "Paula no me escribió nada ese día." }
                },
                new ClueData
                {
                    id = "3A_humo", playerName = "Humo negro", holder = "vecina", kind = ClueKind.Incriminates,
                    summary = "Sobre las 21:00 del sábado salió humo negro del quemadero de Javier. Olía a goma quemada y a algo dulzón.",
                    topic = "lo que viste u oliste el sábado por la noche",
                    fact = "sobre las 21:00 del sábado salió un humo negro y espeso del quemadero de Javier; olía a goma quemada y a algo dulzón.",
                    anchors = new[]
                    {
                        new[] { "humo" },
                        new[] { "negro", "goma", "dulzon", "21:00", "nueve" }
                    },
                    calibrationQuestions = new[] { "¿Vio algo raro en la finca de Javier el sábado?", "¿Qué me puede contar del humo del sábado?" },
                    sampleHits = new[]
                    {
                        "Sobre las nueve salió un humo negro del quemadero, hijo, olía a goma.",
                        "Un humo muy negro, que olía a goma quemada y a algo dulzón."
                    },
                    sampleMisses = new[] { "No vi humo ninguno ese día.", "Por aquí se queman rastrojos a menudo, hijo." }
                },
                new ClueData
                {
                    id = "3A_garrafas", playerName = "Viaje nocturno", holder = "vecina", kind = ClueKind.Incriminates,
                    summary = "A las 21:30 la camioneta de Javier salió de la finca y volvió a las 21:50 cargada de garrafas.",
                    topic = "si alguien salió o entró de la finca de Javier esa noche",
                    fact = "a las 21:30 la camioneta de Javier salió de la finca y volvió a las 21:50 cargada de garrafas.",
                    anchors = new[]
                    {
                        new[] { "camioneta", "coche de javier" },
                        new[] { "garrafa", "bidon", "21:30", "21:50", "volvio" }
                    },
                    calibrationQuestions = new[] { "¿Salió o entró alguien de la finca de Javier el sábado por la noche?", "¿Vio la camioneta de Javier esa noche?" },
                    sampleHits = new[]
                    {
                        "A las 21:30 salió la camioneta de Javier y a las 21:50 volvió cargada de garrafas.",
                        "La camioneta volvió con bidones, hijo, ya de noche."
                    },
                    sampleMisses = new[] { "Javier siempre anda con la camioneta arriba y abajo." }
                },
                new ClueData
                {
                    id = "3A_audios", playerName = "Audios de voz", holder = "madre", kind = ClueKind.Incriminates,
                    summary = "Javier le mandó audios a Lucía: 'Si me quitas a la niña, no la vuelves a ver'. La vista de custodia era el lunes.",
                    topic = "si Javier te había amenazado",
                    fact = "«Javier me mandó audios la semana pasada: 'Si me quitas a la niña, no la vuelves a ver'. Y el lunes era la vista.»",
                    anchors = new[]
                    {
                        new[] { "audio", "de voz", "whatsapp" },
                        new[] { "no la vuelves a ver", "no la volveria a ver", "quitas a la nina", "quitaba a paula", "amenaz" }
                    },
                    calibrationQuestions = new[] { "¿Le había amenazado Javier alguna vez?", "¿Cómo era su relación con Javier estas semanas?" },
                    sampleHits = new[]
                    {
                        "Me mandó audios: si me quitas a la niña, no la vuelves a ver.",
                        "Tengo sus mensajes de voz. Me amenazó la semana pasada.",
                        "Javier me mandó audios la semana pasada diciendo que si me quitaba a Paula no la volvería a ver."
                    },
                    sampleMisses = new[] { "Javier y yo ya no hablamos, todo va por abogados." }
                },
                new ClueData
                {
                    id = "3A_granada", playerName = "Sábado en Granada", holder = "hermano", kind = ClueKind.Clears, clears = "madre",
                    summary = "Álex y Lucía pasaron todo el sábado en casa de su tía, en Granada, a cien kilómetros.",
                    topic = "dónde estuvisteis tu madre y tú el sábado",
                    fact = "tu madre y tú pasasteis todo el sábado en casa de tu tía en Granada, a cien kilómetros; tu tía y tus primos lo pueden decir.",
                    anchors = new[]
                    {
                        new[] { "granada", "casa de mi tia" },
                        new[] { "todo el sabado", "todo el dia", "mis primos", "mi tia" }
                    },
                    calibrationQuestions = new[] { "¿Dónde estuvisteis tu madre y tú el sábado?", "¿Alguien puede confirmar dónde estaba tu madre el sábado?" },
                    sampleHits = new[]
                    {
                        "Mi madre y yo estuvimos todo el sábado en Granada, en casa de mi tía.",
                        "En Granada, con mis primos. Todo el día."
                    },
                    sampleMisses = new[] { "No me acuerdo bien, estuvimos por ahí." }
                }
            }
        };
    }

    // ============================================
    // 3B · LUCÍA — Paula está viva
    // ============================================

    private static VariantData Variant3B()
    {
        return new VariantData
        {
            id = "3B",
            culpritId = "madre",
            epilogue = "Paula está viva. Convencida de que iba a perder la custodia, Lucía dejó a Álex en Granada a mediodía, subió a la finca a las 19:00 mientras Javier " +
                       "estaba en el bar y se llevó a Paula. A las 19:30 quemó su mochila y sus zapatillas en el quemadero para que culparan a Javier. " +
                       "Dejó a Paula con su prima en Portugal y volvió de madrugada. La Guardia Civil encuentra a Paula en Ayamonte.",
            morningReports = new[]
            {
                "",
                FirstReport,
                "En la hoguera solo hay ropa, una mochila y unas zapatillas. Nada más.",
                "La autopista registra el sábado por la noche un coche de Granada camino de Huelva.",
                "El pasaporte de Paula no aparece en ninguna de las dos casas.",
                "Alguien llama al cuartel desde un número portugués y cuelga sin hablar.",
                LastDay
            },
            roles = new List<CharacterRole>
            {
                new CharacterRole
                {
                    characterId = "madre",
                    knowledge = new[] { MentionAlex, MentionEncarna, "La vista de custodia es el lunes y temes perder a Paula." },
                    version = "El sábado estuve todo el día en Granada, en casa de mi hermana, con Álex.",
                    secret = "Paula está viva. El sábado a las 19:00 subiste a la finca mientras Javier estaba en el bar, te llevaste a Paula a casa de tu prima en Portugal " +
                             "y quemaste su mochila en el quemadero para que culparan a Javier. Volviste de madrugada.",
                    admitsWhen = CulpritAdmits,
                    nervousAbout = "la finca, tu coche rojo y Portugal.",
                    ifAccused = "Te quedas helada y luego dices, muy bajo, que tú solo quieres proteger a tu hija.",
                    doesNotKnow = "Qué vio Encarna desde su casa.",
                    lieQuote = "estuve todo el sábado en Granada",
                    lieAnchors = new[]
                    {
                        new[] { "todo el dia en granada", "todo el sabado en granada", "estuve en granada", "estuve todo el dia" },
                        new[] { "granada", "con alex", "casa de mi hermana" }
                    },
                    versionB = "Admites que dejaste a Álex en casa de tu hermana a mediodía y saliste a dar una vuelta con el coche, pero niegas haber ido a la finca.",
                    admissionSamples = new[]
                    {
                        "Salí un rato por la tarde a despejarme con el coche. Volví tarde.",
                        "Dejé a Álex en casa de mi hermana a mediodía y me fui a dar una vuelta."
                    }
                },
                new CharacterRole
                {
                    characterId = "padre",
                    knowledge = new[] { MentionAlex, MentionEncarna },
                    version = "El sábado a las 18:30 fui al bar del pueblo y volví a las 21:30; pensé que Paula dormía. El domingo por la mañana ya no estaba.",
                    secret = "Bebiste mucho y no entraste a ver a Paula al volver; te avergüenza y temes que la jueza lo use contra ti.",
                    admitsWhen = "el inspector insiste",
                    nervousAbout = "lo que bebiste el sábado.",
                    ifAccused = "Estallas: gritas que es tu hija y que su madre os ha puesto a todos en contra.",
                    doesNotKnow = "Quién subió a la finca mientras estabas fuera."
                },
                new CharacterRole
                {
                    characterId = "hermano",
                    knowledge = new[] { "Paula quería vivir con mamá, pero no quería hacerle daño a papá." },
                    version = "El sábado estuve en casa de mi tía en Granada.",
                    secret = "Mamá te pidió que dijeras que estuvo contigo todo el sábado; te sientes fatal por mentir.",
                    admitsWhen = "el inspector insiste",
                    nervousAbout = "dónde estuvo tu madre el sábado.",
                    ifAccused = "Aprietas los puños y te callas.",
                    doesNotKnow = "Qué pasó en la finca el sábado."
                },
                InnocentEncarna("El sábado estuve en casa toda la tarde; desde mi ventana se ve el camino de la finca.",
                    "Javier bebe mucho desde el divorcio.", "Conoces el coche de todos los que suben por el camino.")
            },
            clues = new List<ClueData>
            {
                new ClueData
                {
                    id = "3B_coche", playerName = "Un coche rojo", holder = "vecina", kind = ClueKind.Incriminates, exposesLie = true,
                    summary = "A las 19:00 del sábado Encarna vio un coche pequeño rojo subir a la finca. La camioneta de Javier no estaba.",
                    topic = "si viste algún coche en la finca el sábado por la tarde",
                    fact = "a las 19:00 del sábado viste un coche pequeño rojo subir por el camino de la finca de Javier; la camioneta de Javier no estaba, él estaba en el pueblo.",
                    anchors = new[]
                    {
                        new[] { "rojo", "ibiza" },
                        new[] { "19:00", "siete", "subir", "subio", "camino" }
                    },
                    calibrationQuestions = new[] { "¿Vio algún coche en la finca de Javier el sábado por la tarde?", "¿Subió alguien a la finca el sábado?" },
                    sampleHits = new[]
                    {
                        "A las siete subió un coche pequeño rojo por el camino, hijo.",
                        "Vi un coche rojo subir a la finca a las 19:00; la camioneta de Javier no estaba."
                    },
                    sampleMisses = new[] { "Por el camino sube poca gente, hijo." }
                },
                new ClueData
                {
                    id = "3B_carta", playerName = "La carta de la mesilla", holder = "padre", kind = ClueKind.Incriminates,
                    summary = "En la mesilla de Paula, Javier encontró una carta de Lucía: 'Pronto estaremos lejos de él, tú hazme caso'.",
                    topic = "si encontraste algo en el cuarto de Paula o si Paula tenía contacto con su madre",
                    fact = "el domingo encontraste en la mesilla de Paula una carta de Lucía: 'Pronto estaremos lejos de él, tú hazme caso'.",
                    anchors = new[]
                    {
                        new[] { "carta", "nota" },
                        new[] { "lejos de el", "hazme caso", "lucia", "su madre" }
                    },
                    calibrationQuestions = new[] { "¿Encontró algo en el cuarto de Paula?", "¿Tenía Paula algún contacto con su madre ese fin de semana?" },
                    sampleHits = new[]
                    {
                        "En su mesilla había una carta de Lucía: pronto estaremos lejos de él, tú hazme caso.",
                        "Encontré una nota de su madre en la mesilla."
                    },
                    sampleMisses = new[] { "En el cuarto de Paula no había nada raro." }
                },
                new ClueData
                {
                    id = "3B_noche", playerName = "Una noche larga", holder = "hermano", kind = ClueKind.Incriminates, exposesLie = true, isSecret = true,
                    summary = "Lucía dejó a Álex en Granada a mediodía y volvió de madrugada, sobre las cuatro, con un ticket de peaje de Huelva.",
                    topic = "si tu madre estuvo contigo todo el sábado",
                    fact = "«Mamá me dejó en casa de la tía a mediodía y no volvió hasta las cuatro de la madrugada; en su coche vi un ticket de peaje de Huelva.»",
                    anchors = new[]
                    {
                        new[] { "madrugada", "las cuatro", "4:00", "mediodia", "me dejo" },
                        new[] { "peaje", "huelva", "volvio" }
                    },
                    calibrationQuestions = new[]
                    {
                        "¿Estuvo tu madre contigo todo el sábado? || No me mientas, Álex. Sé que tu madre no estaba. ¿Cuándo volvió?",
                        "¿A qué hora volvió tu madre el sábado? || Insisto: sé que me ocultas algo sobre tu madre."
                    },
                    sampleHits = new[]
                    {
                        "Mamá me dejó en casa de mi tía a mediodía y volvió a las cuatro de la madrugada.",
                        "Vi un ticket de peaje de Huelva en su coche; volvió de madrugada."
                    },
                    sampleMisses = new[] { "Mamá estuvo conmigo todo el día." }
                },
                new ClueData
                {
                    id = "3B_armario", playerName = "El armario medio vacío", holder = "hermano", kind = ClueKind.Incriminates,
                    summary = "En casa de Lucía faltan la maleta grande y el pasaporte de Paula.",
                    topic = "si echas en falta algo de Paula en casa",
                    fact = "en casa de tu madre faltan la maleta grande y el pasaporte de Paula; te diste cuenta el domingo.",
                    anchors = new[]
                    {
                        new[] { "maleta", "pasaporte" },
                        new[] { "falta", "no esta", "desaparec" }
                    },
                    calibrationQuestions = new[] { "¿Echas en falta algo de Paula en casa?", "¿Se llevó Paula algo de casa?" },
                    sampleHits = new[]
                    {
                        "Faltan la maleta grande y el pasaporte de Paula.",
                        "Su pasaporte no está, y la maleta grande tampoco."
                    },
                    sampleMisses = new[] { "Las cosas de Paula están como siempre." }
                },
                new ClueData
                {
                    id = "3B_cuenta", playerName = "La cuenta del bar", holder = "padre", kind = ClueKind.Clears, clears = "padre",
                    summary = "Javier estuvo en el bar Casino de 18:30 a 21:30; el camarero le cobró a las 21:25.",
                    topic = "quién puede confirmar dónde estabas el sábado",
                    fact = "de 18:30 a 21:30 estuviste en el bar Casino del pueblo; el camarero te cobró a las 21:25 y te vio todo el rato.",
                    anchors = new[]
                    {
                        new[] { "bar", "casino" },
                        new[] { "21:25", "camarero", "me cobro", "ticket" }
                    },
                    calibrationQuestions = new[] { "¿Dónde estuvo usted el sábado por la tarde?", "¿Alguien puede confirmar dónde estaba el sábado?" },
                    sampleHits = new[]
                    {
                        "Estuve en el Casino de 18:30 a 21:30; el camarero me cobró a las 21:25.",
                        "Pregunte al camarero del bar, me vio toda la tarde."
                    },
                    sampleMisses = new[] { "Estuve por ahí, qué más da." }
                }
            }
        };
    }

    // ============================================
    // 3C · ENCARNA — la despedida junto al pozo
    // ============================================

    private static VariantData Variant3C()
    {
        return new VariantData
        {
            id = "3C",
            culpritId = "vecina",
            epilogue = "Para Encarna, Paula era su Rocío. El sábado a las 20:00 Paula fue a despedirse de ella y de los caballos porque se mudaba a Madrid. " +
                       "Encarna no la dejó irse: discutieron junto al pozo y la empujó. Después entró por la cancela con su vieja llave y quemó las cosas de Paula " +
                       "en el quemadero de Javier, para que le culparan a él. Javier volvió borracho del bar a las 22:00 y no entró a ver a su hija.",
            morningReports = new[]
            {
                "",
                FirstReport,
                "Las zapatillas medio quemadas tienen barro que no es del quemadero.",
                "El candado de la cancela entre las dos fincas está recién engrasado.",
                "Los perros de la Guardia Civil se detienen junto a un pozo de la zona.",
                "Las amigas de Paula dicen que se iba a Madrid y que solo le daba pena despedirse de alguien.",
                LastDay
            },
            roles = new List<CharacterRole>
            {
                new CharacterRole
                {
                    characterId = "vecina",
                    knowledge = new[] { "Javier bebe mucho desde el divorcio.", "Paula venía a ver tus caballos cuando estaba en la finca." },
                    version = "El sábado no vi a Paula. Estuve en casa toda la tarde viendo la tele.",
                    secret = "Paula era como tu Rocío. El sábado a las 20:00 vino a despedirse porque se iba a Madrid; no la dejaste irse, discutisteis junto al pozo y la empujaste. " +
                             "Después quemaste sus cosas en el quemadero de Javier para que le culparan a él.",
                    admitsWhen = CulpritAdmits,
                    nervousAbout = "el pozo, la cancela y tu Rocío.",
                    ifAccused = "Lloras y dices que tú querías a esa niña como a una hija.",
                    doesNotKnow = "Qué pasó dentro de la casa de Javier.",
                    lieQuote = "el sábado no vi a Paula",
                    lieAnchors = new[]
                    {
                        new[] { "no vi a paula", "no vi a la nina", "no la vi" },
                        new[] { "sabado", "toda la tarde", "viendo la tele", "en casa" }
                    },
                    versionB = "Admites que Paula vino un momento a las 20:00 a ver los caballos, pero juras que se fue enseguida hacia la casa de su padre.",
                    admissionSamples = new[]
                    {
                        "Paula vino un momento a ver los caballos, pero se fue enseguida.",
                        "Sí, pasó por aquí a despedirse, pobrecita, y se marchó."
                    }
                },
                new CharacterRole
                {
                    characterId = "padre",
                    knowledge = new[] { MentionAlex, MentionEncarna },
                    version = "El sábado estuve en el bar del pueblo. Volví tarde y pensé que Paula dormía. El domingo ya no estaba.",
                    secret = "Esa noche bebiste demasiado y no entraste a ver a Paula al volver; te avergüenza.",
                    admitsWhen = "el inspector insiste",
                    nervousAbout = "lo que bebiste el sábado.",
                    ifAccused = "Estallas y gritas que Encarna te odia por lo del pozo.",
                    doesNotKnow = "Adónde fue Paula el sábado por la tarde."
                },
                new CharacterRole
                {
                    characterId = "madre",
                    knowledge = new[] { MentionAlex, MentionEncarna, "Paula iba a mudarse contigo a Madrid después de la vista." },
                    version = "El sábado estuve en Granada con Álex, en casa de mi hermana.",
                    secret = "Planeabas mudarte a Madrid con Paula sin decírselo al juzgado.",
                    admitsWhen = "el inspector insiste",
                    nervousAbout = "Madrid y tu medicación.",
                    ifAccused = "Te quedas helada y dices, muy bajo, que eres su madre.",
                    doesNotKnow = "Qué pasó en la finca el sábado."
                },
                new CharacterRole
                {
                    characterId = "hermano",
                    knowledge = new[] { "Paula le tenía mucho cariño a Encarna y a sus caballos." },
                    version = "El sábado estuve en Granada con mi madre.",
                    secret = "Leíste el último mensaje de Paula tarde, a medianoche, porque estabas de fiesta; no te lo perdonas.",
                    admitsWhen = "el inspector insiste",
                    nervousAbout = "por qué tardaste en leer el mensaje de Paula.",
                    ifAccused = "Aprietas los puños y te callas.",
                    doesNotKnow = "Qué pasó en la finca después de las ocho."
                }
            },
            clues = new List<ClueData>
            {
                new ClueData
                {
                    id = "3C_despedida", playerName = "La despedida", holder = "hermano", kind = ClueKind.Incriminates, exposesLie = true,
                    summary = "A las 19:50 Paula le escribió a Álex: 'Voy a despedirme de Encarna y de los caballos, luego te llamo'. Nunca llamó.",
                    topic = "la última vez que supiste de Paula",
                    fact = "a las 19:50 Paula te escribió: 'Voy a despedirme de Encarna y de los caballos, luego te llamo'. Nunca te llamó.",
                    anchors = new[]
                    {
                        new[] { "despedir", "despedida" },
                        new[] { "encarna", "caballos", "vecina" }
                    },
                    calibrationQuestions = new[] { "¿Cuándo supiste de Paula por última vez?", "¿Te dijo Paula qué iba a hacer el sábado por la tarde?" },
                    sampleHits = new[]
                    {
                        "A las 19:50 me escribió que iba a despedirse de Encarna y de los caballos.",
                        "Me dijo que se iba a despedir de la vecina. Luego me llamaba. No llamó."
                    },
                    sampleMisses = new[] { "Paula no me escribió nada ese día." }
                },
                new ClueData
                {
                    id = "3C_llave", playerName = "La llave de la cancela", holder = "padre", kind = ClueKind.Incriminates,
                    summary = "Encarna es la única con llave de la cancela que une su finca con la de Javier.",
                    topic = "quién más puede entrar en tu finca",
                    fact = "Encarna es la única persona con llave de la cancela que une su finca con la tuya.",
                    anchors = new[]
                    {
                        new[] { "llave", "cancela" },
                        new[] { "entre las fincas", "encarna" }
                    },
                    calibrationQuestions = new[] { "¿Quién más puede entrar en su finca?", "¿Hay otra entrada al quemadero además de la suya?" },
                    sampleHits = new[]
                    {
                        "Encarna tiene llave de la cancela; se la dio mi padre y nunca la devolvió.",
                        "Por la cancela solo entra quien tenga la llave, y esa es Encarna.",
                        "No, solo hay esa entrada con la cancela que comparte Encarna."
                    },
                    sampleMisses = new[] { "La finca está abierta, entra quien quiere." }
                },
                new ClueData
                {
                    id = "3C_fotos", playerName = "Dos niñas en la pared", holder = "madre", kind = ClueKind.Incriminates,
                    summary = "Encarna tiene fotos de Paula junto a las de su hija muerta, Rocío, y le regalaba ropa de Rocío.",
                    topic = "qué opinas de Encarna o qué relación tenía con Paula",
                    fact = "«Encarna está obsesionada con Paula: tiene fotos suyas junto a las de su hija muerta, Rocío, y le regalaba ropa de Rocío. Me da miedo.»",
                    anchors = new[]
                    {
                        new[] { "foto", "ropa", "obsesion", "vigilaba", "pendiente de" },
                        new[] { "rocio", "hija muerta", "hija fallecida" }
                    },
                    calibrationQuestions = new[] { "¿Qué opina de Encarna, la vecina?", "¿Qué relación tenía Paula con Encarna?" },
                    sampleHits = new[]
                    {
                        "Tiene fotos de Paula junto a las de su hija muerta, Rocío.",
                        "Le regalaba ropa de Rocío. A mí me daba miedo.",
                        "Paula y Encarna se llevaban bien, pero Encarna la vigilaba demasiado. Me daba miedo por lo de Rocío."
                    },
                    sampleMisses = new[] { "Encarna es una vecina amable, poco más." }
                },
                new ClueData
                {
                    id = "3C_pisadas", playerName = "Pisadas en la ceniza", holder = "padre", kind = ClueKind.Incriminates,
                    summary = "El domingo, Javier encontró en la ceniza del quemadero huellas de bota pequeña, de mujer. Él calza un 44.",
                    topic = "si encontraste algo en el quemadero",
                    fact = "el domingo encontraste en la ceniza del quemadero huellas de botas pequeñas, de mujer; tú calzas un 44.",
                    anchors = new[]
                    {
                        new[] { "huella", "pisada" },
                        new[] { "ceniza", "quemadero", "bota", "mujer" }
                    },
                    calibrationQuestions = new[] { "¿Encontró algo en el quemadero?", "¿Qué vio usted el domingo en la finca?" },
                    sampleHits = new[]
                    {
                        "En la ceniza había huellas de bota pequeña, de mujer. Yo calzo un 44.",
                        "Encontré pisadas en el quemadero, y no eran mías."
                    },
                    sampleMisses = new[] { "En el quemadero no había nada raro." }
                },
                new ClueData
                {
                    id = "3C_bar", playerName = "La noche del bar", holder = "padre", kind = ClueKind.Clears, clears = "padre", isSecret = true,
                    summary = "Javier estuvo en el bar Casino de 19:30 a 22:00; volvió borracho y no entró a ver a Paula.",
                    topic = "qué hiciste el sábado y a qué hora volviste",
                    fact = "Estuviste en el bar Casino de 19:30 a 22:00, el camarero lo sabe; volviste borracho y no entraste a ver a Paula.",
                    anchors = new[]
                    {
                        new[] { "bar", "casino" },
                        new[] { "19:30", "22:00", "camarero", "borracho" }
                    },
                    calibrationQuestions = new[]
                    {
                        "¿Dónde estuvo el sábado por la tarde? || No me cuadra. ¿Qué hizo exactamente y hasta qué hora?",
                        "¿Entró a ver a Paula al volver a casa? || Insisto, dígame la verdad."
                    },
                    sampleHits = new[]
                    {
                        "Estuve en el Casino de 19:30 a 22:00. Volví borracho y no entré a verla.",
                        "El camarero del bar se lo puede decir. Volví a las 22:00."
                    },
                    sampleMisses = new[] { "Estuve por ahí, qué más da." }
                }
            }
        };
    }
}
