# Model-to-dashboard pipeline

The implementation and run instructions live in [pipeline/README.md](../../pipeline/README.md).
The worker loads the gatekeeper and multiclass UBJ models, parses their text label
mappings, normalizes CSV headers to lowercase, and uses the web application's
SQL Server connection settings.

Run `python generate_csv.py` from `pipeline/` to place synthetic inputs and their
completion marker in the real inbox. Run `python -m ids_pipeline run` to classify
them. All predictions come from the models; the old direct dummy insert scripts
and random C# detector have been removed.

Results are persisted to `RawPackets`, `Benign_Table`, and `Attack_Table` and shown
in `/Dashboard` and `/LiveDashboard`. `DetectionMonitorService` broadcasts newly
saved results through SignalR. The separate administrative `SecurityAlerts` table
is not populated by this flow.

The CSV list has 119 numeric features plus optional `label`; the artifacts have
one additional, unused training column. See the pipeline README for the guarded
handling of that column, input validation, storage defaults, and recovery behavior.
