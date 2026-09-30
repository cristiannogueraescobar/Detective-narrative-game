# Detectives

![Unity](https://img.shields.io/badge/Unity-6000.3-000000?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![LLM](https://img.shields.io/badge/LLM-Ollama%20%7C%20Claude-7B68EE?style=for-the-badge)

> A noir detective game for phones (portrait) where you interrogate suspects in free text. The suspects are
> played by a language model; the rules — clues, lies, contradictions and endings — are decided by the game.
> The game is in Spanish.

---

## 📖 About

Three stories, each with **three variants**: the cast and the crime are the same, but the culprit changes, so
you never know who did it until you work it out. You have **7 days and 5 questions per day** (7 or 4 depending
on the difficulty). Ask anything, in your own words; compare each suspect's version in your notebook with your
clues; show a clue to the suspect you think is lying; accuse once, naming your key piece of evidence.

| Story | Setting | Victim |
|---|---|---|
| *La hija perfecta* | A family house in Santiago de Compostela | Elena, 12 |
| *Noche de verano* | A Galician coastal village during the summer fair | Sofía, 19 |
| *Humo y silencio* | An olive farm in Jaén | Paula, 15 (missing) |

**How a question works**
1. You type a question (or tap one of the suggested openers) and, optionally, attach a clue as evidence.
2. The suspect answers in character, with a hidden emotional state (calm, nervous, scared, angry, sad) that
   drives their portrait.
3. The game — not the model — checks the answer against deterministic **anchors** for each clue. A matching
   answer puts the clue in your notebook; a clue that contradicts the culprit's lie gets a **CONTRADICTION** stamp.
4. When you accuse, the ending depends on who you accused and how much evidence you gathered: *Caso cerrado*,
   *Cerrado con dudas*, *Sobreseído* or *Caso fallido*, followed by the true timeline, the clues you missed, your
   detective rank and the culprit's police file.
5. Stuck? **Think** (in the notebook) gives a tiered hint: first who to talk to, then a concrete question.

---

## ✨ Features

- **Mobile-first UI** (uGUI + TextMeshPro): chat bubbles, typewriter answers, notebook with tappable clues and
  suspects, lineup accusation, stamps and day-card transitions, safe-area aware, 48 dp touch targets.
- **Noir look**: 2.5D lit portraits (relief + interrogation lamp, computed in a UI shader from the original art),
  URP post-processing (grading, split toning, highlight-only bloom) with WCAG contrast measured *after* the effect,
  procedurally generated pixel-art backdrops per story. Every big visual change can be switched off in the theme.
- **Accessibility**: screen reader support (TalkBack / VoiceOver through Unity 6's accessibility module), three
  text sizes (seeded from the system font scale), high contrast (AAA), visible focus, text speed, reduce motion,
  noir filter toggle — all applied live and saved; WCAG 2.2 AA audited.
- **Game design**: difficulty levels, tiered hints, key-evidence accusation and detective ranks, natural
  character unlocks by topic, the notebook keeps each suspect's version to compare with the clues, your own
  notes per suspect (suspicion / ruled out) and every morning report; the lineup strikes through whoever you or a
  found clue ruled out; replaying a story brings a culprit you haven't seen yet.
- **Two LLM providers** behind one interface: Ollama (default, local, `qwen2.5:7b-instruct`) or Anthropic Claude.
- **Robust to the model's quirks**: repeated answers and answers with invented times are asked once more
  (invented times −74 %); suspects reject false premises in leading questions (22 % → 2 %); character sheets are
  kept under a measured word budget; a narrative validator checks timelines, lies, alibis and reports.
- **Save / continue**, case records with your best ending, Android back button, Windows build that opens as a
  phone-shaped window.
- **Tooling** (Unity editor, batch mode): a bot that plays full games with the real game logic, clue and emotion
  calibrators, probes for leading questions and opener questions, screenshot and animation capture.
- **Tests**: 700+ EditMode and PlayMode tests (layout validation at several screen sizes, text sizes and
  high contrast, save compatibility, state machine and game flows, screen reader).

---

## 🚀 Running it

1. Open the project in **Unity 6000.3.2f1** and open `Assets/Scenes/Game.unity`.
2. LLM: install [Ollama](https://ollama.com/) and run `ollama pull qwen2.5:7b-instruct`, or select the
   Anthropic provider on `AIConversationManager` and put your key in `ANTHROPIC_API_KEY` or in
   `anthropic_api_key.txt` at the project root (git-ignored). See `QUICK-START.md`.
3. Press Play with the Game view set to a portrait resolution (1080x1920).

Builds: `Detective → Build de Windows` (see `docs/BUILD.md`); Android steps in `docs/BUILD.md`.

---

## 🗂️ Project structure

```
Assets/Scripts/
├── GameManager.cs, MenuManager.cs          game flow, menu, case selection, save/continue
├── AIConversationManager.cs                one question → LLM → analysis (clues, lies, emotion)
├── InterrogationUI*.cs                     interrogation, notebook, accusation and endings
├── Cases/                                  the 3 stories × 3 variants, prompt builder, clue detector
├── LLM/                                    Ollama and Anthropic providers
├── UI/, UI/Chat/, UI/Fx/, Emotions/, Audio/, Save/
└── Editor/                                 bot player, calibrators, probes, capture and build tools
Assets/Tests/EditMode, Assets/Tests/PlayMode
docs/                                       design notes, reports, research, build instructions
```

---

## 📄 License

MIT License - Feel free to use this code for learning purposes.

---

## 👨‍💻 Author

**Cristian Noguera**

- Computer Science Student at Solent University London
- Specializing in Game Development and Backend Systems

---

## 🙏 Acknowledgments

- **Anthropic** for Claude API
- **Unity Technologies** for the game engine
- **TextMeshPro** for advanced text rendering

---

## 📮 Contact

For questions or collaboration opportunities, find me on GitHub: [@cristiannogueraescobar](https://github.com/cristiannogueraescobar)

---

**Note**: This project is part of my Computer Science portfolio, showcasing skills in game development, AI integration, and software architecture.
