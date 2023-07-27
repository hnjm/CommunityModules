using Community.UserAccount.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Community.UserAccount.UI.Areas.Account.Pages
{
    [Authorize]
    public class SettingsModel : PageModel
    {
        private readonly UserManager<CommunityUser> _userManager;
        private readonly SignInManager<CommunityUser> _signInManager;
        private readonly IGPGService _GPGService;

        public SettingsModel(UserManager<CommunityUser> userManager, SignInManager<CommunityUser> signInManager, IGPGService gPGService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _GPGService = gPGService;
        }

        [TempData]
        public string? StatusMessage { get; set; }

        public string? Username { get; set; }

        [BindProperty]
        public string? GPGPublicKey { get; set; }

        [BindProperty]
        public bool Is2FAEnabled { get; set; }

        public async Task OnGetAsync()
        {
            await LoadUserAsync();
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
                return Page();
            }

            if (!string.IsNullOrEmpty(GPGPublicKey) && GPGPublicKey != user.GpgPublicKey)
            {
                user.GpgPublicKey = GPGPublicKey;
                var fingerprint = _GPGService.ImportGpgPublicKey(GPGPublicKey);
                if (string.IsNullOrEmpty(fingerprint))
                {
                    StatusMessage = "Error: Failed to process your PGP public key.";
                    return RedirectToPage();
                }

                user.GpgFingerprint = fingerprint;
                var userSaved = await _userManager.UpdateAsync(user);

                if (!userSaved.Succeeded)
                {
                    StatusMessage = "Error: Unable to save changes.";
                    return RedirectToPage();
                }

                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("HasValidGPGPublicKey", "true"));
                await _signInManager.SignInAsync(user, false);

                StatusMessage = "Success: Your key has been updated successfully";
                return RedirectToPage();
            }

            await LoadUserAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteGPGPublicKeyAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null) return NotFound();

            var claim = User.Claims.FirstOrDefault(c => c.Type == "HasValidGPGPublicKey");

            user.GpgFingerprint = null;
            user.GpgPublicKey = null;

            await _userManager.RemoveClaimAsync(user, claim);
            await _userManager.UpdateAsync(user);
            await _signInManager.SignInAsync(user, false);

            if (Is2FAEnabled)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, false);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDisable2FAAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred disabling 2FA.");
            }

            StatusMessage = "2fa has been disabled. You can reenable 2fa when you setup an authenticator app";
            return RedirectToPage();
        }

        private async Task LoadUserAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            Username = user.UserName;
            GPGPublicKey = user.GpgPublicKey;
            Is2FAEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        }
    }
}
