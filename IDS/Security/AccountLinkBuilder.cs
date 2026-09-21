using Microsoft.AspNetCore.Mvc;

namespace IDS.Security;

/// <summary>
/// Builds account links from a configured public origin, so an untrusted Host header
/// cannot change the destination of an emailed password link.
/// </summary>
public sealed class AccountLinkBuilder
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public AccountLinkBuilder(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public bool IsConfigured => GetPublicOrigin() != null;

    public string? PasswordResetLink(IUrlHelper url, string encodedToken)
    {
        var origin = GetPublicOrigin();
        var path = url.Page("/Account/ResetPassword", values: new { area = "Identity", code = encodedToken });
        return origin == null || string.IsNullOrWhiteSpace(path) ? null : origin + path;
    }

    private string? GetPublicOrigin()
    {
        var value = Environment.GetEnvironmentVariable("PUBLIC_BASE_URL")
            ?? _configuration["Application:PublicBaseUrl"];
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment) ||
            uri.AbsolutePath != "/")
        {
            return null;
        }

        // Local HTTP is useful during development; deployed account links require HTTPS.
        if (uri.Scheme != Uri.UriSchemeHttps &&
            !(_environment.IsDevelopment() && uri.Scheme == Uri.UriSchemeHttp && uri.IsLoopback))
        {
            return null;
        }

        return uri.GetLeftPart(UriPartial.Authority);
    }
}
