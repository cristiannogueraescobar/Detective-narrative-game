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
you never know who did it until you work it out. You have **7 days and 5 questions per day**. Ask anything, in
your own words; show a clue from your notebook to the suspect you think is lying; accuse once.

| Story | Setting | Victim |
|---|---|---|
| *La hija perfecta* | A family house in Santiago de Compostela | Elena, 12 |
| *Noche de verano* | A Galician coastal village during the summer fair | Sofía, 19 |
| *Humo y silencio* | An olive farm in Jaén | Paula, 14 |

**How a question works**
1. You type a question (or tap one of the suggested openers) and, optionally, attach a clue as evidence.
2. The suspect answers in character, with a hidden emotional state (calm, nervous, scared, angry, sad) that
   drives their portrait.
3. The game — not the model — checks the answer against deterministic **anchors** for each clue. A matching
   answer puts the clue in your notebook; a clue that contradicts the culprit's lie gets a **CONTRADICTION** stamp.
4. When you accuse, the ending depends on who you accused and how much evidence you gathered: *Caso cerrado*,
   *Cerrado con dudas*, *Sobreseído* or *Caso fallido*, followed by the true timeline.

---

## ✨ Features

- **Mobile-first UI** (uGUI + TextMeshPro): chat bubbles, typewriter answers, notebook with tappable clues and
  suspects, lineup accusation, stamps and day-card transitions, safe-area aware, 48 dp touch targets.
- **Accessibility**: three text sizes, high contrast (AAA), text speed, reduce motion, noir filter toggle — all
  applied live and saved.
- **Two LLM providers** behind one interface: Ollama (default, local, `qwen2.5:7b-instruct`) or Anthropic Claude.
- **Robust to the model's quirks**: repeated answers and answers with invented times are asked once more;
  character sheets are kept under a word budget; lies and clues are validated by tests.
- **Save / continue**, case records with your best ending, Android back button, Windows build that opens as a
  phone-shaped window.
- **Tooling** (Unity editor, batch mode): a bot that plays full games with the real game logic, clue and emotion
  calibrators, probes for leading questions and opener questions, screenshot and animation capture.
- **Tests**: 550+ EditMode and PlayMode tests (layout validation at several screen sizes, text sizes and
  high contrast, save compatibility, game flows).

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
