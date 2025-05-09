using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RencardUserClient.Database;
using RencardUserClient.Models.Identity;
using RencardUserClient.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрация DbContext с PostGIS
builder.Services.AddDbContext<RencardUserDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npg => npg.UseNetTopologySuite()
    );
});

// Добавление Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<RencardUserDbContext>()
    .AddDefaultTokenProviders();

// Подключаем схему аутентификации (Cookie + JWT, если нужно)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
    .AddCookie(IdentityConstants.ApplicationScheme)
    .AddBearerToken(IdentityConstants.BearerScheme);

builder.Services.AddAuthorization();

var app = builder.Build();

// Pipeline
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

// Встроенные Identity API (register/login/refresh/reset-password и т.п.)
app.MapIdentityApi<User>();

// Ваши кастомные Identity-эндпоинты (регистрация с доп. полями, профиль, логаут)
app.MapIdentityEndpoints();

app.Run();
