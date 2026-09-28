# Rediseño de historias, pistas, personalidades y mecánicas

Fecha: 2026-09-28 · Estado: aprobado · Modelo objetivo: `qwen2.5:7b-instruct` (Ollama)

## Objetivo

Que las 9 variantes (3 historias × 3 culpables) sean coherentes, jugables y terminables con el final BUENO:
la IA conoce hechos concretos de la historia, las pistas se detectan en la **respuesta** del portador,
el jugador ve nombres que no destripan el caso, el elenco corresponde a la historia activa y las
contradicciones afectan al final.

## Problemas que resuelve

1. Pistas detectadas por palabras de la pregunta del jugador → se detectarán en la respuesta del portador.
2. Historias 2 y 3 sin pistas (final BUENO imposible en 6/9 variantes) → 5-6 pistas por variante.
3. IDs de pista visibles (`1B_madre_esperó`) → nombre neutro + resumen en la libreta.
4. La IA no conoce hechos → cada personaje tiene una ficha con hechos, versión, secreto y límites.
5. Elenco fijo de 7 → 4 personajes por historia; acusación limitada al elenco.
6. Personalidades mal asignadas (Detective/Vecina → Daniel, Madre de la historia 3 → Carmen) → ficha por personaje y variante.
7. Contradicciones decorativas (22:00 vs 23:00) → contradicciones deterministas ligadas a la mentira del culpable, puntúan en el final.
8. `OnSuspectMentioned` sin suscriptor, dropdown inicial de escena, HUD "/7" fijo, reflexión en `GameManager`.
9. Contexto de Ollama por defecto e historial ilimitado → `num_ctx` 8192 e historial acotado.

## Historias

Cada historia tiene 4 personajes (retratos existentes). Trasfondo fijo por personaje + capa por variante
(papel, lo que sabe, versión, secreto, nervios, reacción a la acusación, lo que no sabe). El jugador es un
inspector llegado de fuera.

### Historia 1 · "La Hija Perfecta"
Santiago, viernes de septiembre. Elena Mendoza, 12 años, adoptada a los 3. Llamada al 112 a las 23:15.

| Personaje (retrato) | Trasfondo |
|---|---|
| Daniel Mendoza (padre), 48, abogado | Bufete en apuros, obsesión por las apariencias, controlador |
| Carmen Vidal (madre), 45, pediatra | Insomne, toma zolpidem; volcada en Elena |
| Lucas Mendoza (hermano), 16 | Hijo biológico, celoso, se encierra con los cascos |
| Rosario Gil (vecina), 70 | Viuda insomne, mira con prismáticos. Se desbloquea al ser mencionada |

- **1A · Daniel.** Móvil: vació el fondo de herencia de Elena (180.000 €); ella lo descubrió. 22:00 Carmen se duerme con su pastilla · 22:30 Daniel sube un cacao con zolpidem y cierra con llave · 22:35 Rosario le ve cerrando cortinas · 23:05 "la encuentra" · 23:15 112. Mentira: "No entré en su cuarto; se acostó sola a las diez."
- **1B · Carmen (Münchausen por poderes).** Móvil: Elena decía "no estoy enferma" y pidió otro médico a su tutora. 21:00 Daniel sale · 21:40 triple dosis de la "medicación del corazón" recetada por Carmen · 22:10 Lucas oye "mamá, no quiero más" · 22:00-23:15 luz encendida, Carmen inmóvil junto a la cama · 23:05 llega Daniel · 23:15 112. Mentira: "Estaba perfecta al acostarse; la encontré así a las 23:10."
- **1C · Lucas (accidente encubierto).** Móvil: Elena descubrió que vendía sus pastillas del TDAH. Carmen de guardia hasta 23:00 · 21:45 forcejeo en la escalera, caída · Lucas la acuesta, friega con lejía · 21:52 llama a Daniel · 22:15 llega Daniel, no quiere "dramas" · ~23:00 deja de respirar · 23:15 112. Mentira: "Estuve toda la noche con los cascos, no oí nada."

### Historia 2 · "Noche de Verano"
Portomar, costa gallega, madrugada del martes de fiestas. Sofía Vargas, 19, estudiante de Periodismo. Sale de La Marea a las 5:00; 5:08 "Estoy cerca". Denuncia 8:30; su bolso aparece en la Cala do Corvo.

| Personaje (retrato) | Trasfondo |
|---|---|
| Marcos Rial (dueño del bar), 42 | Rudo y simpático, conoce a todo el pueblo |
| Andrés Souto (cartero), 52 | Metódico, solitario, 15 años de ruta |
| Inspector Ruiz (detective), 55 | Comisario local, cínico, llevó las primeras horas |
| Maruxa Pena (vecina), 74 | Casa en la curva de la carretera, madruga. Se desbloquea al ser mencionada |

- **2A · Marcos.** Acoso todo el verano; 4:30 ella le tira la copa y le llama acosador · 4:55 desenchufa la cámara · 5:05 sale en su Volvo ranchera oscuro · 5:20 coche sin luces en la curva · 6:30 vuelve. Mentira: "Cerré a las cinco y limpié hasta las seis y media."
- **2B · Andrés.** Cartas anónimas sin sello; Sofía lo descubrió · 5:12 la recoge en su Berlingo blanca · ficha a las 6:15 (siempre 5:15) · 7:30 lava la furgoneta. Mentira: "A las cinco y cuarto ya estaba clasificando."
- **2C · Ruiz.** Cobra de narcolanchas; 4:20 Sofía le graba con un sobre en el muelle · 4:40 se lo cuenta a Marcos · 5:10 Ruiz en su Opel gris la recoge · el móvil de Sofía se "extravía" como prueba. Mentira: "Estuve en comisaría hasta las seis."

### Historia 3 · "Humo y Silencio"
Finca Los Olivares, Jaén, octubre. Paula Romero Navarro, 15. Divorcio hace 3 meses, vista de custodia el lunes. Humo negro el sábado. Javier denuncia el domingo 22:30.

| Personaje (retrato) | Trasfondo |
|---|---|
| Javier Romero (padre), 44, olivarero | Amargado, bebe, victimista |
| Lucía Navarro (madre), 41, profesora | Depresión tratada, pánico a perder a Paula |
| Álex Romero (hermano), 17 | Vive con la madre, confidente de Paula. Pista falsa. Se desbloquea al ser mencionado |
| Encarna Molina (vecina), 63 | Finca colindante, pleito por un pozo; perdió a su hija Rocío (15) en 1998. Se desbloquea al ser mencionada |

- **3A · Javier.** Paula iba a contar a la jueza la bebida y las amenazas · 20:30 discusión · 20:40 WhatsApp a Álex "papá está fatal, ha bebido, ven a por mí" · 20:50 golpe fatal · 21:00 hoguera con neumáticos · 21:30-21:50 vuelve con garrafas. Mentira: "Cenó tranquila, se acostó a las diez, el domingo ya no estaba."
- **3B · Lucía (Paula está viva).** Se la lleva a casa de su prima en Portugal y quema su mochila en el quemadero para incriminar a Javier · Javier en el bar 18:30-21:30 · 19:00 Ibiza rojo sube · 19:30 humo · 19:45 se van · vuelve de madrugada con ticket de peaje de Huelva. Mentira: "Todo el sábado en Granada con mi hermana y mi hijo."
- **3C · Encarna.** Obsesión con Paula como sustituta de Rocío; Paula va a despedirse (se muda a Madrid); empujón junto al pozo; quema sus cosas en el quemadero de Javier entrando con su llave de la cancela · Javier en el bar 19:30-22:00, volvió borracho sin mirar a Paula (su secreto). Mentira: "El sábado no vi a Paula."

## Pistas

### Modelo

| Campo | Uso |
|---|---|
| `id` | Interno, nunca se muestra |
| `playerName` | Nombre neutro visible |
| `summary` | Texto de la libreta al descubrirla (también el texto mostrado al confrontar) |
| `holder` | Único personaje que la conoce |
| `topic` + `fact` | En el prompt del portador: "Si te preguntan por {topic}: {fact}" |
| `anchors` | Grupos de anclas (AND entre grupos, OR dentro); se buscan en la respuesta normalizada |
| `kind` | `Incriminates`, `Clears` (con `clears` = personaje), `Context` |
| `exposesLie` | Si es la pista ⚡ que choca con la mentira del culpable |
| `calibrationQuestions` | Preguntas típicas para el script de calibración |

Normalización: minúsculas, sin tildes, `h`/`.` en horas → `:` (22.35, 22h35 → 22:35), espacios colapsados.
Solo cuenta la respuesta del portador. Una pista se descubre una vez por partida.

**Regla:** el culpable nunca es portador de pistas necesarias para alcanzar el final BUENO.

### Pistas por variante (I = incrimina, D = descarta, ⚡ = expone la mentira)

**1A** — La taza de la mesilla (Carmen, I) · La puerta de Elena (Lucas, I) · Lo que vio la ventana (Rosario, I⚡) · Papeles del despacho (Lucas, I) · La partida de Lucas (Lucas, D:Hermano) · El frasco medio vacío (Carmen, D:Madre)

**1B** — Un historial abultado (Daniel, I) · La receta de casa (Daniel, I) · A través de la pared (Lucas, I⚡) · La luz encendida (Rosario, I) · La cena del viernes (Daniel, D:Padre)

**1C** — Gritos en la escalera (Rosario, I⚡) · Olor a lejía (Carmen, I) · Una llamada corta (Daniel, I) · La pantalla rota (Carmen, I) · El fichaje del hospital (Carmen, D:Madre)

**2A** — Una copa por la cara (Ruiz, I) · La cámara del bar (Ruiz, I) · Un aparcamiento vacío (Andrés, I⚡) · Coche en la curva (Maruxa, I) · El GPS de Correos (Ruiz, D:Cartero)

**2B** — Cartas sin sello (Marcos, I) · Un fichaje tardío (Ruiz, I⚡) · La furgoneta blanca (Maruxa, I) · Manguera al amanecer (Marcos, I) · Imágenes del bar (Ruiz, D:Dueño)

**2C** — Algo gordo en el puerto (Marcos, I) · Un coche conocido (Maruxa, I⚡) · Comisaría cerrada (Andrés, I⚡) · Una prueba perdida (Ruiz, I, prescindible) · El GPS de Correos (Andrés, D:Cartero) · Grabación completa (Marcos, D:Dueño)

**3A** — El último mensaje (Álex, I⚡) · Humo negro (Encarna, I) · Viaje nocturno (Encarna, I) · Audios de voz (Lucía, I) · Sábado en Granada (Álex, D:Madre)

**3B** — Un coche rojo (Encarna, I⚡) · La carta de la mesilla (Javier, I) · Una noche larga (Álex, I⚡) · El armario medio vacío (Álex, I) · La cuenta del bar (Javier, D:Padre)

**3C** — La despedida (Álex, I⚡) · La llave de la cancela (Javier, I) · Dos niñas en la pared (Lucía, I) · Pisadas en la ceniza (Javier, I) · La noche del bar (Javier, D:Padre)

Los hechos, anclas y preguntas de calibración concretos viven en los ficheros de datos de cada historia.

### Desbloqueos
- Cada personaje tiene `startsUnlocked` y `mentionAliases` (nombre, apellido, rol).
- Se desbloquea cuando una **respuesta** de cualquier personaje menciona un alias. Todas las fichas de la
  familia/entorno incluyen una frase que menciona de forma natural a los bloqueados.
- Red de seguridad: al empezar el día 3 se desbloquea todo lo pendiente con un aviso narrativo.

## Personalidades

Ficha de 250-350 palabras, secciones fijas y en este orden:

```
Eres {nombre}, {edad}, {rol}. {trasfondo}
CÓMO HABLAS: {estilo} Ejemplo: "{frase}"
EL CASO: {3 frases}
LO QUE SABES Y CUENTAS SI TE PREGUNTAN:
- Si te preguntan por {topic}: {fact}
TU VERSIÓN: {versión}
LO QUE OCULTAS: {secreto} Lo admites solo si {condición}.
TE PONE NERVIOSO: {temas}
SI TE ACUSAN: {reacción}
NO SABES: {ignorancia}. Si te preguntan por eso, di que no lo sabes.
[CULPABLE] SI TE MUESTRAN {prueba}: {versión B, verdad parcial, sin confesar}
PRUEBAS QUE YA TE HAN MOSTRADO: ...
HOY ES EL DÍA {n} DE LA INVESTIGACIÓN.
REGLAS: español, primera persona, 2-4 frases, sin asteriscos ni listas, nunca digas que eres una IA,
no inventes horas ni nombres que no estén en esta ficha.
```

- Culpable: mantiene la versión; ante la prueba ⚡ pasa a la versión B; nunca confiesa el crimen.
- Inocentes: secreto menor que les hace parecer culpables; lo niegan una vez y lo admiten si insisten.
  Admitirlo coincide con su pista de descarte.

## Mecánicas

- **Elenco por historia**: el dropdown muestra "Daniel (padre)", se rellena desde datos al empezar; la acusación solo ofrece el elenco de la historia.
- **Contradicciones deterministas**: por variante, `lieAnchors` del culpable + pistas ⚡. Se registra una contradicción
  por pista ⚡ descubierta cuando además (a) el culpable dijo su mentira (anclas en su respuesta) o (b) fue confrontado con esa pista.
  Texto: *"La versión de Daniel («…») choca con: Lo que vio la ventana."*
- **Confrontar con prueba**: selector "Mostrar prueba" (creado por código si la escena no lo tiene). Cuesta 1 pregunta; añade
  `[El inspector te muestra: {summary}]` al mensaje y lo registra en "pruebas ya mostradas".
- **Final**: evidencia = pistas I descubiertas + 2 × contradicciones del culpable.
  Acierto y ≥5 → GOOD; 3-4 → BITTERSWEET; ≤2 → INSUFFICIENT; fallo → BAD (texto "ignoraste pruebas" si tenías la pista D del acusado).
  Pantalla final con epílogo por variante.
- **Parte de la mañana**: línea narrativa por día y variante (días 2-7), sin destripar. Aviso en días 6 y 7.
- **Robustez LLM**: `num_ctx` 8192; últimos 8 intercambios por sospechoso; temperatura 0.6; `maxTokens` 250.

## Añadidos aprobados

1. **Modo debug (solo editor)**: en el Inspector de `GameManager`, desplegable para forzar historia y variante
   (`Aleatoria` por defecto; ignorado fuera del editor). Log en consola por cada evaluación de pista: pista, portador,
   grupos de anclas, anclas encontradas por grupo y resultado.
2. **Implementación por etapas**: etapa 1 = arquitectura + historia 1 completa (1A/1B/1C) + calibración de la historia 1.
   Tras la prueba del usuario en el editor, etapa 2 = historias 2 y 3.
3. **Calibración**: herramienta de editor (menú `Detective/Calibrar pistas` y ejecutable en batchmode con `-executeMethod`)
   que, para cada pista de las variantes elegidas, construye el prompt real del portador, lanza 3 veces sus preguntas típicas a
   Ollama con historial vacío, pasa la respuesta por el mismo detector y mide la tasa. Pistas con < 2/3 se marcan y el informe
   incluye las respuestas fallidas para ampliar anclas. Informe en `Logs/clue-calibration.md`.

## Arquitectura

```
Assets/Scripts/Detective.asmdef              (nuevo; Unity.TextMeshPro, UnityEngine.UI)
Assets/Scripts/Cases/CaseModels.cs           StoryData, CharacterData, VariantData, CharacterRole, ClueData, ...
Assets/Scripts/Cases/CaseLibrary.cs          registro de historias, búsqueda por id de variante
Assets/Scripts/Cases/Story1HijaPerfecta.cs   datos de la historia 1
Assets/Scripts/Cases/PromptBuilder.cs        ficha → system prompt (puro)
Assets/Scripts/Cases/ClueDetector.cs         normalización + evaluación de anclas con traza (puro)
Assets/Scripts/Cases/InvestigationState.cs   pistas, mostradas, mentiras, contradicciones, cálculo del final (puro)
Assets/Scripts/AIConversationManager.cs      orquesta LLM + detector + estado
Assets/Scripts/GameManager.cs                días, elenco, desbloqueos, parte de la mañana, acusación
Assets/Scripts/InterrogationUI.cs            libreta, HUD, selector de prueba, epílogo
Assets/Scripts/Editor/Detective.Editor.asmdef
Assets/Scripts/Editor/ClueCalibrator.cs
Assets/Tests/EditMode/Detective.Tests.asmdef
Assets/Tests/EditMode/*Tests.cs              detector, prompt, estado, validación de datos
```

## Pruebas

- Tests EditMode: normalización y anclas; validación de datos de cada variante registrada (portadores en el elenco,
  culpable no portador de pistas necesarias, alcanzable evidencia ≥5 sin el culpable, 4-6 pistas, al menos 1 ⚡ y 1 D,
  cada pista con preguntas de calibración, respuestas de ejemplo positivas detectadas y negativas no);
  cálculo del final; contradicciones; desbloqueos.
- Calibración contra qwen de la historia 1 con ≥ 2/3 en todas sus pistas antes de dar la etapa 1 por terminada.
- Prueba manual del usuario en el editor.

## Fuera de alcance

- "Presentar el caso" (elegir pruebas al acusar).
- Historias 2 y 3 hasta que se valide la etapa 1 (mientras tanto el sorteo solo incluye variantes registradas).
