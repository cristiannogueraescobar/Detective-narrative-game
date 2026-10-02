# Art commission — 5 characters × 3 expressions (15 portraits)

*Internal note (Spanish team): este es el paquete para enviar a un artista. Sustituye al intento de generación local de
la sesión B (13 pruebas ciegas sin aprobar, ver `docs/REPORT-SESION-B.md` en `feature/retratos-javier`). Las
herramientas de generación (`Tools/portrait_gen/`, `docs/art/LOCAL-GEN.md`, LoRA en `C:\AI\lora`) quedan documentadas
en esa rama y aparcadas. Envía la página 1 y la carpeta `docs/art/encargo/`.*

---

## Page 1 — Summary for the artist

**The game.** *Detectives* is a mobile (portrait 9:16) noir interrogation game set in small-town Spain. The player
questions suspects; each suspect is shown as a full-body pixel-art portrait that changes with their emotional state.

**What we need.** 15 full-body pixel-art portraits: **5 characters × 3 expressions**, matching the style of three
existing portraits we send as references (`references/02`, `03`, `04`).

| # | Character | Age | Expressions (file names) |
|---|---|---|---|
| 1 | Javier Romero — olive farmer | 44 | `javier_tranquilo`, `javier_triste`, `javier_nervioso` |
| 2 | Daniel Mendoza — lawyer | 48 | `daniel_tranquilo`, `daniel_nervioso`, `daniel_enfadado` |
| 3 | Amparo Gil — retired widow | 70 | `rosario_tranquilo`, `rosario_nervioso`, `rosario_triste` |
| 4 | Maruxa Pena — Galician widow | 74 | `maruxa_tranquilo`, `maruxa_triste`, `maruxa_nervioso` |
| 5 | Encarna Molina — widow in mourning | 63 | `encarna_tranquilo`, `encarna_nervioso`, `encarna_triste` |

(*tranquilo* = calm, *triste* = sad, *nervioso* = nervous/defensive, *enfadado* = angry. Amparo's files use the
internal key `rosario`.)

**Hard requirements (all 15 images):**
1. **PNG, 768 × 1024 px, transparent background**, sRGB.
2. **Full body**, standing, front three-quarter view, centred. Figure height **89-92 % of the canvas** (~920 px), feet
   ~40 px above the bottom edge.
3. **Style of the references:** medium visible pixel blocks (4-5 px at this size), realistic adult proportions
   (head ≈ 1/5 of body height; not chibi), **thick black outline**, flat cel shading, warm frontal light from slightly
   left, warm palette.
4. **Soft grey shadow under the feet** (semi-transparent, ~18 % opacity), falling slightly to the right.
5. **The 3 expressions of a character share the same pose and framing**: only the face, shoulders and hands change.
   Framing difference between expressions **≤ 1 %** (the game swaps images live; the figure must not jump).
6. **No expression may suggest guilt or innocence** (several characters are guilty in one scenario and innocent in
   others, with the same images).
7. Fictional characters: **no likeness to real people**.

**Deliverables:** 15 PNGs + layered source files (Aseprite or PSD), one folder per character, in one zip. Delivery of
the 5 `*_tranquilo` images first, for approval, before the other 10.

---

## 1. Style guide (measured on the references)

The current game has two styles. Match **references 02, 03 and 04 only**. Daniel's current portrait (`current/`) is the
other, older style: bigger pixels and a big head. Do not follow it.

| Aspect | Value |
|---|---|
| Canvas | 768 × 1024 px (3:4), transparent PNG |
| Framing | Full body, figure 89-92 % of the height, feet ~40 px from the bottom, centred |
| Proportions | Adult realistic: head (crown to chin) ≈ 0.19-0.24 of figure height. Not chibi. |
| Pixel | Medium visible blocks, 4-5 px at 768 × 1024 (≈ 200-230 "art pixels" tall). No smooth gradients. |
| Outline | Black (#000000), thick: 1-2.4 % of the figure height (≈ 10-22 px). |
| Shading | Flat cel shading, 2-3 tones per material, crisp highlights. Faces line-drawn: clear eyes (white + pupil), brows and mouth. |
| Palette | Warm skin (#D29F61, #DD9D53, #F3CE95), cream lights (#EFE0B2, #FCEDA8), rust/terracotta (#A44927, #B93314, #7A1A16), browns (#3E1411, #5C2815, #37322D), muted blue (#182E43, #2A6581). |
| Light | Frontal, warm, soft, slightly from the left. No rim light, fog or dramatic lighting (the game adds the noir mood itself). |
| Shadow | Soft grey under the feet, ~18 % opacity, slightly to the right of each shoe, 3-12 % wider than the shoes. |
| Props | One prop per character, held in a hand (listed below). |

**References** (in `references/`):

| File | Use it for |
|---|---|
| `02-style-main-adult-man.png` | **Main style reference**: adult heavy-set man, rolled sleeves, prop in hand, proportions, pixel size, outline, shadow |
| `04-style-line-teen.png` | Line quality, natural pose, same pixel and head size |
| `03-style-palette-woman.png` | **Palette only** (her head is slightly larger than the target) |

**Current images** (in `current/`) show what each character looks like today. They are placeholders made by recolouring
other portraits: **do not copy their faces or poses**. The three widows (Amparo, Maruxa, Encarna) share one face and one
pose today; the new ones must be three clearly different women.

## 2. Characters

### 2.1 Javier Romero (44)
- **Who:** olive farmer from Jaén (southern Spain), owner of the farm "Los Olivares". Divorced three months ago; his
  daughter has gone missing days before the custody hearing.
- **Personality:** bitter and defensive; shifts between self-pity and anger; says things like *"Sure, now I'm the bad
  guy. As always."*
- **Look:** heavy build, weathered tanned face, square jaw; short dark-brown hair greying at the temples; several days'
  stubble beard (**no moustache**); red-rimmed tired eyes. Olive-and-brown plaid flannel shirt, sleeves rolled up;
  dusty brown work trousers; worn leather boots. **Prop:** a green beer bottle hanging from his right hand.
- **Height in game:** 175 cm.
- **Expressions:**
  - `javier_tranquilo` (base): exhausted and closed, not hostile; straight mouth, heavy eyelids, looking ahead.
  - `javier_triste`: grief and a little self-pity; inner brows raised, eyes down and watery, shoulders slumped, the
    bottle hanging low. Not remorse.
  - `javier_nervioso` (also used for angry and scared): cornered and defensive with a hint of anger; furrowed brow,
    clenched jaw, side glance, one sweat drop, fingers tight on the bottle.

### 2.2 Daniel Mendoza (48)
- **Who:** lawyer with his own firm in Santiago de Compostela (north-west Spain); father of a teenage son and adoptive
  father of a girl who has died.
- **Personality:** controlled, precise, cold; cares about appearances. *"I'd ask you to be precise, inspector. I am."*
- **Look:** tall, well kept; neat dark hair, clean-shaven or a short trimmed moustache; impeccable dark suit, white
  shirt, dark tie loosened a little. **Prop:** a smartphone in one hand.
- **Height in game:** 178 cm.
- **Note:** his current portrait is in the other, older style; redraw him in the style of 02-04.
- **Expressions:**
  - `daniel_tranquilo` (base): composed and cool; neutral mouth, steady gaze, upright posture.
  - `daniel_nervioso` (also used for scared): controlled tension; tight lips, brief side glance, hand tightening on the
    phone, a slightly stiffer neck. Subtle.
  - `daniel_enfadado`: cold anger, not shouting; narrowed eyes, jaw set, chin slightly up, looking down at the viewer.

### 2.3 Amparo Gil (70) — file key `rosario`
- **Who:** retired widow in Santiago; lives right across the street from the family and sleeps little.
- **Personality:** friendly busybody, warm and talkative. *"Look, dear, it's not that I spy, but one has eyes."*
- **Look:** small and plump; silver hair in a bun; reading glasses on a chain; floral house dress, purple cardigan,
  slippers. **Prop:** a cup of coffee or a handkerchief (**no binoculars**).
- **Height in game:** 155 cm.
- **Expressions:**
  - `rosario_tranquilo` (base): friendly and curious; slight smile, attentive eyes.
  - `rosario_nervioso`: flustered; hand to her chest, eyebrows up, a quick nervous smile.
  - `rosario_triste`: moved and sorry; eyes wet, handkerchief near her mouth, shoulders down.

### 2.4 Maruxa Pena (74)
- **Who:** very old widow living alone in a house on a bend of a coastal road in Galicia; gets up at dawn every day.
- **Personality:** suspicious and wary at first, then talks. *"I don't know anything, dear... well, I did see something."*
  (She uses a Galician word, *fillo*, "son".)
- **Look:** thin, weathered; white hair in a bun under a dark headscarf; navy dress with small white flowers, brown wool
  cardigan, thick stockings, dark shoes; work-worn hands. **Prop:** a wicker basket or a walking stick.
- **Height in game:** 150 cm.
- **Expressions:**
  - `maruxa_tranquilo` (base): wary but calm; squinting slightly, mouth closed, hands on the basket/stick.
  - `maruxa_triste`: quiet sorrow; eyes down, mouth trembling slightly.
  - `maruxa_nervioso`: uneasy; glancing aside, hand pulling the cardigan closed.

### 2.5 Encarna Molina (63)
- **Who:** widow who owns the farm next to "Los Olivares", keeps horses; lost her own daughter many years ago. Very
  religious.
- **Personality:** sweet-spoken, pious, warm. *"Oh, dear, such terrible things happen. May the Virgin protect her."*
- **Look:** medium build; dark-grey hair in a bun; dressed in mourning — black dress, charcoal cardigan; small medal of
  the Virgin on a chain; riding boots. **Prop:** a rosary or a horse brush.
- **Height in game:** 160 cm.
- **Important:** her expressions must be **warm and kind, never sinister or unsettling**: in one scenario she is guilty
  and in the others she is not, and her face must not tell which.
- **Expressions:**
  - `encarna_tranquilo` (base): kind and gentle; soft smile, hands together.
  - `encarna_nervioso`: anxious; fingers on the medal, brows up, a strained smile.
  - `encarna_triste`: grieving; eyes wet and down, rosary held close.

## 3. Deliverables and delivery format

| Item | Detail |
|---|---|
| Images | 15 PNG, 768 × 1024, transparent, sRGB, file names exactly as in the table on page 1 (lower case, `.png`) |
| Source | Layered files (Aseprite `.aseprite` or `.psd`), one per character, expressions as layers or frames |
| Structure | One zip: `javier/`, `daniel/`, `rosario/`, `maruxa/`, `encarna/`, each with its 3 PNG + source |
| Milestones | (1) the 5 `*_tranquilo` for approval; (2) the remaining 10 after approval |
| Checks we run | Size and transparency; framing difference between a character's expressions ≤ 1 %; outline thickness; figure height 89-92 %; side-by-side with references 02-04 |
| Rights | Full commercial rights for the game and its marketing (to be confirmed in the contract) |

## 4. Internal integration notes (Spanish team)
- Destino: `Assets/Art/Portraits/<artId>_<estado>.png` (Amparo: `rosario`). El juego prefiere el arte nuevo al derivado.
- Encuadre: `PortraitCrops.For` (rama `feature/retratos-javier`, sin fusionar) ya tiene la entrada de Javier; añadir las
  de los demás tras medir. Importación: bilineal con mipmaps, máximo 1024.
- Cambiar las vecinas exige comprobar la rueda de alturas (155, 150 y 160 cm) y la galería antes/después.
- Decisiones de Cristian pendientes: presupuesto y plazo; si se admite que el artista use IA; el teléfono como objeto de
  Daniel (alternativa: un maletín).
