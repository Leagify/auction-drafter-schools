namespace Leagify.AuctionDrafter.Shared.Dtos
{
    public class UserDetailsDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        // Add other details you might want to send to the client about the user
        // For example, roles, display name, etc.
    }
}
