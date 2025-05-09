using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RencardUserClient.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using RencardUserClient.Configurations;

namespace RencardUserClient.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtOptions _opts;

        public TokenService(IOptions<JwtOptions> opts)
        {
            _opts = opts.Value;
        }

        public string GenerateAccessToken(string userId, IDictionary<string, string>? claims = null)
        {
            var claimsList = new List<Claim> { new("sub", userId) };
            if (claims != null)
                foreach (var kv in claims)
                    claimsList.Add(new Claim(kv.Key, kv.Value));

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.Secret)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _opts.Issuer,
                audience: _opts.Audience,
                claims: claimsList,
                expires: DateTime.UtcNow.Add(_opts.Lifetime),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
