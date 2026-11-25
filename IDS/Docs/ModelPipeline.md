# Model-to-dashboard pipeline

The implementation and run instructions live in [pipeline/README.md](../../pipeline/README.md).
The worker loads the gatekeeper and multiclass UBJ models, parses their JSON label
mappings, normalizes CSV headers to lowercase, and uses the web application's
SQL Server connection settings.

Run `python generate_csv.py` from `pipeline/` to place synthetic inputs and their
completion marker in the real inbox. Run `python -m ids_pipeline run` to classify
them. All predictions come from the models; the old direct dummy insert scripts
and random C# detector have been removed.

Results are persisted to `RawPackets`, `Benign_Table`, and `Attack_Table` and shown
in `/Dashboard` and `/LiveDashboard`. `DetectionMonitorService` broadcasts newly
saved results through SignalR. ASP.NET Core reads classified detections and
persists actionable groups according to the configured security alert rules. The
Python pipeline continues to write detections, SHAP jobs, and worker heartbeats.

Both workers verify the ordered 120-feature contract in
`pipeline/Features_used/features_used.json`. The gate uses the configured 0.50
Attack threshold. Its 15-class successor stage runs only for flagged rows.
The SQL staging tables retain the validated handshake flag and nullable
measurements for replay, and attack explanations retain exact float32 snapshots.
See the pipeline README for the migration, preprocessing and rollback commands.
