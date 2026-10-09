# IDS CSV detection pipeline

Independent Python 3.11+ worker for the existing ASP.NET IDS application. Everything
new lives here: source, configuration, database setup, model slots, CSV folders,
tests and demo. Run commands below **from this `pipeline/` directory**.

No trained models are included. Default configuration fails closed until both
models are configured. Nothing writes to SQL Server until you supply its connection
string and explicitly run setup/the worker. The SQLite demo never connects to IDS.

## Layout

```text
pipeline/
  ids_pipeline/             CSV validation, two-stage inference, persistence, worker, CLI
  config/
    pipeline.toml           Production defaults (placeholder models)
    demo.toml               Isolated SQLite + synthetic predictions
    flow-schema.json        All 172 existing NetworkFlowBase fields and types
    model-release.example.json
  database/001_pipeline.sql PipelineImports and PipelineRows metadata tables
  models/gatekeeper/        Put trusted model 1 artifact here
  models/multiclass/        Put trusted model 2 artifact here
  data/incoming/            Drop CSV and its .ready marker here
  data/processing/          Worker-owned files; retain for recovery
  data/archive/             Completed files, including files with rejected rows
  data/quarantine/          Structurally invalid CSV files
  data/duplicates/          Duplicate file copies, retained for inspection
  examples/                Synthetic demo CSV generator
  tests/                   Standard-library automated tests
```

## Try the complete workflow without SQL Server or trained models

```bash
python -m ids_pipeline --config config/demo.toml init-db
python examples/create_demo_csv.py
python -m ids_pipeline --config config/demo.toml run --once
python -m ids_pipeline --config config/demo.toml status
```

Use `python3` if that is your Python command. No dependencies are needed for the
demo. Expected result: three raw records, two benign records and one `DemoAttack`
in `data/demo/demo.sqlite3`. Demo CSV folders are separately under `data/demo/`, so
testing cannot consume the production incoming folder. The demo rule checks port 22: it is
NOT a security model. The website does not read this demo database.

For continuous watching, omit `--once`. Generated inputs, database, credentials,
local config and model artifacts are ignored by Git.

## Connect to the website's SQL Server

1. Apply the web application's existing EF migrations first. The production
   worker expects `dbo.RawPackets`, `dbo.Benign_Table` and `dbo.Attack_Table`.
2. Create a virtual environment here and install the SQL Server dependency:

   ```bash
   python -m venv .venv
   # Linux/WSL: source .venv/bin/activate
   # PowerShell: .venv\Scripts\Activate.ps1
   python -m pip install -e '.[sqlserver]'
   ```

   Install Microsoft's ODBC Driver 18 for SQL Server on the worker host separately.
3. Set `IDS_PIPELINE_CONNECTION_STRING` in the worker environment. It must target
   the same server/database as the app, but use ODBC syntax, not its .NET connection
   string verbatim. For example, with integrated authentication configured:

   ```text
   Driver={ODBC Driver 18 for SQL Server};Server=YOUR_SERVER;Database=YOUR_IDS_DATABASE;Trusted_Connection=yes;Encrypt=yes;TrustServerCertificate=no;
   ```

   Bash: `export IDS_PIPELINE_CONNECTION_STRING='...'`

   PowerShell: `$env:IDS_PIPELINE_CONNECTION_STRING = '...'`

   SQL authentication can use `UID`/`PWD` supplied through your service's secret
   environment. Do not commit credentials. No `.env` file is loaded automatically.
   Integrated authentication on Linux requires its own Kerberos configuration.
4. With a schema-deployment identity, run:

   ```bash
   python -m ids_pipeline init-db
   ```

   This applies `database/001_pipeline.sql` and verifies expected columns. It creates
   only pipeline metadata; it never changes the app's tables or runs EF migrations.
   Keep these pipeline-owned migrations here; coordinate any future changes to app
   tables through the .NET migrations. Setup is idempotent, not an automatic schema
   upgrade mechanism.
5. Run the worker under a separate identity with SELECT/INSERT/UPDATE on the five
   detection/pipeline tables and permission to acquire the database application
   lock. It needs no access to Identity, chat or administration tables. Schema
   creation rights are needed only for setup.

## Send dummy records to the actual web dashboard

`send_test_data.py` is a separate, explicit SQL Server/UI test. It does not require
trained models, CSV files or the two pipeline metadata tables. It uses the existing
ODBC connection environment variable and writes linked `RawPackets`, `Benign_Table`
and `Attack_Table` records with current timestamps. It does not create tables.

After installing `.[sqlserver]` and setting `IDS_PIPELINE_CONNECTION_STRING` as
described above, start the web app and open its Live Dashboard. Allow the app's
background monitor to start (about five seconds). Stop the regular Python worker
while using this script; both use the same exclusive database lock.

From `C:\Projects\IDS\pipeline` in PowerShell:

```powershell
python send_test_data.py
# Or choose the counts and pacing:
python send_test_data.py --benign 20 --attacks 10 --interval 1
# Optional custom connection configuration (models are not loaded):
python send_test_data.py --config config/local.toml
```

Default: 10 benign and 5 attack records, interleaved at half-second intervals.
Attack examples include DoS, PortScan, BruteForce, DDoS and SQLInjection, with
different severities. Every result label/attack type begins with `TEST`, and
ModelVersion contains a unique `dashboard-test-...` run identifier printed by the
script. The data is synthetic, including its confidence scores and zero-filled
unused features. This tests database/display behavior, not model accuracy.

Records persist and contribute to dashboard counts; each new invocation adds a new
set. The script prints IDs and committed counts. It never clears existing data or
automatically retries a failed run. Allow about 2 seconds for detection events and
10 seconds for statistics, or refresh the page. The separate admin Alerts page
uses `SecurityAlerts` and is not populated by this test. SQLite/demo configuration
is deliberately rejected here, because it would not update the website.

## Connect both real models

1. Place trusted artifacts under `models/gatekeeper/` and `models/multiclass/`.
2. Each joblib artifact must contain its fitted preprocessing + classifier and
   expose `predict_proba` and `classes_`. Different feature lists/preprocessing for
   the two stages are supported. The gatekeeper has exactly two classes; the
   multiclass model has attack classes only.
3. Copy `config/model-release.example.json` to `config/model-release.json`. Replace
   every example value with the training contract: feature order, class labels,
   versions, artifact SHA-256 digests, thresholds and severity mapping. The two
   example features are illustrations, not the correct features for your model.
4. Copy `config/pipeline.toml` to `config/local.toml`, set `models.adapter` to
   `"sklearn"`. Install `.[sklearn]` using dependency constraints matching the
   exact training environment. The optional dependency ranges are compatibility
   bounds, NOT a reproducible model environment; retain your tested lock/constraints
   file under this folder before deployment.
5. Verify and start:

   ```bash
   python -m ids_pipeline --config config/local.toml check
   python -m ids_pipeline --config config/local.toml run
   ```

Models load once at startup. The release manifest is fingerprinted and pinned per
import. Changing thresholds, feature lists or model digests prevents unfinished
imports from silently resuming under a different release. Finish imports before
switching releases, or restore their original manifest/artifacts to resume.
ModelVersion on result tables contains the release ID; PipelineImports holds both
model versions. PipelineRows holds both stage scores. GateScore always means
probability of attack; the existing benign ConfidenceScore means probability of
benign, and attack ConfidenceScore means the conditional attack-type confidence.
These are not interchangeable or automatically calibrated probabilities.

Low multiclass confidence produces `UnknownAttack`; technical model failures leave
rows pending and retry. Severity comes from the explicit label policy, not a claim
that probability alone measures harm. Tune thresholds on held-out data and measure
end-to-end false negatives and per-class recall before using real predictions.

Joblib can execute code when loading artifacts. Load only artifacts you trust;
checksums detect mismatches, not whether a model author is trustworthy. Use the
same dependency versions as training. See [scikit-learn model persistence](https://scikit-learn.org/stable/model_persistence.html).
For another model format, implement the same batch `predict(rows)` interface in
`ids_pipeline/models.py` and explicitly register an adapter; do not change database
or folder-processing logic.

## Drop CSV files

1. Copy `traffic.csv` into `data/incoming/` and finish/close the copy.
2. Create an empty `traffic.csv.ready` alongside it **after** copying completes.
   Bash: `touch data/incoming/traffic.csv.ready`.
   PowerShell: `New-Item data/incoming/traffic.csv.ready -ItemType File`.
3. The next scan claims it and stages validated rows in bounded batches, then runs
   gatekeeper inference followed by multiclass inference for attack candidates.
4. Committed results appear in the existing website's detection tables.

Keep incoming/processing/archive on the same local filesystem. Do not modify a
ready or worker-owned file. If you disable ready markers, the producer MUST copy
to a temporary extension and atomically rename to `.csv`; plain drag/copy is not
safe in that mode. Periodic scans recover missed folder events. CSV records are
numbered starting at 1 after the header, including quoted multiline records.

The schema requires every column in `flow-schema.json` except `Label` (optional).
PascalCase and snake_case headers are accepted; order may differ. Duplicate or
unknown headers fail validation. Supply UTF-8 CSV and timestamps with UTC `Z` or
an explicit offset. Missing/nonfinite numeric values are rejected, never replaced
with zeros. Valid numeric sentinel values are not globally forbidden: further
domain/range policy must match your training feature extractor.

Raw Label preserves any source label. Model feature lists forbid Label. Result
Label is always the prediction. Event timestamps stay original and UTC; processing
time is separately recorded, so historical imports do not look like live events.

## Recovery and operating behavior

- One active worker per database is enforced using a session-owned SQL Server
  application lock (OS file lock for local SQLite). Keep one writer initially.
  A SQL disconnect stops the process rather than reconnecting without its lock.
  Use your service supervisor to restart it with backoff. The SQL lock releases
  when its session ends; see [Microsoft's sp_getapplock documentation](https://learn.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-getapplock-transact-sql).
- Input SHA-256 identifies an import. Duplicate content under another filename is
  retained in duplicates/, not classified twice. Identical content intentionally
  reprocessed under a new release requires a future explicit reprocessing workflow;
  changing the active release does not bypass deduplication.
- Raw staging and CSV checkpoints commit together. A restart re-reads the CSV to
  the last committed record. Large files therefore cost a sequential re-read on
  recovery, but memory remains bounded by batch size.
- Result insertion, raw classification linkage and row completion commit in one
  transaction per batch. Retries operate only on pending rows. Inference happens
  outside transactions. Inserts currently use parameterized per-row statements
  within the batch transaction, not SQL bulk copy; benchmark before higher volumes.
- Invalid row values are recorded in PipelineRows with their row number/reason;
  valid rows continue. The original archived CSV provides rejected input evidence.
  A file with rejected rows ends as `completed_with_errors`.
- Invalid encoding, headers or CSV quoting quarantine the entire file. If parsing
  failed after staged batches, those raw rows remain unprocessed under a rejected
  import for inspection; no results are inferred for that file. Correct the file
  and submit it as a new import. Do not manually change raw completion flags.
- Processing exceptions retry with exponential backoff, capped at five attempts
  by default. Failed imports stay in processing/. The original files and pending
  rows remain available. After fixing the cause, run:

  ```bash
  # Stop the worker first; retry also obtains its exclusive database lock.
  python -m ids_pipeline --config config/local.toml status
  python -m ids_pipeline --config config/local.toml retry FULL_FILE_HASH
  ```

- `status` can run alongside the worker; mutations such as `retry` require stopping
  the worker first.
- `run --once` performs one scan; scheduled retries wait for a later scan. It exits
  nonzero if rejected, failed or unfinished imports remain. Normal continuous mode
  keeps scanning while individual imports fail. Monitor status rather than treating
  process liveness alone as successful detection.
- Logs show import IDs/counts and exception types, never connection strings or raw
  row contents. data/heartbeat.json records the last completed scan. During a large
  import it does not advance: use logs and PipelineImports.UpdatedAt for progress.
- Handle shutdown at file boundaries. Allow sufficient service stop grace time for
  large imports; a forced stop is recovered from committed database checkpoints.

Run as a dedicated systemd service, Windows service or supervised container, with
working directory set here, persistent storage for data/, read-only model files,
environment-injected credentials and restart-on-failure. Set retention/backups for
raw records, files and metadata together; deleting deduplication metadata allows
old content to be imported again. The worker does not delete evidence automatically.

## Existing website integration and limits

The dashboard/LiveDetectionService already reads Benign_Table and Attack_Table;
DetectionMonitorService broadcasts their rows through SignalR. No app changes are
required for the basic detection feed. This worker does not invoke the existing
random C# detector or write NetworkEvents.

The admin Alerts page reads SecurityAlerts, not Attack_Table. This worker does not
create incident records there; incident grouping/linking requires a separate app
change. Likewise, import status is currently available via this CLI/database, not
a new web page. Historical CSVs are excluded from the app's recent-time statistics
when their original timestamps fall outside the selected window.

The existing monitor reads 100 rows per table per two-second poll and has no durable
notification cursor. Large imports can outpace its live feed; database results are
still saved. Its max-ID polling also needs redesign before concurrent writers or
durable alert delivery. A transactional outbox, batched SignalR updates, incident
integration, aggregate statistics and multiple inference workers are future app/
scaling work, not features silently implied by this initial worker.

## Tests

```bash
python -m unittest discover -s tests -v
```

Tests use temporary SQLite databases and simulated models. They cover routing,
feature validation, schema agreement with C#, labels, transaction rollback, restart
checkpoints, retries, duplicate imports, readiness, model pinning and exclusive
worker locks. SQL Server/ODBC and real model artifacts require deployment validation
against your own environment; SQLite tests do not prove SQL Server integration.
