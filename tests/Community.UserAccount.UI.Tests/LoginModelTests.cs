using Community.UserAccount.UI.Areas.Account.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Community.UserAccount.UI.Tests
{
    public class LoginModelTests
    {
        private LoginModel _pageModel;
        private Mock<SignInManager<CommunityUser>> _signInManager;
        private Mock<UserManager<CommunityUser>> _userManager;
        private Mock<IUserStore<CommunityUser>> _userStore;

        public LoginModelTests()
        {
            _userStore = new Mock<IUserStore<CommunityUser>>();
            _userManager = new Mock<UserManager<CommunityUser>>(
                _userStore.Object, null, null, null, null, null, null, null, null);
            _signInManager = new Mock<SignInManager<CommunityUser>>(
                _userManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<CommunityUser>>(),
                null, null, null, null);

            var httpContext = new DefaultHttpContext();
            var modelState = new ModelStateDictionary();
            var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
            var modelMetadataProvider = new EmptyModelMetadataProvider();
            var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            var pageContext = new PageContext(actionContext)
            {
                ViewData = viewData
            };



            _pageModel = new LoginModel(
                _signInManager.Object,
                Mock.Of<ILogger<LoginModel>>()
            )
            {
                PageContext = pageContext,
                TempData = tempData,
                Url = new UrlHelper(actionContext)
            };
        }

        [Theory]
        [InlineData(null)]
        [InlineData("/Something")]
        [InlineData("/")]
        public async Task UsesReturnUrlParameterOrDefaultRoot(string? url)
        {
            await _pageModel.OnGetAsync(url);
            Assert.Equal(url ?? "/", _pageModel.ReturnUrl);
        }

        [Fact]
        public async Task ErrorMessageCauseValidationError()
        {
            _pageModel.ErrorMessage = "Test error";
            await _pageModel.OnGetAsync();
            Assert.False(_pageModel.ModelState.IsValid);
        }

        [Fact]
        public async Task SingoutIsCalledOnGet()
        {
            await _pageModel.OnGetAsync();
            _signInManager.Verify(s => s.SignOutAsync(), Times.Once);
        }

        [Fact]
        public async Task ReturnPageResultWhenModelIsInvalid()
        {
            _pageModel.ModelState.AddModelError("Test", "Test");
            var result = await _pageModel.OnPostAsync();
            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public async Task SuccessFullLoginReturnsLocalRedirect()
        {
            setupSignInManager(Microsoft.AspNetCore.Identity.SignInResult.Success);
            var result = await _pageModel.OnPostAsync();
            Assert.IsType<LocalRedirectResult>(result);
        }

        [Fact]
        public async Task Requires2FAReturnsRedirectToPage()
        {
            setupSignInManager(Microsoft.AspNetCore.Identity.SignInResult.TwoFactorRequired);
            var result = await _pageModel.OnPostAsync() as RedirectToPageResult;
            Assert.Equal("./LoginWith2fa", result?.PageName);
        }

        [Fact]
        public async Task LockedAccountReturnsLockoutPage()
        {
            setupSignInManager(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);
            var result = await _pageModel.OnPostAsync() as RedirectToPageResult;
            Assert.Equal("./Lockout", result?.PageName);
        }

        [Fact]
        public async Task FailedAttemptsReturnPage()
        {
            setupSignInManager(Microsoft.AspNetCore.Identity.SignInResult.Failed);
            var result = await _pageModel.OnPostAsync();
            Assert.IsType<PageResult>(result);
        }

        private void setupSignInManager(Microsoft.AspNetCore.Identity.SignInResult result)
        {
            _signInManager
                .Setup(s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(result);

            _pageModel.Input = new LoginModel.InputModel { UserName = "test", Password = "test", RememberMe = false };
        }
    }
}
