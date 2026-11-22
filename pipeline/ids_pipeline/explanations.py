"""Exact Tree SHAP of the deployed model, never a substitute prediction path."""
from datetime import datetime, timezone
import json
import logging
import time

import numpy as np


def select_output(values, expected, *, feature_count, class_count, class_index):
    """Accept documented pre/post SHAP 0.45 layouts; reject ambiguous layouts."""
    if isinstance(values, list):
        if len(values) != class_count:
            raise ValueError("Unexpected SHAP output count")
        vector = np.asarray(values[class_index])
        if vector.shape != (1, feature_count):
            raise ValueError(f"Unsupported SHAP list shape {vector.shape}")
        vector = vector[0]
    else:
        array = np.asarray(values)
        if class_count == 1 and array.shape == (1, feature_count):
            vector = array[0]
        elif array.shape == (1, feature_count, class_count):
            vector = array[0, :, class_index]
        else:
            raise ValueError(f"Unsupported SHAP array shape {array.shape}")
    base = np.asarray(expected).reshape(-1)
    if base.size != class_count:
        raise ValueError("Unexpected SHAP base value count")
    return vector.astype(float), float(base[class_index])


class ExplanationService:
    def __init__(self, models, reference=None):
        self.models, self.reference, self.explainers = models, reference, {}

    def generate(self, stage, context, *, attack_id=None, raw_id=None):
        import shap
        import xgboost
        started = time.perf_counter()
        model = self.models.loaded[stage]
        binary = stage == "gatekeeper"
        version = self.models.gate_version if binary else self.models.type_version
        if (context["stage"] != stage or context["modelVersion"] != version or
                context["fingerprint"] != self.models.fingerprint or
                context["release"] != self.models.release or
                context["featureNames"] != self.models.feature_names):
            raise ValueError("Original model release/feature order is unavailable")
        # Reconstruct only the immutable float32 snapshot made by the inference adapter.
        row = dict(zip(context["featureNames"], context["featureValues"], strict=True))
        data, matrix = self.models.input_matrix(stage, [row])
        if [None if np.isnan(v) else v for v in data[0].tolist()] != context["featureValues"]:
            raise ValueError("Stored input differs from inference input")
        output = 0 if binary else context["classIndex"]
        if binary:
            if context["explainedClass"] != "Attack" or context["classIndex"] != self.models.attack_index:
                raise ValueError("Binary output must explain the Attack log-odds")
        elif self.models.labels[stage].get(output) != context["explainedClass"]:
            raise ValueError("Selected class does not match the original label mapping")
        probabilities = self.models.probabilities(stage, [row])[0]
        if (not np.isclose(float(probabilities[output]), context["probability"], atol=1e-6, rtol=1e-5)
                or (not binary and int(np.argmax(probabilities)) != output)):
            raise ValueError("Original prediction cannot be reproduced")
        threshold = self.models.config.gatekeeper_threshold if binary else self.models.config.multiclass_threshold
        above_threshold = float(probabilities[output]) >= threshold
        expected_label = ("Attack" if above_threshold else "Benign") if binary else (
            context["explainedClass"] if above_threshold else "UnknownAttack")
        if context["predictedLabel"] != expected_label or context["threshold"] != threshold:
            raise ValueError("Stored decision does not match the original probability and threshold")
        if stage not in self.explainers:
            # Slicing preserves early-stopping semantics, including multiclass tree groups.
            end = self.models.iteration_range(stage)[1]
            explained_model = model[:end] if end else model
            self.explainers[stage] = shap.TreeExplainer(
                explained_model, model_output="raw", feature_perturbation="tree_path_dependent")
        explainer = self.explainers[stage]
        values = explainer.shap_values(matrix, check_additivity=True)
        contributions, base = select_output(values, explainer.expected_value,
            feature_count=data.shape[1], class_count=1 if binary else len(self.models.labels[stage]),
            class_index=output)
        margin = float(model.predict(matrix, output_margin=True, strict_shape=True,
                                    iteration_range=self.models.iteration_range(stage))[0, output])
        if binary and self.models.attack_index == 0:
            # XGBoost explains class 1. Class 0 log-odds have the opposite sign.
            contributions, base, margin = -contributions, -base, -margin
        reconstructed = base + float(contributions.sum())
        if (not np.isfinite(contributions).all() or not np.isfinite(base) or
                not np.isclose(reconstructed, margin, atol=2e-4, rtol=2e-5)):
            raise ValueError(f"SHAP additivity failed: reconstructed={reconstructed}, margin={margin}")
        profile = self.reference
        if profile and profile["modelFingerprint"] != self.models.fingerprint:
            profile = None
        features = []
        for name, value, contribution in zip(context["featureNames"], context["featureValues"], contributions, strict=True):
            reference = profile["features"].get(name) if profile else None
            features.append(dict(name=name, value=value, contribution=float(contribution),
                                 units="model input units", reference=reference))
        return {**context, "attackId": str(attack_id) if attack_id is not None else None,
                "rawId": str(raw_id) if raw_id is not None else None,
                "stage": stage, "modelName": "Binary XGBoost" if binary else "Multiclass XGBoost",
                "modelVersion": version,
                "outputUnits": "log-odds" if binary else "raw class margin",
                "baseValue": base, "rawMargin": margin, "reconstructedMargin": reconstructed,
                "additivityError": abs(margin-reconstructed), "features": features,
                "reference": {k: v for k, v in profile.items() if k != "features"} if profile else None,
                "generatedAt": datetime.now(timezone.utc).isoformat(),
                "generationMs": (time.perf_counter()-started)*1000,
                "shapVersion": shap.__version__, "xgboostVersion": xgboost.__version__}


class ExplanationWorker:
    """Independent process, SQL durable queue; no HTTP or detector CPU work."""
    def __init__(self, store, models, service):
        self.store, self.models, self.service = store, models, service

    def once(self):
        job = self.store.claim_explanation(self.models.fingerprint)
        if job is None:
            return False
        try:
            context = json.loads(job["ContextJson"])
            if (job["ModelVersion"] != context["modelVersion"] or
                    context["attackId"] != str(job["AttackTrafficId"]) or
                    context["rawId"] != str(job["RawPacketId"])):
                raise ValueError("Queued model or detection identifiers differ from the prediction snapshot")
            payload = self.service.generate(job["Stage"], context, attack_id=job["AttackTrafficId"],
                                            raw_id=job["RawPacketId"])
            self.store.finish_explanation(job["Id"], payload)
            logging.getLogger(__name__).info("SHAP completed for attack %s, stage %s (%.1f ms)",
                job["AttackTrafficId"], job["Stage"], payload["generationMs"])
        except Exception as error:
            # Explainability failure never rolls back the original incident.
            self.store.fail_explanation(job["Id"], error)
        return True
