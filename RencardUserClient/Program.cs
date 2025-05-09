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

// ����������� DbContext � PostGIS
builder.Services.AddDbContext<RencardUserDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npg => npg.UseNetTopologySuite()
    );
});

// ���������� Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<RencardUserDbContext>()
    .AddDefaultTokenProviders();

// ���������� ����� �������������� (Cookie + JWT, ���� �����)
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

// ��������
app.MapControllers();

// ���������� Identity API (register/login/refresh/reset-password � �.�.)
app.MapIdentityApi<User>();

// ���� ��������� Identity-��������� (����������� � ���. ������, �������, ������)
app.MapIdentityEndpoints();

app.Run();
