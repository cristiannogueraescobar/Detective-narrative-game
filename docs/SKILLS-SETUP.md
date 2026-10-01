# Skills: comprobar que cargan

## Qué pasó (01-10-2026)

En las sesiones del 30-09 y del 01-10, `finecomb`, `unity-perf`, `design-skills` y `unity` dieron
`Unknown skill`, mientras `superpowers` y `design` funcionaban. Esto es lo que se comprobó:

| Comprobación | Resultado |
|---|---|
| `claude plugin list` | los 10 plugins `✔ loaded` (sincronizados desde claude.ai, `@synced`) |
| Carpetas en `~/.claude/plugins/synced/…/` | los que funcionaban llegaron el **28-09 a las 17:16**; los cuatro que fallaban, el **30-09 a las 10:05** |
| Inicio del proceso de esta conversación | **28-09 a las 20:12** (transcript `d6f71d4d…`, primera marca de tiempo 18:12 UTC) |
| `claude -p` nuevo, invocando las 4 skills | las cuatro `LOADED` |
| Versión de Claude Code | 2.1.285 (no influye) |

**Causa:** Claude Code registra los plugins y las skills **al arrancar el proceso**. Esta conversación fue el
mismo proceso desde el 28-09: cada "sesión nueva" era una continuación (resume o compactación), así que nunca vio
los plugins que se sincronizaron después de arrancar. No era la instalación, ni el alcance (usuario o proyecto), ni
la versión.

**Observado después (01-10-2026, 10:20):** al crear `.claude/skills/` a mitad de la sesión, Claude Code volvió a
leer la lista de skills de esa misma sesión, con los cuatro plugins incluidos. `unity:sprite-segment-3x3grid` cargó sin
reiniciar. No está documentado como garantía, así que lo fiable sigue siendo reiniciar.

## Al empezar cada sesión (30 segundos)

1. **¿Es un proceso nuevo?** Si la conversación viene de días atrás, sal (`/exit`) y vuelve a entrar con
   `claude --continue`: se conserva la conversación y se cargan los plugins y las skills de nuevo.
2. **¿Qué ve el CLI?** En otra terminal: `claude plugin list`. Todos deben estar `✔ loaded`.
3. **¿Los ve la sesión?** Pide al agente que invoque una skill de cada plugin que vaya a usar (por ejemplo
   `unity-perf:code-standards`). Si responde `Unknown skill` pero el paso 2 dice `✔ loaded`, la sesión es
   anterior a la sincronización: vuelve al paso 1.
4. **Skills del proyecto** (`.claude/skills/`): se cargan igual, al arrancar. Una skill nueva o editada no la ve
   una sesión que ya estaba abierta.

Prueba rápida sin abrir sesión (gasta unos céntimos de tokens):

```bash
claude -p "Invoke the Skill tool with 'unity-perf:code-standards' and reply LOADED or the exact error." --max-turns 3 < /dev/null
```

## Si no cargan

| Síntoma | Qué hacer |
|---|---|
| `Unknown skill` y `plugin list` dice `✔ loaded` | Proceso viejo: `/exit` y `claude --continue` |
| El plugin no sale en `plugin list` | Reinstálalo en claude.ai (Ajustes → Plugins) y abre una sesión nueva; la sincronización baja a `~/.claude/plugins/synced/` |
| Sale con error de carga | `claude plugin validate <ruta del plugin>` y mira el mensaje |
| Una skill del proyecto no se activa sola | Revisa su `description` (es lo único que se lee para decidir) y prueba con `claude -p` en un proceso nuevo |

## Cómo se probaron las skills del proyecto

`writing-skills`: cada tarea simulada se ejecutó en un proceso nuevo (`claude -p`, sin permiso para modificar
archivos), primero **sin** skills (línea base) y luego **con** ellas. `CLAUDE.md` y el mapa quedaron fuera, así que cada
skill tenía que activarse solo por su descripción. Resultado de las 14 tareas:

| | Sin skills | Con skills |
|---|---|---|
| Se activó la skill correcta sola | — | **14/14** |
| Comprobaciones (5 por tarea, revisadas a mano) | 53/70 | 63/70 por regex (≈67 a mano) |
| Llamadas a herramientas | 140 | 93 (−34 %) |
| Tiempo | 13,3 min | 9,9 min (−26 %) |
| Coste | 6,19 $ | 4,24 $ (−31 %) |

Donde más cambió: `game-state-reset` (2 → 4-5: test con marcador por las cuatro entradas), `story-authoring` (3 → 5:
NarrativeValidator y recalibrar), `calibration-run` (3 → 5: `-tries 10` y worktree aparte), `batchmode-capture` (3 → 5:
`capture-anim.sh` y UnityConnect). Cinco tareas ya salían 5/5 sin skill (el agente lee los docs del proyecto). En esas, la
skill solo ahorra tiempo y coste.

Para repetir la prueba tras cambiar una skill: misma tarea en `claude -p` en un proceso nuevo, con
`--output-format stream-json --verbose`. Busca la invocación `"name":"Skill"` y compara la respuesta.
