#!/usr/bin/env bash
# Runs every parity Hurl file against a running API and writes masked snapshots.
# Usage: ./run.sh <output-dir> [host]   (default host: http://localhost:5100)
# Compare with the baseline:  diff -r baseline <output-dir>
set -euo pipefail
cd "$(dirname "$0")"
OUT="${1:?output dir required}"
HOST="${2:-http://localhost:5100}"
mkdir -p "$OUT"
status=0
for f in [0-9][0-9]-*.hurl; do
  name="${f%.hurl}"
  raw="$(mktemp)"
  # A leading letter keeps Hurl from reading an all-digit suffix as a number (which drops leading zeros).
  suffix="x$(uuidgen | tr 'A-Z' 'a-z' | cut -c1-7)"
  if ! hurl --json --variable "host=$HOST" --variable "suffix=$suffix" "$f" > "$raw"; then
    echo "FAILED: $f" >&2
    status=1
  fi
  python3 mask.py "$raw" "$suffix" > "$OUT/$name.json"
  rm -f "$raw"
done
exit $status
