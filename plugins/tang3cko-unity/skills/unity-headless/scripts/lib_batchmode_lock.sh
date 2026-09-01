#!/usr/bin/env bash
# Batchmode serialization for headless Unity runs. Sourced by test.sh and
# build.sh, not run directly. Requires lib_resolve_unity.sh (project_is_open)
# to be sourced first.
#
# A Unity project has one lock: an open Editor and a batchmode run cannot share
# it, and neither can two batchmode runs. Without this file that rule is a
# promise in CLAUDE.md; with it, the scripts enforce it:
#
#   acquire_batchmode_lock <project_path>
#     Takes a per-project mkdir lock, refusing while an Editor has the project
#     open. Waits up to UNITY_LOCK_WAIT seconds for another batchmode run to
#     finish, reclaiming locks whose holder is gone. Sets HAVE_LOCK=1. Exits 2
#     ("the run never happened") when the wait is exhausted.
#   release_batchmode_lock
#     Idempotent; arm it in a trap BEFORE acquiring, so a signal in the gap
#     cannot leak the directory.
#   run_unity_watchdogged <timeout_secs> <unity_exe> <args...>
#     Runs Unity in the background and SIGKILLs it if it is still alive after
#     <timeout_secs>. Sets TIMED_OUT=1 when the watchdog fired. Returns Unity's
#     exit status (137 after a kill).
#
# Tunables (environment):
#   UNITY_LOCKDIR       lock directory (default: one per project path under
#                       ${TMPDIR:-/tmp}/unity-headless/)
#   UNITY_LOCK_WAIT     seconds to wait for a held lock before giving up (900)
#   UNITY_LOCK_POLL     seconds between lock attempts (5)
#   UNITY_LOCK_GRACE    age in seconds after which a lock directory with no
#                       owner file counts as abandoned (30)
#   UNITY_WATCHDOG_POLL seconds between liveness checks (5)
#
# What the watchdog does NOT do: it kills the pid it started and nothing else.
# Unity puts its helpers (Unity.Licensing.Client, UnityPackageManager,
# Unity.ILPP.Runner, UnityAutoQuitter) in their own process groups, so neither
# a plain kill nor a process-group kill reaches them and they can be left as
# orphans after a timeout. That is an accepted cost. A name-matched sweep of
# the machine is deliberately not implemented: a previous attempt at one in
# Puzzle_003 (2026-07-28) shipped with an empty pattern that matched every
# process and SIGKILLed the whole machine, four times. If orphans remain after
# a watchdog kill, list them and remove them by hand.

HAVE_LOCK=0
# shellcheck disable=SC2034  # read by the sourcing script after the run
TIMED_OUT=0
LOCKDIR=""

_bm_log() { printf '[unity-headless %s] %s\n' "$(date '+%H:%M:%S')" "$*" >&2; }

# One lock per project path, not per project name: two checkouts of the same
# project are two projects, and a sibling whose name happens to match is not
# this one. cksum is POSIX; the basename is only there for humans reading /tmp.
_bm_default_lockdir() {
  local project_path="$1" key
  key="$(printf '%s' "$project_path" | cksum | awk '{print $1}')"
  printf '%s/unity-headless/%s-%s.lock' "${TMPDIR:-/tmp}" "$(basename "$project_path")" "$key"
}

# Seconds since $LOCKDIR was last modified. Both stat spellings are tried; a
# failure to read the age returns nonzero rather than guessing 0, which would
# make an ownerless lock look instantly reclaimable.
_bm_lock_age_seconds() {
  local mtime now
  mtime=$(stat -f %m "$LOCKDIR" 2>/dev/null || stat -c %Y "$LOCKDIR" 2>/dev/null) || return 1
  [ -n "$mtime" ] || return 1
  now=$(date +%s)
  printf '%s' "$(( now - mtime ))"
}

# Reclaims a lock whose holder is gone. Two ways a lock goes stale, and both are
# handled: the owner file names a pid that no longer exists, or there is no
# owner file at all. The second is not hypothetical - acquire takes the lock
# with mkdir and writes the owner on the next line, so a kill inside that
# window leaves a directory no pid check can ever reclaim. Multiplayer_009
# measured this on 2026-08-08: one driver killed in that window made every
# later run wait the full 900 s. The ownerless case is judged by age, not
# existence: a run that took the lock this instant is about to write its
# owner, and stealing it would hand the project to two editors.
_bm_clear_stale_lock() {
  [ -d "$LOCKDIR" ] || return 1

  if [ ! -f "$LOCKDIR/owner" ]; then
    local age grace="${UNITY_LOCK_GRACE:-30}"
    age=$(_bm_lock_age_seconds) || return 1
    [ "$age" -ge "$grace" ] || return 1
    _bm_log "clearing ownerless lock abandoned ${age}s ago (holder died between mkdir and owner write)"
    rmdir "$LOCKDIR" 2>/dev/null
    return 0
  fi

  local owner_pid
  owner_pid=$(awk 'match($0, /pid=[0-9]+/) { print substr($0, RSTART+4, RLENGTH-4); exit }' \
    "$LOCKDIR/owner" 2>/dev/null)
  [ -n "$owner_pid" ] || return 1
  kill -0 "$owner_pid" 2>/dev/null && return 1   # holder is alive; not stale
  _bm_log "clearing stale lock left by dead pid ${owner_pid}"
  rm -f "$LOCKDIR/owner"; rmdir "$LOCKDIR" 2>/dev/null
  return 0
}

acquire_batchmode_lock() {
  local project_path="$1"
  local waited=0 limit="${UNITY_LOCK_WAIT:-900}" poll="${UNITY_LOCK_POLL:-5}" announced=0
  LOCKDIR="${UNITY_LOCKDIR:-$(_bm_default_lockdir "$project_path")}"
  mkdir -p "$(dirname "$LOCKDIR")"

  while :; do
    _bm_clear_stale_lock || true
    # Order matters. A batchmode Unity is itself an Editor process carrying
    # -projectPath, so project_is_open cannot tell "another headless run is in
    # progress" from "a human has the project open". The lock can: a live lock
    # means one of these scripts holds the project and will give it back, so
    # wait. No lock but an Editor on the project means nobody here is going to
    # close it - Core Rule 1 says the caller must sequence them - so stop now
    # rather than burn the full wait. Measured 2026-09-02 on Action_006: with
    # the Editor check first, a second test.sh started during a live run took
    # the open-Editor exit instead of queueing.
    if [ ! -d "$LOCKDIR" ]; then
      if project_is_open "$project_path"; then
        echo "error: a Unity Editor has ${project_path} open; batchmode cannot share the project lock." >&2
        echo "       Quit it (or drive the task through unity-mcp instead). See references/pitfalls.md." >&2
        exit 2
      fi
      if mkdir "$LOCKDIR" 2>/dev/null; then
        # The pid is what _bm_clear_stale_lock reads back to tell a live holder
        # from one whose driver died before its trap could release.
        printf 'unity-headless pid=%s %s\n' "$$" "$(date)" > "$LOCKDIR/owner"
        HAVE_LOCK=1
        [ "$announced" = 1 ] && _bm_log "lock acquired after ${waited}s"
        return 0
      fi
    fi
    if [ "$waited" -ge "$limit" ]; then
      echo "error: could not take the batchmode lock within ${limit}s (held by: $(cat "$LOCKDIR/owner" 2>/dev/null || echo '<unknown>'))." >&2
      echo "       Another headless run against ${project_path} is still going; this run never started." >&2
      exit 2
    fi
    if [ "$announced" = 0 ]; then
      _bm_log "waiting for batchmode lock ${LOCKDIR} (held by: $(cat "$LOCKDIR/owner" 2>/dev/null || echo '<unknown>'), up to ${limit}s)"
      announced=1
    fi
    sleep "$poll"; waited=$(( waited + poll ))
  done
}

release_batchmode_lock() {
  [ "$HAVE_LOCK" = 1 ] || return 0
  HAVE_LOCK=0
  rm -f "$LOCKDIR/owner"; rmdir "$LOCKDIR" 2>/dev/null
}

run_unity_watchdogged() {
  local timeout="$1"; shift
  local poll="${UNITY_WATCHDOG_POLL:-5}" waited=0 upid

  TIMED_OUT=0
  "$@" &
  upid=$!
  while [ "$waited" -lt "$timeout" ]; do
    sleep "$poll"; waited=$(( waited + poll ))
    kill -0 "$upid" 2>/dev/null || break
  done
  if kill -0 "$upid" 2>/dev/null; then
    _bm_log "watchdog: Unity pid ${upid} still running after ${timeout}s - killing it"
    # shellcheck disable=SC2034  # read by the sourcing script after the run
    TIMED_OUT=1
    kill -9 "$upid" 2>/dev/null
  fi
  wait "$upid" 2>/dev/null
}
