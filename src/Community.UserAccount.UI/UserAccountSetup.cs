using Community.UserAccount.Interfaces;
using Community.UserAccount.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Community.UserAccount.UI
{
    public static class UserAccountSetup
    {
        public static IServiceCollection AddCommunityUserSupport<T>(this IServiceCollection services, Action<DbContextOptionsBuilder>? optionsAction)
            where T : CommunityContext
        {
            services.AddDbContext<T>(optionsAction);

            services.
                AddAuthentication(o =>
                {
                    o.DefaultScheme = IdentityConstants.ApplicationScheme;
                    o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddIdentityCookies(o => { });

            services
                .AddIdentityCore<CommunityUser>(options => { })
                .AddEntityFrameworkStores<T>()
                .AddDefaultTokenProviders()
                .AddSignInManager();

            services.Configure<IdentityOptions>(options =>
            {
                // Default Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
            });

            services.AddHttpContextAccessor();
            services.AddTransient<IGPGService, GPGService>();
            services.AddTransient<IGpgAuthenticatorService, GpgAuthenticatorService>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    "GPGOnly", 
                    policy => policy.RequireClaim("HasValidGPGPublicKey")
                );
            });

            // Add services to the container.
            services.AddRazorPages();

            return services;
        }
    }
}
