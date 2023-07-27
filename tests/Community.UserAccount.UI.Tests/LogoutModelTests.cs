using Community.UserAccount.UI.Areas.Account.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Community.UserAccount.UI.Tests;

public class LogoutModelTests
{
    private LogoutModel _pageModel;
    private Mock<SignInManager<CommunityUser>> _signInManager;
    private Mock<UserManager<CommunityUser>> _userManager;
    private Mock<IUserStore<CommunityUser>> _userStore;


    public LogoutModelTests()
    {
        _userStore = new Mock<IUserStore<CommunityUser>>();
        _userManager = new Mock<UserManager<CommunityUser>>(
            _userStore.Object, null, null, null, null, null, null, null, null);
        _signInManager = new Mock<SignInManager<CommunityUser>>(
            _userManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<CommunityUser>>(),
            null, null, null, null);

        _pageModel = new LogoutModel(_signInManager.Object, Mock.Of<ILogger<LogoutModel>>());
    }

    [Fact]
    public async void LogoutCallSignOut()
    {
        await _pageModel.OnPostAsync();
        _signInManager.Verify(s => s.SignOutAsync(), Times.Once);
    }

    [Fact]
    public async void LogoutReturnsRedirectToPage()
    {
        var result = await _pageModel.OnPostAsync();
        Assert.IsType<LocalRedirectResult>(result);
    }
}
