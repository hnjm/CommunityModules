using Community.UserAccount.Interfaces;
using Community.UserAccount.UI.Areas.Account.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Security.Claims;

namespace Community.UserAccount.UI.Tests
{
    public class EnableAuthenticatorTests
    {
        private EnableAuthenticatorModel _pageModel;
        private Mock<SignInManager<CommunityUser>> _signInManager;
        private Mock<UserManager<CommunityUser>> _userManager;
        private Mock<IUserStore<CommunityUser>> _userStore;
        private Mock<IGpgAuthenticatorService> _gpgAuthenticatorService;
        private CommunityUser _validUser = new CommunityUser() { UserName = "Test" };

        private readonly string _encryptedMessage = "The truely encrypted message";
        private readonly string _invalidCode = "Theinvalidcode";
        private readonly string _validCode = "Thevalidcode";
        private readonly string _userId = "test";
        private readonly int _remainingCodeLeft = 10;
        private readonly IEnumerable<string> _recoveryCodes = new List<string>() { "Code 1 ", "Code 2" };

        public EnableAuthenticatorTests()
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

            _gpgAuthenticatorService.Setup(g => g.GenerateEncryptedAuthenticationCode(It.IsAny<CommunityUser>())).ReturnsAsync(_encryptedMessage);
            _gpgAuthenticatorService.Setup(g => g.ValidateAuthenticationCode(It.IsAny<CommunityUser>(), _invalidCode)).ReturnsAsync(false);
            _gpgAuthenticatorService.Setup(g => g.ValidateAuthenticationCode(It.IsAny<CommunityUser>(), _validCode)).ReturnsAsync(true);

            _userManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_validUser);
            _userManager.Setup(m => m.CountRecoveryCodesAsync(It.IsAny<CommunityUser>())).ReturnsAsync(_remainingCodeLeft);
            _userManager.Setup(m => m.GetUserIdAsync(It.IsAny<CommunityUser>())).ReturnsAsync(_userId);
            _userManager.Setup(m => m.GenerateNewTwoFactorRecoveryCodesAsync(It.IsAny<CommunityUser>(), It.IsAny<Int32>())).ReturnsAsync(_recoveryCodes);

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

            _pageModel = new EnableAuthenticatorModel(_userManager.Object, Mock.Of<ILogger<EnableAuthenticatorModel>>(), _gpgAuthenticatorService.Object)
            {
                PageContext = pageContext,
                TempData = tempData,
                Url = new UrlHelper(actionContext)
            };
        }

        [Fact]
        public async Task GetNotFoundIfUserNotFound()
        {
            _userManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))!.ReturnsAsync(value: null);
            var result = await _pageModel.OnGetAsync();
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetPageWhenUserFound()
        {
            var result = await _pageModel.OnGetAsync();
            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public async Task EncryptedMessageIsGeneratedOnGet()
        {
            await _pageModel.OnGetAsync();
            Assert.Equal(_encryptedMessage, _pageModel.GPGMessage);
        }

        [Fact]
        public async Task PostNotFoundIfUserIsNull()
        {
            _userManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))!.ReturnsAsync(value: null);
            var result = await _pageModel.OnPostAsync();
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task PostReturnPageResultWhenModelIsInvalid()
        {
            _pageModel.ModelState.AddModelError("Test", "Test");
            var result = await _pageModel.OnPostAsync();
            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public async Task GpgMessageIsGeneratedWhenModelIsInvalid()
        {
            _pageModel.ModelState.AddModelError("Test", "Test");
            await _pageModel.OnPostAsync();
            _gpgAuthenticatorService.Verify(a => a.GenerateEncryptedAuthenticationCode(It.IsAny<CommunityUser>()), Times.Once());
        }

        [Fact]
        public async Task GpgMessageIsSetWhenModelIsInvalid()
        {
            _pageModel.ModelState.AddModelError("Test", "Test");
            await _pageModel.OnPostAsync();
            Assert.Equal(_encryptedMessage, _pageModel.GPGMessage);
        }

        [Fact]
        public async Task InvalidCodeReturnPageResult()
        {
            _pageModel.Code = _invalidCode;
            var result = await _pageModel.OnPostAsync();
            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public async Task InvalidCodeAddValidationError()
        {
            _pageModel.Code = _invalidCode;
            var result = await _pageModel.OnPostAsync();
            Assert.False(_pageModel.ModelState.IsValid);
        }
        
        [Fact]
        public async Task InvalidCodeRegenerateEncryptedMessage()
        {
            _pageModel.Code = _invalidCode;
            var result = await _pageModel.OnPostAsync();
            _gpgAuthenticatorService.Verify(a => a.GenerateEncryptedAuthenticationCode(It.IsAny<CommunityUser>()), Times.Once);
        }

        [Fact]
        public async Task RedirectToShowRecoveryCodeWhenNewCodesAreGenerated()
        {
            _userManager.Setup(m => m.CountRecoveryCodesAsync(It.IsAny<CommunityUser>())).ReturnsAsync(0);
            _pageModel.Code = _validCode;
            var result = await _pageModel.OnPostAsync() as RedirectToPageResult;
            Assert.Equal("./ShowRecoveryCodes", result?.PageName);
        }

        [Fact]
        public async Task RecoveryCodesAreSetWhenNewCodesAreGenerated()
        {
            _userManager.Setup(m => m.CountRecoveryCodesAsync(It.IsAny<CommunityUser>())).ReturnsAsync(0);
            _pageModel.Code = _validCode;
            await _pageModel.OnPostAsync();
            Assert.Equal(_pageModel.RecoveryCodes, _recoveryCodes.ToArray());
        }

        [Fact]
        public async Task ResetAuthenticatorKeyAsyncIsCalledWhenValidCodeEntered()
        {
            _pageModel.Code = _validCode;
            await _pageModel.OnPostAsync();
            _userManager.Verify(m => m.ResetAuthenticatorKeyAsync(It.IsAny<CommunityUser>()), Times.Once);
        }

        [Fact]
        public async Task SetTwoFactorEnabledAsyncIsCalledWhenValidCodeEntered()
        {
            _pageModel.Code = _validCode;
            await _pageModel.OnPostAsync();
            _userManager.Verify(m => m.SetTwoFactorEnabledAsync(It.IsAny<CommunityUser>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task StatusMessageIsNotNullWhenCalideCodeEntered()
        {
            _pageModel.Code = _validCode;
            await _pageModel.OnPostAsync();
            Assert.NotNull(_pageModel.StatusMessage);
        }
    }
}
