namespace RencardUserClient.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(string userId, IDictionary<string, string>? claims = null);
        string GenerateRefreshToken();
    }
}
