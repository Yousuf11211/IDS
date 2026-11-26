/* Numerical charts only. Plotly is pinned and served locally. */
(() => {
    'use strict';
    const selectFeatures = (features, count, sort) => {
        const result = [...features].sort(sort === 'name' ? (a, b) => a.name.localeCompare(b.name) :
            sort === 'positive' ? (a, b) => b.contribution - a.contribution :
                (a, b) => Math.abs(b.contribution) - Math.abs(a.contribution));
        return count === 'all' ? result : result.slice(0, Number(count));
    };
    const difference = (feature) => {
        if (feature.reference?.median == null || feature.value == null || feature.name === 'dst_port' ||
            ['categorical', 'binary'].includes(feature.reference.kind)) return null;
        const delta = feature.value - feature.reference.median;
        return { delta, percent: feature.reference.median === 0 ? null : delta / Math.abs(feature.reference.median) * 100 };
    };
    const waterfallParts = (data, selected) => {
        const included = new Set(selected.map(f => f.name));
        const other = data.features.filter(f => !included.has(f.name));
        const parts = [{ name: 'Model base value', contribution: data.baseValue, total: true }, ...selected];
        if (other.length) parts.push({ name: `${other.length} other features (sum)`, contribution: other.reduce((sum, f) => sum + f.contribution, 0) });
        let running = data.baseValue;
        return parts.map((part, i) => {
            const start = i === 0 ? 0 : running;
            if (i !== 0) running += part.contribution;
            return { ...part, start, end: i === 0 ? data.baseValue : running };
        }).concat([{ name: 'Raw model output', contribution: data.rawMargin, start: 0, end: data.rawMargin, total: true }]);
    };
    if (typeof module !== 'undefined') module.exports = { selectFeatures, difference, waterfallParts };
    if (typeof document === 'undefined') return;
    const root = document.getElementById('inspection');
    if (!root) return;
    const byId = id => document.getElementById(id);
    const number = value => value == null ? 'Unavailable' : Number(value).toLocaleString(undefined, { maximumSignificantDigits: 9 });
    const percent = value => value == null ? 'Unavailable' : `${(value * 100).toFixed(2)}%`;
    const escape = value => String(value).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    const node = (tag, text, className) => {
        const result = document.createElement(tag);
        if (text != null) result.textContent = text;
        if (className) result.className = className;
        return result;
    };
    const chartConfig = { responsive: true, displaylogo: false, scrollZoom: false,
        modeBarButtonsToRemove: ['toImage', 'select2d', 'lasso2d'], showTips: false };
    const css = name => getComputedStyle(root).getPropertyValue(name).trim();
    const colors = () => ({ positive: css('--chart-malicious'), negative: css('--chart-benign'),
        neutral: css('--chart-total'), range: css('--ref-band'), median: css('--ref-median'), observed: css('--ref-observed') });
    const layout = (height, axis) => ({
        height, paper_bgcolor: 'transparent', plot_bgcolor: 'transparent',
        font: { family: 'Inter, system-ui, sans-serif', color: css('--text-primary'), size: 11 },
        margin: { l: window.innerWidth < 650 ? 155 : 250, r: 35, t: 20, b: 60 },
        xaxis: { title: { text: axis }, gridcolor: css('--border-color'), zerolinecolor: css('--text-muted'), automargin: true },
        yaxis: { autorange: 'reversed', automargin: true },
        hoverlabel: { bgcolor: css('--bg-card'), bordercolor: css('--border-color'), font: { color: css('--text-primary'), size: 12 } },
        showlegend: false
    });
    const featureName = feature => byId('technicalNames').checked ? feature.name : feature.metadata?.displayName ?? feature.name;
    const valueWithUnit = (value, feature) => value == null ? 'Missing input' : [number(value), feature.units].filter(Boolean).join(' ');
    const outsideInputScale = feature => feature.value != null &&
        ((feature.metadata?.expectedMinimum != null && feature.value < feature.metadata.expectedMinimum) ||
         (feature.metadata?.expectedMaximum != null && feature.value > feature.metadata.expectedMaximum));
    const observedValue = feature => outsideInputScale(feature) ? number(feature.value) + ' (recorded input)' :
        valueWithUnit(feature.value, feature);
    const scaleWarning = feature => outsideInputScale(feature)
        ? 'The recorded input falls outside the extractor’s documented ' + feature.units + ' scale.' : '';
    const frequencyLabel = (value, feature) => feature.reference?.kind === 'binary'
        ? (value === 0 ? '0 · clear' : '1 · set') : number(value);
    const referenceTableValue = feature => {
        const reference = feature.reference;
        if (!reference?.sampleCount) return '—';
        if (reference.frequencies?.length)
            return reference.observedCount != null ? percent(reference.observedCount / reference.sampleCount) + ' of benign flows' : 'See frequencies';
        return reference.median == null ? '—' : valueWithUnit(reference.median, feature);
    };
    let report, pollTimer, pollCount = 0, rendering = Promise.resolve(), disposed = false;
    const selectedStage = () => report.stages.find(s => s.stage === byId('stageSelect').value);
    const notice = (message, isError = false) => {
        byId('inspectionNotice').textContent = message;
        byId('inspectionNotice').classList.toggle('error', isError);
    };
    const direction = (feature, data) => feature.contribution === 0 ? 'No contribution to this output' :
        `${feature.contribution > 0 ? 'Increases' : 'Decreases'} ${data.explainedClass} ${data.outputUnits}`;
    const details = (feature, data) => {
        const info = feature.metadata;
        const reference = feature.reference;
        const change = outsideInputScale(feature) ? null : difference(feature);
        const lines = [info ? featureName(feature) + ' · ' + info.category : feature.name];
        if (info) {
            lines.push('Model feature: ' + feature.name, info.description);
        }
        lines.push('Observed: ' + observedValue(feature),
            'SHAP: ' + number(feature.contribution) + ' ' + data.outputUnits + ' · ' + direction(feature, data));
        if (outsideInputScale(feature)) lines.push(scaleWarning(feature));
        if (reference?.sampleCount && !reference.frequencies?.length) {
            lines.push('Benign median: ' + valueWithUnit(reference.median, feature));
            if (change != null) lines.push('Difference from benign median: ' + valueWithUnit(change.delta, feature));
        } else if (reference?.observedCount != null) {
            lines.push('Observed value frequency in benign training: ' + referenceTableValue(feature));
        }
        return lines.join('\n');
    };
    function modelCards() {
        const cards = byId('modelCards');
        cards.replaceChildren();
        for (const stageName of ['gatekeeper', 'multiclass']) {
            const stage = report.stages.find(s => s.stage === stageName), data = stage?.data;
            const binary = stageName === 'gatekeeper';
            const card = node('article', null, `model-card ${stageName}`);
            card.append(node('div', binary ? 'STAGE 1 / BINARY GATEKEEPER' : 'STAGE 2 / ATTACK CLASSIFIER', 'stage-label'),
                node('h3', binary ? 'Binary XGBoost' : 'Multiclass XGBoost'),
                node('div', data?.predictedLabel ?? (binary ? 'Attack · stored classification' : report.attackType), 'decision-label'),
                node('div', percent(data?.probability ?? (binary ? null : report.categoryProbability)), 'score'),
                node('p', binary ? 'Attack probability' : 'Selected category probability', 'inspection-muted'));
            if (data?.threshold != null) card.append(node('p', `${binary ? 'Attack decision threshold' : 'UnknownAttack fallback threshold'}: ${percent(data.threshold)}`, 'inspection-muted'));
            if (!binary && data?.explainedClass && data.explainedClass !== data.predictedLabel)
                card.append(node('p', `SHAP explains highest-scoring model class ${data.explainedClass}; the confidence policy returned ${data.predictedLabel}.`, 'inspection-muted'));
            if (!binary && data?.probability < 0.5)
                card.append(node('p', 'Review confidence: the selected class has less than 50% probability. This is a display cue, not a changed decision threshold.', 'inspection-muted'));
            if (data?.topProbabilities?.length) {
                const list = node('ol', null, 'probability-list');
                for (const item of data.topProbabilities) {
                    const li = node('li');
                    li.append(node('span', item.label), node('span', percent(item.probability)));
                    const track = node('span', null, 'probability-track'), fill = node('span');
                    fill.style.width = `${item.probability * 100}%`;
                    track.append(fill); li.append(track); list.append(li);
                }
                card.append(list);
            }
            card.append(node('p', !stage ? 'Explanation unavailable · historical flow may require verified backfill' :
                stage.status === 'completed' ? 'Explanation stored · additivity verified' :
                    stage.status === 'failed' ? 'Explanation unavailable' : 'Explanation processing', 'inspection-muted'));
            cards.append(card);
        }
        byId('summaryBinary').textContent = percent(report.stages.find(s => s.stage === 'gatekeeper')?.data?.probability);
        byId('summaryMulticlass').textContent = percent(report.categoryProbability);
    }
    const milliseconds = value => Number.isFinite(value) && value >= 0
        ? value.toLocaleString(undefined, { maximumFractionDigits: 3 }) + ' ms' : null;
    const utc = value => value ? new Date(value).toISOString().replace('T', ' ').replace('Z', ' UTC') : null;
    function pipeline() {
        const stages = report.stages;
        const binary = stages.find(stage => stage.stage === 'gatekeeper');
        const multiclass = stages.find(stage => stage.stage === 'multiclass');
        const timing = binary?.data?.pipelineTiming ?? multiclass?.data?.pipelineTiming;
        const batch = (value, count) => milliseconds(value) && count > 0
            ? milliseconds(value) + ' for ' + number(count) + (count === 1 ? ' flow' : ' flows') : 'Timing not recorded';
        const queue = [binary, multiclass].flatMap((stage, index) => {
            const time = milliseconds(stage?.data?.queueWaitMs);
            return time ? [(index === 0 ? 'Binary ' : 'Multiclass ') + time] : [];
        });
        const shap = [binary, multiclass].flatMap((stage, index) => {
            const time = milliseconds(stage?.data?.processingMs ?? stage?.data?.generationMs);
            return time ? [(index === 0 ? 'Binary ' : 'Multiclass ') + time] : [];
        });
        const values = [
            ['CSV ingestion', utc(report.csvReceivedAt) ?? 'Registration timestamp not recorded'],
            ['Feature extraction', 'Completed externally · duration was not recorded'],
            ['Binary classification', batch(timing?.binaryMs, timing?.binaryBatchSize)],
            ['Multiclass classification', batch(timing?.multiclassMs, timing?.multiclassBatchSize)],
            ['SHAP queue wait', queue.join(' · ') || 'First-attempt wait not recorded'],
            ['SHAP processing', shap.join(' · ') || (stages.some(stage => ['pending', 'processing'].includes(stage.status))
                ? 'Explanation processing' : 'Timing not recorded')]
        ];
        if (milliseconds(timing?.inferenceMs)) values.push(['Total batch inference', milliseconds(timing.inferenceMs)]);
        if (report.classificationRecordedAt) values.push(['Classification recorded', utc(report.classificationRecordedAt)]);
        const end = Math.max(...stages.map(stage => Date.parse(stage.data?.generatedAt)).filter(Number.isFinite));
        const elapsed = end - Date.parse(report.csvReceivedAt);
        if (timing && stages.length === 2 && stages.every(stage => stage.status === 'completed') && milliseconds(elapsed))
            values.push(['CSV receipt to SHAP result', milliseconds(elapsed)]);
        byId('pipelineSteps').replaceChildren(...values.map(([label, state]) => {
            const li = node('li', label);
            li.append(node('small', state));
            return li;
        }));
    }
    function technical() {
        const fields = Object.entries(report.storedFlow), search = byId('flowSearch').value.toLowerCase();
        byId('flowFields').replaceChildren(...fields.filter(([key]) => key.toLowerCase().includes(search)).map(([key, value]) => {
            const tr = node('tr'); tr.append(node('td', key), node('td', value == null || value === '' ? 'Unavailable / empty' : String(value))); return tr;
        }));
        byId('explanationMetadata').replaceChildren(...report.stages.map(stage => {
            const block = node('div'), data = stage.data;
            block.append(node('h3', stage.stage === 'gatekeeper' ? 'Binary XGBoost' : 'Multiclass XGBoost'));
            for (const line of [`Version: ${stage.modelVersion}`, `Release: ${data?.release ?? report.modelRelease ?? 'Unavailable'}`,
                `Contract fingerprint: ${data?.fingerprint ?? 'Unavailable'}`, `Status: ${stage.status} · attempts: ${stage.attempts}`,
                `Output: ${data?.outputUnits ?? 'Awaiting explanation'}`, `Generated: ${data?.generatedAt ?? 'Unavailable'}`,
                `Generation: ${number(data?.generationMs)} ms`, `SHAP ${data?.shapVersion ?? 'unavailable'} / XGBoost ${data?.xgboostVersion ?? 'unavailable'}`,
                `Input contract: ${data?.adapterNote ?? 'Unavailable'}`]) block.append(node('div', line));
            if (stage.error) block.append(node('p', stage.error, 'inspection-muted'));
            return block;
        }));
    }
    async function charts() {
        const stage = selectedStage(), data = stage?.data;
        const complete = stage?.status === 'completed' && data?.features?.length;
        byId('shapContent').hidden = !complete;
        byId('shapEmpty').hidden = !!complete;
        byId('contributionCaption').textContent = complete ?
            (stage.stage === 'gatekeeper' ? 'Binary XGBoost · ' : 'Multiclass XGBoost · ') +
            data.explainedClass + ' · ' + data.outputUnits + '. Each contribution shifts this raw score. Correlated features may share attribution.' : '';
        await prepareReference(complete ? data : null).catch(() =>
            notice('The reference chart could not be drawn. The recorded numerical evidence is available.', true));
        if (!complete) {
            byId('shapEmpty').textContent = !stage ? 'Explanation unavailable. Historical records require verified backfill with the original model release.' :
                stage.status === 'failed' ? 'Explanation unavailable. Review the recorded error before retrying.' : 'Explanation processing';
            return;
        }

        const sort = byId('featureSort').value;
        const features = sort === 'name' && !byId('technicalNames').checked
            ? [...data.features].sort((a, b) => featureName(a).localeCompare(featureName(b)))
            : data.features;
        const count = byId('featureCount').value;
        const selected = sort === 'name' && !byId('technicalNames').checked
            ? features.slice(0, count === 'all' ? undefined : Number(count))
            : selectFeatures(features, count, sort);
        const show = feature => { byId('featureDetail').textContent = details(feature, data); };
        if (selected.length) show(selected[0]);
        byId('featureEvidence').replaceChildren(...selected.map(feature => {
            const tr = node('tr'), td = node('td'), button = node('button', featureName(feature), 'feature-button');
            button.type = 'button';
            button.dataset.featureName = feature.name;
            button.title = feature.name;
            button.addEventListener('focus', () => show(feature));
            button.addEventListener('click', () => {
                show(feature);
                if (feature.reference?.sampleCount) {
                    byId('referenceFeature').value = feature.name;
                    queueReferenceRender();
                }
            });
            td.append(button);
            tr.append(td, node('td', observedValue(feature)),
                node('td', number(feature.contribution) + ' ' + data.outputUnits),
                node('td', referenceTableValue(feature)));
            return tr;
        }));
        byId('additivity').textContent = 'Base ' + number(data.baseValue) + ' + Σ contributions = ' +
            number(data.reconstructedMargin) + ' · model margin ' + number(data.rawMargin) +
            ' · absolute residual ' + number(data.additivityError);
        if (!window.Plotly) throw new Error('The local chart library could not be loaded. Numerical feature evidence is available below.');
        const palette = colors();
        const height = Math.max(340, selected.length * 29 + 100);
        const barLayout = layout(height, 'SHAP contribution · ' + data.outputUnits);
        barLayout.yaxis = { ...barLayout.yaxis, tickmode: 'array', tickvals: selected.map(f => f.name),
            ticktext: selected.map(f => escape(featureName(f))) };
        await Plotly.react(byId('shapBar'), [{ type: 'bar', orientation: 'h',
            x: selected.map(f => f.contribution), y: selected.map(f => f.name),
            marker: { color: selected.map(f => f.contribution >= 0 ? palette.positive : palette.negative) },
            customdata: selected.map(f => details(f, data).split('\n').map(escape).join('<br>')),
            hovertemplate: '%{customdata}<extra></extra>' }], barLayout, chartConfig);
        byId('shapBar').removeAllListeners('plotly_click');
        byId('shapBar').on('plotly_click', event => {
            const feature = selected[event.points[0].pointIndex];
            if (feature) show(feature);
        });
        const parts = waterfallParts(data, selected);
        const waterfall = layout(height + 90, 'Raw output · ' + data.outputUnits);
        waterfall.yaxis = { ...waterfall.yaxis, tickmode: 'array', tickvals: parts.map(p => p.name),
            ticktext: parts.map(p => escape(p.metadata ? featureName(p) : p.name)) };
        await Plotly.react(byId('shapWaterfall'), [{ type: 'bar', orientation: 'h',
            y: parts.map(p => p.name), x: parts.map(p => Math.abs(p.end - p.start)), base: parts.map(p => Math.min(p.start, p.end)),
            marker: { color: parts.map(p => p.total ? palette.neutral : p.contribution >= 0 ? palette.positive : palette.negative) },
            customdata: parts.map(p => escape(p.metadata ? featureName(p) : p.name) + '<br>' +
                (p.total ? 'Value' : 'Contribution') + ': ' + number(p.contribution) + ' ' + escape(data.outputUnits) +
                '<br>Running output: ' + number(p.end)),
            hovertemplate: '%{customdata}<extra></extra>' }], waterfall, chartConfig);
    }

    async function prepareReference(data) {
        const select = byId('referenceFeature');
        const previous = select.value;
        const available = data?.features?.filter(feature => feature.reference?.sampleCount > 0) ?? [];
        byId('referencePanel').hidden = available.length === 0;
        const caption = byId('referenceCaption');
        if (!available.length) {
            caption.textContent = data?.referenceDiagnostic ?? (data ?
                'Benign reference unavailable for this explanation.' : 'A completed explanation is required for benign comparison.');
            return;
        }
        const sample = data.reference?.benignSampleCount;
        caption.textContent = (data.reference?.datasetVersion ?? 'Stored benign reference') +
            (sample > 0 ? ' · ' + number(sample) + ' benign training flows' : '');
        const categories = new Map();
        for (const feature of available) {
            const name = feature.metadata?.category ?? 'Model inputs';
            if (!categories.has(name)) {
                const group = node('optgroup');
                group.label = name;
                categories.set(name, group);
            }
            const option = node('option', featureName(feature));
            option.value = feature.name;
            categories.get(name).append(option);
        }
        select.replaceChildren(...categories.values());
        select.value = available.some(feature => feature.name === previous) ? previous :
            selectFeatures(available, '1', 'magnitude')[0].name;
        await showReference(data);
    }

    async function showReference(data) {
        const feature = data?.features?.find(item => item.name === byId('referenceFeature').value);
        if (!feature?.reference?.sampleCount) return;
        const stats = feature.reference;
        byId('referenceDescription').textContent = (feature.metadata?.description ?? feature.name) +
            ' ' + number(stats.sampleCount) + ' valid samples' +
            (stats.missingCount == null ? '.' : ' · ' + number(stats.missingCount) + ' missing.') +
            (outsideInputScale(feature) ? ' ' + scaleWarning(feature) : '');
        const frequencyMode = !!stats.frequencies?.length;
        const chart = byId('referenceChart');
        byId('referenceFrequencies').hidden = !frequencyMode;
        byId('referenceLegend').hidden = frequencyMode || !window.Plotly;
        chart.hidden = frequencyMode || !window.Plotly;
        const metrics = [['Observed value', observedValue(feature)]];
        if (frequencyMode) {
            const freq = stats.observedCount != null ? referenceTableValue(feature) :
                (feature.value == null ? '—' : 'Outside the listed values');
            metrics.push(['Observed value frequency', freq]);
            const entries = [...stats.frequencies];
            if (stats.observedCount === 0 && !entries.some(item => item.value === feature.value))
                entries.push({ value: feature.value, count: 0 });
            if (stats.otherCount > 0) entries.push({ value: null, count: stats.otherCount });
            byId('referenceFrequencyRows').replaceChildren(...entries.map(item => {
                const row = node('tr');
                const observed = item.value != null && item.value === feature.value;
                if (observed) row.classList.add('frequency-observed');
                const share = node('td', null, 'frequency-share');
                const bar = node('progress');
                bar.max = stats.sampleCount;
                bar.value = item.count;
                bar.setAttribute('aria-hidden', 'true');
                share.append(bar, node('span', percent(item.count / stats.sampleCount)));
                row.append(node('td', item.value == null ? 'Other values' :
                    frequencyLabel(item.value, feature) + (observed ? ' · Observed' : '')),
                    node('td', number(item.count)), share);
                return row;
            }));
        } else {
            const position = feature.value == null ? 'Missing input' : feature.value < stats.p05 ? 'Below the benign range' :
                feature.value > stats.p95 ? 'Above the benign range' : 'Within the benign range';
            metrics.push(['Benign median', valueWithUnit(stats.median, feature)],
                ['5th percentile', valueWithUnit(stats.p05, feature)], ['95th percentile', valueWithUnit(stats.p95, feature)],
                ['Range position', position]);
        }
        byId('referenceMetrics').replaceChildren(...metrics.map(([label, value]) => {
            const group = node('div');
            group.append(node('dt', label), node('dd', value));
            return group;
        }));
        if (frequencyMode || !window.Plotly) return;

        const palette = colors();
        const traces = [{ type: 'scatter', mode: 'lines', x: [stats.p05, stats.p95], y: [0, 0],
            line: { color: palette.range, width: 13 },
            hovertemplate: 'Benign 5th–95th: ' + escape(valueWithUnit(stats.p05, feature)) + ' to ' +
                escape(valueWithUnit(stats.p95, feature)) + '<extra></extra>' },
        { type: 'scatter', mode: 'markers', x: [stats.median], y: [0],
            marker: { size: 15, color: palette.median, symbol: 'line-ns', line: { width: 3 } },
            hovertemplate: 'Benign median: ' + escape(valueWithUnit(stats.median, feature)) + '<extra></extra>' }];
        if (feature.value != null) traces.push({ type: 'scatter', mode: 'markers', x: [feature.value], y: [0.15],
            marker: { size: 11, color: palette.observed, symbol: 'diamond' },
            hovertemplate: 'Observed: ' + escape(observedValue(feature)) + '<extra></extra>' });
        const referenceLayout = layout(142, outsideInputScale(feature) ? 'Recorded model value' : feature.units ?? featureName(feature));
        referenceLayout.margin = { l: 16, r: 21, t: 12, b: 56 };
        referenceLayout.yaxis = { visible: false, range: [-0.45, 0.55], fixedrange: true };
        referenceLayout.xaxis.nticks = window.innerWidth < 650 ? 4 : 8;
        chart.setAttribute('aria-label', featureName(feature) + ' benign reference range and observed value. Values are listed above.');
        await Plotly.react(chart, traces, referenceLayout, chartConfig);
    }

    function queueReferenceRender() {
        rendering = rendering.then(() => showReference(selectedStage()?.data))
            .catch(() => notice('The reference chart could not be drawn. Numerical comparison is shown above.', true));
    }
    function render() {
        modelCards(); pipeline(); technical();
        rendering = rendering.then(charts).catch(error => notice(`Chart unavailable: ${error.message}`, true));
        return rendering;
    }
    function schedulePoll() {
        clearTimeout(pollTimer);
        if (!disposed && report.stages.some(s => ['pending', 'processing'].includes(s.status)) && pollCount < 20)
            pollTimer = setTimeout(() => refresh(false), Math.min(30000, 4000 + pollCount * 2000));
    }
    async function refresh(manual) {
        if (manual) pollCount = 0;
        byId('refreshInspection').disabled = true;
        try {
            const response = await fetch(root.dataset.endpoint, { credentials: 'same-origin', cache: 'no-store',
                headers: { Accept: 'application/json' }, signal: AbortSignal.timeout(15000) });
            if (!response.ok || response.redirected || !response.headers.get('content-type')?.includes('application/json'))
                throw new Error(response.status === 401 || response.status === 403 || response.redirected ?
                    'Your authorized session has ended. Sign in again to load this report.' : 'The report could not be loaded. Try Refresh report.');
            report = await response.json(); pollCount++;
            notice(report.stages.some(s => ['pending', 'processing'].includes(s.status)) ? 'Explanation processing' :
                'Report loaded from stored evidence.');
            await render(); schedulePoll();
        } catch (error) { notice(error.message || 'The report could not be loaded.', true); }
        finally { byId('refreshInspection').disabled = false; }
    }
    for (const id of ['stageSelect', 'featureCount', 'featureSort', 'technicalNames'])
        byId(id).addEventListener('change', () => render());
    byId('referenceFeature').addEventListener('change', queueReferenceRender);
    new MutationObserver(() => { if (report) render(); })
        .observe(document.documentElement, { attributes: true, attributeFilter: ['data-theme'] });
    byId('flowSearch').addEventListener('input', technical);
    byId('refreshInspection').addEventListener('click', () => refresh(true));
    window.addEventListener('pagehide', () => { disposed = true; clearTimeout(pollTimer); });
    try {
        report = JSON.parse(byId('inspectionData').textContent);
        notice(report.stages.some(s => ['pending', 'processing'].includes(s.status)) ? 'Explanation processing' :
            'Recorded detection · probabilities and contributions reflect the original model release.');
        render(); schedulePoll();
    } catch { notice('The stored report could not be loaded. Try Refresh report.', true); }
})();
