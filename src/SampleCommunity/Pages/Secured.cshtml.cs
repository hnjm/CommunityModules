using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SampleCommunity.Pages
{
    [Authorize(Roles = "Test")]
    public class SecuredModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
