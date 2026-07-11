#!/usr/bin/env bash
# Shared helpers for headless Unity invocation. Sourced by test.sh and build.sh,
# not run directly.
#
# resolve_unity_executable <project_path>
#   Reads ProjectSettings/ProjectVersion.txt and echoes the matching Unity Hub
#   executable path for the current OS. Returns nonzero (and prints to stderr)
#   if the version can't be read or the executable isn't installed.
#
# check_editor_running <project_path>
#   Best-effort check for a Unity process already holding this project (the
#   project lock that makes -batchmode fail). Returns 0 if one appears to be
#   running, 1 otherwise. Best-effort: absence of a match does not guarantee
#   the project is unlocked (e.g. a just-closed Editor may still hold the OS
#   file lock briefly).

resolve_unity_executable() {
  local project_path="$1"
  local version_file="${project_path}/ProjectSettings/ProjectVersion.txt"

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

check_editor_running() {
  local project_path="$1"
  local project_name
  project_name="$(basename "$project_path")"

  if command -v pgrep >/dev/null 2>&1; then
    pgrep -f "Unity.*-projectPath.*${project_name}" >/dev/null 2>&1
    return $?
  fi

  # pgrep unavailable (rare) - fall back to a plain ps scan.
  ps aux 2>/dev/null | grep -i "Unity.*-projectPath.*${project_name}" | grep -v grep >/dev/null 2>&1
}
