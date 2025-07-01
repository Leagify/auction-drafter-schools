using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Leagify.AuctionDrafter.Shared.Dtos;
using Leagify.AuctionDrafter.Server.Data; // For ApplicationUser
using System.Threading.Tasks;
using System.Linq; // Required for Select in GetCurrentUser
using System.Security.Claims; // Required for ClaimTypes

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

        // Initiates the external login flow (e.g., redirect to Google)
        // The 'provider' will be "Google"
        [HttpGet("signin/{provider}")]
        [AllowAnonymous] // This endpoint itself doesn't require auth, it starts the auth flow
        public IActionResult SignIn(string provider, string? returnUrl = null)
        {
            _logger.LogInformation("Attempting to sign in with provider: {Provider}, returnUrl: {ReturnUrl}", provider, returnUrl);
            // Request a redirect to the external login provider.
            // The redirect URL will be this controller's ExternalLoginCallback action.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        // Handles the callback from the external login provider (e.g., Google)
        [HttpGet("externallogincallback")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/"); // Default to home page
            if (remoteError != null)
            {
                _logger.LogError("Error from external provider: {RemoteError}", remoteError);
                // Redirect to a login failure page or display error on login page
                // For Blazor WASM, redirecting back to a client route that can show error is good.
                return Redirect($"/account/login?message=Error from external provider: {remoteError}");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogWarning("Error loading external login information.");
                return Redirect($"/account/login?message=Error loading external login information.");
            }

            _logger.LogInformation("External login info received for {LoginProvider}, ProviderKey: {ProviderKey}", info.LoginProvider, info.ProviderKey);

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserEmail} logged in with {LoginProvider} provider.", info.Principal.FindFirstValue(ClaimTypes.Email), info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return Redirect("/account/lockout"); // Placeholder, create this page if needed
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                _logger.LogInformation("User does not have a local account. Attempting to create one for {UserEmail} from {LoginProvider}.", info.Principal.FindFirstValue(ClaimTypes.Email), info.LoginProvider);

                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (email == null)
                {
                    _logger.LogError("Email claim not received from external provider {LoginProvider}.", info.LoginProvider);
                    // You might redirect to a page where they can manually enter an email if this happens
                    return Redirect($"/account/login?message=Email claim not received from {info.LoginProvider}.");
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true }; // Assume email is confirmed by Google
                    var createUserResult = await _userManager.CreateAsync(user);
                    if (!createUserResult.Succeeded)
                    {
                        var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                        _logger.LogError("Error creating new user {UserEmail}: {Errors}", email, errors);
                        return Redirect($"/account/login?message=Error creating user: {errors}");
                    }
                    _logger.LogInformation("New user {UserEmail} created.", email);
                }
                else
                {
                     _logger.LogInformation("User {UserEmail} already exists locally. Linking external login.", email);
                }

                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (!addLoginResult.Succeeded)
                {
                    var errors = string.Join(", ", addLoginResult.Errors.Select(e => e.Description));
                    _logger.LogError("Error adding external login for user {UserEmail}: {Errors}", email, errors);
                    return Redirect($"/account/login?message=Error linking external login: {errors}");
                }
                _logger.LogInformation("External login linked for user {UserEmail}.", email);

                // Re-attempt sign-in now that the user and external login are linked
                await _signInManager.SignInAsync(user, isPersistent: true); // Sign in the newly created/linked user
                 _logger.LogInformation("User {UserEmail} signed in after creating/linking external login.", email);

                return LocalRedirect(returnUrl);
            }
        }
    }
}
