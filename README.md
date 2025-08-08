## Shorthand — Very lightweight local only prompt-to-command CLI for macos

Local-only CLI that turns natural-language prompts into safe commands using a tiny Ollama model.

### Install

1) Install and run Ollama

```sh
brew install ollama
brew services start ollama
```

2) Build the small local model

```sh
ollama create cmdgen -f Modelfile
```

Optional: pull base model first (done automatically if missing):

```sh
ollama pull llama3.2:3b
```

3) Make CLI executable

```sh
chmod +x bin/termgen
```

### Usage

### Zsh Widget

Source `zsh/_coprompt.zsh` from your `~/.zshrc`:

```sh
source "$HOME/shorthand/zsh/_coprompt.zsh"
```

Press Ctrl+G, type a prompt, and the command is inserted into your buffer (not auto-run). Prefix your prompt with `--unsafe` to allow destructive commands.

### Safety

- Destructive operations require `--unsafe`; otherwise it emits:

```sh
echo "# refused: destructive without --unsafe"
```
