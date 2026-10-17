"""Real artifacts, deterministic inputs, durable queue contract and SHAP layouts."""
from dataclasses import replace
import json
import tempfile
from pathlib import Path
import unittest
from unittest.mock import Mock

import numpy as np

from ids_pipeline.config import Config, ROOT
from ids_pipeline.database import Store
from ids_pipeline.explanations import ExplanationService, ExplanationWorker, select_output
from ids_pipeline.models import load_models
from ids_pipeline.reference import build_profile, load_reference
from ids_pipeline.schema import validate
from test_pipeline import fixture


class OutputShapeTests(unittest.TestCase):
    def test_modern_and_legacy_multiclass_select_correct_nonzero_class(self):
        array = np.arange(12).reshape(1, 4, 3)
        expected = [10, 20, 30]
        for values in (array, [array[:, :, i] for i in range(3)]):
            vector, base = select_output(values, expected, feature_count=4, class_count=3, class_index=2)
            np.testing.assert_array_equal(vector, [2, 5, 8, 11])
            self.assertEqual(base, 30)

    def test_binary_and_unsupported_shape(self):
        vector, base = select_output(np.ones((1, 4)), .5, feature_count=4, class_count=1, class_index=0)
        self.assertEqual(vector.sum(), 4)
        self.assertEqual(base, .5)
        with self.assertRaisesRegex(ValueError, 'Unsupported'):
            select_output(np.ones((3, 1, 4)), [0, 0, 0], feature_count=4, class_count=3, class_index=2)


class RealExplanationTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        if not (ROOT / Config().gatekeeper).exists():
            raise unittest.SkipTest('Local UBJ artifacts not installed')
        cls.models = load_models(Config())
        cls.service = ExplanationService(cls.models)
        cls.rows = [validate(fixture(i)) for i in range(30)]
        cls.predictions = cls.models.predict(cls.rows)

    def test_real_artifacts_both_stages_additivity_and_selected_class(self):
        selected_nonzero = False
        for row, prediction in zip(self.rows, self.predictions):
            if not prediction.attack:
                continue
            selected_nonzero |= prediction.class_index != 0
            for stage in ('gatekeeper', 'multiclass'):
                context = self.models.explanation_context(stage, row, prediction)
                before = self.models.predict([row])
                result = self.service.generate(stage, context, attack_id=123, raw_id=55)
                self.assertEqual(self.models.predict([row]), before)
                self.assertEqual(result['featureNames'], self.models.features[stage])
                self.assertEqual(len(result['features']), 120)
                self.assertEqual([f['value'] for f in result['features']], context['featureValues'])
                self.assertAlmostEqual(result['baseValue'] + sum(f['contribution'] for f in result['features']),
                                       result['rawMargin'], delta=2e-4)
                self.assertIsNone(result['reference'])
                self.assertTrue(all(f['reference'] is None for f in result['features']))
                self.assertEqual(result['probability'], prediction.gate_score if stage == 'gatekeeper' else prediction.type_score)
                if stage == 'multiclass':
                    self.assertEqual(result['explainedClass'], self.models.labels[stage][prediction.class_index])
                    self.assertEqual(len(result['topProbabilities']), 3)
                json.dumps(result, allow_nan=False)
            if selected_nonzero:
                break
        self.assertTrue(selected_nonzero, 'Deterministic fixture must exercise a nonzero attack class')

    def test_predictions_equal_original_adapter_before_shap_integration(self):
        original = json.loads((Path(__file__).parent / 'fixtures/original_predictions.json').read_text())
        self.assertEqual(original['release'], self.models.release)
        for expected, actual in zip(original['predictions'], self.predictions, strict=True):
            for field, value in expected.items():
                self.assertEqual(getattr(actual, field), value, field)

    def test_benign_binary_explains_attack_score_without_changing_label(self):
        row, prediction = next((r, p) for r, p in zip(self.rows, self.predictions) if not p.attack)
        result = self.service.generate('gatekeeper', self.models.explanation_context('gatekeeper', row, prediction))
        self.assertEqual(result['predictedLabel'], 'Benign')
        self.assertEqual(result['explainedClass'], 'Attack')
        self.assertEqual(result['outputUnits'], 'log-odds')

    def test_rejects_changed_input_release_order_probability_and_class(self):
        row, prediction = next((r, p) for r, p in zip(self.rows, self.predictions) if p.attack)
        context = self.models.explanation_context('multiclass', row, prediction)
        for change in ({'fingerprint': 'other'}, {'featureNames': context['featureNames'][::-1]},
                       {'probability': -1}, {'classIndex': (context['classIndex'] + 1) % 14},
                       {'featureValues': [float('nan')] * 120}):
            with self.subTest(change=next(iter(change))), self.assertRaises(ValueError):
                self.service.generate('multiclass', {**context, **change})

    def test_unknown_fallback_explains_actual_selected_model_class(self):
        row, original = next((r, p) for r, p in zip(self.rows, self.predictions) if p.attack)
        prediction = replace(original, attack_type='UnknownAttack')
        result = self.service.generate('multiclass', self.models.explanation_context('multiclass', row, prediction))
        self.assertEqual(result['predictedLabel'], 'UnknownAttack')
        self.assertEqual(result['explainedClass'], original.selected_label)

    def test_queue_snapshot_parameterization_and_unique_guard(self):
        store = Store.__new__(Store)
        store.conn = Mock()
        row, prediction = next((r, p) for r, p in zip(self.rows, self.predictions) if p.attack)
        store.enqueue_explanations(123, 55, row, prediction, self.models)
        self.assertEqual(store.conn.execute.call_count, 2)
        for call in store.conn.execute.call_args_list:
            sql, params = call.args
            self.assertIn('UPDLOCK,HOLDLOCK', sql)
            self.assertIn('WHERE NOT EXISTS', sql)
            self.assertEqual(params[:2], (123, 55))
            self.assertEqual(sql.count('?'), len(params))
            self.assertEqual(len(json.loads(params[5])['featureValues']), 120)


class FailureAndReferenceTests(unittest.TestCase):
    def test_generation_failure_isolated_from_detection(self):
        store, models, service = Mock(), Mock(), Mock()
        store.claim_explanation.return_value = dict(Id=9, Stage='multiclass', ContextJson='{}', AttackTrafficId=123, RawPacketId=55)
        service.generate.side_effect = ValueError('unsupported artifact')
        self.assertTrue(ExplanationWorker(store, models, service).once())
        store.fail_explanation.assert_called_once()
        store.finish_explanation.assert_not_called()

    def test_reference_rejects_evaluation_or_unidentified_sources(self):
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / 'manifest.json'
            for manifest in ({'split': 'test', 'provenance': 'holdout'}, {'split': 'training'}):
                path.write_text(json.dumps(manifest))
                with self.assertRaises(ValueError):
                    build_profile(Config(), path)

    def test_missing_or_invalid_reference_never_blocks_explanations(self):
        with tempfile.TemporaryDirectory() as tmp:
            config = replace(Config(), root=Path(tmp), reference_profile='reference.json')
            self.assertIsNone(load_reference(config))
            (Path(tmp) / 'reference.json').write_text('[]')
            self.assertIsNone(load_reference(config))
            (Path(tmp) / 'reference.json').write_text('{"schemaVersion": 1, "split": "test"}')
            self.assertIsNone(load_reference(config))


if __name__ == '__main__':
    unittest.main()
