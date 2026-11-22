import argparse
import logging
from pathlib import Path
import signal
import sys
import threading

from .config import Config, ROOT
from .database import Store
from .models import load_models
from .worker import Worker


def main(argv=None):
    parser = argparse.ArgumentParser(description="IDS CSV pipeline")
    parser.add_argument("--config", type=Path, default=ROOT / "config/pipeline.toml")
    sub = parser.add_subparsers(dest="command", required=True)
    sub.add_parser("init-db", help="Create pipeline metadata in the web app database")
    sub.add_parser("check", help="Verify schema and load/validate models without consuming files")
    sub.add_parser("check-models", help="Validate both UBJ models without opening the database")
    sub.add_parser("status", help="Show import progress")
    retry = sub.add_parser("retry", help="Reset a failed job after fixing its cause")
    retry.add_argument("file_hash")
    run = sub.add_parser("run", help="Watch incoming/ and resume processing/")
    run.add_argument("--once", action="store_true", help="Scan incoming files, allow one settling interval, then exit")
    explain = sub.add_parser("explain", help="Run the independent durable SHAP worker")
    explain.add_argument("--once", action="store_true", help="Drain currently eligible jobs and exit")
    backfill = sub.add_parser("backfill-explanations", help="Queue reproducible historical pipeline attacks")
    backfill.add_argument("--limit", type=int, default=100)
    retry_explain = sub.add_parser("retry-explanations", help="Retry failed stages for an attack using its original model")
    retry_explain.add_argument("attack_id", type=int)
    args = parser.parse_args(argv)
    logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(message)s")
    config = Config.load(args.config)
    # Fail before DB access/file movement when production model setup is missing.
    models = load_models(config) if args.command in ("run", "check", "check-models", "retry",
                                                  "explain", "backfill-explanations", "retry-explanations") else None
    if args.command == "check-models":
        print(f"Model release {models.release} verified: {len(models.feature_names)} ordered features, {len(models.labels['gatekeeper'])} binary / {len(models.labels['multiclass'])} attack labels")
        return 0
    store = Store(config)
    try:
        if args.command in ("init-db", "run", "retry"):
            store.acquire_lock()
        if args.command == "init-db":
            store.initialize()
            store.verify()
            print("Database schema initialized and verified")
            return 0
        store.verify()
        if args.command in ("explain", "backfill-explanations", "retry-explanations"):
            if args.command == "backfill-explanations":
                if not 1 <= args.limit <= 10000:
                    raise ValueError("Backfill limit must be between 1 and 10000")
                print(f"Queued {store.backfill_explanations(models, args.limit)} verified historical attacks")
                return 0
            # Backfill only inserts idempotent jobs and is safe while the worker
            # consumes them. Claim/recovery and failed-job resets need exclusivity.
            store.acquire_explanation_lock()
            if args.command == "retry-explanations":
                if args.attack_id <= 0:
                    raise ValueError("Attack ID must be positive")
                store.retry_explanations(models.fingerprint, args.attack_id)
                return 0
            from .explanations import ExplanationService, ExplanationWorker
            from .reference import load_reference
            explanation_worker = ExplanationWorker(store, models, ExplanationService(models, load_reference(config)))
            logging.info("SHAP worker ready for %s; waiting for queued explanations", models.release)
            logging.info("Restart the detection worker after installing code updates so it queues SHAP jobs")
            stopped = threading.Event()
            for sig in (signal.SIGINT, signal.SIGTERM):
                signal.signal(sig, lambda *_: stopped.set())
            while not stopped.is_set():
                worked = explanation_worker.once()
                if not worked:
                    if args.once:
                        return 0
                    stopped.wait(config.poll_seconds)
            return 0
        if args.command == "check":
            print(f"Database and model release {models.release} verified")
            return 0
        if args.command == "status":
            for job in store.fetch(f"SELECT FileHash,Status,LastRow,Attempts,LastError FROM {store.table('PipelineImports')} ORDER BY CreatedAt"):
                print(job, store.counts(job["FileHash"]))
            return 0
        if args.command == "retry":
            job = store.job(args.file_hash)
            if not job or job["Status"] != "failed":
                raise ValueError("retry requires the full hash of a failed import")
            if job["Fingerprint"] != models.fingerprint:
                raise ValueError("Restore the original model release before retrying")
            # Re-read through durable staging checkpoint, then resume pending inference.
            store.update(args.file_hash, Status="retry_staging", Attempts=0, NextAttempt=0, LastError=None)
            return 0
        worker = Worker(config, store, models)
        logging.info("Detection worker ready for %s; SHAP job enqueueing is enabled", models.release)
        logging.info("Watching %s (%s)", worker.data / "incoming",
                     "empty .csv.ready marker required" if config.require_ready_marker else
                     f"plain CSVs accepted after {config.file_settle_seconds:g}s stable")
        stopped = threading.Event()
        for sig in (signal.SIGINT, signal.SIGTERM):
            signal.signal(sig, lambda *_: stopped.set())
        while not stopped.is_set():
            worker.scan()
            if args.once and worker.observed_files:
                if stopped.wait(config.file_settle_seconds):
                    return 1
                worker.scan()
            worker.heartbeat()
            if args.once:
                incomplete = store.fetch(f"SELECT FileHash FROM {store.table('PipelineImports')} "
                                         "WHERE Status NOT IN ('completed','completed_with_errors')")
                return 1 if incomplete or worker.observed_files else 0
            stopped.wait(config.poll_seconds)
        return 0
    finally:
        store.close()


if __name__ == "__main__":
    try:
        sys.exit(main())
    except KeyboardInterrupt:
        sys.exit(130)
    except Exception as error:
        # Configuration errors have controlled messages; avoid printing driver secrets.
        if isinstance(error, (ValueError, RuntimeError, FileNotFoundError, ImportError)):
            logging.error("%s", error)
        else:
            logging.error("Pipeline stopped (%s). Check database connectivity and configuration.", type(error).__name__)
        sys.exit(1)
