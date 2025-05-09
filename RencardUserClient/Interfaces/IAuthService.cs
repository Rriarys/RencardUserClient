using RencardUserClient.DTOs.Auth;

namespace RencardUserClient.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, TokenResponse? Token, IEnumerable<string> Errors)> RegisterAsync(RegisterRequest dto);
        Task<(bool Success, TokenResponse? Token, IEnumerable<string> Errors)> LoginAsync(LoginRequest dto);
        Task LogoutAsync();
        Task<(bool Success, TokenResponse? Token, IEnumerable<string> Errors)> RefreshTokenAsync(RefreshTokenRequest dto);
        Task<(bool Success, IEnumerable<string> Errors)> ChangePasswordAsync(ChangePasswordRequest dto);
    }
}
