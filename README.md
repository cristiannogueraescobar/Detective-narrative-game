# Detective Narrative Game

![Unity](https://img.shields.io/badge/Unity-6000.3-000000?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Claude AI](https://img.shields.io/badge/Claude_AI-Powered-7B68EE?style=for-the-badge)

> An AI-powered detective game featuring dynamic interrogations with Claude API integration

**⚠️ Work in Progress** - This game is currently under active development. See [Roadmap](#-roadmap) for planned features.

---

## 📖 About

A narrative-driven detective game where players investigate criminal cases by interrogating AI-powered characters. Each character has unique personality traits, knowledge bases, and behavior patterns that respond dynamically to player questions.

**Key Features:**
- 🎭 **9 Criminal Cases** - Diverse scenarios from theft to murder
- 🤖 **AI-Powered NPCs** - 7 unique character personalities powered by Claude API
- 💬 **Dynamic Interrogation System** - Natural language conversations with suspects
- 🔍 **Clue Discovery** - Keyword-based evidence revelation mechanic
- ⚖️ **Accusation System** - Make your case and accuse the guilty party

---

## 🎮 Gameplay

### Core Mechanics

1. **Case Selection**: Choose from 9 different criminal cases
2. **Character Selection**: Interrogate up to 7 different characters per case
3. **Investigation**: Ask questions in natural language to gather clues
4. **Clue Discovery**: Find specific keywords that unlock evidence
5. **Accusation**: Make your case and identify the perpetrator

### Character System

Each NPC has:
- **Unique Personality**: From nervous to aggressive behavior patterns
- **Dynamic Responses**: AI-generated answers based on character traits
- **Knowledge Base**: Case-specific information and clues
- **Emotional States**: Reactions change based on interrogation approach

---

## 🛠️ Technical Implementation

### Architecture

```
Game Architecture:
├── GameManager.cs (2,048 lines total)
│   └── Manages game state, case logic, and flow
├── AIConversationManager.cs
│   └── Case data, prompts, clue detection; talks to the LLM via ILLMProvider
├── InterrogationUI.cs
│   └── Manages UI, chat interface, and clue system
└── MenuManager.cs
    └── Main menu and navigation
```

### Key Technologies

- **Unity 6 (6000.3)** - Game engine
- **C#** - Programming language
- **Ollama / Claude API** - AI conversation system (swappable providers)
- **Async/Await** - HTTP communication
- **TextMeshPro** - UI text rendering
- **UnityWebRequest** - API calls

### API Integration

The game integrates with Anthropic's Claude API to generate dynamic NPC responses:

```csharp
// AI-powered character responses
await MakeAPICall(userInput, currentCase, selectedCharacter);
```

**Features:**
- Asynchronous API calls
- Character-specific prompts
- Context-aware responses
- Error handling and retry logic

---

## 🎯 Current Status

### ✅ Implemented Features

- [x] Full interrogation system with AI integration
- [x] 9 complete criminal case scenarios
- [x] 7 unique character personalities
- [x] Keyword-based clue discovery system
- [x] Dynamic accusation dropdown system
- [x] Main menu and case selection UI
- [x] Chat interface with message history
- [x] Google Fonts integration (TextMeshPro)
- [x] Event-driven architecture

### 📊 Code Statistics

- **~2,048 lines of C# code**
- **4 core scripts**
- **9 criminal cases**
- **7 character archetypes**
- **Deterministic clue system**

---

## 🚧 Roadmap

### High Priority (In Development)

- [ ] **Audio System**
  - Background music
  - Sound effects for UI interactions
  - Ambient sounds for atmosphere

- [ ] **Voice Input**
  - Speech-to-text integration
  - Microphone input for questions
  - Voice recording UI

- [ ] **Settings Menu**
  - Volume controls
  - Graphics options
  - Language preferences

### Medium Priority (Planned)

- [ ] **Enhanced Storytelling**
  - More complex case narratives
  - Multiple interrogation paths
  - Consequence system for accusations

- [ ] **Visual Improvements**
  - Character portraits
  - Evidence photos
  - Crime scene backgrounds

- [ ] **Save System**
  - Progress persistence
  - Case history
  - Achievement tracking

### Low Priority (Future)

- [ ] **Multiplayer Mode**
  - Cooperative investigations
  - Competitive detective challenges

- [ ] **Mobile Version**
  - iOS build optimization
  - Touch controls
  - Responsive UI

---

## 🎨 Game Design

### Case Structure

Each case includes:
- **Scenario**: Crime description and context
- **Characters**: List of suspects and witnesses
- **Evidence**: Clues hidden in conversations
- **Solution**: The guilty party and motive

### Character Personalities

1. **Nervous** - Fidgety, hesitant, reveals information under pressure
2. **Aggressive** - Confrontational, defensive, hides guilt
3. **Calm** - Composed, logical, provides measured responses
4. **Evasive** - Vague, changes subjects, avoids direct answers
5. **Cooperative** - Helpful, detailed, possibly too helpful
6. **Arrogant** - Condescending, believes they're untouchable
7. **Scared** - Frightened, confused, potentially innocent

---

## 🚀 Getting Started

### Prerequisites

- Unity 6000.3.2f1
- An LLM backend, either:
  - [Ollama](https://ollama.com/) running locally (default), or
  - A Claude API key from Anthropic

### Installation

1. Clone the repository:
```bash
git clone https://github.com/cristiannogueraescobar/detective-narrative-game.git
```

2. Open the project in Unity and open `Assets/Scenes/Game.unity`

3. Choose a provider on the `AIConversationManager` component (Inspector → **Provider**)

4. Press Play

### LLM Providers

The game talks to the LLM through `ILLMProvider` (`Assets/Scripts/LLM/`). Switching provider only requires changing the **Provider** field in the Inspector.

**Ollama (default)**

```bash
ollama pull qwen2.5:7b-instruct
ollama serve
```

URL, model, timeout (180 s) and `keep_alive` (60m) are configurable under **Ollama Settings**. The model is preloaded in the background when the scene starts, so it is usually ready by the time the player asks the first question. Use `"-1m"` for `keep_alive` to keep it loaded indefinitely.

**Anthropic**

The API key is never stored in the scene. It is read from, in order:
1. The `ANTHROPIC_API_KEY` environment variable
2. An `anthropic_api_key.txt` file in the project root (next to the `.exe` in a build). This file is in `.gitignore`.

Get a key from the [Anthropic Console](https://console.anthropic.com/). Model and timeout are configurable under **Anthropic Settings**.

If a request fails (backend not running, timeout, invalid key...), a warning is shown in the chat and the question is not counted against the day's limit.

---

## 🎓 Code Examples

### Making an AI Call

```csharp
private async Task MakeAPICall(string userInput, int caseIndex, int characterIndex)
{
    var requestBody = new
    {
        model = "claude-sonnet-4-20250514",
        max_tokens = 1000,
        messages = new[]
        {
            new { role = "user", content = BuildPrompt(userInput, caseIndex, characterIndex) }
        }
    };
    
    // Send request and process response
    await SendAPIRequest(requestBody);
}
```

### Clue Discovery System

```csharp
private void CheckForClues(string aiResponse)
{
    foreach (var clue in currentCase.clues)
    {
        if (!discoveredClues.Contains(clue.keyword) && 
            aiResponse.Contains(clue.keyword))
        {
            discoveredClues.Add(clue.keyword);
            DisplayClue(clue);
        }
    }
}
```

---

## 📝 Development Notes

### Language

- **Game Content**: Spanish (UI, cases, characters)
- **Code**: English (comments, variable names)
- **Documentation**: English

This approach makes the codebase accessible to international developers while maintaining the original Spanish narrative content.

### Project Structure

```
Assets/
├── Scenes/              # Unity scenes
├── Scripts/             # C# game logic
├── Backgrounds/         # Visual assets
├── Fonts/              # TextMeshPro fonts
├── Images/             # Sprites and icons
├── UI_Images/          # UI elements
├── Prefabs/            # Reusable game objects
└── Settings/           # Render pipeline settings
```

---

## 🐛 Known Issues

- [ ] iOS deployment requires additional configuration
- [ ] Some special characters may not render correctly in TextMeshPro
- [ ] API rate limiting not yet implemented
- [ ] No offline mode (requires internet for AI responses)

---

## 🤝 Contributing

This is a personal project for educational purposes, but suggestions and feedback are welcome!

If you'd like to contribute:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

---

## 📚 Learning Resources

This project demonstrates:
- Async/await patterns in Unity
- REST API integration
- Event-driven architecture
- Dynamic UI generation
- AI prompt engineering

Useful for students learning:
- Unity game development
- C# programming
- API integration
- Game design
- AI implementation

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
