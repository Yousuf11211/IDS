// A restored browser snapshot must never stand in for a server authorization check.
(() => {
    'use strict';
    const root = document.documentElement;
    if (root.dataset.authenticated !== 'true') return;

    window.addEventListener('pagehide', event => {
        if (event.persisted) root.style.visibility = 'hidden';
    });
    window.addEventListener('pageshow', event => {
        const navigation = performance.getEntriesByType('navigation')[0];
        if (event.persisted || navigation?.type === 'back_forward') {
            root.style.visibility = 'hidden';
            window.location.reload();
        }
    });
})();
