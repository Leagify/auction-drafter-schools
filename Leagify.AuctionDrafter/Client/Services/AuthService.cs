using Leagify.AuctionDrafter.Shared.Dtos;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization; // Required for AuthenticationStateProvider

namespace Leagify.AuctionDrafter.Client.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public AuthService(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
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
            // Call the server-side logout to clear the auth cookie
            var serverResponse = await _httpClient.PostAsync("api/account/logout", null);
            var authResponse = await serverResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

            if (authResponse == null)
            {
                 return new AuthResponseDto { IsSuccess = false, Message = "Failed to process logout response." };
            }

            // Notify the AuthenticationStateProvider for client-side state update
            if (_authenticationStateProvider is PersistentAuthenticationStateProvider customAuthStateProvider)
            {
                customAuthStateProvider.MarkUserAsLoggedOut(); // Removed await
            }
            return authResponse;
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
