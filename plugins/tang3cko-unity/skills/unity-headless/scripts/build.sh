#!/usr/bin/env bash
# Run a Unity player build headlessly against a Unity project via
# -batchmode -executeMethod.
#
# Usage:
#   build.sh <project_path> <execute_method>
#
# Example:
#   build.sh ~/Workspace/MyGame BuildScript.BuildDevelopment
#
# Requires the target project to already have a matching Editor-only entry
# point (static method with no arguments). See ../assets/BuildScript.cs.template
# for a starting point - copy it into <project>/Assets/Editor/ and fill in the
# TODOs (scene list, output path/target); those are project-specific and
# cannot be generalized into this script.
#
# Serialization: the run refuses to start while an Editor has the project open,
# and takes a per-project lock so two headless runs never race for the project
# (lib_batchmode_lock.sh; UNITY_LOCK_WAIT and friends tune it). A watchdog kills
# the Editor if it is still running after UNITY_BUILD_TIMEOUT seconds (default
# 3600); it kills only that pid - see lib_batchmode_lock.sh for what may be
# left behind.
#
# Exit codes: 0 = Unity exited cleanly (see the caveat printed at the end),
#             2 = the build never ran: Editor open, lock wait exhausted, or
#                 watchdog kill,
#             anything else = Unity's own nonzero exit, or a setup error (1).

set -uo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib_resolve_unity.sh
source "${SCRIPT_DIR}/lib_resolve_unity.sh"
# shellcheck source=lib_batchmode_lock.sh
source "${SCRIPT_DIR}/lib_batchmode_lock.sh"

PROJECT_PATH="${1:?usage: build.sh <project_path> <execute_method>}"
EXECUTE_METHOD="${2:?usage: build.sh <project_path> <execute_method>}"
TIMEOUT="${UNITY_BUILD_TIMEOUT:-3600}"

# Absolute, because the lock is keyed on the path and the open-Editor check
# compares it against what the Hub put in the Editor's argv.
PROJECT_PATH="$(cd "$PROJECT_PATH" 2>/dev/null && pwd)" || {
  echo "error: project path not found: ${1}" >&2; exit 1; }

UNITY_EXE="$(resolve_unity_executable "$PROJECT_PATH")" || exit 1

# Same directory test.sh writes to, so a project gitignores one path for all
# headless output instead of a stray build.log at its root.
mkdir -p "${PROJECT_PATH}/TestResults"
LOG_FILE="${PROJECT_PATH}/TestResults/build.log"

# -quit stays here: nothing else ends an -executeMethod run (unlike -runTests,
# which exits on its own). See references/testing.md step 3 for the measurement.
ARGS=(-batchmode -nographics -quit -projectPath "$PROJECT_PATH"
      -executeMethod "$EXECUTE_METHOD" -logFile "$LOG_FILE")

# Trap first, then take the lock: a signal in the gap between the two would
# leak the lock directory. release is idempotent, so arming it early is free.
trap 'release_batchmode_lock' EXIT INT TERM HUP
acquire_batchmode_lock "$PROJECT_PATH"

echo "Running: ${UNITY_EXE} ${ARGS[*]}"
run_unity_watchdogged "$TIMEOUT" "$UNITY_EXE" "${ARGS[@]}"
EXIT_CODE=$?

release_batchmode_lock
trap - EXIT INT TERM HUP

if [ "$TIMED_OUT" = 1 ]; then
  echo "error: the watchdog killed the Editor at the ${TIMEOUT}s ceiling (UNITY_BUILD_TIMEOUT); the build never finished." >&2
  echo "       Unity helper processes may remain; check with: ps -Ao pid,ppid,args | grep -i '[U]nity'" >&2
  echo "       last 40 lines of ${LOG_FILE}:" >&2
  tail -n 40 "$LOG_FILE" >&2 2>/dev/null || true
  exit 2
fi

if [ $EXIT_CODE -ne 0 ]; then
  echo "error: build exited ${EXIT_CODE}; last 40 lines of ${LOG_FILE}:" >&2
  tail -n 40 "$LOG_FILE" >&2 2>/dev/null || true
  exit $EXIT_CODE
fi

echo "build finished (exit 0). This confirms the process exited cleanly, not that BuildPipeline.BuildPlayer succeeded -"
echo "the entry point must call EditorApplication.Exit(1) on report.summary.result != Succeeded (see the template)."
echo "Verify the artifact next - see references/verification.md."
