namespace Leagify.AuctionDrafter.Shared.Dtos
{
    public class CreateAuctionResponseDto
    {
        public int AuctionId { get; set; }
        public string? AuctionName { get; set; }
        public string JoinCode { get; set; } = string.Empty;
        public string MasterToken { get; set; } = string.Empty; // This is the un-hashed token
    }
}
