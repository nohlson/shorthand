## Shorthand — Very lightweight local-only prompt-to-command CLI for macOS

Local-only CLI that turns natural-language prompts into safe commands using a tiny Ollama model.

### Install (Homebrew)

```sh
brew tap nohlson/tap
brew install nohlson/tap/shorthand
```

Then:

1) Install and start Ollama

```sh
brew install ollama && brew services start ollama
```

2) One-time model setup

```sh
shorthand-init
```

3) Use it

- In your terminal use Ctrl+G and type a prompt (e.g., "create file its_a_great_day.txt) and press enter.

### Manual install

0) Clone this repo

```sh
git clone https://github.com/nohlson/shorthand.git
cd shorthand
```

1) Install and start Ollama

```sh
brew install ollama && brew services start ollama
```

2) Install CLI and assets to `~/.local`

```sh
./bin/install.sh
```

3) One-time model setup (required before first use)

```sh
shorthand-init
```

### Safety

- Destructive operations require `--unsafe` prior to any other prompt; without;

```sh
echo "# refused: destructive without --unsafe"
```
