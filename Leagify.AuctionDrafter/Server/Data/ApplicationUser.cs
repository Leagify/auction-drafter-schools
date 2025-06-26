using Microsoft.AspNetCore.Identity;

namespace Leagify.AuctionDrafter.Server.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    // This class will be used by ASP.NET Core Identity
    public class ApplicationUser : IdentityUser // IdentityUser already has Id, UserName, Email, PasswordHash etc.
    {
        // You can add custom properties here if needed, for example:
        public string? DisplayName { get; set; }
        // public DateOnly? DateOfBirth { get; set; }
    }
}
