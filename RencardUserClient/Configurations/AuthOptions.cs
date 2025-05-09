namespace RencardUserClient.Configurations
{
    public class AuthOptions
    {
        public bool UseJwt { get; set; } = false;
        public TimeSpan CookieLifetime { get; set; } = TimeSpan.FromDays(7);
    }

    public class JwtOptions
    {
        public required string Secret { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public TimeSpan Lifetime { get; set; }
    }
}
