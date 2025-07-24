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
        Task AssignRoleAsync(int auctionId, int userId, Role role);
        // Add more methods as needed: UpdateAuction, AddTeamToAuction, etc.
    }

    public class AuctionService : IAuctionService
    {
        private readonly ICsvParsingService _csvParsingService;
        private readonly ILogger<AuctionService> _logger;
        private readonly IHubContext<AuctionHub> _hubContext;
        // In-memory store for now
        private readonly List<Auction> _auctions = new List<Auction>();
        private static int _nextAuctionId = 1;

        public AuctionService(ICsvParsingService csvParsingService, ILogger<AuctionService> logger, IHubContext<AuctionHub> hubContext)
        {
            _csvParsingService = csvParsingService;
            _logger = logger;
            _hubContext = hubContext;
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
                SchoolsAvailable = new List<School>(),
                JoinCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()
            };

            if (schoolDataCsvStream != null)
            {
                _logger.LogInformation("Processing school data CSV for auction {AuctionName}.", auctionName);
                var schools = await _csvParsingService.ParseSchoolsFromCsvAsync(schoolDataCsvStream);
                _logger.LogInformation("Parsed {SchoolCount} schools from CSV for auction {AuctionName}.", schools.Count, auctionName);

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
                _logger.LogInformation("Assigned {SchoolCount} schools to auction {AuctionName}.", auction.SchoolsAvailable.Count, auctionName);
            }
            else
            {
                _logger.LogInformation("No school data CSV provided for auction {AuctionName}.", auctionName);
            }

            _auctions.Add(auction);
            _logger.LogInformation("Auction {AuctionName} created with ID {AuctionId} and {SchoolCount} schools.", auction.Name, auction.Id, auction.SchoolsAvailable.Count);
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

        public async Task AssignRoleAsync(int auctionId, int userId, Role role)
        {
            var auction = _auctions.FirstOrDefault(a => a.Id == auctionId);
            if (auction != null)
            {
                var user = auction.Participants.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    user.Role = role;
                    await _hubContext.Clients.Group(auctionId.ToString()).SendAsync("RoleAssigned", userId, role);
                }
            }
        }
    }
}
