namespace Community.UserAccount;

using Microsoft.AspNetCore.Identity;

public class CommunityUser : IdentityUser<Guid>
{
    [ProtectedPersonalData]
    public string? GpgPublicKey { get; set; }
    [ProtectedPersonalData]
    public string? GpgFingerprint { get; set; }
}
