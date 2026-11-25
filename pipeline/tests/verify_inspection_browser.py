"""Browser assertions against the isolated .NET integration host only."""
import json
import os
from pathlib import Path
import sys

from playwright.sync_api import sync_playwright


def main():
    base_url, attack_id = sys.argv[1:]
    errors = []
    with sync_playwright() as p:
        browser = p.chromium.launch(channel='msedge', headless=True)
        context = browser.new_context(viewport={'width': 1440, 'height': 1100},
            reduced_motion='reduce', extra_http_headers={'X-Inspection-Test-Role': 'Support'})
        page = context.new_page()
        page.on('pageerror', lambda error: errors.append(str(error)))
        # Charts must work without a remote font/icon/CDN connection.
        page.route('https://**/*', lambda route: route.abort())
        page.goto(f'{base_url}/InspectAttack/{attack_id}', wait_until='networkidle')
        page.wait_for_function("document.getElementById('shapBar').data?.[0]?.x.length === 10")
        assert page.locator('#referenceCaption').inner_text() == 'Reference unavailable'
        assert page.locator('#featureEvidence tr').count() == 10
        initial = page.evaluate("JSON.parse(document.getElementById('inspectionData').textContent)")
        gate = next(s for s in initial['stages'] if s['stage'] == 'gatekeeper')['data']
        assert f"{gate['probability'] * 100:.2f}%" in page.locator('#summaryBinary').inner_text()
        # Hover and keyboard provide actual values and correct raw units.
        # Plotly intentionally places a transparent interaction layer over SVG bars.
        page.locator('#shapBar .barlayer .point').first.hover(force=True)
        page.wait_for_selector('#shapBar .hoverlayer .hovertext')
        assert 'Observed:' in page.locator('#shapBar .hoverlayer').text_content()
        button = page.locator('#featureEvidence button').first
        button.focus()
        assert 'log-odds' in page.locator('#featureDetail').inner_text()
        assert 'Reference unavailable' in page.locator('#featureDetail').inner_text()
        page.select_option('#featureCount', '5')
        page.wait_for_function("document.getElementById('shapBar').data[0].x.length === 5")
        page.select_option('#stageSelect', 'multiclass')
        page.wait_for_function("document.getElementById('contributionCaption').textContent.includes('raw class margin')")
        page.wait_for_function("document.getElementById('shapWaterfall').data[0].x.length >= 7")
        multi = next(s for s in initial['stages'] if s['stage'] == 'multiclass')['data']
        assert multi['explainedClass'] in page.locator('#contributionCaption').inner_text()
        assert 'raw class margin' in page.locator('#featureDetail').inner_text()
        assert multi['classIndex'] != 0
        if os.environ.get('IDS_SHAP_SCREENSHOTS'):
            target = Path(os.environ['IDS_SHAP_SCREENSHOTS'])
            target.mkdir(parents=True, exist_ok=True)
            page.screenshot(path=str(target / 'inspection-desktop.png'), full_page=True)
        page.select_option('#featureCount', 'all')
        page.wait_for_function("document.getElementById('shapBar').data[0].x.length === 120")
        page.select_option('#featureSort', 'name')
        page.wait_for_function("document.querySelector('#featureEvidence button').textContent === 'ack_flag_counts'")
        missing = page.locator('#featureEvidence tr').filter(has=page.get_by_role('button', name='delta_start', exact=True))
        assert missing.locator('td').nth(1).inner_text() == 'Unavailable'
        assert page.locator(f'a[href="/EscalateAttack/{attack_id}"]').count() > 0
        page.set_viewport_size({'width': 390, 'height': 844})
        page.select_option('#featureCount', '5')
        page.wait_for_function("document.getElementById('shapBar').data[0].x.length === 5")
        assert page.evaluate('document.documentElement.scrollWidth <= window.innerWidth + 2'), 'Mobile layout overflows'
        if os.environ.get('IDS_SHAP_SCREENSHOTS'):
            page.screenshot(path=str(Path(os.environ['IDS_SHAP_SCREENSHOTS']) / 'inspection-mobile.png'), full_page=True)
        # API errors retain evidence and show an actionable state.
        page.route('**/api/detection/*/inspection', lambda route: route.fulfill(status=503,
            content_type='application/json', body='{}'))
        page.locator('#refreshInspection').click()
        page.wait_for_function("document.getElementById('inspectionNotice').classList.contains('error')")
        assert page.locator('#featureEvidence tr').count() == 5
        # Missing/processing/failed stages are rendered from server numerical data.
        for state in ('pending', 'failed'):
            changed = json.loads(json.dumps(initial))
            for stage in changed['stages']:
                stage['status'] = state
            page.unroute('**/api/detection/*/inspection')
            page.route('**/api/detection/*/inspection', lambda route, request, payload=changed:
                route.fulfill(content_type='application/json', body=json.dumps(payload)))
            page.locator('#refreshInspection').click()
            expected = 'Explanation processing' if state == 'pending' else 'Explanation unavailable'
            page.wait_for_function('(text) => document.getElementById("shapEmpty").textContent.includes(text)', arg=expected)
            assert not page.locator('#shapContent').is_visible()
        page.unroute('**/api/detection/*/inspection')
        page.route('**/lib/plotly/**', lambda route: route.abort())
        page.reload(wait_until='networkidle')
        page.wait_for_function("document.getElementById('inspectionNotice').textContent.includes('chart library could not be loaded')")
        assert page.locator('#featureEvidence tr').count() == 10
        page.unroute('**/lib/plotly/**')
        # Drive the dashboard's real event handlers offline. The .NET integration test
        # separately delivers a real NewDetection message over an authenticated WebSocket.
        page.set_extra_http_headers({'X-Inspection-Test-Role': 'Admin'})
        page.route('**/microsoft-signalr/**', lambda route: route.fulfill(content_type='text/javascript', body="""
            window.dashboardEvents = {};
            window.signalR = { LogLevel: { Information: 2 }, HubConnectionBuilder: class {
                withUrl() { return this; } withAutomaticReconnect() { return this; }
                configureLogging() { return this; } build() { return this; }
                on(name, handler) { dashboardEvents[name] = handler; }
                onreconnecting() {} onreconnected() {} onclose() {} async start() {} async invoke() {}
            }};
        """))
        page.goto(f'{base_url}/LiveDashboard', wait_until='networkidle')
        page.locator('select[data-range]').select_option('24h')
        for label in ('PortScan', 'DoS_SYN_Flood'):
            page.locator('select[name=category]').select_option(label)
            page.get_by_role('button', name='Apply filters').click()
            page.wait_for_function(
                'label => [...document.querySelectorAll("[data-feed-rows] [data-attack-label]")].length > 0 && '
                '[...document.querySelectorAll("[data-feed-rows] [data-attack-label]")].every(n => n.dataset.attackLabel === label)',
                arg=label)
        page.wait_for_function("document.querySelector('[data-chart=distribution]').data?.[0]?.type === 'pie'")
        page.wait_for_selector('[data-feed-recent] a')
        assert page.locator('[data-feed-recent] tr').count() <= 12
        assert page.evaluate('document.documentElement.scrollWidth <= window.innerWidth + 2'), 'Mobile feed overflows'
        page.wait_for_function("document.querySelector('[data-worker=overall]').textContent === 'Offline'")
        if os.environ.get('IDS_SHAP_SCREENSHOTS'):
            page.screenshot(path=str(target / 'phase2-feed-mobile.png'), full_page=True)
        page.locator('[data-pause]').click()
        page.get_by_text('Live updates paused.', exact=True).first.wait_for()
        page.evaluate("() => dashboardEvents.NewDetection({ isAttack: true, attackType: 'PortScan' })")
        assert page.locator('[data-pause]').get_attribute('aria-pressed') == 'true'
        page.locator('[data-pause]').click()
        page.wait_for_function("document.querySelector('[data-chart-state]').textContent.includes('UTC')")
        page.goto(f'{base_url}/Dashboard', wait_until='networkidle')
        page.wait_for_function("document.querySelector('[data-chart=ratio]').data?.[0]?.type === 'pie'")
        page.wait_for_function("document.querySelector('[data-chart=types]').data?.[0]?.type === 'bar'")
        assert page.evaluate("document.querySelector('footer').getBoundingClientRect().top >= window.innerHeight")
        page.locator('.navbar-toggler').click()
        admin_nav = page.get_by_role('button', name='Administration')
        assert not admin_nav.get_attribute('aria-current')
        admin_nav.click()
        assert admin_nav.get_attribute('aria-expanded') == 'true'
        page.keyboard.press('Escape')
        assert admin_nav.get_attribute('aria-expanded') == 'false'
        page.set_viewport_size({'width': 1440, 'height': 1000})
        page.evaluate("window.IDS.ThemeManager.select('light')")
        page.wait_for_function("document.querySelector('[data-chart=traffic]').layout.font.color === '#0f172a'")
        if os.environ.get('IDS_SHAP_SCREENSHOTS'):
            page.screenshot(path=str(target / 'phase2-dashboard-light.png'), full_page=True)
        page.evaluate("window.IDS.ThemeManager.select('dark')")
        page.wait_for_function("document.querySelector('[data-chart=traffic]').layout.font.color === '#f1f5f9'")
        if os.environ.get('IDS_SHAP_SCREENSHOTS'):
            page.screenshot(path=str(target / 'phase2-dashboard-dark.png'), full_page=True)
        page.route('**/api/monitoring/analytics?range=1h&live=false', lambda route: route.fulfill(
            content_type='application/json', body=json.dumps({
                'snapshot': {'from': '2026-01-01T00:00:00Z', 'to': '2026-01-01T01:00:00Z',
                             'bucketMinutes': 1, 'totals': {'benign': 0, 'malicious': 0},
                             'traffic': [], 'types': [], 'incidents': [], 'incidentMetrics': [],
                             'explanations': [], 'sources': [], 'ports': []},
                'range': '1h', 'autoRefresh': False, 'refreshSeconds': 30
            })))
        page.locator('[data-range]').select_option('1h')
        page.locator('[data-chart=traffic]').get_by_text('No records in this window.').wait_for()
        for path, heading in [('/Admin/Alerts', 'Security alerts'),
                              ('/Admin/Operations', 'Administration settings'),
                              ('/Admin/EmailDelivery', 'Email delivery'),
                              ('/Admin/RecoveryRequests', 'Authenticator reset requests'),
                              ('/Admin/Logs', 'System Logs')]:
            response = page.goto(base_url + path, wait_until='networkidle')
            assert response.ok, (path, response.status)
            page.get_by_role('heading', name=heading).first.wait_for()
            assert page.get_by_role('button', name='Administration').get_attribute('aria-current') == 'location'
            for theme in ('light', 'dark'):
                page.evaluate('theme => window.IDS.ThemeManager.select(theme)', theme)
                assert page.evaluate('document.documentElement.scrollWidth <= window.innerWidth + 2'), path
                if path == '/Admin/Logs':
                    expected = 'rgb(255, 255, 255)' if theme == 'light' else 'rgb(30, 41, 59)'
                    assert page.locator('.form-control-ids').first.evaluate(
                        "element => getComputedStyle(element).backgroundColor") == expected
                    assert page.locator('.metric-card').first.evaluate(
                        "element => getComputedStyle(element).backgroundColor") == expected
                if os.environ.get('IDS_SHAP_SCREENSHOTS') and path in ('/Admin/Alerts', '/Admin/Logs', '/Admin/RecoveryRequests'):
                    page.screenshot(path=str(target / f'phase2-{path.rsplit("/", 1)[-1].lower()}-{theme}.png'), full_page=True)
        page.goto(f'{base_url}/Inbox', wait_until='networkidle')
        assert abs(page.evaluate("document.querySelector('footer').getBoundingClientRect().bottom - window.innerHeight")) <= 2
        # A fixture user signs in through real Identity cookies and a one-time recovery code.
        inbox_context = browser.new_context(viewport={'width': 1024, 'height': 768})
        inbox_context.route('https://**/*', lambda route: route.abort())
        inbox = inbox_context.new_page()
        inbox.on('pageerror', lambda error: errors.append(str(error)))
        for path in ('/Identity/Account/ForgotPassword', '/Identity/Account/AuthenticatorRecovery',
                     '/Identity/Account/HelpSupport', '/Identity/Account/RecoveryEnrollment'):
            response = inbox.goto(base_url + path)
            assert response.ok, (path, response.status)
            for theme in ('light', 'dark'):
                inbox.evaluate('theme => window.IDS.ThemeManager.select(theme)', theme)
                assert inbox.evaluate('document.documentElement.scrollWidth <= window.innerWidth + 2'), path
        inbox.goto(base_url + '/Identity/Account/Login?returnUrl=%2FInbox')
        inbox.locator('#Input_Email').fill(os.environ['IDS_TEST_INBOX_LOGIN'])
        inbox.locator('#Input_Password').fill(os.environ['IDS_TEST_INBOX_PASSWORD'])
        inbox.locator('button[type=submit]').first.click()
        inbox.get_by_role('link', name='Use Recovery Code').click()
        inbox.locator('#Input_RecoveryCode').fill(os.environ['IDS_TEST_INBOX_RECOVERY'])
        inbox.locator('button[type=submit]').first.click()
        inbox.get_by_role('heading', name='Notification inbox').wait_for()
        inbox.locator('[data-inbox-count]:text-is("1")').wait_for()
        assert inbox.get_by_role('button', name='Inbox').get_attribute('aria-current') == 'page'
        inbox.goto(base_url + '/Dashboard')
        inbox.locator('[data-inbox-count]:text-is("1")').wait_for()
        assert not inbox.get_by_role('button', name='Inbox').get_attribute('aria-current')
        inbox.goto(base_url + '/Inbox')
        inbox.get_by_role('heading', name='Incident escalated Unread').wait_for()
        inbox.get_by_role('button', name='Open incident').click()
        inbox.get_by_text('Review fixture security incident', exact=True).wait_for()
        assert '/ViewTicket' in inbox.url
        inbox.goto(base_url + '/Inbox')
        assert inbox.locator('.notification-item').get_attribute('data-unread') == 'false'
        assert not errors, errors
        browser.close()
    print(json.dumps(dict(browser='Microsoft Edge / Chromium', verified=['real stored numerical charts',
        'binary and nonzero multiclass selection', 'hover', 'keyboard feature detail', 'top 5/10/all',
        'sorting', 'waterfall', 'reference unavailable', 'mobile 390px', 'API error', 'processing', 'failed',
        'existing escalation link', 'missing chart library with numerical fallback',
        'local charts with external requests blocked', 'traffic charts and empty state', 'bounded linked alerts',
        'worker status', 'short and long page footers', 'MFA login and persistent inbox navigation'], javascriptErrors=errors)))


if __name__ == '__main__':
    main()
