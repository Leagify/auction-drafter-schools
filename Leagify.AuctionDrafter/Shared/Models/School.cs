using System;

namespace Leagify.AuctionDrafter.Shared.Models
{
    public class School
    {
        public int Id { get; set; } // Primary Key for DB
        public string? Name { get; set; }
        public string? Conference { get; set; }
        public double? ProjectedPoints { get; set; }
        public int? NumberOfProspects { get; set; }
        public string? SchoolURL { get; set; }
        public double? SuggestedAuctionValue { get; set; }
        public string? LeagifyPosition { get; set; }
        public double? ProjectedPointsAboveAverage { get; set; }
        public double? ProjectedPointsAboveReplacement { get; set; }
        public double? AveragePointsForPosition { get; set; }
        public double? ReplacementValueAverageForPosition { get; set; }

        // Navigation properties (if needed later, e.g., for which Auction this school data belongs to)
        // public int AuctionId { get; set; }
        // public virtual Auction Auction { get; set; }
    }
}
