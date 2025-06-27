namespace Leagify.AuctionDrafter.Shared.Dtos
{
    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserDetailsDto? UserDetails { get; set; }
        // If using JWT tokens, you would add a string? Token property here.
        // For cookie-based auth, the cookie is handled by the browser/http client automatically.
    }
}
