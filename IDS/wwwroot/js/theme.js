// Loaded in <head> so every page uses the saved appearance before its first paint.
(function () {
    'use strict';

    const STORAGE_KEY = 'ids-theme';
    const systemTheme = window.matchMedia('(prefers-color-scheme: dark)');
    const normalize = value => ['light', 'dark', 'system'].includes(value) ? value : 'dark';
    let preference = 'dark';

    try { preference = normalize(localStorage.getItem(STORAGE_KEY)); } catch { /* Storage may be unavailable. */ }

    function apply(value) {
        preference = normalize(value);
        const dark = preference === 'dark' || (preference === 'system' && systemTheme.matches);
        const root = document.documentElement;
        root.classList.toggle('theme-dark', dark);
        root.dataset.theme = dark ? 'dark' : 'light';
        root.style.colorScheme = dark ? 'dark' : 'light';

        const toggle = document.getElementById('themeToggle');
        const label = `Switch to ${dark ? 'light' : 'dark'} mode`;
        if (toggle) {
            toggle.setAttribute('aria-label', label);
            toggle.setAttribute('aria-pressed', String(dark));
            toggle.title = label;
        }
        const icon = document.getElementById('themeIcon');
        if (icon) {
            icon.classList.toggle('fa-sun', dark);
            icon.classList.toggle('fa-moon', !dark);
        }
        document.querySelectorAll('[data-theme-choice]').forEach(input => {
            input.checked = input.value === preference;
        });
    }

    function select(value) {
        apply(value);
        let saved = true;
        try { localStorage.setItem(STORAGE_KEY, preference); } catch { saved = false; }
        document.querySelectorAll('[data-theme-status]').forEach(status => {
            status.textContent = saved ? 'Appearance saved for this browser.' : 'Appearance applied. Your browser could not save the preference.';
        });
    }

    const ThemeManager = {
        apply,
        select,
        toggle() { select(document.documentElement.classList.contains('theme-dark') ? 'light' : 'dark'); }
    };
    window.IDS = { ...window.IDS, ThemeManager };
    apply(preference);

    document.addEventListener('DOMContentLoaded', () => {
        apply(preference);
        document.getElementById('themeToggle')?.addEventListener('click', ThemeManager.toggle);
        document.querySelectorAll('[data-theme-choice]').forEach(input => {
            input.addEventListener('change', () => { if (input.checked) select(input.value); });
        });
    });
    systemTheme.addEventListener('change', () => { if (preference === 'system') apply(preference); });
    window.addEventListener('storage', event => {
        if (event.key === STORAGE_KEY || event.key === null) apply(event.newValue);
    });
})();
