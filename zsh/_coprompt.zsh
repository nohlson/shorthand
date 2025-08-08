function _coprompt() {
  local q cmd unsafe_flag=""
  zle -I
  printf 'Generate a command: ' > /dev/tty
  IFS= read -r q < /dev/tty
  [[ -z "$q" ]] && return 0

  if [[ "$q" == --unsafe* ]]; then
    unsafe_flag="--unsafe"
    q="${q#--unsafe }"
  fi

  cmd="$("$HOME/shorthand/bin/termgen" $unsafe_flag --model=cmdgen -- "$q" 2>/dev/null)"
  [[ -z "$cmd" ]] && return 0
  LBUFFER+="$cmd"
  zle redisplay
}
zle -N _coprompt
bindkey '^G' _coprompt