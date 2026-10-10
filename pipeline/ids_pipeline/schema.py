"""Lowercase CSV/model names, with an explicit mapping to the web app's SQL columns."""
from datetime import datetime, timezone
from decimal import Decimal, InvalidOperation
from ipaddress import ip_address
import json
import math
import re

from .config import ROOT

COLUMNS = json.loads((ROOT / "config/flow-schema.json").read_text())["columns"]
NAMES = [column["name"] for column in COLUMNS]  # Existing SQL/C# property names only.
FEATURES = json.loads((ROOT / "config/model-features.json").read_text())["features"]


def snake_case(value):
    value = re.sub(r"([a-z0-9])([A-Z])", r"\1_\2", value.strip())
    return re.sub(r"[ -]+", "_", value).lower()


SQL_TO_CSV = {name: snake_case(name) for name in NAMES}
CSV_TO_SQL = {value: key for key, value in SQL_TO_CSV.items()}
ALIASES = {name.replace("_", ""): name for name in CSV_TO_SQL}


def normalize(value):
    lower = snake_case(value)
    return ALIASES.get(lower.replace("_", ""), lower)


def headers(names):
    if not names:
        raise ValueError("CSV has no header")
    normalized = [normalize(name) for name in names]
    if len(set(normalized)) != len(normalized):
        raise ValueError("CSV contains duplicate or ambiguous headers after lowercase normalization")
    unknown = sorted(set(normalized) - CSV_TO_SQL.keys())
    missing = sorted(set(FEATURES) - set(normalized))
    if unknown or missing:
        raise ValueError(f"CSV schema mismatch: missing={','.join(missing)}; unknown={','.join(unknown)}")
    return normalized


def validate(values, *, timestamp=None):
    names = [normalize(name) for name in values]
    if len(set(names)) != len(names):
        raise ValueError("Duplicate columns after lowercase normalization")
    values = dict(zip(names, values.values(), strict=True))
    result = {}
    for spec in COLUMNS:
        name, kind = SQL_TO_CSV[spec["name"]], spec["type"]
        if name not in values:
            if name in FEATURES:
                raise ValueError(f"{name}: missing model feature")
            # Only non-model storage fields may default. Preserve supplied values.
            if name == "timestamp":
                result[name] = timestamp or datetime.now(timezone.utc).replace(tzinfo=None)
            elif kind.startswith("string"):
                result[name] = None if kind.endswith("?") else ""
            else:
                result[name] = 0
            continue
        raw = values[name]
        value = "" if raw is None else str(raw).strip()
        if kind.startswith("string"):
            if len(value) > spec.get("max_length", 100):
                raise ValueError(f"{name}: value exceeds maximum length")
            if name == "src_ip" and value:
                try:
                    ip_address(value)
                except ValueError:
                    raise ValueError("src_ip: invalid IP address") from None
            result[name] = value or (None if kind.endswith("?") else "")
            continue
        if not value:
            raise ValueError(f"{name}: missing value")
        try:
            if kind == "DateTime":
                dt = datetime.fromisoformat(value.replace("Z", "+00:00"))
                if dt.tzinfo is None:
                    raise ValueError("timezone required")
                result[name] = dt.astimezone(timezone.utc).replace(tzinfo=None)
            elif kind in ("int", "long"):
                # Exporters often serialize counts as 1.0 or 1e3; never truncate fractions.
                number = Decimal(value)
                bits = 32 if kind == "int" else 64
                if (not number.is_finite() or number != number.to_integral_value()
                        or not -(2 ** (bits - 1)) <= number < 2 ** (bits - 1)):
                    raise ValueError("invalid integer")
                result[name] = int(number)
            else:
                number = float(value)
                if not math.isfinite(number) or abs(number) > 3.4028234663852886e38:
                    raise ValueError("nonfinite or overflowing model input")
                result[name] = number
        except (ValueError, OverflowError, InvalidOperation):
            raise ValueError(f"{name}: invalid {kind}; timestamps require a timezone") from None
    if not 0 <= result["dst_port"] <= 65535:
        raise ValueError("dst_port: must be between 0 and 65535")
    return result
