## Shorthand — Very lightweight local-only prompt-to-command CLI for macOS

Local-only CLI that turns natural-language prompts into safe commands using a tiny Ollama model.

### Install (manual)

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
chmod +x bin/termgen bin/shorthand-init
```

Or use the manual installer to install to `~/.local` (bin/share), then run the initializer:

```sh
./bin/install.sh
shorthand-init
```

### Install from tap (Homebrew)

```sh
brew tap nohlson/tap
brew install nohlson/tap/shorthand
```

After install, run the initializer once to set up the model:

```sh
shorthand-init
```

The `Modelfile` is installed to a shared location, and `shorthand-init` will find it automatically. Re-run with `SHORTHAND_MODELDIR=/path` to point to a custom `Modelfile`.

### Usage

### Zsh Widget

Source the Zsh widget from your `~/.zshrc`:

```sh
source "$HOME/shorthand/zsh/_coprompt.zsh"

If installed via Homebrew, you can also source the file from the opt path:

```sh
source "$(brew --prefix)/opt/shorthand/share/shorthand/_coprompt.zsh"
```
```

Press Ctrl+G, type a prompt, and the command is inserted into your buffer (not auto-run). Prefix your prompt with `--unsafe` to allow destructive commands.

### Safety

- Destructive operations require `--unsafe`; otherwise it emits:

```sh
echo "# refused: destructive without --unsafe"
```
