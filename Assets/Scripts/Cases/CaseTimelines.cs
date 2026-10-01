using System.Collections.Generic;

/// <summary>
/// Línea temporal real de cada variante: quién estaba dónde y haciendo qué, hora a hora. No llega a las fichas de
/// los personajes; la usa el validador narrativo para comprobar que nadie está en dos sitios a la vez y que las
/// horas de las pistas encajan. "~" = hora aproximada (se deduce de los datos). Redactada en la auditoría del día 3
/// (docs/STORY-AUDIT.md).
/// </summary>
public static class CaseTimelines
{
    public static List<TimelineEvent> For(string variantId)
    {
        switch (variantId)
        {
            case "1A":
                return new List<TimelineEvent>
                {
                    E("21:30", "hermano", "su cuarto", "empieza la partida online (registro hasta las 00:00)"),
                    E("22:00", "madre", "dormitorio", "toma su zolpidem y se acuesta"),
                    E("22:00", "victima", "su cuarto", "se acuesta"),
                    E("22:30", "padre", "cuarto de Elena", "sube el cacao con zolpidem triturado"),
                    E("22:35", "padre", "cuarto de Elena", "cierra las cortinas"),
                    E("22:35", "vecina", "su salón, enfrente", "ve a Daniel cerrando las cortinas"),
                    E("~22:40", "padre", "pasillo", "cierra el cuarto con llave por fuera y se la guarda"),
                    E("~22:45", "victima", "su cama", "muere (el forense: entre 22:30 y 23:15)"),
                    E("23:05", "padre", "puerta de Elena", "aporrea la puerta, abre con la llave del bolsillo y finge encontrarla"),
                    E("~23:05", "hermano", "pasillo", "sale al oír los golpes; ve la llave"),
                    E("~23:05", "madre", "cuarto de Elena", "la despiertan los gritos; ve la taza en la mesilla"),
                    E("23:15", "padre", "casa", "llama al 112"),
                    E("~23:20", "padre", "cuarto de Elena", "retira la taza antes de que llegue la policía"),
                    E("00:00", "hermano", "su cuarto", "termina la partida"),
                };
            case "1B":
                return new List<TimelineEvent>
                {
                    E("21:00", "padre", "casa de Marta", "sale de casa («cena con clientes»)"),
                    E("21:30", "victima", "su cuarto", "se acuesta"),
                    E("21:40", "madre", "cuarto de Elena", "le da el triple de su medicación del corazón"),
                    E("22:00", "madre", "junto a la cama", "sentada, quieta, sin llamar a nadie; luz encendida"),
                    E("22:00", "vecina", "su salón, enfrente", "empieza a verla junto a la cama"),
                    E("22:10", "hermano", "su cuarto", "se quita los cascos y oye «mamá, no quiero más»"),
                    E("~22:40", "victima", "su cama", "muere"),
                    E("23:00", "padre", "casa de Marta", "se va (Marta y el portero)"),
                    E("23:05", "padre", "casa", "vuelve"),
                    E("~23:10", "madre", "arriba", "grita"),
                    E("23:15", "madre", "casa", "llama al 112"),
                };
            case "1C":
                return new List<TimelineEvent>
                {
                    E("15:00", "madre", "hospital", "empieza la guardia"),
                    E("21:45", "hermano", "escalera", "discute con Elena, le arranca el móvil; ella cae y se golpea"),
                    E("21:45", "vecina", "su salón, enfrente", "oye gritos y un golpe; ve a Lucas en la ventana de la escalera"),
                    E("~21:50", "hermano", "escalera", "la acuesta, friega el tercer escalón con lejía y esconde el móvil"),
                    E("21:52", "hermano", "casa", "llama a Daniel llorando"),
                    E("22:15", "padre", "casa", "llega; ve el chichón; no la lleva al hospital"),
                    E("22:15", "vecina", "su salón, enfrente", "ve llegar el coche del padre"),
                    E("23:00", "madre", "hospital", "ficha la salida"),
                    E("~23:00", "victima", "su cama", "deja de respirar"),
                    E("23:15", "padre", "casa", "llama al 112 y después a Carmen"),
                    E("23:20", "madre", "casa", "llega con la ambulancia en la puerta; lejía en la escalera"),
                    E("~23:30", "madre", "cuarto de Lucas", "encuentra el móvil de Elena con la pantalla rota"),
                };
            case "2A":
                return new List<TimelineEvent>
                {
                    E("~02:30", "detective", "La Marea", "dos copas de servicio"),
                    E("03:00", "detective", "comisaría", "vuelve de guardia"),
                    E("04:30", "victima", "La Marea", "tira una copa a Marcos y le llama acosador"),
                    E("04:55", "bar", "La Marea", "desenchufa la cámara"),
                    E("05:00", "victima", "La Marea", "sale sola"),
                    E("05:00", "vecina", "cocina, casa de la curva", "se levanta a encender la cocina de leña"),
                    E("05:05", "bar", "La Marea", "sale en su Volvo ranchera"),
                    E("05:08", "victima", "carretera de la costa", "escribe «Estoy cerca»"),
                    E("~05:10", "bar", "la curva", "la alcanza"),
                    E("05:10", "cartero", "delante de La Marea", "bar a oscuras, sin el Volvo"),
                    E("05:15", "cartero", "oficina de Correos", "entra; GPS hasta las 07:00"),
                    E("05:20", "vecina", "casa de la curva", "ve una ranchera oscura parada sin luces"),
                    E("~06:00", "bar", "La Marea", "vuelve al bar a «limpiar»"),
                    E("06:30", "bar", "La Marea", "termina de «limpiar»"),
                    E("07:00", "cartero", "oficina de Correos", "sale (GPS)"),
                    E("08:30", "detective", "comisaría", "denuncia de la tía; empieza a buscar"),
                };
            case "2B":
                return new List<TimelineEvent>
                {
                    E("~02:30", "detective", "La Marea", "dos copas de servicio"),
                    E("03:00", "detective", "comisaría", "vuelve"),
                    E("05:00", "victima", "La Marea", "sale sola"),
                    E("05:00", "bar", "La Marea", "cierra; la cámara le graba recogiendo hasta las 06:30"),
                    E("05:00", "vecina", "cocina, casa de la curva", "se levanta"),
                    E("05:08", "victima", "carretera de la costa", "escribe «Estoy cerca»"),
                    E("05:12", "cartero", "la curva", "para su furgoneta blanca; Sofía sube"),
                    E("05:12", "vecina", "casa de la curva", "lo ve por la ventana"),
                    E("~05:15", "cartero", "(su mentira)", "hora a la que dice que ya estaba clasificando"),
                    E("06:15", "cartero", "oficina de Correos", "ficha, una hora tarde por primera vez en quince años"),
                    E("06:30", "bar", "La Marea", "termina de recoger"),
                    E("07:30", "cartero", "detrás de su casa", "lava la furgoneta a manguerazos"),
                    E("07:30", "bar", "camino de casa", "le ve lavarla"),
                    E("08:30", "detective", "comisaría", "denuncia de la tía"),
                };
            case "2C":
                return new List<TimelineEvent>
                {
                    E("04:20", "detective", "muelle", "coge un sobre; Sofía le graba con el móvil"),
                    E("04:40", "victima", "La Marea", "vuelve pálida: «algo gordo en el puerto»"),
                    E("05:00", "victima", "La Marea", "sale sola"),
                    E("05:00", "bar", "La Marea", "cierra; la cámara graba toda la noche"),
                    E("05:05", "cartero", "comisaría", "va a dejar un certificado: cerrada y a oscuras"),
                    E("05:08", "victima", "carretera de la costa", "escribe «Estoy cerca»"),
                    E("05:10", "detective", "la curva", "la recoge en su Opel gris"),
                    E("05:10", "vecina", "casa de la curva", "lo ve; ella sube"),
                    E("05:15", "cartero", "carretera nacional", "el GPS le sitúa camino de la oficina"),
                    E("06:30", "bar", "La Marea", "sigue dentro recogiendo (grabación)"),
                    E("08:30", "detective", "comisaría", "denuncia de la tía; se queda el caso"),
                };
            case "3A":
                return new List<TimelineEvent>
                {
                    E("20:30", "padre", "casa de la finca", "discute con Paula"),
                    E("20:40", "victima", "casa de la finca", "escribe a Álex: «Papá está fatal, ha bebido»"),
                    E("20:50", "padre", "casa de la finca", "la golpea; no vuelve a levantarse"),
                    E("21:00", "padre", "quemadero", "quema sus cosas con neumáticos: humo negro"),
                    E("21:00", "vecina", "su finca", "ve el humo"),
                    E("21:30", "padre", "camino de la finca", "sale en la camioneta"),
                    E("21:40", "padre", "gasolinera del pueblo", "compra gasoil en garrafas"),
                    E("21:50", "padre", "finca", "vuelve cargado de garrafas"),
                };
            case "3B":
                return new List<TimelineEvent>
                {
                    E("12:00", "madre", "Granada", "deja a Álex en casa de su hermana"),
                    E("18:30", "padre", "bar Casino", "llega"),
                    E("19:00", "madre", "camino de la finca", "sube en su Ibiza rojo; la camioneta no está"),
                    E("19:00", "vecina", "su finca", "ve subir un coche pequeño rojo"),
                    E("~19:30", "madre", "quemadero", "quema la mochila y las zapatillas de Paula"),
                    E("~19:40", "madre", "coche", "sale con Paula hacia Portugal"),
                    E("21:25", "padre", "bar Casino", "el camarero le cobra"),
                    E("21:30", "padre", "finca", "vuelve borracho; no entra a ver a Paula"),
                    E("04:00", "madre", "Granada", "(domingo) vuelve, con un ticket de peaje de Huelva"),
                };
            case "3C":
                return new List<TimelineEvent>
                {
                    E("19:30", "padre", "bar Casino", "llega"),
                    E("19:50", "victima", "finca", "escribe a Álex: «Voy a despedirme de Encarna y de los caballos»"),
                    E("20:00", "victima", "finca de Encarna", "llega a despedirse"),
                    E("~20:15", "vecina", "junto al pozo", "discuten; la empuja"),
                    E("~21:00", "vecina", "quemadero de Javier", "entra por la cancela con su llave y quema las cosas de Paula"),
                    E("22:00", "padre", "finca", "vuelve borracho del Casino; no entra a verla"),
                    E("00:00", "hermano", "Granada", "lee tarde el mensaje de Paula"),
                };
            default:
                return new List<TimelineEvent>();
        }
    }

    private static TimelineEvent E(string time, string who, string where, string what)
    {
        return new TimelineEvent(time, who, where, what);
    }
}
