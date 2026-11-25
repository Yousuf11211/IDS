(() => {
    const panel = document.querySelector('[data-analytics]');
    if (!panel) return;

    const select = panel.querySelector('[data-range]');
    const state = panel.querySelector('[data-chart-state]');
    const colors = ['#4b83ca', '#b75458', '#38887d', '#9075c0', '#ab722f', '#65808a'];
    let delay = 30000;
    let timer;
    let requested;
    let rendered = new Map();

    const safeText = text => String(text).replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;');
    const labelFor = value => ({ inprogress: 'In progress', processing: 'Processing', pending: 'Pending',
        completed: 'Completed', failed: 'Failed' }[value.toLowerCase()] || value);

    function chart(name, key, traces, options = {}) {
        const node = panel.querySelector(`[data-chart="${name}"]`);
        if (!node || rendered.get(name) === key) return;
        rendered.set(name, key);
        if (!traces.length) {
            if (node.data) Plotly.purge(node);
            node.textContent = 'No records in this time range.';
            node.classList.add('chart-empty');
            return;
        }

        if (node.classList.contains('chart-empty')) node.replaceChildren();
        node.classList.remove('chart-empty');
        const theme = getComputedStyle(panel);
        const axis = { gridcolor: theme.getPropertyValue('--border-color').trim(), automargin: true };
        const layout = {
            height: 255,
            margin: { l: 42, r: 12, t: 10, b: 45 },
            separator: '.,',
            paper_bgcolor: 'transparent',
            plot_bgcolor: 'transparent',
            font: { color: theme.color, family: theme.fontFamily, size: 12 },
            legend: { orientation: 'h', y: -0.3 },
            xaxis: axis,
            yaxis: { ...axis, rangemode: 'tozero', tickformat: 'd' },
            ...options
        };
        Plotly.react(node, traces, layout, { responsive: true, displayModeBar: false, scrollZoom: false });
    }

    function render(snapshot) {
        const { traffic, types, categories, incidents, explanations } = snapshot;
        const total = field => traffic.reduce((sum, bucket) => sum + bucket[field], 0);
        const counts = [total('benign'), total('malicious')];
        const firstBucket = Date.parse(traffic[0]?.start);
        const minutesPerBucket = (Date.parse(traffic[1]?.start) - firstBucket) / 60000;
        const bucket = Number.isFinite(minutesPerBucket) ? minutesPerBucket : 0;
        const times = traffic.map(row => row.start.replace('T', ' ').replace('Z', ''));
        chart('traffic', JSON.stringify([select.value, times, traffic.map(t => [t.benign, t.malicious])]), counts.some(Boolean)
            ? ['benign', 'malicious'].map((key, index) => ({
                x: times, y: traffic.map(row => row[key]),
                type: 'scatter', mode: 'lines+markers', name: index === 0 ? 'Benign' : 'Malicious',
                line: { color: colors[index], width: 2 },
                hovertemplate: '%{x} UTC · %{y:,} detections<extra>%{fullData.name}</extra>'
            })) : [], {
            xaxis: { type: 'date', nticks: 7, title: { text: `UTC · ${bucket}-minute buckets`, standoff: 7 } }
        });

        const doughnut = (name, labels, values) => chart(name,
            JSON.stringify([labels, values]), values.some(Boolean) ? [{
                type: 'pie', labels: labels.map(safeText), values, hole: 0.57, sort: false,
                marker: { colors }, textinfo: 'percent', hovertemplate: '%{label} · %{value:,}<extra></extra>'
            }] : []);
        doughnut('ratio', ['Benign', 'Malicious'], counts);
        doughnut('categories', categories.map(item => item.label), categories.map(item => item.count));
        doughnut('incidents', incidents.map(item => labelFor(item.label)), incidents.map(item => item.count));
        doughnut('explanations', explanations.map(item => labelFor(item.label)), explanations.map(item => item.count));
        chart('types', JSON.stringify(types), types.length ? [{
            type: 'bar', orientation: 'h', y: types.map(item => safeText(item.label)),
            x: types.map(item => item.count), marker: { color: colors[0] },
            hovertemplate: '%{y} · %{x:,} detections<extra></extra>'
        }] : [], { yaxis: { automargin: true, autorange: 'reversed', type: 'category' },
            xaxis: { title: { text: 'Detections' }, tickformat: 'd', rangemode: 'tozero' } });
        state.textContent = '';
    }

    async function refresh() {
        clearTimeout(timer);
        if (document.hidden) return;
        if (!window.Plotly) {
            state.textContent = 'Charts could not load.';
            return;
        }

        requested?.abort();
        const request = new AbortController();
        requested = request;
        const url = new URL(panel.dataset.url, location.origin);
        url.searchParams.set('range', select.value);
        try {
            const response = await fetch(url, { cache: 'no-store',
                signal: AbortSignal.any([request.signal, AbortSignal.timeout(12000)]) });
            if (!response.ok) throw new Error();
            const { snapshot, refreshSeconds } = await response.json();
            if (requested !== request) return;
            delay = refreshSeconds * 1000;
            render(snapshot);
        } catch {
            if (!request.signal.aborted) state.textContent = 'Analytics are temporarily unavailable.';
        } finally {
            if (requested === request) timer = setTimeout(refresh, delay);
        }
    }

    select.addEventListener('change', () => {
        state.textContent = 'Loading analytics…';
        for (const node of panel.querySelectorAll('[data-chart]')) {
            if (node.data) Plotly.purge(node);
            node.replaceChildren();
        }
        rendered = new Map();
        refresh();
    });
    document.addEventListener('visibilitychange', () => {
        if (!document.hidden) refresh();
    });
    refresh();
})();
