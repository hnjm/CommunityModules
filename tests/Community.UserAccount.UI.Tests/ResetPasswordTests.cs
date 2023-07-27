using Community.UserAccount.UI.Areas.Account.Pages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Community.UserAccount.UI.Tests
{
    public class ResetPasswordTests
    {
        private readonly ResetPasswordModel _pageModel;
        private readonly Mock<UserManager<CommunityUser>> _userManager;
        private readonly Mock<IUserStore<CommunityUser>> _userStore;

        // The test doesn't work if the code isn't a valid base64 encoded string.
        private readonly string _testCode = "CfDJ8Eyv106TtERBkjWbcXiWNa7aCUlkmn7tllU8dgbIyogigIRoql7oQy7RQYv1Ig5Yg2zN5qdNLgUNHyLCclwzSP4BbE3AiCGfb0a8BGJLMEtoAduyxmjOg2FaKa1IadFVjOF9bfknEpHIDga/PfJIWExFFdNoT+Do9RasLlG6DktJFk8X3ijTJyyP0sPp1DBuj6ro8adlHI+v8p1ufxVwyKoAZrCD0cvmiSl7Y2mMQ4qR";
        private readonly string _testUsername = "test";
        private readonly CommunityUser _testUser;

        public ResetPasswordTests()
        {
            _userStore = new Mock<IUserStore<CommunityUser>>();
            _userManager = new Mock<UserManager<CommunityUser>>(
                _userStore.Object, null, null, null, null, null, null, null, null);
            
            _pageModel = new ResetPasswordModel(_userManager.Object);
            _pageModel.Input = new ResetPasswordModel.InputModel();
            
            _testUser = new CommunityUser()
            {
                UserName = _testUsername,
            };

            _userManager
                .Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(_testUser);
        }

        [Fact]
        public void ReturnsBadRequestWhenCodeIsNotSet()
        {
            var result = _pageModel.OnGet();
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void PageDecryptCodeOnGet()
        {
            _pageModel.OnGet(_testCode, _testUsername);
            Assert.Equal(Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(_testCode)), _pageModel.Code);
        }

        [Fact]
        public void PageSetsUsernameOnGet()
        {
            _pageModel.OnGet(_testCode, _testUsername);
            Assert.Equal(_testUsername, _pageModel.Username);
        }

        [Fact]
        public void GetReturnsPageResultOnSuccess()
        {
            var result = _pageModel.OnGet(_testCode, _testUsername);
            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public async Task PostReturnPageResultOnError()
        {
            _pageModel.ModelState.AddModelError("", "");
            var result = await _pageModel.OnPostAsync();
            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public async Task PostReturnRedirectToPageResultWhenUserNotFound()
        {
            _userManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))!.ReturnsAsync(value: null);
            var result = await _pageModel.OnPostAsync();
            Assert.IsType<RedirectToPageResult>(result);
        }

        [Fact]
        public async Task PostReturnRedirectToPageResultWhenPasswordChanged()
        {
            _userManager
                .Setup(m => m.ResetPasswordAsync(It.IsAny<CommunityUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _pageModel.OnPostAsync();
            Assert.IsType<RedirectToPageResult>(result);
        }

        [Fact]
        public async Task PostReturnPageResultWhenPasswordCannotBeChanged()
        {
            _userManager
                .Setup(m => m.ResetPasswordAsync(It.IsAny<CommunityUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError() { Description = "" }));

            var result = await _pageModel.OnPostAsync();
            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public async Task PostContainsErrorMessageWhenPasswordCannotBeChanged()
        {
            _userManager
                .Setup(m => m.ResetPasswordAsync(It.IsAny<CommunityUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError() { Description = "" }));

            await _pageModel.OnPostAsync();
            Assert.True(_pageModel.ModelState.ErrorCount > 0);
        }
    }
}
