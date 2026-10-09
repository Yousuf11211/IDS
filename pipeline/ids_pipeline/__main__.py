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
    sub.add_parser("init-db", help="Create pipeline metadata (and isolated demo tables for SQLite)")
    sub.add_parser("check", help="Verify schema and load/validate models without consuming files")
    sub.add_parser("status", help="Show import progress")
    retry = sub.add_parser("retry", help="Reset a failed job after fixing its cause")
    retry.add_argument("file_hash")
    run = sub.add_parser("run", help="Watch incoming/ and resume processing/")
    run.add_argument("--once", action="store_true", help="One scan; exit nonzero if incomplete imports remain")
    args = parser.parse_args(argv)
    logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(message)s")
    config = Config.load(args.config)
    # Fail before DB access/file movement when production model setup is missing.
    models = load_models(config) if args.command in ("run", "check", "retry") else None
    store = Store(config)
    try:
        if args.command != "status":
            store.acquire_lock()
        if args.command == "init-db":
            store.initialize()
            store.verify()
            print("Database schema initialized and verified")
            return 0
        store.verify()
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
        stopped = threading.Event()
        for sig in (signal.SIGINT, signal.SIGTERM):
            signal.signal(sig, lambda *_: stopped.set())
        while not stopped.is_set():
            worker.scan()
            worker.heartbeat()
            if args.once:
                incomplete = store.fetch(f"SELECT FileHash FROM {store.table('PipelineImports')} "
                                         "WHERE Status NOT IN ('completed','completed_with_errors')")
                return 1 if incomplete else 0
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
