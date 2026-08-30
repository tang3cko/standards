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
# Exit codes: 0 = build succeeded, 1 = failure or setup error.

set -uo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib_resolve_unity.sh
source "${SCRIPT_DIR}/lib_resolve_unity.sh"

PROJECT_PATH="${1:?usage: build.sh <project_path> <execute_method>}"
EXECUTE_METHOD="${2:?usage: build.sh <project_path> <execute_method>}"

if check_editor_running "$PROJECT_PATH"; then
  echo "error: Unity Editor appears to already be running for ${PROJECT_PATH}." >&2
  echo "       -batchmode requires the project to be closed first (project lock)." >&2
  echo "       See references/pitfalls.md." >&2
  exit 1
fi

UNITY_EXE="$(resolve_unity_executable "$PROJECT_PATH")" || exit 1

LOG_FILE="${PROJECT_PATH}/build.log"

echo "Running: ${UNITY_EXE} -batchmode -nographics -quit -projectPath ${PROJECT_PATH} -executeMethod ${EXECUTE_METHOD} -logFile ${LOG_FILE}"
"$UNITY_EXE" -batchmode -nographics -quit -projectPath "$PROJECT_PATH" \
  -executeMethod "$EXECUTE_METHOD" -logFile "$LOG_FILE"
EXIT_CODE=$?

if [ $EXIT_CODE -ne 0 ]; then
  echo "error: build exited ${EXIT_CODE}; last 40 lines of ${LOG_FILE}:" >&2
  tail -n 40 "$LOG_FILE" >&2 2>/dev/null || true
  exit $EXIT_CODE
fi

echo "build finished (exit 0). This confirms the process exited cleanly, not that BuildPipeline.BuildPlayer succeeded -"
echo "the entry point must call EditorApplication.Exit(1) on report.summary.result != Succeeded (see the template)."
echo "Verify the artifact next - see references/verification.md."
