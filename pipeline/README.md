# IDS detection pipeline

CSV files go into `data/incoming/`. The worker runs your gatekeeper and multiclass
UBJ models, then saves predictions to the web app's SQL Server database.

## What each folder is for

```text
pipeline/
  generate_csv.py       Generate synthetic CSV inputs
  requirements.txt      Python dependency versions
  README.md             Setup and usage
  config/               Settings, required features, SQL column mapping, severity policy
  models/               Your UBJ models and TXT label mappings
  ids_pipeline/         Worker, validation, model loading, and database code
  database/             SQL setup for import tracking
  data/
    incoming/           Drop completed CSVs and their .ready markers here
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
python -m ids_pipeline init-db
python -m ids_pipeline check
```

Start the web app first so its EF migrations create the detection tables.
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

In another terminal, from `pipeline/` with the environment activated:

```powershell
python generate_csv.py --rows 100 --seed 42
```

The generator atomically drops a lowercase CSV and its `.csv.ready` marker into
`data/incoming/`. It generates inputs only; predictions and confidence scores come
from the real models. These synthetic detections persist in the database.

Open **Live Dashboard** (`/LiveDashboard`) or `/Dashboard` in the web app. The
existing monitor broadcasts detections about every two seconds and stats about
every ten seconds. The separate administrative `SecurityAlerts` table is not part
of this flow.

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
- The required **119 numeric CSV fields** are in `config/model-features.json`.
  Headers are normalized to lowercase snake_case; uppercase, PascalCase and
  reordered columns are accepted. Missing features, ambiguous/unknown headers,
  blanks and invalid numeric values are rejected. `label` is optional source
  metadata and is never supplied to a model.
- Both supplied models declare one extra column,
  `delta_start_incomplete_handshake`, which neither uses in any tree split. The
  adapter supplies zero only after checking that it remains unused. A future
  model using it will fail startup until its training transformation is provided.
- `timestamp` and `src_ip` are optional. Missing timestamps use the CSV's UTC
  modification time; missing IPs use an empty string. Supplied timestamps need
  a timezone. Absent non-model numeric storage columns default to zero; missing
  model inputs never default. Additional fields from `config/flow-schema.json`
  are accepted and preserved.
- Raw labels remain unchanged. Classified labels come from the TXT mappings.
  Benign confidence is `1 - gatekeeper attack probability`; attack confidence is
  the multiclass probability. Severity/category come from `config/attack-policy.json`.
  Thresholds are configured in `config/pipeline.toml`.

For real CSVs, finish copying into `data/incoming/`, then create an empty marker
named `filename.csv.ready`. The generator does this automatically. If markers are
disabled in config, write to a temporary extension and atomically rename to `.csv`.
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
