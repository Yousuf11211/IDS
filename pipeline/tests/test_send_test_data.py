from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

from ids_pipeline.config import Config
from ids_pipeline.database import Store
from send_test_data import make_features, verify_dashboard_tables, write_record


class DashboardDataTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.store = Store(Config(Path(self.temp.name), "sqlite", "demo"))
        self.store.initialize()

    def tearDown(self):
        self.store.close()
        self.temp.cleanup()

    def test_writes_linked_test_records_without_import_metadata(self):
        self.store.conn.execute("DROP TABLE PipelineRows")
        self.store.conn.execute("DROP TABLE PipelineImports")
        self.store.conn.commit()
        verify_dashboard_tables(self.store)
        for attack in (False, True):
            table, result_id, raw_id = write_record(self.store, 0, attack, "dashboard-test-unit")
            result = self.store.fetch(f"SELECT * FROM [{table}] WHERE Id=?", (result_id,))[0]
            raw = self.store.fetch("SELECT * FROM RawPackets WHERE Id=?", (raw_id,))[0]
            self.assertTrue(result["Label"].startswith("TEST "))
            self.assertEqual(result["ModelVersion"], "dashboard-test-unit")
            self.assertEqual(raw["ClassifiedRecordId"], result_id)
            self.assertEqual(raw["Classification"], "Attack" if attack else "Benign")
            self.assertEqual(raw["IsProcessed"], 1)
            if attack:
                self.assertEqual(result["IsAcknowledged"], 0)
                self.assertIn("Not a real detection", result["Notes"])

    def test_result_failure_rolls_back_raw_insert(self):
        original = self.store.insert_flow

        def fail_result(table, *args, **kwargs):
            if table == "Attack_Table":
                raise RuntimeError("Injected failure")
            return original(table, *args, **kwargs)

        with patch.object(self.store, "insert_flow", side_effect=fail_result), self.assertRaises(RuntimeError):
            write_record(self.store, 0, True, "dashboard-test-unit")
        self.assertEqual(self.store.fetch("SELECT COUNT(*) AS N FROM RawPackets")[0]["N"], 0)

    def test_schema_complete_current_timestamp(self):
        from datetime import datetime, timezone
        from ids_pipeline.schema import NAMES
        for attack in (False, True):
            row = make_features(0, attack)
            self.assertEqual(set(row), set(NAMES))
            now = datetime.now(timezone.utc).replace(tzinfo=None)
            self.assertLess(abs((now - row["Timestamp"]).total_seconds()), 5)


if __name__ == "__main__":
    unittest.main()
