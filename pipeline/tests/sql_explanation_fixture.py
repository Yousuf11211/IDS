"""Opt-in SQL integration fixture. Refuses the application's normal database."""
from dataclasses import replace
import csv
import json
import os
from pathlib import Path
import tempfile
import sys

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from generate_csv import generate
from ids_pipeline.config import Config
from ids_pipeline.connection import parse_connection
from ids_pipeline.database import Store
from ids_pipeline.explanations import ExplanationService, ExplanationWorker
from ids_pipeline.features import HANDSHAKE
from ids_pipeline.models import load_models
from ids_pipeline.worker import Worker


def main():
    connection = os.environ['IDS_SHAP_SQL_TEST_CONNECTION']
    options = parse_connection(connection)
    database = options.get('database', options.get('initial catalog', ''))
    if not database.startswith('IDS_Shap_Test_'):
        raise ValueError('SQL integration fixtures require a dedicated IDS_Shap_Test_ database')
    os.environ['CONNECTION_STRING'] = connection
    with tempfile.TemporaryDirectory() as tmp:
        config = replace(Config(), data_dir=str(Path(tmp) / 'traffic'), webapp_dir=tmp, max_attempts=1)
        models = load_models(config)
        store = Store(config)
        try:
            store.acquire_lock()
            store.initialize()
            store.verify()
            worker = Worker(config, store, models)
            path = generate(config, rows=30, seed=42)
            with path.open(newline="") as stream:
                reader = csv.DictReader(stream)
                names = [n for n in reader.fieldnames if n != HANDSHAKE] + ["extra_extractor_column"]
                records = list(reader)
            with path.open("w", newline="") as stream:
                writer = csv.DictWriter(stream, names)
                writer.writeheader()
                for row in records:
                    del row[HANDSHAKE]
                    row.update(delta_start="not a complete handshake", extra_extractor_column="unused",
                               subflow_fwd_bytes="629.5")
                    writer.writerow(row)
            duplicate = path.with_name("duplicate.csv")
            duplicate.write_bytes(path.read_bytes())
            duplicate.with_suffix(".csv.ready").touch()
            worker.scan()
            assert store.fetch("""SELECT COUNT(*) AS Total FROM dbo.RawPackets WHERE IsProcessed=1
                AND DeltaStart IS NULL AND DeltaStartIncompleteHandshake=1 AND Label='SyntheticCSV'""")[0]['Total'] == 30
            assert store.fetch("SELECT COUNT(*) AS Total FROM dbo.PipelineRows WHERE State='completed'")[0]['Total'] == 30
            assert len(list((worker.data / "duplicates").glob("*.csv"))) == 1
            queued = store.fetch('SELECT COUNT(*) AS Total FROM dbo.FlowExplanations')[0]['Total']
            assert queued > 0
            store.acquire_explanation_lock()
            first = store.claim_explanation(models.fingerprint)
            store.fail_explanation(first['Id'], ValueError('Deterministic retry test'))
            assert store.fetch('SELECT Status FROM dbo.FlowExplanations WHERE Id=?', (first['Id'],))[0]['Status'] == 'failed'
            assert store.fetch('SELECT COUNT(*) AS Total FROM dbo.Attack_Table')[0]['Total'] > 0
            store.retry_explanations(models.fingerprint, first['AttackTrafficId'])
            # Remove only this disposable fixture's jobs to exercise historical replay.
            with store.transaction():
                store.conn.execute('DELETE FROM dbo.FlowExplanations WHERE AttackTrafficId=?', (first['AttackTrafficId'],))
            # A separate backfill connection must work while the explanation
            # worker still owns its exclusive application lock.
            backfill_store = Store(config)
            try:
                assert backfill_store.backfill_explanations(models) == 1
                assert backfill_store.backfill_explanations(models) == 0
            finally:
                backfill_store.close()
            service = ExplanationService(models)
            explainer = ExplanationWorker(store, models, service)
            while explainer.once():
                pass
            rows = store.fetch('SELECT * FROM dbo.FlowExplanations ORDER BY Id')
            assert len(rows) == queued and all(row['Status'] == 'completed' for row in rows)
            selected = next(row for row in rows if row['Stage'] == 'multiclass' and json.loads(row['PayloadJson'])['classIndex'] != 0)
            payload = json.loads(selected['PayloadJson'])
            gate = json.loads(next(row['PayloadJson'] for row in rows if row['Stage'] == 'gatekeeper'
                                  and row['AttackTrafficId'] == selected['AttackTrafficId']))
            assert payload['featureNames'] == models.feature_names
            observed = {f['name']: f['value'] for f in payload['features']}
            assert observed['subflow_fwd_bytes'] == 629.5
            assert observed['delta_start'] is None and observed[HANDSHAKE] == 1
            assert len(payload['features']) == 120
            assert payload['reference'] is None
            assert payload['additivityError'] < 2e-4
            print(json.dumps(dict(attackId=str(selected['AttackTrafficId']), rawId=str(selected['RawPacketId']),
                selectedClass=payload['explainedClass'], classIndex=payload['classIndex'],
                probability=payload['probability'], modelRelease=models.release, stagesSaved=len(rows),
                featureCount=len(payload['features']), additivityError=payload['additivityError'],
                generationMs=payload['generationMs'], gateProbability=gate['probability'],
                gateBase=gate['baseValue'], gateRawMargin=gate['rawMargin'],
                multiclassBase=payload['baseValue'], multiclassRawMargin=payload['rawMargin'],
                observed={feature['name']: feature['value'] for feature in payload['features']
                          if feature['name'] in ('dst_port', 'duration', 'packets_rate')},
                retryVerified=True, backfillVerified=True)))
        finally:
            store.close()


if __name__ == '__main__':
    main()
