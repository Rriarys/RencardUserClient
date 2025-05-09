using Microsoft.EntityFrameworkCore;
using RencardUserClient.Database;
using RencardUserClient.Configurations;
using RencardUserClient.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Регистрация DbContext с PostGIS
builder.Services.AddDbContext<RencardUserDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgOptions =>
        {
            npgOptions.UseNetTopologySuite();
        }
    );
});

// Auth & Swagger
builder.Services.AddAuthServices(builder.Configuration);
builder.Services.AddSwaggerAuth();

builder.Services.AddControllers();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
