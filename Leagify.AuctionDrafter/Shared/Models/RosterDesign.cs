using System;
using System.Collections.Generic;

namespace Leagify.AuctionDrafter.Shared.Models
{
    // Defines the structure of a roster for a specific auction
    public class RosterDesign
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Default Roster"; // e.g., "Standard 10-slot Roster"

        public int AuctionId { get; set; } // Each auction has one roster design
        public virtual Auction? Auction { get; set; }

        public virtual ICollection<RosterSlotDefinition>? SlotDefinitions { get; set; }

        public RosterDesign()
        {
            SlotDefinitions = new HashSet<RosterSlotDefinition>();
        }
    }

    // Defines a single slot in the roster design
    public class RosterSlotDefinition
    {
        public int Id { get; set; }
        public string PositionName { get; set; } = string.Empty; // e.g., "SEC", "Big Ten", "Flex"
                                                                 // This should match LeagifyPosition values from School data or be "Flex"

        public int RosterDesignId { get; set; }
        public virtual RosterDesign? RosterDesign { get; set; }

        // Could add display order, color coding here if needed
        public int DisplayOrder { get; set; }
        public string? ColorCode {get; set; } // Hex color code, e.g. #FF0000
    }
}
