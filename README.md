## Shorthand — Local “Copilot for Terminal” (MVP)

Local-only CLI that turns natural-language prompts into one safe zsh command using a tiny Ollama model.

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

Ask for a command:

```sh
./bin/termgen "curl command to POST JSON to localhost:3000/api/users with body {\"name\":\"nate\"}"
```

Flags:
- `--model=<name>`: default `cmdgen`
- `--explain`: print a one-line `#` comment of the prompt before the command
- `--unsafe`: permits destructive output

Stdin works too:

```sh
echo "find all .js files under src" | ./bin/termgen
```

### Zsh Widget

Source `zsh/_coprompt.zsh` from your `~/.zshrc`:

```sh
source "$HOME/shorthand/zsh/_coprompt.zsh"
```

Press Ctrl+G, type a prompt, and the command is inserted into your buffer (not auto-run). Prefix your prompt with `--unsafe` to allow destructive commands.

### Safety

- The model is instructed to return exactly one command.
- Destructive operations require `--unsafe`; otherwise it emits:

```sh
echo "# refused: destructive without --unsafe"
```

### Testing (golden samples)

Run minimal snapshot tests:

```sh
node test/run.js
```

### Latency target

- Tiny prompts should complete in ~<=300ms on M-series for 3B models (after warmup).


Shorthand
