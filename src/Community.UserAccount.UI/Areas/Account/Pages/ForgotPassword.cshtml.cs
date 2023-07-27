using Community.UserAccount.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Community.UserAccount.UI.Areas.Account.Pages
{
    [Authorize]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<CommunityUser> _userManager;
        private readonly IGPGService _gpgService;

        [Display(Name = "Enter your username")]
        [BindProperty]
        [Required]
        public string? UserName { get; set; }

        [BindProperty]
        public string? Code { get; set; }

        public string? PgpMessage { get; set; }

        public ForgotPasswordModel(UserManager<CommunityUser> userManager, IGPGService gpgService)
        {
            _userManager = userManager;
            _gpgService = gpgService;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(UserName);
                if (user == null || string.IsNullOrEmpty(user.GpgFingerprint))
                {
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                PgpMessage = _gpgService.EncryptMessageForUser(code, user.GpgFingerprint);
            }

            return Page();
        }

        public IActionResult OnPostCode()
        {
            if (!string.IsNullOrEmpty(Code))
                return RedirectToPage("./ResetPassword", new
                {
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(Code)),
                    userName = UserName
                });
            return Page();
        }
    }
}
