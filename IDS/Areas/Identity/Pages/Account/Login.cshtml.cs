// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using IDS.Security;
using IDS.Data.Models;

namespace IDS.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // Support login with either username or email
                ApplicationUser user = await _userManager.FindByNameAsync(Input.Email);
                if (user == null)
                {
                    user = await _userManager.FindByEmailAsync(Input.Email);
                }

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }

                // Block suspended users first
                if (await _userManager.IsInRoleAsync(user, AppRoles.Suspended))
                {
                    ModelState.AddModelError(string.Empty, "Your account is currently suspended. Contact admin for further assistance.");
                    _logger.LogWarning("Suspended user {EmailOrUser} attempted to log in.", Input.Email);
                    return Page();
                }

                // Verify the password
                var passwordValid = await _userManager.CheckPasswordAsync(user, Input.Password);
                if (!passwordValid)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }

                // Check if 2FA should be enforced
                var require2faEnvValue = Environment.GetEnvironmentVariable("REQUIRE_2FA_ON_LOGIN");
                var is2faGloballyRequired = string.Equals(require2faEnvValue, "true", StringComparison.OrdinalIgnoreCase);
                var userHas2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

                _logger.LogInformation("Login attempt for {Email}. REQUIRE_2FA_ON_LOGIN={EnvValue}, is2faGloballyRequired={Global}, userHas2fa={User2fa}", 
           Input.Email, require2faEnvValue ?? "(null)", is2faGloballyRequired, userHas2faEnabled);

                // Only require 2FA if BOTH: global setting is true AND user has 2FA enabled
                if (is2faGloballyRequired && userHas2faEnabled)
                {
                    _logger.LogInformation("2FA required for user {Email}. Redirecting to 2FA page.", Input.Email);
   
                    // Use PasswordSignInAsync to set up the 2FA flow properly
                    var signInResult = await _signInManager.PasswordSignInAsync(user, Input.Password, Input.RememberMe, lockoutOnFailure: false);
               
                    if (signInResult.RequiresTwoFactor)
                    {
                        return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                    }
        
                    // If somehow 2FA wasn't triggered, fall through to direct sign-in
                    if (signInResult.Succeeded)
                    {
                        return LocalRedirect(returnUrl);
                    }
                }

                // 2FA is either globally disabled OR user doesn't have it enabled
                // Sign in directly without 2FA
                _logger.LogInformation("Signing in user {Email} directly (2FA bypassed or not enabled).", Input.Email);
                await _signInManager.SignInAsync(user, Input.RememberMe);
                return LocalRedirect(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
