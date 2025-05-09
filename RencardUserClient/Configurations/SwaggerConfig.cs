using Microsoft.OpenApi.Models;

namespace RencardUserClient.Configurations
{
    public static class SwaggerConfig
    {
        public static IServiceCollection AddSwaggerAuth(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("cookieAuth", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    Name = ".AspNetCore.Cookies",
                    In = ParameterLocation.Cookie,
                    Description = "Use cookies for authentication"
                });
                c.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Use JWT token"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme { Reference = new OpenApiReference {
                            Type = ReferenceType.SecurityScheme, Id = "cookieAuth" } },
                        new string[] {}
                    },
                    {
                        new OpenApiSecurityScheme { Reference = new OpenApiReference {
                            Type = ReferenceType.SecurityScheme, Id = "bearerAuth" } },
                        new string[] {}
                    }
                });
            });
            return services;
        }
    }
}
