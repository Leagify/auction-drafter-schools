using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using Leagify.AuctionDrafter.Shared.Dtos; // For UserDetailsDto and AuthResponseDto
using System.Collections.Generic; // For List used in ClaimsIdentity

namespace Leagify.AuctionDrafter.Client.Services
{
    public class PersistentAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private static ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public PersistentAuthenticationStateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Call the server to get current user info.
                // This relies on the auth cookie being sent automatically by the browser.
                var response = await _httpClient.GetAsync("api/account/currentuser");

                if (response.IsSuccessStatusCode)
                {
                    var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                    if (authResponse != null && authResponse.IsSuccess && authResponse.UserDetails != null)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, authResponse.UserDetails.UserId),
                            new Claim(ClaimTypes.Email, authResponse.UserDetails.Email),
                            new Claim(ClaimTypes.Name, authResponse.UserDetails.Email) // Often email is used as Name claim
                            // Add other claims like roles if they are part of UserDetailsDto and needed
                        };
                        var identity = new ClaimsIdentity(claims, "serverauth");
                        return new AuthenticationState(new ClaimsPrincipal(identity));
                    }
                }
            }
            catch
            {
                // Handle exceptions (e.g., network error, API down) by returning anonymous state
                // This could be logged
            }

            return new AuthenticationState(_anonymous); // Not authenticated
        }

        public void MarkUserAsAuthenticated(UserDetailsDto userDetails) // Changed to void
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userDetails.UserId),
                new Claim(ClaimTypes.Email, userDetails.Email),
                new Claim(ClaimTypes.Name, userDetails.Email)
                // Add roles here if available in userDetails
            };
            var identity = new ClaimsIdentity(claims, "serverauth"); // Use the same authentication type
            var principal = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        }

        public void MarkUserAsLoggedOut() // Changed to void
        {
            // The server-side logout (api/account/logout) clears the cookie.
            // This method ensures the Blazor client-side state is updated.
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }
    }
}
