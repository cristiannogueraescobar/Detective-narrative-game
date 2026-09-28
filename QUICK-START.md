# 🚀 Quick Start Guide

## Prerequisites

- Unity 6000.3.2f1
- [Ollama](https://ollama.com/) (default) **or** a Claude API key ([get one here](https://console.anthropic.com/))

## Setup in 3 Steps

### 1️⃣ Clone & Open

```bash
git clone https://github.com/cristiannogueraescobar/detective-narrative-game.git
```

Open the project in Unity Hub and open `Assets/Scenes/Game.unity`.

### 2️⃣ Configure the LLM provider

Select the `AIConversationManager` object and pick a **Provider** in the Inspector.

**Ollama (default):**

```bash
ollama pull qwen2.5:7b-instruct
ollama serve
```

**Anthropic:** set the `ANTHROPIC_API_KEY` environment variable, or create `anthropic_api_key.txt` in the project root containing only the key. That file is git-ignored. Never put the key in the scene.

### 3️⃣ Play

1. Click Play ▶️ in Unity Editor
2. Start the case and begin interrogating!

---

## Quick Tips

- **First question is slow with Ollama**: the model is loaded into memory on the first request
- **API Costs**: Claude API has usage costs - check [Anthropic pricing](https://www.anthropic.com/pricing)
- **Failed requests don't cost a question**: if the backend is unreachable you'll see a warning in the chat and can retry

---

## Troubleshooting

**"No se pudo conectar con Ollama"**
- Run `ollama serve` and check the URL under *Ollama Settings*

**"El modelo '...' no está descargado"**
- Run `ollama pull <model>`

**"Falta la API key de Anthropic" / "La API key de Anthropic no es válida"**
- Check `ANTHROPIC_API_KEY` or `anthropic_api_key.txt` in the project root

**Font Issues?**
- Reimport TextMeshPro fonts
- Check Font Asset settings

---

For detailed documentation, see [README.md](README.md)
