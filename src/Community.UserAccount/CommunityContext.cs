using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Community.UserAccount
{
    public partial class CommunityContext : IdentityDbContext<CommunityUser, IdentityRole<Guid>, Guid>
    {
        public CommunityContext(DbContextOptions<CommunityContext> options) : base(options) { }
    }
}
