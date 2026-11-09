#!/usr/bin/env python3
"""Reduce `hurl --json` output to a stable, masked snapshot: per entry status + captured body.

Usage: mask.py raw.json [random-suffix] > snapshot.json
Lists of objects are sorted (order is not part of the parity contract). Volatile values (guids, jwt tokens, timestamps, epoch expiry, refresh tokens, random suffixes) are masked.
"""
import json, re, sys

GUID = re.compile(r"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}")
JWT = re.compile(r"eyJ[\w-]+\.[\w-]+\.[\w-]+")
TIME = re.compile(r"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?(Z|[+-]\d{2}:\d{2})?")
VOLATILE_KEYS = {"expiryTime", "refreshToken", "accessToken", "correlationId", "creationTime", "lastModificationTime",
                 "userAgent", "clientIp", "concurrencyStamp"}

SUFFIX = sys.argv[2] if len(sys.argv) > 2 else None

def mask_str(s):
    if SUFFIX:
        s = s.replace(SUFFIX, "<sfx>")
    s = JWT.sub("<jwt>", s)
    s = GUID.sub("<guid>", s)
    return TIME.sub("<time>", s)

def mask(v, key=None):
    if key in VOLATILE_KEYS and v is not None:
        return "<masked>"
    if isinstance(v, dict):
        return {k: mask(x, k) for k, x in v.items()}
    if isinstance(v, list):
        items = [mask(x) for x in v]
        if items and all(isinstance(i, dict) for i in items):
            # seed order of permissions is not deterministic; compare lists as sets of objects
            items.sort(key=lambda i: json.dumps(i, sort_keys=True))
        return items
    if isinstance(v, str):
        return mask_str(v)
    return v

def parse_body(text):
    try:
        return json.loads(text)
    except Exception:
        return text

def main():
    raw = json.load(open(sys.argv[1]))
    out = []
    for entry in raw["entries"]:
        calls = entry.get("calls") or []
        status = calls[-1]["response"]["status"] if calls else None
        item = {"line": entry.get("line"), "status": status}
        for cap in entry.get("captures", []):
            name, value = cap["name"], cap["value"]
            if name.startswith("body_"):
                item["body"] = mask(parse_body(value))
            elif name.startswith("status_"):
                item[name] = value
        out.append(item)
    json.dump(out, sys.stdout, indent=2, sort_keys=True, ensure_ascii=False)
    print()

main()
