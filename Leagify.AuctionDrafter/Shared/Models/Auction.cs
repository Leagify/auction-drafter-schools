using System;
using System.Collections.Generic;

namespace Leagify.AuctionDrafter.Shared.Models
{
    public enum AuctionStatus
    {
        NotStarted,
        InProgress,
        Paused,
        Complete
    }

    public class Auction
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public AuctionStatus Status { get; set; }

        // Foreign Key for the Auction Master (User)
        public int AuctionMasterUserId { get; set; }
        public virtual User? AuctionMaster { get; set; }

        // All schools available in this auction (copied from a template or master list)
        public virtual ICollection<School>? SchoolsAvailable { get; set; }

        // Teams participating in this auction
        public virtual ICollection<Team>? Teams { get; set; }

        // Users participating in this auction
        public virtual ICollection<User>? Participants { get; set; }

        public string? JoinCode { get; set; }

        // Roster Design for this auction
        public virtual RosterDesign? RosterDesign { get; set; }
        public int? RosterDesignId { get; set; }


        // Current school up for bid
        public int? CurrentSchoolOnBlockId { get; set; }
        public virtual School? CurrentSchoolOnBlock { get; set; }
        public decimal CurrentBid { get; set; }
        public int? CurrentHighBidderTeamId { get; set; } // TeamId of the current high bidder
        // public virtual Team CurrentHighBidderTeam { get; set; } // Could also be User if proxies are complex

        public Auction()
        {
            CreatedDate = DateTime.UtcNow;
            Status = AuctionStatus.NotStarted;
            SchoolsAvailable = new HashSet<School>();
            Teams = new HashSet<Team>();
            Participants = new HashSet<User>();
        }
    }
}
