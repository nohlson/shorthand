#!/usr/bin/env bash
set -euo pipefail

# install.sh — manual installer to ~/.local for Shorthand
# - Installs bin/termgen and bin/shorthand-init into ~/.local/bin
# - Installs Modelfile and zsh/_coprompt.zsh into ~/.local/share/shorthand

prefix="${PREFIX:-$HOME/.local}"
bindir="$prefix/bin"
sharedir="$prefix/share/shorthand"

info() { printf "[install] %s\n" "$*"; }
die() { printf "[install] ERROR: %s\n" "$*" >&2; exit 1; }

here="$(cd "$(dirname -- "$0")"/.. && pwd)"

mkdir -p "$bindir" "$sharedir"

install -m 0555 "$here/bin/termgen" "$bindir/termgen"
install -m 0555 "$here/bin/shorthand-init" "$bindir/shorthand-init"
install -m 0444 "$here/Modelfile" "$sharedir/Modelfile"
install -m 0444 "$here/zsh/_coprompt.zsh" "$sharedir/_coprompt.zsh"

info "Installed to $prefix"
info "Add to PATH if needed: export PATH=\"$bindir:$PATH\""
info "One-time setup: shorthand-init"
info "Zsh widget: source \"$sharedir/_coprompt.zsh\" from your ~/.zshrc"


