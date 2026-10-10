import csv
from datetime import datetime, timezone
import hashlib
import json
import logging
import time
import uuid

from .schema import headers, validate

LOG = logging.getLogger(__name__)
TERMINAL = {"completed", "completed_with_errors", "rejected"}


def file_hash(path):
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


class Worker:
    def __init__(self, config, store, models):
        self.config, self.store, self.models = config, store, models
        self.data = config.root / config.data_dir
        for folder in ("incoming", "processing", "archive", "quarantine", "duplicates"):
            (self.data / folder).mkdir(parents=True, exist_ok=True)

    def move(self, path, folder):
        target = self.data / folder / path.name
        if target.exists():
            target = target.with_name(uuid.uuid4().hex + "--" + target.name[-180:])
        path.rename(target)
        return target

    def discover(self):
        for path in sorted((self.data / "incoming").glob("*.csv")):
            if path.is_symlink() or not path.is_file():
                continue
            marker = path.with_name(path.name + ".ready")
            if self.config.require_ready_marker and not marker.is_file():
                continue
            # Producers must not modify ready files. Rename claims ownership on same filesystem.
            target = self.data / "processing" / (uuid.uuid4().hex + "--" + path.name[-180:])
            path.rename(target)
            marker.unlink(missing_ok=True)

    def scan(self):
        self.discover()
        for path in sorted((self.data / "processing").glob("*.csv")):
            if path.is_symlink():
                continue
            self.process(path)

    def stage_file(self, path, digest, checkpoint):
        batch = []
        timestamp = datetime.fromtimestamp(path.stat().st_mtime, timezone.utc).replace(tzinfo=None)
        with path.open(encoding="utf-8-sig", newline="") as stream:
            reader = csv.reader(stream, strict=True)
            names = headers(next(reader, None))
            for number, values in enumerate(reader, start=1):
                if number <= checkpoint:
                    continue
                try:
                    if len(values) != len(names):
                        raise ValueError("CSV record has an incorrect column count")
                    features = validate(dict(zip(names, values, strict=True)), timestamp=timestamp)
                    batch.append((number, features, None))
                except ValueError as error:
                    batch.append((number, None, str(error)))
                if len(batch) >= self.config.batch_size:
                    self.store.stage(digest, batch)
                    batch = []
            if batch:
                self.store.stage(digest, batch)

    def process(self, path):
        digest = file_hash(path)
        job = self.store.register(digest, path.name, self.models)
        if job["Status"] in TERMINAL:
            # Recover a crash between completion commit and file movement.
            if job["FileName"] == path.name:
                self.move(path, "quarantine" if job["Status"] == "rejected" else "archive")
            else:
                self.move(path, "duplicates")
            return
        if job["Fingerprint"] != self.models.fingerprint:
            LOG.error("Import %s requires its original model release; leaving it pending", digest[:12])
            return
        if job["Status"] == "failed" or job["NextAttempt"] > time.time():
            return
        if job["FileName"] != path.name:
            # Only one copy owns a job, including across restarts.
            owner = self.data / "processing" / job["FileName"]
            if owner.exists():
                self.move(path, "duplicates")
                return
            raise RuntimeError("Import owner file is missing; restore it to processing with its recorded filename")
        try:
            if job["Status"] in ("staging", "retry_staging"):
                self.stage_file(path, digest, job["LastRow"])
                self.store.update(digest, Status="classifying", LastError=None)
            while rows := self.store.pending(digest, self.config.batch_size):
                predictions = self.models.predict(rows)
                self.store.complete_batch(digest, rows, predictions, self.models.release)
                LOG.info("Import %s committed %d predictions (%d benign, %d attacks)",
                         digest[:12], len(rows), sum(not p.attack for p in predictions),
                         sum(p.attack for p in predictions))
            counts = self.store.counts(digest)
            state = "completed_with_errors" if counts.get("rejected", 0) else "completed"
            self.store.update(digest, Status=state, LastError=None, NextAttempt=0)
            self.move(path, "archive")
            LOG.info("Import %s %s: %s", digest[:12], state, counts)
        except (csv.Error, UnicodeError, ValueError) as error:
            # Model output ValueErrors are retryable inference errors, not CSV schema failures.
            current = self.store.job(digest)
            if current["Status"] in ("staging", "retry_staging"):
                self.store.update(digest, Status="rejected", LastError=str(error)[:1000] if isinstance(error, ValueError) else "Invalid CSV encoding or quoting")
                self.move(path, "quarantine")
                LOG.error("Import %s rejected (%s); inspect CSV contract", digest[:12], type(error).__name__)
            else:
                self.record_failure(digest, error)
        except Exception as error:
            # If DB connectivity is lost, this update also fails and the process exits.
            # Restart obtains a fresh DB session/lock and resumes committed checkpoints.
            self.record_failure(digest, error)

    def record_failure(self, digest, error):
        job = self.store.job(digest)
        attempts = job["Attempts"] + 1
        state = "retry_staging" if job["Status"] in ("staging", "retry_staging") else "classifying"
        if attempts >= self.config.max_attempts:
            state = "failed"
        delay = min(300, self.config.retry_seconds * 2 ** min(attempts - 1, 10))
        # Driver exceptions may contain credentials or row contents: do not log their text.
        self.store.update(digest, Status=state, Attempts=attempts, NextAttempt=time.time()+delay,
                          LastError=f"{type(error).__name__}: processing failed; inspect configuration/model/database")
        LOG.error("Import %s failed (%s), attempt %d, state %s", digest[:12], type(error).__name__, attempts, state)

    def heartbeat(self):
        target = self.data / "heartbeat.json"
        temporary = target.with_suffix(".tmp")
        temporary.write_text(json.dumps({"last_scan_unix": time.time(), "release": self.models.release,
                                         "backend": "sqlserver"}) + "\n")
        temporary.replace(target)
