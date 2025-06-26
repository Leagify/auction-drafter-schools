using System;
using System.Collections.Generic;

namespace Leagify.AuctionDrafter.Shared.Models
{
    public class User
    {
        public int Id { get; set; } // Or string if using ASP.NET Core Identity's default
        public string? UserName { get; set; } // For login
        public string? Email { get; set; }
        public string? DisplayName { get; set; } // For showing in UI

        // For ASP.NET Core Identity, password hash etc. would be managed by IdentityUser
        // public string PasswordHash { get; set; }

        // Navigation properties
        // A user can have multiple roles in different auctions, or system-wide roles
        // This might get more complex depending on how ASP.NET Core Identity is integrated.
        // For now, a simple list of roles.
        public virtual ICollection<Role>? Roles { get; set; }

        // A user can be a coach in multiple teams (across different auctions)
        public virtual ICollection<Team>? TeamsCoached { get; set; }

        public User()
        {
            Roles = new HashSet<Role>();
            TeamsCoached = new HashSet<Team>();
        }
    }
}
