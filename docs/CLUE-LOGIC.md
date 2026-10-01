# Lógica de las pistas (Sesión A, 1-10-2026)

Para cada una de las 48 pistas: **quién la sabe y por qué, qué pregunta razonable la saca, cuándo es lógico que
salga** y si la libreta explica qué se ha descubierto. Análisis hecho por historia (tres agentes en paralelo sobre los
datos, las 9 variantes y 250+ partidas del bot), revisado y aplicado a mano con los tests de guarda del validador
(una pista no puede dispararse con la propia ficha; un parte no puede regalar una pista).

**Cómo leer las tablas:** "Detección medida" es la calibración **anterior** a los cambios (48 pistas × 3 intentos);
la de después está al final. "Bot" = veces que salió en partidas reales; muchas llegaron con la ayuda de Pensar, que
sugiere literalmente las preguntas de calibración (se indica cuando importa).

## Historia 1

### 1A · culpable: Daniel (herencia + zolpidem en el cacao)

| Pista | Portador | Por qué lo sabe | Qué pregunta la saca (natural) | Cuándo es lógico que salga | Detección medida | Veredicto |
|---|---|---|---|---|---|---|
| 1A_taza | Carmen | Entró en el cuarto al oír los gritos (23:05) y vio la taza. Sabe que el cacao lo hace Daniel. Es creíble. | «¿Qué vio al entrar en el cuarto de Elena?», «¿Había algo raro en la habitación?». Es natural desde el día 1. | Cualquier día. El parte del día 5 («la mesilla estaba vacía, alguien recogió») refuerza la pista después, pero la libreta no lo conecta. | 100 % · 26/26 (5 espontáneas) | OK (mejorar la libreta) |
| 1A_puerta | Lucas | Salió al pasillo cuando gritó su padre y vio que sacaba la llave del bolsillo. Es creíble. | «¿Qué viste al salir al pasillo?», «¿Cómo estaba la puerta de Elena?». Su versión dice «salí al pasillo», así que invita a preguntar. | Cualquier día. El parte del día 6 empuja a preguntar, pero la frase «tenía la cerradura por fuera» se lee como una cerradura para encerrar a la niña, lo que es raro y engaña. | 100 % · 26/26 (3 espontáneas) | Problema menor (parte 6) |
| 1A_ventana ⚡ | Amparo | Ve el cuarto de Elena desde su salón, con prismáticos, y no duerme. Es creíble y encaja con el epílogo. | «¿Vio algo raro esa noche desde su ventana?». Los bots la sacan solos con «¿oyó o vio algo raro…?». | En cuanto aparece Amparo (con las raíces de la pregunta o por el parte del día 2). | 100 % · 20/26 (19 espontáneas) | OK |
| 1A_papeles | Lucas | Elena le enseñó las fotos de los extractos. Es creíble, y además su «sabe» menciona la discusión por unos papeles. | «¿Te contó Elena algo de tu padre?», «¿Tenía algún secreto?». Pero tras el parte del día 4 (fondo de herencia), lo natural es preguntar «¿sabes algo de la herencia o del dinero de Elena?», y el tema no lo cubre. | Después del día 4, que es cuando el jugador piensa en el dinero. Puede salir antes, pero sin motivo. | 100 % · 6/26 (2 espontáneas) | Problema (el tema no cubre la herencia) |
| 1A_partida | Lucas | Es su propia coartada. | «¿Qué hacías tú esa noche? ¿Alguien lo confirma?». Es lo primero que pregunta cualquiera. | Día 1. | 83 % · 25/26 (25 espontáneas) | OK |
| 1A_frasco (SECRETA) | Carmen | Es su frasco. Calla por vergüenza de ser médica y tomar zolpidem, y por miedo a que la culpen. Es creíble, aunque su versión ya dice «me tomé mi pastilla». | «¿Toma usted pastillas para dormir? ¿Zolpidem?». Después del parte del día 3 («el mismo que toma alguien de la casa») es obligado preguntarlo, pero el jugador escribirá «zolpidem» y el tema solo dice «tus pastillas para dormir». | Después del día 3. Antes el jugador no sabe que el veneno es un somnífero de la casa. | 83 % · 4/26 (4 espontáneas) | Problema (la libreta no dice por qué importa; el tema no nombra el zolpidem) |

### 1B · culpable: Carmen (Münchausen por poderes)

| Pista | Portador | Por qué lo sabe | Qué pregunta la saca (natural) | Cuándo es lógico que salga | Detección medida | Veredicto |
|---|---|---|---|---|---|---|
| 1B_historial | Daniel | Es el padre y conoce las visitas a urgencias. Es creíble. | «¿Cómo era la salud de Elena?». El parte del día 3 («pasaba mucho por urgencias») empuja a preguntar a **Carmen**, la pediatra que lleva a Elena al médico, y no a Daniel. Nada señala a Daniel como quien lo cuenta. | Después del día 2 o 3. | 83 % · 16/16 (0 espontáneas: todas por la ayuda) | Problema (depende de la ayuda; falta un puntero a Daniel) |
| 1B_receta | Daniel | Pidió una segunda opinión y Carmen se enfadó. Es creíble. | «¿Tomaba Elena alguna medicación? ¿Quién se la recetaba?». Es natural después del parte del día 2 (fármaco para el corazón). | Después del día 2. | 83 % · 16/16 (3 espontáneas) | OK (le afecta el mismo puntero que a historial) |
| 1B_pared ⚡ | Lucas | Su cuarto está pegado al de Elena y se quitó los cascos un momento. Es creíble, y su versión («solo me los quité un momento») invita a preguntar. | «¿Oíste algo esa noche?», «¿Cuándo te quitaste los cascos?». | Cualquier día. | 100 % · 7/16 (7 espontáneas) | OK |
| 1B_luz ⚡ | Amparo | Ve el cuarto desde enfrente, con prismáticos. Es creíble. | «¿Qué vio esa noche desde su ventana?». | En cuanto aparece Amparo. El parte del día 6 («alguien pudo pedir ayuda antes») la refuerza. | 100 % · 7/16 (7 espontáneas) | OK (errata en el epílogo) |
| 1B_tutora | Lucas | Elena se lo contó a su hermano. Es creíble. | «¿Te contó Elena algo estos días?». Pero después del parte del día 4 (la tutora) el jugador preguntará «¿sabes por qué Elena habló con su tutora?», y el tema no nombra a la tutora. | Después del día 4. | 100 % · 8/16 (1 espontánea; el resto por la ayuda, casi siempre el día 7) | Problema (el tema no enlaza con el parte 4) |
| 1B_cena (SECRETA) | Daniel | Es su coartada: una aventura con Marta. Callar es creíble, porque tiene una aventura. | «¿Dónde estuvo de verdad? El restaurante no tiene su reserva». El parte del día 5 da la palanca exacta. | Después del día 5, o antes si el jugador duda y dice que lo va a comprobar. | **50 % FALLA** · 5/16 (5 espontáneas) | Problema (anclas: Daniel confiesa «fue una aventura» sin decir «Marta») |

### 1C · culpable: Lucas (caída en la escalera, encubierta)

| Pista | Portador | Por qué lo sabe | Qué pregunta la saca (natural) | Cuándo es lógico que salga | Detección medida | Veredicto |
|---|---|---|---|---|---|---|
| 1C_gritos ⚡ | Amparo | Oyó los gritos desde enfrente y vio a Lucas en la ventana de la escalera. Es creíble. | «¿Oyó o vio algo raro esa noche?». | En cuanto aparece Amparo. | 83 % · 17/18 (17 espontáneas) | OK |
| 1C_lejia | Carmen | Llega a las 23:20 y el olor es fuerte. Es creíble. | «¿Notó algo raro al llegar a casa?». | Cualquier día. | 100 % · 17/18 (2 espontáneas) | OK |
| 1C_pantalla | Carmen | Encontró el móvil «esa misma noche» debajo de la cama de Lucas. **Es poco creíble**: con la ambulancia y la policía en casa, ¿qué hacía mirando debajo de la cama de su hijo? Además, el parte del día 5 («nadie encuentra el móvil») implica que no se lo dio a la policía, y aun así lo cuenta sin resistencia a la primera pregunta. | «¿Dónde está el móvil de Elena?». Es natural después del día 5. | Después del día 5, cuando el jugador sabe que falta el móvil. | 100 % · 18/18 (4 espontáneas) | Problema (motivo y momento incoherentes) |
| 1C_llamada (SECRETA) | Daniel | Lucas le llamó. Callar para proteger a su hijo es creíble. Pero su «oculta» no menciona la llamada, solo la hora de llegada. | «¿Recibió alguna llamada?» o, mejor, enfrentarle con lo que sabe Amparo: «la vecina vio su coche llegar a las diez y cuarto». Esa palanca existe (está en el «sabe» de Amparo), pero no está en sus condiciones de admisión. El parte del día 4 (registro de llamadas) sí es una palanca, pero el jugador tiene que farolear. | Después del día 4, o después de hablar con Amparo del coche. | **33 % FALLA** · 1/18 (0 espontáneas) | Problema grave (casi inalcanzable) |
| 1C_fichaje | Carmen | Es su coartada. | «¿Dónde estaba usted esa noche?». Su versión ya dice «estaba de guardia». | Día 1. | 100 % · 15/18 (≈13 espontáneas) | OK |

### Personaje bloqueado: Amparo (vecina)
- Motivo para que aparezca: la situación del caso dice «Enfrente vive una vecina que duerme poco» y los tres Mendoza tienen en su «sabe» que «Amparo… se pasa las noches mirando por la ventana». Preguntar por ella es lógico, y las raíces (`vecin, enfrente, ventana, cortina, testig, desde fuera, alguien vio, amparo`) lo cubren. Si el jugador no pregunta, el parte del día 2 la trae con un motivo propio («no durmió y quiere hablar»). **OK**, no es un «día N mágico».
- Detalles: «cortina» solo tiene sentido en 1A. Faltan formas habituales como «quién más», «alguien oyó», «oyó algo». «ventana» se dispara con preguntas que no tienen nada que ver («¿estaba abierta la ventana de Elena?»), pero es aceptable.

### Orden de los partes
- **1A**: día 2 hora y somnífero → día 3 zolpidem de la casa (frasco) → día 4 herencia (papeles) → día 5 mesilla vacía (taza) → día 6 cerradura (puerta). El orden es bueno, pero el texto del día 6 confunde.
- **1B**: día 2 fármaco del corazón (receta) → día 3 urgencias (historial, pero señala a Carmen) → día 4 tutora (tutora, pero el tema no enlaza) → día 5 restaurante (cena) → día 6 «pudo pedir ayuda» (luz o pared). El orden es bueno. Los partes 3 y 4 apuntan a gente que no es la que tiene la pista.
- **1C**: día 2 golpe → día 3 dentro de la casa → día 4 registro de llamadas (llamada) → día 5 móvil (pantalla) → día 6 pastillas (motivo). El orden es bueno. El día 4 no da ninguna palanca usable contra Daniel.

### Cambios aplicados (Sesión A)
- **1C_llamada** (33 % → a medir): Daniel también cede si le dicen que **alguien vio su coche llegar antes de las
  23:00** (la palanca natural: Amparo), no solo con el registro de llamadas; tema "las llamadas de esa noche o a qué
  hora llegaste de verdad"; el parte del día 4 dice que habrá que saber quién llamó a quién. *No* se añadió la llamada
  a su secreto: el test de guarda mostró que copiaba la pista en la ficha (la pista, secreta, ya le dice confesarla).
- **1B_cena** (50 %): el detector exigía "Marta" y Daniel confiesa "fue una aventura, no cené en ningún restaurante".
  Primer grupo de anclas: marta / amante / otra mujer / una aventura con / ningún restaurante. *No* se nombró a Marta
  en su secreto: la pista se habría disparado con su propia ficha (test de guarda).
- **1B_historial**: el parte del día 3 dice "siempre con su madre" (antes mandaba a preguntar solo a Carmen); lo que
  sabe Lucas incluye que papá quiso pedir una segunda opinión (redactado sin "otro médico", ancla de su propia pista).
- **1B_tutora**: tema enlazado con el parte del día 4 (la tutora) y pregunta de calibración nueva.
- **1C_pantalla**: Carmen encuentra el móvil **a la mañana siguiente** y lo calla por miedo (encontrarlo "esa misma
  noche", con la ambulancia en casa, y no dárselo a la policía no se sostenía); parte del día 5 con "nunca se separaba
  de él".
- **1C**: lo que sabe Carmen ya no regala el motivo del culpable el día 1 ("algo del instituto").
- **1A_papeles / 1A_frasco**: temas con "su dinero" y "el zolpidem" (lo que el jugador pregunta tras los partes 4 y
  3); la libreta del frasco explica por qué importa.
- **1A_puerta**: el parte del día 6 ya no dice "cerradura por fuera" (sugería una cerradura para encerrar a la niña):
  "tiene cerradura. ¿Quién la cerró esa noche?" (con "con llave" el parte regalaba la pista: test).
- **1A_taza**: la libreta enlaza con el parte del día 5 (la taza ya no estaba cuando llegó la policía).
- **Amparo**: raíces de desbloqueo sin "cortina" (solo valía en 1A) y con "alguien oy". *No* se añadió "quien mas":
  "¿quién más estaba en casa?" la traería sin motivo.
- **Rosario → Amparo** (decisión 4) en todo lo que ven el jugador y el modelo; errata del epílogo de 1B.

## Historia 2

### 2A · culpable Marcos (bar)

| Pista | Portador | Por qué lo sabe | Qué pregunta la saca (natural) | Cuándo es lógico que salga | Detección medida | Veredicto |
|---|---|---|---|---|---|---|
| 2A_copa | Ruiz | Se lo contaron varios testigos en las primeras horas (él ya se había ido del bar a las 3:00). No lo usó porque cree que fue una caída | «¿Qué pasó en el bar esa noche?» / tras el parte D3 (gritos a las 4:30): «¿Qué fueron los gritos de La Marea?». Muy natural | Cualquier día; el parte D3 lo empuja | cal. 100 % · bot 8/10 | OK |
| 2A_camara | Ruiz | Revisó la cámara. La omite del informe (parte D5), cosa creíble porque también grabaría sus copas de servicio (su secreto) | «¿Hay cámaras en el bar?». Es la pregunta clásica, y el parte D5 la hace obligada | Desde D1; D5 la señala | cal. 100 % · bot 10/10 | OK (el motivo de la omisión no lo dice la ficha, ver 9) |
| 2A_aparcamiento ⚡ | Andrés | Pasa cada madrugada por La Marea camino de la oficina (lo dice en su versión) | «¿Qué vio al pasar por La Marea a las 5:10?». Es natural porque él mismo menciona el paso. PERO el jugador suele preguntar «¿vio algo raro/extraño cerca del bar?» y Andrés contesta «nada raro»: para él un bar a oscuras tras cerrar es normal | Después de oír a Marcos decir «no salí del bar» (lo dice sin que se lo pidan); sin problema de orden | cal. 100 % · bot **3/10** | **Problema**: el hecho no le parece raro al portador, y la libreta no dice qué mentira rompe |
| 2A_curva | Maruxa | Madruga y mira por la ventana, que da a la curva | «¿Vio algo raro en la curva / desde su casa esa madrugada?». Muchos jugadores preguntan por «la cala» (el brief solo nombra la cala), y entonces falla | Tras el parte D4 (neumático de coche grande) encaja aún mejor | cal. 67 % · bot 10/10 | OK con retoque (ver 4) |
| 2A_gps | Ruiz | Comprobó el GPS de Correos | «¿Comprobaron dónde estaba el cartero?». Aceptable. Es más natural preguntarle a Andrés «¿alguien lo confirma?», y ahí no sale nada | Cuando se sospecha de Andrés | cal. **50 %** · bot 3/10 | Problema leve (tema estrecho) |

### 2B · culpable Andrés (cartero)

| Pista | Portador | Por qué lo sabe | Qué pregunta la saca (natural) | Cuándo es lógico que salga | Detección medida | Veredicto |
|---|---|---|---|---|---|---|
| 2B_cartas | Marcos | Sofía era clienta de cada noche y se las enseñó hace una semana | «¿Tenía Sofía miedo de alguien / problemas este verano?». Muy natural; el parte D3 (buzón) la refuerza | Cualquier día | cal. 100 % · bot 28/28 | OK |
| 2B_fichaje ⚡ | Ruiz | Tiene en comisaría los registros de Correos (parte D5). Como cree que fue una caída, no los miró con interés | Un humano, tras D3 (buzón) + D4 (sobre sin sello) + D5 («los registros los tiene Ruiz»), sí iría a Ruiz: «¿Me enseña los registros de fichaje? ¿A qué hora fichó Andrés?». Eso es **plausible** para un humano. PERO el tema es «los horarios del cartero» y la ficha de Ruiz no dice que tenga los registros. Cuando el bot preguntó justo por «anomalías en los registros de fichaje», Ruiz **inventó lo contrario**: «Andrés siempre ficha puntualmente a las 5:15… nada raro en los registros». Otra vez entendió que Andrés «vino a fichar» a comisaría. El humano que acierta recibe un desmentido | D5 en adelante. Además el parte D5 no nombra al cartero («todos sus empleados») | cal. 100 % (con la pregunta exacta) · bot **1/28** | **Problema grave**: el tema no cubre «registros/fichaje» y el parte D5 es ambiguo |
| 2B_furgoneta | Maruxa | Lo vio desde la ventana a las 5:12 | «¿Vio algo en la curva / pasar algún vehículo?». Natural; D6 («furgoneta pequeña, clara») la refuerza. Falla si se pregunta por la cala o «algo raro por el bar» | Cualquier día | cal. **50 %** · bot 28/28 | OK con retoque (ver 4) |
| 2B_manguera | Marcos | Volvía a casa a las 7:30 (lo dice en su versión) y pasa por detrás de casa de Andrés | «¿Vio algo al volver a casa?» solo se le ocurre a quien se fija en el «a casa hacia las siete y media». Casi todos los aciertos del bot son la pregunta de calibración copiada. Un humano, tras D6 («furgoneta pequeña, clara»), preguntaría «¿Quién tiene una furgoneta blanca?», y eso no está en el tema | Tras D6 o tras 2B_furgoneta | cal. **50 %** · bot 27/28 (inflado) | **Problema**: requiere adivinar el tema |
| 2B_imagenes | Ruiz | Revisó la cámara del bar | «¿Hay grabaciones de cámaras?». Natural | Cualquier día | cal. 67 % · bot 15/28 | OK |

### 2C · culpable Ruiz (inspector)

| Pista | Portador | Por qué lo sabe | Qué pregunta la saca (natural) | Cuándo es lógico que salga | Detección medida | Veredicto |
|---|---|---|---|---|---|---|
| 2C_puerto | Marcos | Sofía volvió al bar a las 4:40 y se lo contó. No se lo dijo a Ruiz porque ella «no se fiaba de la policía», y a él el puerto le toca de cerca (tabaco de contrabando) | «¿Cómo estaba Sofía esa noche?». Muy natural. Tras D3 (reportaje del puerto) se pregunta «¿Qué sabe del reportaje de Sofía?», y eso no está en el tema | Cualquier día; D3 la señala | cal. **50 %** · bot 18/18 | OK con retoque (ver 6) |
| 2C_opel ⚡ | Maruxa | Lo vio desde la ventana y conoce todos los coches del pueblo. Callar es creíble: es el coche del policía y ella esconde el orujo | «¿Vio algo en la curva / algún coche?». Natural | Cualquier día | cal. **50 %** · bot 13/18 | OK con retoque (ver 4) |
| 2C_comisaria ⚡ | Andrés | Llevaba un certificado urgente a la comisaría (está en su versión) | La cadena natural sería: Ruiz dice «estuve en comisaría hasta las seis», y el jugador le pregunta a Andrés «¿Vio a Ruiz en la comisaría?». Pero Andrés rara vez menciona la comisaría. A «¿vio algo raro en su ruta?» contesta «nada», porque la ficha no dice que le extrañara. Casi todos los aciertos del bot vienen de «¿alguien puede confirmar dónde estaba?» | Después de oír la coartada de Ruiz. Ningún parte apunta al certificado | cal. 100 % · bot **6/18** | **Problema**: depende de un detalle de la versión que el LLM omite; ningún parte lo señala |
| 2C_prueba | Ruiz | Él registró el móvil y lo hizo desaparecer. Admite solo lo que consta en papel («se extravió») | Tras D4 («el móvil no está entre las pruebas»): «¿Dónde está el móvil de Sofía?». Muy natural para un humano (el bot casi nunca la hizo) | D4 en adelante | cal. **50 %** · bot 1/18 (fallo del bot, no del diseño) | OK con retoque (ver 7) |
| 2C_gps | Andrés | GPS de su furgoneta de Correos | «¿Alguien puede confirmar dónde estaba?». Natural | Cualquier día | cal. 67 % · bot 18/18 | OK |
| 2C_grabacion | Marcos | Su cámara. La entregó a la Guardia Civil y no a Ruiz, lo que encaja con la desconfianza | «¿Alguien puede confirmar dónde estaba? / ¿Tiene cámaras?». Natural | Cualquier día | cal. 83 % · bot 15/18 | OK |

**Orden de los partes:**
- 2A es coherente: D3 gritos → copa; D4 neumático → curva/aparcamiento; D5 cámara; D6 acoso.
- 2B tiene buen orden: D3 buzón y D4 sobre sin sello apuntan al cartero, D5 lleva a Ruiz y D6 a la furgoneta. Falla solo la redacción de D5.
- 2C: D3 → Marcos, D4 → Ruiz (móvil), D5–D6 → Ruiz. Ningún parte lleva al segundo ⚡ (comisaría vacía).

### Cambios aplicados (Sesión A)
- **2B_fichaje** (⚡; el bot la sacó 1 vez en 28): la ficha de Ruiz dice que **tiene en comisaría los registros de
  fichaje de Correos** (antes, preguntado por ellos, inventaba "ficha puntual"); tema con "registros de fichaje"; el
  parte del día 5 dice que Correos los envió a la comisaría de Ruiz, incluido el del cartero.
- **2A_aparcamiento / 2C_comisaria** (⚡): a Andrés le **extrañó** lo que vio (el bar a oscuras sin el Volvo; la
  comisaría cerrada en fiestas): antes, a "¿vio algo raro?" contestaba "no". El parte del día 6 de 2C menciona el
  certificado urgente sin firmar. Las libretas dicen qué mentira rompen.
- **Maruxa (2A_curva, 2B_furgoneta, 2C_opel)**: temas con "desde tu ventana" (lo que se pregunta de verdad).
- **2B_manguera**: tema con "alguien con una furgoneta blanca pequeña" (la pregunta natural tras el parte del día 6).
- **2C_puerto, 2C_prueba, 2A_gps**: temas enlazados con sus partes (el reportaje, las pruebas enviadas a Vigo, la
  coartada del cartero).
- Cada cambio de tema lleva su pregunta de calibración nueva.

## Historia 3

### 3A · culpable: Javier (padre)

| Pista | Portador | Por qué lo sabe | Qué pregunta la saca (natural) | Cuándo es lógico que salga | Detección medida | Veredicto (OK / Problema) |
|---|---|---|---|---|---|---|
| 3A_mensaje ⚡ | Álex | Paula le escribió a él (se lo cuenta todo). Plausible. | «¿Cuándo hablaste con Paula por última vez?» / «¿Te escribió ese fin de semana?». Natural para cualquier desaparición. | En cuanto aparece Álex (D1-2, por mención o parte D2 «con el móvil en la mano»). Sin problema de orden. | 100 % · 23/34 (8 natural, 15 sugerida) · casi siempre D6-7 | **Problema**: su ficha le manda a la vez contarlo (hecho) y ocultarlo (secreto: «Paula te pidió ayuda y no se lo dijiste a nadie»). Contradictorio y hace que salga tarde. Además nada de las mañanas empuja a preguntar por el móvil. |
| 3A_humo | Encarna | Vive al lado; desde su ventana ve la finca. | «¿Vio el humo del sábado? ¿A qué hora?». El brief e intro ya hablan del humo visto desde la finca de al lado. | D2-3 (Encarna aparece por mención o parte D3). D3 el laboratorio confirma neumático → encaja con «goma». | 100 % · 34/34 (30 natural) · D3 | OK |
| 3A_garrafas | Encarna | Misma ventana; ve la camioneta salir y volver. | «¿Salió o entró alguien de la finca esa noche?» / «¿Vio la camioneta?». Natural tras el humo. | D3; el parte D4 (gasolinera 21:40) lo corrobora. Buen orden. | 67 % · 34/34 (20 natural) | OK |
| 3A_audios | Lucía | Los recibió ella; odia a Javier y lo contaría. | «¿Cómo era su relación con Javier? ¿La había amenazado?». Natural en un divorcio con vista de custodia. | Desde D1. El parte D5 (Paula iba a declarar) refuerza el móvil. | 67 % · 33/34 (1 natural, 32 sugerida) | OK (lógica); el bot no la pide solo, un humano sí. Riesgo: a veces niega («no me consta que me amenazara»). |
| 3A_granada (descarta a Lucía) | Álex | Estuvo con ella en casa de la tía Remedios; tía y primos Nerea y Hugo lo confirman. | «¿Dónde estabais tu madre y tú el sábado? ¿Quién lo confirma?». Muy natural. | D1-2. | 50 % (medido antes de añadir «remedios»/«toda la jornada»; re-medir) · 30/34 (27 natural) | **Problema menor**: el secreto de Álex («bebiendo con tus primos en Granada») sugiere que salió de fiesta, lo que choca con «todo el sábado en casa de tu tía» con su madre. |

### 3B · culpable: Lucía (madre) — Paula está viva

| Pista | Portador | Por qué lo sabe | Qué pregunta la saca (natural) | Cuándo es lógico que salga | Detección medida | Veredicto (OK / Problema) |
|---|---|---|---|---|---|---|
| 3B_coche ⚡ | Encarna | Ve el camino; su ficha dice «conoces el coche de todos los que suben». | «¿Vio algún coche o a alguien subir a la finca el sábado?». Natural. | D3 (aparición de Encarna). | 100 % · 20/34 (19 natural) · D3 | **Problema**: la libreta dice «un coche pequeño rojo», pero no de quién. Quien lo sabe es Javier (conocimiento, no pista) y el jugador tiene que adivinar que debe preguntarle «¿qué coche tiene Lucía?». Incoherente con «conoces el coche de todos». Sin ese enlace la pista ⚡ no destapa nada. Además en 3B nadie tiene la hora del humo (el bot pregunta y Encarna se la inventa: «21:00»). |
| 3B_carta | Javier | La encontró el domingo en la mesilla de Paula. | «¿Encontró algo en el cuarto de Paula? ¿Paula hablaba con su madre?». Natural. | D1 (Javier empieza desbloqueado). Javier odia a su ex: lógico que la saque. | 100 % · 33/34 (1 natural) · D3 | OK. El modelo a veces tergiversa «tú hazme caso» → «yo debía hacerle caso»; la libreta lo deja claro. |
| 3B_noche ⚡ SECRETA | Álex | Lucía le dejó a mediodía en casa de la tía y volvió a las 4 con un ticket de peaje de Huelva. | Lo natural tras el coche rojo o el parte D4 (coche de Granada a Huelva): «¿Tu madre estuvo contigo todo el sábado? ¿A qué hora volvió?» + insistir. | D4 o después (parte de la autopista). Buen orden si el coche rojo ya apunta a Lucía. | 83 % · 12/34 (10 natural) | **Problema**: la versión de Álex («estuve en casa de mi tía Remedios») no incluye la mentira que su madre le pidió, así que no la defiende y lo suelta sin presión (D2 en 5 partidas). El secreto no funciona como secreto. |
| 3B_armario | Álex | Vive con su madre; buscando a Paula el domingo nota que faltan maleta y pasaporte. | Antes de D5 hay que adivinar («¿Echas en falta algo de Paula?»). Tras el parte D5 (pasaporte) es natural: «¿Sabes dónde está el pasaporte de Paula?». | D5 (parte del pasaporte). | 100 % · 34/34 (2 natural, 32 sugerida) · D5 | **Problema menor**: el tema solo se alcanza adivinando; el parte D5 casi repite la pista. |
| 3B_cuenta (descarta a Javier) | Javier | Estuvo en el Casino; el camarero le cobró 21:25. | «¿Dónde estuvo el sábado? ¿Alguien lo puede confirmar?». Natural. | D1. | 50 % · 8/34 (3 natural) | **Problema**: a «¿dónde estuvo?» responde su versión (bar 18:30-21:30) sin camarero, y el ancla exige camarero/21:25. El jugador oye la coartada pero no se apunta. |

### 3C · culpable: Encarna (vecina)

| Pista | Portador | Por qué lo sabe | Qué pregunta la saca (natural) | Cuándo es lógico que salga | Detección medida | Veredicto (OK / Problema) |
|---|---|---|---|---|---|---|
| 3C_despedida ⚡ | Álex | Paula le escribió a las 19:50. | «¿Cuándo supiste de Paula por última vez? ¿Qué te dijo que iba a hacer?». Natural. | D1-2. El parte D6 (amigas: «le daba pena despedirse de alguien») la refuerza, pero tarde. | 83 % · **4/26** | **Problema grave (detector)**: el ancla «despedir» no casa con «despidiéndose» / «se despidió» (cambio e→i). En ≥6 partidas Álex lo dijo y no se apuntó. Además el modelo lo convierte en «la vi en la finca a las 19:50» (como si estuviera allí). Su secreto también menciona «el último mensaje», que el modelo confunde con ocultarlo. |
| 3C_llave | Javier | Es su cancela; sabe quién tiene llave. | «¿Quién más puede entrar en su finca?»; tras el parte D4 (candado recién engrasado), «¿Quién tiene llave de la cancela?». Natural tras D4. | D4. | 50 % · 26/26 (0 natural, todas sugeridas) | **Problema**: la pregunta 2 de calibración («¿otra entrada al quemadero?») lleva a «solo desde mi finca» sin nombrar a Encarna. El tema no recoge «cancela/candado», que es lo que el parte hace preguntar. La libreta no dice por qué importa. |
| 3C_fotos | Lucía | Los años de vecinas y lo que le contaba Paula. | «¿Qué opina de Encarna? ¿Qué relación tenía con Paula?». Natural cuando Encarna empieza a sonar (cancela D4, pozo D5). | D4-5. | 100 % · 26/26 (0 natural) · D5 | OK. |
| 3C_pisadas | Javier | Vio el quemadero el domingo por la mañana. | «¿Encontró algo en el quemadero?». Natural tras el parte D2 (restos). | D2+. | 83 % · 21/26 (8 natural) | OK (detalle: si miró la ceniza, raro que no viera la mochila; pasable). |
| 3C_bar (descarta a Javier) SECRETA | Javier | Estuvo en el Casino 19:30-22:00. | «¿Dónde estuvo el sábado y a qué hora volvió?». Natural. | D1. | 33 % · 14/26 | **Problema**: la coartada no es vergonzosa, lo vergonzoso es no entrar a ver a Paula (ya está en su secreto). Al ser SECRETA el modelo la esconde e incluso inventa «estuve en casa toda la tarde». En 3B la misma coartada es abierta: incoherente. |

### Personajes bloqueados

| Personaje | Disparadores | Fallback | Veredicto |
|---|---|---|---|
| Álex | Menciones («Álex, el hermano de Paula, es con quien ella más habla» en Javier y Lucía) + temas: hermano, alex, mensaje, whatsapp, movil de paula, con quien hablaba. Aparece D1-2 en todas las partidas. | D2: «llega… con el móvil en la mano: quiere enseñarle algo». | Motivo claro en 3A/3C. **En 3B engaña**: Álex no tiene nada que enseñar en el móvil (lo que sabe es de su madre). |
| Encarna | Menciones (Javier y Lucía la citan) + temas: vecin, finca de al lado, caballo, cancela, pozo, quien vio, encarna. Aparece D2-3. | D3: «la dueña de la de al lado vio humo el sábado». | Motivo claro (la intro dice que el humo se vio desde la finca de al lado). Faltan disparadores obvios: «testigo», «alguien vio». En 3B promete un humo cuya hora nadie tiene. |

### Partes de la mañana (orden)

- **3A**: restos (D2) → neumático/gasoil (D3, confirma humo) → gasolinera 21:40 (D4, confirma garrafas) → Paula iba a declarar (D5, móvil de Javier / audios) → perros en el olivar (D6). Buen orden. Falta algo que empuje hacia el móvil de Paula (el ⚡ sale en D6-7).
- **3B**: restos → solo ropa (D3, sugiere que no hay cuerpo) → coche de Granada a Huelva (D4, apunta a Lucía y a la noche larga) → pasaporte (D5, armario/Portugal) → llamada portuguesa (D6). Buen orden, pero depende de que el coche rojo ya se asocie a Lucía.
- **3C**: restos → barro (D3) → candado engrasado (D4, llave) → perros junto al pozo (D5) → amigas: Madrid y despedida (D6). La única pista que apunta al ⚡ (despedida) llega en D6; nada anterior lleva a preguntar a Álex por los planes de Paula.

### Cambios aplicados (Sesión A)
- **3C_despedida** (⚡; 4 de 26 partidas): el ancla "despedir" no casaba con "despidiéndose" ni "se despidió" → anclas
  despedir / despid / despedida / adiós (test con las tres formas: en rojo con los datos viejos). El hecho dice "estando
  tú en Granada, por WhatsApp" (el modelo lo convertía en "la vi en la finca").
- **Álex**: en 3A su secreto le pedía ocultar lo mismo que su pista le pide contar → ahora oculta **por qué no fue**
  (bebía con sus primos en casa de la tía Remedios); en 3B su versión incluye la mentira que su madre le pidió ("mamá
  y yo estuvimos todo el día…"); en 3C el secreto ya no dice "mensaje" (para que no lo esconda).
- **3B_coche** (⚡): "un coche pequeño rojo, **como el de Lucía**" (Encarna conoce el coche de todos); ancla "coche de
  lucia".
- **3B_cuenta / 3C_bar** (coartadas de Javier, 33-50 %): la versión de 3B ya nombra el bar Casino (sin el camarero,
  que dispararía la pista con su propia ficha); anclas con "me vio / me vieron"; 3C_bar deja de ser SECRETA (lo
  vergonzoso ya está en su secreto).
- **3C_llave**: tema "quién tiene llave de la cancela", anclas con "candado".
- **Partes**: 3A día 5 y 3C día 3 dicen a qué hora se conectó por última vez el móvil de Paula (las ⚡ ya no dependen
  solo del día 6). **3B_armario**: tema con el pasaporte (el parte del día 5).
- **Desbloqueos**: el texto de reserva de Álex ya no promete "el móvil en la mano" (falso en 3B) y el de Encarna ya no
  promete humo (sin hora en 3B); Encarna suma "alguien vio" y "testig". *No* se añadió "última vez que" a Álex: la
  pregunta de ejemplo "¿Cuándo supiste de Paula por última vez?" lo traería con el primer toque.

## Después de los cambios (medido)

| Medida | Antes (cierre del día 3) | Después (Sesión A) | Objetivo |
|---|---|---|---|
| Pistas ≥ 2/3 (calibración completa, 48 × 3 intentos) | 36/48 · media 80 % | **41/48 · media 83 %** | ≥ 80 % ✓ |
| Premisas falsas aceptadas (sonda de 72) | 5 % | **1 %** | ≤ 5 % ✓ |
| Estados emocionales coherentes (144) | 95 % | **95 %** (bien formados 99 %) | ≥ 95 % ✓ |
| Bot en las 9 variantes (18 partidas, semilla 1919) | 14/18 | **14/18** (primera pista a las 6,2 preguntas) | ≥ 14/18 ✓ |

**Pistas que bajaron tras los cambios** (las miré una a una, no me quedé con la media):
- **1C_pantalla** (100 → 17 %) y **2C_prueba** (50 → 11 %) y **1A_frasco**: el sospechoso **sí** lo contaba, con
  palabras que las anclas no esperaban ("**lo** encontré… con la pantalla rota", "el zolpidem estaba casi vacío",
  "se registró como prueba y… son cosas que pasan"). Anclas ampliadas y esas respuestas reales fijadas como muestras
  (test): **83 %, 78 % y 67 %**.
- **3C_bar**: una pregunta de calibración era de cuando la pista era secreta ("¿entró a ver a Paula?"); cambiada por
  la que haría un jugador: **67 %**.
- **Siguen por debajo de 2/3:** **1B_cena** (33 %; Daniel no confiesa la aventura ni insistiendo: comportamiento del
  modelo, no del detector; antes 50 %, diferencia dentro del ruido de 6 respuestas) y **1C_llamada** (56 %; antes
  33 %). Con las pistas recalibradas, 46 de 48 pasan de 2/3.
- Memoria entre partidas en esa ronda del bot: 1 aviso, y era un **falso positivo** (misma variante y pregunta, y
  Lucía respondió palabra por palabra lo mismo que en la partida anterior porque es su ficha). El chequeo ya
  descuenta lo que la propia partida vuelve a decir (test): 0 fugas reales en 17 partidas.

### Sesión B (01-10-2026): segunda vía para 1B_cena y 1C_llamada, probada y revertida
- **Intento:** 1B_cena: Amparo vio a Daniel volver pasadas las once con una mujer de copiloto (en su conocimiento, no
  como pista nueva: 1B ya tiene 6 y el diseño es 5-6) y Daniel admite la aventura si se lo echan en cara; tercera
  pregunta de calibración con esa palanca. 1C_llamada: el parte del día 4 dice que Daniel recibió una llamada corta
  antes de las diez, y Daniel lo admite si el inspector se lo dice.
- **Medida (qwen2.5:7b, temperatura 0,6, mismos worktrees de main y de la rama):** con 3 intentos parecía subir (33 →
  56 % las dos), pero con `-tries 10` no: **1B_cena 7/20 = 35 % en main, 6/30 = 20 % en la rama; 1C_llamada 16/30 =
  53 % en main, 11/30 = 37 % en la rama.** La pregunta con la palanca de Amparo acertó 1 de 10: Daniel niega la
  aventura aunque le pongan delante al testigo. Es el modelo, no la falta de vía.
- **Revertido** (`848a690`). Las dos siguen siendo las más débiles; la siguiente idea es que la pista la *dé* otro
  personaje (otro portador), lo que exige que una pista admita dos portadores (cambio de código en TurnAnalyzer,
  PromptBuilder, HintAdvisor y el calibrador), o subir 1B a 7 pistas cambiando el diseño de 5-6. Las dos son
  decisiones de Cristian.
- Otras pistas que cambiaron de lado entre main y la rama con 3 intentos se midieron también con 10: 1A_papeles
  80 % / 60 %, 1C_pantalla 50 / 40 %, 2C_grabacion 50 / 45 %, 3C_bar 85 / 90 %. El detector decide exactamente igual
  que antes (`NegationEquivalenceTests`, todas las anclas sobre todos los textos), así que son variación del modelo.

## Sesión C (01-10-2026): pistas con dos portadores y pistas bajo el 70 %

Medido con qwen2.5:7b-instruct, temperatura 0,6, **10 intentos por pregunta**, worktrees de `main` (`a05a686`) y de la
rama `feature/pistas-dos-portadores`. Una fila por portador: con dos, cada uno se calibra con sus preguntas.

### Dos portadores (punto 4)
Una pista puede tener portadores extra (`ClueData.alsoHeldBy`): cada uno la cuenta con su tema, su hecho, sus anclas,
sus preguntas y su resumen en la libreta. Sale con cualquiera; el estado recuerda **quién** la contó (también en el
guardado), así que la ficha de un portador nunca "recuerda" una confesión que hizo el otro.

| Pista | Segundo portador | Por qué lo sabe |
|---|---|---|
| 1B_cena | Amparo (vecina) | La cotilla de enfrente conoce a Marta, «la del bufete»: otras noches la ha visto traer a Daniel a casa; esa noche él volvió pasadas las once con ella detrás en su coche |
| 1C_llamada | Amparo (vecina) | Ya veía a Lucas en la ventana de la escalera tras el golpe (1C_gritos): unos minutos después le ve llorando al móvil; a las 22:15 llega el coche del padre |
| 2B_imagenes | Marcos (bar) | La cámara es suya. Ruiz tiene motivos para callarlo (bebió en La Marea); Marcos prefiere una multa (la cámara también graba copas a menores) a una acusación de asesinato |

### Las pistas bajo el 70 % (bloque A)
48 pistas ×10 en main: 42/48 ≥ 2/3, media 84 %. Seis bajo el 70 %, clasificadas por su causa leyendo cada respuesta:

| Pista | main | Causa | Arreglo | Rama (×10) |
|---|---|---|---|---|
| 1B_cena | 45 % | Modelo: Daniel mantiene la cena o inventa otra coartada | Segundo portador | Daniel 15 % · **Amparo 90 %** |
| 1C_pantalla | 45 % | Pregunta: la 2.ª preguntaba por «esa noche» y el hecho es de la mañana siguiente | «¿Ha encontrado algo de Elena al recoger la casa estos días?» | **90 %** |
| 1C_llamada | 43 % | Modelo (12 negaciones) y anclas (5 respuestas con «lloraba», «una llamada de mi hijo») | Anclas + segundo portador | **Daniel 60 %** · Amparo 50 % |
| 2B_imagenes | 60 % | Diseño: el portador (Ruiz) tiene motivos para callarlo | Segundo portador | Ruiz 60 % · Marcos 45 % |
| 2C_grabacion | 60 % | Anclas: «seis y media» en letra, «recogiendo», «todo el rato» | Anclas (las respuestas reales, como ejemplos) | **95 %** |
| 3C_pisadas | 55 % | Pregunta: «¿Qué vio el domingo en la finca?» se entendía como su coartada | «¿Vio algo raro el domingo en el quemadero?» | **100 %** |

Con un segundo portador, la pista se puede conseguir por las dos vías: para 1B_cena, por Amparo en 9 de cada 10
intentos. Ninguna pista que pasara del 70 % se tocó. 2B_imagenes sigue floja por las dos vías (60 % y 45 %).
Bot en las 9 variantes: ver abajo.
