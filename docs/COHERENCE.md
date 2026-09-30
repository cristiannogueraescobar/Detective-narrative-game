# Coherencia de las historias (checklist del bloque A)

Revisión a mano de las 3 historias × 3 variantes con la checklist pedida, **antes** de escribir más tests. Debajo
está la matriz generada desde los datos reales (`Detective → Matriz de coherencia`, `CoherenceReport.cs`), que no
se desfasa: si una ficha cambia, se regenera. Lo automatizable de la checklist está además en tests
(`NarrativeValidator`, `CaseDataValidationTests`, `ResolvabilityTests`, `LieDetectionTests`).

## Checklist

| Punto | 1A | 1B | 1C | 2A | 2B | 2C | 3A | 3B | 3C | Cómo se comprueba |
|---|---|---|---|---|---|---|---|---|---|---|
| Culpable definido, móvil y mentira claros | ✓ | ✓ | ✓ | ✓ | ✓¹ | ✓ | ✓ | ✓ | ✓ | a mano (matriz) |
| Pista ⚡ existe y **no** la tiene el culpable | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | test (validador) |
| La mentira es lo contrario de la ⚡ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓² | ✓ | a mano + test (la versión dispara la mentira) |
| Cada pista tiene portador del reparto | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | test |
| El portador puede saberlo (estaba allí, a esa hora) | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | a mano + línea temporal |
| Incrimina / descarta / contexto, documentado | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | datos (`kind`, `clears`) + test |
| Reparto de 4–5 con nombre y papel | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | test |
| Desbloqueo: mención o, como tarde, día 3 | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | test + matriz |
| Nadie en dos sitios; horas de las pistas en la línea temporal | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | test (validador) |
| Partes sin pistas gratis ni spoiler; epílogo sin horas nuevas | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | test (validador) |

¹ 2B: el móvil (cartas anónimas) no se unía al cartero; ahora las cartas aparecen "antes de amanecer", la hora de
su reparto (docs/STORY-AUDIT.md).
² 3B: la ⚡ 3B_coche (un coche rojo pequeño) solo contradice "estuve en Granada" si se sabe que Lucía conduce un
Ibiza rojo: ahora lo sabe Javier. La otra ⚡ (3B_noche, de Álex) la contradice directamente.

Todo lo que no cuadraba se arregló **antes** de escribir los tests nuevos (lista completa en docs/STORY-AUDIT.md).

## Limitaciones de qwen 7B (las que quedan sin solución completa)

| Limitación | Medida | Mitigación | Estado |
|---|---|---|---|
| Se inventa horas | 3–6 % de las respuestas | Regla en la ficha + reintento si la hora no está en la ficha (0d) | Mitigada; A/B en docs/NIGHT-LOG.md |
| Acepta premisas falsas de preguntas capciosas | 23 % (sonda de premisas) | Día 3: "si el inspector da por hecho algo que no está en tu ficha, di que no te consta; lo que sí está, confírmalo" → **2 %**, sin perder pistas (42/48, 83 %) ni estados (95 %); 3A_audios aguanta (5/6) | **Cerrada** (medida) |
| Inventa hechos vistosos (un testigo "ve" a la víctima ya muerta) | Día 3, medido: 11 nombres inventados en ~3000 respuestas del bot (0,4 %), casi todos inofensivos (grupos de música, marcas); el único dañino, un culpable que se dio a otro sospechoso como coartada (1 partida de 2A) | Regla "no inventes hechos"; temas de testigo más concretos; regla contra premisas falsas | Abierta (rara; un reintento como el de las horas no compensa el riesgo) |
| Testigos que se cierran ("no vi nada") ante preguntas naturales | varios en el bot | Temas más amplios (1A_ventana, Maruxa, 3C) | Mitigada |
| Repite palabra por palabra | raro | Reintento automático | Resuelta |
| Deriva al gallego (Maruxa) | ~1 de 60 | Ejemplo de habla en español | Resuelta |
| Estado emocional casi siempre "nervioso" | 70 % → 42 % | Guía de estados | Resuelta |

<!-- MATRIZ GENERADA (Detective/Matriz de coherencia): no editar debajo -->

## Historia 1: La Hija Perfecta

Víctima: Elena Mendoza, 12 años, adoptada a los tres. Murió en su cama; a las 23:15 llamaron al 112 desde casa.

| Personaje | Nombre | Papel | Aparece |
|---|---|---|---|
| `padre` | Daniel Mendoza | padre | desde el día 1 |
| `madre` | Carmen Vidal | madre | desde el día 1 |
| `hermano` | Lucas Mendoza | hermano | desde el día 1 |
| `vecina` | Rosario Gil | vecina | cuando lo mencionan (alias: rosario, vecina, la de enfrente), cuando el jugador pregunta por «vecin», «enfrente», «ventana», «cortina», «testig», «desde fuera», «alguien vio», «rosario» o, si no, el día 2 (parte: «Una vecina de enfrente se presenta en comisaría: dice que esa noche no durmió y quiere hablar.») |

### 1A · culpable: Daniel Mendoza (padre)

- **Qué hizo y por qué (secreto):** Llevas un año sacando dinero de la herencia de Elena para tapar las deudas del bufete. Elena encontró los extractos en tu despacho y amenazó con contárselo a Carmen. A las 22:30 le subiste un cacao con el zolpidem de Carmen triturado y cerraste las cortinas y la puerta con llave.
- **Su mentira:** «no entré en su cuarto» · versión: Esa noche no entré en el cuarto de Elena hasta las 23:05: se acostó sola a las diez. A las 23:05 fui a verla, no respiraba, y a las 23:15 llamé al 112.
- **Si le muestran la prueba (versión B):** Admites que a las 22:30 entraste un momento a darle las buenas noches y a cerrar las cortinas, pero insistes en que estaba bien y en que no le diste nada.
- **⚡ 1A_ventana** (Rosario (vecina)): a las 22:35 viste al padre, Daniel, en el cuarto de la niña cerrando las cortinas. Te extrañó porque él casi nunca entra en ese cuarto.
- **Validador narrativo:** sin problemas

| Pista | Portador | Tipo | Secreta | Hecho |
|---|---|---|---|---|
| 1A_taza | Carmen (madre) | incrimina |  | al entrar en el cuarto viste en la mesilla una taza de cacao a medio beber. Elena nunca tomaba cacao por la noche y en casa el cacao solo lo prepara Daniel. |
| 1A_puerta | Lucas (hermano) | incrimina |  | papá aporreaba la puerta de Elena; estaba cerrada con llave por fuera y sacó la llave de su bolsillo. Elena nunca cerraba con llave. |
| ⚡ 1A_ventana | Rosario (vecina) | incrimina |  | a las 22:35 viste al padre, Daniel, en el cuarto de la niña cerrando las cortinas. Te extrañó porque él casi nunca entra en ese cuarto. |
| 1A_papeles | Lucas (hermano) | incrimina |  | hace una semana Elena te enseñó fotos de unos extractos del banco del despacho de papá y te dijo que papá le había robado su herencia. |
| 1A_partida | Lucas (hermano) | descarta a `hermano` |  | de 21:30 a 00:00 estuviste en una partida online con tus amigos; el juego guarda el registro. |
| 1A_frasco | Carmen (madre) | descarta a `madre` | sí | Tu frasco de zolpidem, que abriste hace una semana, estaba casi vacío por la mañana: faltaban muchas pastillas que tú no tomaste. |

### 1B · culpable: Carmen Vidal (madre)

- **Qué hizo y por qué (secreto):** Llevas dos años inventando enfermedades a Elena y dándole medicación que no necesita. Ella empezaba a decir 'no estoy enferma'. A las 21:40 le diste el triple de su medicación del corazón y te quedaste junto a su cama sin llamar a nadie hasta las 23:15.
- **Su mentira:** «estaba perfecta; la encontré a las 23:10» · versión: Elena estaba perfectamente al acostarse, a las 21:30. Yo estuve en el salón leyendo. A las 23:10 subí a verla y la encontré así; a las 23:15 llamé al 112.
- **Si le muestran la prueba (versión B):** Admites que subiste antes, sobre las 22:00, porque Elena se encontraba mal, y que te quedaste con ella pensando que se le pasaría. Dices que fue un error de juicio, nada más.
- **⚡ 1B_pared** (Lucas (hermano)): sobre las 22:10 oíste a Elena llorar a través de la pared diciendo 'mamá, no quiero más'. Te habías quitado los cascos un momento.
- **⚡ 1B_luz** (Rosario (vecina)): viste a la madre, Carmen, sentada junto a la cama de la niña, muy quieta, sin llamar a nadie, desde las 22:00 hasta las 23:15.
- **Validador narrativo:** sin problemas

| Pista | Portador | Tipo | Secreta | Hecho |
|---|---|---|---|---|
| 1B_historial | Daniel (padre) | incrimina |  | en dos años Elena fue once veces a urgencias, siempre llevada por Carmen, y ningún especialista le encontró nada. |
| 1B_receta | Daniel (padre) | incrimina |  | Carmen le recetó ella misma a Elena un medicamento para el corazón, y se puso furiosa cuando pediste una segunda opinión a un cardiólogo. |
| ⚡ 1B_pared | Lucas (hermano) | incrimina |  | sobre las 22:10 oíste a Elena llorar a través de la pared diciendo 'mamá, no quiero más'. Te habías quitado los cascos un momento. |
| ⚡ 1B_luz | Rosario (vecina) | incrimina |  | viste a la madre, Carmen, sentada junto a la cama de la niña, muy quieta, sin llamar a nadie, desde las 22:00 hasta las 23:15. |
| 1B_tutora | Lucas (hermano) | incrimina |  | hace unos días Elena te contó que le había pedido a su tutora del colegio que la llevaran a otro médico, porque ella decía que no estaba enferma y que las pastillas la mareaban. |
| 1B_cena | Daniel (padre) | descarta a `padre` | sí | Estuviste de 21:00 a 23:00 en casa de Marta, una compañera del bufete; ella y el portero pueden confirmarlo. |

### 1C · culpable: Lucas Mendoza (hermano)

- **Qué hizo y por qué (secreto):** Vendes en el instituto tus pastillas para el TDAH. Elena lo descubrió y te amenazó con contarlo. A las 21:45 discutisteis en la escalera, le quitaste el móvil a tirones, ella se cayó y se golpeó la cabeza. Parecía estar bien: la acostaste, fregaste el escalón con lejía y a las 21:52 llamaste a papá.
- **Su mentira:** «estuve con los cascos, no oí nada» · versión: Estuve toda la noche en mi cuarto con los cascos puestos, jugando. No oí nada hasta que papá empezó a gritar.
- **Si le muestran la prueba (versión B):** Admites que discutiste con Elena en la escalera y que ella se cayó, pero insistes en que fue un accidente y en que estaba bien y hablaba cuando la acostaste.
- **⚡ 1C_gritos** (Rosario (vecina)): sobre las 21:45 oíste gritos de los dos chicos, Lucas y la niña, y luego un golpe seco. Justo después viste a Lucas parado en la ventana de la escalera, con las manos en la cabeza.
- **Validador narrativo:** sin problemas

| Pista | Portador | Tipo | Secreta | Hecho |
|---|---|---|---|---|
| ⚡ 1C_gritos | Rosario (vecina) | incrimina |  | sobre las 21:45 oíste gritos de los dos chicos, Lucas y la niña, y luego un golpe seco. Justo después viste a Lucas parado en la ventana de la escalera, con las manos en la cabeza. |
| 1C_lejia | Carmen (madre) | incrimina |  | al llegar a las 23:20 olía muchísimo a lejía en la escalera y la alfombra del tercer escalón estaba mojada, como recién fregada. En casa nadie friega a esas horas. |
| 1C_pantalla | Carmen (madre) | incrimina |  | esa misma noche encontraste el móvil de Elena en el cuarto de Lucas, debajo de la cama, con la pantalla rota. Elena nunca soltaba su móvil. |
| 1C_llamada | Daniel (padre) | incrimina | sí | Lucas te llamó a las 21:52 llorando: 'Elena se ha caído por la escalera, pero está bien'. Por eso llegaste a casa a las 22:15, no a las 23:00. |
| 1C_fichaje | Carmen (madre) | descarta a `madre` |  | estuviste de guardia en el hospital desde las 15:00 hasta las 23:00; tu fichaje de salida lo registra y tus compañeros de urgencias pueden confirmarlo. |

## Historia 2: Noche de Verano

Víctima: Sofía Vargas, 19 años, estudiante de Periodismo. Salió sola del bar La Marea a las 5:00 y a las 5:08 escribió «Estoy cerca».

| Personaje | Nombre | Papel | Aparece |
|---|---|---|---|
| `bar` | Marcos Rial | dueño del bar | desde el día 1 |
| `cartero` | Andrés Souto | cartero | desde el día 1 |
| `detective` | Inspector Ruiz | inspector | desde el día 1 |
| `vecina` | Maruxa Pena | vecina | cuando lo mencionan (alias: maruxa, la de la curva, casa de la curva, vecina), cuando el jugador pregunta por «curva», «vecin», «testig», «furgoneta», «quien vio», «maruxa» o, si no, el día 2 (parte: «Una patrulla recorre la carretera de la costa: la vecina de la casa de la curva pide hablar con el inspector.») |

### 2A · culpable: Marcos Rial (dueño del bar)

- **Qué hizo y por qué (secreto):** Llevas todo el verano detrás de Sofía, aunque ella te rechazó. A las 4:30 te humilló delante de todos. A las 4:55 desenchufaste la cámara del bar, a las 5:05 saliste en tu Volvo ranchera y la alcanzaste en la curva.
- **Su mentira:** «no salí del bar» · versión: Cerré a las cinco y estuve limpiando dentro hasta las seis y media. No salí del bar.
- **Si le muestran la prueba (versión B):** Admites que saliste un rato con el coche a tomar el aire, pero juras que no viste a Sofía.
- **⚡ 2A_aparcamiento** (Andrés (cartero)): a las 5:10, al pasar por La Marea, el bar estaba a oscuras y el Volvo ranchera de Marcos no estaba aparcado en su sitio.
- **Validador narrativo:** sin problemas

| Pista | Portador | Tipo | Secreta | Hecho |
|---|---|---|---|---|
| 2A_copa | Ruiz (inspector) | incrimina |  | a las 4:30 Sofía le tiró una copa a Marcos y le llamó acosador delante de todo el bar; varios testigos te lo contaron. |
| 2A_camara | Ruiz (inspector) | incrimina |  | la cámara de La Marea dejó de grabar a las 4:55 porque alguien la desenchufó a mano, justo antes de que Sofía saliera. |
| ⚡ 2A_aparcamiento | Andrés (cartero) | incrimina |  | a las 5:10, al pasar por La Marea, el bar estaba a oscuras y el Volvo ranchera de Marcos no estaba aparcado en su sitio. |
| 2A_curva | Maruxa (vecina) | incrimina |  | a las 5:20 viste un coche grande y oscuro, tipo ranchera, parado en la curva con las luces apagadas. Ahí nunca para nadie. |
| 2A_gps | Ruiz (inspector) | descarta a `cartero` |  | el GPS de la furgoneta de Correos sitúa a Andrés en la oficina de clasificación desde las 5:15 hasta las 7:00. |

### 2B · culpable: Andrés Souto (cartero)

- **Qué hizo y por qué (secreto):** Llevas todo el verano dejando cartas anónimas sin sello en el buzón de la tía de Sofía. Ella lo descubrió y amenazó con denunciarte. A las 5:12 la recogiste en la curva con tu furgoneta particular. Fichaste tarde y por la mañana la lavaste.
- **Su mentira:** «a las cinco y cuarto ya estaba clasificando» · versión: A las cinco y cuarto ya estaba en la oficina clasificando, como cada día. No vi a Sofía.
- **Si le muestran la prueba (versión B):** Admites que llegaste tarde a la oficina porque te dormiste, pero niegas haber visto a Sofía.
- **⚡ 2B_fichaje** (Ruiz (inspector)): Andrés fichó en la oficina de Correos a las 6:15 esa madrugada, cuando lleva quince años fichando a las 5:15 sin fallar un día.
- **Validador narrativo:** sin problemas

| Pista | Portador | Tipo | Secreta | Hecho |
|---|---|---|---|---|
| 2B_cartas | Marcos (dueño del bar) | incrimina |  | hace una semana Sofía te enseñó unas cartas anónimas que le dejaban en el buzón de su tía antes de amanecer, sin sello ni matasellos; estaba asustada. |
| ⚡ 2B_fichaje | Ruiz (inspector) | incrimina |  | Andrés fichó en la oficina de Correos a las 6:15 esa madrugada, cuando lleva quince años fichando a las 5:15 sin fallar un día. |
| 2B_furgoneta | Maruxa (vecina) | incrimina |  | «A las 5:12 paró una furgoneta blanca pequeña junto a la rapaza, en la curva, y ella se subió.» |
| 2B_manguera | Marcos (dueño del bar) | incrimina |  | a las 7:30, al volver a casa, viste a Andrés lavando a manguerazos su furgoneta blanca detrás de su casa. En quince años nunca le viste lavarla. |
| 2B_imagenes | Ruiz (inspector) | descarta a `bar` |  | la cámara de La Marea grabó a Marcos dentro del bar, recogiendo, desde las 5:00 hasta las 6:30. |

### 2C · culpable: Inspector Ruiz (inspector)

- **Qué hizo y por qué (secreto):** Cobras de una red de narcolanchas por avisar de las patrullas. A las 4:20 Sofía te grabó cogiendo un sobre en el muelle. A las 5:10 la recogiste en la curva con tu Opel gris. Después hiciste desaparecer su móvil de las pruebas.
- **Su mentira:** «estuve en comisaría hasta las seis» · versión: Esa noche estuve en comisaría hasta las seis. No vi a la chica en ningún momento.
- **Si le muestran la prueba (versión B):** Admites que saliste a hacer una ronda en el coche hacia las cinco, pero niegas haber visto a Sofía.
- **⚡ 2C_opel** (Maruxa (vecina)): a las 5:10 viste el Opel gris del inspector Ruiz, lo conoces de sobra, parar junto a la chica en la curva; ella se subió.
- **⚡ 2C_comisaria** (Andrés (cartero)): a las 5:05 fuiste a la comisaría a dejar el certificado urgente y estaba cerrada, con las luces apagadas y sin nadie dentro.
- **Validador narrativo:** sin problemas

| Pista | Portador | Tipo | Secreta | Hecho |
|---|---|---|---|---|
| 2C_puerto | Marcos (dueño del bar) | incrimina |  | a las 4:40 Sofía volvió pálida al bar y te dijo que había grabado con el móvil algo gordo en el puerto, y que no se fiaba de la policía. |
| ⚡ 2C_opel | Maruxa (vecina) | incrimina |  | a las 5:10 viste el Opel gris del inspector Ruiz, lo conoces de sobra, parar junto a la chica en la curva; ella se subió. |
| ⚡ 2C_comisaria | Andrés (cartero) | incrimina |  | a las 5:05 fuiste a la comisaría a dejar el certificado urgente y estaba cerrada, con las luces apagadas y sin nadie dentro. |
| 2C_prueba | Ruiz (inspector) | incrimina |  | el móvil de la chica apareció en la cala, se registró como prueba y se extravió en el traslado a Vigo. Son cosas que pasan. |
| 2C_gps | Andrés (cartero) | descarta a `cartero` |  | «Nadie me vio, pero mírelo en el GPS de la furgoneta de Correos: a las 5:15 me sitúa en la nacional, camino de la oficina. Compruébelo.» |
| 2C_grabacion | Marcos (dueño del bar) | descarta a `bar` |  | la cámara de tu bar grabó toda la noche y tú mismo le entregaste la grabación a la Guardia Civil: se te ve dentro recogiendo hasta las 6:30. |

## Historia 3: Humo y Silencio

Víctima: Paula Romero Navarro, 15 años. Pasaba el fin de semana con su padre en la finca.

| Personaje | Nombre | Papel | Aparece |
|---|---|---|---|
| `padre` | Javier Romero | padre | desde el día 1 |
| `madre` | Lucía Navarro | madre | desde el día 1 |
| `hermano` | Álex Romero | hermano | cuando lo mencionan (alias: alex, hermano), cuando el jugador pregunta por «hermano», «alex», «mensaje», «whatsapp», «movil de paula», «con quien hablaba» o, si no, el día 2 (parte: «El hermano de Paula llega al cuartel desde Granada con el móvil en la mano: quiere enseñarle algo.») |
| `vecina` | Encarna Molina | vecina | cuando lo mencionan (alias: encarna, vecina, finca de al lado), cuando el jugador pregunta por «vecin», «finca de al lado», «caballo», «cancela», «pozo», «quien vio», «encarna» o, si no, el día 3 (parte: «La Guardia Civil toma declaración en las fincas vecinas: la dueña de la de al lado vio humo el sábado.») |

### 3A · culpable: Javier Romero (padre)

- **Qué hizo y por qué (secreto):** Paula iba a contarle a la jueza que bebes y la amenazas. A las 20:30 discutisteis y a las 20:50 la golpeaste; no volvió a levantarse. Luego quemaste sus cosas y saliste a por gasoil.
- **Su mentira:** «cenó tranquila y se acostó a las diez» · versión: Paula cenó conmigo tan tranquila y se acostó a las diez. El domingo por la mañana ya no estaba.
- **Si le muestran la prueba (versión B):** Admites que discutisteis a las 20:30 porque ella quería vivir con su madre, pero juras que luego se calmó y se encerró en su cuarto.
- **⚡ 3A_mensaje** (Álex (hermano)): a las 20:40 Paula te escribió por WhatsApp: 'Papá está fatal, ha bebido, ven a por mí'. Fue su último mensaje.
- **Validador narrativo:** sin problemas

| Pista | Portador | Tipo | Secreta | Hecho |
|---|---|---|---|---|
| ⚡ 3A_mensaje | Álex (hermano) | incrimina |  | a las 20:40 Paula te escribió por WhatsApp: 'Papá está fatal, ha bebido, ven a por mí'. Fue su último mensaje. |
| 3A_humo | Encarna (vecina) | incrimina |  | sobre las 21:00 del sábado salió un humo negro y espeso del quemadero de Javier; olía a goma quemada y a algo dulzón. |
| 3A_garrafas | Encarna (vecina) | incrimina |  | a las 21:30 la camioneta de Javier salió de la finca y volvió a las 21:50 cargada de garrafas. |
| 3A_audios | Lucía (madre) | incrimina |  | «Javier me mandó audios la semana pasada: 'Si me quitas a la niña, no la vuelves a ver'. Y el lunes era la vista.» |
| 3A_granada | Álex (hermano) | descarta a `madre` |  | tu madre y tú pasasteis todo el sábado en casa de tu tía en Granada, a cien kilómetros; tu tía y tus primos lo pueden decir. |

### 3B · culpable: Lucía Navarro (madre)

- **Qué hizo y por qué (secreto):** Paula está viva. El sábado a las 19:00 subiste a la finca mientras Javier estaba en el bar, te llevaste a Paula a casa de tu prima en Portugal y quemaste su mochila en el quemadero para que culparan a Javier. Volviste de madrugada.
- **Su mentira:** «estuve todo el sábado en Granada» · versión: El sábado estuve todo el día en Granada, en casa de mi hermana, con Álex.
- **Si le muestran la prueba (versión B):** Admites que dejaste a Álex en casa de tu hermana a mediodía y saliste a dar una vuelta con el coche, pero niegas haber ido a la finca.
- **⚡ 3B_coche** (Encarna (vecina)): a las 19:00 del sábado viste un coche pequeño rojo subir por el camino de la finca de Javier; la camioneta de Javier no estaba, él estaba en el pueblo.
- **⚡ 3B_noche** (Álex (hermano)): «Mamá me dejó en casa de la tía a mediodía y no volvió hasta las cuatro de la madrugada; en su coche vi un ticket de peaje de Huelva.»
- **Validador narrativo:** sin problemas

| Pista | Portador | Tipo | Secreta | Hecho |
|---|---|---|---|---|
| ⚡ 3B_coche | Encarna (vecina) | incrimina |  | a las 19:00 del sábado viste un coche pequeño rojo subir por el camino de la finca de Javier; la camioneta de Javier no estaba, él estaba en el pueblo. |
| 3B_carta | Javier (padre) | incrimina |  | el domingo encontraste en la mesilla de Paula una carta de Lucía: 'Pronto estaremos lejos de él, tú hazme caso'. |
| ⚡ 3B_noche | Álex (hermano) | incrimina | sí | «Mamá me dejó en casa de la tía a mediodía y no volvió hasta las cuatro de la madrugada; en su coche vi un ticket de peaje de Huelva.» |
| 3B_armario | Álex (hermano) | incrimina |  | en casa de tu madre faltan la maleta grande y el pasaporte de Paula; te diste cuenta el domingo. |
| 3B_cuenta | Javier (padre) | descarta a `padre` |  | de 18:30 a 21:30 estuviste en el bar Casino del pueblo; el camarero te cobró a las 21:25 y te vio todo el rato. |

### 3C · culpable: Encarna Molina (vecina)

- **Qué hizo y por qué (secreto):** Paula era como tu Rocío. El sábado a las 20:00 vino a despedirse porque se iba a Madrid; no la dejaste irse, discutisteis junto al pozo y la empujaste. Después quemaste sus cosas en el quemadero de Javier para que le culparan a él.
- **Su mentira:** «el sábado no vi a Paula» · versión: El sábado no vi a Paula. Estuve en casa toda la tarde viendo la tele.
- **Si le muestran la prueba (versión B):** Admites que Paula vino un momento a las 20:00 a ver los caballos, pero juras que se fue enseguida hacia la casa de su padre.
- **⚡ 3C_despedida** (Álex (hermano)): a las 19:50 Paula te escribió: 'Voy a despedirme de Encarna y de los caballos, luego te llamo'. Nunca te llamó.
- **Validador narrativo:** sin problemas

| Pista | Portador | Tipo | Secreta | Hecho |
|---|---|---|---|---|
| ⚡ 3C_despedida | Álex (hermano) | incrimina |  | a las 19:50 Paula te escribió: 'Voy a despedirme de Encarna y de los caballos, luego te llamo'. Nunca te llamó. |
| 3C_llave | Javier (padre) | incrimina |  | Encarna es la única persona con llave de la cancela que une su finca con la tuya. |
| 3C_fotos | Lucía (madre) | incrimina |  | «Encarna está obsesionada con Paula: tiene fotos suyas junto a las de su hija muerta, Rocío, y le regalaba ropa de Rocío. Me da miedo.» |
| 3C_pisadas | Javier (padre) | incrimina |  | el domingo encontraste en la ceniza del quemadero huellas de botas pequeñas, de mujer; tú calzas un 44. |
| 3C_bar | Javier (padre) | descarta a `padre` | sí | Estuviste en el bar Casino de 19:30 a 22:00, el camarero lo sabe; volviste borracho y no entraste a ver a Paula. |

## Horas inventadas: reintento más frío (día 3, ronda 20)
A/B con el bot (3B, 1C, 3A; 12 + 12 partidas): con el reintento a temperatura 0,3 se arreglan 10 de 11 respuestas
con horas inventadas (antes 5 de 9). Activado por defecto (`AIConversationManager.CoolTimeRetry`).

## Nombres inventados por un hueco de la ficha (día 3, ronda 27)
"Tu tía" en Granada, sin nombre, hacía que qwen la llamara "tía María" y lo repitiera (5 marcas en una partida).
Con nombre en las fichas (Remedios), 0 en la comprobación. Regla: si una ficha nombra a alguien por su parentesco
y los personajes pueden hablar de esa persona, darle nombre.
