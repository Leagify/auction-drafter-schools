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
        // AuctionName comes from the DTO, SchoolDataCsv comes as a separate IFormFile parameter
        // And user authentication to get auctionMasterUserId
        [HttpPost("create")]
        public async Task<IActionResult> CreateAuction([FromForm] CreateAuctionDto auctionDetails, IFormFile? schoolDataCsvFile)
        {
            if (auctionDetails == null)
            {
                return BadRequest("Auction details are null.");
            }

            // schoolDataCsvFile is now a direct parameter, can be null if no file uploaded.
            // The client UI should ideally enforce that a file is selected if it's mandatory.
            // For now, service handles null stream.

            Stream? csvStream = null;
            if (schoolDataCsvFile != null)
            {
                if (schoolDataCsvFile.Length == 0)
                {
                    return BadRequest("CSV file is empty.");
                }
                csvStream = schoolDataCsvFile.OpenReadStream();
            }

            // TODO: Get auctionMasterUserId from authenticated user context later
            var auctionMasterUserId = 1; // Placeholder

            try
            {
                var auction = await _auctionService.CreateAuctionAsync(
                    auctionDetails.AuctionName ?? "Unnamed Auction",
                    auctionMasterUserId,
                    csvStream);

                if (csvStream != null)
                {
                    await csvStream.DisposeAsync();
                }

                // Return a more detailed Auction DTO if needed
                return CreatedAtAction(nameof(GetAuction), new { auctionId = auction.Id }, auction);
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

        // POST: api/auction/{auctionId}/assignrole
        [HttpPost("{auctionId}/assignrole")]
        public async Task<IActionResult> AssignRole(int auctionId, [FromBody] AssignRoleDto assignRoleDto)
        {
            if (assignRoleDto == null)
            {
                return BadRequest("Assign role details are null.");
            }

            if (!Enum.IsDefined(typeof(Role), assignRoleDto.Role))
            {
                return BadRequest("Invalid role specified.");
            }

            try
            {
                await _auctionService.AssignRoleAsync(auctionId, assignRoleDto.UserId, assignRoleDto.Role);
                return Ok();
            }
            catch (System.Exception ex)
            {
                // Log the exception ex
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error assigning role: {ex.Message}");
            }
        }
    }
}
