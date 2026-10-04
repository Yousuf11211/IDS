# Employee account setup

Admin invitations and password resets send a one-time Identity link. The link is valid for 24 hours and lets the employee choose a password; no password is sent by email. An invited account cannot sign in until the link is used. The first login then requires authenticator enrollment, and later logins require an authenticator code.

Configure `EMAIL_STATUS=true`, `MAILJET_API_KEY`, and `MAILJET_SECRET_KEY` before creating employees. The application reads its `.env` from the content root (the `IDS` project directory during local development). Restart the application after changing these settings.

Account links also require a fixed origin, configured through `PUBLIC_BASE_URL` or `Application:PublicBaseUrl`. Development configuration defaults to `https://localhost:7157`, matching the local HTTPS launch profile. These local links must be opened on the computer running IDS. To send usable links to people on other devices, set `PUBLIC_BASE_URL` to the reachable HTTPS origin of the application, such as `https://ids.example.com` (without a path or query). Production has no localhost fallback. HTTP is accepted only for a loopback address in Development. A fixed origin prevents a forged request Host header from changing emailed links.

The create-user button is enabled only when email settings and the account-link origin are configured and employee invitations are allowed in Security controls. When unavailable, the page displays the blocking reason beside the button. Recovery links require email settings and an account-link origin, but do not depend on the invitation policy.

If an invitation fails or expires, open the employee's admin page and choose **Send Account Link**. This also sends a password reset link to an established employee without changing their current password until they use it. The email service must be configured for this action.
