"""Explicit CSV contract matching the existing C# database properties."""
from datetime import datetime, timezone
from ipaddress import ip_address
import json
import math
import re

from .config import ROOT

COLUMNS = json.loads((ROOT / "config/flow-schema.json").read_text())["columns"]
NAMES = [column["name"] for column in COLUMNS]


def canonical(value):
    return re.sub(r"[ _-]", "", value.strip()).lower()


def headers(names):
    if not names:
        raise ValueError("CSV has no header")
    mapping = {canonical(name): name for name in NAMES}
    normalized = [canonical(name) for name in names]
    if len(set(normalized)) != len(normalized):
        raise ValueError("CSV contains duplicate or ambiguous headers")
    unknown = [name for name in normalized if name not in mapping]
    missing = [name for name in NAMES if name != "Label" and canonical(name) not in normalized]
    if unknown or missing:
        raise ValueError(f"CSV schema mismatch: {len(missing)} missing, {len(unknown)} unknown columns")
    return [mapping[name] for name in normalized]


def validate(values):
    result = {}
    for spec in COLUMNS:
        name, kind = spec["name"], spec["type"]
        value = str(values.get(name, "") or "").strip()
        if kind.startswith("string"):
            if len(value) > spec.get("max_length", 100):
                raise ValueError(f"{name}: value exceeds maximum length")
            if name == "SrcIp":
                try:
                    ip_address(value)
                except ValueError:
                    raise ValueError("SrcIp: invalid IP address") from None
            result[name] = value or (None if kind.endswith("?") else "")
            continue
        if not value:
            raise ValueError(f"{name}: missing value")
        try:
            if kind == "DateTime":
                timestamp = datetime.fromisoformat(value.replace("Z", "+00:00"))
                if timestamp.tzinfo is None:
                    raise ValueError("timezone required")
                result[name] = timestamp.astimezone(timezone.utc).replace(tzinfo=None)
            elif kind in ("int", "long"):
                number = int(value)
                bits = 32 if kind == "int" else 64
                if not -(2 ** (bits - 1)) <= number < 2 ** (bits - 1):
                    raise ValueError("integer overflow")
                result[name] = number
            else:
                number = float(value)
                if not math.isfinite(number):
                    raise ValueError("nonfinite number")
                result[name] = number
        except (ValueError, OverflowError):
            raise ValueError(f"{name}: invalid {kind}; timestamps require a timezone") from None
    if not 0 <= result["DstPort"] <= 65535:
        raise ValueError("DstPort: must be between 0 and 65535")
    return result
