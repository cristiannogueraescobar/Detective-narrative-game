using System.Collections.Generic;

/// <summary>
/// HISTORIA 2 · "Noche de Verano"
/// Portomar, costa gallega, madrugada del martes de fiestas. Sofía Vargas (19) sale sola de La Marea a las 5:00;
/// a las 5:08 escribe "Estoy cerca" y desaparece. Su bolso aparece en la Cala do Corvo.
/// 2A: Marcos (acoso, humillación) · 2B: Andrés (cartas anónimas) · 2C: Ruiz (narcolanchas, grabación)
/// </summary>
public static class Story2NocheDeVerano
{
    private const string MentionMaruxa = "Maruxa, la de la casa de la curva, madruga y ve pasar a todo el mundo.";

    private const string CulpritAdmits =
        "te enseñan una prueba concreta, y aun así solo admites lo que esa prueba demuestra";

    private const string LastDay = "La familia de Sofía ha llegado a Portomar. Mañana tienes que acusar a alguien.";

    public static StoryData Build()
    {
        return new StoryData
        {
            id = "2",
            title = "Noche de Verano",
            victim = "Sofía",
            place = "Portomar, costa gallega. Madrugada del martes, fiestas del patrón.",
            victimSummary = "Sofía Vargas, 19 años, estudiante de Periodismo. Salió sola del bar La Marea a las 5:00 y a las 5:08 escribió «Estoy cerca».",
            situation = "Nunca llegó a casa de su tía. Su bolso apareció en la Cala do Corvo. El inspector local llevó las primeras horas.",
            intro = @"Portomar, costa gallega. Madrugada del martes, fiestas del patrón.

Sofía Vargas, 19 años, estudiante de Periodismo, sale sola del bar La Marea a las 5:00. A las 5:08 escribe a una amiga: «Estoy cerca».

Nunca llega a casa de su tía. A las 8:30 denuncian su desaparición. Esa tarde su bolso aparece en la Cala do Corvo.

Te envían desde Vigo para llevar el caso. El inspector local, Ruiz, se encargó de las primeras horas.

Tienes 7 días.",
            caseBrief = "Sofía Vargas, 19 años, salió sola del bar La Marea en Portomar a las 5:00 de la madrugada del martes, en fiestas. " +
                        "A las 5:08 escribió 'Estoy cerca' y desapareció; su bolso apareció en la Cala do Corvo. " +
                        "Te interroga un inspector enviado desde Vigo.",
            cast = new List<CharacterData>
            {
                new CharacterData
                {
                    id = "bar", name = "Marcos Rial", shortName = "Marcos", artId = "marcos", roleLabel = "dueño del bar", portraitKey = "Dueño del Bar",
                    identity = "Eres Marcos Rial, 42 años, dueño del bar La Marea en Portomar. Conoces a todo el pueblo y vives de los veranos.",
                    speech = "Gallego campechano y rudo; llamas 'chaval' al inspector y te quejas de la mala suerte.",
                    speechExample = "Mira, chaval, aquí en verano no se duerme, se trabaja.",
                    startsUnlocked = true,
                    mentionAliases = new[] { "marcos", "la marea" }
                },
                new CharacterData
                {
                    id = "cartero", name = "Andrés Souto", shortName = "Andrés", artId = "andres", roleLabel = "cartero", portraitKey = "Cartero",
                    identity = "Eres Andrés Souto, 52 años, cartero de Portomar desde hace quince años. Vives solo y conoces cada buzón del pueblo.",
                    speech = "Metódico y preciso; hablas de tu ruta y tus horarios. Te incomodan las preguntas directas.",
                    speechExample = "Yo salgo cada día a la misma hora. Siempre. Es mi trabajo.",
                    startsUnlocked = true,
                    mentionAliases = new[] { "andres", "cartero" }
                },
                new CharacterData
                {
                    id = "detective", name = "Inspector Ruiz", shortName = "Ruiz", artId = "ruiz", roleLabel = "inspector", portraitKey = "Detective",
                    identity = "Eres el inspector Ruiz, 55 años, jefe de la comisaría de Portomar. Llevaste las primeras horas del caso de Sofía.",
                    speech = "Jerga policial y cinismo cansado; restas importancia a todo y te molesta que revisen tu trabajo.",
                    speechExample = "Veinticinco años en el cuerpo, compañero. No me enseñe a hacer mi trabajo.",
                    startsUnlocked = true,
                    mentionAliases = new[] { "ruiz", "inspector" }
                },
                new CharacterData
                {
                    id = "vecina", name = "Maruxa Pena", shortName = "Maruxa", artId = "maruxa", roleLabel = "vecina", portraitKey = "Vecina",
                    identity = "Eres Maruxa Pena, 74 años, viuda. Vives en la casa de la curva de la carretera de la costa y madrugas todos los días.",
                    speech = "Español con retranca gallega; desconfías de forasteros y sueltas alguna palabra gallega suelta. Llamas 'fillo' al inspector.",
                    speechExample = "Yo no sé nada, fillo... bueno, algo sí vi.",
                    startsUnlocked = false,
                    mentionAliases = new[] { "maruxa", "la de la curva", "casa de la curva", "vecina" }
                }
            },
            variants = new List<VariantData> { Variant2A(), Variant2B(), Variant2C() }
        };
    }

    // Roles de inocentes que se repiten con matices entre variantes

    private static CharacterRole InnocentMaruxa(params string[] knowledge)
    {
        return new CharacterRole
        {
            characterId = "vecina",
            knowledge = knowledge,
            version = "Esa madrugada me levanté a las cinco, como siempre, a encender la cocina de leña, y estuve un buen rato mirando por la ventana.",
            secret = "Destilas orujo en casa sin licencia y lo vendes a los vecinos.",
            admitsWhen = "te preguntan qué hacías levantada tan temprano con tanto humo en la cocina",
            nervousAbout = "que la policía entre en tu casa.",
            ifAccused = "Te santiguas y te ríes: a tus años, qué vas a hacer tú.",
            doesNotKnow = "Nada de lo que pasa dentro del bar La Marea."
        };
    }

    // ============================================
    // 2A · MARCOS — acoso y humillación en el bar
    // ============================================

    private static VariantData Variant2A()
    {
        return new VariantData
        {
            id = "2A",
            culpritId = "bar",
            epilogue = "Marcos Rial llevaba todo el verano acosando a Sofía. A las 4:30 ella le tiró una copa y le llamó acosador delante de todo el bar. " +
                       "A las 4:55 Marcos desenchufó la cámara, a las 5:05 salió en su Volvo ranchera y la alcanzó en la curva. " +
                       "Maruxa vio el coche parado sin luces a las 5:20; Andrés vio el bar vacío a las 5:10. Hacia las 6:00 volvió al bar a 'limpiar' hasta las 6:30.",
            morningReports = new[]
            {
                "",
                "Los buzos encuentran el cuerpo de Sofía en la Cala do Corvo. El forense descarta una caída: murió de un golpe.",
                "Un camarero de las fiestas recuerda gritos en La Marea hacia las 4:30.",
                "En el camino de la Cala do Corvo hay marcas de neumático de un coche grande.",
                "La Marea tiene una cámara de seguridad. El informe del inspector local no la menciona.",
                "Las amigas de Sofía dicen que este verano 'alguien del pueblo' la agobiaba.",
                LastDay
            },
            roles = new List<CharacterRole>
            {
                new CharacterRole
                {
                    characterId = "bar",
                    knowledge = new[] { MentionMaruxa, "Sofía pasó el verano en La Marea con sus amigas." },
                    version = "Cerré a las cinco y estuve limpiando dentro hasta las seis y media. No salí del bar.",
                    secret = "Llevas todo el verano detrás de Sofía, aunque ella te rechazó. A las 4:30 te humilló delante de todos. " +
                             "A las 4:55 desenchufaste la cámara del bar, a las 5:05 saliste en tu Volvo ranchera y la alcanzaste en la curva.",
                    admitsWhen = CulpritAdmits,
                    nervousAbout = "la cámara del bar, tu coche y lo que pasó con Sofía a las 4:30.",
                    ifAccused = "Te pones rojo, golpeas la mesa y dices que en este pueblo todos te conocen.",
                    doesNotKnow = "Qué vio Maruxa desde su casa.",
                    lieQuote = "no salí del bar",
                    lieAnchors = new[]
                    {
                        new[] { "no sali del bar", "no me movi del bar", "no sali de aqui", "estuve limpiando" },
                        new[] { "6:30", "seis y media", "dentro", "toda la noche" }
                    },
                    versionB = "Admites que saliste un rato con el coche a tomar el aire, pero juras que no viste a Sofía.",
                    admissionSamples = new[]
                    {
                        "Vale, salí un rato con el coche a tomar el aire, chaval, pero a Sofía no la vi.",
                        "Cogí el Volvo cinco minutos para despejarme. Nada más."
                    }
                },
                new CharacterRole
                {
                    characterId = "cartero",
                    knowledge = new[] { MentionMaruxa, "Cada madrugada vas a la oficina de clasificación y entras a las 5:15." },
                    version = "A las 5:10 pasé por delante de La Marea camino de la oficina, y a las 5:15 ya estaba clasificando.",
                    secret = "Guardas en casa postales que nunca llegaste a entregar; te da vergüenza que se sepa.",
                    admitsWhen = "el inspector insiste",
                    nervousAbout = "que registren tu casa.",
                    ifAccused = "Te encoges y repites que solo haces tu trabajo.",
                    doesNotKnow = "Qué pasó dentro del bar esa noche."
                },
                new CharacterRole
                {
                    characterId = "detective",
                    knowledge = new[] { MentionMaruxa, "Crees que Sofía se cayó por las rocas de la cala." },
                    version = "Esa noche estuve de guardia en comisaría. A las 8:30 denunció la tía y empecé a buscar.",
                    secret = "Bebiste dos copas de servicio en La Marea antes de volver a la comisaría a las 3:00; si se sabe, te expedientan.",
                    admitsWhen = "el inspector insiste",
                    nervousAbout = "lo que bebiste esa noche y las prisas por cerrar el caso.",
                    ifAccused = "Te ríes con desprecio y dices que no vas a aguantar lecciones de un chaval de Vigo.",
                    doesNotKnow = "Quién conducía el coche que vio Maruxa."
                },
                InnocentMaruxa("Sofía pasaba cada noche por la curva de vuelta a casa de su tía.", "En fiestas pasan coches a todas horas.")
            },
            clues = new List<ClueData>
            {
                new ClueData
                {
                    id = "2A_copa", playerName = "Una copa por la cara", holder = "detective", kind = ClueKind.Incriminates,
                    summary = "A las 4:30 Sofía le tiró una copa a Marcos y le llamó acosador delante de todo el bar.",
                    topic = "lo que pasó en el bar esa noche",
                    fact = "a las 4:30 Sofía le tiró una copa a Marcos y le llamó acosador delante de todo el bar; varios testigos te lo contaron.",
                    anchors = new[]
                    {
                        new[] { "copa" },
                        new[] { "acosador", "acoso", "le tiro", "discutieron", "discusion" }
                    },
                    calibrationQuestions = new[] { "¿Qué pasó en el bar esa noche?", "¿Tuvo Sofía algún problema con alguien en La Marea?" },
                    sampleHits = new[]
                    {
                        "A las 4:30 la chica le tiró una copa a Marcos y le llamó acosador, compañero.",
                        "Hubo una discusión: Sofía le tiró la copa por la cara al dueño."
                    },
                    sampleMisses = new[] { "Esa noche en el bar no pasó nada raro, la gente bebiendo como siempre." }
                },
                new ClueData
                {
                    id = "2A_camara", playerName = "La cámara del bar", holder = "detective", kind = ClueKind.Incriminates,
                    summary = "La cámara de La Marea dejó de grabar a las 4:55: alguien la desenchufó a mano.",
                    topic = "las cámaras o grabaciones",
                    fact = "la cámara de La Marea dejó de grabar a las 4:55 porque alguien la desenchufó a mano, justo antes de que Sofía saliera.",
                    anchors = new[]
                    {
                        new[] { "camara", "grabacion", "grabar" },
                        new[] { "4:55", "cinco menos cinco", "desenchuf", "dejo de grabar", "apago" }
                    },
                    calibrationQuestions = new[] { "¿Hay grabaciones de cámaras de esa noche?", "¿Revisaron la cámara del bar La Marea?" },
                    sampleHits = new[]
                    {
                        "La cámara del bar dejó de grabar a las 4:55, alguien la desenchufó.",
                        "Alguien apagó la cámara de La Marea a las cinco menos cinco."
                    },
                    sampleMisses = new[] { "No hay cámaras en esa carretera, compañero." }
                },
                new ClueData
                {
                    id = "2A_aparcamiento", playerName = "Un aparcamiento vacío", holder = "cartero", kind = ClueKind.Incriminates, exposesLie = true,
                    summary = "A las 5:10 Andrés pasó por La Marea: el bar estaba a oscuras y el Volvo de Marcos no estaba. Marcos dice que no salió del bar.",
                    topic = "lo que viste al pasar por La Marea, o si viste algo raro o algún coche cerca del bar esa madrugada",
                    fact = "a las 5:10, al pasar por La Marea, el bar estaba a oscuras y el Volvo ranchera de Marcos no estaba aparcado en su sitio. Te extrañó: otras noches se queda recogiendo con la luz encendida.",
                    anchors = new[]
                    {
                        new[] { "volvo", "coche de marcos", "ranchera" },
                        new[] { "no estaba", "vacio", "a oscuras", "apagado" }
                    },
                    calibrationQuestions = new[] { "¿Qué vio al pasar por delante de La Marea?", "¿Estaba el coche de Marcos en el bar esa madrugada?", "¿Vio algo raro cerca del bar La Marea esa madrugada?" },
                    sampleHits = new[]
                    {
                        "A las 5:10 pasé por La Marea y estaba a oscuras; el Volvo de Marcos no estaba.",
                        "El coche de Marcos, la ranchera, no estaba en su sitio a esa hora."
                    },
                    sampleMisses = new[] { "Pasé por delante del bar como cada día, nada especial." }
                },
                new ClueData
                {
                    id = "2A_curva", playerName = "Coche en la curva", holder = "vecina", kind = ClueKind.Incriminates,
                    summary = "A las 5:20 Maruxa vio un coche grande y oscuro, tipo ranchera, parado en la curva con las luces apagadas.",
                    topic = "lo que viste desde tu ventana o en la curva esa madrugada, o si viste pasar a alguien o algún vehículo",
                    fact = "a las 5:20 viste un coche grande y oscuro, tipo ranchera, parado en la curva con las luces apagadas. Ahí nunca para nadie.",
                    anchors = new[]
                    {
                        new[] { "coche", "ranchera" },
                        new[] { "luces apagadas", "sin luces", "parado", "5:20" }
                    },
                    calibrationQuestions = new[] { "¿Vio algo raro en la curva esa madrugada?", "¿Pasó algún coche por delante de su casa hacia las cinco?", "¿Qué vio desde su ventana esa madrugada?" },
                    sampleHits = new[]
                    {
                        "A las 5:20 había un coche grande, oscuro, parado en la curva sin luces, fillo.",
                        "Vi una ranchera oscura parada con las luces apagadas."
                    },
                    sampleMisses = new[] { "Pasaron coches, como siempre en fiestas." }
                },
                new ClueData
                {
                    id = "2A_gps", playerName = "El GPS de Correos", holder = "detective", kind = ClueKind.Clears, clears = "cartero",
                    summary = "El GPS de la furgoneta de Correos sitúa a Andrés en la oficina de clasificación de 5:15 a 7:00.",
                    topic = "dónde estaba el cartero esa madrugada o si alguien comprobó la coartada de Andrés",
                    fact = "el GPS de la furgoneta de Correos sitúa a Andrés en la oficina de clasificación desde las 5:15 hasta las 7:00.",
                    anchors = new[]
                    {
                        new[] { "gps", "localizador" },
                        new[] { "oficina", "clasificacion", "5:15", "7:00" }
                    },
                    calibrationQuestions = new[] { "¿Comprobaron dónde estaba el cartero esa madrugada?", "¿Qué sabe de Andrés Souto?", "¿Alguien ha comprobado la coartada de Andrés?" },
                    sampleHits = new[]
                    {
                        "El GPS de Correos pone al cartero en la oficina de 5:15 a 7:00.",
                        "Según el localizador de la furgoneta, Andrés estaba en la oficina de clasificación."
                    },
                    sampleMisses = new[] { "El cartero es un tipo raro, pero no sé dónde andaba." }
                }
            }
        };
    }

    // ============================================
    // 2B · ANDRÉS — cartas anónimas sin sello
    // ============================================

    private static VariantData Variant2B()
    {
        return new VariantData
        {
            id = "2B",
            culpritId = "cartero",
            epilogue = "Andrés Souto llevaba todo el verano dejando cartas anónimas sin sello en el buzón de la tía de Sofía. " +
                       "Ella lo descubrió y amenazó con denunciarle. A las 5:12 Andrés la esperó en la curva con su furgoneta blanca y ella subió. " +
                       "Fichó a las 6:15, una hora tarde por primera vez en quince años, y a las 7:30 lavó la furgoneta a manguerazos.",
            morningReports = new[]
            {
                "",
                "Los buzos encuentran el cuerpo de Sofía en la Cala do Corvo. El forense descarta una caída: murió de un golpe.",
                "La tía de Sofía comenta que este verano alguien dejaba cosas en su buzón.",
                "En el bolso de Sofía había un sobre sin sello ni remitente, vacío.",
                "Correos ha enviado a la comisaría de Ruiz los registros de fichaje de su oficina de esa madrugada, incluido el del cartero.",
                "Un vecino dice que esa madrugada vio pasar una furgoneta 'pequeña, clara'.",
                LastDay
            },
            roles = new List<CharacterRole>
            {
                new CharacterRole
                {
                    characterId = "cartero",
                    knowledge = new[] { MentionMaruxa, "Cada madrugada vas a la oficina de clasificación." },
                    version = "A las cinco y cuarto ya estaba en la oficina clasificando, como cada día. No vi a Sofía.",
                    secret = "Llevas todo el verano dejando cartas anónimas sin sello en el buzón de la tía de Sofía. Ella lo descubrió y amenazó con denunciarte. " +
                             "A las 5:12 la recogiste en la curva con tu furgoneta particular. Fichaste tarde y por la mañana la lavaste.",
                    admitsWhen = CulpritAdmits,
                    nervousAbout = "las cartas, tu furgoneta y la hora a la que fichaste.",
                    ifAccused = "Te quedas muy quieto, hablas en voz baja y repites que eres un funcionario intachable.",
                    doesNotKnow = "Qué vio Maruxa desde su ventana.",
                    lieQuote = "a las cinco y cuarto ya estaba clasificando",
                    lieAnchors = new[]
                    {
                        new[] { "ya estaba en la oficina", "ya estaba clasificando", "estaba en la oficina clasificando" },
                        new[] { "5:15", "cinco y cuarto", "como cada dia", "como siempre" }
                    },
                    versionB = "Admites que llegaste tarde a la oficina porque te dormiste, pero niegas haber visto a Sofía.",
                    admissionSamples = new[]
                    {
                        "Llegué más tarde a la oficina, a las seis y cuarto; me quedé dormido.",
                        "Esa madrugada fiché tarde, a las 6:15. Me dormí, es la primera vez."
                    }
                },
                new CharacterRole
                {
                    characterId = "bar",
                    knowledge = new[] { MentionMaruxa, "Sofía venía a La Marea casi cada noche con sus amigas." },
                    version = "Cerré a las cinco y me quedé recogiendo dentro hasta las seis y media. Luego un café y a casa hacia las siete y media.",
                    secret = "Esa noche serviste alcohol a menores y cerraste fuera de hora; te pueden multar.",
                    admitsWhen = "el inspector insiste",
                    nervousAbout = "la hora de cierre y los menores.",
                    ifAccused = "Golpeas la mesa y dices que en este pueblo todos te conocen.",
                    doesNotKnow = "Dónde estaba el cartero de madrugada."
                },
                new CharacterRole
                {
                    characterId = "detective",
                    knowledge = new[] { MentionMaruxa, "Crees que Sofía se cayó por las rocas de la cala." },
                    version = "Esa noche estuve de guardia en comisaría. A las 8:30 denunció la tía.",
                    secret = "Bebiste dos copas de servicio en La Marea antes de volver a la comisaría a las 3:00; si se sabe, te expedientan.",
                    admitsWhen = "el inspector insiste",
                    nervousAbout = "lo que bebiste esa noche y las prisas por cerrar el caso.",
                    ifAccused = "Te ríes con desprecio y dices que no vas a aguantar lecciones de un chaval de Vigo.",
                    doesNotKnow = "Qué vio Maruxa esa madrugada."
                },
                InnocentMaruxa("Sofía pasaba cada noche por la curva de vuelta a casa de su tía.", "En fiestas pasan coches a todas horas.")
            },
            clues = new List<ClueData>
            {
                new ClueData
                {
                    id = "2B_cartas", playerName = "Cartas sin sello", holder = "bar", kind = ClueKind.Incriminates,
                    summary = "Sofía le enseñó a Marcos cartas anónimas que le dejaban en el buzón, sin sello ni matasellos. Estaba asustada.",
                    topic = "si Sofía tenía miedo de alguien o te contó algo raro",
                    fact = "hace una semana Sofía te enseñó unas cartas anónimas que le dejaban en el buzón de su tía antes de amanecer, sin sello ni matasellos; estaba asustada.",
                    anchors = new[]
                    {
                        new[] { "carta" },
                        new[] { "sin sello", "anonima", "matasellos", "buzon" }
                    },
                    calibrationQuestions = new[] { "¿Tenía Sofía miedo de alguien?", "¿Le contó Sofía algo raro este verano?" },
                    sampleHits = new[]
                    {
                        "Sofía me enseñó unas cartas anónimas, chaval, sin sello ni nada.",
                        "Le dejaban cartas en el buzón de su tía, sin matasellos."
                    },
                    sampleMisses = new[] { "Sofía no tenía miedo de nadie, era muy echada para adelante." }
                },
                new ClueData
                {
                    id = "2B_fichaje", playerName = "Un fichaje tardío", holder = "detective", kind = ClueKind.Incriminates, exposesLie = true,
                    summary = "Según los registros que tiene Ruiz, Andrés fichó a las 6:15; lleva quince años fichando a las 5:15. Andrés dice que a las 5:15 ya clasificaba.",
                    topic = "los horarios del cartero, a qué hora llegó o fichó Andrés esa madrugada, o los registros de fichaje de Correos",
                    fact = "tienes en comisaría los registros de fichaje de Correos de esa madrugada: Andrés fichó en la oficina a las 6:15, cuando lleva quince años fichando a las 5:15 sin fallar un día. No le diste importancia porque crees que fue una caída.",
                    anchors = new[]
                    {
                        new[] { "fich" },
                        new[] { "6:15", "seis y cuarto", "una hora tarde", "tarde" }
                    },
                    calibrationQuestions = new[] { "¿Comprobaron los horarios del cartero?", "¿A qué hora llegó Andrés a su trabajo esa madrugada?", "¿Puedo ver los registros de fichaje de Correos?" },
                    sampleHits = new[]
                    {
                        "El cartero fichó a las 6:15, compañero, y lleva quince años fichando a las 5:15.",
                        "Andrés fichó una hora tarde ese día."
                    },
                    sampleMisses = new[] { "Andrés es muy puntual, siempre a su hora." }
                },
                new ClueData
                {
                    id = "2B_furgoneta", playerName = "La furgoneta blanca", holder = "vecina", kind = ClueKind.Incriminates,
                    summary = "A las 5:12 Maruxa vio una furgoneta blanca pequeña parar junto a la chica en la curva. Ella subió.",
                    topic = "lo que viste desde tu ventana o en la curva esa madrugada, o lo que pasó por delante de tu casa, personas o vehículos",
                    fact = "«A las 5:12 paró una furgoneta blanca pequeña junto a la rapaza, en la curva, y ella se subió.»",
                    anchors = new[]
                    {
                        new[] { "furgoneta", "coche pequeno" },
                        new[] { "blanca", "se subio", "subio", "subirse", "5:12", "paro", "sofia", "la nina", "la chica", "la rapaza" }
                    },
                    calibrationQuestions = new[] { "¿Vio algo raro en la curva esa madrugada?", "¿Pasó algún vehículo por delante de su casa hacia las cinco?", "¿Qué vio desde su ventana esa madrugada?" },
                    sampleHits = new[]
                    {
                        "A las 5:12 paró una furgoneta blanca pequeña y la rapaza se subió.",
                        "Vi una furgoneta blanca; la chica subió."
                    },
                    sampleMisses = new[] { "Pasaron coches, como siempre en fiestas." }
                },
                new ClueData
                {
                    id = "2B_manguera", playerName = "Manguera al amanecer", holder = "bar", kind = ClueKind.Incriminates,
                    summary = "A las 7:30 Marcos vio a Andrés lavando a manguerazos su furgoneta blanca. Nunca la lava.",
                    topic = "lo que viste por la mañana al volver a casa, o si sabes de alguien con una furgoneta blanca pequeña",
                    fact = "a las 7:30, al volver a casa, viste a Andrés lavando a manguerazos su furgoneta blanca detrás de su casa. En quince años nunca le viste lavarla.",
                    anchors = new[]
                    {
                        new[] { "lavando", "lavaba", "manguera", "lavarla", "lavar la" },
                        new[] { "furgoneta", "andres", "cartero" }
                    },
                    calibrationQuestions = new[] { "¿Vio algo raro esa mañana al volver a casa?", "¿Qué vio al irse a casa después de recoger el bar?", "¿Conoce a alguien del pueblo con una furgoneta blanca pequeña?" },
                    sampleHits = new[]
                    {
                        "A las 7:30 vi al cartero lavando la furgoneta a manguerazos, chaval.",
                        "Andrés estaba con la manguera limpiando su furgoneta blanca.",
                        // Bot, día 3 (2B_1): no se detectó
                        "Andrés se fue con una furgoneta blanca y vino a lavarla a las siete y media detrás de su casa."
                    },
                    sampleMisses = new[] { "Por la mañana no vi a nadie, estaba reventado." }
                },
                new ClueData
                {
                    id = "2B_imagenes", playerName = "Imágenes del bar", holder = "detective", kind = ClueKind.Clears, clears = "bar",
                    summary = "La cámara de La Marea grabó a Marcos dentro del bar, recogiendo, de 5:00 a 6:30.",
                    topic = "las cámaras o grabaciones",
                    fact = "la cámara de La Marea grabó a Marcos dentro del bar, recogiendo, desde las 5:00 hasta las 6:30.",
                    anchors = new[]
                    {
                        new[] { "camara", "grabacion", "grabo", "6:30", "seis y media" },
                        new[] { "marcos", "dueno", "dentro del bar", "recogiendo" }
                    },
                    calibrationQuestions = new[] { "¿Hay grabaciones de cámaras de esa noche?", "¿Qué sabe de Marcos, el del bar?" },
                    sampleHits = new[]
                    {
                        "La cámara del bar grabó a Marcos recogiendo de 5:00 a 6:30, compañero.",
                        "En la grabación se ve al dueño dentro del bar toda la madrugada."
                    },
                    sampleMisses = new[] { "No hay cámaras en esa carretera, compañero." }
                }
            }
        };
    }

    // ============================================
    // 2C · RUIZ — narcolanchas y una grabación
    // ============================================

    private static VariantData Variant2C()
    {
        return new VariantData
        {
            id = "2C",
            culpritId = "detective",
            epilogue = "El inspector Ruiz cobraba de una red de narcolanchas por avisar de las patrullas. A las 4:20 Sofía, que preparaba un reportaje sobre el puerto, " +
                       "le grabó con el móvil cogiendo un sobre en el muelle. A las 5:10 Ruiz la recogió en la curva con su Opel gris. " +
                       "Después llevó él mismo el caso y el móvil de Sofía desapareció de las pruebas. La comisaría estuvo vacía toda esa madrugada.",
            morningReports = new[]
            {
                "",
                "Los buzos encuentran el cuerpo de Sofía en la Cala do Corvo. El forense descarta una caída: murió de un golpe.",
                "La facultad confirma que Sofía preparaba un reportaje sobre el puerto de Portomar.",
                "El móvil de Sofía no está entre las pruebas enviadas a Vigo.",
                "La Guardia Civil investiga movimientos de lanchas rápidas en la ría esa misma noche.",
                "El informe de las primeras horas es sorprendentemente corto. Correos dice que esa madrugada el cartero llevaba un certificado urgente a la comisaría y que sigue sin firmar.",
                LastDay
            },
            roles = new List<CharacterRole>
            {
                new CharacterRole
                {
                    characterId = "detective",
                    knowledge = new[] { MentionMaruxa, "Sofía hacía un reportaje sobre el puerto para la facultad." },
                    version = "Esa noche estuve en comisaría hasta las seis. No vi a la chica en ningún momento.",
                    secret = "Cobras de una red de narcolanchas por avisar de las patrullas. A las 4:20 Sofía te grabó cogiendo un sobre en el muelle. " +
                             "A las 5:10 la recogiste en la curva con tu Opel gris. Después hiciste desaparecer su móvil de las pruebas.",
                    admitsWhen = CulpritAdmits,
                    nervousAbout = "el puerto, tu coche y el móvil de Sofía.",
                    ifAccused = "Te ríes con frialdad y dices que un chaval de Vigo no sabe dónde se mete.",
                    doesNotKnow = "Qué vio el cartero esa madrugada.",
                    lieQuote = "estuve en comisaría hasta las seis",
                    lieAnchors = new[]
                    {
                        new[] { "en comisaria", "en la comisaria" },
                        new[] { "hasta las seis", "6:00", "toda la noche", "de guardia" }
                    },
                    versionB = "Admites que saliste a hacer una ronda en el coche hacia las cinco, pero niegas haber visto a Sofía.",
                    admissionSamples = new[]
                    {
                        "Salí un momento de la comisaría a hacer una ronda, nada más.",
                        "Vale, di una vuelta con el coche hacia las cinco. Rutina, compañero."
                    }
                },
                new CharacterRole
                {
                    characterId = "bar",
                    knowledge = new[] { MentionMaruxa, "Sofía estaba con un trabajo de la facultad sobre el pueblo." },
                    version = "Cerré a las cinco y me quedé recogiendo dentro hasta las seis y media.",
                    secret = "Vendes tabaco de contrabando que te traen del puerto; conoces a gente de las lanchas.",
                    admitsWhen = "el inspector insiste",
                    nervousAbout = "el puerto y la gente de las lanchas.",
                    ifAccused = "Golpeas la mesa y dices que en este pueblo todos te conocen.",
                    doesNotKnow = "Qué pasó en la curva después de las cinco."
                },
                new CharacterRole
                {
                    characterId = "cartero",
                    // Aquí Maruxa solo se nombra (la desbloquea): si "ve pasar a todo el mundo", el modelo la ofrece
                    // como testigo en vez del GPS de la furgoneta, que es la pista de esta variante
                    knowledge = new[] { "Maruxa, la de la casa de la curva, madruga mucho.", "Esa madrugada llevabas un certificado urgente para la comisaría." },
                    version = "Esa madrugada pasé antes por la comisaría con un certificado urgente y llegué a la oficina pasadas las cinco y cuarto.",
                    secret = "Guardas en casa postales que nunca llegaste a entregar; te da vergüenza que se sepa.",
                    admitsWhen = "el inspector insiste",
                    nervousAbout = "que registren tu casa.",
                    ifAccused = "Te encoges y repites que solo haces tu trabajo.",
                    doesNotKnow = "Qué pasó dentro del bar esa noche."
                },
                InnocentMaruxa("Sofía pasaba cada noche por la curva de vuelta a casa de su tía.", "Conoces el coche de todo el mundo en Portomar.")
            },
            clues = new List<ClueData>
            {
                new ClueData
                {
                    id = "2C_puerto", playerName = "Algo gordo en el puerto", holder = "bar", kind = ClueKind.Incriminates,
                    summary = "A las 4:40 Sofía volvió pálida al bar: había grabado 'algo gordo' en el puerto y no se fiaba de la policía.",
                    topic = "cómo estaba Sofía esa noche, qué te contó, o lo que sabes de su reportaje sobre el puerto",
                    fact = "a las 4:40 Sofía volvió pálida al bar y te dijo que había grabado con el móvil algo gordo en el puerto, y que no se fiaba de la policía.",
                    anchors = new[]
                    {
                        new[] { "grabado", "grabo", "video" },
                        new[] { "puerto", "muelle", "algo gordo", "algo importante", "no se fiaba de la policia" }
                    },
                    calibrationQuestions = new[] { "¿Cómo estaba Sofía esa noche?", "¿Le contó Sofía algo antes de irse?", "¿Sabe algo del reportaje de Sofía sobre el puerto?" },
                    sampleHits = new[]
                    {
                        "A las 4:40 volvió blanca, chaval: dijo que había grabado algo gordo en el puerto.",
                        "Me contó que tenía un vídeo del muelle y que no se fiaba de la policía.",
                        // Bot, día 3 (2C_1): no se detectó
                        "Me dijo que había grabado algo importante y que no se fiaba de la policía."
                    },
                    sampleMisses = new[] { "Sofía estaba de fiesta con sus amigas, como siempre." }
                },
                new ClueData
                {
                    id = "2C_opel", playerName = "Un coche conocido", holder = "vecina", kind = ClueKind.Incriminates, exposesLie = true,
                    summary = "A las 5:10 Maruxa vio el Opel gris del inspector Ruiz parar junto a la chica en la curva. Ella subió.",
                    topic = "lo que viste desde tu ventana o en la curva esa madrugada, o si viste pasar a alguien o algún vehículo",
                    fact = "a las 5:10 viste el Opel gris del inspector Ruiz, lo conoces de sobra, parar junto a la chica en la curva; ella se subió.",
                    anchors = new[]
                    {
                        new[] { "opel", "coche del inspector", "coche de ruiz", "coche gris", "ruiz con su coche", "ruiz en su coche", "inspector con su coche", "inspector en su coche" },
                        new[] { "ruiz", "inspector", "se subio", "5:10" }
                    },
                    calibrationQuestions = new[] { "¿Vio algo raro en la curva esa madrugada?", "¿Pasó algún coche por delante de su casa hacia las cinco?", "¿Qué vio desde su ventana esa madrugada?" },
                    sampleHits = new[]
                    {
                        "A las 5:10 paró el Opel gris del inspector Ruiz junto a la rapaza, fillo.",
                        "Era el coche de Ruiz, lo conozco de sobra; la chica se subió.",
                        // Bot, día 3 (2C_1): no se detectó
                        "A esas horas solo vi pasar al inspector Ruiz con su coche, fillo."
                    },
                    sampleMisses = new[] { "Pasaron coches, como siempre en fiestas." }
                },
                new ClueData
                {
                    id = "2C_comisaria", playerName = "Comisaría cerrada", holder = "cartero", kind = ClueKind.Incriminates, exposesLie = true,
                    summary = "A las 5:05 Andrés fue a la comisaría con un certificado urgente: estaba cerrada y vacía. Ruiz dice que estuvo allí hasta las seis.",
                    topic = "lo que hiciste entre las cinco y las cinco y cuarto, si viste a alguien en la comisaría, o lo que pasó cuando fuiste con el certificado",
                    fact = "a las 5:05 fuiste a la comisaría a dejar el certificado urgente y estaba cerrada, con las luces apagadas y sin nadie dentro. Te extrañó: en fiestas siempre hay alguien de guardia.",
                    anchors = new[]
                    {
                        new[] { "comisaria", "certificado", "5:05", "cinco y cinco" },
                        new[] { "cerrada", "cerrado", "sin nadie", "nadie dentro", "vacia", "luces apagadas" }
                    },
                    calibrationQuestions = new[] { "¿Pasó por la comisaría esa madrugada?", "¿Qué hizo usted entre las cinco y las cinco y cuarto?", "¿Vio al inspector Ruiz en la comisaría esa madrugada?" },
                    sampleHits = new[]
                    {
                        "A las 5:05 fui a la comisaría con el certificado y estaba cerrada, sin nadie dentro.",
                        "La comisaría estaba vacía, con las luces apagadas.",
                        "Sí, pasé por allí a las 5:05 para dejar un certificado urgente, pero estaba todo cerrado.",
                        "Sí, pasé por allí a las 5:05, pero estaba cerrada y no pude dejar el certificado."
                    },
                    sampleMisses = new[] { "Dejé el certificado en la comisaría y seguí mi ruta." }
                },
                new ClueData
                {
                    id = "2C_prueba", playerName = "Una prueba perdida", holder = "detective", kind = ClueKind.Incriminates,
                    summary = "El móvil de Sofía apareció en la cala, se registró como prueba y 'se extravió' en el traslado a Vigo.",
                    topic = "el móvil de Sofía o las pruebas que se enviaron a Vigo",
                    fact = "el móvil de la chica apareció en la cala, se registró como prueba y se extravió en el traslado a Vigo. Son cosas que pasan.",
                    anchors = new[]
                    {
                        new[] { "movil", "telefono", "prueba" },
                        new[] { "extravi", "se perdio", "perdido", "traslado", "desaparecio" }
                    },
                    calibrationQuestions = new[] { "¿Dónde está el móvil de Sofía?", "¿Encontraron el teléfono de la chica?", "¿Por qué el móvil de Sofía no está entre las pruebas enviadas a Vigo?" },
                    sampleHits = new[]
                    {
                        "El móvil se extravió en el traslado a Vigo, compañero. Pasa.",
                        "Encontramos el teléfono en la cala, pero se perdió por el camino."
                    },
                    sampleMisses = new[] { "El móvil lo tiene la científica, no se preocupe." }
                },
                new ClueData
                {
                    id = "2C_gps", playerName = "El GPS de Correos", holder = "cartero", kind = ClueKind.Clears, clears = "cartero",
                    summary = "La furgoneta de Correos tiene GPS: a las 5:15 Andrés estaba en la nacional camino de la oficina.",
                    topic = "quién o qué puede confirmar dónde estabas, o cómo demostrar tus horarios",
                    fact = "«Nadie me vio, pero mírelo en el GPS de la furgoneta de Correos: a las 5:15 me sitúa en la nacional, camino de la oficina. Compruébelo.»",
                    anchors = new[]
                    {
                        new[] { "gps", "localizador", "me situa", "me localiza", "lo registra" },
                        new[] { "nacional", "oficina", "5:15", "comprobar" }
                    },
                    calibrationQuestions = new[] { "¿Alguien puede confirmar dónde estaba usted?", "¿Cómo sé que dice la verdad sobre sus horarios?" },
                    sampleHits = new[]
                    {
                        "La furgoneta lleva GPS: a las 5:15 estaba en la nacional, compruébelo.",
                        "Mire el localizador de Correos, me sitúa camino de la oficina.",
                        "A las 5:15 me sitúa en la nacional, camino de la oficina. Compruébelo.",
                        "A las 5:15 entré en la oficina de Correos, como siempre. Mi furgoneta lo registra todo."
                    },
                    sampleMisses = new[] { "Yo trabajo solo, nadie me acompaña." }
                },
                new ClueData
                {
                    id = "2C_grabacion", playerName = "Grabación completa", holder = "bar", kind = ClueKind.Clears, clears = "bar",
                    summary = "La cámara de La Marea grabó toda la noche y Marcos entregó la grabación a la Guardia Civil: se le ve dentro hasta las 6:30.",
                    topic = "quién puede confirmar dónde estabas",
                    fact = "la cámara de tu bar grabó toda la noche y tú mismo le entregaste la grabación a la Guardia Civil: se te ve dentro recogiendo hasta las 6:30.",
                    anchors = new[]
                    {
                        new[] { "camara", "grabacion" },
                        new[] { "guardia civil", "entregue", "toda la noche", "6:30" }
                    },
                    calibrationQuestions = new[] { "¿Alguien puede confirmar dónde estaba usted?", "¿Tiene cámaras en el bar?" },
                    sampleHits = new[]
                    {
                        "La cámara grabó toda la noche, chaval; le di la grabación a la Guardia Civil.",
                        "Está todo en la grabación: se me ve dentro hasta las 6:30."
                    },
                    sampleMisses = new[] { "Estuve en el bar, pregunte a quien quiera." }
                }
            }
        };
    }
}
