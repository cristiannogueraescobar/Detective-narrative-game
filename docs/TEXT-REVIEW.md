# Revisión de textos de la interfaz (sesión C, 01-10-2026)

Skill **design:ux-copy**. Se revisaron los 462 textos de la interfaz que ve el jugador (extraídos del código; las escenas
los reescribe el código al arrancar). Se respetó el glosario de `docs/DESIGN-CRITIQUE.md` (pista, prueba, contradicción,
parte, caso —nunca «partida»—, Fin del día / Terminar el día, sospecha / descarte, culpables posibles; números pequeños
en letra). **No se tocaron las fichas de los personajes** ni nada que entre en un prompt (exigiría recalibrar).
Rama `feature/revision-textos`. Tests: EditMode 765/766 (con `LayoutValidationTests`), PlayMode 64, 0 fallos;
`TextReviewTests` nuevo y cinco tests existentes que fijaban el texto antiguo, actualizados.

## Cambios aplicados

| Dónde | Antes | Después | Motivo |
|---|---|---|---|
| Tutorial (contradicción) | ¡Lo has pillado en una mentira! | ¡Has pillado una mentira! | Concordancia: salta igual con sospechosas (Carmen, Amparo, Encarna…) |
| Tutorial (libreta) | Aquí se apunta lo que dice cada uno; toca «añadir nota» para marcarlo. Si una pista no cuadra, enséñasela: así se pilla una mentira. | Se apunta lo que dice cada uno; con «añadir nota» marcas sospecha o descarte. Si una pista no cuadra, enséñasela: pillarás la mentira. | «marcarlo» no decía qué; usa los términos del glosario. Cabe en el límite de 25 palabras del tutorial (test) |
| Final, prueba clave | Tu prueba clave, «…», le señalaba. | Tu prueba clave, «…», señalaba a quien lo hizo. | «le» con una culpable es leísmo femenino |
| Desbloqueo (chat) | NUEVO SOSPECHOSO: Amparo (Vecina) | PUEDES INTERROGAR A: Amparo (Vecina) | Sin género y dice qué hacer |
| Desbloqueo (aviso) | Tu pregunta te pone sobre la pista de … | Tu pregunta te lleva hasta … | *pista* es lo que se apunta en la libreta: hacía creer que había una pista nueva |
| Ajustes, sección | PARTIDA | JUEGO | Glosario: *caso*, nunca «partida» |
| Chat, prueba enseñada | Muestra: … | Prueba: … | «Muestra» se leía como imperativo o sustantivo; lo que se enseña es una *prueba* |
| Ayuda nivel 2 | Prueba a preguntarle a X: «…» | Pregúntale a X: «…» | «Prueba» como verbo chocaba con el término *prueba*; más corto |
| Menú principal | Instrucciones | Cómo se juega | El botón se llama como la página que abre («CÓMO SE JUEGA») |
| Cómo se juega | el séptimo día es obligatorio | el séptimo día tendrás que hacerlo | Se leía «el día es obligatorio» |
| Cómo se juega | Culpable sin pruebas: sale libre. | Culpable sin pruebas: sobreseído, sale libre. | Los otros tres finales nombran su sello; este no (SOBRESEÍDO) |
| Terminar el día | Te queda 1 pregunta / Te quedan 3 preguntas | Te queda una pregunta / Te quedan tres preguntas | Números en letra |
| Botón Pensar | Pensar (1 pregunta) / Pensar (2 preguntas) | Pensar (una pregunta) / Pensar (dos preguntas) | Números en letra (cabe en el botón) |
| Acusación | En tu libreta: 3 pistas y 1 contradicción. | En tu libreta: tres pistas y una contradicción. | Números en letra, como el resumen de ayer |
| Dificultad | 7 / 4 / 5 preguntas al día… | Siete / Cuatro / Cinco preguntas al día… | Números en letra (la frase mezclaba «5» y «una») |
| Final, solidez | Con 2 puntos más de solidez… | Con dos puntos más de solidez… | Números en letra dentro de una frase (el marcador «3 de 7» sigue en cifra) |
| Mostrar prueba | Mostrar prueba: ninguna · 3 en la libreta (41 car.) | Mostrar prueba (tres en la libreta) (34 car.) | Pasaba del ancho de una línea en el desplegable |
| Lo que pasó de verdad (10 citas) | 'cena con clientes', 'Estoy cerca'… | «cena con clientes», «Estoy cerca»… | Toda la interfaz usa comillas latinas |

## No aplicados: decisiones para Cristian
1. **Etiquetas de estado sin género.** El retrato y la libreta dicen TRANQUILO / NERVIOSO / ASUSTADO / ENFADADO /
   TRISTE también con sospechosas. Propuesta: CALMA · NERVIOS · MIEDO · ENFADO · TRISTEZA (solo lo que se muestra; el
   enum y el prompt igual). Es un cambio de diseño visible en todas las pantallas y en el lector de pantalla.
2. **Títulos de las historias** con mayúsculas en cada palabra («La Hija Perfecta») → «La hija perfecta». Están en
   `Story*.cs`; no entran en ningún prompt (comprobado por el revisor), pero son archivos de historia.
3. **Error de la clave de Anthropic** («API key» en inglés y sin decir cómo arreglarlo): está en la capa de
   proveedores, fuera de límites salvo la excepción del punto 5.
4. **«Historia» con dos sentidos** (la dificultad y cada argumento) y **caso / historia** en el menú. Parece
   deliberado; no se cambió.

Revisado y correcto (sin cambios): glosario de la fase de juego, errores con «qué pasó + qué hacer», tuteo y castellano
de España, longitud de todos los botones (≤ 22 caracteres), tildes y ortografía.
