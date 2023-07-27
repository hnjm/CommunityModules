using Community.UserAccount;
using Community.UserAccount.UI;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CommunityContextConnection");

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddCommunityUserSupport<CommunityContext>(
    options =>
    {
        options.UseSqlite(
            connectionString, 
            b => b.MigrationsAssembly("SampleCommunity.Migrations")
        );
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

