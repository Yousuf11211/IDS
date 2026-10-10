from dataclasses import dataclass
from pathlib import Path
import math
import tomllib

ROOT = Path(__file__).resolve().parents[1]


@dataclass(frozen=True)
class Config:
    root: Path = ROOT
    batch_size: int = 500
    poll_seconds: float = 2
    max_attempts: int = 5
    retry_seconds: float = 10
    require_ready_marker: bool = True
    data_dir: str = "data"
    webapp_dir: str = "../IDS"
    environment: str = "Development"
    odbc_driver: str = "auto"
    gatekeeper: str = "models/gatekeeper/gatekeeper_combined_full_features.ubj"
    gatekeeper_labels: str = "models/gatekeeper/gatekeeper_label_mapping.txt"
    multiclass: str = "models/multiclass/attack_multiclass_full_features.ubj"
    multiclass_labels: str = "models/multiclass/attack_multiclass_label_mapping.txt"
    attack_policy: str = "config/attack-policy.json"
    gatekeeper_threshold: float = 0.5
    multiclass_threshold: float = 0.0

    @classmethod
    def load(cls, path: Path):
        data = tomllib.loads(path.read_text(encoding="utf-8-sig"))
        config = cls(**data.get("worker", {}), **data.get("database", {}),
                     **data.get("models", {}))
        if not 1 <= config.batch_size <= 5000:
            raise ValueError("batch_size must be between 1 and 5000")
        for value in (config.poll_seconds, config.max_attempts, config.retry_seconds):
            if not math.isfinite(value) or value <= 0:
                raise ValueError("Polling, attempts and retry settings must be positive and finite")
        if not 0 < config.gatekeeper_threshold < 1 or not 0 <= config.multiclass_threshold <= 1:
            raise ValueError("Invalid model confidence thresholds")
        for value in (config.data_dir, config.gatekeeper, config.multiclass,
                      config.gatekeeper_labels, config.multiclass_labels, config.attack_policy):
            if not (config.root / value).resolve().is_relative_to(config.root.resolve()):
                raise ValueError("Data and model paths must stay inside pipeline/")
        return config
