using Community.UserAccount.Interfaces;
using Community.UserAccount.UI.Areas.Account.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace Community.UserAccount.UI.Tests
{
    public class LoginWith2faModelTests
    {
        private LoginWith2faModel _pageModel;
        private Mock<SignInManager<CommunityUser>> _signInManager;
        private Mock<UserManager<CommunityUser>> _userManager;
        private Mock<IUserStore<CommunityUser>> _userStore;
        private Mock<IGpgAuthenticatorService> _gpgAuthenticatorService;

        private readonly CommunityUser _user = new CommunityUser() { UserName = "Test" };
        private readonly string _encryptedMessage = "The encrypted message";

        public LoginWith2faModelTests()
        {
            _userStore = new Mock<IUserStore<CommunityUser>>();
            _userManager = new Mock<UserManager<CommunityUser>>(
                _userStore.Object, null, null, null, null, null, null, null, null);
            _signInManager = new Mock<SignInManager<CommunityUser>>(
                _userManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<CommunityUser>>(),
                null, null, null, null);
            _gpgAuthenticatorService = new Mock<IGpgAuthenticatorService>();

            _gpgAuthenticatorService
                .Setup(g => g.GenerateEncryptedAuthenticationCode(It.IsAny<CommunityUser>()))
                .ReturnsAsync(_encryptedMessage);

            _signInManager.Setup(s => s.GetTwoFactorAuthenticationUserAsync()).ReturnsAsync(_user);

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

            _pageModel = new LoginWith2faModel(_signInManager.Object, _userManager.Object, Mock.Of<ILogger<LoginWith2faModel>>(), _gpgAuthenticatorService.Object)
            {
                PageContext = pageContext,
                TempData = tempData,
                Url = new UrlHelper(actionContext)
            };
        }

        [Fact]
        public async Task GetReturnsPageResult()
        {
            var result = await _pageModel.OnGetAsync(false);
            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public async Task GetSetsTheGPGMessage()
        {
            await _pageModel.OnGetAsync(false);
            Assert.Equal(_encryptedMessage, _pageModel.GPGMessage);
        }

        [Fact]
        public async Task GetThrowsExceptionsIfUserNotFound()
        {
            _signInManager.Setup(s => s.GetTwoFactorAuthenticationUserAsync())!.ReturnsAsync(value: null);
            try
            {
                await _pageModel.OnGetAsync(false);
            }
            catch (Exception ex)
            {
                Assert.IsType<InvalidOperationException>(ex);
            }
        }

        [Fact]
        public async Task ReturnsPageWhenModelStateIsInvalid()
        {
            _pageModel.ModelState.AddModelError("Test", "Test");
            var result = await _pageModel.OnPostAsync(true);
            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public async Task PostThrowsExceptionsIfUserNotFound()
        {
            _signInManager.Setup(s => s.GetTwoFactorAuthenticationUserAsync())!.ReturnsAsync(value: null);
            try
            {
                await _pageModel.OnPostAsync(false);
            }
            catch (Exception ex)
            {
                Assert.IsType<InvalidOperationException>(ex);
            }
        }

        [Fact]
        public async Task PostReturnsLocalRedirectOnSuccess()
        {
            _signInManager
                .Setup(s => s.TwoFactorSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _pageModel.TwoFactorCode = "ValidCode";
            var result = await _pageModel.OnPostAsync(true);
            Assert.IsType<LocalRedirectResult>(result);
        }

        [Fact]
        public async Task PostReturnsRedirectToPageOnLockout()
        {
            _signInManager
                .Setup(s => s.TwoFactorSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

            _pageModel.TwoFactorCode = "ValidCode";
            var result = await _pageModel.OnPostAsync(true);
            Assert.IsType<RedirectToPageResult>(result);
        }

        [Fact]
        public async Task PostReturnsPageResultOnFailure()
        {
            _signInManager
                .Setup(s => s.TwoFactorSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            _pageModel.TwoFactorCode = "ValidCode";
            var result = await _pageModel.OnPostAsync(true);
            Assert.IsType<PageResult>(result);
        }
    }
}
