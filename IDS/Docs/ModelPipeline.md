# Model pipeline for this project

The web dashboard already reads `RawPackets`, `Benign_Table`, and `Attack_Table`. Keep inference in a separate worker that writes validated results to those tables; the web application should display results and handle incidents, not load a large model inside a request handler. `Core/Services/DetectionService.cs` is currently a demonstration stub with random classification and must not be used as an actual detector.

## Model artifacts

Store each released model in a versioned directory outside `wwwroot` and outside the source repository, for example `models/flow-classifier/2026-09-20/`. For deployment, use an access-controlled artifact store or a read-only mounted directory. Each release should contain the model, its exact preprocessing pipeline, a feature-schema manifest, label map, training/evaluation report, and SHA-256 checksums. Configure the active version through the worker's environment. Record that version in each `Benign_Table` or `Attack_Table` result.

## Data contract

Use one canonical feature extractor for both training and inference. The worker should verify field names, order, numeric types, units, missing values, and finite ranges before calling the model. Reject or quarantine incomplete flows; do not silently replace every missing feature with zero. A label in training data must never be supplied as an inference feature. Keep raw input, prediction, confidence, model version, and processing status linked so an analyst can trace an alert back to its source flow.

## Online and offline paths

For live traffic: capture packets → assemble network flows → extract and validate features → infer with the pinned model version → write the raw flow and classification in one database transaction → let the existing monitor update dashboards and notifications. Process in bounded batches, retry transient database failures, and use a stable flow ID to avoid duplicate records after retries.

CSV is useful for offline training, evaluation, and replay tests. For production detection, pass typed flow records directly from the capture/feature worker to inference. If a CSV import is needed, load it through a staging step with the same schema validation and deduplication rules; do not make the web request or dashboard parse CSV files.

## Releasing a new model

Evaluate on held-out traffic, check confusion matrix and false-positive rate by attack type, verify the feature contract against a replay sample, then deploy the new version beside the current one. Switch the active version only after a health check and keep the previous version available for rollback. The database's `ModelVersion` field allows performance comparison after release.
