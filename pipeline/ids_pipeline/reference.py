"""Build an identifiable benign TRAINING/reference profile, never from detections."""
import argparse
import csv
from datetime import datetime, timezone
import hashlib
import json
import logging
from pathlib import Path

import numpy as np

from .config import Config, ROOT
from .models import load_models
from .schema import headers


def build_profile(config, manifest_path):
    manifest = json.loads(manifest_path.read_text(encoding="utf-8-sig"))
    if manifest.get("split") not in ("training", "reference") or not manifest.get("provenance"):
        raise ValueError("Reference requires declared training/reference split and documented provenance")
    source = (manifest_path.parent / manifest["file"]).resolve()
    if source.is_relative_to((config.root / config.data_dir).resolve()):
        raise ValueError("Detection CSVs cannot be used as a training reference")
    digest = hashlib.sha256(source.read_bytes()).hexdigest()
    if digest != manifest["sha256"] or not manifest.get("datasetVersion"):
        raise ValueError("Reference dataset hash/version must be supplied and match")
    models = load_models(config)
    rows = []
    with source.open(encoding="utf-8-sig", newline="") as stream:
        reader = csv.reader(stream)
        names = headers(next(reader, None), models.feature_names)
        if "label" not in names:
            raise ValueError("Reference must include ground-truth labels")
        for values in reader:
            row = dict(zip(names, values, strict=True))
            if row["label"].strip() == manifest.get("benignLabel", "Benign"):
                rows.append(row)
    if not rows:
        raise ValueError("No ground-truth benign rows in reference dataset")
    data, _ = models.input_matrix("gatekeeper", rows)
    features = {}
    for i, name in enumerate(models.feature_names):
        if name in ("dst_port", "delta_start_incomplete_handshake"):
            continue  # Port identifiers and the binary handshake flag are categorical.
        values = data[:, i].astype(float)
        values = values[np.isfinite(values)]
        if not len(values):
            continue
        q = np.percentile(values, [5, 25, 50, 75, 95])
        features[name] = dict(mean=float(values.mean()), standardDeviation=float(values.std()),
                              p05=float(q[0]), p25=float(q[1]), median=float(q[2]),
                              p75=float(q[3]), p95=float(q[4]), sampleCount=len(values))
    return dict(schemaVersion=1, datasetVersion=manifest["datasetVersion"], datasetSha256=digest,
                split=manifest["split"], provenance=manifest["provenance"],
                modelFingerprint=models.fingerprint, group="all benign reference traffic",
                createdAt=datetime.now(timezone.utc).isoformat(), features=features)


def load_reference(config):
    if not config.reference_profile:
        return None
    try:
        return validate_reference(json.loads((config.root / config.reference_profile).read_text()))
    except (OSError, ValueError, KeyError, TypeError):
        logging.getLogger(__name__).warning("Reference unavailable: configured artifact is missing or invalid")
        return None


def validate_reference(result):
    if not isinstance(result, dict) or not isinstance(result.get("features"), dict):
        raise ValueError("Reference profile must contain a feature statistics object")
    if result.get("schemaVersion") != 1 or result.get("split") not in ("training", "reference"):
        raise ValueError("Unsupported/unverified benign reference profile")
    if (not result.get("datasetVersion") or not result.get("provenance") or
            len(result.get("datasetSha256", "")) != 64 or len(result.get("modelFingerprint", "")) != 64):
        raise ValueError("Reference profile lacks dataset/model provenance")
    for name, statistics in result["features"].items():
        values = [statistics[key] for key in ("median", "mean", "standardDeviation", "p05", "p25", "p75", "p95")]
        if (name == "dst_port" or not np.isfinite(values).all() or statistics["sampleCount"] <= 0 or
                statistics["standardDeviation"] < 0 or not
                statistics["p05"] <= statistics["p25"] <= statistics["median"] <= statistics["p75"] <= statistics["p95"]):
            raise ValueError("Invalid numerical benign reference statistics")
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("manifest", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("--config", type=Path, default=ROOT / "config/pipeline.toml")
    args = parser.parse_args()
    result = build_profile(Config.load(args.config), args.manifest)
    args.output.write_text(json.dumps(result, allow_nan=False, indent=2), encoding="utf-8")


if __name__ == "__main__":
    main()
