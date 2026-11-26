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
from test_pipeline import FEATURES, fixture, validate


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
                self.assertEqual(result['featureNames'], FEATURES)
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

    def test_missing_input_and_reversed_binary_mapping(self):
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / 'reversed.json'
            path.write_text('{"1": "Benign", "0": "Attack"}')
            reversed_models = load_models(replace(Config(), gatekeeper_labels=str(path)))
        service = ExplanationService(reversed_models)
        row = {**self.rows[0], "delta_start": None, "delta_start_incomplete_handshake": 1}
        original = self.models.predict([row])[0]
        reversed_prediction = reversed_models.predict([row])[0]
        self.assertAlmostEqual(1 - original.gate_score, reversed_prediction.gate_score, places=6)
        context = reversed_models.explanation_context('gatekeeper', row, reversed_prediction)
        payload = service.generate('gatekeeper', json.loads(json.dumps(context, allow_nan=False)))
        self.assertEqual(payload['classIndex'], 0)
        self.assertAlmostEqual(1 / (1 + np.exp(-payload['rawMargin'])), reversed_prediction.gate_score, places=5)
        missing = next(f for f in payload['features'] if f['name'] == 'delta_start')
        self.assertIsNone(missing['value'])
        json.dumps(payload, allow_nan=False)

    def test_benign_binary_explains_attack_score_without_changing_label(self):
        row, prediction = next((r, p) for r, p in zip(self.rows, self.predictions) if not p.attack)
        result = self.service.generate('gatekeeper', self.models.explanation_context('gatekeeper', row, prediction))
        self.assertEqual(result['predictedLabel'], 'Benign')
        self.assertEqual(result['explainedClass'], 'Attack')
        self.assertEqual(result['outputUnits'], 'log-odds')

    def test_rejects_changed_input_release_order_probability_and_class(self):
        row, prediction = next((r, p) for r, p in zip(self.rows, self.predictions) if p.attack)
        context = self.models.explanation_context('multiclass', row, prediction)
        for change in ({'fingerprint': 'other'}, {'modelVersion': self.models.gate_version}, {'predictedLabel': 'wrong'},
                       {'featureNames': context['featureNames'][::-1]},
                       {'probability': -1}, {'classIndex': (context['classIndex'] + 1) % len(self.models.labels['multiclass'])},
                       {'featureValues': [float('nan')] * 120}):
            with self.subTest(change=next(iter(change))), self.assertRaises(ValueError):
                self.service.generate('multiclass', {**context, **change})

    def test_unknown_fallback_explains_actual_selected_model_class(self):
        models = load_models(replace(Config(), multiclass_threshold=1.0))
        row = next(r for r, p in zip(self.rows, self.predictions) if p.attack)
        prediction = models.predict([row])[0]
        result = ExplanationService(models).generate('multiclass', models.explanation_context('multiclass', row, prediction))
        self.assertEqual(result['predictedLabel'], 'UnknownAttack')
        self.assertEqual(result['explainedClass'], prediction.selected_label)

    def test_training_profile_aligns_with_both_models_and_keeps_payload_small(self):
        config = replace(Config(), reference_profile='config/benign-reference.json')
        profile = load_reference(config, self.models)
        self.assertEqual(profile['featureNames'], self.models.feature_names)
        self.assertEqual(profile['benignSampleCount'], 362127)
        self.assertEqual(sum(source['benignRows'] for source in profile['sources']), 362127)
        self.assertTrue(all(source['file'].split('/')[0] in ('IDS2018', 'Created_Data') for source in profile['sources']))
        for stats in profile['features'].values():
            self.assertGreater(stats['sampleCount'], 0)
            self.assertEqual(stats['sampleCount'] + stats['missingCount'], 362127)
        port = profile['features']['dst_port']
        self.assertEqual(port['kind'], 'categorical')
        self.assertEqual(sum(entry['count'] for entry in port['frequencies']), port['sampleCount'])
        self.assertGreater(profile['features']['delta_start']['missingCount'], 0)

        row, prediction = next((r, p) for r, p in zip(self.rows, self.predictions) if p.attack)
        timed = {}
        self.assertEqual(self.models.predict([row], timings=timed), [prediction])
        self.assertEqual(timed['binaryBatchSize'], 1)
        self.assertEqual(timed['multiclassBatchSize'], 1)
        self.assertGreaterEqual(timed['inferenceMs'], timed['binaryMs'] + timed['multiclassMs'])
        context = self.models.explanation_context('multiclass', row, prediction)
        result = ExplanationService(self.models, profile).generate('multiclass', context)
        self.assertEqual(result['reference']['datasetSha256'], profile['datasetSha256'])
        self.assertTrue(all(feature['reference'] is None for feature in result['features']))
        self.assertLess(result['additivityError'], 2e-4)

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
        store.claim_explanation.return_value = dict(Id=9, Stage='multiclass', ContextJson='{"modelVersion":"v","attackId":"123","rawId":"55"}',
                                                    ModelVersion='v', AttackTrafficId=123, RawPacketId=55)
        service.generate.side_effect = ValueError('unsupported artifact')
        self.assertTrue(ExplanationWorker(store, models, service).once())
        service.generate.assert_called_once()
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
