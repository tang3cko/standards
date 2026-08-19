#!/usr/bin/env python3
"""Summarize a Unity Test Framework NUnit result XML into a short pass/fail report.

Usage:
  summarize_test_results.py <results.xml> [--floor-file PATH] [--establish-baseline]

Invoked by test.sh - not usually run standalone.

`--floor-file` opts into the test-count ratchet: the highest total ever observed
for this filter is kept in PATH, and a later run that collects fewer tests fails.
Without the flag nothing about the ratchet applies and the exit codes below
behave exactly as they did before it existed.

Exit codes: 0 = no failures, 1 = one or more failures, 2 = bad input or a run
that collected no tests at all, 3 = the suite shrank below its recorded floor,
or the ratchet was asked for and no floor has been established yet.
"""
import sys
import xml.etree.ElementTree as ET
from pathlib import Path


def read_floor(path: Path):
    """The recorded high-water mark, or None when there is no usable one."""
    try:
        return int(path.read_text().strip())
    except FileNotFoundError:
        return None
    except (OSError, ValueError) as exc:
        # An unreadable or corrupt floor is not "no floor": silently treating it
        # as absent is how a ratchet stops ratcheting without anyone noticing.
        print(f"error: floor file {path} exists but could not be read: {exc}", file=sys.stderr)
        sys.exit(3)


def write_floor(path: Path, count: int) -> None:
    try:
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(f"{count}\n")
    except OSError as exc:
        print(f"error: could not record the floor in {path}: {exc}", file=sys.stderr)
        sys.exit(3)


def apply_floor(path: Path, total: int, failed: int, establish: bool) -> int:
    """The ratchet. Returns the exit code contribution, 0 when the floor is satisfied."""
    floor = read_floor(path)

    if establish:
        # Deliberate, and overrides the floor in both directions - this is the
        # only way to record a suite that legitimately got smaller.
        write_floor(path, total)
        print(f"Floor established at {total} ({path})")
        return 0

    if floor is None:
        print(
            f"error: no test-count floor recorded in {path}. Every test passed, but nothing "
            "here checked that the whole suite ran - re-run with ESTABLISH_BASELINE=1 to record "
            "the current count as the floor.",
            file=sys.stderr,
        )
        return 3

    if total < floor:
        print(
            f"error: {total} tests ran but the floor is {floor}. {floor - total} test(s) "
            "disappeared from this filter - a renamed class or a moved file drops out silently.",
            file=sys.stderr,
        )
        return 3

    # Only a passing run may raise the floor: a failed run's total is not a
    # count anyone should be held to next time.
    if total > floor and failed == 0:
        write_floor(path, total)
        print(f"Floor raised {floor} -> {total} ({path})")

    return 0


def main() -> int:
    args = sys.argv[1:]
    establish = False
    floor_file = None

    positional = []
    i = 0
    while i < len(args):
        if args[i] == "--establish-baseline":
            establish = True
        elif args[i] == "--floor-file":
            i += 1
            if i >= len(args):
                print("usage: summarize_test_results.py <results.xml> [--floor-file PATH] [--establish-baseline]", file=sys.stderr)
                return 2
            floor_file = Path(args[i])
        else:
            positional.append(args[i])
        i += 1

    if len(positional) != 1:
        print("usage: summarize_test_results.py <results.xml> [--floor-file PATH] [--establish-baseline]", file=sys.stderr)
        return 2

    try:
        root = ET.parse(positional[0]).getroot()
    except ET.ParseError as exc:
        print(f"error: could not parse {positional[0]}: {exc}", file=sys.stderr)
        return 2
    except OSError as exc:
        print(f"error: could not read {positional[0]}: {exc}", file=sys.stderr)
        return 2

    total = root.get("total", "0")
    passed = root.get("passed", "0")
    failed = root.get("failed", "0")
    skipped = root.get("skipped", "0")

    print(f"Total={total} Passed={passed} Failed={failed} Skipped={skipped}")

    # A run that collected nothing is not a pass. The usual cause is a
    # -testFilter that matches no test - a renamed class, a typo - which
    # otherwise reports Total=0 Failed=0 and grades as green.
    if int(total) == 0:
        print("error: 0 tests ran - a filter that matches nothing is not a pass.", file=sys.stderr)
        return 2

    for test_case in root.iter("test-case"):
        if test_case.get("result") == "Failed":
            message_el = test_case.find("./failure/message")
            message = message_el.text.strip() if message_el is not None and message_el.text else ""
            print(f"FAIL: {test_case.get('fullname')} - {message}")

    if floor_file is not None:
        floor_code = apply_floor(floor_file, int(total), int(failed), establish)
        # A failing suite is reported as a failing suite; the floor verdict only
        # decides the exit code when the tests themselves passed.
        if int(failed) > 0:
            return 1
        if floor_code != 0:
            return floor_code

    return 1 if int(failed) > 0 else 0


if __name__ == "__main__":
    sys.exit(main())
