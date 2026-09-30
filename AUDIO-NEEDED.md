# Audio que falta

El juego pide cada sonido en el momento justo (`SoundManager`). **Ahora mismo suenan provisionales sintetizados
por código** (`Tools/make_placeholder_audio.py` → `Assets/Resources/Audio/**.wav`): efectos discretos y ambientes
de 30 s (lluvia en el menú, mar en la historia 2, viento en la 3, pulso grave en la acusación). Son para que el
juego no esté mudo mientras tanto: sustitúyelos por los definitivos con **el mismo nombre** (cualquier formato:
`.ogg`, `.wav`, `.mp3`) y borra el `.wav` provisional. Si borras la carpeta entera, el juego funciona en silencio. Para añadir uno, suelta el archivo en la ruta indicada (`.ogg` recomendado; también vale `.wav`
o `.mp3`), sin extensión en el nombre del recurso. No hay que tocar código ni la escena.

Carpeta base: `Assets/Resources/`. Ejemplo: `Audio/sfx/clic` → `Assets/Resources/Audio/sfx/clic.ogg`.

Tono general: noir de los años 40-50 trasladado a la España actual. Seco, analógico, con algo de sala (reverb corta).
Nada de sonidos de videojuego "brillantes". Los volúmenes se mezclan en el juego (Ajustes → Música / Efectos);
normaliza todo a −16 LUFS aprox. y deja 5 ms de silencio al principio.

## Efectos (`Audio/sfx/`)

| Ruta | Duración | Cuándo suena | Descripción |
|---|---|---|---|
| `Audio/sfx/clic` | 0,05–0,1 s | Cualquier botón | Clic seco de tecla de máquina de escribir o interruptor de baquelita. Muy corto y suave (suena mucho). |
| `Audio/sfx/enviar` | 0,3 s | Enviar una pregunta | Retorno de carro de máquina de escribir (el "ding" muy apagado o sin él). |
| `Audio/sfx/respuesta` | 0,2–0,4 s | Llega la respuesta del sospechoso | Papel que se desliza sobre una mesa, o un carraspeo muy discreto. |
| `Audio/sfx/maquina` | 0,05 s | Mientras se escribe la respuesta (cada pocas letras) | Una sola pulsación de máquina de escribir, suave; se reproduce con variación de tono. |
| `Audio/sfx/pista` | 0,8–1,2 s | Pista descubierta (la ficha cae en la libreta) | Ficha de cartulina que cae sobre la mesa + un acorde breve de piano grave o vibráfono, misterioso. |
| `Audio/sfx/contradiccion` | 1–1,5 s | Contradicción detectada | Golpe de cuerdas graves (stinger) con un poco de cola; tenso, no de terror. |
| `Audio/sfx/sello` | 0,3 s | Cualquier sello de tinta (contradicción, confidencial, finales) | Sello de goma golpeando papel sobre madera. |
| `Audio/sfx/nuevo_dia` | 1,5–2 s | Hoja de calendario del día nuevo | Hoja de papel que se arranca de un taco + un reloj que da una campanada lejana. |
| `Audio/sfx/acusacion` | 2–3 s | "El jurado delibera…" | Mazo de juez o puerta pesada que se cierra, seguido de silencio con zumbido de sala. |
| `Audio/sfx/latido` | 0,8 s (en bucle opcional) | Pantalla de acusación | Latido de corazón grave (lub-dub), apagado, casi subgrave. |
| `Audio/sfx/final_bueno` | 3–5 s | Final bueno | Resolución en tono mayor, piano y contrabajo; alivio sobrio, nada triunfal. |
| `Audio/sfx/final_agridulce` | 3–5 s | Final agridulce | Misma frase que el bueno pero que queda suspendida (acorde sin resolver). |
| `Audio/sfx/final_insuficiente` | 3–5 s | Culpable libre por falta de pruebas | Frase melancólica de saxo o trompeta con sordina; puerta que se cierra al final. |
| `Audio/sfx/final_malo` | 3–5 s | Acusación equivocada | Cuerdas graves disonantes que se apagan; lluvia de fondo. |

## Música (`Audio/musica/`, en bucle)

| Ruta | Duración | Cuándo suena | Descripción |
|---|---|---|---|
| `Audio/musica/menu` | 1–2 min, bucle limpio | Menú principal | Jazz noir lento: contrabajo con arco, piano, escobillas. Lluvia muy baja de fondo encaja con el menú (hay lluvia en la ventana). |
| `Audio/musica/historia1` | 2–3 min, bucle | "La hija perfecta" (casa familiar, Santiago) | Íntimo y doméstico: piano solo, reloj de pared, algo de inquietud. |
| `Audio/musica/historia2` | 2–3 min, bucle | "Noche de verano" (costa gallega, fiestas) | Verano que se torció: ecos de verbena lejana, guitarra, mar de fondo; melancólico. |
| `Audio/musica/historia3` | 2–3 min, bucle | "Humo y silencio" (finca en Jaén) | Seco y rural: guitarra española sobria, viento, crepitar de fuego muy bajo. |
| `Audio/musica/acusacion` | 1 min, bucle | Pantalla de acusación (reservado) | Pedal grave sostenido con pulsos; tensión creciente, sin melodía. |

## Notas

- Todo se carga con `Resources.Load`, así que los archivos van dentro de `Assets/Resources/Audio/`.
- Los efectos pueden solaparse (hasta 6 a la vez); la música cambia con un fundido cruzado de 1,2 s.
- "Reducir animaciones" no afecta al sonido. El volumen general, la música y los efectos se ajustan por separado.
