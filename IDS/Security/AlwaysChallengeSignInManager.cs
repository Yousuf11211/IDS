using IDS.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace IDS.Security;

/// <summary>
/// Requires an authenticator challenge on every password login for enrolled users.
/// Existing "remember this browser" cookies never bypass the second factor.
/// </summary>
public sealed class AlwaysChallengeSignInManager : SignInManager<ApplicationUser>
{
    public AlwaysChallengeSignInManager(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<ApplicationUser>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<ApplicationUser> confirmation)
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
    }

    public override Task<bool> IsTwoFactorClientRememberedAsync(ApplicationUser user)
        => Task.FromResult(false);
}
