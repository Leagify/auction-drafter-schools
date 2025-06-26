using System;

namespace Leagify.AuctionDrafter.Shared.Models
{
    // Represents a school that has been successfully drafted by a team
    public class DraftPick
    {
        public int Id { get; set; }

        public int TeamId { get; set; }
        public virtual Team? Team { get; set; }

        public int SchoolId { get; set; }
        public virtual School? School { get; set; }

        public decimal AuctionCost { get; set; }

        // The specific roster slot this pick fills.
        // This links back to the RosterSlotDefinition to ensure it's a valid placement.
        public int RosterSlotDefinitionId { get; set; }
        public virtual RosterSlotDefinition? RosterSlotDefinition { get; set; }

        public DateTime TimeDrafted { get; set; }

        public DraftPick()
        {
            TimeDrafted = DateTime.UtcNow;
        }
    }
}
