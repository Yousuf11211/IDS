# SHAP incident inspection

This integration adds stored, per-flow Tree SHAP explanations to the existing
two-stage XGBoost CSV pipeline and a Razor Pages investigation report. It does
not retrain models or change thresholds, class mappings, escalation handlers,
dashboard polling, or SignalR delivery.

## Repository audit and integration decisions

- The application is ASP.NET Core 8 Razor Pages with Identity, EF Core SQL Server,
  controllers and SignalR. `LiveDashboard` renders both initial and incoming
  attack events; severe attacks already link to `EscalateAttack/{id}`.
- The actual attack primary key is **`dbo.Attack_Table.Id` (bigint)**. Raw input is
  `dbo.RawPackets.Id`; `Classification='Attack'` disambiguates the polymorphic
  `RawPackets.ClassifiedRecordId`. `PipelineRows.RawId` has a real raw-packet FK,
  and `PipelineRows.ResultId` identifies the classified attack/benign record.
- Imports run asynchronously in Python. Detection and its input checkpoint commit
  in one SQL transaction. `Attack_Table.ConfidenceScore` contains the multiclass
  probability; `PipelineRows.GateScore` holds the binary Attack probability.
  The live feed now labels category probability explicitly.
- Both UBJs match the 120 names and order in `pipeline/Features_used/features_used.json`.
  Inference and SHAP share the same float32 adapter. The raw SQL row retains the
  validated handshake flag, and explanation snapshots encode missing inputs as JSON null.
  The worker reproduces the prediction and orients binary contributions toward Attack
  according to the binary mapping.
- Original training preprocessing code and labeled training/reference datasets
  are absent. This implementation explains the **deployed** adapter exactly; it
  cannot independently certify how the training data were originally transformed.
  Archived synthetic detections are never used as a benign reference.
- Attack rows have no user/tenant ownership field. Detailed evidence is restricted
  to existing **Admin/Support** roles, with the same check inside the service.
  Employees and suspended/anonymous principals cannot inspect guessed IDs.
  Existing escalation permissions remain unchanged.
- Historical scores below refer to the previous model release.

## Python explanation and processing contract

`XGBoostModels.input_values/input_matrix` implement the common input adapter.
Detection stores an immutable stage snapshot containing feature order, exact
float32 values, original probability, predicted label, selected class index and
label, thresholds, top three multiclass scores, and the complete release
fingerprint. Queue insertion adds no SHAP computation to detection.

`ExplanationService` lazily reuses `shap.TreeExplainer` instances with
`feature_perturbation='tree_path_dependent'` and `model_output='raw'`. If an artifact
has `best_iteration`, a booster slice reproduces the inference iteration range.
It verifies the original score/class before explaining. Binary output is the
Attack log-odds, including when a reusable service call explains a benign flow.
Multiclass output is the selected class's raw margin, even if confidence policy
returned `UnknownAttack`. A multiclass SHAP contribution concerns that class,
not a generic Attack score. Neither raw output is displayed as a percentage.

Documented old list and modern `(samples, features, outputs)` SHAP layouts are
handled explicitly; unsupported shapes fail. Every completed result must satisfy
`base + sum(contributions) ≈ raw margin` with `atol=2e-4`, `rtol=2e-5`.
See the upstream [TreeExplainer contract](https://shap.readthedocs.io/en/stable/generated/shap.TreeExplainer.html).

The separate `explain` process owns a SQL application lock independent of the CSV
worker. It claims one eligible stage, commits the claim, computes outside a SQL
transaction, and persists JSON. A crash leaves a durable job; the next exclusive
owner requeues abandoned processing jobs. Failures retry with configured delay
and attempt limit; terminal failures retain the incident and an unavailable state.
The worker needs its original release fingerprint to consume a job. Restore old
artifacts/configuration to explain older releases; no replacement model is used.

## Database design and migration

The explanation table is owned by EF Core:
`20261016214443_AddFlowExplanations` (plus generated designer and model snapshot).
Its `Up` creates only this table and its indexes/constraints. No traffic table is
altered or dropped. Rollback of this migration would remove explanation data.

| Column / constraint | Purpose |
| --- | --- |
| `Id bigint identity` | Explanation/job primary key |
| `AttackTrafficId bigint` | Required FK → `Attack_Table.Id`, restricted deletion |
| `RawPacketId bigint null` | FK → `RawPackets.Id`, restricted deletion |
| `Stage`, `ModelVersion`, `Fingerprint` | Stage, artifact version, complete inference contract fingerprint |
| `ContextJson nvarchar(max)` | Immutable original values, labels, probabilities, thresholds and class selection |
| `PayloadJson nvarchar(max) null` | Full numerical explanation, base, units, all 120 contributions, versions, timestamp, timing, additivity and optional reference statistics |
| `Status`, `Attempts`, `NextAttempt`, `LastError` | Durable work and bounded retries |
| `CreatedAt`, `UpdatedAt datetime2` | UTC queue/persistence timestamps |
| Unique `(AttackTrafficId, Stage, ModelVersion)` | Safe retries/backfill without duplicates |
| Index `(Status, Fingerprint, NextAttempt, Id)` | Worker queue selection |
| Stage/status/attempt checks | Reject invalid stage/state/negative attempts |

The class, probability and explanation metadata reside in versioned JSON rather
than redundant columns on `Attack_Table`. The selected output class is immutable
for a particular incident/stage/model. SQL writes are parameterized.

For the current flow-input migration and deployment commands see [the pipeline runbook](../../pipeline/README.md#setup).
The following commands describe the earlier explanation-table migration on an
**existing migrated database** using the same configuration as the application:

```powershell
dotnet ef migrations script 20260923143405_AddSecurityChangeRequests 20261016214443_AddFlowExplanations --project IDS --startup-project IDS --output FlowExplanations.sql
# Review the script, then apply it through your normal database procedure, or:
dotnet run --project IDS/IDS.csproj --no-launch-profile -- --migrate-only
```

The maintenance command applies all pending migrations; review the pending set
first on a deployed database. This work did not migrate or change the existing
`IDS` database. The existing application still migrates automatically in
Development, and requires explicit migration in deployed environments.

**Existing migration limitation found during testing:** bootstrapping a completely
blank SQL database through the historical chain fails with “There is already an
object named 'Roles' in the database.” This precedes the new migration. The isolated
SQL test creates the historical non-nullable flow schema without explanations,
seeds a historical record and executes the explanation and nullable-input migrations.

## Startup, routes, retries and historical detections

Windows PowerShell, with the existing Python environment activated:

```powershell
# Solution directory: apply/review migration as above, then start the application.
dotnet run --project IDS/IDS.csproj

# Separate terminal, pipeline directory:
.\.venv\Scripts\Activate.ps1
python -m pip install -r requirements.txt
python -m ids_pipeline init-db
python -m ids_pipeline check
python -m ids_pipeline run

# Another terminal in pipeline/, same activated environment/configuration:
python -m ids_pipeline explain
```

Admin/Support users open `/LiveDashboard`, then **Inspect** on any attack. The
dedicated route is **`/InspectAttack/{Attack_Table.Id}`**; the read API is
**`GET /api/detection/{id}/inspection`**. Responses are private/no-store. Report
loads are read-only and never trigger Python computation. New attacks expose
their original scores while explanations are pending. The page polls pending
work with a bounded backoff and provides a manual Refresh action.

Historical attacks can show stored summary/flow details immediately after migration.
They need explanation backfill for charts. Backfill can run alongside `explain`:

```powershell
python -m ids_pipeline backfill-explanations --limit 100
python -m ids_pipeline explain

# After resolving a terminal failure, stop explain before retrying:
python -m ids_pipeline retry-explanations 123
python -m ids_pipeline explain
```

Backfill accepts only linked original pipeline rows whose full release fingerprint,
attack label, binary score, multiclass score and stored confidence can be reproduced
from the saved raw inputs. It inserts both stage jobs transactionally. Repeating
backfill does not duplicate jobs. Unlinked records or unavailable releases are
left unavailable; no synthetic explanation is inserted. `--limit` bounds a scan
to 1–10,000 candidates. Backfill reports queued attacks; incompatible records are
logged. An operator must restore the original artifacts/config for older releases.

After a Python code update, restart the existing `run` and `explain` processes.
An already running detector retains its old code and can keep saving predictions
without queueing the newly introduced explanation jobs. A current detector logs
`SHAP job enqueueing is enabled`; the explanation worker logs each completed stage.
If a new incident shows both stage probabilities unavailable and no stage metadata,
restart the detector and run verified backfill for the missed jobs. Backfill inserts
idempotent work through a separate connection and does not interrupt explanations.

## Benign reference profile

No verified reference dataset was available, so **Reference unavailable** is the
correct initial state. A versioned JSON artifact is appropriate here: small,
auditable, deployment-controlled, and no new database lifecycle. Completed payloads
embed the exact statistics/version used so changing a future profile cannot
silently change old reports.

Prepare a manifest outside detection `data/`, for a genuine labeled training or
independent reference CSV with the complete model feature contract and `label`:

```json
{
  "file": "your-verified-training.csv",
  "sha256": "REPLACE_WITH_THE_ACTUAL_64_CHARACTER_FILE_SHA256",
  "datasetVersion": "YOUR_IDENTIFIABLE_DATASET_AND_TRAINING_SPLIT_VERSION",
  "split": "training",
  "benignLabel": "Benign",
  "provenance": "DOCUMENT_THE_SOURCE_GROUND_TRUTH_AND_SPLIT_PROCEDURE"
}
```

Use `Get-FileHash your-verified-training.csv -Algorithm SHA256` to obtain the hash
(lowercase it in the manifest). Then, from `pipeline/`:

```powershell
python -m ids_pipeline.reference path/to/manifest.json config/benign-reference.json
```

Uncomment `reference_profile = "config/benign-reference.json"` under `[models]`
in `config/pipeline.toml`, then restart the explanation process. The builder
requires a matching supplied file hash, documented provenance, explicit
training/reference split and actual benign labels. Test/evaluation splits and
files inside detection `data/` are rejected. The dataset custodian must still
verify truthful provenance; a manifest cannot independently prove a split.

Statistics are mean, population standard deviation, median, 5th/25th/75th/95th
percentiles and benign sample count, computed on the float32 model values. The profile excludes
port identifiers, the categorical handshake flag and missing observations per feature.
Missing or invalid profiles and fingerprint mismatches omit reference statistics and allow
SHAP processing. A zero median never yields an infinite percentage. The schema
has no protocol identity, so the available profile group is all benign reference
traffic; protocol-specific grouping is not claimed. Physical measurement units
are not documented in the source contract; the report labels model input units
instead of guessing seconds/bytes for ambiguous features.

## Interactive report

The report uses a local, pinned Plotly **basic 3.1.0** bundle (about 1 MB uncompressed),
loaded only on the inspection page. No React, runtime CDN, static chart images,
or extra service is introduced. Existing CSP behavior is unchanged; no CSP header
was found in this repository. Third-party fonts/icons belong to the existing
layout; charts also passed with external requests blocked.

Signed horizontal bars and a floating-bar waterfall expose top 5/top 10/all
features, sorting and binary/multiclass switching. The waterfall retains the sum
of omitted features and both baseline/final output. Hover and keyboard-selectable
evidence show actual input, contribution, direction, units and available benign
statistics/differences. Each reference feature uses an independent numeric axis.
The page includes current acknowledgement/escalation status, stored-flow search,
versions, additivity residuals and generation timing. Missing IP/port/protocol
fields are explicitly unavailable; records are not described as active attacks.
Mobile charts scroll within their panels. Motion honors reduced-motion preferences.
API/library errors retain numerical evidence and show an actionable message.

## Dependencies and verification

The verified local environment is Windows Python **3.12.10**, XGBoost CPU **3.4.1**,
NumPy **2.5.3**, SHAP **0.54.1**, Microsoft ODBC and SQL Server, .NET 8 runtime with
SDK 9.0.318, and installed Microsoft Edge. SHAP and test-only Playwright 1.56.0
were installed in the existing Python environment. The Python 3.11 requirements
retain their existing XGBoost 3.2.0 branch and use SHAP 0.50.0 (Python 3.11 capable);
that separate environment was **not executed** here. No .NET package was added.

Run checks from their indicated directory:

```powershell
# Solution directory:
dotnet test IDS.sln --no-restore --verbosity minimal
node IDS.Tests/attack-inspection.test.cjs

# pipeline/:
python -m ids_pipeline check-models
python -m unittest discover -s tests -v
python -m pip install -r requirements-test.txt
python tests/run_sql_verification.py --browser
# If matching test binaries are already built and the application is running:
python tests/run_sql_verification.py --browser --no-build
# Optional UI review captures only:
python tests/run_sql_verification.py --browser --screenshots
```

The opt-in SQL test creates a **new** `IDS_Shap_Test_*` database, refusing the normal
database. It retains the dedicated database for review and writes numerical test
evidence to ignored `IDS.Tests/TestResults/shap-verification.json` after success.
It uses the real Python worker/models, real SQL Server, actual migration commands,
the actual EF inspection service, API controller and compiled Razor page on an
isolated Kestrel host. Test-only header authentication exercises production role
authorization middleware; it is never registered in `Program.cs`. This is not a
claim of a complete production Identity/MFA login browser test. The ordinary suite
skips this opt-in test unless explicitly configured. Review/remove disposable
test databases through SQL administration when finished; the tool never drops
the existing application database.

### Historical verification of the previous model release (2026-10-16)

| Check | Actual result |
| --- | --- |
| Python unittest suite | 31 passed, including real UBJ loading, both stages, nonzero class selection, SHAP additivity, exact observed values/order, original predictions, failed jobs and missing reference |
| .NET full suite | 56 passed; 1 opt-in SQL test skipped in this ordinary run; no failures |
| Separate opt-in SQL + Edge test | 1 passed (20 seconds); actual additive migration, durable processing, retry, backfill twice, SQL uniqueness/FK rejection, HTTP API, Razor and browser charts |
| JavaScript numerical tests | 3 passed (`node IDS.Tests/attack-inspection.test.cjs`) |
| Browser checks | Hover, keyboard details, both stages, correct nonzero class, top 5/10/all, sorting, waterfall, 390px mobile, preserved escalation link, API error, processing/failed states, unavailable baseline, missing Plotly fallback, external requests blocked, no JavaScript errors |
| Authorization checks | Anonymous 401 and Employee 403 on both HTTP routes; Support succeeds; suspended principals denied in service tests; private no-store response |
| Original classification preservation | All original fields match 30 deterministic predictions executed from pre-change Git code, exactly |
| Repository whitespace check | `git diff --check` passed |

One fully verified deterministic demonstration flow in the retained database
`IDS_Shap_Test_f3ee63959960` follows this path:

1. `generate_csv.py`, seed 42, row 26 supplies the existing feature contract.
   Actual model values include `dst_port=80`, `duration=0.14024700224399567`,
   `packets_rate=15971.8203125` after float32 conversion. These are synthetic
   demonstration inputs, not research evaluation data or a benign reference.
2. Binary XGBoost reports **Attack probability 0.9068147540092468**, above the
   unchanged 0.5 gate. Its base is `-0.019720066338777542` log-odds and raw output
   is `2.275348663330078` log-odds.
3. Multiclass XGBoost selects **index 3, `Brute_Force_SSH`**, with probability
   **0.30241450667381287**. Its base is `3.1758124828338623` and raw selected-class
   output is `-0.029387224465608597`. A selected multiclass margin can be negative;
   softmax compares it with all other class margins.
4. The inference/queue transaction links **RawPackets.Id 26 → Attack_Table.Id 2**.
   Both stages are explained using release `ubj-55e5e7e8be3742d91b9d`; all 120
   exact input values and SHAP contributions are saved in SQL Server. Multiclass
   reconstruction residual is **1.0458199994900497e-6**. This stage took about
   **406 ms** in the recorded run; this single observation is not a throughput
   benchmark. The whole fixture stored six stages for three attacks.
5. The actual .NET API at `/api/detection/2/inspection` retrieves that incident's
   persisted results; `/InspectAttack/2` renders the actual Razor report. Edge
   consumes those numbers and verifies bar/waterfall data and interactions.
   The test host uses the real role authorization middleware with a test-only
   authentication scheme; production cookie/MFA sign-in was not exercised.

Unverified limits: no genuine benign reference dataset/profile was supplied, so
no live benign-comparison findings are claimed; the range-chart branch awaits
such a profile. The complete historical migration chain cannot bootstrap a blank
database as described above. Python 3.11 and large sustained traffic loads were
not tested. Neither the existing `IDS` database nor its historical rows were
migrated/backfilled during this implementation; apply the migration and run the
documented backfill to enable historical charts there.

## File manifest

Modified:

```text
IDS/Data/ApplicationDbContext.cs
IDS/Data/Migrations/ApplicationDbContextModelSnapshot.cs
IDS/Pages/LiveDashboard.cshtml
IDS/Program.cs
pipeline/README.md
pipeline/config/pipeline.toml
pipeline/ids_pipeline/__main__.py
pipeline/ids_pipeline/config.py
pipeline/ids_pipeline/database.py
pipeline/ids_pipeline/models.py
pipeline/ids_pipeline/worker.py
pipeline/requirements.txt
pipeline/tests/test_pipeline.py
```

Created:

```text
IDS/Api/AttackInspectionController.cs
IDS/Core/Services/AttackInspectionService.cs
IDS/Data/FlowExplanationConfiguration.cs
IDS/Data/Models/FlowExplanation.cs
IDS/Data/Models/InspectionModels.cs
IDS/Data/Migrations/20261016214443_AddFlowExplanations.cs
IDS/Data/Migrations/20261016214443_AddFlowExplanations.Designer.cs
IDS/Pages/InspectAttack.cshtml
IDS/Pages/InspectAttack.cshtml.cs
IDS/wwwroot/css/attack-inspection.css
IDS/wwwroot/js/attack-inspection.js
IDS/wwwroot/lib/plotly/plotly-basic-3.1.0.min.js
IDS/wwwroot/lib/plotly/README.md
IDS/Docs/ShapInspection.md
IDS.Tests/AttackInspectionTests.cs
IDS.Tests/SqlServerExplanationIntegrationTests.cs
IDS.Tests/attack-inspection.test.cjs
pipeline/ids_pipeline/explanations.py
pipeline/ids_pipeline/reference.py
pipeline/requirements-test.txt
pipeline/tests/test_explanations.py
pipeline/tests/fixtures/original_predictions.json
pipeline/tests/run_sql_verification.py
pipeline/tests/sql_explanation_fixture.py
pipeline/tests/verify_inspection_browser.py
```

The original-prediction fixture was executed from the unmodified inference adapter
at commit `d5f7d89c67d10423527741bcab18f20bb3ecc5b2`, using the saved artifacts and
existing deterministic test inputs. It preserves all original prediction fields
for 30 rows; it is not an evaluation dataset or research performance metric.
