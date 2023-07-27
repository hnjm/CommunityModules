using Community.UserAccount.UI.Areas.Account.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Community.UserAccount.UI.Tests
{
    public class ShowRecoveryCodesModelTests
    {
        private ShowRecoveryCodesModel _pageModel;

        public ShowRecoveryCodesModelTests()
        {
            _pageModel = new ShowRecoveryCodesModel();
            _pageModel.RecoveryCodes = new string[5] { "1", "2", "3", "4", "5" };
        }

        [Fact]
        public void ReturnsGetReturnsPageResultWhenRecoveryCodesAvailable()
        {
            var result = _pageModel.OnGet();
            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public void RedirectToPageWhenNoRecoveryCodesLeft()
        {
            _pageModel.RecoveryCodes = null;
            var result = _pageModel.OnGet();
            Assert.IsType<RedirectToPageResult>(result);
        }

        [Fact]
        public void RedirectToPageWhenNoRecoveryCodesLeftEmptyArray()
        {
            _pageModel.RecoveryCodes = new string[0];
            var result = _pageModel.OnGet();
            Assert.IsType<RedirectToPageResult>(result);
        }

        [Fact]
        public void RedirectsToSettings()
        {
            _pageModel.RecoveryCodes = null;
            var result = _pageModel.OnGet() as RedirectToPageResult;
            Assert.Equal("./Settings", result?.PageName);
        }
    }
}
