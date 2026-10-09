"""Batch inference adapters. Production never falls back to simulated predictions."""
from dataclasses import dataclass
import hashlib
import json
import math

from .schema import NAMES


@dataclass(frozen=True)
class Prediction:
    attack: bool
    gate_score: float  # Probability of Attack, including for benign rows.
    type_score: float | None = None
    attack_type: str | None = None
    category: str | None = None
    severity: str = "Medium"


class PlaceholderModels:
    def __init__(self):
        raise RuntimeError("Models are not configured. Add both trained artifacts and a release manifest, "
                           "then select adapter='sklearn'. Use config/demo.toml for an isolated demo.")


class DemoModels:
    release = "demo-only-v1"
    gate_version = "demo-gate-v1"
    type_version = "demo-multiclass-v1"
    fingerprint = "demo-only-v1"

    def predict(self, rows):
        # Deliberately simple fixtures; these are NOT intrusion detection models.
        return [Prediction(True, .9, .8, "DemoAttack", "Demo", "Medium")
                if row["DstPort"] == 22 else Prediction(False, .1) for row in rows]


class SklearnModels:
    def __init__(self, config):
        import joblib
        self.manifest = json.loads((config.root / config.manifest).read_text())
        m = self.manifest
        self.release = m["release"]
        if not isinstance(self.release, str) or not 1 <= len(self.release) <= 50:
            raise ValueError("Release ID must contain 1 to 50 characters")
        self.fingerprint = hashlib.sha256(json.dumps(m, sort_keys=True).encode()).hexdigest()
        self.gate_version = m["gatekeeper"]["version"]
        self.type_version = m["multiclass"]["version"]
        for version in (self.gate_version, self.type_version):
            if not isinstance(version, str) or not 1 <= len(version) <= 100:
                raise ValueError("Model versions must contain 1 to 100 characters")
        for key in ("gatekeeper_threshold", "multiclass_threshold"):
            if not 0 < m[key] <= 1:
                raise ValueError(f"Invalid {key}")
        self.loaded = {}
        for stage in ("gatekeeper", "multiclass"):
            spec = m[stage]
            features = spec["features"]
            if (not features or len(set(features)) != len(features)
                    or any(name not in NAMES or name == "Label" for name in features)):
                raise ValueError(f"Invalid feature list for {stage}; Label is forbidden")
            artifact = (config.root / spec["artifact"]).resolve()
            if not artifact.is_relative_to((config.root / "models").resolve()):
                raise ValueError("Model artifacts must be inside pipeline/models")
            with artifact.open("rb") as stream:
                digest = hashlib.file_digest(stream, "sha256").hexdigest()
            if digest != spec["sha256"]:
                raise ValueError(f"Artifact checksum mismatch for {stage}")
            # Only trusted artifacts: joblib is executable serialization.
            model = joblib.load(artifact)
            if not callable(getattr(model, "predict_proba", None)):
                raise ValueError(f"{stage} must support predict_proba")
            if hasattr(model, "feature_names_in_") and list(model.feature_names_in_) != features:
                raise ValueError(f"{stage} feature order differs from fitted model")
            self.loaded[stage] = model
        gate_classes = list(map(str, self.loaded["gatekeeper"].classes_))
        expected = [str(m["gatekeeper"][key]) for key in ("attack_label", "benign_label")]
        if len(set(expected)) != 2 or len(gate_classes) != 2 or set(gate_classes) != set(expected):
            raise ValueError("Gatekeeper must have exactly the configured attack and benign classes")
        classes = list(map(str, self.loaded["multiclass"].classes_))
        if set(classes) != set(m["multiclass"]["labels"]):
            raise ValueError("Multiclass label map must cover every fitted class exactly")
        for meta in m["multiclass"]["labels"].values():
            if not meta["attack_type"] or len(meta["attack_type"]) > 100:
                raise ValueError("Attack type must contain 1 to 100 characters")
            if len(meta.get("category", "")) > 50:
                raise ValueError("Attack category exceeds 50 characters")
            if meta["severity"] not in ("Low", "Medium", "High", "Critical"):
                raise ValueError("Invalid severity")

    def probabilities(self, stage, rows):
        import pandas as pd
        features = self.manifest[stage]["features"]
        # Each artifact includes its OWN fitted preprocessing pipeline.
        frame = pd.DataFrame([{name: row[name] for name in features} for row in rows], columns=features)
        model = self.loaded[stage]
        probabilities = model.predict_proba(frame)
        if len(probabilities) != len(rows):
            raise ValueError("Model returned an incorrect batch size")
        for scores in probabilities:
            if (len(scores) != len(model.classes_)
                    or any(not math.isfinite(float(s)) or not 0 <= s <= 1 for s in scores)
                    or not math.isclose(float(sum(scores)), 1, abs_tol=1e-5)):
                raise ValueError("Model returned invalid probabilities")
        return probabilities

    def predict(self, rows):
        m = self.manifest
        gate_classes = list(map(str, self.loaded["gatekeeper"].classes_))
        attack_index = gate_classes.index(str(m["gatekeeper"]["attack_label"]))
        gate_scores = [float(scores[attack_index]) for scores in self.probabilities("gatekeeper", rows)]
        predictions = [Prediction(False, score) for score in gate_scores]
        indices = [i for i, score in enumerate(gate_scores) if score >= m["gatekeeper_threshold"]]
        if indices:
            scores = self.probabilities("multiclass", [rows[i] for i in indices])
            labels = list(map(str, self.loaded["multiclass"].classes_))
            for i, probabilities in zip(indices, scores, strict=True):
                best = max(range(len(probabilities)), key=lambda j: probabilities[j])
                confidence = float(probabilities[best])
                meta = m["multiclass"]["labels"][labels[best]]
                if confidence < m["multiclass_threshold"]:
                    predictions[i] = Prediction(True, gate_scores[i], confidence, "UnknownAttack", None, "Medium")
                else:
                    predictions[i] = Prediction(True, gate_scores[i], confidence,
                                                meta["attack_type"], meta.get("category"), meta["severity"])
        return predictions


def load_models(config):
    if config.adapter == "demo":
        if config.backend != "sqlite":
            raise ValueError("Demo mode cannot use SQL Server")
        return DemoModels()
    if config.adapter == "sklearn":
        return SklearnModels(config)
    return PlaceholderModels()
