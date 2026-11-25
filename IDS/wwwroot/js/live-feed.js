(() => {
    const panel = document.querySelector('[data-feed]');
    const analytics = document.querySelector('[data-analytics="live"]');
    if (!panel || !analytics) return;
    const find = query => panel.querySelector(query);
    const form = find('[data-feed-form]');
    const status = find('[data-feed-status]');
    let range, asOf, request;
    let paused = false;
    let page = 1;
    let filters = new URLSearchParams();
    const rendered = new Map();
    const node = (tag, value) => {
        const item = document.createElement(tag);
        if (value != null) item.textContent = value;
        return item;
    };
    const link = (url, text) => {
        const item = node('a', text);
        item.href = url;
        return item;
    };
    const idLink = (path, id) => {
        const url = new URL(path, location.origin);
        if (url.searchParams.has('id')) url.searchParams.set('id', id);
        else url.pathname = url.pathname.replace(/\/1\/?$/, '/' + id);
        return url;
    };
    const queryLink = (path, key, value) => {
        const url = new URL(path, location.origin);
        url.searchParams.set(key, value);
        return url;
    };
    function render(target, rows, recent) {
        const key = JSON.stringify(rows);
        if (rendered.get(target) === key) return;
        rendered.set(target, key);
        const output = rows.map(row => {
            const tr = node('tr');
            const timestamp = row.timestamp.replace('T', ' ').slice(0, 19);
            const category = node('td', row.category || '—');
            if (row.category) category.dataset.attackLabel = row.category;
            const confidence = row.confidence == null ? '—' : new Intl.NumberFormat(undefined, {
                style: 'percent', maximumFractionDigits: 1
            }).format(row.confidence);
            const incident = node('td');
            if (row.ticketId) incident.append(link(idLink(panel.dataset.ticket, row.ticketId), 'View incident'));
            else incident.textContent = '—';
            const action = node('td');
            if (row.isAttack) {
                action.append(panel.dataset.inspect
                    ? link(idLink(panel.dataset.inspect, row.id), 'Inspect')
                    : link(queryLink(panel.dataset.history, 'id', row.id), 'View'));
                if (!row.ticketId && row.escalation && ['High', 'Critical'].includes(row.severity)) {
                    action.append(' · ', link(idLink(panel.dataset.escalate, row.id), 'Escalate'));
                }
            } else action.textContent = '—';
            const base = [node('td', row.source || '—'), node('td', row.port ?? '—')];
            const details = [node('td', confidence), node('td', row.severity || '—'), node('td', row.explanation || '—')];
            tr.append(node('td', timestamp));
            if (recent) tr.append(category, ...base, ...details, action);
            else tr.append(...base, node('td', row.isAttack ? 'Malicious' : 'Benign'), category, ...details, incident, action);
            return tr;
        });
        if (!output.length) {
            const row = node('tr');
            const cell = node('td', recent ? 'No malicious activity in this window.' : 'No detections match this window and filters.');
            cell.colSpan = recent ? 8 : 10;
            row.append(cell);
            output.push(row);
        }
        target.replaceChildren(...output);
        window.IDS?.applyAttackColors?.(target);
    }
    async function refresh(manual = false, preserve = false) {
        if (!range || (paused && !manual)) return;
        request?.abort();
        const current = new AbortController();
        request = current;
        const url = new URL(panel.dataset.url, location.origin);
        url.search = filters;
        url.searchParams.set('range', range);
        url.searchParams.set('page', page);
        if ((page > 1 || preserve) && asOf) url.searchParams.set('asOf', asOf);
        try {
            const response = await fetch(url, { signal: AbortSignal.any([current.signal, AbortSignal.timeout(12000)]) });
            if (!response.ok) throw new Error();
            const data = await response.json();
            if (request !== current) return;
            asOf = data.asOf;
            render(find('[data-feed-rows]'), data.rows.map(row => ({ ...row, escalation: data.escalation })), false);
            render(find('[data-feed-recent]'), data.recent.map(row => ({ ...row, escalation: data.escalation })), true);
            find('[data-recent-section]').hidden = !data.recentEnabled;
            find('[data-page-step="-1"]').disabled = page === 1;
            find('[data-page-step="1"]').disabled = !data.hasNext;
            find('[data-page-number]').textContent = `Page ${page}`;
            status.textContent = page > 1 ? 'Historical page held at ' + asOf.slice(11, 19) + ' UTC' : paused ? 'Live updates paused.' : '';
        } catch {
            if (!current.signal.aborted) status.textContent = 'Detection records are temporarily unavailable.';
        }
    }
    analytics.addEventListener('monitoring:updated', event => {
        const payload = event.detail;
        const changed = range !== payload.range;
        range = payload.range;
        paused = payload.paused;
        if (changed) { page = 1; asOf = null; }
        const classes = form.elements.category;
        const selection = classes.value;
        const values = payload.snapshot.types.map(type => type.label);
        if (selection && !values.includes(selection)) values.push(selection);
        if (Array.from(classes.options).slice(1).map(option => option.value).join('\0') !== values.join('\0')) {
            classes.replaceChildren(new Option('All classes', ''), ...values.map(value => new Option(value, value)));
            classes.value = selection;
        }
        if (page === 1 || changed) {
            asOf = payload.snapshot.to;
            refresh(changed || !paused, true);
        }
    });
    analytics.addEventListener('monitoring:paused', () => {
        paused = true;
        request?.abort();
        status.textContent = 'Live updates paused.';
    });
    form.addEventListener('submit', event => {
        event.preventDefault();
        filters = new URLSearchParams(new FormData(form));
        page = 1;
        refresh(true, paused);
    });
    form.addEventListener('reset', () => { filters = new URLSearchParams(); page = 1; refresh(true, paused); });
    panel.querySelectorAll('[data-page-step]').forEach(button => button.addEventListener('click', () => {
        page += Number(button.dataset.pageStep);
        refresh(true, true);
    }));
    find('[data-refresh-feed]').addEventListener('click', () => { page = 1; refresh(true); });
    find('[data-hide-feed]').addEventListener('click', () => {
        request?.abort();
        rendered.clear();
        find('[data-feed-rows]').replaceChildren();
        find('[data-feed-recent]').replaceChildren();
        status.textContent = 'Display cleared.';
    });

    const connectionStatus = find('[data-feed-connection]');
    const desktop = find('[data-feed-desktop]');
    desktop.addEventListener('change', async () => {
        if (desktop.checked && (!('Notification' in window) || await Notification.requestPermission() !== 'granted')) desktop.checked = false;
    });
    let audio, lastSignal = 0;
    const sound = find('[data-feed-sound]');
    sound.addEventListener('change', () => {
        if (sound.checked && window.AudioContext) {
            audio ??= new AudioContext();
            audio.resume().catch(() => {});
        }
    });
    if (!window.signalR) {
        connectionStatus.textContent = 'Live event connection unavailable. Scheduled refresh remains active.';
        return;
    }
    const connection = new signalR.HubConnectionBuilder().withUrl(panel.dataset.hub).withAutomaticReconnect().build();
    connection.on('NewDetection', event => {
        document.dispatchEvent(new Event('monitoring:activity'));
        if (paused || !event.isAttack || Date.now() - lastSignal < 10000) return;
        lastSignal = Date.now();
        if (desktop.checked && Notification.permission === 'granted') new Notification('IDS Security · Malicious flow detected');
        if (sound.checked && audio?.state === 'running') {
            const tone = audio.createOscillator();
            const volume = audio.createGain();
            tone.frequency.value = 480;
            volume.gain.value = 0.1;
            tone.connect(volume).connect(audio.destination);
            tone.start();
            tone.stop(audio.currentTime + 0.16);
        }
    });
    const start = () => connection.start()
        .then(() => { connectionStatus.textContent = 'Live event connection active'; })
        .catch(() => { connectionStatus.textContent = 'Reconnecting to live events. Scheduled refresh remains active.'; setTimeout(start, 15000); });
    connection.onreconnecting(() => { connectionStatus.textContent = 'Reconnecting to live events…'; });
    connection.onreconnected(() => { connectionStatus.textContent = 'Live event connection active'; });
    connection.onclose(() => setTimeout(start, 15000));
    start();
})();
