"""Insert explicitly synthetic records into the SQL Server dashboard tables.

Run: python pipeline/send_test_data.py
Requires IDS_PIPELINE_CONNECTION_STRING and the existing app database schema.
No CSV files, trained models or pipeline metadata tables are required.
"""
import argparse
from datetime import datetime, timezone
import math
from pathlib import Path
import sys
import time
import uuid

from ids_pipeline.config import Config, ROOT
from ids_pipeline.database import Store
from ids_pipeline.schema import COLUMNS, NAMES, validate


ATTACKS = (
    ("DoS", "DoS", "High", 80),
    ("PortScan", "Probe", "Medium", 8080),
    ("BruteForce", "Authentication", "High", 22),
    ("DDoS", "DoS", "Critical", 443),
    ("SQLInjection", "Web", "High", 80),
)


def make_features(index, attack):
    """Complete schema fixture; zeros are intentional synthetic test values."""
    row = {c["name"]: "" if c["type"].startswith("string") else "0" for c in COLUMNS}
    packets = 1000 + index if attack else 20 + index
    port = ATTACKS[index % len(ATTACKS)][3] if attack else (443, 80, 53)[index % 3]
    row.update(
        Timestamp=datetime.now(timezone.utc).isoformat(),
        SrcIp=f"{'198.51.100' if attack else '192.0.2'}.{index % 254 + 1}",
        DstPort=str(port), Duration="2.5", PacketsCount=str(packets),
        FwdPacketsCount=str(packets // 2), BwdPacketsCount=str(packets - packets // 2),
        TotalPayloadBytes=str(packets * 100), FwdTotalPayloadBytes=str((packets // 2) * 100),
        BwdTotalPayloadBytes=str((packets - packets // 2) * 100),
        PayloadBytesMax="100", PayloadBytesMean="100", BytesRate=str(packets * 40),
        PacketsRate=str(packets / 2.5), HandshakeState="TEST",
        Label="TEST synthetic input",
    )
    return validate(row)


def verify_dashboard_tables(store):
    required = {
        "RawPackets": ["Id", "IsProcessed", "Classification", "ClassifiedRecordId"],
        "Benign_Table": ["Id", "ConfidenceScore", "ModelVersion"],
        "Attack_Table": ["Id", "ConfidenceScore", "ModelVersion", "AttackType",
                         "AttackCategory", "Severity", "IsAcknowledged", "Notes"],
    }
    for table, extra in required.items():
        columns = ",".join(f"[{name}]" for name in NAMES + extra)
        store.conn.execute(f"SELECT {columns} FROM {store.table(table)} WHERE 1=0")
    store.conn.commit()


def write_record(store, index, attack, run_id):
    """Commit a raw record and its linked classification together."""
    features = make_features(index, attack)
    classification = "Attack" if attack else "Benign"
    with store.transaction():
        raw_id = store.insert_flow("RawPackets", features, IsProcessed=0)
        result_features = {**features, "Label": "TEST Benign"}
        extras = dict(ConfidenceScore=0.98, ModelVersion=run_id)
        table = "Benign_Table"
        if attack:
            attack_type, category, severity, _ = ATTACKS[index % len(ATTACKS)]
            result_features["Label"] = f"TEST {attack_type}"
            extras.update(ConfidenceScore=0.95, AttackType=f"TEST {attack_type}",
                          AttackCategory=category, Severity=severity, IsAcknowledged=0,
                          Notes=f"Synthetic dashboard test. Not a real detection. Run: {run_id}")
            table = "Attack_Table"
        result_id = store.insert_flow(table, result_features, **extras)
        store.conn.execute(f"""UPDATE {store.table('RawPackets')}
            SET IsProcessed=1, Classification=?, ClassifiedRecordId=? WHERE Id=?""",
            (classification, result_id, raw_id))
    return table, result_id, raw_id


def count_arg(value):
    number = int(value)
    if not 0 <= number <= 10000:
        raise argparse.ArgumentTypeError("Count must be between 0 and 10000")
    return number


def interval_arg(value):
    number = float(value)
    if not math.isfinite(number) or not 0 <= number <= 60:
        raise argparse.ArgumentTypeError("Interval must be between 0 and 60 seconds")
    return number


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--config", type=Path, default=ROOT / "config/pipeline.toml")
    parser.add_argument("--benign", type=count_arg, default=10, help="Benign record count (default: 10)")
    parser.add_argument("--attacks", type=count_arg, default=5, help="Attack record count (default: 5)")
    parser.add_argument("--interval", type=interval_arg, default=0.5,
                        help="Seconds between records, so live updates are visible (default: 0.5)")
    args = parser.parse_args(argv)
    if args.benign + args.attacks == 0:
        parser.error("Request at least one benign or attack record")
    config = Config.load(args.config)
    if config.backend != "sqlserver":
        parser.error("This script targets the website's SQL Server; use the CSV demo for SQLite")
    # Intentionally bypass model loading: this is an explicit database/UI test.
    run_id = "dashboard-test-" + uuid.uuid4().hex
    store = Store(config)
    benign_written = attacks_written = 0
    try:
        # Coordinate with the worker so concurrent identity allocation cannot confuse its UI monitor.
        store.acquire_lock()
        verify_dashboard_tables(store)
        print(f"Writing synthetic data to the configured IDS database. Run: {run_id}", flush=True)
        print("Open the web app's Live Dashboard. Every generated label begins with TEST.", flush=True)
        for index in range(max(args.benign, args.attacks)):
            for attack, count in ((False, args.benign), (True, args.attacks)):
                if index >= count:
                    continue
                if benign_written + attacks_written:
                    time.sleep(args.interval)
                table, result_id, raw_id = write_record(store, index, attack, run_id)
                if attack:
                    attacks_written += 1
                else:
                    benign_written += 1
                print(f"Saved {table} Id={result_id}, RawPackets Id={raw_id}", flush=True)
    finally:
        store.close()
        print(f"Committed: {benign_written} benign, {attacks_written} attacks. Run: {run_id}", flush=True)
    print("Finished. Allow a few seconds for live/statistics updates, or refresh the dashboard.")
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except KeyboardInterrupt:
        print("Stopped. Previously committed test records remain in the database.", file=sys.stderr)
        sys.exit(130)
    except (ValueError, RuntimeError, ImportError, FileNotFoundError) as error:
        print(f"Setup error: {error}", file=sys.stderr)
        sys.exit(1)
    except Exception as error:
        # ODBC exceptions can include credentials: never print their message.
        print(f"Database test failed ({type(error).__name__}). Check the ODBC driver, connection "
              "environment variable, permissions and existing app migrations. Committed rows "
              "remain; an interrupted record is rolled back.", file=sys.stderr)
        sys.exit(1)
