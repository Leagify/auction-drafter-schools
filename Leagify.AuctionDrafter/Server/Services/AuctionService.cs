using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Leagify.AuctionDrafter.Shared.Models;

namespace Leagify.AuctionDrafter.Server.Services
{
    public interface IAuctionService
    {
        Task<Auction> CreateAuctionAsync(string auctionName, int auctionMasterUserId, Stream? schoolDataCsvStream);
        Task<Auction?> GetAuctionByIdAsync(int auctionId);
        Task<List<School>> GetSchoolsForAuctionAsync(int auctionId);
        // Add more methods as needed: UpdateAuction, AddTeamToAuction, etc.
    }

    public class AuctionService : IAuctionService
    {
        private readonly ICsvParsingService _csvParsingService;
        // In-memory store for now
        private readonly List<Auction> _auctions = new List<Auction>();
        private static int _nextAuctionId = 1;

        public AuctionService(ICsvParsingService csvParsingService)
        {
            _csvParsingService = csvParsingService;
        }

        public async Task<Auction> CreateAuctionAsync(string auctionName, int auctionMasterUserId, Stream? schoolDataCsvStream)
        {
            var auction = new Auction
            {
                Id = _nextAuctionId++,
                Name = auctionName,
                AuctionMasterUserId = auctionMasterUserId, // Assuming User ID 1 is a valid master for now
                Status = AuctionStatus.NotStarted,
                CreatedDate = DateTime.UtcNow,
                SchoolsAvailable = new List<School>()
            };

            if (schoolDataCsvStream != null)
            {
                var schools = await _csvParsingService.ParseSchoolsFromCsvAsync(schoolDataCsvStream);
                // In a real DB scenario, these schools would be linked to this auction.
                // For in-memory, we can add them to the auction's collection.
                // We might also want to assign new IDs to these schools if they are specific to this auction instance.
                int schoolIdCounter = 1;
                foreach (var school in schools)
                {
                    school.Id = schoolIdCounter++; // Assign a temporary ID for this auction context
                    // school.AuctionId = auction.Id; // If School model had AuctionId
                }
                auction.SchoolsAvailable = schools;
            }

            _auctions.Add(auction);
            return auction;
        }

        public Task<Auction?> GetAuctionByIdAsync(int auctionId)
        {
            var auction = _auctions.FirstOrDefault(a => a.Id == auctionId);
            return Task.FromResult(auction);
        }

        public Task<List<School>> GetSchoolsForAuctionAsync(int auctionId)
        {
            var auction = _auctions.FirstOrDefault(a => a.Id == auctionId);
            if (auction != null && auction.SchoolsAvailable != null)
            {
                return Task.FromResult(auction.SchoolsAvailable.ToList());
            }
            return Task.FromResult(new List<School>());
        }
    }
}
