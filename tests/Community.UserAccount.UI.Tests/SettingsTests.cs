using Community.UserAccount.Interfaces;
using Community.UserAccount.UI.Areas.Account.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Community.UserAccount.UI.Tests
{
    public class SettingsTests
    {
        private readonly SettingsModel _pageModel;
        private Mock<SignInManager<CommunityUser>> _signInManager;
        private Mock<UserManager<CommunityUser>> _userManagerMock;
        private Mock<IUserStore<CommunityUser>> _userStore;
        private readonly Mock<IGPGService> _gpgServiceMock;
        private readonly string _testUsername = "Test";
        private readonly string _testGpgPublicKey = "TestGPG";
        private readonly string _differentTestPublicKey = "diffrent key";
        private readonly string _testFingerprint = "fingerprint";

        public SettingsTests()
        {
            _userStore = new Mock<IUserStore<CommunityUser>>();
            _userManagerMock = new Mock<UserManager<CommunityUser>>(
                _userStore.Object, null, null, null, null, null, null, null, null);
            _signInManager = new Mock<SignInManager<CommunityUser>>(
                _userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<CommunityUser>>(),
                null, null, null, null);

            _gpgServiceMock = new Mock<IGPGService>();

            _userManagerMock
                .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                !.ReturnsAsync(new CommunityUser() { UserName = _testUsername, GpgPublicKey = _testGpgPublicKey });


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

            _pageModel = new SettingsModel(_userManagerMock.Object, _signInManager.Object, _gpgServiceMock.Object)
            {
                PageContext = pageContext,
                TempData = tempData,
                Url = new UrlHelper(actionContext)
            };
        }

        [Fact]
        public async Task GetCallsGetUserAsync()
        {
            await _pageModel.OnGetAsync();
            _userManagerMock.Verify(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
        }

        [Fact]
        public async Task GetSetsUserName()
        {
            await _pageModel.OnGetAsync();
            Assert.Equal(_testUsername, _pageModel.Username);
        }

        [Fact]
        public async Task GetSetsGpgPublicKey()
        {
            await _pageModel.OnGetAsync();
            Assert.Equal(_testGpgPublicKey, _pageModel.GPGPublicKey);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetSetsIs2FAEnabled(bool value)
        {
            _userManagerMock.Setup(m => m.GetTwoFactorEnabledAsync(It.IsAny<CommunityUser>())).ReturnsAsync(value);
            await _pageModel.OnGetAsync();
            Assert.Equal(value, _pageModel.Is2FAEnabled);
        }

        [Fact]
        public async Task GetCallsGetTwoFactorEnabledAsync()
        {
            await _pageModel.OnGetAsync();
            _userManagerMock.Verify(m => m.GetTwoFactorEnabledAsync(It.IsAny<CommunityUser>()), Times.Once);
        }

        [Fact]
        public async Task ReturnsNotFoundResultWhenUserNotFound()
        {
            _userManagerMock
                .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                !.ReturnsAsync(value: null);

            var result = await _pageModel.OnPostAsync();
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task PostReturnPageOnModelError()
        {
            _pageModel.ModelState.AddModelError("", "");
            var result = await _pageModel.OnPostAsync();
            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public async Task PostReturnsPageWhenPgpIsEmpty()
        {
            _pageModel.GPGPublicKey = string.Empty;
            var result = await _pageModel.OnPostAsync();
            Assert.IsType<PageResult>(result);
        }


        [Fact]
        public async Task PostCallsGetUserAsyncWhenPGPIsNotChanged()
        {
            _pageModel.GPGPublicKey = string.Empty;
            await _pageModel.OnPostAsync();
            _userManagerMock.Verify(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Exactly(2));
        }

        [Fact]
        public async Task PostSetsUsernameWhenPGPIsNotChanged()
        {
            _pageModel.GPGPublicKey = string.Empty;
            await _pageModel.OnPostAsync();
            Assert.Equal(_testUsername, _pageModel.Username);
        }

        [Fact]
        public async Task PostSetsGPGPublicKeyWhenPGPIsNotChanged()
        {
            _pageModel.GPGPublicKey = string.Empty;
            await _pageModel.OnPostAsync();
            Assert.Equal(_testGpgPublicKey, _pageModel.GPGPublicKey);
        }

        [Fact]
        public async Task PostCallsImportGPGPublicKey()
        {
            _pageModel.GPGPublicKey = _differentTestPublicKey;
            await _pageModel.OnPostAsync();
            _gpgServiceMock.Verify(g => g.ImportGpgPublicKey(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task PostReturnsRedirectToPageResultWhenUnableToImportKey()
        {
            _pageModel.GPGPublicKey = _differentTestPublicKey;
            _gpgServiceMock.Setup(g => g.ImportGpgPublicKey(It.IsAny<string>())).Returns(string.Empty);
            var result = await _pageModel.OnPostAsync();
            Assert.IsType<RedirectToPageResult>(result);
        }

        [Fact]
        public async Task PostReturnsSetsStatusMessageWhenUnableToImportKey()
        {
            _pageModel.GPGPublicKey = _differentTestPublicKey;
            _gpgServiceMock.Setup(g => g.ImportGpgPublicKey(It.IsAny<string>())).Returns(string.Empty);
            await _pageModel.OnPostAsync();
            Assert.Equal("Error: Failed to process your PGP public key.", _pageModel.StatusMessage);
        }

        [Fact]
        public async Task PostReturnsRedirectToPageResultWhenUserIsNotSaved()
        {
            _pageModel.GPGPublicKey = _differentTestPublicKey;
            _gpgServiceMock.Setup(g => g.ImportGpgPublicKey(It.IsAny<string>())).Returns(_testFingerprint);
            _userManagerMock
                .Setup(m => m.UpdateAsync(It.IsAny<CommunityUser>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError() { Description = "test" })); var result = await _pageModel.OnPostAsync();
            Assert.IsType<RedirectToPageResult>(result);
        }

        [Fact]
        public async Task PostSetsStatusMessageWhenUserIsNotSaved()
        {
            _pageModel.GPGPublicKey = _differentTestPublicKey;
            _gpgServiceMock.Setup(g => g.ImportGpgPublicKey(It.IsAny<string>())).Returns(_testFingerprint);
            _userManagerMock
                .Setup(m => m.UpdateAsync(It.IsAny<CommunityUser>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError() { Description = "test" }));
            var result = await _pageModel.OnPostAsync();
            Assert.Equal("Error: Unable to save changes.", _pageModel.StatusMessage);
        }

        [Fact]
        public async Task PostReturnsRedirectToPageResultWhenUserIsSaved()
        {
            _pageModel.GPGPublicKey = _differentTestPublicKey;
            _gpgServiceMock.Setup(g => g.ImportGpgPublicKey(It.IsAny<string>())).Returns(_testFingerprint);
            _userManagerMock
                .Setup(m => m.UpdateAsync(It.IsAny<CommunityUser>()))
                .ReturnsAsync(IdentityResult.Success); 
            var result = await _pageModel.OnPostAsync();
            Assert.IsType<RedirectToPageResult>(result);
        }

        [Fact]
        public async Task PostReturnsSetsStatusMessageWhenUserIsSaved()
        {
            _pageModel.GPGPublicKey = _differentTestPublicKey;
            _gpgServiceMock.Setup(g => g.ImportGpgPublicKey(It.IsAny<string>())).Returns(_testFingerprint);
            _userManagerMock
                .Setup(m => m.UpdateAsync(It.IsAny<CommunityUser>()))
                .ReturnsAsync(IdentityResult.Success);
            await _pageModel.OnPostAsync();
            Assert.Equal("Success: Your key has been updated successfully", _pageModel.StatusMessage);
        }

        [Fact]
        public async Task PostAddClaimsOnGpgChange()
        {
            _pageModel.GPGPublicKey = _differentTestPublicKey;
            _gpgServiceMock.Setup(g => g.ImportGpgPublicKey(It.IsAny<string>())).Returns(_testFingerprint);
            _userManagerMock
                .Setup(m => m.UpdateAsync(It.IsAny<CommunityUser>()))
                .ReturnsAsync(IdentityResult.Success);

            await _pageModel.OnPostAsync();

            _userManagerMock.Verify(m => m.AddClaimAsync(It.IsAny<CommunityUser>(), It.IsAny<Claim>()), Times.Once);
        }

        [Fact]
        public async Task PostCallsSignInOnGpgChange()
        {
            _pageModel.GPGPublicKey = _differentTestPublicKey;
            _gpgServiceMock.Setup(g => g.ImportGpgPublicKey(It.IsAny<string>())).Returns(_testFingerprint);
            _userManagerMock
                .Setup(m => m.UpdateAsync(It.IsAny<CommunityUser>()))
                .ReturnsAsync(IdentityResult.Success);

            await _pageModel.OnPostAsync();

            _signInManager.Verify(s => s.SignInAsync(It.IsAny<CommunityUser>(), It.IsAny<bool>(), null), Times.Once());
        }

        [Fact]
        public async Task OnPostDeleteGPGPublicKeyAsyncReturnsNotFoundWhenNoUser()
        {
            _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))!.ReturnsAsync(value: null);   
            var result = await _pageModel.OnPostDeleteGPGPublicKeyAsync();
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task OnPostDeleteGPGPublicKeyAsyncCallsRemoveClaimAsync()
        {
            await _pageModel.OnPostDeleteGPGPublicKeyAsync();
            _userManagerMock.Verify(m => m.RemoveClaimAsync(It.IsAny<CommunityUser>(), It.IsAny<Claim>()), Times.Once);
        }

        [Fact]
        public async Task OnPostDeleteGPGPublicKeyAsyncCallsUpdateAsync()
        {
            await _pageModel.OnPostDeleteGPGPublicKeyAsync();
            _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<CommunityUser>()), Times.Once);
        }

        [Fact]
        public async Task OnPostDeleteGPGPublicKeyAsyncCallsSignInAsync()
        {
            await _pageModel.OnPostDeleteGPGPublicKeyAsync();
            _signInManager.Verify(s => s.SignInAsync(It.IsAny<CommunityUser>(), It.IsAny<bool>(), null), Times.Once);
        }
    }
}
