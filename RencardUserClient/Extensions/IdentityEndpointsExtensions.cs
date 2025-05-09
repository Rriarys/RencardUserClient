using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using RencardUserClient.Database;
using RencardUserClient.Models.About;
using RencardUserClient.Models.Identity;
using RencardUserClient.Models.Location;
using RencardUserClient.Models.Photos;
using RencardUserClient.Models.Preferences;
using System.ComponentModel.DataAnnotations;

namespace RencardUserClient.Extensions
{
    public static class IdentityEndpointsExtensions
    {
        public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPut("/user-phone-sex-age", async (
                UserManager<User> userManager,
                RencardUserDbContext db,
                HttpContext ctx,
                PhoneSexAgeRequest request) =>
            {
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(request, new ValidationContext(request), validationResults, true))
                {
                    return Results.BadRequest(validationResults.Select(e => e.ErrorMessage));
                }

                var user = await userManager.GetUserAsync(ctx.User);
                if (user == null) return Results.Unauthorized();

                if (await db.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber))
                    return Results.BadRequest("Phone number is already in use");

                user.PhoneNumber = request.PhoneNumber;
                user.Sex = request.Sex;
                user.BirthDate = request.BirthDate;

                var result = await userManager.UpdateAsync(user);
                return result.Succeeded
                    ? Results.Ok()
                    : Results.BadRequest(result.Errors);
            }).RequireAuthorization();

            endpoints.MapPost("/initialize-profile", async (
                UserManager<User> userManager,
                RencardUserDbContext db,
                HttpContext ctx) =>
            {
                var user = await userManager.GetUserAsync(ctx.User);
                if (user == null) return Results.Unauthorized();

                if (!await db.AboutUsers.AnyAsync(a => a.UserId == user.Id))
                    db.AboutUsers.Add(new AboutUser { UserId = user.Id });

                if (!await db.UserPreferences.AnyAsync(p => p.UserId == user.Id))
                    db.UserPreferences.Add(new UserPreferences { UserId = user.Id });

                if (!await db.UserLocations.AnyAsync(l => l.UserId == user.Id))
                    db.UserLocations.Add(new UserLocation
                    {
                        UserId = user.Id,
                        Geography = new Point(0, 0) { SRID = 4326 }
                    });

                if (!await db.UserPhotos.AnyAsync(p => p.UserId == user.Id))
                    db.UserPhotos.Add(new UserPhoto { UserId = user.Id });

                await db.SaveChangesAsync();
                return Results.Ok("Profile initialized");
            }).RequireAuthorization();

            endpoints.MapGet("/me", async (
                UserManager<User> userManager,
                RencardUserDbContext db,
                HttpContext ctx) =>
            {
                var user = await userManager.GetUserAsync(ctx.User);
                if (user == null) return Results.Unauthorized();

                var about = await db.AboutUsers.FirstOrDefaultAsync(a => a.UserId == user.Id);
                var location = await db.UserLocations.FirstOrDefaultAsync(l => l.UserId == user.Id);
                var preferences = await db.UserPreferences.FirstOrDefaultAsync(p => p.UserId == user.Id);

                return Results.Ok(new ProfileResponse(
                    About: about?.ToDto(),
                    Location: location?.ToDto(),
                    Preferences: preferences?.ToDto()
                ));
            }).RequireAuthorization();

            endpoints.MapPut("/me", async (
                UserManager<User> userManager,
                RencardUserDbContext db,
                HttpContext ctx,
                ProfileUpdateRequest request) =>
            {
                var user = await userManager.GetUserAsync(ctx.User);
                if (user == null) return Results.Unauthorized();

                // Обновление About
                var about = await db.AboutUsers.FirstOrDefaultAsync(a => a.UserId == user.Id)
                            ?? new AboutUser { UserId = user.Id };
                about.UpdateFrom(request.About);
                db.AboutUsers.Update(about);

                // Обновление Preferences
                var preferences = await db.UserPreferences.FirstOrDefaultAsync(p => p.UserId == user.Id)
                                    ?? new UserPreferences { UserId = user.Id };
                preferences.UpdateFrom(request.Preferences);
                db.UserPreferences.Update(preferences);

                // Обновление Location
                var location = await db.UserLocations.FirstOrDefaultAsync(l => l.UserId == user.Id)
                                 ?? new UserLocation { UserId = user.Id };
                location.UpdateFrom(request.Location);
                db.UserLocations.Update(location);

                await db.SaveChangesAsync();
                return Results.NoContent();
            }).RequireAuthorization();

            endpoints.MapPost("/logout", async (
            SignInManager<User> signInManager,
            HttpContext context) =>
            {
                // Выход из системы (удаляет куки аутентификации)
                await signInManager.SignOutAsync();

                /*// Если используется JWT, можно добавить его в "черный список"
                // (требует дополнительной настройки в вашем проекте)
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (!string.IsNullOrEmpty(token))
                {
                    // Здесь можно добавить логику инвалидации токена (например, сохранить в Redis/BanList)
                    // Пример: await tokenService.RevokeTokenAsync(token);
                }*/

                return Results.Ok("Logged out successfully");
            }).RequireAuthorization();


            return endpoints;
        }
    }

    #region DTO Classes
    public record PhoneSexAgeRequest(
        [Required] DateTime BirthDate,
        [Required][RegularExpression("male|female")] string Sex,
        [Required][Phone][StringLength(12)] string PhoneNumber
    );

    public record ProfileResponse(
        AboutDto? About,
        LocationDto? Location,
        PreferencesDto? Preferences
    );

    public record ProfileUpdateRequest(
        [Required] AboutDto About,
        [Required] LocationDto Location,
        [Required] PreferencesDto Preferences
    );

    public record AboutDto(
        string Description = "",
        bool IsSmoker = false,
        string Alcohol = "None",
        string Religion = "Other"
    );

    public record LocationDto(
        double Longitude = 0,
        double Latitude = 0,
        DateTime LastUpdated = default
    );

    public record PreferencesDto(
        string PreferredSex = "both",
        int MinPreferredAge = 18,
        int MaxPreferredAge = 100,
        int SearchRadiusKm = 1
    );
    #endregion

    #region Extension Methods
    public static class EntityExtensions
    {
        public static AboutDto ToDto(this AboutUser entity) => new(
            entity.Description,
            entity.IsSmoker,
            entity.Alcohol,
            entity.Religion
        );

        public static LocationDto ToDto(this UserLocation entity) => new(
            entity.Geography?.X ?? 0,
            entity.Geography?.Y ?? 0,
            entity.LastUpdated
        );

        public static PreferencesDto ToDto(this UserPreferences entity) => new(
            entity.PreferredSex,
            entity.MinPreferredAge,
            entity.MaxPreferredAge,
            entity.SearchRadiusKm
        );

        public static void UpdateFrom(this AboutUser entity, AboutDto dto)
        {
            entity.Description = dto.Description;
            entity.IsSmoker = dto.IsSmoker;
            entity.Alcohol = dto.Alcohol;
            entity.Religion = dto.Religion;
        }

        public static void UpdateFrom(this UserPreferences entity, PreferencesDto dto)
        {
            entity.PreferredSex = dto.PreferredSex;
            entity.MinPreferredAge = dto.MinPreferredAge;
            entity.MaxPreferredAge = dto.MaxPreferredAge;
            entity.SearchRadiusKm = dto.SearchRadiusKm;
        }

        public static void UpdateFrom(this UserLocation entity, LocationDto dto)
        {
            entity.Geography = new Point(dto.Longitude, dto.Latitude) { SRID = 4326 };
            entity.LastUpdated = DateTime.UtcNow;
        }
    }
    #endregion

    #region Custom Validation
    public class CustomAgeValidationAttribute : ValidationAttribute
    {
        private readonly int _minAge;
        public CustomAgeValidationAttribute(int minAge) => _minAge = minAge;

        public override bool IsValid(object? value)
        {
            if (value is DateTime date)
            {
                var age = DateTime.Today.Year - date.Year;
                if (date > DateTime.Today.AddYears(-age)) age--;
                return age >= _minAge;
            }
            return false;
        }

        public override string FormatErrorMessage(string name)
            => $"{name} must indicate age of at least {_minAge} years.";
    }
    #endregion
}