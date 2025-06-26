// Removed: using Microsoft.AspNetCore.Http;

namespace Leagify.AuctionDrafter.Shared.Dtos
{
    public class CreateAuctionDto
    {
        public string AuctionName { get; set; } = "New Auction";

        // IFormFile property removed.
        // The actual file will be passed as a separate IFormFile parameter
        // in the controller action, or handled via properties like FileName/FileContent
        // if a different mechanism is chosen. For now, controller will take IFormFile separately.
    }
}
