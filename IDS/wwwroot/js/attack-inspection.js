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
        if (!feature.reference || feature.value == null || feature.name === 'dst_port') return null;
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
    const number = value => value == null ? 'Unavailable' : Number(value).toLocaleString(undefined, { maximumSignificantDigits: 7 });
    const percent = value => value == null ? 'Unavailable' : `${(value * 100).toFixed(2)}%`;
    const escape = value => String(value).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    const node = (tag, text, className) => {
        const result = document.createElement(tag);
        if (text != null) result.textContent = text;
        if (className) result.className = className;
        return result;
    };
    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const colors = { positive: '#ef8391', negative: '#5bd5ef', neutral: '#75a9de' };
    const chartConfig = { responsive: true, displaylogo: false, scrollZoom: false,
        modeBarButtonsToRemove: ['toImage', 'select2d', 'lasso2d'], showTips: false };
    const layout = (height, axis) => ({
        height, paper_bgcolor: 'transparent', plot_bgcolor: 'transparent',
        font: { family: 'Inter, system-ui, sans-serif', color: '#b6c9dc', size: 11 },
        margin: { l: window.innerWidth < 650 ? 155 : 250, r: 35, t: 20, b: 65 },
        xaxis: { title: { text: axis }, gridcolor: '#49617e30', zerolinecolor: '#91b1d570', automargin: true },
        yaxis: { autorange: 'reversed', automargin: true },
        hoverlabel: { bgcolor: '#14263b', bordercolor: '#5782a6', font: { color: '#edf4fb', size: 12 } },
        transition: { duration: reducedMotion ? 0 : 250 }, showlegend: false
    });
    let report, pollTimer, pollCount = 0, rendering = Promise.resolve(), disposed = false;
    const selectedStage = () => report.stages.find(s => s.stage === byId('stageSelect').value);
    const notice = (message, isError = false) => {
        byId('inspectionNotice').textContent = message;
        byId('inspectionNotice').classList.toggle('error', isError);
    };
    const direction = (feature, data) => feature.contribution === 0 ? 'No contribution to this output' :
        `${feature.contribution > 0 ? 'Increases' : 'Decreases'} ${data.explainedClass} ${data.outputUnits}`;
    const details = (feature, data) => {
        const change = difference(feature);
        const reference = feature.reference;
        return `${feature.name}\nObserved: ${number(feature.value)} ${feature.units}\n` +
            `SHAP: ${number(feature.contribution)} ${data.outputUnits} · ${direction(feature, data)}\n` +
            (reference ? `Benign median: ${number(reference.median)} · 5th–95th: ${number(reference.p05)} to ${number(reference.p95)}\n` +
                `Difference: ${number(change?.delta)} · ${change?.percent == null ? 'Percentage unavailable (zero denominator or categorical value)' : `${number(change.percent)}%`}\n` +
                `Reference samples: ${reference.sampleCount}` : 'Reference unavailable');
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
    function pipeline() {
        const stages = report.stages, active = stages.some(s => ['pending', 'processing'].includes(s.status));
        const shapStatus = stages.length === 2 && stages.every(s => s.status === 'completed') ? 'Stored · additivity verified' :
            active ? 'Explanation processing' : 'Explanation unavailable';
        const values = [['Flow data', 'Stored'], ['Feature extraction', 'Upstream · timing unavailable'],
            ['Binary XGBoost', stages.some(s => s.stage === 'gatekeeper') ? 'Attack classification stored' : 'Stage metadata unavailable'],
            ['Multiclass XGBoost', stages.some(s => s.stage === 'multiclass') ? report.attackType : 'Stage metadata unavailable'],
            ['SHAP explanation', shapStatus], ['Incident report', 'Stored evidence loaded']];
        byId('pipelineSteps').replaceChildren(...values.map(([label, state], index) => {
            const li = node('li', label, active && index === 4 ? 'processing' : '');
            li.append(node('small', state)); return li;
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
            `Explained output: ${data.explainedClass} · ${data.outputUnits}. Contributions are not probability percentages. Correlated features may share attribution.` : '';
        byId('referenceCaption').textContent = 'Reference unavailable';
        const referenceNode = byId('referenceChart');
        if (window.Plotly && referenceNode.data) Plotly.purge(referenceNode);
        referenceNode.replaceChildren();
        if (!complete) {
            byId('shapEmpty').textContent = !stage ? 'Explanation unavailable. Historical records require a verified backfill using their original model release.' :
                stage.status === 'failed' ? 'Explanation unavailable. The detection is preserved; an operator can retry after resolving the recorded error.' : 'Explanation processing';
            return;
        }
        const selected = selectFeatures(data.features, byId('featureCount').value, byId('featureSort').value);
        byId('featureDetail').textContent = selected.length ? details(selected[0], data) : 'No explained features available.';
        const evidence = byId('featureEvidence');
        evidence.replaceChildren(...selected.map(feature => {
            const tr = node('tr'), td = node('td'), button = node('button', feature.name, 'feature-button');
            button.type = 'button';
            const show = () => { byId('featureDetail').textContent = details(feature, data); };
            button.addEventListener('focus', show); button.addEventListener('click', show);
            td.append(button); tr.append(td, node('td', number(feature.value)), node('td', `${number(feature.contribution)} ${data.outputUnits}`),
                node('td', feature.reference ? number(feature.reference.median) : 'Reference unavailable')); return tr;
        }));
        if (!window.Plotly) throw new Error('The local chart library could not be loaded. Numerical feature evidence is available below.');
        const height = Math.max(340, selected.length * 29 + 100);
        await Plotly.react(byId('shapBar'), [{ type: 'bar', orientation: 'h',
            x: selected.map(f => f.contribution), y: selected.map(f => f.name),
            marker: { color: selected.map(f => f.contribution >= 0 ? colors.positive : colors.negative) },
            customdata: selected.map(f => details(f, data).split('\n').map(escape).join('<br>')),
            hovertemplate: '%{customdata}<extra></extra>' }], layout(height, `SHAP contribution · ${data.outputUnits}`), chartConfig);
        byId('shapBar').removeAllListeners('plotly_click');
        byId('shapBar').on('plotly_click', event => {
            const feature = selected[event.points[0].pointIndex];
            if (feature) byId('featureDetail').textContent = details(feature, data);
        });
        const parts = waterfallParts(data, selected);
        await Plotly.react(byId('shapWaterfall'), [{ type: 'bar', orientation: 'h',
            y: parts.map(p => p.name), x: parts.map(p => Math.abs(p.end - p.start)), base: parts.map(p => Math.min(p.start, p.end)),
            marker: { color: parts.map(p => p.total ? colors.neutral : p.contribution >= 0 ? colors.positive : colors.negative) },
            customdata: parts.map(p => `${escape(p.name)}<br>${p.total ? 'Value' : 'Contribution'}: ${number(p.contribution)} ${escape(data.outputUnits)}<br>Running output: ${number(p.end)}`),
            hovertemplate: '%{customdata}<extra></extra>' }], layout(height + 90, `Raw output · ${data.outputUnits}`), chartConfig);
        byId('additivity').textContent = `Base ${number(data.baseValue)} + Σ contributions = ${number(data.reconstructedMargin)} · model margin ${number(data.rawMargin)} · absolute residual ${number(data.additivityError)}`;
        await referenceChart(selected, data);
    }
    async function referenceChart(selected, data) {
        const features = selected.filter(f => f.reference && f.value != null && f.name !== 'dst_port');
        if (!features.length) return;
        byId('referenceCaption').textContent = `${data.reference?.datasetVersion ?? 'Version unavailable'} · ${data.reference?.group ?? ''} · ${data.reference?.split ?? ''}. Statistics use verified labeled benign reference inputs.`;
        const traces = [], axes = {}, annotations = [], shapes = [], count = features.length;
        features.forEach((feature, i) => {
            const suffix = i ? String(i + 1) : '', xaxis = `x${suffix}`, yaxis = `y${suffix}`, r = feature.reference;
            axes[`xaxis${suffix}`] = { domain: [0, 1], anchor: yaxis, gridcolor: '#49617e30', zeroline: false, tickfont: { size: 10 } };
            axes[`yaxis${suffix}`] = { domain: [(count-i-1)/count + 0.28/count, (count-i)/count - 0.17/count],
                anchor: xaxis, range: [-1, 1], visible: false };
            // A separate independent x axis for every feature avoids comparing incompatible units.
            traces.push({ type: 'scatter', mode: 'lines', x: [r.p05, r.p95], y: [0, 0], xaxis, yaxis,
                line: { color: '#75a9de55', width: 18 }, hovertemplate: `Benign 5th–95th: ${number(r.p05)} to ${number(r.p95)}<extra></extra>` },
                { type: 'scatter', mode: 'markers', x: [r.median, feature.value], y: [0, 0], xaxis, yaxis,
                    marker: { size: [12, 10], color: [colors.negative, colors.positive], symbol: ['line-ns', 'diamond'], line: { width: 2 } },
                    customdata: [`Benign median: ${number(r.median)} · n=${r.sampleCount}`, details(feature, data).split('\n').map(escape).join('<br>')],
                    hovertemplate: '%{customdata}<extra></extra>' });
            annotations.push({ xref: 'paper', yref: `${yaxis} domain`, x: 0, y: 1.4, text: escape(feature.name),
                xanchor: 'left', showarrow: false, font: { color: '#b6c9dc', size: 11 } });
        });
        await Plotly.react(byId('referenceChart'), traces, { ...layout(Math.max(180, count * 100), ''),
            margin: { l: 25, r: 35, t: 45, b: 30 }, ...axes, annotations, shapes }, chartConfig);
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
    for (const id of ['stageSelect', 'featureCount', 'featureSort']) byId(id).addEventListener('change', () => render());
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
