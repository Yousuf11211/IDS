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
        assert not errors, errors
        browser.close()
    print(json.dumps(dict(browser='Microsoft Edge / Chromium', verified=['real stored numerical charts',
        'binary and nonzero multiclass selection', 'hover', 'keyboard feature detail', 'top 5/10/all',
        'sorting', 'waterfall', 'reference unavailable', 'mobile 390px', 'API error', 'processing', 'failed',
        'existing escalation link', 'missing chart library with numerical fallback',
        'local charts with external requests blocked'], javascriptErrors=errors)))


if __name__ == '__main__':
    main()
