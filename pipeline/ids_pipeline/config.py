from dataclasses import dataclass
from pathlib import Path
from zoneinfo import ZoneInfo, ZoneInfoNotFoundError
import math
import tomllib

ROOT = Path(__file__).resolve().parents[1]
MODEL_DEFAULTS = tomllib.loads((ROOT / "config/pipeline.toml").read_text(encoding="utf-8-sig"))["models"]


@dataclass(frozen=True)
class Config:
    root: Path = ROOT
    batch_size: int = 500
    poll_seconds: float = 2
    max_attempts: int = 5
    retry_seconds: float = 10
    require_ready_marker: bool = False
    file_settle_seconds: float = 4
    source_timezone: str = ""
    data_dir: str = "data"
    webapp_dir: str = "../IDS"
    environment: str = "Development"
    odbc_driver: str = "auto"
    gatekeeper: str = MODEL_DEFAULTS["gatekeeper"]
    gatekeeper_labels: str = MODEL_DEFAULTS["gatekeeper_labels"]
    multiclass: str = MODEL_DEFAULTS["multiclass"]
    multiclass_labels: str = MODEL_DEFAULTS["multiclass_labels"]
    feature_list: str = MODEL_DEFAULTS["feature_list"]
    attack_policy: str = MODEL_DEFAULTS["attack_policy"]
    gatekeeper_threshold: float = MODEL_DEFAULTS["gatekeeper_threshold"]
    multiclass_threshold: float = MODEL_DEFAULTS["multiclass_threshold"]
    reference_profile: str = ""

    def timestamp_zone(self):
        if not self.source_timezone:
            return None
        try:
            return ZoneInfo(self.source_timezone)
        except (ValueError, ZoneInfoNotFoundError) as error:
            raise ValueError(f"Invalid worker.source_timezone: {self.source_timezone}") from error

    @classmethod
    def load(cls, path: Path):
        data = tomllib.loads(path.read_text(encoding="utf-8-sig"))
        config = cls(**data.get("worker", {}), **data.get("database", {}),
                     **data.get("models", {}))
        if not 1 <= config.batch_size <= 5000:
            raise ValueError("batch_size must be between 1 and 5000")
        for value in (config.poll_seconds, config.max_attempts, config.retry_seconds, config.file_settle_seconds):
            if not math.isfinite(value) or value <= 0:
                raise ValueError("Polling, attempts, retry and file settling settings must be positive and finite")
        if not 0 < config.gatekeeper_threshold < 1 or not 0 <= config.multiclass_threshold <= 1:
            raise ValueError("Invalid model confidence thresholds")
        for value in (config.data_dir, config.gatekeeper, config.multiclass,
                      config.gatekeeper_labels, config.multiclass_labels, config.attack_policy,
                      config.feature_list, config.reference_profile):
            if not (config.root / value).resolve().is_relative_to(config.root.resolve()):
                raise ValueError("Data and model paths must stay inside pipeline/")
        return config
