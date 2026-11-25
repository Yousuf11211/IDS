# IDS Phase 2 implementation and operations

## Dashboard and Live Feed

The Dashboard shows processed, benign, and malicious flows and the malicious share for a selected UTC window. It compares equivalent prior windows when those windows contain observations. The trend chart can show total traffic or separate benign and malicious series. Historical ranges include 60 minutes, 24 hours, 7 days, 30 days, all time, and custom UTC dates.

Attack analytics uses the deployed model's stored `AttackType` values. The horizontal bar chart, doughnut, comparison trends, and linked table reconcile with the malicious flow count. The table shows first and last observations in the window. Blank labels, the `UnknownAttack` threshold fallback, and the legacy binary-only `Attack` label are grouped as Unclassified. They count toward malicious flows and are excluded from the number of distinct predicted categories.

The Dashboard shows observed malicious source addresses, destination ports, and recent classes. The classified flow tables supply source IP and destination port. Destination IP, source port, and protocol are unavailable in the classified flow schema. A source address is an observation associated with a flow and by itself establishes no attribution. Administrators and Support can open filtered incident queues and completed explanation records. SHAP totals count jobs across model stages. Reliable processing duration is unavailable from the stored job timestamps.

Live Feed uses rolling 5-minute, 15-minute, 30-minute, 60-minute, and 24-hour windows. It shows current traffic, flows per minute, pending SHAP jobs, bounded malicious activity, and a paginated detection table. The table supports search, sorting, classification, class, and explanation status filters. Inspection and incident links enforce existing permissions. Pause freezes browser updates and leaves Python workers running. The application reuses SignalR and its local Plotly bundle.

Dashboard, Live Feed, and the worker API share heartbeat evaluation. Both running workers report Operational, one reports Degraded, and neither reports Offline. Heartbeat age and the worker running flag determine health. The separate SHAP job generation setting controls new jobs, and an idle worker can remain online.

Administration follows the current section or open dropdown. The Inbox follows its page or open dropdown, and a badge tracks unread messages. The shared theme supplies surfaces, typography, chart colors, and focus indicators. The shell keeps the footer at the viewport bottom on short pages.

## Security alerts

The previous Administration page queried `SecurityAlerts` and grouped rows by severity and acknowledgement. It could acknowledge existing alert rows. The Python workers persist model results in `Attack_Table` or `Benign_Table`, and the live views had their own warnings based on those results. No component produced rows for the separate administrative alert table, so the alert statistics stayed at zero even when malicious flows existed.

| Record | Purpose |
| --- | --- |
| Detection | One classified network flow |
| Security alert | An actionable group of detections or an operational or account security event |
| Incident | A ticket for investigation, assignment, escalation, or resolution |
| Notification | A persisted message addressed to one application user |
| Activity or audit log | Historical application activity or a recorded security action |

The ASP.NET Core monitor starts at the newest detection ID visible on first activation. Each batch advances a durable cursor in the same transaction as the alert changes. Grouping uses class, observed source, destination port, and a fixed UTC interval. The default rule is five flows in ten minutes at Medium severity. Overrides accept one exact label per line in the form `class | threshold | severity`. The unique grouping key and cursor prevent count inflation across batches and restarts. Changes to the grouping interval affect subsequent batches.

The Alerts page offers explicit one-time processing of 1–30 days of pre-existing detections. Live processing and backfill use disjoint detection IDs and can update the same group. Alerts show severity and observed-date distributions, filters, acknowledgement, representative detection inspection, incident creation or association, and resolution. A linked incident must be resolved or closed before manual alert resolution. A resolved detection group can continue accumulating evidence during its observation interval.

Optional worker failure alerts track outages after a worker has previously reported a heartbeat. An observed outage produces one active alert and can auto-resolve when the worker returns and the linked incident is terminal, if present. Operations also controls account recovery events and Inbox notices for new high or critical alerts. Resolved-alert age affects recent views. All time retains access to archived alerts.

## Mailjet and account recovery

The existing MailKit Mailjet sender and credentials remain the transport. Administration → Settings → Email delivery manages enablement, port, sender, support and administrator contacts, and support copies for recovery review. The SMTP host remains controlled by deployment configuration. Blank replacement credential fields retain the current values. ASP.NET Core Data Protection encrypts credentials stored in the database and pending email payloads. Preserve the application's key ring, application discriminator, and access controls between restarts. Share the key ring across application instances in the same deployment.

Email settings and test submissions require fresh administrator verification. Tests target the acting administrator's verified address. The durable queue records attempts, retries, terminal failures, and SMTP acceptance. It uses a stable Message-ID, and a host crash between SMTP acceptance and recording acceptance in SQL can still cause a retry. Acceptance confirms provider submission. Recipient delivery and inbox placement require provider-side evidence.

The shared HTML and plain-text templates cover new-user onboarding, password reset requests and completions, profile password changes, authenticator recovery verification and review notices, approvals, rejections, successful replacement, authenticator activation and deactivation, and verified email changes. Email change confirmations go to the previous verified and newly verified addresses when applicable. Administrator-edited addresses require new verification. Mandatory account notifications do not rely on user preferences. Messages include the event time and support instructions and omit passwords, authenticator secrets, and recovery codes.

Self-service password reset starts with a generic confirmation. The trusted HTTPS link wraps an ASP.NET Core Identity token bound to the user and their security stamp. Users with a configured authenticator supply a current authenticator code or consume a one-time recovery code. Identity validates the password and revokes earlier tokens and sessions on success. Rate limits persist by account identifier and remote address. SMTP-disabled or unconfigured deployments cannot initiate email-dependent recovery.

Authenticator recovery proves control of the registered email and records the user's reason in an expiring request. Administrators receive Inbox notices, and configured support contacts receive review email. Only administrators have review permission in the current role model. An independent administrator verifies identity, records a decision reason, and reauthenticates with their password and current authenticator if enrolled. Approval leaves the existing authenticator intact. The user receives a decision by email and in the Inbox and can view their own recent request statuses in Profile Settings.

The approved link opens dedicated enrollment without creating an authenticated session. The user verifies their current password and proves control of the new authenticator. The new key, new recovery codes, revoked prior credentials and sessions, completed request, and account-security message are committed together.

### Operator-assisted recovery

Use the organization's independent identity-verification procedure and secure support channel. Record the verified case reference.

- For an employee or Support account that lost both credentials, obtain an independently approved, unexpired request. A deployment operator re-verifies the case and runs the command below. It revokes both credentials and all sessions, cancels the approved enrollment link, and returns the existing account to its invite-required state. An administrator reissues setup from Employees and roles, and the user completes password setup and normal authenticator enrollment.
- For the remaining administrator, use the existing deployment-only recovery procedure. Prefer provisioning a second independently verified, enrolled administrator and using the ordinary review path. When the emergency operator reset is required, verify identity under the organization's emergency procedure, record the reason, and confirm completion of mandatory authenticator enrollment. [Administrator security guide](AdminSecurity.md)

```powershell
dotnet run --project IDS/IDS.csproj --no-launch-profile -- --recover-account=employee@example.test --request=42 "--reason=Verified support case reference and operator review"
```

## Migration and startup

Migration `20261125194332_AddMonitoringAlertsAndAccountRecovery` adds the recovery requests, rate limits, and protected email outbox tables. It extends `SecurityAlerts` with observation, status, grouping, and incident association fields, and `InboxNotifications` with destination metadata. Unique and lookup indexes cover alert grouping, recovery expiry, email deduplication, and queue readiness. The classified flow and SHAP SQL contracts retain their schema.

From the solution root, inspect the connection target and apply the upgrade to the existing database.

```powershell
dotnet ef migrations script 20261125180505_AddOperationsMonitoringAndInbox 20261125194332_AddMonitoringAlertsAndAccountRecovery --idempotent --project IDS/IDS.csproj
dotnet run --project IDS/IDS.csproj --no-launch-profile -- --migrate-only
dotnet run --project IDS/IDS.csproj --launch-profile https
```

Set `PUBLIC_BASE_URL` or `Application:PublicBaseUrl` to the trusted public HTTPS origin. Loopback HTTP is permitted in Development. Retain the working Mailjet credentials or configure protected replacements in Email delivery. Reverse proxy request logs should omit account recovery query strings.

Run the workers from `pipeline` in separate terminals.

```powershell
.\.venv\Scripts\python.exe -m ids_pipeline run
.\.venv\Scripts\python.exe -m ids_pipeline explain
```

The application adds no package dependency. It uses the existing MailKit, Identity, Data Protection, QRCoder, SignalR, Plotly, and SQL Server integration.

### Verification commands

```powershell
dotnet test IDS.sln
node --test IDS.Tests/attack-inspection.test.cjs
cd pipeline
.\.venv\Scripts\python.exe -m unittest discover -s tests -p "test_*.py"
.\.venv\Scripts\python.exe tests/run_sql_verification.py --browser --screenshots
```

The opt-in runner creates a uniquely named disposable SQL database. It exercises real prediction and SHAP output, upgrades over the historical flow schema, API queries, heartbeat transitions, SignalR, and Edge browser interactions. The repository has a pre-existing duplicate Roles migration for blank-database bootstrap. The opt-in fixture validates an upgrade from the historical schema.

## Repository file inventory

### Modified

- `IDS.Tests/AccountSetupTests.cs`
- `IDS.Tests/SecurityAdministrationTests.cs`
- `IDS.Tests/SqlServerExplanationIntegrationTests.cs`
- `IDS/Api/MonitoringController.cs`
- `IDS/Areas/Identity/Pages/Account/ConfirmEmailChange.cshtml`
- `IDS/Areas/Identity/Pages/Account/ConfirmEmailChange.cshtml.cs`
- `IDS/Areas/Identity/Pages/Account/ForgotPassword.cshtml`
- `IDS/Areas/Identity/Pages/Account/ForgotPassword.cshtml.cs`
- `IDS/Areas/Identity/Pages/Account/ForgotPasswordConfirmation.cshtml`
- `IDS/Areas/Identity/Pages/Account/HelpSupport.cshtml`
- `IDS/Areas/Identity/Pages/Account/HelpSupport.cshtml.cs`
- `IDS/Areas/Identity/Pages/Account/Login.cshtml`
- `IDS/Areas/Identity/Pages/Account/Manage/ChangePassword.cshtml`
- `IDS/Areas/Identity/Pages/Account/Manage/ChangePassword.cshtml.cs`
- `IDS/Areas/Identity/Pages/Account/Manage/Disable2fa.cshtml.cs`
- `IDS/Areas/Identity/Pages/Account/Manage/Email.cshtml`
- `IDS/Areas/Identity/Pages/Account/Manage/Email.cshtml.cs`
- `IDS/Areas/Identity/Pages/Account/Manage/EnableAuthenticator.cshtml.cs`
- `IDS/Areas/Identity/Pages/Account/Manage/ResetAuthenticator.cshtml.cs`
- `IDS/Areas/Identity/Pages/Account/Manage/TwoFactorAuthentication.cshtml`
- `IDS/Areas/Identity/Pages/Account/Manage/TwoFactorAuthentication.cshtml.cs`
- `IDS/Areas/Identity/Pages/Account/ResetPassword.cshtml`
- `IDS/Areas/Identity/Pages/Account/ResetPassword.cshtml.cs`
- `IDS/Core/Services/LiveDetectionService.cs`
- `IDS/Core/Services/OperationsSettings.cs`
- `IDS/Core/Services/WorkerHealthService.cs`
- `IDS/Data/ApplicationDbContext.cs`
- `IDS/Data/Migrations/ApplicationDbContextModelSnapshot.cs`
- `IDS/Data/Models/DetectionModels.cs`
- `IDS/Data/Models/IncidentWorkflow.cs`
- `IDS/Data/Models/NidsModels.cs`
- `IDS/Data/Services/MailjetEmailSender.cs`
- `IDS/Docs/ModelPipeline.md`
- `IDS/IDS.csproj`
- `IDS/Pages/Admin/AlertDetails.cshtml`
- `IDS/Pages/Admin/AlertDetails.cshtml.cs`
- `IDS/Pages/Admin/Alerts.cshtml`
- `IDS/Pages/Admin/Alerts.cshtml.cs`
- `IDS/Pages/Admin/CreateUser.cshtml.cs`
- `IDS/Pages/Admin/EditUser.cshtml.cs`
- `IDS/Pages/Admin/Operations.cshtml`
- `IDS/Pages/Admin/Operations.cshtml.cs`
- `IDS/Pages/Dashboard.cshtml`
- `IDS/Pages/Dashboard.cshtml.cs`
- `IDS/Pages/DetectionHistory.cshtml`
- `IDS/Pages/DetectionHistory.cshtml.cs`
- `IDS/Pages/Help.cshtml`
- `IDS/Pages/Help.cshtml.cs`
- `IDS/Pages/Inbox.cshtml`
- `IDS/Pages/Inbox.cshtml.cs`
- `IDS/Pages/LiveDashboard.cshtml`
- `IDS/Pages/LiveDashboard.cshtml.cs`
- `IDS/Pages/Shared/_InboxNotifications.cshtml`
- `IDS/Pages/Shared/_Layout.cshtml`
- `IDS/Pages/Shared/_TrafficAnalytics.cshtml`
- `IDS/Pages/Shared/_WorkerHealth.cshtml`
- `IDS/Pages/Support/Tickets.cshtml.cs`
- `IDS/Program.cs`
- `IDS/Security/AccountLinkBuilder.cs`
- `IDS/Security/AdminVerification.cs`
- `IDS/Security/PrivateResponseCacheMiddleware.cs`
- `IDS/Security/SecurityOperatorCommands.cs`
- `IDS/wwwroot/css/site.css`
- `IDS/wwwroot/js/site.js`
- `IDS/wwwroot/js/traffic-analytics.js`
- `IDS/wwwroot/js/worker-health.js`
- `pipeline/README.md`
- `pipeline/tests/verify_inspection_browser.py`

### Added

- `IDS.Tests/SecurityAlertTests.cs`
- `IDS/Api/MonitoringController.Feed.cs`
- `IDS/Areas/Identity/Pages/Account/AuthenticatorRecovery.cshtml`
- `IDS/Areas/Identity/Pages/Account/AuthenticatorRecovery.cshtml.cs`
- `IDS/Areas/Identity/Pages/Account/RecoveryEnrollment.cshtml`
- `IDS/Areas/Identity/Pages/Account/RecoveryEnrollment.cshtml.cs`
- `IDS/Core/Services/DetectionLabels.cs`
- `IDS/Core/Services/MonitoringWindow.cs`
- `IDS/Core/Services/SecurityAlertMonitor.cs`
- `IDS/Data/DatabaseLock.cs`
- `IDS/Data/Migrations/20261125194332_AddMonitoringAlertsAndAccountRecovery.Designer.cs`
- `IDS/Data/Migrations/20261125194332_AddMonitoringAlertsAndAccountRecovery.cs`
- `IDS/Data/Models/AccountSecurity.cs`
- `IDS/Data/Services/AccountEmailQueue.cs`
- `IDS/Data/Services/EmailConfiguration.cs`
- `IDS/Docs/Phase2.md`
- `IDS/Pages/Admin/EmailDelivery.cshtml`
- `IDS/Pages/Admin/EmailDelivery.cshtml.cs`
- `IDS/Pages/Admin/RecoveryRequests.cshtml`
- `IDS/Pages/Admin/RecoveryRequests.cshtml.cs`
- `IDS/Security/AccountRecovery.cs`
- `IDS/Templates/Email/AccountSecurity.html`
- `IDS/Templates/Email/AccountSecurity.txt`
- `IDS/wwwroot/js/live-feed.js`

### Removed

- `IDS/Pages/Dashboard.cshtml.css`
- `IDS/Pages/Shared/_Layout.cshtml.css`
