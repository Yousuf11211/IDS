"""Regression checks for the real CSV contract, UBJ models and SQL Server writer."""
from collections import Counter
from dataclasses import replace
from datetime import datetime
from pathlib import Path
import csv
import json
import os
import random
import re
import tempfile
import unittest
from unittest.mock import Mock, patch

from generate_csv import generate, synthetic_row
from ids_pipeline.config import Config, ROOT
from ids_pipeline.connection import parse_connection, to_odbc, webapp_connection
from ids_pipeline.database import Store
from ids_pipeline.models import Prediction, XGBoostModels, load_models, read_labels
from ids_pipeline.schema import COLUMNS, FEATURES, NAMES, SQL_TO_CSV, headers, validate
from ids_pipeline.worker import Worker, file_hash


def fixture(index=0):
    return synthetic_row(random.Random(42 + index), index, '2026-10-09T12:00:00Z')


class SchemaTests(unittest.TestCase):
    def test_schema_matches_web_application(self):
        source = (ROOT.parent / 'IDS/Data/Models/NetworkFlowBase.cs').read_text()
        expected = re.findall(r'public (DateTime|string\??|int|long|double) (\w+) \{', source)
        self.assertEqual([(c['type'], c['name']) for c in COLUMNS], expected)
        self.assertEqual(len(FEATURES), 119)
        self.assertNotIn('label', FEATURES)
        self.assertTrue(set(FEATURES) <= set(SQL_TO_CSV.values()))

    def test_header_case_order_and_pascal_names(self):
        supplied = list(reversed(FEATURES)) + ['label']
        self.assertEqual(headers([name.upper() for name in supplied]), supplied)
        self.assertEqual(headers(NAMES), list(SQL_TO_CSV.values()))
        for bad in (FEATURES[:-1], FEATURES + ['ACK_FLAG_COUNTS'], FEATURES + ['wrong']):
            with self.assertRaises(ValueError):
                headers(bad)

    def test_minimum_input_and_storage_defaults(self):
        row = {name: fixture()[name] for name in FEATURES}
        timestamp = datetime(2026, 10, 9)
        parsed = validate(row, timestamp=timestamp)
        self.assertEqual(parsed['timestamp'], timestamp)
        self.assertEqual(parsed['src_ip'], '')
        self.assertEqual(parsed['active_mean'], 0)
        self.assertEqual(parsed['label'], '')
        self.assertEqual(set(parsed), set(SQL_TO_CSV.values()))

    def test_invalid_features_never_silently_default(self):
        for column, value in [('duration', 'nan'), ('duration', ''), ('duration', 'inf'),
                              ('duration', '1e100'), ('dst_port', 65536), ('packets_count', '1.5'),
                              ('packets_count', '2147483648'), ('src_ip', 'bad'),
                              ('timestamp', '2026-10-09T12:00:00')]:
            with self.subTest(column=column, value=value), self.assertRaises(ValueError):
                validate({**fixture(), column: value})
        row = fixture()
        del row['duration']
        with self.assertRaises(ValueError):
            validate(row)

    def test_integral_float_csv_and_timezone(self):
        parsed = validate({**fixture(), 'packets_count': '1e3', 'dst_port': '443.0',
                           'timestamp': '2026-10-09T14:00:00+02:00'})
        self.assertEqual(parsed['packets_count'], 1000)
        self.assertEqual(parsed['dst_port'], 443)
        self.assertEqual(parsed['timestamp'], datetime(2026, 10, 9, 12))
        self.assertEqual(validate({k.upper(): v for k, v in fixture().items()}), validate(fixture()))


class ConnectionTests(unittest.TestCase):
    def test_default_web_connection_and_quoted_credentials(self):
        value = 'Server=localhost;Database=IDS;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True'
        odbc = to_odbc(value, 'ODBC Driver 17 for SQL Server')
        for part in ['Server={localhost}', 'Database={IDS}', 'Trusted_Connection={yes}',
                     'MARS_Connection={yes}', 'TrustServerCertificate={yes}', 'Encrypt={yes}']:
            self.assertIn(part, odbc)
        value = 'Data Source=host;Initial Catalog=IDS;User ID=user;Password="a;}}b""c";Encrypt=false'
        self.assertEqual(parse_connection(value)['password'], 'a;}}b"c')
        self.assertIn('PWD={a;}}}}b"c}', to_odbc(value, 'driver'))
        with self.assertRaises(ValueError):
            to_odbc(value + ';Unsupported=value', 'driver')
        with self.assertRaises(ValueError):
            parse_connection('Password="unclosed')

    def test_configuration_precedence_matches_program(self):
        with tempfile.TemporaryDirectory() as tmp, patch.dict(os.environ, {}, clear=True):
            app = Path(tmp)
            (app / 'appsettings.json').write_text('\ufeff' + json.dumps({'ConnectionStrings': {'DefaultConnection': 'base'}}), encoding='utf-8')
            (app / 'appsettings.Development.json').write_text(json.dumps({'ConnectionStrings': {'DefaultConnection': 'development'}}))
            config = Config(root=app, webapp_dir='.')
            self.assertEqual(webapp_connection(config), 'development')
            os.environ['ConnectionStrings__DefaultConnection'] = 'provider'
            self.assertEqual(webapp_connection(config), 'provider')
            os.environ['CONNECTION_STRING'] = 'environment'
            self.assertEqual(webapp_connection(config), 'environment')
            (app / '.env').write_text('CONNECTION_STRING="dotenv"\n')
            self.assertEqual(webapp_connection(config), 'dotenv')
            self.assertEqual(os.environ['CONNECTION_STRING'], 'environment')


class ModelTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        if not (ROOT / Config().gatekeeper).exists():
            raise unittest.SkipTest('Local UBJ model artifacts are not installed')
        cls.models = load_models(Config())

    def test_real_artifacts_order_case_and_no_label_leakage(self):
        rows = [validate(fixture(i)) for i in range(30)]
        expected = self.models.predict(rows)
        reordered = [{k.upper(): ('Attack' if k == 'label' else v) for k, v in reversed(list(row.items()))} for row in rows]
        self.assertEqual(self.models.predict(reordered), expected)
        self.assertEqual(len(self.models.features['gatekeeper']), 120)
        self.assertEqual(len(self.models.labels['multiclass']), 14)
        self.assertTrue(all(0 <= p.gate_score <= 1 for p in expected))

    def test_extra_feature_unused_and_new_usage_stops_startup(self):
        import numpy as np
        import xgboost as xgb
        row = validate(fixture())
        for stage, model in self.models.loaded.items():
            names = self.models.features[stage]
            self.assertNotIn('delta_start_incomplete_handshake', model.get_score())
            data = [[value if name == 'delta_start_incomplete_handshake' else row[name] for name in names]
                    for value in (0, 1, 1000000)]
            scores = model.predict(xgb.DMatrix(np.asarray(data), feature_names=model.feature_names))
            np.testing.assert_array_equal(scores[0], scores[1])
            np.testing.assert_array_equal(scores[0], scores[2])
        with patch('xgboost.Booster.get_score', return_value={'delta_start_incomplete_handshake': 1}):
            with self.assertRaisesRegex(ValueError, 'training transformation'):
                load_models(Config())

    def test_only_gatekeeper_attacks_reach_multiclass(self):
        model = XGBoostModels.__new__(XGBoostModels)
        model.config = Config(multiclass_threshold=.8)
        model.labels = {'multiclass': {0: 'Attack', 1: 'Bot'}}
        model.policy = {'Attack': {'category': 'Unspecified', 'severity': 'Medium'},
                        'Bot': {'category': 'Botnet', 'severity': 'High'}}
        model.probabilities = Mock(side_effect=[[[.1], [.9], [.8]], [[.1, .9], [.55, .45]]])
        rows = [{'duration': i} for i in range(3)]
        result = model.predict(rows)
        self.assertEqual(model.probabilities.call_args_list[1].args, ('multiclass', rows[1:]))
        self.assertFalse(result[0].attack)
        self.assertEqual(result[1].attack_type, 'Bot')
        self.assertEqual(result[2].attack_type, 'UnknownAttack')

    def test_mapping_rejects_duplicate_and_noncontiguous_ids(self):
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / 'labels.txt'
            for content in ['0 = Benign\n0 = Attack', '0 = Benign\n2 = Attack', 'no labels']:
                path.write_text(content)
                with self.assertRaises(ValueError):
                    read_labels(path)

    def test_bad_probabilities_fail(self):
        import numpy as np
        with patch.object(self.models.loaded['gatekeeper'], 'predict', return_value=np.array([[float('nan')]])):
            with self.assertRaisesRegex(ValueError, 'probabilities'):
                self.models.predict([validate(fixture())])


class SqlWriterTests(unittest.TestCase):
    def setUp(self):
        self.store = Store.__new__(Store)
        self.store.conn = Mock()
        self.store.conn.execute.return_value.fetchone.return_value = [123]

    def test_parameterized_insert_maps_to_existing_sql_columns(self):
        row = validate(fixture())
        result = self.store.insert_flow('RawPackets', row, IsProcessed=0)
        sql, values = self.store.conn.execute.call_args.args
        self.assertIn('INSERT INTO [dbo].[RawPackets]', sql)
        self.assertIn('[DstPort]', sql)
        self.assertIn('OUTPUT INSERTED.Id', sql)
        self.assertEqual(values[NAMES.index('DstPort')], row['dst_port'])
        self.assertEqual(sql.count('?'), len(values))
        self.assertEqual(result, 123)

    def test_prediction_raw_link_and_checkpoint_commit_together(self):
        row = {**validate(fixture()), 'RawId': 55, 'RowNumber': 2}
        self.store.complete_batch('hash', [row], [Prediction(True, .9, .8, 'Bot', 'Botnet', 'High')], 'ubj-release')
        calls = self.store.conn.execute.call_args_list
        self.assertIn('[dbo].[Attack_Table]', calls[0].args[0])
        self.assertEqual(calls[0].args[1][NAMES.index('Label')], 'Bot')
        self.assertEqual(calls[1].args[1], ('Attack', 123, 55))
        self.assertEqual(calls[2].args[1][:3], (.9, .8, 123))
        self.store.conn.commit.assert_called_once()
        self.store.conn.rollback.assert_not_called()
        self.assertEqual(row['label'], 'SyntheticCSV')

    def test_database_failure_rolls_back_batch(self):
        row = {**validate(fixture()), 'RawId': 55, 'RowNumber': 2}
        self.store.conn.execute.side_effect = [Mock(fetchone=lambda: [123]), RuntimeError('failure')]
        with self.assertRaises(RuntimeError):
            self.store.complete_batch('hash', [row], [Prediction(False, .1)], 'release')
        self.store.conn.rollback.assert_called_once()
        self.store.conn.commit.assert_not_called()


class MemoryStore:
    """Worker test double only; no runtime/database fallback exists."""
    def __init__(self):
        self.jobs, self.rows, self.results = {}, {}, []
    def job(self, digest):
        return self.jobs.get(digest)
    def register(self, digest, filename, models):
        return self.jobs.setdefault(digest, dict(Status='staging', FileName=filename, LastRow=0,
                                    Fingerprint=models.fingerprint, NextAttempt=0, Attempts=0))
    def update(self, digest, **changes):
        self.jobs[digest].update(changes)
    def stage(self, digest, rows):
        for number, features, error in rows:
            self.rows[digest, number] = dict(features=features, error=error, state='pending' if features else 'rejected')
        self.jobs[digest]['LastRow'] = rows[-1][0]
    def pending(self, digest, size):
        return [{**row['features'], 'RowNumber': n, 'RawId': n} for (d, n), row in self.rows.items()
                if d == digest and row['state'] == 'pending'][:size]
    def complete_batch(self, digest, rows, predictions, release):
        for row, prediction in zip(rows, predictions, strict=True):
            self.results.append(prediction)
            self.rows[digest, row['RowNumber']]['state'] = 'completed'
    def counts(self, digest):
        return dict(Counter(row['state'] for (d, _), row in self.rows.items() if d == digest))


class WorkerTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        if not (ROOT / Config().gatekeeper).exists():
            raise unittest.SkipTest('Local UBJ model artifacts are not installed')
        cls.models = load_models(Config())

    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.config = replace(Config(), root=Path(self.temp.name), batch_size=7)
        self.store = MemoryStore()
        self.worker = Worker(self.config, self.store, self.models)

    def test_generator_real_models_archive_and_duplicates(self):
        path = generate(self.config, rows=100)
        content = path.read_bytes()
        with path.open(newline='') as stream:
            reader = csv.DictReader(stream)
            self.assertEqual(reader.fieldnames, [n.lower() for n in reader.fieldnames])
            self.assertEqual(len(list(reader)), 100)
        digest = file_hash(path)
        self.worker.scan()
        self.assertEqual(self.store.jobs[digest]['Status'], 'completed')
        self.assertEqual(self.store.counts(digest), {'completed': 100})
        self.assertEqual(len(self.store.results), 100)
        self.assertTrue(any(p.attack for p in self.store.results))
        self.assertTrue(any(not p.attack for p in self.store.results))
        self.assertEqual(len(list((self.worker.data / 'archive').glob('*.csv'))), 1)
        duplicate = self.worker.data / 'incoming/copy.csv'
        duplicate.write_bytes(content)
        duplicate.with_suffix('.csv.ready').touch()
        self.worker.scan()
        self.assertEqual(len(self.store.results), 100)
        self.assertEqual(len(list((self.worker.data / 'duplicates').glob('*.csv'))), 1)

    def test_not_ready_file_and_bad_header(self):
        path = self.worker.data / 'incoming/bad.csv'
        path.write_text('wrong\n1\n')
        self.worker.scan()
        self.assertTrue(path.exists())
        path.with_suffix('.csv.ready').touch()
        self.worker.scan()
        self.assertEqual(len(list((self.worker.data / 'quarantine').glob('*.csv'))), 1)
        self.assertEqual(self.store.results, [])

    def test_bad_row_rejected_while_valid_row_classified(self):
        path = generate(self.config, rows=2)
        with path.open(newline='') as stream:
            reader = csv.DictReader(stream)
            names, rows = reader.fieldnames, list(reader)
        rows[0]['duration'] = 'nan'
        with path.open('w', newline='') as stream:
            writer = csv.DictWriter(stream, fieldnames=names)
            writer.writeheader()
            writer.writerows(rows)
        digest = file_hash(path)
        self.worker.scan()
        self.assertEqual(self.store.jobs[digest]['Status'], 'completed_with_errors')
        self.assertEqual(self.store.counts(digest), {'rejected': 1, 'completed': 1})

    def test_restart_resumes_pending_rows_without_duplicates(self):
        path = generate(self.config, rows=20)
        digest = file_hash(path)
        complete = self.store.complete_batch
        calls = 0
        def fail_second_batch(*args):
            nonlocal calls
            calls += 1
            if calls == 2:
                raise RuntimeError('transient failure')
            return complete(*args)
        with patch.object(self.store, 'complete_batch', side_effect=fail_second_batch):
            self.worker.scan()
        self.assertEqual(len(self.store.results), 7)
        self.store.jobs[digest]['NextAttempt'] = 0
        Worker(self.config, self.store, self.models).scan()
        self.assertEqual(len(self.store.results), 20)
        self.assertEqual(self.store.jobs[digest]['Status'], 'completed')

    def test_model_change_does_not_resume_pinned_import(self):
        generate(self.config, rows=2)
        self.worker.discover()
        path = next((self.worker.data / 'processing').glob('*.csv'))
        digest = file_hash(path)
        self.store.register(digest, path.name, self.models)['Fingerprint'] = 'old-release'
        self.worker.scan()
        self.assertEqual(self.store.results, [])
        self.assertTrue(path.exists())


if __name__ == '__main__':
    unittest.main()
