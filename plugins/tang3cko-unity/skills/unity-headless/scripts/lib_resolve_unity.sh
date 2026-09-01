#!/usr/bin/env bash
# Shared helpers for headless Unity invocation. Sourced by test.sh and build.sh,
# not run directly.
#
# resolve_unity_executable <project_path>
#   Reads ProjectSettings/ProjectVersion.txt and echoes the matching Unity Hub
#   executable path for the current OS. Returns nonzero (and prints to stderr)
#   if the version can't be read or the executable isn't installed.
#   UNITY_PATH, when set, overrides the lookup (a non-Hub install, or a stand-in
#   executable for scripts/selftest_batchmode.sh).
#
# project_is_open <project_path>
#   Returns 0 if a Unity Editor process currently has this project open (the
#   project lock that makes -batchmode fail), 1 otherwise. Best-effort: a
#   just-closed Editor may still hold the OS file lock briefly after its
#   process is gone.

resolve_unity_executable() {
  local project_path="$1"
  local version_file="${project_path}/ProjectSettings/ProjectVersion.txt"

  if [ -n "${UNITY_PATH:-}" ]; then
    if [ ! -x "$UNITY_PATH" ]; then
      echo "error: UNITY_PATH is set but not executable: ${UNITY_PATH}" >&2
      return 1
    fi
    echo "$UNITY_PATH"
    return 0
  fi

  if [ ! -f "$version_file" ]; then
    echo "error: ${version_file} not found - is ${project_path} a Unity project?" >&2
    return 1
  fi

  local version
  version=$(grep 'm_EditorVersion:' "$version_file" | awk '{print $2}')
  if [ -z "$version" ]; then
    echo "error: could not parse m_EditorVersion from ${version_file}" >&2
    return 1
  fi

  local exe
  case "$(uname -s)" in
    Darwin)
      exe="/Applications/Unity/Hub/Editor/${version}/Unity.app/Contents/MacOS/Unity"
      ;;
    Linux)
      exe="${HOME}/Unity/Hub/Editor/${version}/Editor/Unity"
      ;;
    MINGW*|MSYS*|CYGWIN*)
      exe="C:/Program Files/Unity/Hub/Editor/${version}/Editor/Unity.exe"
      ;;
    *)
      echo "error: unsupported OS $(uname -s)" >&2
      return 1
      ;;
  esac

  if [ ! -x "$exe" ] && [ ! -f "$exe" ]; then
    echo "error: Unity ${version} not found at ${exe} (install it via Unity Hub)" >&2
    return 1
  fi

  echo "$exe"
}

# Pids whose executable is a Unity Editor. Matched on the executable (ps comm),
# never on the whole command line: `pgrep -f 'Unity.*-projectPath'` also hits
# Unity Hub, which keeps `-projectPath <project>` in its own argv after
# launching an Editor and stays resident after that Editor closes - so the old
# check reported a lock that was no longer held (Puzzle_003, 2026-07-28). It
# also hits any wrapper, shell, or this very script that merely names the
# editor in its arguments.
editor_pids() {
  ps -Ao pid=,comm= 2>/dev/null | awk '
    /\/Unity\.app\/Contents\/MacOS\/Unity$/ { print $1; next }   # macOS Hub install
    /\/Editor\/Unity$/                       { print $1; next }   # Linux Hub install
    /\/Editor\/Unity\.exe$/                  { print $1 }'        # Windows (MSYS ps)
}

# Only the confirmed Editor pids have their arguments read. The flag is matched
# case-insensitively: the Hub launches with `-projectpath`, batchmode runs pass
# `-projectPath`, and matching only the documented spelling misses every Editor
# a human opened. A separator is required after the path so a sibling directory
# whose name merely starts with this project's path does not count.
project_is_open() {
  local project_path="$1" pid
  for pid in $(editor_pids); do
    if ps -p "$pid" -o args= 2>/dev/null | UNITY_PROJECT="$project_path" awk '
      BEGIN { want = ENVIRON["UNITY_PROJECT"]; flag = "-projectpath " }
      {
        p = index(tolower($0), flag)
        if (p == 0) next
        rest = substr($0, p + length(flag))
        if (index(rest, want) == 1) {
          tail = substr(rest, length(want) + 1)
          if (tail == "" || substr(tail, 1, 1) == " ") found = 1
        }
      }
      END { exit found ? 0 : 1 }'; then
      return 0
    fi
  done
  return 1
}
