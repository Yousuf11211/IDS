"""Ordered input contract shared by CSV import, XGBoost and SHAP replay."""
from decimal import Decimal, InvalidOperation
import json
import math

HANDSHAKE = "delta_start_incomplete_handshake"
FEATURE_COUNT = 120


def load_features(config):
    path = config.root / config.feature_list
    try:
        names = json.loads(path.read_text(encoding="utf-8-sig"))
    except ValueError as error:
        raise ValueError(f"{path}: invalid feature JSON: {error}") from error
    if (not isinstance(names, list) or len(names) != FEATURE_COUNT
            or any(not isinstance(name, str) or not name or name != name.strip().lower() for name in names)
            or len(set(names)) != FEATURE_COUNT):
        raise ValueError(f"{path}: expected {FEATURE_COUNT} unique, lowercase feature names in training order")
    if "label" in names or not {HANDSHAKE, "delta_start"}.issubset(names):
        raise ValueError(f"{path}: invalid handshake or source label feature definition")
    return names


def check_columns(names, features):
    available = set(names)
    if HANDSHAKE not in available:
        if "delta_start" not in available:
            raise ValueError(f"Cannot reconstruct {HANDSHAKE}: delta_start is missing")
        available.add(HANDSHAKE)
    missing = [name for name in features if name not in available]
    if missing:
        raise ValueError("Missing required model features: " + ", ".join(missing))


def prepare_handshake(values):
    marker = "not a complete handshake" in str(values["delta_start"]).lower()
    if HANDSHAKE in values:
        try:
            raw = values[HANDSHAKE]
            flag = Decimal(raw.strip()) if isinstance(raw, str) else raw
        except InvalidOperation:
            flag = None
        if isinstance(flag, Decimal) and not flag.is_finite():
            flag = None
        if flag not in (0, 1) or (marker and flag != 1):
            raise ValueError(f"{HANDSHAKE} must be 0 or 1 and must be 1 for an incomplete handshake")
        values[HANDSHAKE] = int(flag)
    else:
        values[HANDSHAKE] = int(marker)
    if marker:
        values["delta_start"] = None


def numeric(value):
    """SQL NULL and JSON null represent the model's native NaN between stages."""
    try:
        number = float(value)
    except (TypeError, ValueError):
        return None
    if math.isnan(number):
        return None
    if math.isinf(number):
        return 0
    if abs(number) > 3.4028234663852886e38:
        raise ValueError("Finite model input exceeds float32 range")
    return number
