using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Leagify.AuctionDrafter.Shared.Dtos;
using Leagify.AuctionDrafter.Server.Data; // For ApplicationUser
using System.Threading.Tasks;
using System.Linq; // Required for Select in GetCurrentUser

namespace Leagify.AuctionDrafter.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "Invalid payload.", UserDetails = null });
            }

            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "Passwords do not match.", UserDetails = null });
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "User with this email already exists.", UserDetails = null });
            }

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserEmail} created successfully.", model.Email);
                // You might want to sign in the user immediately after registration
                // await _signInManager.SignInAsync(user, isPersistent: false);
                // Or return user details for the client to decide next steps

                // For now, let's assign a default role if needed or just confirm registration
                // Example: await _userManager.AddToRoleAsync(user, "AuctionViewer");


                return Ok(new AuthResponseDto {
                    IsSuccess = true,
                    Message = "User registered successfully.",
                    UserDetails = new UserDetailsDto { UserId = user.Id, Email = user.Email }
                });
            }

            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new AuthResponseDto { IsSuccess = false, Message = string.Join(", ", errors), UserDetails = null });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "Invalid payload.", UserDetails = null });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                 _logger.LogWarning("Login attempt for non-existent user {UserEmail}.", model.Email);
                return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "Invalid email or password.", UserDetails = null });
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: true, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserEmail} logged in successfully.", model.Email);
                return Ok(new AuthResponseDto {
                    IsSuccess = true,
                    Message = "Login successful.",
                    UserDetails = new UserDetailsDto { UserId = user.Id, Email = user.Email }
                });
            }

            // Handle other cases like lockout, 2FA if configured
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {UserEmail} account locked out.", model.Email);
                return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "Account locked out.", UserDetails = null });
            }
            else
            {
                _logger.LogWarning("Invalid login attempt for user {UserEmail}.", model.Email);
                return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "Invalid email or password.", UserDetails = null });
            }
        }

        [Authorize] // Requires user to be authenticated
        [HttpGet("logout")] // Changed from HttpPost to HttpGet for diagnostics
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out successfully (via GET).");
            // For Blazor WASM, the client also needs to clear its state.
            // This endpoint mainly clears the server-side auth cookie.
            return Ok(new AuthResponseDto { IsSuccess = true, Message = "Logout successful (via GET)." });
        }

        [Authorize] // Requires user to be authenticated
        [HttpGet("currentuser")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User); // User comes from ControllerBase
            if (user == null)
            {
                // This case should ideally not be hit if [Authorize] is effective
                // and the auth cookie is correctly managed.
                _logger.LogWarning("GetCurrentUser called but user was not found, though endpoint is Authorized.");
                return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "User not authenticated."});
            }

            return Ok(new AuthResponseDto {
                IsSuccess = true,
                Message = "User details retrieved.",
                UserDetails = new UserDetailsDto { UserId = user.Id, Email = user.Email ?? "N/A" }
            });
        }
    }
}
