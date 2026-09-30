# Informe de la noche 2 (30 sep 2026, 01:30 → mañana)

Rama: **`feature/noche2`** (sale de `feature/ui-layout`, que sale de `feature/ui-movil`). Nada en `main`, nada subido.
Diario minuto a minuto: `docs/NIGHT-LOG.md`. Tests: **EditMode y PlayMode en verde** (cifras al final).

---

## 1. Resumen

| Área | Qué hay ahora |
|---|---|
| **Ver mi trabajo** | Capturas en batchmode: `ScreenshotTool` (vista previa, todos los paneles + 4 finales, 1080x1920 y 1080x2400) y `AnimationCapture` (en juego, fotogramas de cada animación). Todo en `docs/screenshots/2026-09-30/`. |
| **Layout** | LayoutGroups en todos los paneles, textos que no desbordan, zonas táctiles de 48 dp, safe area, scroll solo vertical. Test automático (`LayoutValidationTests`): desbordes, solapes, glifos, contraste WCAG de textos y casillas, zonas táctiles, 3 pantallas y texto "muy grande". |
| **Chat** | Burbujas (tú a la derecha, sospechoso a la izquierda con mini-retrato y hora de juego), avisos centrados, "escribiendo…", máquina de escribir que se salta con un toque, auto-scroll que respeta al que lee + "Nuevos mensajes", pool de filas. |
| **Animación y ambiente** | Menú vivo (polvo en la luz, parpadeo de lámpara, lluvia en el cristal, vapor del café, título que se enciende), retratos con postura por emoción (temblor y sudor, retroceso, sacudida y rojo, bajada y desaturación), ficha de pista que vuela a la libreta, sello CONTRADICCIÓN, calendario del nuevo día, viñeta y latido en la acusación, "El jurado delibera…", finales con sello, color propio y línea temporal, expediente con sello CONFIDENCIAL, filtro noir (grano + viñeta). |
| **Sonido** | `SoundManager` con eventos para todo, música por historia y menú con fundido cruzado, volúmenes de música y efectos, silencio si faltan archivos. `AUDIO-NEEDED.md`. |
| **Contenido** | Instrucciones y Acerca de reescritos, tutorial de 4 indicaciones (saltable), HUD claro ("quedan N preguntas"), selección de caso con el mejor final de cada historia, botón Atrás de Android. |
| **Narrativa con qwen** | Pistas flojas: 3A_audios 60 %→100 %, 3C_fotos 60 %→80 %, 2C_gps 60 %→50 % (ver pendientes). Víctima: "triste" 36/36 (antes "tranquilo" 16/36). Jugador bot: 9 variantes jugadas por qwen de principio a fin. Sin respuestas repetidas palabra por palabra. |
| **Accesibilidad** | Tamaño de texto (3 niveles), alto contraste (AAA), velocidad del texto, reducir animaciones, filtro noir, vibración. Todo persistente y en caliente. |
| **Build** | Game.unity única escena. Build de Windows OK y **el .exe arranca** (prueba de humo "SMOKE OK"). Android: módulo no instalado → pasos en `docs/BUILD.md`. |

(Este informe se completa al final de la noche con métricas, galería y orden de pruebas.)

---

## 2. Decisiones creativas y por qué

- **Sutileza antes que espectáculo.** Todo efecto dura menos de 1,6 s y solo aparece cuando pasa algo que
  importa (pista, contradicción, día, veredicto). El resto del tiempo el juego respira: la lámpara del menú casi
  no parpadea, el polvo solo se ve en la luz, el grano está al 5 %.
- **El papel como material del juego.** Fichas de pista, hoja de calendario, libreta, expedientes de la
  selección de caso y sellos de tinta comparten el mismo papel crema y tinta roja: es la "prop" del detective y da
  coherencia sin arte nuevo.
- **Sellos como lenguaje.** CONTRADICCIÓN, CONFIDENCIAL y los cuatro finales (CASO CERRADO, CERRADO CON DUDAS,
  SOBRESEÍDO, CASO FALLIDO) son sellos que caen y golpean. En los finales el sello es el titular y el informe
  se descubre línea a línea, con la verdad como línea temporal con horas.
- **El chat manda.** Es un juego de conversación: el retrato pasa a plano medio junto a los controles y el chat
  ocupa ~60 % de la pantalla (antes ~36 %). El estado emocional se lee en la cara (tinte, postura, temblor,
  sudor) y en una etiqueta.
- **Arte existente, sin deformar.** El fondo del menú se estiraba del 2:3 al 9:16; ahora cubre sin deformar.
  El fondo de la sala de interrogatorios estaba en el proyecto sin usar: ahora es el fondo del expediente.
  Los retratos pixel art de cuerpo entero se muestran en plano medio (recorte medido, originales intactos).
- **Arte nuevo solo por código y en archivos nuevos:** iconos de línea (`Tools/make_icons.py`), icono de la
  aplicación (`Tools/make_app_icon.py`), sprites de interfaz generados en ejecución (esquinas, círculo, viñeta,
  grano).
- **La variante es sorpresa.** En la selección de caso eliges historia, no culpable: se mantiene la gracia de
  "3 historias × 3 variantes".
- **Nada de sustos para el que lee.** Si el jugador ha subido a releer, el chat no se le mueve: aparece
  "Nuevos mensajes".
- **El Atrás de Android nunca saca del juego**: cierra lo que haya abierto y, en el interrogatorio, no hace nada.

---

## 3. Qué rama probar y en qué orden

**Rama `feature/noche2`.** El proyecto principal (`C:\Dev\Detective-narrative-game`) ya está en esa rama;
abre Unity y deja que reimporte. Game View a **1080x1920 (vertical)**; después prueba 1080x2400.

1. **Tests primero** (Window → General → Test Runner): *EditMode* → Run All (todo verde). *PlayMode* → Run All
   (verde; `AnimationCapture` sale como *Explicit* y no corre sola: son las capturas).
2. **Menú** (Play): mira 5–10 s sin tocar. Polvo en la luz de la lámpara, algún parpadeo, lluvia en la ventana
   (arriba a la izquierda), vapor del café (derecha), título que se enciende. Nada estirado.
3. **Ajustes**: sube *Tamaño del texto* a "Muy grande" y vuelve (cambia al momento); *Alto contraste* sí/no;
   *Filtro noir* sí/no; el deslizador de *Efectos* suena al soltar si hay audio. Atrás (Esc) vuelve al menú.
4. **Instrucciones** y **Acerca de**: se leen enteras, con scroll.
5. **Jugar → Elige un caso**: tres expedientes con "SIN RESOLVER". Elige uno. Esc aquí cierra la selección.
6. **Intro**: "EXPEDIENTE Nº 00X", el parte se escribe (un toque lo completa) y cae el sello CONFIDENCIAL.
7. **Interrogatorio**:
   - Arriba el retrato en plano medio con su etiqueta de estado; a la derecha sospechoso / Fin del día / Acusar.
   - El chat empieza con la tarjeta del DÍA 1. Sale la primera indicación del tutorial (prueba "Saltar tutorial"
     en otra partida).
   - Pregunta algo: tu burbuja sale al momento, luego "escribiendo…", luego la respuesta letra a letra (un toque
     la completa). Mira que el retrato cambie según el estado.
   - Sube a leer mientras responde: no te mueve; aparece "Nuevos mensajes".
   - Si sale una pista: ficha que cae, destella y vuela a "Libreta" (contador rojo). Abre la libreta: papel.
   - Gasta las 5 preguntas: "Fin del día" pasa a dorado. Púlsalo: hoja de calendario con el parte (dos toques).
8. **Acusar**: rueda de reconocimiento con los bustos; el que tocas se marca. "Volver" (o Esc) vuelve y
   recupera la música del caso. Acusa: "El jurado delibera…" y el final con su sello y la línea temporal.
9. **Jugar otra vez** → selección de caso: el expediente muestra el mejor final que sacaste.
10. **Continuar**: cierra el juego a mitad de partida y vuelve: "Continuar · <caso>, día N" y el chat entero.
11. **Build**: `Builds/Windows/Detectives.exe` (ver docs/BUILD.md; `-smoketest` para la prueba de humo).
