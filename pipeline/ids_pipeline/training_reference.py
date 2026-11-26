"""Rebuild the deployed Experiment D benign reference from its exact training rows."""
import argparse
from collections import defaultdict
from concurrent.futures import ThreadPoolExecutor
import csv
from datetime import datetime, timezone
import gzip
import hashlib
import io
import json
from pathlib import Path

import numpy as np
import pandas as pd

from .config import Config, ROOT
from .models import MODEL_SCHEMA_VERSION, load_models
from .reference import validate_reference

DELTA = "delta_start"
HANDSHAKE = "delta_start_incomplete_handshake"
BENIGN_SOURCES = {"IDS2018": "historical", "Created_Data": "lab"}
EXPERIMENT_DIR = "Experiments/Experiment D"


def sha256(path):
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def schema_hash(names):
    return hashlib.sha256("\n".join(names).encode("utf-8")).hexdigest()


def source_path(root, relative):
    path = (root / relative).resolve()
    if not path.is_relative_to(root):
        raise ValueError(f"Training path is outside the project: {relative}")
    return path


def preprocess(frame, names, audit):
    """The float32 conversion, missing handling and flag from run_four_experiments.py."""
    frame.columns = [str(name).strip().lower() for name in frame.columns]
    marker = frame[DELTA].fillna("").astype(str).str.strip().str.lower().eq("not a complete handshake")
    if HANDSHAKE in frame:
        flag = pd.to_numeric(frame[HANDSHAKE], errors="coerce")
        if (~flag.isin([0, 1]) | (marker & flag.ne(1))).any():
            raise ValueError("Invalid training handshake flag")
        frame[HANDSHAKE] = flag
    else:
        frame[HANDSHAKE] = marker.astype(np.float32)

    label_column = audit["label_column"]
    if label_column:
        labels = frame[label_column].fillna("").astype(str).str.strip().map(audit["label_mapping"])
        if not labels.eq("Benign").all():
            raise ValueError("Selected row is not labeled Benign by the training audit")
    elif audit.get("override") != "Benign":
        raise ValueError("Selected training file lacks verified benign labels")

    columns = {}
    for name in names:
        with np.errstate(over="ignore", invalid="ignore"):
            values = pd.to_numeric(frame[name], errors="coerce").to_numpy(dtype=np.float32, copy=True)
        values[np.isinf(values)] = 0.0
        values[values == 0] = 0.0
        values[np.isnan(values)] = np.nan
        columns[name] = values
    return pd.DataFrame(columns, columns=names)


def replay_file(root, relative, selected, names, audit):
    path = source_path(root, relative)
    if not path.is_file():
        raise FileNotFoundError(f"Experiment D training CSV is required: {path}")
    if len({number for number, _ in selected}) != len(selected):
        raise ValueError(f"Duplicate source row in the training manifest: {relative}")
    selected = sorted(selected)
    result = np.empty((len(selected), len(names)), dtype=np.float32)
    digest = hashlib.sha256()
    with path.open("rb", buffering=8 * 1024 * 1024) as stream:
        header = stream.readline()
        digest.update(header)
        original = pd.read_csv(io.BytesIO(header), nrows=0, encoding="utf-8-sig").columns
        normalized = {str(name).strip().lower(): name for name in original}
        if len(normalized) != len(original):
            raise ValueError(f"Duplicate training CSV header: {relative}")
        columns = [normalized[name] for name in names if name != HANDSHAKE or name in normalized]
        if audit["label_column"]:
            columns.append(normalized[audit["label_column"]])

        def verify(lines, start, stop):
            batch = pd.read_csv(io.BytesIO(header + b"".join(lines)), dtype=str, usecols=columns,
                                encoding="utf-8-sig", low_memory=False)
            processed = preprocess(batch, names, audit)
            observed = pd.util.hash_pandas_object(processed, index=False).to_numpy(dtype=np.uint64)
            expected = np.asarray([value for _, value in selected[start:stop]], dtype=np.uint64)
            if not np.array_equal(observed, expected):
                raise ValueError(f"Manifest feature fingerprint mismatch in {relative}, source row {selected[start][0]}")
            result[start:stop] = processed.to_numpy(dtype=np.float32)

        lines = []
        index = start = row_count = 0
        target = selected[0][0]
        for raw in stream:
            digest.update(raw)
            # The capture CSVs have one flow per line. The input digest, audited
            # row count, and selected feature hashes verify that assumption.
            if raw.isspace():
                continue
            row_count += 1
            if row_count != target:
                continue
            lines.append(raw)
            index += 1
            target = selected[index][0] if index < len(selected) else -1
            if len(lines) >= 4000 or target == -1:
                verify(lines, start, index)
                lines = []
                start = index
    if (index != len(selected) or row_count != audit["raw_rows"] or digest.hexdigest() != audit["sha256"]):
        raise ValueError(f"Training CSV differs from the audited input: {relative}")
    print(f"Verified {len(selected):,} benign training rows from {relative}", flush=True)
    return result


def summarize(data, metadata):
    finite = data[np.isfinite(data)].astype(np.float64)
    count = len(finite)
    kind = metadata["kind"]
    result = dict(kind=kind, sampleCount=count, missingCount=len(data) - count, units=metadata["units"])
    if count == 0:
        return result
    quantiles = np.percentile(finite, [5, 25, 50, 75, 95])
    result.update(minimum=float(finite.min()), maximum=float(finite.max()), mean=float(finite.mean()),
                  standardDeviation=float(finite.std()), **dict(zip(
                      ("p05", "p25", "median", "p75", "p95"), map(float, quantiles), strict=True)))
    if kind in ("categorical", "binary", "discrete"):
        values, counts = np.unique(finite, return_counts=True)
        # Keep all port and binary categories, so an observed value has an exact
        # lookup even when it is rare. Bound payload sizes for general counts.
        order = np.lexsort((values, -counts))
        if kind == "discrete":
            order = order[:20]
        result.update(frequencies=[dict(value=float(values[i]), count=int(counts[i])) for i in order],
                      otherCount=count - int(counts[order].sum()), distinctValues=len(values))
    return result


def build(config, training_root, *, workers=2):
    root = training_root.resolve()
    experiment = root / EXPERIMENT_DIR
    manifest = experiment / "training_row_manifest.csv.gz"
    models = load_models(config)
    names = models.feature_names
    trained = json.loads((experiment / "features_used.json").read_text(encoding="utf-8-sig"))
    if trained != names:
        raise ValueError("Experiment D feature order differs from the deployed feature contract")
    model_hashes = {}
    for stage, relative, deployed in (
        ("gatekeeper", "binary_gatekeeper/binary.ubj", config.gatekeeper),
        ("multiclass", "multiclass_attack_model/multiclass.ubj", config.multiclass)
    ):
        digest = sha256(experiment / relative)
        if digest != sha256(config.root / deployed):
            raise ValueError(f"Deployed {stage} model does not match Experiment D")
        model_hashes[stage] = digest

    with gzip.open(manifest, mode="rt", encoding="utf-8", newline="") as stream:
        grouped = defaultdict(list)
        for row in csv.DictReader(stream):
            if row["__label"] != "Benign":
                continue
            relative = row["__file"]
            if BENIGN_SOURCES.get(relative.split("/")[0]) != row["__source"]:
                raise ValueError(f"Manifest row is outside the Experiment D training sources: {relative}")
            grouped[relative].append((int(row["__row"]), int(row["__hash"])))
    expected = json.loads((experiment / "training_composition.json").read_text())["class_counts"]["Benign"]
    if expected <= 0 or sum(map(len, grouped.values())) != expected:
        raise ValueError("Benign membership differs from Experiment D training composition")
    audit_path = root / "Experiments/input_file_audit.json"
    audit = {item["file"]: item for item in json.loads(audit_path.read_text(encoding="utf-8-sig"))["files"]}
    matrix = np.empty((expected, len(names)), dtype=np.float32)
    start = 0
    with ThreadPoolExecutor(max_workers=workers) as executor:
        paths = sorted(grouped)
        for batch_start in range(0, len(paths), workers):
            batch = paths[batch_start:batch_start + workers]
            futures = [executor.submit(replay_file, root, name, grouped[name], names, audit[name]) for name in batch]
            for pending in futures:
                values = pending.result()
                matrix[start:start + len(values)] = values
                start += len(values)

    metadata = json.loads((config.root / "config/feature-metadata.json").read_text(encoding="utf-8"))["features"]
    if list(metadata) != names:
        raise ValueError("Feature metadata order differs from deployed schema")
    features = {name: summarize(matrix[:, index], metadata[name]) for index, name in enumerate(names)}
    manifest_sha = sha256(manifest)
    training_script_sha = sha256(root / "run_four_experiments.py")
    return validate_reference(dict(schemaVersion=2, datasetVersion="Experiment D · final benign training rows",
        datasetSha256=manifest_sha, split="training", group="Experiment D benign training flows",
        provenance="Verified against the Experiment D training manifest, training input audit, feature fingerprints and both deployed UBJ files.",
        benignSampleCount=expected, modelFingerprint=models.fingerprint, modelRelease=models.release,
        modelFiles=model_hashes, featureSchemaVersion=MODEL_SCHEMA_VERSION, featureSchemaSha256=schema_hash(names),
        preprocessingVersion=f"run_four_experiments.py SHA-256 {training_script_sha}",
        reproductionLibraries=dict(pandas=pd.__version__, numpy=np.__version__),
        createdAt=datetime.now(timezone.utc).isoformat(), featureNames=names, features=features,
        sources=[dict(file=name, sha256=audit[name]["sha256"], benignRows=len(grouped[name]))
                 for name in sorted(grouped)]))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--training-root", type=Path, required=True)
    parser.add_argument("--config", type=Path, default=ROOT / "config/pipeline.toml")
    parser.add_argument("--output", type=Path, default=ROOT / "config/benign-reference.json")
    parser.add_argument("--workers", type=int, choices=range(1, 5), default=2)
    args = parser.parse_args()
    profile = build(Config.load(args.config), args.training_root, workers=args.workers)
    target = args.output.resolve()
    if not target.is_relative_to(ROOT.resolve()):
        raise ValueError("Write the reference artifact inside the IDS pipeline directory")
    temporary = target.with_suffix(".tmp")
    temporary.write_text(json.dumps(profile, ensure_ascii=False, allow_nan=False, indent=2) + "\n", encoding="utf-8")
    temporary.replace(target)
    print(f"Wrote {len(profile['features'])} features from {profile['benignSampleCount']:,} benign training rows to {target}")


if __name__ == "__main__":
    main()
