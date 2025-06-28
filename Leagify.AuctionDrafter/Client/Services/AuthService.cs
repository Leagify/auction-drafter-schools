using Leagify.AuctionDrafter.Shared.Dtos;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization; // Required for AuthenticationStateProvider
using Microsoft.Extensions.Logging; // Added for ILogger

namespace Leagify.AuctionDrafter.Client.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ILogger<AuthService> _logger;

        public AuthService(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider, ILogger<AuthService> logger)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerModel)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/register", registerModel);
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            // Log or handle null authResponse if necessary
            if (authResponse == null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Failed to process registration response." };
            }
            return authResponse;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginModel)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/login", loginModel);
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            if (authResponse == null)
            {
                 return new AuthResponseDto { IsSuccess = false, Message = "Failed to process login response." };
            }

            if (authResponse.IsSuccess && authResponse.UserDetails != null)
            {
                // Notify the AuthenticationStateProvider.
                // The actual implementation of how MarkUserAsAuthenticated works will be in PersistentAuthenticationStateProvider
                if (_authenticationStateProvider is PersistentAuthenticationStateProvider customAuthStateProvider)
                {
                    customAuthStateProvider.MarkUserAsAuthenticated(authResponse.UserDetails); // Removed await
                }
            }
            return authResponse;
        }

        public async Task<AuthResponseDto> LogoutAsync()
        {
            _logger.LogInformation("AuthService.LogoutAsync: Method entered, preparing to call API.");
            Console.WriteLine("AuthService.LogoutAsync: Method entered, preparing to call API.");
            try
            {
                Console.WriteLine("AuthService.LogoutAsync: Attempting to GET from api/account/logout.");
                var serverResponse = await _httpClient.GetAsync("api/account/logout");

                Console.WriteLine($"AuthService.LogoutAsync: Logout API (GET) response status: {serverResponse.StatusCode}");

                string rawResponse = await serverResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"AuthService.LogoutAsync: Logout API raw response: {rawResponse}");

                if (serverResponse.IsSuccessStatusCode)
                {
                    AuthResponseDto? authResponse = null;
                    try
                    {
                        // Ensure System.Text.Json.JsonSerializerOptions if needed, but default should work for simple DTOs
                        authResponse = System.Text.Json.JsonSerializer.Deserialize<AuthResponseDto>(rawResponse, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                    catch (System.Text.Json.JsonException jsonEx)
                    {
                        Console.WriteLine($"AuthService.LogoutAsync: Logout API JSON deserialization error: {jsonEx.Message}");
                        return new AuthResponseDto { IsSuccess = false, Message = "Failed to parse logout server response." };
                    }

                    if (authResponse == null)
                    {
                        Console.WriteLine("AuthService.LogoutAsync: Deserialized authResponse is null.");
                        return new AuthResponseDto { IsSuccess = false, Message = "Failed to process logout response (null after deserialize)." };
                    }

                    if (_authenticationStateProvider is PersistentAuthenticationStateProvider customAuthStateProvider)
                    {
                        customAuthStateProvider.MarkUserAsLoggedOut();
                    }
                    Console.WriteLine("AuthService.LogoutAsync: Successfully processed logout and notified auth provider.");
                    return authResponse;
                }
                else
                {
                    Console.WriteLine($"AuthService.LogoutAsync: Logout API returned non-success status: {serverResponse.StatusCode}. Response: {rawResponse}");
                    return new AuthResponseDto { IsSuccess = false, Message = $"Logout failed on server: {serverResponse.StatusCode}" };
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "AuthService.LogoutAsync: HttpRequestException encountered. Message: {ExceptionMessage}, InnerException: {InnerExceptionMessage}", httpEx.Message, httpEx.InnerException?.Message);
                Console.WriteLine($"AuthService.LogoutAsync: HttpRequestException: {httpEx.Message} - Inner: {httpEx.InnerException?.Message}"); // Keep for direct console visibility too
                return new AuthResponseDto { IsSuccess = false, Message = "Network error during logout." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuthService.LogoutAsync: General exception encountered.");
                Console.WriteLine($"AuthService.LogoutAsync: General exception: {ex.ToString()}"); // Log full exception details
                return new AuthResponseDto { IsSuccess = false, Message = "An unexpected error occurred during logout." };
            }
        }

        public async Task<UserDetailsDto?> GetCurrentUserAsync()
        {
            // This method might be more useful within PersistentAuthenticationStateProvider itself
            // but can be exposed via AuthService if needed elsewhere.
            var response = await _httpClient.GetAsync("api/account/currentuser");
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                return authResponse?.UserDetails;
            }
            return null;
        }
    }

    // Interface for the AuthService
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerModel);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto loginModel);
        Task<AuthResponseDto> LogoutAsync();
        Task<UserDetailsDto?> GetCurrentUserAsync();
    }
}
