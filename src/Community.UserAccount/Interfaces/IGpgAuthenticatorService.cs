namespace Community.UserAccount.Interfaces;

public interface IGpgAuthenticatorService
{
    string TokenType { get; }
    Task<string?> GenerateEncryptedAuthenticationCode(CommunityUser? user);
    Task<bool> ValidateAuthenticationCode(CommunityUser user, string? code);
}
