"""Run real SQL/Python/HTTP verification in a NEW disposable database.

Requires an installed .NET SDK on the same platform as Python. --browser also
requires playwright and an installed Microsoft Edge. Never changes IDS/.env.
"""
import argparse
import os
from pathlib import Path
import shutil
import subprocess
import sys
import uuid

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from ids_pipeline.config import Config, ROOT
from ids_pipeline.connection import webapp_connection


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--browser', action='store_true')
    parser.add_argument('--screenshots', action='store_true', help='Optional UI review captures in ignored TestResults/')
    args = parser.parse_args()
    environment = dict(os.environ)
    database = 'IDS_Shap_Test_' + uuid.uuid4().hex[:12]
    environment['IDS_SHAP_SQL_TEST_CONNECTION'] = webapp_connection(Config()) + ';Database=' + database
    environment['IDS_SHAP_PYTHON'] = sys.executable
    environment['IDS_SHAP_BROWSER'] = '1' if args.browser else '0'
    if args.screenshots:
        environment['IDS_SHAP_SCREENSHOTS'] = str(ROOT.parent / 'IDS.Tests/TestResults')
    print('Dedicated verification database: ' + database, flush=True)
    result = subprocess.run([shutil.which('dotnet') or 'dotnet', 'test', str(ROOT.parent / 'IDS.Tests/IDS.Tests.csproj'),
        '--no-restore', '--filter', 'FullyQualifiedName~SqlServerExplanationIntegrationTests',
        '--logger', 'console;verbosity=normal'], cwd=ROOT.parent, env=environment)
    print('The dedicated database is retained for review: ' + database, flush=True)
    return result.returncode


if __name__ == '__main__':
    sys.exit(main())
