#!/usr/bin/env bash
# Verify a Unity Player.log shows a clean startup (engine initialized, no
# fatal error/uncaught exception logged).
#
# Usage:
#   verify_player_log.sh <player_log_path>
#
# Player.log's location is platform- and PlayerSettings-dependent (company/
# product name) - see references/verification.md for the default path per OS.
# That lookup is left to the caller since it can't be generalized here.
#
# Exit codes: 0 = clean startup, 1 = missing log or fatal error found.

set -uo pipefail

LOG_PATH="${1:?usage: verify_player_log.sh <player_log_path>}"

if [ ! -f "$LOG_PATH" ]; then
  echo "error: ${LOG_PATH} not found - the player may not have started at all" >&2
  exit 1
fi

if ! grep -q "Initialize engine version" "$LOG_PATH"; then
  echo "error: no 'Initialize engine version' line in ${LOG_PATH} - player did not reach engine init" >&2
  exit 1
fi

if grep -qiE "Fatal Error|Unhandled Exception" "$LOG_PATH"; then
  echo "error: fatal error found in ${LOG_PATH}:" >&2
  grep -iE "Fatal Error|Unhandled Exception" "$LOG_PATH" >&2
  exit 1
fi

echo "player started cleanly: ${LOG_PATH}"
