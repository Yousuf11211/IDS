"""Two-stage inference using the supplied XGBoost UBJ artifacts and JSON label maps."""
from dataclasses import dataclass
import hashlib
import json
import math

from .features import load_features, numeric
from .schema import CSV_TO_SQL, prepare_row


@dataclass(frozen=True)
class Prediction:
    attack: bool
    gate_score: float
    type_score: float | None = None
    attack_type: str | None = None
    category: str | None = None
    severity: str = "Medium"
    class_index: int | None = None
    selected_label: str | None = None
    top_probabilities: tuple = ()


def read_labels(path):
    def unique_keys(pairs):
        result = dict(pairs)
        if len(pairs) != len(result):
            raise ValueError("duplicate class ID")
        return result

    try:
        mapping = json.loads(path.read_text(encoding="utf-8-sig"), object_pairs_hook=unique_keys)
    except ValueError as error:
        raise ValueError(f"{path}: invalid JSON mapping: {error}") from error
    if not isinstance(mapping, dict) or not mapping or set(mapping) != {str(i) for i in range(len(mapping))}:
        raise ValueError(f"{path}: labels must be contiguous class IDs starting at zero")
    labels = {int(index): label for index, label in mapping.items()}
    if (any(not isinstance(label, str) or not label or label != label.strip() or len(label) > 100
            for label in labels.values()) or len(set(labels.values())) != len(labels)):
        raise ValueError(f"{path}: invalid or duplicate labels")
    return labels


class XGBoostModels:
    def __init__(self, config):
        import xgboost as xgb
        self.config = config
        self.loaded, self.labels = {}, {}
        self.feature_names = load_features(config)
        if missing := set(self.feature_names) - CSV_TO_SQL.keys():
            raise ValueError(f"SQL storage contract lacks model features: {sorted(missing)}")
        self.policy = json.loads((config.root / config.attack_policy).read_text(encoding="utf-8-sig"))
        contract = {"schema_version": 3, "features": self.feature_names, "policy": self.policy,
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
            try:
                model.load_model(bytearray(content))
            except xgb.core.XGBoostError as error:
                raise ValueError(f"{stage}: could not load UBJ model {artifact}: {error}") from error
            model.set_param({"device": "cpu", "nthread": 2})
            if model.num_features() != len(self.feature_names) or model.feature_names != self.feature_names:
                raise ValueError(f"{stage}: trained feature count, names or order differ from {config.feature_list}")
            labels = read_labels(label_path)
            learner = json.loads(model.save_config())["learner"]
            objective = learner["objective"]["name"]
            if stage == "gatekeeper":
                if (objective != "binary:logistic" or set(labels.values()) != {"Benign", "Attack"}
                        or int(learner["learner_model_param"]["num_target"]) != 1):
                    raise ValueError("Gatekeeper requires binary:logistic and a binary mapping for Benign and Attack")
                self.attack_index = next(index for index, label in labels.items() if label == "Attack")
            elif (objective != "multi:softprob"
                  or len(labels) != 15 or int(learner["learner_model_param"]["num_class"]) != len(labels)):
                raise ValueError("Multiclass must use multi:softprob and match the 15-class label mapping")
            self.loaded[stage], self.labels[stage] = model, labels
            contract[stage] = {"sha256": digest, "labels": labels}
            versions[stage] = stage + "-" + digest[:16]
        if set(self.policy) != set(self.labels["multiclass"].values()):
            raise ValueError("Attack policy must cover every multiclass label exactly")
        for label, meta in self.policy.items():
            if (not isinstance(meta, dict) or meta.get("severity") not in ("Low", "Medium", "High", "Critical")
                    or not isinstance(meta.get("category"), str) or not 0 < len(meta["category"].strip()) <= 50):
                raise ValueError(f"Invalid attack severity/category policy for {label}")
        self.fingerprint = hashlib.sha256(json.dumps(contract, sort_keys=True).encode()).hexdigest()
        self.release = "ubj-" + self.fingerprint[:20]
        self.gate_version, self.type_version = versions["gatekeeper"], versions["multiclass"]

    def input_values(self, stage, rows):
        """Single adapter shared by prediction and explanation; float32 is model input."""
        import numpy as np
        vectors = []
        for row in rows:
            values = prepare_row(row, self.feature_names)
            vectors.append([numeric(values[name]) for name in self.feature_names])
        data = np.asarray(vectors, dtype=np.float32).reshape(-1, len(self.feature_names))
        return data

    def input_matrix(self, stage, rows):
        import xgboost as xgb
        data = self.input_values(stage, rows)
        return data, xgb.DMatrix(data, feature_names=self.feature_names, nthread=2)

    def iteration_range(self, stage):
        best = self.loaded[stage].attr("best_iteration")
        return (0, int(best) + 1) if best is not None else (0, 0)

    def probabilities(self, stage, rows):
        import numpy as np
        model = self.loaded[stage]
        _, matrix = self.input_matrix(stage, rows)
        scores = model.predict(matrix, validate_features=True, strict_shape=True,
                               iteration_range=self.iteration_range(stage))
        expected = 1 if stage == "gatekeeper" else len(self.labels[stage])
        if (scores.shape != (len(rows), expected) or not np.isfinite(scores).all()
                or (scores < 0).any() or (scores > 1).any()):
            raise ValueError(f"{stage}: invalid model probabilities")
        if stage == "multiclass" and not np.allclose(scores.sum(axis=1), 1, atol=1e-5):
            raise ValueError("Multiclass probabilities do not sum to one")
        if stage == "gatekeeper" and self.attack_index == 0:
            scores = 1 - scores
        return scores

    def predict(self, rows):
        if not rows:
            return []
        gate_scores = [float(scores[0]) for scores in self.probabilities("gatekeeper", rows)]
        predictions = [Prediction(False, score) for score in gate_scores]
        indices = [i for i, score in enumerate(gate_scores) if score >= self.config.gatekeeper_threshold]
        if indices:
            scores = self.probabilities("multiclass", [rows[i] for i in indices])
            for i, probabilities in zip(indices, scores, strict=True):
                best = max(range(len(probabilities)), key=lambda j: probabilities[j])
                confidence = float(probabilities[best])
                label = selected_label = self.labels["multiclass"][best]
                meta = self.policy[label]
                if confidence < self.config.multiclass_threshold:
                    label, meta = "UnknownAttack", {"category": "Unspecified", "severity": "Medium"}
                predictions[i] = Prediction(True, gate_scores[i], confidence,
                                            label, meta["category"], meta["severity"], best, selected_label,
                                            tuple((self.labels["multiclass"][j], float(probabilities[j]))
                                                  for j in sorted(range(len(probabilities)),
                                                                  key=lambda j: -probabilities[j])[:3]))
        return predictions

    def explanation_context(self, stage, row, prediction):
        data = self.input_values(stage, [row])
        binary = stage == "gatekeeper"
        return {
            "schemaVersion": 1, "release": self.release, "fingerprint": self.fingerprint,
            "stage": stage, "modelVersion": self.gate_version if binary else self.type_version,
            "featureNames": self.feature_names,
            "featureValues": [None if math.isnan(v) else v for v in data[0].tolist()],
            "classIndex": self.attack_index if binary else prediction.class_index,
            "explainedClass": "Attack" if binary else prediction.selected_label,
            "predictedLabel": ("Attack" if prediction.attack else "Benign") if binary else prediction.attack_type,
            "probability": prediction.gate_score if binary else prediction.type_score,
            "threshold": self.config.gatekeeper_threshold if binary else self.config.multiclass_threshold,
            "topProbabilities": [] if binary else [dict(label=label, probability=value)
                                                    for label, value in prediction.top_probabilities],
            "adapterNote": "Ordered float32 inputs with native missing values and a validated handshake flag",
        }


def load_models(config):
    return XGBoostModels(config)
