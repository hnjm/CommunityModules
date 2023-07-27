using Community.UserAccount.Interfaces;
using Community.UserAccount.Services;
using Libgpgme;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Community.UserAccount.Tests;

public class GpgAuthenticatorServiceTests
{
    private readonly IGpgAuthenticatorService _gpgAuthenticatorService;
    private readonly Mock<IGPGService> _gpgService;
    private Mock<UserManager<CommunityUser>> _userManager;
    private Mock<IUserStore<CommunityUser>> _userStore;

    private readonly CommunityUser _invalidUser = new CommunityUser() { GpgFingerprint = null };
    private readonly CommunityUser _validUser = new CommunityUser() { GpgFingerprint = "ValidKey" };
    private readonly string _valideCode = "The valid code";
    private readonly string _encryptedMessage = "The encrypted message";

    public GpgAuthenticatorServiceTests()
    {
        _userStore = new Mock<IUserStore<CommunityUser>>();
        _userManager = new Mock<UserManager<CommunityUser>>(
            _userStore.Object, null, null, null, null, null, null, null, null);
        _gpgService = new Mock<IGPGService>();
        _gpgAuthenticatorService = new GpgAuthenticatorService(_userManager.Object, _gpgService.Object);

        _userManager.Setup(u => u.GenerateTwoFactorTokenAsync(_validUser, It.IsAny<string>())).ReturnsAsync(_valideCode);
        _gpgService.Setup(g => g.EncryptMessageForUser(_valideCode, _validUser.GpgFingerprint)).Returns(_encryptedMessage);
    }

    [Fact]
    public async Task GenerateEncryptedAuthenticationCodeReturnsNullWhenUserIsNull()
    {
        var result = await _gpgAuthenticatorService.GenerateEncryptedAuthenticationCode(null);
        Assert.Null(result);
    }

    [Fact]
    public async Task GenerateEncryptedAuthenticationCodeReturnsNullWhenUserDoesntHaveGPGFingerprint()
    {
        var result = await _gpgAuthenticatorService.GenerateEncryptedAuthenticationCode(_invalidUser);
        Assert.Null(result);
    }

    [Fact]
    public async Task GenerateEncryptedAuthenticationCodeReturnsEncryptedMessageOnSuccess()
    {
        var result = await _gpgAuthenticatorService.GenerateEncryptedAuthenticationCode(_validUser);
        Assert.Equal(_encryptedMessage, result);
    }

    [Fact]
    public async Task GenerateEncryptedAuthenticationCodeCallsGenerateTwoFactorToken()
    {
        await _gpgAuthenticatorService.GenerateEncryptedAuthenticationCode(_validUser);
        _userManager.Verify(u => u.GenerateTwoFactorTokenAsync(_validUser, It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public async Task GenerateEncryptedAuthenticationCodeCallsEncryptMessageForUser()
    {
        await _gpgAuthenticatorService.GenerateEncryptedAuthenticationCode(_validUser);
        _gpgService.Verify(g => g.EncryptMessageForUser(_valideCode, _validUser.GpgFingerprint!), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ValidateAuthenticationCodeReturnsResult(bool verificationResult)
    {
        _userManager.Setup(u => u.VerifyTwoFactorTokenAsync(_validUser, It.IsAny<string>(), _valideCode)).ReturnsAsync(verificationResult);
        var result = await _gpgAuthenticatorService.ValidateAuthenticationCode(_validUser, _valideCode);

        Assert.Equal(verificationResult, result);
    }

    [Fact]
    public async Task ValidateAuthenticationCodeCallsVerifyTwoFactorTokenAsync()
    {
        await _gpgAuthenticatorService.ValidateAuthenticationCode(_validUser, _valideCode);
        _userManager.Verify(u => u.VerifyTwoFactorTokenAsync(_validUser, It.IsAny<string>(), _valideCode), Times.Once);
    }
}
