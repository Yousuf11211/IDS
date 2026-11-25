# IDS detection pipeline

CSV files go into `data/incoming/`. The worker runs your gatekeeper and multiclass
UBJ models, then saves predictions to the web app's SQL Server database.

## What each folder is for

```text
pipeline/
  generate_csv.py       Generate synthetic CSV inputs
  requirements.txt      Python dependency versions
  README.md             Setup and usage
  config/               Settings, SQL column mapping and severity policies
  Features_used/        Authoritative ordered features_used.json
  models/               Binary and multiclass UBJs and JSON class mappings
  ids_pipeline/         Worker, validation, model loading, and database code
  database/             SQL setup for import tracking
  data/
    incoming/           Drop flow CSVs here
    processing/         Files currently being processed or awaiting retry
    archive/            Processed CSVs
    quarantine/         Files with invalid headers, encoding, or CSV structure
    duplicates/         Copies already imported
  tests/                Automated regression checks
  .venv/                Installed Python environment
```

`data/heartbeat.json` records the last worker scan. The archived CSVs belong to
saved imports and are kept for inspection and recovery.

## Setup

Use Windows Python 3.11+ on the machine running the web app. From PowerShell in
`C:\Projects\IDS\pipeline`:

```powershell
# Only if the environment does not exist yet:
python -m venv .venv

.\.venv\Scripts\Activate.ps1
python -m pip install -r requirements.txt
python -m ids_pipeline check-models
```

Apply the web application's EF migrations before starting either worker. For an
existing IDS database at `AddFlowExplanations`, stop the workers, review the current
migration history and generate the upgrade script from the solution directory:

```powershell
dotnet ef migrations list --project IDS --startup-project IDS
dotnet ef migrations script 20261016214443_AddFlowExplanations 20261122202918_PreserveFractionalSubflows --idempotent --project IDS --startup-project IDS --output IDS-flow-upgrade.sql
```

Verify the target database, take a backup and review the generated SQL. Apply the
reviewed script through your SQL deployment process, or apply this migration
explicitly to the configured target:

```powershell
dotnet ef database update 20261122202918_PreserveFractionalSubflows --project IDS --startup-project IDS
```

If the database is behind the stated baseline, generate and review the script
starting at its last applied migration. The historical blank-database bootstrap
limitation is documented in [the inspection guide](../IDS/Docs/ShapInspection.md).
The migration preserves historical values and indexes, permits SQL NULL for
numeric extractor measurements across the three flow tables, and adds one nullable
`bit` handshake flag per table. Raw rows must retain the flag across staging and
restart, before an attack's SHAP snapshots exist. Historical flags remain NULL. A subsequent migration widens the four subflow
packet/byte measurements to SQL float(53) across the same tables. It checks
that existing byte counts can be converted exactly before altering them.

After the migration, run the database preflight from `pipeline/`:

```powershell
# For installations that still need the pipeline metadata tables:
python -m ids_pipeline init-db
python -m ids_pipeline check
```

`init-db` adds only `PipelineImports` and `PipelineRows` to the same database.
It needs table-creation permission; the worker needs SELECT/INSERT/UPDATE access
on those tables and `RawPackets`, `Benign_Table`, and `Attack_Table`.

Run commands from `pipeline/`. The code runs directly from this folder; no
`pip install -e .` or generated `.egg-info` directory is needed. Python may
recreate `__pycache__` folders during execution; they are ignored by Git. Use
`python -B` instead of `python` if you want to avoid creating those caches.

## Run and generate traffic

Start the worker:

```powershell
python -m ids_pipeline run
```

In a separate terminal, run the independent explanation worker:

```powershell
python -m ids_pipeline explain
```

Detection commits freeze model inputs and enqueue per-stage work. Only the
explanation process runs SHAP. Admin/Support users can select **Inspect** beside
an attack in the live feed. Existing attacks require verified backfilling:

```powershell
python -m ids_pipeline backfill-explanations --limit 100
python -m ids_pipeline explain
```

Backfill can run while the explanation process stays running. Restart both workers
after updating Python code: an existing process retains its previously loaded code.
The explanation worker logs each completed attack and stage. Administrators control
automatic enqueueing for new detections in **Administration → Settings**. Existing
jobs continue processing and historical backfill remains an explicit operator command.
Stop the explanation process before
resetting failed explanations. After fixing a failure, use
`python -m ids_pipeline retry-explanations ATTACK_TABLE_ID` and restart it.

In another terminal, from `pipeline/` with the environment activated:

```powershell
python generate_csv.py --rows 100 --seed 42
```

The generator atomically drops a lowercase CSV and its `.csv.ready` marker into
`data/incoming/`. It generates inputs only; predictions and confidence scores come
from the real models. These synthetic detections persist in the database.

Open **Live Dashboard** (`/LiveDashboard`) or `/Dashboard` in the web app. The
existing monitor broadcasts detections about every two seconds and stats about
every ten seconds. The ASP.NET Core alert monitor reads persisted malicious flow
results and groups eligible events separately. See the [Phase 2 operations guide](../IDS/Docs/Phase2.md).

### Worker health

Apply the web migration `20261125180505_AddOperationsMonitoringAndInbox`
before starting updated workers. Both workers write their own SQL Server UTC
heartbeat while polling and reaching progress checkpoints. Idle workers remain
live. A graceful exit clears the running flag. A crash or stalled operation
becomes offline after the configured timeout. Dashboard and Live Feed read the
same health endpoint and refresh every five seconds while visible.

The default timeout is 120 seconds. Set a timeout in **Administration →
Settings** that exceeds the longest legitimate uninterrupted database or model
operation on your equipment. These settings apply to monitoring and new work.
Start and stop the Python processes in their own terminals.

For a single scan, use `python -m ids_pipeline run --once`.

## Connection and CSV requirements

- `config/pipeline.toml` points to `../IDS`. Connection settings follow the web
  app: appsettings, environment-specific settings, Development user secrets,
  `ConnectionStrings__DefaultConnection`, then `CONNECTION_STRING` (overridden by
  `IDS/.env` when defined). Use the same Windows account and environment as the
  app. The default is `localhost` / `IDS` with Windows authentication. A web-app
  command-line connection override must also be supplied through a shared
  environment variable.
- The environment comes from `DOTNET_ENVIRONMENT`, `ASPNETCORE_ENVIRONMENT`, or
  `database.environment` in the config. Installed Microsoft ODBC Driver 18 or 17
  is selected automatically. WSL/Linux requires its own drivers and authentication.
- The active feature contract is `Features_used/features_used.json`. Both boosters
  must declare those exact 120 unique names in that order. CSV headers are trimmed
  and lowercased. Existing PascalCase storage aliases still work. Duplicate names
  after normalization and missing required fields reject the file. Extra columns
  are accepted. Known storage fields and the original `label` are retained in raw
  rows. Only the ordered model features enter XGBoost.
- When the flag is absent, a case-insensitive `not a complete handshake` substring
  in `delta_start` produces `delta_start_incomplete_handshake=1`; other values
  produce 0. The marker becomes a missing measurement. Supplied flags must be
  numeric 0 or 1 and must be 1 when a marker is present. A supplied 1 remains
  valid when the marker was already removed upstream. The delta column is required.
- Blank or nonnumeric model measurements become NaN; either infinity becomes zero.
  Input to both boosters is float32. Missing inputs round-trip through SQL NULL
  and JSON null. Stored integer counters and ports retain the existing integer
  and range checks. A missing timestamp column uses the file's UTC modification
  time. Offset-bearing timestamps are converted to UTC. For timestamps without an offset, set
  `worker.source_timezone` to the exporter’s IANA zone and restart the detector.
  With an empty setting, or for ambiguous or nonexistent local times, supply an explicit offset. Absent non-model numeric columns
  retain the existing storage default of zero.
- The binary probability is interpreted using `binary_label_mapping.json`.
  At an Attack probability of 0.50 or higher, the same ordered inputs go to
  `multiclass.ubj`; its 15 probabilities are interpreted through
  `multiclass_label_mapping.json`. Benign confidence is one minus the Attack
  probability. For attacks the classified label and confidence come from the
  highest-scoring class under the default multiclass threshold of 0.0.
  `config/attack-policy-v2.json` defines severity and response grouping.
- The web app reads stored labels for the feed, type filter and statistics. The
  SHAP worker claims jobs for its own fingerprint. Completed historical explanations
  remain readable. For a detector rollback use the previous Python worker code
  (repository revision `0157e2c`), `config/pipeline.legacy.toml` and its original
  UBJs and TXT mappings from your release backup. Keep the updated web reader and
  the nullable schema when retaining new detections. Finish pinned imports and
  explanation jobs with the worker and exact artifacts for their release.

For a manual import, copy the file into `data/incoming/` with its **.csv** extension.
The default detector waits for unchanged size and modification time across scans
for at least four seconds. The generator supplies a separate empty `.csv.ready`
marker after closing the CSV, which bypasses the settling delay. If a producer can
pause mid-copy for longer than the settling interval, copy with a temporary
extension and rename to `.csv` on completion, or enable `require_ready_marker`
and have the producer create the empty marker. Always keep the data in the CSV.
Restart the detector after updating code or configuration.
Keep the data folders on the same filesystem and leave worker-owned files alone.

## Status, recovery and checks

```powershell
python -m ids_pipeline status
python -m ids_pipeline check-models
python -m unittest discover -s tests -v
```

The worker prevents concurrent writers, skips duplicate file content, and commits
results with their raw-record links and checkpoints. It retries transient failures
and resumes pending rows after restart. Invalid rows are recorded in
`PipelineRows.Error`; valid rows continue. Invalid file structure is quarantined.
Model files, mappings, thresholds and policy are fingerprinted; unfinished imports
require their original model release. `run --once` exits nonzero for unfinished
or rejected imports.

After fixing a failed import, stop the worker, run
`python -m ids_pipeline retry FULL_FILE_HASH`, then restart it. Database results are
durable; the existing live monitor is not a durable notification queue and reads
up to 100 rows per table per poll.
