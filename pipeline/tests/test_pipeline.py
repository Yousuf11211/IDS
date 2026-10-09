import csv
from datetime import datetime
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

from ids_pipeline.__main__ import main
from ids_pipeline.config import Config, ROOT
from ids_pipeline.database import Store
from ids_pipeline.models import DemoModels, SklearnModels, load_models
from ids_pipeline.schema import COLUMNS, NAMES, headers, validate
from ids_pipeline.worker import Worker, file_hash


def fixture(port=443):
    row = {c["name"]: "" if c["type"].startswith("string") else "0" for c in COLUMNS}
    row.update(Timestamp="2026-01-01T12:00:00Z", SrcIp="192.0.2.1", DstPort=str(port), Label="GroundTruth")
    return row


class SchemaTests(unittest.TestCase):
    def test_schema_matches_csharp(self):
        import re
        source = ROOT.parent / "IDS/Data/Models/NetworkFlowBase.cs"
        if not source.exists():
            self.skipTest("Repository source unavailable")
        expected = re.findall(r"public (DateTime|string\??|int|long|double) (\w+) \{", source.read_text())
        self.assertEqual([(c["type"], c["name"]) for c in COLUMNS], expected)

    def test_snake_case_mapping_and_optional_label(self):
        names = [name for name in NAMES if name != "Label"]
        names[names.index("SrcIp")] = "src_ip"
        self.assertIn("SrcIp", headers(names))
        with self.assertRaises(ValueError):
            headers(NAMES + ["src_ip"])

    def test_invalid_values_are_not_zero_filled(self):
        for column, value in (("Duration", "NaN"), ("Duration", ""), ("SrcIp", "no"),
                              ("DstPort", "65536"), ("PacketsCount", "2147483648"),
                              ("Timestamp", "2026-01-01T12:00:00")):
            with self.subTest(column=column, value=value), self.assertRaises(ValueError):
                validate({**fixture(), column: value})

    def test_timezone_normalized(self):
        result = validate({**fixture(), "Timestamp": "2026-01-01T14:00:00+02:00"})
        self.assertEqual(result["Timestamp"], datetime(2026, 1, 1, 12))

    def test_placeholders_fail_closed(self):
        with self.assertRaises(RuntimeError):
            load_models(Config(ROOT, "sqlserver", "placeholder"))
        with self.assertRaises(ValueError):
            load_models(Config(ROOT, "sqlserver", "demo"))

    def test_demo_folders_are_separate_from_production(self):
        demo = Config.load(ROOT / "config/demo.toml")
        production = Config.load(ROOT / "config/pipeline.toml")
        self.assertNotEqual(demo.data_dir, production.data_dir)
        self.assertEqual(demo.backend, "sqlite")

    def test_default_run_stops_before_database_access(self):
        with patch("ids_pipeline.__main__.Store") as store, self.assertRaises(RuntimeError):
            main(["run", "--once"])
        store.assert_not_called()


class ModelTests(unittest.TestCase):
    def test_only_attack_candidates_reach_stage_two(self):
        model = SklearnModels.__new__(SklearnModels)
        model.loaded = {
            "gatekeeper": type("Gate", (), {"classes_": ["Attack", "Benign"]})(),
            "multiclass": type("Types", (), {"classes_": ["DoS", "PortScan"]})(),
        }
        model.manifest = {"gatekeeper_threshold": .5, "multiclass_threshold": .8,
                          "gatekeeper": {"attack_label": "Attack"},
                          "multiclass": {"labels": {
                              "DoS": {"attack_type": "DoS", "category": "DoS", "severity": "High"},
                              "PortScan": {"attack_type": "PortScan", "category": "Probe", "severity": "Medium"}}}}
        received = []

        def probabilities(stage, rows):
            if stage == "gatekeeper":
                return [[.1, .9], [.9, .1], [.8, .2]]
            received.extend(rows)
            return [[.9, .1], [.6, .4]]

        model.probabilities = probabilities
        results = model.predict([{"id": 1}, {"id": 2}, {"id": 3}])
        self.assertEqual(received, [{"id": 2}, {"id": 3}])
        self.assertFalse(results[0].attack)
        self.assertEqual(results[1].attack_type, "DoS")
        self.assertEqual(results[2].attack_type, "UnknownAttack")
        self.assertTrue(results[2].attack)


class WorkerTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.config = Config(Path(self.temp.name), "sqlite", "demo", batch_size=2, max_attempts=2)
        self.store = Store(self.config)
        self.store.initialize()
        self.store.acquire_lock()
        self.worker = Worker(self.config, self.store, DemoModels())

    def tearDown(self):
        self.store.close()
        self.temp.cleanup()

    def write_csv(self, name="flows.csv", rows=None, ready=True):
        path = self.config.root / "data/incoming" / name
        with path.open("w", newline="", encoding="utf-8") as stream:
            writer = csv.DictWriter(stream, fieldnames=NAMES)
            writer.writeheader()
            writer.writerows(rows if rows is not None else [fixture(), fixture(22), fixture(80)])
        if ready:
            path.with_suffix(".csv.ready").touch()
        return path

    def total(self, table):
        return self.store.fetch(f"SELECT COUNT(*) AS Total FROM [{table}]")[0]["Total"]

    def test_routes_and_preserves_source_labels(self):
        path = self.write_csv()
        digest = file_hash(path)
        self.worker.scan()
        self.assertEqual(self.total("RawPackets"), 3)
        self.assertEqual(self.total("Benign_Table"), 2)
        self.assertEqual(self.total("Attack_Table"), 1)
        self.assertEqual(self.store.job(digest)["Status"], "completed")
        self.assertEqual(self.store.fetch("SELECT Label FROM RawPackets")[0]["Label"], "GroundTruth")
        self.assertEqual(self.store.fetch("SELECT Label FROM Attack_Table")[0]["Label"], "DemoAttack")
        self.assertEqual(self.store.fetch("SELECT SUM(IsProcessed) AS Total FROM RawPackets")[0]["Total"], 3)

    def test_duplicate_files_do_not_duplicate_results(self):
        path = self.write_csv()
        content = path.read_bytes()
        self.worker.scan()
        other = self.config.root / "data/incoming/copy.csv"
        other.write_bytes(content)
        other.with_suffix(".csv.ready").touch()
        self.worker.scan()
        self.assertEqual(self.total("RawPackets"), 3)
        self.assertEqual(self.total("PipelineImports"), 1)
        self.assertEqual(len(list((self.worker.data / "duplicates").glob("*.csv"))), 1)

    def test_incomplete_copy_is_not_claimed(self):
        path = self.write_csv(ready=False)
        self.worker.scan()
        self.assertTrue(path.exists())
        self.assertEqual(self.total("RawPackets"), 0)

    def test_invalid_row_is_rejected_and_valid_rows_continue(self):
        path = self.write_csv(rows=[fixture(), {**fixture(), "Duration": "inf"}, fixture(22)])
        digest = file_hash(path)
        self.worker.scan()
        self.assertEqual(self.store.counts(digest), {"completed": 2, "rejected": 1})
        self.assertEqual(self.store.job(digest)["Status"], "completed_with_errors")

    def test_transaction_failure_rolls_back_entire_prediction_batch(self):
        path = self.write_csv()
        digest = file_hash(path)
        self.worker.discover()
        claimed = next((self.worker.data / "processing").glob("*.csv"))
        self.store.register(digest, claimed.name, DemoModels())
        self.worker.stage_file(claimed, digest, 0)
        rows = self.store.pending(digest, 2)
        original = self.store.insert_flow
        calls = 0

        def broken(*args, **kwargs):
            nonlocal calls
            calls += 1
            if calls == 2:
                raise RuntimeError("Injected write failure")
            return original(*args, **kwargs)

        with patch.object(self.store, "insert_flow", broken), self.assertRaises(RuntimeError):
            self.store.complete_batch(digest, rows, DemoModels().predict(rows), "demo-only-v1")
        self.assertEqual(self.total("Benign_Table"), 0)
        self.assertEqual(self.total("Attack_Table"), 0)
        self.assertEqual(self.store.counts(digest), {"pending": 3})

    def test_restart_resumes_after_staging_checkpoint(self):
        path = self.write_csv()
        digest = file_hash(path)
        self.worker.discover()
        claimed = next((self.worker.data / "processing").glob("*.csv"))
        self.store.register(digest, claimed.name, DemoModels())
        self.store.stage(digest, [(1, validate(fixture()), None)])
        self.store.close()
        self.store = Store(self.config)
        self.store.acquire_lock()
        self.worker = Worker(self.config, self.store, DemoModels())
        self.worker.scan()
        self.assertEqual(self.total("RawPackets"), 3)
        self.assertEqual(self.store.counts(digest), {"completed": 3})

    def test_restart_after_result_commit_does_not_duplicate(self):
        path = self.write_csv()
        digest = file_hash(path)
        self.worker.discover()
        claimed = next((self.worker.data / "processing").glob("*.csv"))
        self.store.register(digest, claimed.name, DemoModels())
        self.worker.stage_file(claimed, digest, 0)
        self.store.update(digest, Status="classifying")
        rows = self.store.pending(digest, 2)
        self.store.complete_batch(digest, rows, DemoModels().predict(rows), "demo-only-v1")
        self.worker.scan()
        self.assertEqual(self.total("Benign_Table"), 2)
        self.assertEqual(self.total("Attack_Table"), 1)

    def test_model_failure_never_becomes_benign_and_retries_are_bounded(self):
        path = self.write_csv()
        digest = file_hash(path)
        with patch.object(self.worker.models, "predict", side_effect=RuntimeError("failure")):
            self.worker.scan()
            self.assertEqual(self.store.counts(digest), {"pending": 3})
            self.store.update(digest, NextAttempt=0)
            self.worker.scan()
        self.assertEqual(self.store.job(digest)["Status"], "failed")
        self.assertEqual(self.total("Benign_Table"), 0)

    def test_release_mismatch_leaves_pending_work_untouched(self):
        path = self.write_csv()
        digest = file_hash(path)
        self.worker.discover()
        claimed = next((self.worker.data / "processing").glob("*.csv"))
        models = DemoModels()
        models.fingerprint = "previous-model-release"
        self.store.register(digest, claimed.name, models)
        self.worker.scan()
        self.assertEqual(self.total("RawPackets"), 0)
        self.assertTrue(claimed.exists())

    def test_second_worker_cannot_acquire_database(self):
        other = Store(self.config)
        try:
            with self.assertRaises(RuntimeError):
                other.acquire_lock()
        finally:
            other.close()

    def test_bad_header_quarantines_file(self):
        path = self.config.root / "data/incoming/bad.csv"
        path.write_text("unknown\n1\n")
        path.with_suffix(".csv.ready").touch()
        digest = file_hash(path)
        self.worker.scan()
        self.assertEqual(self.store.job(digest)["Status"], "rejected")
        self.assertEqual(self.total("RawPackets"), 0)

    def test_completion_before_archive_move_is_recoverable(self):
        path = self.write_csv()
        digest = file_hash(path)
        self.worker.discover()
        claimed = next((self.worker.data / "processing").glob("*.csv"))
        self.store.register(digest, claimed.name, DemoModels())
        self.worker.stage_file(claimed, digest, 0)
        rows = self.store.pending(digest, 10)
        self.store.complete_batch(digest, rows, DemoModels().predict(rows), "demo-only-v1")
        self.store.update(digest, Status="completed")
        self.worker.scan()
        self.assertEqual(self.total("RawPackets"), 3)
        self.assertEqual(self.total("Attack_Table"), 1)
        self.assertFalse(claimed.exists())
        self.assertTrue((self.worker.data / "archive" / claimed.name).exists())


if __name__ == "__main__":
    unittest.main()
