using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RencardUserClient.DTOs.Auth;
using RencardUserClient.Interfaces;
using RencardUserClient.Models.Identity;
using RencardUserClient.Configurations;
using Microsoft.AspNetCore.Authentication;

namespace RencardUserClient.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly AuthOptions _authOptions;
        private readonly IUserService _userService;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ITokenService tokenService,
            IHttpContextAccessor httpContext,
            IOptions<AuthOptions> authOptions,
            IUserService userService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _httpContext = httpContext;
            _authOptions = authOptions.Value;
            _userService = userService;
        }

        public async Task<(bool Success, TokenResponse? Token, IEnumerable<string> Errors)> RegisterAsync(RegisterRequest dto)
        {
            if (dto.BirthDate > DateTime.UtcNow.AddYears(-18))
                return (false, null, new[] { "User must be at least 18 years old." });

            User user;
            try
            {
                user = await _userService.CreateAsync(
                    dto.Email, dto.Password, dto.PhoneNumber, dto.BirthDate, dto.Sex);
            }
            catch (InvalidOperationException ex)
            {
                return (false, null, ex.Message.Split(';'));
            }

            return await IssueTokensAsync(user, dto.UseJwt);
        }

        public async Task<(bool Success, TokenResponse? Token, IEnumerable<string> Errors)> LoginAsync(LoginRequest dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return (false, null, new[] { "Invalid credentials" });

            var res = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!res.Succeeded)
                return (false, null, new[] { "Invalid credentials" });

            return await IssueTokensAsync(user, dto.UseJwt);
        }

        public async Task LogoutAsync()
        {
            if (!_authOptions.UseJwt)
                await _signInManager.SignOutAsync();
            // при JWT – токен в AspNetUserTokens останется до истечения
        }

        public async Task<(bool Success, TokenResponse? Token, IEnumerable<string> Errors)> RefreshTokenAsync(RefreshTokenRequest dto)
        {
            var userId = dto.UserId;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, null, new[] { "User not found" });

            var stored = await _userManager.GetAuthenticationTokenAsync(
                user, "RencardRefresh", "refresh_token");
            if (stored != dto.RefreshToken)
                return (false, null, new[] { "Invalid refresh token" });

            return await IssueTokensAsync(user, _authOptions.UseJwt);
        }

        public async Task<(bool Success, IEnumerable<string> Errors)> ChangePasswordAsync(ChangePasswordRequest dto)
        {
            var userId = _httpContext.HttpContext?.User?.FindFirst("sub")?.Value;
            if (userId == null)
                return (false, new[] { "Unauthorized" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, new[] { "User not found" });

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            return (result.Succeeded, result.Errors.Select(e => e.Description));
        }

        private async Task<(bool Success, TokenResponse Token, IEnumerable<string> Errors)> IssueTokensAsync(User user, bool useJwt)
        {
            // Генерируем JWT через TokenService:
            var accessToken = _tokenService.GenerateAccessToken(user.Id);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Сохраняем refresh в AspNetUserTokens
            await _userManager.RemoveAuthenticationTokenAsync(user, "RencardRefresh", "refresh_token");
            await _userManager.SetAuthenticationTokenAsync(
                user,
                loginProvider: "RencardRefresh",
                tokenName: "refresh_token",
                tokenValue: refreshToken);

            if (!useJwt)
            {
                await _signInManager.SignInAsync(user, new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.Add(_authOptions.CookieLifetime),
                    AllowRefresh = true
                });
            }

            return (true, new TokenResponse { AccessToken = accessToken, RefreshToken = refreshToken }, Array.Empty<string>());
        }
    }
}
