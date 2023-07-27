using Community.UserAccount.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Community.UserAccount.Services;

public class GpgAuthenticatorService : IGpgAuthenticatorService
{
    public string TokenType => TokenOptions.DefaultPhoneProvider;

    private readonly UserManager<CommunityUser> _userManager;
    private readonly IGPGService _gpgService;

    public GpgAuthenticatorService(UserManager<CommunityUser> userManager, IGPGService gpgService)
    {
        _userManager = userManager;
        _gpgService = gpgService;
    }


    public async Task<string?> GenerateEncryptedAuthenticationCode(CommunityUser? user)
    {
        if (user != null && !string.IsNullOrEmpty(user?.GpgFingerprint))
        {
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, TokenType);
            var encryptedMessage = _gpgService.EncryptMessageForUser(code, user.GpgFingerprint);
            return encryptedMessage;
        }

        return null;
    }

    public async Task<bool> ValidateAuthenticationCode(CommunityUser user, string? code)
    {
        return await _userManager.VerifyTwoFactorTokenAsync(user, TokenType, code);
    }
}
