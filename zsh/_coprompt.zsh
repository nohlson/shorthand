function _coprompt() {
  local q cmd unsafe_flag=""
  zle -I
  BUFFER=""
  CURSOR=0
  printf 'Generate a command: ' > /dev/tty
  IFS= read -r q < /dev/tty
  [[ -z "$q" ]] && return 0

  if [[ "$q" == --unsafe* ]]; then
    unsafe_flag="--unsafe"
    q="${q#--unsafe }"
  fi

  # Resolve termgen from PATH first, then relative to this file, then fallback to $HOME
  local termgen_bin
  termgen_bin="${TERMGEN_BIN:-$(command -v termgen 2>/dev/null)}"
  if [[ -z "$termgen_bin" ]]; then
    local script_dir
    script_dir="${${(%):-%N}:A:h}"
    if [[ -x "$script_dir/../bin/termgen" ]]; then
      termgen_bin="$script_dir/../bin/termgen"
    elif [[ -x "$HOME/shorthand/bin/termgen" ]]; then
      termgen_bin="$HOME/shorthand/bin/termgen"
    else
      print -u2 "shorthand: termgen not found in PATH; set TERMGEN_BIN or install via Homebrew"
      return 1
    fi
  fi

  cmd="$($termgen_bin $unsafe_flag --model=cmdgen -- "$q" 2>/dev/null)"
  [[ -z "$cmd" ]] && return 0
  BUFFER="$cmd"
  CURSOR=${#BUFFER}
  zle redisplay
}
zle -N _coprompt
bindkey '^G' _coprompt