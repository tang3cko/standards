#!/usr/bin/env bash
# Headless test runner for a godot-mono project's two dotnet test layers:
# xUnit link-referenced pure C# (Tests/) and GdUnit4Net SceneRunner E2E (Tests.E2E/).
#
# Usage:
#   run-tests.sh <project_path> [unit|e2e|all]
#
# Examples:
#   run-tests.sh ~/Workspace/MyGame
#   run-tests.sh ~/Workspace/MyGame unit
#   run-tests.sh ~/Workspace/MyGame e2e
#
# Assumes the target project layout: <project>/Tests/Tests.csproj (xUnit, no Godot
# runtime) and <project>/Tests.E2E/Tests.E2E.csproj (GdUnit4Net, spawns godot-mono
# per GODOT_BIN in Tests.E2E/.runsettings if present).
#
# NOTE: this script itself lives in a skill's scripts/ directory, not inside any
# consuming project. When copying the *pattern* into a project (rather than
# invoking this script directly), place it under Tools/ or another name distinct
# from Scripts/ - macOS's default case-insensitive filesystem merges a lowercase
# "scripts/" into an existing "Scripts/" (C# source) directory. See
# references/pitfalls.md and godot-core's references/gotchas.md.
#
# Exit codes: 0 = all requested layers passed, 1 = failures or a layer's csproj
# was not found.

set -uo pipefail

PROJECT="${1:?usage: run-tests.sh <project_path> [unit|e2e|all]}"
LAYER="${2:-all}"

UNIT_CSPROJ="$PROJECT/Tests/Tests.csproj"
E2E_CSPROJ="$PROJECT/Tests.E2E/Tests.E2E.csproj"
E2E_SETTINGS="$PROJECT/Tests.E2E/.runsettings"

run_unit() {
  echo "== Tests (xUnit, no Godot runtime) =="
  if [ ! -f "$UNIT_CSPROJ" ]; then
    echo "error: $UNIT_CSPROJ not found" >&2
    return 1
  fi
  dotnet test "$UNIT_CSPROJ"
}

run_e2e() {
  echo "== Tests.E2E (GdUnit4Net, headless Godot) =="
  if [ ! -f "$E2E_CSPROJ" ]; then
    echo "error: $E2E_CSPROJ not found" >&2
    return 1
  fi
  if [ -f "$E2E_SETTINGS" ]; then
    dotnet test "$E2E_CSPROJ" --settings "$E2E_SETTINGS"
  else
    echo "warning: $E2E_SETTINGS not found - running without explicit GODOT_BIN/MaxCpuCount settings" >&2
    dotnet test "$E2E_CSPROJ"
  fi
}

case "$LAYER" in
  unit) run_unit; exit $? ;;
  e2e) run_e2e; exit $? ;;
  all)
    run_unit; UNIT_EXIT=$?
    run_e2e; E2E_EXIT=$?
    if [[ $UNIT_EXIT -ne 0 || $E2E_EXIT -ne 0 ]]; then
      echo "FAIL (unit=$UNIT_EXIT, e2e=$E2E_EXIT)" >&2
      exit 1
    fi
    echo "PASS"
    ;;
  *)
    echo "Usage: $0 <project_path> [unit|e2e|all]" >&2
    exit 64
    ;;
esac
