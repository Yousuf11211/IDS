"""Lowercase CSV/model names, with an explicit mapping to the web app's SQL columns."""
from datetime import datetime, timezone
from decimal import Decimal, InvalidOperation
from ipaddress import ip_address
import json
import re

from .config import ROOT
from .features import check_columns, numeric, prepare_handshake

COLUMNS = json.loads((ROOT / "config/flow-schema.json").read_text())["columns"]
NAMES = [column["name"] for column in COLUMNS]  # Existing SQL/C# property names only.


def snake_case(value):
    value = re.sub(r"([a-z0-9])([A-Z])", r"\1_\2", value.strip())
    return re.sub(r"[ -]+", "_", value).lower()


SQL_TO_CSV = {name: snake_case(name) for name in NAMES}
CSV_TO_SQL = {value: key for key, value in SQL_TO_CSV.items()}
ALIASES = {name.replace("_", ""): name for name in CSV_TO_SQL}


def normalize(value):
    lower = value.strip().lower()
    alias = lower.replace("_", "").replace(" ", "").replace("-", "")
    return ALIASES.get(alias, lower)


def headers(names, features):
    if not names:
        raise ValueError("CSV has no header")
    normalized = [normalize(name) for name in names]
    if len(set(normalized)) != len(normalized):
        raise ValueError("CSV contains duplicate or ambiguous headers after lowercase normalization")
    check_columns(normalized, features)
    return normalized


def prepare_row(values, features):
    names = headers(list(values), features)
    values = dict(zip(names, values.values(), strict=True))
    prepare_handshake(values)
    return values


def utc_timestamp(value, source_zone):
    try:
        dt = datetime.fromisoformat(value.replace("Z", "+00:00"))
    except ValueError:
        raise ValueError("timestamp: expected an ISO timestamp") from None
    if dt.tzinfo is not None:
        return dt.astimezone(timezone.utc).replace(tzinfo=None)
    if source_zone is None:
        raise ValueError("timestamp: supply an explicit offset or configure worker.source_timezone")
    # A local wall time may identify zero or two instants at a DST transition.
    candidates = set()
    for fold in (0, 1):
        utc = dt.replace(tzinfo=source_zone, fold=fold).astimezone(timezone.utc)
        if utc.astimezone(source_zone).replace(tzinfo=None) == dt:
            candidates.add(utc)
    if len(candidates) != 1:
        raise ValueError("timestamp: ambiguous or nonexistent local time; supply an explicit offset")
    return candidates.pop().replace(tzinfo=None)


def validate(values, features, *, timestamp=None, source_zone=None):
    values = prepare_row(values, features)
    result = {}
    for spec in COLUMNS:
        name, kind = SQL_TO_CSV[spec["name"]], spec["type"]
        if name not in values:
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
        if kind == "DateTime":
            result[name] = utc_timestamp(value, source_zone)
            continue
        try:
            if kind == "bool?":
                result[name] = bool(raw)
            elif (number := numeric(raw)) is None or number == 0:
                result[name] = number
            elif kind.rstrip("?") in ("int", "long"):
                # Exporters often serialize counts as 1.0 or 1e3; never truncate fractions.
                number = Decimal(value)
                bits = 32 if kind.rstrip("?") == "int" else 64
                if (not number.is_finite() or number != number.to_integral_value()
                        or not -(2 ** (bits - 1)) <= number < 2 ** (bits - 1)):
                    raise ValueError("invalid integer")
                result[name] = int(number)
            else:
                result[name] = number
        except (ValueError, OverflowError, InvalidOperation):
            raise ValueError(f"{name}: invalid {kind.rstrip('?')} value") from None
    if result["dst_port"] is not None and not 0 <= result["dst_port"] <= 65535:
        raise ValueError("dst_port: must be between 0 and 65535")
    return result
