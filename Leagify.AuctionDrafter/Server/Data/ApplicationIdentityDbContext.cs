using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Leagify.AuctionDrafter.Server.Data
{
    // DbContext for ASP.NET Core Identity
    // It will use the ApplicationUser class we just defined.
    // If using custom roles, you would specify IdentityRole or a derivative here too.
    // For example: IdentityDbContext<ApplicationUser, ApplicationRole, string>
    public class ApplicationIdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationIdentityDbContext(DbContextOptions<ApplicationIdentityDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
