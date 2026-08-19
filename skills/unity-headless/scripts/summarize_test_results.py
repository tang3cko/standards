#!/usr/bin/env python3
"""Summarize a Unity Test Framework NUnit result XML into a short pass/fail report.

Usage:
  summarize_test_results.py <results.xml>

Invoked by test.sh - not usually run standalone.

Exit codes: 0 = no failures, 1 = one or more failures, 2 = bad input or a run
that collected no tests at all.
"""
import sys
import xml.etree.ElementTree as ET


def main() -> int:
    if len(sys.argv) != 2:
        print("usage: summarize_test_results.py <results.xml>", file=sys.stderr)
        return 2

    try:
        root = ET.parse(sys.argv[1]).getroot()
    except ET.ParseError as exc:
        print(f"error: could not parse {sys.argv[1]}: {exc}", file=sys.stderr)
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

    return 1 if int(failed) > 0 else 0


if __name__ == "__main__":
    sys.exit(main())
