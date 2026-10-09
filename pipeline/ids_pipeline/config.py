from dataclasses import dataclass
from pathlib import Path
import tomllib

ROOT = Path(__file__).resolve().parents[1]


@dataclass(frozen=True)
class Config:
    root: Path
    backend: str
    adapter: str
    batch_size: int = 500
    poll_seconds: float = 5
    max_attempts: int = 5
    retry_seconds: float = 10
    require_ready_marker: bool = True
    connection_env: str = "IDS_PIPELINE_CONNECTION_STRING"
    sqlite_path: str = "data/demo.sqlite3"
    manifest: str = "config/model-release.json"
    data_dir: str = "data"

    @classmethod
    def load(cls, path: Path):
        data = tomllib.loads(path.read_text(encoding="utf-8"))
        worker, db, models = data["worker"], data["database"], data["models"]
        config = cls(ROOT, db["backend"], models["adapter"], **worker,
                     connection_env=db.get("connection_env", "IDS_PIPELINE_CONNECTION_STRING"),
                     sqlite_path=db.get("sqlite_path", "data/demo.sqlite3"),
                     manifest=models.get("manifest", "config/model-release.json"))
        if config.backend not in ("sqlite", "sqlserver"):
            raise ValueError("database.backend must be sqlite or sqlserver")
        if config.adapter not in ("placeholder", "demo", "sklearn"):
            raise ValueError("models.adapter must be placeholder, demo or sklearn")
        if config.adapter == "demo" and config.backend != "sqlite":
            raise ValueError("Demo predictions may only be written to the isolated SQLite database")
        if not 1 <= config.batch_size <= 5000:
            raise ValueError("batch_size must be between 1 and 5000")
        if min(config.poll_seconds, config.max_attempts, config.retry_seconds) <= 0:
            raise ValueError("Polling, attempts and retry settings must be positive")
        for value in (config.data_dir, config.sqlite_path, config.manifest):
            if not (config.root / value).resolve().is_relative_to(config.root.resolve()):
                raise ValueError("Pipeline data, SQLite and manifest paths must stay inside pipeline/")
        return config
