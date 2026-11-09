#!/usr/bin/env python3
"""Prints "<requests> <milliseconds>" for a `hurl --json` result file (entries executed and the sum of their times)."""
import json, sys

try:
    entries = json.load(open(sys.argv[1]))["entries"]
except Exception:
    entries = []

print(len(entries), int(sum(entry.get("time", 0) for entry in entries)))
