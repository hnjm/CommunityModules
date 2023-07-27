using Community.UserAccount.UI.Areas.Account.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace Community.UserAccount.UI.Tests
{
    public class RegisterModelTests
    {
        private RegisterModel _pageModel;
        private Mock<SignInManager<CommunityUser>> _signInManager;
        private Mock<UserManager<CommunityUser>> _userManager;
        private Mock<IUserStore<CommunityUser>> _userStore;
        private ILogger<RegisterModel> _logger;

        public RegisterModelTests()
        {
            _userStore = new Mock<IUserStore<CommunityUser>>();
            _userManager = new Mock<UserManager<CommunityUser>>(
                _userStore.Object, null, null, null, null, null, null, null, null);
            _signInManager = new Mock<SignInManager<CommunityUser>>(
                _userManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<CommunityUser>>(),
                null, null, null, null);

            _logger = Mock.Of<ILogger<RegisterModel>>();

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
            _pageModel = new RegisterModel(
                _signInManager.Object,
                _userManager.Object,
                _userStore.Object,
                _logger
            )
            {
                PageContext = pageContext,
                TempData = tempData,
                Url = new UrlHelper(actionContext)
            };
        }

        [Theory]
        [InlineData("/Abount")]
        [InlineData(null)]
        public void ReturnUrlIsSetOnGet(string? url)
        {
            _pageModel.OnGet(url);
            Assert.Equal(url, _pageModel.ReturnUrl);
        }

        [Fact]
        public async Task ReturnsPageOnModelError()
        {
            _pageModel.ModelState.AddModelError("UserName", "Username is empty");
            var result = await _pageModel.OnPostAsync();

            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public async Task CanRegister()
        {
            setupWorkingTest();

            var result = await _pageModel.OnPostAsync();
            Assert.IsType<LocalRedirectResult>(result);
        }

        [Fact]
        public async Task UserStoreIsCalled()
        {
            setupWorkingTest();

            var result = await _pageModel.OnPostAsync();
            _userStore.Verify(
                s => s.SetUserNameAsync(It.IsAny<CommunityUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task UserManagerIsCalled()
        {
            setupWorkingTest();
            var result = await _pageModel.OnPostAsync();

            _userManager.Verify(
                m => m.CreateAsync(It.IsAny<CommunityUser>(), It.IsAny<string>()),
                Times.Once
            );
        }

        [Fact]
        public async Task SignInManagerIsCalled()
        {
            setupWorkingTest();
            var result = await _pageModel.OnPostAsync();

            _signInManager.Verify(s => s.SignInAsync(It.IsAny<CommunityUser>(), It.IsAny<bool>(), null), Times.Once);
        }

        [Fact]
        public async Task ReturnsPageOnIdentityError()
        {
            setupErrorTest();
            var result = await _pageModel.OnPostAsync();

            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public async Task NoSignInOnIdentityError()
        {
            setupErrorTest();
            var result = await _pageModel.OnPostAsync();

            _signInManager.Verify(s => s.SignInAsync(It.IsAny<CommunityUser>(), It.IsAny<bool>(), null), Times.Never);
        }

        private void setupWorkingTest()
        {
            _userManager
                .Setup(u => u.CreateAsync(It.IsAny<CommunityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _pageModel.Input = new RegisterModel.InputModel { UserName = "Test", Password = "Test", ConfirmPassword = "Test" };
        }

        private void setupErrorTest()
        {
            _userManager
                .Setup(u => u.CreateAsync(It.IsAny<CommunityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError() { Description = "Test1" }, new IdentityError() { Description = "Test2" }));

            _pageModel.Input = new RegisterModel.InputModel { UserName = "Test", Password = "Test", ConfirmPassword = "Test" };
        }
    }
}
