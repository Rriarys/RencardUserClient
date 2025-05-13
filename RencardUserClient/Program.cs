using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RencardUserClient.Database;
using RencardUserClient.Models.Identity;
using RencardUserClient.Extensions;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

// Сервисы
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext + PostGIS
builder.Services.AddDbContext<RencardUserDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        pg => pg.UseNetTopologySuite()
    )
);

// Identity (стандартные UserManager, SignInManager, Cookie-сессии и токены)
builder.Services.AddIdentityCore<User>()
    .AddEntityFrameworkStores<RencardUserDbContext>()
    .AddApiEndpoints();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.User.RequireUniqueEmail = true;
});

// Аутентификация: Cookie + Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddCookie(IdentityConstants.ApplicationScheme)
.AddBearerToken(IdentityConstants.BearerScheme);

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Маршруты
app.MapControllers();

// Кастомные эндпоинты из IdentityEndpointsExtensions.cs
app.MapIdentityEndpoints();

// Стандартные Identity-эндпоинты (Register/Login/Refresh/Reset и т.п.)
// эти методы (MapIdentityApi) доступны прямо после AddDefaultTokenProviders()
app.MapIdentityApi<User>();

app.Run();
