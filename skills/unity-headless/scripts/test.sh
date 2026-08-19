#!/usr/bin/env bash
# Run Unity EditMode (or PlayMode) tests headlessly against a Unity project,
# then summarize the NUnit result XML.
#
# Usage:
#   test.sh <project_path> [test_filter] [test_platform]
#
# Examples:
#   test.sh ~/Workspace/MyGame
#   test.sh ~/Workspace/MyGame "MyNamespace.MyTests"
#   test.sh ~/Workspace/MyGame "" PlayMode
#
# test_filter is passed straight through to Unity's -testFilter (semicolon-
# separated names, regex, or "!Name" to exclude - see Unity Test Framework docs).
# test_platform defaults to EditMode.
#
# Optional test-count ratchet, off unless TEST_COUNT_FLOOR_DIR is set:
#   TEST_COUNT_FLOOR_DIR=<dir>   keep a per-platform, per-filter high-water mark
#                                of the test count there, and fail a later run
#                                that collects fewer. Point it at a gitignored
#                                directory - the floor is local observation, not
#                                a fact about the repository
#   ESTABLISH_BASELINE=1         record the current count as the floor, in both
#                                directions. The only way to shrink a suite on
#                                purpose, and deliberately a separate gesture
#                                from running the tests
#
# Exit codes: 0 = all tests passed, 1 = failures, setup error, or crash,
#             2 = the run collected no tests at all,
#             3 = the suite shrank below its floor, or the ratchet was asked for
#                 with no floor established yet (see summarize_test_results.py).

set -uo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib_resolve_unity.sh
source "${SCRIPT_DIR}/lib_resolve_unity.sh"

PROJECT_PATH="${1:?usage: test.sh <project_path> [test_filter] [test_platform]}"
TEST_FILTER="${2:-}"
TEST_PLATFORM="${3:-EditMode}"

if check_editor_running "$PROJECT_PATH"; then
  echo "error: Unity Editor appears to already be running for ${PROJECT_PATH}." >&2
  echo "       -batchmode requires the project to be closed first (project lock)." >&2
  echo "       See references/pitfalls.md." >&2
  exit 1
fi

UNITY_EXE="$(resolve_unity_executable "$PROJECT_PATH")" || exit 1

RESULTS_XML="${PROJECT_PATH}/TestResults.xml"
LOG_FILE="${PROJECT_PATH}/test.log"
rm -f "$RESULTS_XML"

# No -quit here, deliberately. Combined with -runTests the editor quits before
# the run produces anything: measured 2026-08-19 on Action_006 with Unity
# 6000.3.11f1, back to back on the same project - without -quit, 445/445 and a
# result XML; with -quit, "Exiting batchmode successfully now!" and no XML at
# all. -runTests exits on its own once the run finishes, so -quit buys nothing.
# Unity's own docs do not mention -quit on the run-tests-from-command-line page;
# this is field measurement, first seen in Puzzle_003 and recorded in
# Multiplayer_012's ADR-0002, now reproduced on a second project.
ARGS=(-batchmode -nographics -projectPath "$PROJECT_PATH" -runTests
      -testPlatform "$TEST_PLATFORM" -testResults "$RESULTS_XML" -logFile "$LOG_FILE")
[ -n "$TEST_FILTER" ] && ARGS+=(-testFilter "$TEST_FILTER")

echo "Running: ${UNITY_EXE} ${ARGS[*]}"
"$UNITY_EXE" "${ARGS[@]}"
# Unity's own exit code is not authoritative for pass/fail (it can exit 0 with
# failing tests, or nonzero for unrelated licensing noise) - the result XML
# below is the source of truth. Deliberately not checking $? here.

if [ ! -f "$RESULTS_XML" ]; then
  echo "error: no result XML produced - Unity likely crashed or failed to start." >&2
  echo "       last 40 lines of ${LOG_FILE}:" >&2
  tail -n 40 "$LOG_FILE" >&2 2>/dev/null || true
  exit 1
fi

SUMMARIZE_ARGS=("$RESULTS_XML")
if [ -n "${TEST_COUNT_FLOOR_DIR:-}" ]; then
  # One floor per platform and filter. A filtered run is a subset, and comparing
  # a subset against the whole suite's high-water mark would fail every time -
  # so the filter is part of the identity of the number, not a detail about it.
  FILTER_KEY="$(printf '%s' "${TEST_FILTER:-all}" | tr -c 'A-Za-z0-9._-' '_')"
  SUMMARIZE_ARGS+=(--floor-file "${TEST_COUNT_FLOOR_DIR}/test-count-high-water-${TEST_PLATFORM}-${FILTER_KEY}")
  [ "${ESTABLISH_BASELINE:-}" = "1" ] && SUMMARIZE_ARGS+=(--establish-baseline)
fi

python3 "${SCRIPT_DIR}/summarize_test_results.py" "${SUMMARIZE_ARGS[@]}"
