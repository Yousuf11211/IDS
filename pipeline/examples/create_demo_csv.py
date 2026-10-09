"""Generate schema-complete synthetic fixtures, ONLY for config/demo.toml."""
import csv
from datetime import datetime, timezone
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
columns = json.loads((ROOT / "config/flow-schema.json").read_text())["columns"]
target = ROOT / "data/demo/incoming/demo.csv"
target.parent.mkdir(parents=True, exist_ok=True)
if target.exists() or target.with_suffix(".csv.ready").exists():
    raise SystemExit("demo.csv already exists; process it first")
with target.open("w", newline="", encoding="utf-8") as stream:
    writer = csv.DictWriter(stream, fieldnames=[c["name"] for c in columns])
    writer.writeheader()
    for port in (443, 22, 80):
        row = {c["name"]: "" if c["type"].startswith("string") else 0 for c in columns}
        row.update(Timestamp=datetime.now(timezone.utc).isoformat(), SrcIp="192.0.2.10", DstPort=port,
                   Label="SyntheticFixture")
        writer.writerow(row)
target.with_suffix(".csv.ready").touch()
print(f"Created {target}: expected 2 benign and 1 DemoAttack in isolated SQLite")
