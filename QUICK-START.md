# 🚀 Quick Start Guide

## Prerequisites

- Unity 2022.3 LTS or higher
- Claude API key ([Get one here](https://console.anthropic.com/))

## Setup in 3 Steps

### 1️⃣ Clone & Open

```bash
git clone https://github.com/cristiannogueraescobar/detective-narrative-game.git
```

Open the project in Unity Hub.

### 2️⃣ Configure API Key

1. Open `Assets/Scripts/AIConversationManager_COMPLETO.cs`
2. Find the line with `YOUR_API_KEY`
3. Replace it with your actual Claude API key:

```csharp
private string apiKey = "sk-ant-api03-..."; // Your key here
```

### 3️⃣ Play

1. Open the main scene in `Assets/Scenes/`
2. Click Play ▶️ in Unity Editor
3. Select a case and start interrogating!

---

## Quick Tips

- **Internet Required**: The game needs internet for AI responses
- **API Costs**: Claude API has usage costs - check [Anthropic pricing](https://www.anthropic.com/pricing)
- **Case Selection**: Start with Case 1 for the simplest scenario

---

## Troubleshooting

**API Not Working?**
- Check your API key is correct
- Verify internet connection
- Check Anthropic API status

**Font Issues?**
- Reimport TextMeshPro fonts
- Check Font Asset settings

---

For detailed documentation, see [README.md](README.md)
