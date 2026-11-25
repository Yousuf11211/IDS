(() => {
    const panel = document.querySelector('[data-analytics]');
    if (!panel) return;

    const live = panel.dataset.analytics === 'live';
    const select = panel.querySelector('[data-range]');
    const state = panel.querySelector('[data-chart-state]');
    const from = panel.querySelector('[data-from]');
    const through = panel.querySelector('[data-through]');
    const trendSelect = panel.querySelector('[data-trend-labels]');
    const trafficMode = panel.querySelector('[data-traffic-mode]');
    const pause = panel.querySelector('[data-pause]');
    const renderKeys = new Map();
    const formatter = new Intl.NumberFormat(undefined, { maximumFractionDigits: 2 });
    const format = value => formatter.format(value);
    const percentage = (count, total) => total ? format(count * 100 / total) + '%' : '—';
    const date = value => value.replace('T', ' ').slice(0, 16);
    const safe = value => String(value).replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;');
    const el = (tag, value) => {
        const node = document.createElement(tag);
        if (value != null) node.textContent = value;
        return node;
    };
    let snapshot, request, timer;
    let delay = 30000;
    let autoRefresh = true;
    let paused = false;
    let lastStarted = 0;

    const categoryColor = label => window.IDS.attackColor(label);
    function style() { return getComputedStyle(panel); }
    function chart(name, traces, extra = {}) {
        const node = panel.querySelector(`[data-chart="${name}"]`);
        if (!node) return;
        const theme = style();
        const key = JSON.stringify([document.documentElement.dataset.theme, traces, extra]);
        if (renderKeys.get(name) === key) return;
        renderKeys.set(name, key);
        if (!traces.length || !window.Plotly) {
            if (node.data) Plotly.purge(node);
            node.classList.add('chart-empty');
            node.textContent = window.Plotly ? 'No records in this window.' : 'Chart unavailable.';
            return;
        }
        if (node.classList.contains('chart-empty')) node.replaceChildren();
        node.classList.remove('chart-empty');
        const css = name => theme.getPropertyValue(name).trim();
        const axes = { gridcolor: css('--border-color'), automargin: true, zerolinecolor: css('--border-color') };
        const { xaxis, yaxis, ...layout } = extra;
        Plotly.react(node, traces, {
            height: 235, margin: { l: 44, r: 16, t: 30, b: 58 }, autosize: true,
            font: { color: css('--text-primary'), family: theme.fontFamily, size: 12 },
            paper_bgcolor: 'transparent', plot_bgcolor: 'transparent',
            hoverlabel: { bgcolor: css('--bg-card'), bordercolor: css('--border-color'), font: { color: css('--text-primary') } },
            legend: { orientation: 'h', y: 1.08, yanchor: 'bottom' },
            xaxis: { ...axes, ...xaxis }, yaxis: { ...axes, rangemode: 'tozero', tickformat: ',d', ...yaxis },
            uirevision: select.value, ...layout
        }, { responsive: true, displayModeBar: false, scrollZoom: false });
    }
    function line(name, color, times, values) {
        return { type: 'scatter', mode: 'lines+markers', name: safe(name), x: times, y: values,
            marker: { size: 3 }, line: { color, width: 2 },
            hovertemplate: '%{x} UTC · %{y:,} flows<extra>%{fullData.name}</extra>' };
    }
    function donut(name, rows, colors) {
        chart(name, rows.some(row => row.count > 0) ? [{
            type: 'pie', labels: rows.map(row => safe(row.label)), values: rows.map(row => row.count),
            hole: 0.62, sort: false, marker: { colors, line: { width: 1, color: style().getPropertyValue('--bg-card').trim() } }, textinfo: 'none',
            hovertemplate: '%{label} · %{value:,} · %{percent}<extra></extra>'
        }] : [], { legend: { orientation: 'h', y: -0.1 }, margin: { l: 8, r: 8, t: 8, b: 62 } });
    }
    function metric(key, value, context = '') {
        const node = panel.querySelector(`[data-metric="${key}"]`);
        const caption = panel.querySelector(`[data-context="${key}"]`);
        if (node) node.textContent = value;
        if (caption) caption.textContent = context;
    }
    function list(name, rows) {
        const list = panel.querySelector(`[data-insight="${name}"]`);
        if (list) list.replaceChildren(...(rows.length ? rows : [['No observations', '—']]).map(([label, value]) => {
            const row = el('div');
            const term = el('dt');
            term.append(label);
            row.append(term, el('dd', value));
            return row;
        }));
    }
    function classLink(type, data) {
        const url = new URL(panel.dataset.history, location.origin);
        url.searchParams.set('category', type.label);
        url.searchParams.set('from', data.from);
        url.searchParams.set('to', data.to);
        const link = el('a', type.label);
        link.href = url;
        link.className = 'category-mark';
        link.style.setProperty('--category-color', categoryColor(type.label));
        return link;
    }
    function render(data) {
        const { totals, previous, traffic, types, incidents, incidentMetrics, explanations, sources, ports } = data;
        const total = totals.benign + totals.malicious;
        const priorTotal = previous ? previous.benign + previous.malicious : 0;
        const prior = value => previous ? `${format(value)} in prior window` : '';
        metric('total', format(total), prior(priorTotal));
        metric('benign', format(totals.benign), prior(previous?.benign));
        metric('malicious', format(totals.malicious), prior(previous?.malicious));
        metric('ratio', percentage(totals.malicious, total), previous ? `${percentage(previous.malicious, priorTotal)} in prior window` : '');
        metric('classes', format(types.filter(type => type.label !== 'Unclassified').length));
        metric('rate', total ? format(total / Math.max(1, (Date.parse(data.to) - Date.parse(data.from)) / 60000)) : '0');
        const jobs = status => explanations.find(row => row.label === status)?.count ?? 0;
        metric('pending', format(jobs('pending')));
        if (!paused) state.textContent = `${date(data.from)} – ${date(data.to)} UTC`;
        const css = key => style().getPropertyValue(key).trim();
        const times = traffic.map(row => row.start.replace('T', ' ').replace('Z', ''));
        const axes = { xaxis: { type: 'date', nticks: 6, title: { text: `UTC · ${data.bucketMinutes}-minute intervals`, standoff: 12 } } };
        const benign = css('--chart-benign');
        const malicious = css('--chart-malicious');
        chart('traffic', !total ? [] : trafficMode.value === 'total'
            ? [line('Total', css('--chart-total'), times, traffic.map(row => row.benign + row.malicious))]
            : [line('Benign', benign, times, traffic.map(row => row.benign)),
                line('Malicious', malicious, times, traffic.map(row => row.malicious))], axes);

        donut('ratio', [{ label: 'Benign', count: totals.benign }, { label: 'Malicious', count: totals.malicious }], [benign, malicious]);
        donut('distribution', types, types.map(type => categoryColor(type.label)));
        chart('types', types.length ? [{
            type: 'bar', orientation: 'h', y: types.map(type => safe(type.label)), x: types.map(type => type.count),
            marker: { color: types.map(type => categoryColor(type.label)) }, text: types.map(type => format(type.count)),
            textposition: 'outside', cliponaxis: false,
            customdata: types.map(type => percentage(type.count, totals.malicious)),
            hovertemplate: '%{y} · %{x:,} detections · %{customdata}<extra></extra>'
        }] : [], { height: Math.max(225, types.length * 29 + 52), margin: { l: 120, r: 50, t: 8, b: 35 },
            yaxis: { autorange: 'reversed', type: 'category' }, xaxis: { rangemode: 'tozero', tickformat: ',d' } });

        const rows = types.map(type => {
            const row = el('tr');
            const name = el('td');
            name.append(classLink(type, data));
            row.append(name, el('td', format(type.count)), el('td', percentage(type.count, totals.malicious)));
            if (!live) row.append(el('td', date(type.first)), el('td', date(type.last)));
            return row;
        });
        if (!rows.length) {
            const row = el('tr');
            const cell = el('td', 'No malicious flows were recorded in this window.');
            cell.colSpan = live ? 3 : 5;
            row.append(cell);
            rows.push(row);
        }
        panel.querySelector('[data-class-rows]').replaceChildren(...rows);
        if (trendSelect) {
            const selected = new Set(Array.from(trendSelect.selectedOptions, option => option.value));
            const available = types.map(type => type.label);
            if (Array.from(trendSelect.options, option => option.value).join('\0') !== available.join('\0')) {
                trendSelect.replaceChildren(...available.map((label, index) => new Option(label, label, false,
                    selected.has(label) || (!selected.size && index === 0))));
            }
            const chosen = new Set(Array.from(trendSelect.selectedOptions, option => option.value).slice(0, 6));
            chart('trends', types.filter(type => chosen.has(type.label)).map(type =>
                line(type.label, categoryColor(type.label), times, type.trend)), axes);
        }
        list('sources', sources.map(row => [row.label, format(row.count)]));
        list('ports', ports.map(row => [row.label, format(row.count)]));
        list('recent', [...types].sort((a, b) => b.last.localeCompare(a.last)).slice(0, 5)
            .map(row => [classLink(row, data), date(row.last)]));
        const labels = { open: 'Open or under investigation', 'high-priority': 'Active high or critical',
            assigned: 'Assigned', unassigned: 'Unassigned', escalated: 'Escalated', InProgress: 'Under investigation', Resolved: 'Resolved' };
        list('incidents', incidentMetrics.map(row => {
            const link = el('a', labels[row.label]);
            const url = new URL(panel.dataset.tickets, location.origin);
            url.search = new URLSearchParams({ incident: row.label, from: data.from, to: data.to });
            link.href = url;
            return [link, format(row.count)];
        }));
        donut('incidents', incidents.map(row => ({ ...row, label: row.label === 'InProgress' ? 'In progress' : row.label })),
            [css('--chart-total'), benign, malicious, '#8b7597']);
        const explanationLink = panel.querySelector('[data-explanation-link]');
        if (explanationLink) {
            const url = new URL(explanationLink.href);
            url.searchParams.set('from', data.from);
            url.searchParams.set('to', data.to);
            explanationLink.href = url;
        }
        list('explanations', [['Completed', format(jobs('completed'))], ['Pending', format(jobs('pending'))],
            ['Processing', format(jobs('processing'))], ['Failed', format(jobs('failed'))],
            ['Completion', percentage(jobs('completed'), explanations.reduce((sum, row) => sum + row.count, 0))]]);
    }
    async function refresh(manual = false) {
        clearTimeout(timer);
        timer = null;
        if (document.hidden || (paused && !manual)) return;
        if (select.value === 'custom' && (!from.value || !through.value || from.value > through.value)) {
            state.textContent = 'Choose valid start and end dates.';
            return;
        }
        lastStarted = Date.now();
        request?.abort();
        const current = new AbortController();
        request = current;
        const url = new URL(panel.dataset.url, location.origin);
        if (select.value) url.searchParams.set('range', select.value);
        url.searchParams.set('live', live);
        if (select.value === 'custom') {
            url.searchParams.set('from', from.value);
            url.searchParams.set('through', through.value);
        }
        try {
            const response = await fetch(url, { signal: AbortSignal.any([current.signal, AbortSignal.timeout(15000)]) });
            if (!response.ok) throw new Error(response.status === 400 ? 'Choose a valid UTC date range.' : 'Analytics are temporarily unavailable.');
            const payload = await response.json();
            if (request !== current) return;
            if (!select.value) select.value = payload.range;
            if (!snapshot && ['split', 'total'].includes(payload.trafficSeries)) trafficMode.value = payload.trafficSeries;
            snapshot = payload.snapshot;
            delay = payload.refreshSeconds * 1000;
            autoRefresh = payload.autoRefresh;
            render(snapshot);
            panel.dispatchEvent(new CustomEvent('monitoring:updated', { detail: { ...payload, paused }, bubbles: true }));
        } catch (error) {
            if (!current.signal.aborted) state.textContent = error.message.startsWith('Choose') ? error.message : 'Analytics are temporarily unavailable.';
        } finally {
            if (request === current && !paused && autoRefresh) {
                clearTimeout(timer);
                timer = setTimeout(refresh, delay);
            }
        }
    }
    select.addEventListener('change', () => {
        panel.querySelector('[data-custom]').hidden = select.value !== 'custom';
        refresh(true);
    });
    panel.querySelector('[data-window-form]').addEventListener('submit', event => { event.preventDefault(); refresh(true); });
    pause?.addEventListener('click', () => {
        paused = !paused;
        pause.setAttribute('aria-pressed', String(paused));
        pause.textContent = paused ? 'Resume refresh' : 'Pause refresh';
        if (paused) {
            request?.abort();
            clearTimeout(timer);
            state.textContent = 'Live updates paused.';
            panel.dispatchEvent(new CustomEvent('monitoring:paused', { bubbles: true }));
        } else refresh(true);
    });
    trendSelect?.addEventListener('change', () => { if (snapshot) render(snapshot); });
    trafficMode.addEventListener('change', () => { if (snapshot) render(snapshot); });
    new MutationObserver(() => { if (snapshot) render(snapshot); })
        .observe(document.documentElement, { attributes: true, attributeFilter: ['data-theme'] });
    document.addEventListener('visibilitychange', () => { if (!document.hidden && (live || autoRefresh)) refresh(); });
    document.addEventListener('monitoring:activity', () => {
        if (live && !paused && !document.hidden && Date.now() - lastStarted >= delay) refresh();
    });
    refresh();
})();
