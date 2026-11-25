(() => {
    const panel = document.querySelector('[data-alert-panel]');
    if (!panel) return;
    const list = panel.querySelector('[data-alert-list]');
    const state = panel.querySelector('[data-alert-state]');
    const counter = panel.querySelector('[data-new-alerts]');
    const controls = panel.querySelector('[data-alert-controls]');
    let displayed;
    let highestSeen;
    let running = false;
    let newAlerts = 0;

    function clear() {
        newAlerts = 0;
        counter.hidden = controls.hidden = true;
    }

    function render(alerts, enabled) {
        if (list.contains(document.activeElement)) return;
        const items = JSON.stringify(alerts);
        if (displayed === items && state.hidden && enabled && alerts.length) return;
        displayed = items;
        state.textContent = !enabled ? 'Recent alerts are disabled.' :
            alerts.length ? '' : 'No detections recorded.';
        state.hidden = enabled && alerts.length > 0;
        if (!enabled) clear();

        if (alerts.length) {
            const ids = alerts.map(alert => BigInt(alert.id));
            if (highestSeen !== undefined) newAlerts = Math.min(100, newAlerts + ids.filter(id => id > highestSeen).length);
            highestSeen = [highestSeen ?? 0n, ...ids].reduce((max, id) => id > max ? id : max);
            counter.hidden = controls.hidden = newAlerts === 0;
            counter.textContent = newAlerts > 99 ? '99+ new' : `${newAlerts} new`;
        }

        list.replaceChildren(...alerts.map(alert => {
            const link = document.createElement('a');
            link.className = 'detection-alert-row';
            const url = new URL(panel.dataset.history, location.origin);
            url.searchParams.set('id', alert.id);
            link.href = url;
            const title = document.createElement('strong');
            title.textContent = alert.type;
            const severity = document.createElement('span');
            const style = ['critical', 'high', 'medium', 'low'].includes(alert.severity.toLowerCase())
                ? alert.severity.toLowerCase() : 'medium';
            severity.className = `severity-badge severity-${style}`;
            severity.textContent = alert.severity;
            const detail = document.createElement('small');
            detail.textContent = `${alert.source || 'Source unavailable'} · Port ${alert.port ?? 'unavailable'}`;
            const time = document.createElement('time');
            const timestamp = new Date(alert.timestamp.endsWith('Z') ? alert.timestamp : alert.timestamp + 'Z');
            time.dateTime = timestamp.toISOString();
            time.textContent = timestamp.toLocaleString();
            link.append(title, severity, detail, time);
            return link;
        }));
    }

    async function refresh() {
        if (running || document.hidden) return;
        running = true;
        try {
            const response = await fetch(panel.dataset.url, { cache: 'no-store', signal: AbortSignal.timeout(8000) });
            if (!response.ok) throw new Error();
            const { alerts, enabled } = await response.json();
            render(alerts, enabled);
        } catch {
            state.hidden = false;
            state.textContent = 'Recent alerts are temporarily unavailable.';
        } finally {
            running = false;
        }
    }

    panel.querySelector('[data-clear-alerts]').addEventListener('click', clear);
    document.addEventListener('visibilitychange', refresh);
    setInterval(refresh, 5000);
    refresh();
})();
