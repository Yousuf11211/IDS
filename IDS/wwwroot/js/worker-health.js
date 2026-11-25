(() => {
    const panel = document.querySelector('[data-worker-health-url]');
    if (!panel) return;

    function show(name, status, heartbeat) {
        const entry = panel.querySelector(`[data-worker="${name}"]`);
        const date = heartbeat && new Date(heartbeat);
        const value = date ? `${status} · ${date.toLocaleString()}` : status;
        if (entry.textContent !== value) entry.textContent = value;
        entry.dataset.status = status;
        if (date) entry.title = `Last heartbeat: ${date.toLocaleString()}`;
        else entry.removeAttribute('title');
    }

    async function refresh() {
        if (!document.hidden) {
            try {
                const response = await fetch(panel.dataset.workerHealthUrl, {
                    cache: 'no-store', signal: AbortSignal.timeout(8000)
                });
                if (!response.ok) throw new Error('Worker health unavailable');
                const health = await response.json();
                show('overall', health.overall);
                panel.querySelector('[data-shap-disabled]').hidden = health.shapJobsEnabled !== false;
                for (const name of ['detection', 'explanation']) {
                    show(name, health[name].status, health[name].lastHeartbeatUtc);
                }
            } catch {
                for (const name of ['overall', 'detection', 'explanation']) show(name, 'Unavailable');
            }
        }
        setTimeout(refresh, 5000);
    }

    refresh();
})();
