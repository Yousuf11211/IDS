(() => {
    const panel = document.getElementById('approvalNotifications');
    if (!panel) return;
    const count = document.getElementById('approvalCount');
    const status = document.getElementById('approvalStatus');
    const list = document.getElementById('approvalList');
    const notice = document.getElementById('approvalNotice');
    let previousIds = new Set();
    let busy = false;
    let stopped = false;
    const reviewUrl = id => `${panel.dataset.reviewUrl}?requestId=${encodeURIComponent(id)}#request-${encodeURIComponent(id)}`;

    document.getElementById('dismissApprovalNotice').addEventListener('click', () => { notice.hidden = true; });

    async function refresh() {
        if (busy || stopped || document.hidden) return;
        busy = true;
        try {
            const response = await fetch(panel.dataset.endpoint, {
                credentials: 'same-origin', cache: 'no-store', headers: { Accept: 'application/json' }
            });
            if (response.redirected || response.status === 401 || response.status === 403) {
                stopped = true;
                list.replaceChildren();
                count.hidden = true;
                notice.hidden = true;
                status.textContent = 'Sign in with an eligible administrator account to review requests.';
                return;
            }
            if (!response.ok) throw new Error('Request unavailable');
            const data = await response.json();
            count.textContent = String(data.count);
            count.hidden = data.count === 0;
            status.textContent = data.count === 0 ? 'No requests need your approval.' :
                `${data.count} request${data.count === 1 ? '' : 's'} awaiting your approval.`;
            list.replaceChildren();
            for (const request of data.requests) {
                const item = document.createElement('li');
                const link = document.createElement('a');
                link.href = reviewUrl(request.id);
                link.textContent = `${request.requestedByEmail} requested: ${request.action}${request.targetEmail ? ` for ${request.targetEmail}` : ''}`;
                item.append(link);
                list.append(item);
            }
            const fresh = data.requests.find(request => !previousIds.has(request.id));
            if (fresh) {
                document.getElementById('approvalNoticeText').textContent = `${fresh.requestedByEmail} requested your approval: ${fresh.action}.`;
                document.getElementById('approvalNoticeLink').href = reviewUrl(fresh.id);
                notice.dataset.requestId = String(fresh.id);
                notice.hidden = false;
            } else if (!data.requests.some(request => String(request.id) === notice.dataset.requestId)) {
                notice.hidden = true;
            }
            previousIds = new Set(data.requests.map(request => request.id));
        } catch {
            status.textContent = 'Could not refresh notifications. Retrying shortly; you can also open security controls.';
        } finally {
            busy = false;
        }
    }

    refresh();
    setInterval(refresh, 10000);
    document.addEventListener('visibilitychange', refresh);
    panel.addEventListener('toggle', () => { if (panel.open) refresh(); });
})();
