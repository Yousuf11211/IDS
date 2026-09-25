# Administrator security controls

Open **Admin → Security controls** (`/Admin/Security`). Admin pages require the current password and an authenticator code every five minutes, except for the explicit Development administrator testing bypass described below. This verification is tied to the browser session and account security stamp, so changing credentials or revoking sessions invalidates it.

Administrators have an **Approvals** bell in the site header. While the page is visible, it checks for requests from other administrators every ten seconds and displays the requester's email and requested action. Pending requests also appear after signing in again; notifications do not depend on both people being online at the same time. Choose **Review request** to see the target and reason, then **Approve** or **Reject**. Expired, reviewed, and self-requested changes are excluded from the notification list. Approval still requires recent administrator verification. The notification endpoint is read-only and restricted to eligible administrators.

## Available actions

| Action | Required authorization |
| --- | --- |
| Disable messaging or pause new employee invitations | One recently verified administrator, with a reason |
| Enable messaging or resume invitations | Request and approval by two different, active administrators |
| Bypass employee authenticator prompts for development testing | Two administrators; available only in Development mode |
| Restore employee authenticator checks | One recently verified administrator |
| Reset an employee's lost authenticator | Two administrators; verify the employee's identity outside the app first |
| Promote an enrolled employee to administrator | Two administrators |
| Revoke an account's sessions | One recently verified administrator, with a reason |
| Recover or suspend an existing administrator | Deployment operator on the server |

Requests expire after 24 hours. A change to the requester's credentials, or the target account's credentials/email, makes an earlier request unusable. Emergency disabling cancels outstanding requests to enable that feature. Changes and their audit records are committed together.

Administrator MFA cannot be switched off through the web interface. Employee authenticator recovery invalidates the old key, recovery codes and login sessions, then requires fresh enrollment before application access unless the development testing bypass is active. Administrators cannot reset another administrator through employee management. Self-service authenticator replacement and recovery-code generation require both the password and current authenticator code, even during testing.

## Two local test administrators and separate admin bypass

The ignored `IDS/.env` file contains `ADMIN_EMAIL` / `ADMIN_PASSWORD` for the first account and `ADMIN2_EMAIL` / `ADMIN2_PASSWORD` for the second. They are temporary readable test credentials; the database stores Identity password hashes. Normal startup does not synchronize these passwords. To explicitly apply both configured passwords (or create missing test administrators), run from `IDS/` in a Development environment:

```sh
dotnet run --launch-profile http -- --setup-test-admins
```

The command exits without starting the server. It refuses Production and Staging, duplicate email addresses, existing non-admin accounts, and restricted accounts. It does not reset authenticator keys or clear lockouts. Password changes invalidate existing sessions and are audited. Repeating it with matching passwords does not change security stamps.

Set `ADMIN_MFA_TEST_BYPASS=true` in `IDS/.env` and restart the Development server to skip administrator authenticator prompts and enrollment during testing. This is separate from the employee bypass. Admin sign-in still requires the password; admin pages still require password verification every five minutes; sensitive changes still require a different administrator's approval. The account header and security page show that the bypass is active. Existing authenticator keys, recovery codes, and enrollment state are preserved.

When testing ends, set `ADMIN_MFA_TEST_BYPASS=false` (or remove it) and restart. Test login sessions and password-only admin verification are then rejected; enrolled administrators must use their authenticator and unenrolled administrators must enroll. Production and Staging ignore the flag even when it is true. Remove the temporary credentials and rotate any passwords retained beyond local testing. Two accounts owned by one person are suitable for exercising the UI, not independent production approval.

## Skip employee codes during local testing

In **Admin → Security**, use **Request testing bypass**, enter a reason, and have the second administrator approve it. This bypasses authenticator prompts for all non-admin accounts, including enrolled employees and support staff. It also lets unenrolled employees test the app without being redirected into authenticator setup. Passwords, account confirmation, lockout and suspension still apply. Existing authenticator keys and recovery codes are not changed.

The employee bypass is off by default and only takes effect when the application environment is `Development`. The supplied local launch profiles already use that environment. Production and Staging ignore a stored bypass setting and refuse requests to enable it; never deploy a company service in Development mode. The employee setting does not affect administrators; their separate testing flag is described above.

Use **Restore employee 2FA checks** when testing ends. Sessions created through the bypass are marked in their protected login cookies. Restoring checks, or running outside Development, ends those sessions on the next HTTP request or live-connection check. Enrolled employees must enter a code on their next login; unenrolled employees must complete enrollment. A visible account-menu badge identifies the active test bypass. This setting uses the existing `SystemSettings` table and does not require a new migration.

Security-stamp validation runs on every authenticated HTTP request. SignalR checks each connection and invocation; a background check runs every five seconds for idle connections. Actual disconnect latency includes database and scheduling time. Revocation ends existing sessions but does not block a new login with valid credentials; use suspension for that.

## First installation and deployment

Apply database migrations before starting the deployed application:

```sh
dotnet run --project IDS/IDS.csproj --no-launch-profile -- --migrate-only
```

Normal startup only creates the first administrator when none exists and the configured bootstrap account is absent. It no longer restores removed roles or clears lockouts. Remove bootstrap credentials from the running service's environment once provisioning is complete.

If there is only one administrator, first invite a second trusted person as an employee. That person must finish email/password setup and enroll an authenticator. A deployment operator can then provision that existing account from the server:

```sh
dotnet run --project IDS/IDS.csproj --no-launch-profile -- --provision-admin=reviewer@example.com "--reason=Initial independent security reviewer"
```

Subsequent promotions can use the two-person web approval workflow. Do not use two accounts owned by the same person as independent reviewers.

An operator can recover an administrator's authenticator or suspend a compromised administrator:

```sh
dotnet run --project IDS/IDS.csproj --no-launch-profile -- --recover-admin=admin@example.com "--reason=Verified recovery incident INC-123"
dotnet run --project IDS/IDS.csproj --no-launch-profile -- --suspend-admin=admin@example.com "--reason=Contain compromised account INC-124"
```

These commands use the application's configured database, record an operator audit event and exit without starting the web server. Recovery does not change the password or clear lockout. Protect deployment-host access separately from web administrator access. Suspended administrators cannot be restored through ordinary employee management; restoration needs a separately reviewed operator procedure.

## Messaging rollout

Messaging is disabled by default. A persisted `Security.MessagingEnabled` setting takes precedence over the deployment's `Chat:Enabled` fallback. Administrators must acknowledge the preview limitations when requesting activation. The policy applies to the chat page, navigation, new connections and existing hub calls. See [ChatSecurityReview.md](ChatSecurityReview.md) before enabling it; the approval workflow does not make the encryption independently reviewed.

## What a compromised administrator can still do

These controls limit a single account's ability to weaken authentication, promote accomplices or reset another administrator. They cannot make a stolen administrator account harmless: it can still access permitted company data, manage ordinary employee accounts, disable optional features and revoke sessions. Those actions can disrupt operations. Two compromised reviewers can approve sensitive changes together.

Server or database compromise bypasses application approval checks. Audit records cannot be edited through this app, but a database operator can modify them. For company deployment, use separately controlled, off-host audit storage; restricted database/service identities; protected deployment secrets and keys; monitored recovery procedures; and an independent security review. Consider phishing-resistant passkeys or hardware keys: current TOTP codes can be phished.

Identity credential tables and security settings are excluded from the generic database viewer. Personal-data exports exclude authenticator secrets. Direct detection-table writes remain controlled by the deployment setting `DatabaseViewer:AllowWrites`, which should remain false in production.

The design follows OWASP guidance on [MFA recovery and factor changes](https://cheatsheetseries.owasp.org/cheatsheets/Multifactor_Authentication_Cheat_Sheet.html) and [least-privilege authorization](https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html).
