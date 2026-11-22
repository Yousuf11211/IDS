const test = require('node:test');
const assert = require('node:assert/strict');
const { selectFeatures, difference, waterfallParts } = require('../IDS/wwwroot/js/attack-inspection.js');

test('signed SHAP sorting and top feature controls preserve original input', () => {
    const features = [{ name: 'a', contribution: -7 }, { name: 'b', contribution: 2 }, { name: 'c', contribution: 4 }];
    assert.equal(selectFeatures(features, '1', 'magnitude')[0].name, 'a');
    assert.equal(selectFeatures(features, '1', 'positive')[0].name, 'c');
    assert.equal(selectFeatures(features, 'all', 'name').length, 3);
    assert.equal(features[0].name, 'a');
});
test('waterfall retains omitted contribution sum and reconstructs raw margin', () => {
    const features = [{ name: 'a', contribution: -7 }, { name: 'b', contribution: 2 }, { name: 'c', contribution: 4 }];
    const parts = waterfallParts({ features, baseValue: 3, rawMargin: 2 }, [features[0]]);
    assert.equal(parts[2].contribution, 6);
    assert.equal(parts.at(-2).end, 2);
    assert.equal(parts.at(-1).end, 2);
});
test('reference missing, zero median and categorical ports cannot produce misleading percentages', () => {
    assert.equal(difference({ name: 'a', value: 10 }), null);
    assert.equal(difference({ name: 'delta_start', value: null, reference: { median: 1 } }), null);
    assert.equal(difference({ name: 'dst_port', value: 443, reference: { median: 80 } }), null);
    assert.deepEqual(difference({ name: 'a', value: 10, reference: { median: 0 } }), { delta: 10, percent: null });
    assert.deepEqual(difference({ name: 'a', value: 30, reference: { median: 20 } }), { delta: 10, percent: 50 });
});
