using System;
using System.Collections.Generic;

namespace Leagify.AuctionDrafter.Shared.Models
{
    public class Role
    {
        public int Id { get; set; } // Or string if using ASP.NET Core Identity's default
        public string? Name { get; set; } // e.g., "AuctionMaster", "TeamCoach", "ProxyCoach", "AuctionViewer"

        // Navigation properties
        public virtual ICollection<User>? Users { get; set; }

        public Role()
        {
            Users = new HashSet<User>();
        }

        // Static role names for consistency (optional, but can be helpful)
        public const string AuctionMaster = "AuctionMaster";
        public const string TeamCoach = "TeamCoach";
        public const string ProxyCoach = "ProxyCoach";
        public const string AuctionViewer = "AuctionViewer";
    }
}
