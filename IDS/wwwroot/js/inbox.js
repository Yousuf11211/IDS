(() => {
    const menu = document.querySelector('[data-inbox-url]');
    if (!menu) return;

    const count = menu.querySelector('[data-inbox-count]');
    const list = menu.querySelector('[data-inbox-recent]');
    const form = menu.querySelector('[data-inbox-form]');
    let busy = false;
    let previous;
    async function refresh() {
        if (busy || document.hidden) return;
        busy = true;
        try {
            const response = await fetch(menu.dataset.inboxUrl, {
                cache: 'no-store', signal: AbortSignal.timeout(8000)
            });
            if (!response.ok) throw new Error();
            const { unread, recent } = await response.json();
            count.hidden = unread === 0;
            count.textContent = unread > 99 ? '99+' : String(unread);
            count.setAttribute('aria-label', `${unread} unread notifications`);
            const snapshot = JSON.stringify(recent);
            if (previous === snapshot || list.contains(document.activeElement)) return;
            previous = snapshot;
            const items = recent.map(item => {
                const button = document.createElement('button');
                button.type = 'submit';
                button.name = 'id';
                button.value = item.id;
                button.className = 'dropdown-item text-wrap rounded';
                const title = document.createElement('strong');
                title.textContent = item.isUnread ? `${item.type} · Unread` : item.type;
                const message = document.createElement('div');
                message.className = 'inbox-preview text-break';
                message.textContent = item.message;
                const meta = document.createElement('small');
                const time = new Date(item.createdAt.endsWith('Z') ? item.createdAt : item.createdAt + 'Z');
                meta.textContent = `${item.senderName || 'IDS Security'} · ${time.toLocaleString()}`;
                button.append(title, message, meta);
                return button;
            });
            list.replaceChildren(...(items.length ? items : ['No notifications yet.']));
        } catch {
            if (!previous) list.textContent = 'Notifications unavailable. Open the inbox to retry.';
        } finally {
            busy = false;
        }
    }

    form.addEventListener('submit', event => {
        if (!event.submitter?.value) event.preventDefault();
    });
    menu.addEventListener('show.bs.dropdown', refresh);
    document.addEventListener('visibilitychange', refresh);
    setInterval(refresh, 15000);
    refresh();
})();
