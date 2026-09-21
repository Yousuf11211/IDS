# Employee account setup

Admin invitations and password resets send a one-time Identity link. The link is valid for 24 hours and lets the employee choose a password; no password is sent by email. An invited account cannot sign in until the link is used. The first login then requires authenticator enrollment, and later logins require an authenticator code.

Configure `EMAIL_STATUS=true`, `MAILJET_API_KEY`, `MAILJET_SECRET_KEY`, and `PUBLIC_BASE_URL` before creating employees. `PUBLIC_BASE_URL` is the public HTTPS origin of the application, such as `https://ids.example.com` (without a path or trailing query). HTTP is accepted only for a loopback address in Development. A fixed origin prevents a forged request Host header from changing emailed links.

If an invitation fails or expires, open the employee's admin page and choose **Send Account Link**. This also sends a password reset link to an established employee without changing their current password until they use it. The email service must be configured for this action.
