using IDS.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace IDS.Security;

/// <summary>
/// Requires an authenticator challenge on every password login for enrolled users,
/// except accounts covered by their role's Development-only test bypass.
/// Existing "remember this browser" cookies never bypass the second factor.
/// </summary>
public sealed class AlwaysChallengeSignInManager : SignInManager<ApplicationUser>
{
    private readonly SecurityPolicyService _policies;
    private readonly HashSet<string> _bypassedUserIds = new();
    public AlwaysChallengeSignInManager(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<ApplicationUser>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<ApplicationUser> confirmation,
        SecurityPolicyService policies)
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
        _policies = policies;
    }

    public override async Task<bool> IsTwoFactorEnabledAsync(ApplicationUser user)
    {
        if (!await _policies.CanBypassTwoFactorAsync(user)) return await base.IsTwoFactorEnabledAsync(user);
        _bypassedUserIds.Add(user.Id);
        return false;
    }

    public override async Task SignInWithClaimsAsync(ApplicationUser user, AuthenticationProperties? authenticationProperties,
        IEnumerable<Claim> additionalClaims)
    {
        // Mark test logins so restoring the requirement also ends already-open test sessions.
        var claims = additionalClaims.Where(claim => claim.Type != SecurityPolicyService.BypassedMfaClaim).ToList();
        var refreshingTestSession = Context.User.FindFirstValue(Options.ClaimsIdentity.UserIdClaimType) == user.Id &&
            Context.User.HasClaim(SecurityPolicyService.BypassedMfaClaim, "true");
        if (refreshingTestSession || _bypassedUserIds.Contains(user.Id) || await _policies.CanBypassTwoFactorAsync(user))
            claims.Add(new Claim(SecurityPolicyService.BypassedMfaClaim, "true"));
        await base.SignInWithClaimsAsync(user, authenticationProperties, claims);
    }

    public override Task<bool> IsTwoFactorClientRememberedAsync(ApplicationUser user)
        => Task.FromResult(false);
}
