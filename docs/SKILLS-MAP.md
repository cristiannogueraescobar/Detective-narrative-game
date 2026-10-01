# Mapa de skills

Léelo al empezar cada sesión. Antes, comprueba que las skills cargan (`docs/SKILLS-SETUP.md`): si una da `Unknown skill`, el proceso es anterior a la sincronización.

- **Del proyecto** (`.claude/skills/`): reglas de este juego, cada una sacada de un caso real.
- **Instaladas**: técnica general, con prefijo de plugin (`unity:`, `unity-perf:`, `design-skills:`, `design:`, `superpowers:`, `data:`, `finecomb:`).

Si aplican varias, las de proceso (`superpowers:`) marcan el método y las del proyecto, las reglas.

## Por tipo de tarea

| Tarea | Del proyecto | Instaladas |
|---|---|---|
| **Historia, variante o ficha nueva** | `story-authoring`, `clue-design` | `superpowers:brainstorming` (si cambia la mecánica) |
| **Una pista sale poco o demasiado** | `clue-design`, `calibration-run` | `superpowers:systematic-debugging`, `data:statistical-analysis` (ruido, tamaño de muestra) |
| **Una respuesta del sospechoso suena mal** (memoria fingida, horas, idioma, estado) | `character-voice` | `superpowers:systematic-debugging`, `superpowers:test-driven-development` |
| **Temperatura, prompt, reintentos o modelo** | `qwen-prompting`, `calibration-run` | `data:statistical-analysis` (A/B) |
| **Medir un cambio** (calibrador, bot) | `calibration-run` | `data:statistical-analysis`, `superpowers:verification-before-completion` |
| **Estado nuevo, guardado o "recuerda la partida anterior"** | `game-state-reset` | `superpowers:test-driven-development` |
| **Pantalla o maquetación nueva** (uGUI + TMP) | `batchmode-capture`, `before-after-gallery` | `unity:ui-ugui`, `unity:optimize-text-mesh-pro`, `design-skills:interaction-design`, `design-skills:design-elevation`, `design:design-critique` |
| **Accesibilidad** (contraste, tamaño de texto, objetivos táctiles) | `batchmode-capture` | `design-skills:accessibility-audit`, `design:accessibility-review` |
| **Textos de la interfaz** | — | `design:ux-copy`, `design-skills:ux-writing` |
| **Flujo del jugador** (tutorial, primera partida, finales) | — | `design-skills:journey-mapping`, `design-skills:ux-research` |
| **Efectos y postproceso** (viñeta, noir, relieve) | `before-after-gallery` | `unity:urp-postprocessing`, `unity:validate-urp-render-graph-renderer-feature`, `unity-perf:unity-performance` |
| **Animaciones** | `batchmode-capture` (medir fotogramas), `before-after-gallery` | `design-skills:interaction-design`, `unity-perf:csharp-zero-gc` (Update sin basura) |
| **Retrato sin arte propio** | `pixel-portrait-kitbash`, `before-after-gallery` | `unity:2d-pixel-perfect`, `unity:sprite-editor` |
| **Arte nuevo** (encargo, importación) | `art-brief`, `pixel-portrait-kitbash` | `unity:2d-pixel-perfect`, `unity:manage-sprite-atlas`, `unity:sprite-editor` |
| **Rendimiento o memoria** (móvil) | `calibration-run` (el coste del LLM se mide igual) | `unity-perf:unity-performance`, `unity-perf:csharp-zero-gc`, `unity-perf:code-standards` |
| **Revisión de código o seguridad** | `git-hygiene` (claves) | `finecomb:finecomb`, `superpowers:requesting-code-review`, `unity-perf:code-standards` |
| **Editar muchos archivos con script** | `safe-script-edits` | — |
| **Tests, capturas, build de Windows** | `batchmode-capture`, `windows-unity-env` | `unity:unity-cli`, `superpowers:verification-before-completion` |
| **Android** | `windows-unity-env` | `unity:unity-cli`, `unity-perf:unity-performance` (ver `docs/ANDROID-OPTIONS.md`) |
| **Traducción** | `character-voice` (el modelo habla castellano) | `unity:localization` |
| **Commits, ramas, fusiones** | `git-hygiene` | `superpowers:finishing-a-development-branch`, `superpowers:using-git-worktrees` |
| **Fallo que no se entiende** | — | `superpowers:systematic-debugging` |
| **Plan de varias horas o sesión nocturna** | `session-report` | `superpowers:writing-plans`, `superpowers:executing-plans`, `superpowers:subagent-driven-development` |
| **Cerrar un bloque o una sesión** | `session-report`, `before-after-gallery` | `superpowers:verification-before-completion` |
| **Escribir o cambiar una skill** | — | `superpowers:writing-skills` (y pruébala en un proceso nuevo: `docs/SKILLS-SETUP.md`) |

## Siempre

- Cambio de código: `superpowers:test-driven-development`, con el test en rojo primero.
- Antes de decir "hecho": `superpowers:verification-before-completion`. Cambio visual: render a 1080×1920 y 1080×2400 revisado por ti (`batchmode-capture`).
