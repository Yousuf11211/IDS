"""Two-stage inference using the supplied XGBoost UBJ artifacts and text label maps."""
from dataclasses import dataclass
import hashlib
import json
import re

from .schema import FEATURES, normalize


@dataclass(frozen=True)
class Prediction:
    attack: bool
    gate_score: float
    type_score: float | None = None
    attack_type: str | None = None
    category: str | None = None
    severity: str = "Medium"


def read_labels(path):
    labels = {}
    for line in path.read_text(encoding="utf-8-sig").splitlines():
        match = re.fullmatch(r"\s*(\d+)\s*=\s*(.+?)\s*", line)
        if match:
            index, label = int(match[1]), match[2]
            if index in labels or label in labels.values() or len(label) > 100:
                raise ValueError(f"Invalid or duplicate label in {path.name}")
            labels[index] = label
    if not labels or set(labels) != set(range(len(labels))):
        raise ValueError(f"{path.name}: labels must be contiguous class IDs starting at zero")
    return labels


class XGBoostModels:
    def __init__(self, config):
        import xgboost as xgb
        self.config = config
        self.loaded, self.features, self.labels = {}, {}, {}
        self.policy = json.loads((config.root / config.attack_policy).read_text(encoding="utf-8-sig"))
        contract = {"schema_version": 2, "features": FEATURES, "policy": self.policy,
                    "gate_threshold": config.gatekeeper_threshold,
                    "type_threshold": config.multiclass_threshold}
        versions = {}
        for stage in ("gatekeeper", "multiclass"):
            artifact = config.root / getattr(config, stage)
            label_path = config.root / getattr(config, stage + "_labels")
            if artifact.suffix.lower() != ".ubj":
                raise ValueError(f"{stage}: expected a .ubj model")
            content = artifact.read_bytes()
            digest = hashlib.sha256(content).hexdigest()
            model = xgb.Booster()
            model.load_model(bytearray(content))
            model.set_param({"device": "cpu", "nthread": 2})
            names = model.feature_names
            if not names:
                raise ValueError(f"{stage}: model must contain its trained feature names")
            features = [normalize(name) for name in names]
            extra = "delta_start_incomplete_handshake"
            if len(set(features)) != len(features) or set(features) != set(FEATURES) | {extra}:
                raise ValueError(f"{stage}: model features differ from the supplied CSV contract")
            # The supplied artifacts contain this extra training column, but neither
            # uses it in any tree split. Zero is safe ONLY while it remains unused.
            used = {normalize(name) for name in model.get_score(importance_type="weight")}
            if extra in used:
                raise ValueError(f"{stage} now uses {extra}; configure its training transformation before inference")
            if model.num_features() != len(FEATURES) + 1:
                raise ValueError(f"{stage}: incorrect feature count")
            labels = read_labels(label_path)
            learner = json.loads(model.save_config())["learner"]
            objective = learner["objective"]["name"]
            if stage == "gatekeeper":
                if objective != "binary:logistic" or labels != {0: "Benign", 1: "Attack"}:
                    raise ValueError("Gatekeeper must be binary:logistic with 0=Benign, 1=Attack")
            elif (objective != "multi:softprob"
                  or int(learner["learner_model_param"]["num_class"]) != len(labels)):
                raise ValueError("Multiclass must use multi:softprob and match its label mapping")
            self.loaded[stage], self.features[stage], self.labels[stage] = model, features, labels
            contract[stage] = {"sha256": digest, "labels": labels, "features": features}
            versions[stage] = stage + "-" + digest[:16]
        if set(self.policy) != set(self.labels["multiclass"].values()):
            raise ValueError("Attack policy must cover every multiclass label exactly")
        for meta in self.policy.values():
            if meta["severity"] not in ("Low", "Medium", "High", "Critical") or len(meta["category"]) > 50:
                raise ValueError("Invalid attack severity/category policy")
        self.fingerprint = hashlib.sha256(json.dumps(contract, sort_keys=True).encode()).hexdigest()
        self.release = "ubj-" + self.fingerprint[:20]
        self.gate_version, self.type_version = versions["gatekeeper"], versions["multiclass"]

    def probabilities(self, stage, rows):
        import numpy as np
        import xgboost as xgb
        model = self.loaded[stage]
        data = np.asarray([[0.0 if name == "delta_start_incomplete_handshake" else row[name]
                            for name in self.features[stage]] for row in rows], dtype=np.float32)
        if not np.isfinite(data).all():
            raise ValueError("Nonfinite model input")
        matrix = xgb.DMatrix(data, feature_names=model.feature_names, nthread=2)
        best = model.attr("best_iteration")
        scores = model.predict(matrix, validate_features=True, strict_shape=True,
                               iteration_range=(0, int(best) + 1) if best is not None else (0, 0))
        expected = 1 if stage == "gatekeeper" else len(self.labels[stage])
        if (scores.shape != (len(rows), expected) or not np.isfinite(scores).all()
                or (scores < 0).any() or (scores > 1).any()):
            raise ValueError(f"{stage}: invalid model probabilities")
        if stage == "multiclass" and not np.allclose(scores.sum(axis=1), 1, atol=1e-5):
            raise ValueError("Multiclass probabilities do not sum to one")
        return scores

    def predict(self, rows):
        if not rows:
            return []
        # All callers, including SQL recovery, use the same lowercase naming contract.
        rows = [{normalize(key): value for key, value in row.items()} for row in rows]
        gate_scores = [float(scores[0]) for scores in self.probabilities("gatekeeper", rows)]
        predictions = [Prediction(False, score) for score in gate_scores]
        indices = [i for i, score in enumerate(gate_scores) if score >= self.config.gatekeeper_threshold]
        if indices:
            scores = self.probabilities("multiclass", [rows[i] for i in indices])
            for i, probabilities in zip(indices, scores, strict=True):
                best = max(range(len(probabilities)), key=lambda j: probabilities[j])
                confidence = float(probabilities[best])
                label = self.labels["multiclass"][best]
                meta = self.policy[label]
                if confidence < self.config.multiclass_threshold:
                    label, meta = "UnknownAttack", {"category": "Unspecified", "severity": "Medium"}
                predictions[i] = Prediction(True, gate_scores[i], confidence,
                                            label, meta["category"], meta["severity"])
        return predictions


def load_models(config):
    return XGBoostModels(config)
