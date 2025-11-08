// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using IDS.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }
        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Username")]
            public string UserName { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Display(Name = "First name")]
            public string FirstName { get; set; }

            [Display(Name = "Last name")]
            public string LastName { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            Username = user.UserName;
            Input = new InputModel
            {
                UserName = user.UserName,
                PhoneNumber = await _userManager.GetPhoneNumberAsync(user),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // Update username (with uniqueness check)
            if (!string.Equals(user.UserName, Input.UserName, System.StringComparison.Ordinal))
            {
                var existing = await _userManager.FindByNameAsync(Input.UserName);
                if (existing != null)
                {
                    ModelState.AddModelError(string.Empty, "Username is already taken.");
                    await LoadAsync(user);
                    return Page();
                }
                var setName = await _userManager.SetUserNameAsync(user, Input.UserName);
                if (!setName.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Could not update username.");
                    await LoadAsync(user);
                    return Page();
                }
            }

            // Update phone
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (!string.Equals(Input.PhoneNumber, phoneNumber, System.StringComparison.Ordinal))
            {
                var setPhone = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhone.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Unexpected error when trying to set phone number.");
                    return RedirectToPage();
                }
            }

            // Update email (with confirmation left as-is)
            if (!string.Equals(Input.Email, user.Email, System.StringComparison.OrdinalIgnoreCase))
            {
                user.Email = Input.Email;
                user.EmailConfirmed = false; // optional: force re-confirm
            }

            // Update names
            user.FirstName = Input.FirstName ?? string.Empty;
            user.LastName = Input.LastName ?? string.Empty;

            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Could not update profile.");
                await LoadAsync(user);
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
