using Community.UserAccount.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Community.UserAccount.UI.Areas.Account.Pages
{
    [Authorize]
    public class EnableAuthenticatorModel : PageModel
    {
        private readonly UserManager<CommunityUser> _userManager;
        private readonly ILogger<EnableAuthenticatorModel> _logger;
        private readonly IGpgAuthenticatorService _gpgAuthenticatorService;

        public EnableAuthenticatorModel(
            UserManager<CommunityUser> userManager, ILogger<EnableAuthenticatorModel> logger, IGpgAuthenticatorService gpgAuthenticatorService)
        {
            _userManager = userManager;
            _gpgAuthenticatorService = gpgAuthenticatorService;
            _logger = logger;
        }

        public string? GPGMessage { get; set; }

        [TempData]
        public string[]? RecoveryCodes { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        [BindProperty]
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Verification Code")]
        public string? Code { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            GPGMessage = await _gpgAuthenticatorService.GenerateEncryptedAuthenticationCode(user);

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
                GPGMessage = await _gpgAuthenticatorService.GenerateEncryptedAuthenticationCode(user);
                return Page();
            }

            // Strip spaces and hyphens
            var verificationCode = Code?.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await _gpgAuthenticatorService.ValidateAuthenticationCode(user, verificationCode);
            if (!is2faTokenValid)
            {
                ModelState.AddModelError("Input.Code", "Verification code is invalid.");
                GPGMessage = await _gpgAuthenticatorService.GenerateEncryptedAuthenticationCode(user);
                return Page();
            }

            await _userManager.ResetAuthenticatorKeyAsync(user);
            await _userManager.SetTwoFactorEnabledAsync(user, true);
            var userId = await _userManager.GetUserIdAsync(user);
            _logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", userId);

            StatusMessage = "Your authenticator app has been verified.";

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            RecoveryCodes = recoveryCodes.ToArray();
            return RedirectToPage("./ShowRecoveryCodes");
        }
    }
}
