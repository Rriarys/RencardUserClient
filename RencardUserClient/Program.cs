using Microsoft.EntityFrameworkCore;
using RencardUserClient.Database;
using NetTopologySuite;

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

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
