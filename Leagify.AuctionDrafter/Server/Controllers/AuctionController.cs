using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Leagify.AuctionDrafter.Server.Services;
using Leagify.AuctionDrafter.Shared.Models; // For Auction model
using Leagify.AuctionDrafter.Shared.Dtos; // For DTOs

namespace Leagify.AuctionDrafter.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionService _auctionService;

        public AuctionController(IAuctionService auctionService)
        {
            _auctionService = auctionService;
        }

        // POST: api/auction/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateAuction([FromForm] CreateAuctionDto auctionDetails, IFormFile? schoolDataCsvFile)
        {
            if (auctionDetails == null)
            {
                return BadRequest("Auction details are null.");
            }

            Stream? csvStream = null;
            if (schoolDataCsvFile != null)
            {
                if (schoolDataCsvFile.Length == 0)
                {
                    return BadRequest("CSV file is empty.");
                }
                csvStream = schoolDataCsvFile.OpenReadStream();
            }

            try
            {
                var responseDto = await _auctionService.CreateAuctionAsync(
                    auctionDetails.AuctionName ?? "Unnamed Auction",
                    csvStream);

                if (csvStream != null)
                {
                    await csvStream.DisposeAsync();
                }

                // Return the response DTO which includes the MasterToken for the creator
                return CreatedAtAction(nameof(GetAuction), new { auctionId = responseDto.AuctionId }, responseDto);
            }
            catch (System.Exception ex)
            {
                 if (csvStream != null)
                {
                    await csvStream.DisposeAsync();
                }
                // Log the exception ex
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error creating auction: {ex.Message}");
            }
        }

        // GET: api/auction/{auctionId}
        [HttpGet("{auctionId}")]
        public async Task<IActionResult> GetAuction(int auctionId)
        {
            var auction = await _auctionService.GetAuctionByIdAsync(auctionId);
            if (auction == null)
            {
                return NotFound();
            }
            return Ok(auction); // Consider returning a DTO
        }

        // GET: api/auction/{auctionId}/schools
        [HttpGet("{auctionId}/schools")]
        public async Task<IActionResult> GetAuctionSchools(int auctionId)
        {
            var schools = await _auctionService.GetSchoolsForAuctionAsync(auctionId);
            if (schools == null || !schools.Any())
            {
                // This isn't necessarily a NotFound for the auction itself,
                // but could mean no schools are loaded or the auction doesn't exist.
                // For now, returning Ok with empty list is fine if auction exists but has no schools.
                var auction = await _auctionService.GetAuctionByIdAsync(auctionId);
                if (auction == null) return NotFound($"Auction with ID {auctionId} not found.");
            }
            return Ok(schools);
        }
    }
}
