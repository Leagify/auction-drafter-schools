using System;
using System.Collections.Generic;

namespace Leagify.AuctionDrafter.Shared.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string? Name { get; set; } // e.g., "Jules's Team"

        public int AuctionId { get; set; }
        public virtual Auction? Auction { get; set; }

        // The User who is the primary coach for this team
        public int CoachUserId { get; set; }
        public virtual User? Coach { get; set; }

        // If a proxy is bidding, this could be the User ID of the proxy
        public int? ProxyBiddingUserId { get; set; }
        public virtual User? ProxyBiddingUser { get; set; }


        public decimal Budget { get; set; } = 200; // Default budget
        public decimal MoneySpent { get; set; } = 0;
        public decimal MoneyRemaining => Budget - MoneySpent;

        // Roster: Collection of DraftPicks or filled RosterSlots
        public virtual ICollection<DraftPick>? DraftPicks { get; set; }

        // Order in the draft nomination sequence
        public int NominationOrder { get; set; }


        public Team()
        {
            DraftPicks = new HashSet<DraftPick>();
        }
    }
}
