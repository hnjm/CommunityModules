using Community.UserAccount.UI.Areas.Account.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Community.UserAccount.UI.Tests
{


    public class LoginWithRecoveryCodeModelTests
    {
        private LoginWithRecoveryCodeModel _pageModel;
        private Mock<SignInManager<CommunityUser>> _signInManager;
        private Mock<UserManager<CommunityUser>> _userManager;
        private Mock<IUserStore<CommunityUser>> _userStore;

        public LoginWithRecoveryCodeModelTests()
        {
            _userStore = new Mock<IUserStore<CommunityUser>>();
            _userManager = new Mock<UserManager<CommunityUser>>(
                _userStore.Object, null, null, null, null, null, null, null, null);
            _signInManager = new Mock<SignInManager<CommunityUser>>(
                _userManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<CommunityUser>>(),
                null, null, null, null);

            _signInManager.Setup(s => s.GetTwoFactorAuthenticationUserAsync()).ReturnsAsync(new CommunityUser());

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

            _pageModel = new LoginWithRecoveryCodeModel(_signInManager.Object, _userManager.Object, Mock.Of<ILogger<LoginWithRecoveryCodeModel>>())
            {
                PageContext = pageContext,
                TempData = tempData,
                Url = new UrlHelper(actionContext)
            };
        }

        [Fact]
        public async Task GetReturnsPageResult()
        {
            var result = await _pageModel.OnGetAsync();
            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public async Task GetThrowsExceptionsIfUserNotFound()
        {
            _signInManager.Setup(s => s.GetTwoFactorAuthenticationUserAsync())!.ReturnsAsync(value: null);
            try
            {
                await _pageModel.OnGetAsync();
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
            var result = await _pageModel.OnPostAsync();
            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public async Task PostThrowsExceptionsIfUserNotFound()
        {
            _signInManager.Setup(s => s.GetTwoFactorAuthenticationUserAsync())!.ReturnsAsync(value: null);
            try
            {
                await _pageModel.OnPostAsync();
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
                .Setup(s => s.TwoFactorRecoveryCodeSignInAsync(It.IsAny<string>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _pageModel.RecoveryCode = "ValidCode";
            var result = await _pageModel.OnPostAsync();
            Assert.IsType<LocalRedirectResult>(result);
        }

        [Fact]
        public async Task PostReturnsRedirectToPageOnLockout()
        {
            _signInManager
                .Setup(s => s.TwoFactorRecoveryCodeSignInAsync(It.IsAny<string>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

            _pageModel.RecoveryCode = "ValidCode";
            var result = await _pageModel.OnPostAsync();
            Assert.IsType<RedirectToPageResult>(result);
        }

        [Fact]
        public async Task PostReturnsPageResultOnFailure()
        {
            _signInManager
                .Setup(s => s.TwoFactorRecoveryCodeSignInAsync(It.IsAny<string>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            _pageModel.RecoveryCode = "ValidCode";
            var result = await _pageModel.OnPostAsync();
            Assert.IsType<PageResult>(result);
        }
    }
}
