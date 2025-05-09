using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RencardUserClient.Configurations;
using RencardUserClient.Interfaces;
using RencardUserClient.Services;

namespace RencardUserClient.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration config)
        {
            // 1) Проверяем, что секции конфигурации есть
            var authSection = config.GetSection("Authentication");
            if (!authSection.Exists())
                throw new InvalidOperationException("Configuration section 'Authentication' is missing.");
            var jwtSection = config.GetSection("Jwt");
            if (!jwtSection.Exists())
                throw new InvalidOperationException("Configuration section 'Jwt' is missing.");

            // 2) Привязываем и валидируем типы
            var authOpts = authSection.Get<AuthOptions>();
            if (authOpts is null)
                throw new InvalidOperationException("Unable to bind 'Authentication' settings.");
            var jwtOpts = jwtSection.Get<JwtOptions>();
            if (jwtOpts is null)
                throw new InvalidOperationException("Unable to bind 'Jwt' settings.");

            // 3) Регистрируем сами опции для IOptions<T>
            services.Configure<AuthOptions>(authSection);
            services.Configure<JwtOptions>(jwtSection);

            // 4) Настраиваем схему аутентификации
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opts =>
            {
                opts.ExpireTimeSpan = authOpts.CookieLifetime;
                opts.SlidingExpiration = true;
                opts.LoginPath = "/api/auth/login";
            })
            .AddJwtBearer(opts =>
            {
                opts.RequireHttpsMetadata = true;
                opts.SaveToken = true;
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = jwtOpts.Issuer,
                    ValidAudience = jwtOpts.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpts.Secret))
                };
            });

            // 5) Регистрируем сервисы
            services.AddHttpContextAccessor();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}
