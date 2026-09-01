#!/usr/bin/env bash
# Self-test for the serialization layer of test.sh / build.sh: open-Editor
# detection, the per-project lock, stale-lock reclaim, and the watchdog. No
# Unity is involved - a stand-in executable is injected through UNITY_PATH and
# a stand-in Editor through `exec -a`, so this runs anywhere bash and ps exist.
#
# Usage:
#   selftest_batchmode.sh            # prints one PASS/FAIL line per case
#
# Exit 0 when every case passed, 1 otherwise. The elapsed-time knobs are set
# to their minimum, so the whole thing takes on the order of 20 seconds; the
# hang cases end when the watchdog fires, and the wait cases end when the lock
# wait is exhausted - both counted conditions, not sleeps of a hoped-for length.
#
# What this does NOT verify: that a real Unity Editor's comm/argv look the way
# editor_pids/project_is_open expect (they were taken from ps output of real
# Editors on macOS - see lib_resolve_unity.sh), or that a SIGKILLed real Editor
# leaves the project usable. Those need an actual Unity install.

set -uo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Normalized through pwd: a TMPDIR with a trailing slash makes mktemp return
# "T//name", and the scripts compare the project path as pwd prints it against
# the Editor's argv byte for byte.
WORK="$(mktemp -d "${TMPDIR:-/tmp}/unity-headless-selftest.XXXXXX")"
WORK="$(cd "$WORK" && pwd)"
PROJECT="$WORK/Proj"
FAKE_APP="$WORK/Fake/Unity.app/Contents/MacOS"
FAKE_UNITY="$FAKE_APP/Unity"
LOCKDIR="$WORK/batchmode.lock"
PASSED=0
FAILED=0
BG_PIDS=()

cleanup() {
  local p
  for p in "${BG_PIDS[@]:-}"; do
    [ -n "$p" ] || continue
    pkill -P "$p" 2>/dev/null; kill "$p" 2>/dev/null
  done
  # The hang stand-in forks a child sleep that the watchdog does not reach
  # (by design - see lib_batchmode_lock.sh). Reap what this harness spawned,
  # identified by the marker argument, and nothing else.
  pkill -f "unity-headless-selftest-child $WORK" 2>/dev/null
  rm -r "$WORK"
}
trap cleanup EXIT

mkdir -p "$PROJECT/ProjectSettings" "$FAKE_APP"
printf 'm_EditorVersion: 0.0.0f0\n' > "$PROJECT/ProjectSettings/ProjectVersion.txt"

# The stand-in Unity. Records that it was invoked and its pid, then either
# writes a passing NUnit report (mode=pass), exits 0 with no report
# (mode=build), or forks a child and sleeps forever (mode=hang).
cat > "$FAKE_UNITY" <<'EOF'
#!/usr/bin/env bash
echo "$$" > "$FAKE_STATE/invoked"
results=""
while [ $# -gt 0 ]; do
  [ "$1" = "-testResults" ] && results="$2"
  shift
done
case "$FAKE_MODE" in
  pass)
    printf '<test-run total="3" passed="3" failed="0" skipped="0"></test-run>\n' > "$results" ;;
  build)
    exit 0 ;;
  hang)
    bash -c 'sleep 600; true' unity-headless-selftest-child "$FAKE_STATE" &
    echo "$!" > "$FAKE_STATE/child"
    sleep 600 ;;
esac
EOF
chmod +x "$FAKE_UNITY"

export UNITY_PATH="$FAKE_UNITY"
export UNITY_LOCKDIR="$LOCKDIR"
export UNITY_LOCK_POLL=1
export UNITY_WATCHDOG_POLL=1
export FAKE_STATE="$WORK"

reset_state() {
  rm -f "$WORK/invoked" "$WORK/child" "$PROJECT/TestResults.xml"
  [ -d "$LOCKDIR" ] && { rm -f "$LOCKDIR/owner"; rmdir "$LOCKDIR" 2>/dev/null; }
  return 0
}

# run_case <name> <expected_exit> <check_fn> <command...>
# check_fn receives the captured stderr path and may add its own conditions.
run_case() {
  local name="$1" want="$2" check="$3"; shift 3
  local err="$WORK/stderr" got
  "$@" >"$WORK/stdout" 2>"$err"; got=$?
  local why=""
  [ "$got" = "$want" ] || why="exit ${got}, wanted ${want}"
  [ -z "$why" ] && { why="$("$check" "$err")" || true; }
  if [ -z "$why" ]; then
    echo "PASS ${name}"; PASSED=$((PASSED + 1))
  else
    echo "FAIL ${name}: ${why}"; FAILED=$((FAILED + 1))
    sed 's/^/    | /' "$err"
  fi
}

# --- checks ------------------------------------------------------------------

lock_released()   { [ ! -d "$LOCKDIR" ] || echo "lock directory left behind"; }
lock_still_held() { [ -d "$LOCKDIR" ] || echo "lock directory was stolen"; }
unity_invoked()   { [ -f "$WORK/invoked" ] || echo "stand-in Unity was never started"; }
unity_not_invoked() { [ ! -f "$WORK/invoked" ] || echo "stand-in Unity was started; it should have been refused first"; }

check_happy() { lock_released; unity_invoked; }

check_editor_refused() {
  grep -q "has .* open" "$1" || echo "no open-Editor message on stderr"
  unity_not_invoked; lock_released
}

check_lock_exhausted() {
  grep -q "could not take the batchmode lock" "$1" || echo "no lock-exhausted message on stderr"
  unity_not_invoked; lock_still_held
}

check_reclaimed_dead() {
  grep -q "clearing stale lock left by dead pid" "$1" || echo "no reclaim message on stderr"
  check_happy
}

check_reclaimed_ownerless() {
  grep -q "clearing ownerless lock" "$1" || echo "no ownerless-reclaim message on stderr"
  check_happy
}

check_watchdog() {
  grep -q "watchdog" "$1" || echo "no watchdog message on stderr"
  unity_invoked; lock_released
  local upid; upid="$(cat "$WORK/invoked" 2>/dev/null)"
  if [ -n "$upid" ] && kill -0 "$upid" 2>/dev/null; then
    echo "stand-in Unity pid ${upid} survived the watchdog"
  fi
}

check_waited() {
  grep -q "waiting for batchmode lock" "$1" || echo "second run did not report waiting"
  grep -q "lock acquired after" "$1" || echo "second run did not report acquiring after the wait"
  check_happy
}

# --- cases -------------------------------------------------------------------

TEST_SH=("$SCRIPT_DIR/test.sh" "$PROJECT")
BUILD_SH=("$SCRIPT_DIR/build.sh" "$PROJECT" "Fake.Build")

reset_state
FAKE_MODE=pass run_case "test.sh: passing suite exits 0 and releases the lock" 0 check_happy "${TEST_SH[@]}"

reset_state
FAKE_MODE=build run_case "build.sh: clean exit 0 and releases the lock" 0 check_happy "${BUILD_SH[@]}"

# An Editor with this project open: comm is the Editor executable path (via
# exec -a) and argv carries the Hub's lowercase -projectpath.
reset_state
(exec -a "$FAKE_UNITY" bash -c 'sleep 600; true' unity-headless-selftest-editor -projectpath "$PROJECT") &
BG_PIDS+=("$!")
sleep 0.3
FAKE_MODE=pass run_case "test.sh: refuses while an Editor has the project open" 2 check_editor_refused "${TEST_SH[@]}"
FAKE_MODE=build run_case "build.sh: refuses while an Editor has the project open" 2 check_editor_refused "${BUILD_SH[@]}"
kill "${BG_PIDS[-1]}" 2>/dev/null; wait "${BG_PIDS[-1]}" 2>/dev/null; unset 'BG_PIDS[-1]'

# An Editor with a sibling project open whose path starts with ours must not
# count - the separator rule in project_is_open.
reset_state
mkdir -p "${PROJECT}2"
(exec -a "$FAKE_UNITY" bash -c 'sleep 600; true' unity-headless-selftest-editor -projectpath "${PROJECT}2") &
BG_PIDS+=("$!")
sleep 0.3
FAKE_MODE=pass run_case "test.sh: an Editor on a sibling path prefix does not block" 0 check_happy "${TEST_SH[@]}"
kill "${BG_PIDS[-1]}" 2>/dev/null; wait "${BG_PIDS[-1]}" 2>/dev/null; unset 'BG_PIDS[-1]'

# A decoy: our path and -projectPath in argv, but the executable is plain bash.
# This is the Unity Hub shape that the old pgrep -f check mistook for an Editor.
reset_state
bash -c 'sleep 600; true' unity-headless-selftest-decoy "$FAKE_UNITY" -projectPath "$PROJECT" &
BG_PIDS+=("$!")
sleep 0.3
FAKE_MODE=pass run_case "test.sh: a non-Editor process naming the project does not block" 0 check_happy "${TEST_SH[@]}"
kill "${BG_PIDS[-1]}" 2>/dev/null; wait "${BG_PIDS[-1]}" 2>/dev/null; unset 'BG_PIDS[-1]'

# Lock held by a live process (this harness): wait is exhausted, lock untouched.
reset_state
mkdir -p "$LOCKDIR"; printf 'selftest pid=%s\n' "$$" > "$LOCKDIR/owner"
FAKE_MODE=pass UNITY_LOCK_WAIT=2 run_case "test.sh: a lock held by a live pid is waited on, then given up (exit 2)" 2 check_lock_exhausted "${TEST_SH[@]}"

# Live lock AND an Editor on the project: that is what another headless run
# looks like from outside (batchmode Unity carries -projectPath too), so the
# answer is the lock wait, not the open-Editor refusal.
reset_state
mkdir -p "$LOCKDIR"; printf 'selftest pid=%s\n' "$$" > "$LOCKDIR/owner"
(exec -a "$FAKE_UNITY" bash -c 'sleep 600; true' unity-headless-selftest-editor -projectPath "$PROJECT") &
BG_PIDS+=("$!")
sleep 0.3
FAKE_MODE=pass UNITY_LOCK_WAIT=2 run_case "test.sh: a live lock plus a batchmode Editor is waited on, not refused as an open Editor" 2 check_lock_exhausted "${TEST_SH[@]}"
kill "${BG_PIDS[-1]}" 2>/dev/null; wait "${BG_PIDS[-1]}" 2>/dev/null; unset 'BG_PIDS[-1]'

# Lock whose owner pid is dead: reclaimed immediately.
reset_state
sleep 0.1 & DEAD=$!; wait "$DEAD"
mkdir -p "$LOCKDIR"; printf 'selftest pid=%s\n' "$DEAD" > "$LOCKDIR/owner"
FAKE_MODE=pass UNITY_LOCK_WAIT=2 run_case "test.sh: a lock whose owner pid is dead is reclaimed" 0 check_reclaimed_dead "${TEST_SH[@]}"

# Ownerless lock, fresh: must not be stolen (a holder may be mid-acquisition).
reset_state
mkdir -p "$LOCKDIR"
FAKE_MODE=pass UNITY_LOCK_WAIT=2 run_case "test.sh: a fresh ownerless lock is not stolen" 2 check_lock_exhausted "${TEST_SH[@]}"

# Ownerless lock older than the grace period: reclaimed. Aged with touch -t,
# not by sleeping through the grace.
reset_state
mkdir -p "$LOCKDIR"; touch -t 202001010000 "$LOCKDIR"
FAKE_MODE=pass UNITY_LOCK_WAIT=2 run_case "test.sh: an ownerless lock past the grace age is reclaimed" 0 check_reclaimed_ownerless "${TEST_SH[@]}"

# Watchdog: the stand-in never exits; the run must end at the ceiling, report
# it, kill the pid it started, and exit 2 (unrun), not 1 (failed).
reset_state
FAKE_MODE=hang UNITY_TEST_TIMEOUT=2 run_case "test.sh: a hung Editor is killed by the watchdog (exit 2)" 2 check_watchdog "${TEST_SH[@]}"
if [ -f "$WORK/child" ] && kill -0 "$(cat "$WORK/child")" 2>/dev/null; then
  echo "NOTE the hung stand-in's child (pid $(cat "$WORK/child")) survived the kill - the documented cost; the harness reaps it"
fi

reset_state
FAKE_MODE=hang UNITY_BUILD_TIMEOUT=2 run_case "build.sh: a hung Editor is killed by the watchdog (exit 2)" 2 check_watchdog "${BUILD_SH[@]}"

# Two runs back to back: the second waits for the first's lock and then runs.
# The first is a hang that the watchdog ends at 3 s; the second's wait limit is
# well above that, so it ends on the release event, not on its own limit.
reset_state
FAKE_STATE="$WORK/first" ; mkdir -p "$FAKE_STATE"
FAKE_MODE=hang UNITY_TEST_TIMEOUT=3 "${TEST_SH[@]}" >/dev/null 2>&1 &
FIRST=$!; BG_PIDS+=("$FIRST")
until [ -d "$LOCKDIR" ] && [ -f "$LOCKDIR/owner" ]; do sleep 0.1; done
FAKE_STATE="$WORK"
FAKE_MODE=pass UNITY_LOCK_WAIT=30 run_case "test.sh: a second run waits for the first's lock, then runs" 0 check_waited "${TEST_SH[@]}"
wait "$FIRST" 2>/dev/null; unset 'BG_PIDS[-1]'

echo
echo "selftest: ${PASSED} passed, ${FAILED} failed"
[ "$FAILED" -eq 0 ]
