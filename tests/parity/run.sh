#!/usr/bin/env bash
# Runs every parity Hurl file against a running API, prints a `hurl --test` style report and writes masked snapshots.
# Usage: ./run.sh <output-dir> [host]   (default host: http://localhost:5100)
# Compare with the baseline:  diff -r baseline <output-dir>
set -uo pipefail
cd "$(dirname "$0")"
OUT="${1:?output dir required}"
HOST="${2:-http://localhost:5100}"
mkdir -p "$OUT"

files=0; failed=0; requests=0; total_ms=0
now_ms() { python3 -c 'import time; print(int(time.time() * 1000))'; }
started=$(now_ms)

for f in [0-9][0-9]-*.hurl; do
  name="${f%.hurl}"
  raw="$(mktemp)"; err="$(mktemp)"
  # A leading letter keeps Hurl from reading an all-digit suffix as a number (which drops leading zeros).
  suffix="x$(uuidgen | tr 'A-Z' 'a-z' | cut -c1-7)"

  if hurl --json --error-format long --variable "host=$HOST" --variable "suffix=$suffix" "$f" > "$raw" 2> "$err"; then
    result="Success"
  else
    result="Failure"
    failed=$((failed + 1))
  fi

  read -r count ms < <(python3 stats.py "$raw")
  requests=$((requests + count)); total_ms=$((total_ms + ms)); files=$((files + 1))

  # On failure Hurl's own message (assertion, request curl command, response) comes first, like `hurl --test`.
  if [[ "$result" == "Failure" ]]; then cat "$err"; echo; fi
  echo "$result tests/parity/$f ($count request(s) in $ms ms)"

  python3 mask.py "$raw" "$suffix" > "$OUT/$name.json"
  rm -f "$raw" "$err"
done

elapsed=$(( $(now_ms) - started ))
succeeded=$((files - failed))
echo "--------------------------------------------------------------------------------"
echo "Executed files:    $files"
echo "Executed requests: $requests"
echo "Succeeded files:   $succeeded ($((100 * succeeded / files))%)"
echo "Failed files:      $failed ($((100 * failed / files))%)"
echo "Duration:          $elapsed ms"

[[ $failed -eq 0 ]]
