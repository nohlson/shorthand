function _coprompt() {
  emulate -L zsh
  setopt localtraps

  local q cmd unsafe_flag="" termgen_bin _cmd_pid _out_tmp

  zle -I
  BUFFER=""
  CURSOR=0

  # Save and restore TTY state in case of interrupts
  local _stty_state
  _stty_state=$(stty -g </dev/tty 2>/dev/null)
  _restore_tty() { [[ -n "$_stty_state" ]] && stty "$_stty_state" </dev/tty >/dev/null 2>&1 }

  TRAPINT() { typeset -g __sh_abort=1; _restore_tty; zle -I; zle redisplay; return 0 }
  TRAPQUIT() { typeset -g __sh_abort=1; _restore_tty; zle -I; zle redisplay; return 0 }
  TRAPTERM() { typeset -g __sh_abort=1; _restore_tty; zle -I; zle redisplay; return 0 }

  # Read the prompt from /dev/tty in a subshell so Ctrl+C cancels cleanly
  q=$({
    printf 'Generate a command: ' > /dev/tty
    IFS= read -r line </dev/tty || exit $?
    print -r -- "$line"
  } 2>/dev/null) || { _restore_tty; return 0 }

  _restore_tty

  [[ -z "$q" ]] && { zle -I; zle redisplay; return 0 }

  if [[ "$q" == --unsafe* ]]; then
    unsafe_flag="--unsafe"
    q="${q#--unsafe }"
  fi

  # Resolve termgen from PATH first, then relative to this file, then fallback to $HOME
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

  # Run generator in background so Ctrl+C can cancel promptly
  _out_tmp=$(mktemp -t shorthand_cmd_XXXXXX 2>/dev/null) || { zle -I; zle redisplay; return 1 }
  {
    "$termgen_bin" $unsafe_flag --model=cmdgen -- "$q" 2>/dev/null >| "$_out_tmp" &
    _cmd_pid=$!
    # Update traps to also terminate the generator if interrupted
    TRAPINT()  { typeset -g __sh_abort=1; [[ -n "$_cmd_pid" ]] && kill -TERM "$_cmd_pid" 2>/dev/null; wait "$_cmd_pid" 2>/dev/null; _restore_tty; zle -I; zle redisplay; rm -f "$_out_tmp"; return 0 }
    TRAPQUIT() { typeset -g __sh_abort=1; [[ -n "$_cmd_pid" ]] && kill -TERM "$_cmd_pid" 2>/dev/null; wait "$_cmd_pid" 2>/dev/null; _restore_tty; zle -I; zle redisplay; rm -f "$_out_tmp"; return 0 }
    TRAPTERM() { typeset -g __sh_abort=1; [[ -n "$_cmd_pid" ]] && kill -TERM "$_cmd_pid" 2>/dev/null; wait "$_cmd_pid" 2>/dev/null; _restore_tty; zle -I; zle redisplay; rm -f "$_out_tmp"; return 0 }
    wait "$_cmd_pid" 2>/dev/null
  }

  if [[ ${__sh_abort:-0} -eq 1 ]]; then
    unset __sh_abort
    zle -I; zle redisplay
    return 0
  fi

  if [[ -r "$_out_tmp" ]]; then
    cmd="$(<"$_out_tmp")"
    rm -f "$_out_tmp"
  else
    cmd=""
  fi
  [[ -z "$cmd" ]] && { zle -I; zle redisplay; return 0 }

  BUFFER="$cmd"
  CURSOR=${#BUFFER}
  zle redisplay
}
zle -N _coprompt
bindkey '^G' _coprompt