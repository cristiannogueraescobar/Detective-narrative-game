# Diario de la noche 2 (2026-09-30)

Inicio: 01:32. Fin previsto: ~12:00–13:30 (10–12 h).
Rama de trabajo: `feature/noche2` (sale de `feature/ui-layout`, que sale de `feature/ui-movil`).
Si el contexto se compacta: releer este archivo y seguir por "Ahora".

## Hecho
- 01:30 Arreglo de raíz del layout (rama `feature/ui-layout`, 3 commits): LayoutKit + LayoutGroups en todos los
  paneles, política de texto (TextStyle: una línea con "…", varias líneas con autoajuste, scroll a tamaño fijo),
  chat solo vertical, barra superior en dos filas, test `LayoutValidationTests` (desbordes, solapes, glifos,
  contraste WCAG, 3 pantallas × 9 paneles). 337/337.
  - Causa de los 72 px en Ajustes: TMP reinicia a sus valores por defecto los textos creados en un panel inactivo
    la primera vez que se activa. `TextStyle` reaplica en `OnEnable` (con `[ExecuteAlways]`).
  - Rojo de peligro aclarado a #E06A5E (5,2:1 sobre el panel; antes 3,2:1).
- `.vscode/`, `*.slnx` y `.superpowers/` al `.gitignore`. Worktree `dng-layout` borrado.

## Ahora
- 0. Capturas en docs/screenshots/<fecha>/ a 1080x1920 y 1080x2400, todos los paneles y los 4 finales.

## Después (orden)
1. Layout: 3 rondas de render→revisión, safe area, zonas táctiles ≥ 48 px (añadir al test).
2. Chat con burbujas, "escribiendo…", máquina de escribir saltable, auto-scroll respetuoso, pooling.
3. Animación y ambientación (menú vivo, retratos por emoción, pista, contradicción, día, acusación, finales,
   expediente, filtro noir).
4. SoundManager + volúmenes + AUDIO-NEEDED.md.
5. Instrucciones, tutorial ligero, Acerca de, revisión de textos.
6. Pistas flojas (2C_gps, 3A_audios, 3C_fotos), emoción al preguntar por la víctima, jugador bot.
7. Accesibilidad: tamaño de texto, alto contraste, velocidad de texto, filtro noir.
8. Build de Windows (y APK si hay módulo).
FINAL. Rondas de revisión.

## Decisiones (opción conservadora, para revisar)
- (ninguna todavía)
