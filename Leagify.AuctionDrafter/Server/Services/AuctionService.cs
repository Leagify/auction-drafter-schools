using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Leagify.AuctionDrafter.Shared.Models;
using Leagify.AuctionDrafter.Shared.Dtos;
using System.Security.Cryptography;
using System.Text;

namespace Leagify.AuctionDrafter.Server.Services
{
    public interface IAuctionService
    {
        Task<CreateAuctionResponseDto> CreateAuctionAsync(string auctionName, Stream? schoolDataCsvStream);
        Task<Auction?> GetAuctionByIdAsync(int auctionId);
        Task<List<School>> GetSchoolsForAuctionAsync(int auctionId);
        // Add more methods as needed: UpdateAuction, AddTeamToAuction, etc.
    }

    public class AuctionService : IAuctionService
    {
        private readonly ICsvParsingService _csvParsingService;
        private readonly ILogger<AuctionService> _logger;
        // In-memory store for now
        private readonly List<Auction> _auctions = new List<Auction>();
        private static int _nextAuctionId = 1;

        public AuctionService(ICsvParsingService csvParsingService, ILogger<AuctionService> logger)
        {
            _csvParsingService = csvParsingService;
            _logger = logger;
        }

        public async Task<CreateAuctionResponseDto> CreateAuctionAsync(string auctionName, Stream? schoolDataCsvStream)
        {
            var masterToken = Guid.NewGuid().ToString();
            var joinCode = GenerateJoinCode();
            var hashedMasterToken = HashToken(masterToken);

            var auction = new Auction
            {
                Id = _nextAuctionId++,
                Name = auctionName,
                JoinCode = joinCode,
                HashedMasterToken = hashedMasterToken,
                Status = AuctionStatus.NotStarted,
                CreatedDate = DateTime.UtcNow
            };

            if (schoolDataCsvStream != null)
            {
                _logger.LogInformation("Processing school data CSV for auction {AuctionName}.", auctionName);
                var schools = await _csvParsingService.ParseSchoolsFromCsvAsync(schoolDataCsvStream);
                _logger.LogInformation("Parsed {SchoolCount} schools from CSV for auction {AuctionName}.", schools.Count, auctionName);

                int schoolIdCounter = 1;
                foreach (var school in schools)
                {
                    school.Id = schoolIdCounter++; // Assign a temporary ID for this auction context
                }
                auction.SchoolsAvailable = schools;
                _logger.LogInformation("Assigned {SchoolCount} schools to auction {AuctionName}.", auction.SchoolsAvailable.Count, auctionName);
            }
            else
            {
                _logger.LogInformation("No school data CSV provided for auction {AuctionName}.", auctionName);
            }

            _auctions.Add(auction);
            _logger.LogInformation("Auction {AuctionName} created with ID {AuctionId}, JoinCode {JoinCode} and {SchoolCount} schools.", auction.Name, auction.Id, auction.JoinCode, auction.SchoolsAvailable.Count);

            return new CreateAuctionResponseDto
            {
                AuctionId = auction.Id,
                AuctionName = auction.Name,
                JoinCode = auction.JoinCode,
                MasterToken = masterToken // Return the un-hashed token to the creator
            };
        }

        private string GenerateJoinCode(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNPQRSTUVWXYZ123456789"; // Omitted O and 0 for clarity
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string HashToken(string token)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
                return Convert.ToBase64String(hashedBytes);
            }
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
