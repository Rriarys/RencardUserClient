using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RencardUserClient.Database;
using RencardUserClient.Models.About;
using RencardUserClient.Models.Identity;
using RencardUserClient.Models.Location;
using RencardUserClient.Models.Preferences;
using System.ComponentModel.DataAnnotations;
using RencardUserClient.Models.DTOs;
using RencardUserClient.Models.DTOs.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using RencardUserClient.Models.DTOs.Update;

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
                // 1. Проверка типа контента
                if (!ctx.Request.HasJsonContentType())
                {
                    return Results.BadRequest("Content-Type must be application/json");
                }

                // 2. Валидация модели
                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(request, null, null);

                bool isValid = Validator.TryValidateObject(
                    request,
                    validationContext,
                    validationResults,
                    validateAllProperties: true);

                Console.WriteLine($"Model validation result: {isValid}");
                foreach (var error in validationResults)
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }

                if (!isValid)
                {
                    return Results.ValidationProblem(
                        validationResults.ToDictionary(
                            e => e.MemberNames.FirstOrDefault() ?? string.Empty,
                            e => new[] { e.ErrorMessage ?? "Invalid value" }
                        )
                    );
                }

                // 3. Дополнительная проверка номера телефона
                if (await db.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber))
                {
                    return Results.BadRequest("Phone number is already in use");
                }

                // 4. Обновление пользователя
                var user = await userManager.GetUserAsync(ctx.User);
                if (user == null) return Results.Unauthorized();

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
                if (user == null)
                    return Results.Unauthorized();

                // Initialize About
                if (!await db.AboutUsers.AnyAsync(a => a.UserId == user.Id))
                {
                    var aboutDto = new AboutDto();
                    db.AboutUsers.Add(aboutDto.ToEntity(user.Id));
                }

                // Initialize Preferences
                if (!await db.UserPreferences.AnyAsync(p => p.UserId == user.Id))
                {
                    var prefsDto = new PreferencesDto();
                    db.UserPreferences.Add(prefsDto.ToEntity(user.Id));
                }

                // Initialize Location with null Geography
                if (!await db.UserLocations.AnyAsync(l => l.UserId == user.Id))
                {
                    var locDto = new LocationDto(
                        Longitude: 0,    // placeholder; not used in ToEntity
                        Latitude: 0,
                        LastUpdated: default
                    );
                    db.UserLocations.Add(locDto.ToEntity(user.Id));
                }

                // Initialize Photo record (without actual photo)
                if (!await db.UserPhotos.AnyAsync(p => p.UserId == user.Id))
                {
                    var photoDto = new PhotoDto
                    {
                        BlobUrl = null,
                        UploadedAt = DateTime.UtcNow
                    };
                    db.UserPhotos.Add(photoDto.ToEntity(user.Id));
                }

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

            // PATCH /me — частичное обновление профиля
            endpoints.MapMethods("/me", new[] { "PATCH" }, async (
                JsonPatchDocument<ProfilePatchDto> patchDoc,
                UserManager<User> userManager,
                RencardUserDbContext db,
                HttpContext ctx) =>
            {
                var user = await userManager.GetUserAsync(ctx.User);
                if (user == null)
                    return Results.Unauthorized();

                // 1) Загружаем текущие сущности или инициализируем (для новых пользователей)
                var aboutE = await db.AboutUsers.FirstOrDefaultAsync(a => a.UserId == user.Id)
                              ?? new AboutUser { UserId = user.Id };
                var prefsE = await db.UserPreferences.FirstOrDefaultAsync(p => p.UserId == user.Id)
                              ?? new UserPreferences { UserId = user.Id };
                var locationE = await db.UserLocations.FirstOrDefaultAsync(l => l.UserId == user.Id)
                                ?? new UserLocation { UserId = user.Id };

                // 2) Мапим сущности в Update-DTO
                var dto = new ProfilePatchDto(
                    About: aboutE.ToDtoUpdate(),
                    Preferences: prefsE.ToDtoUpdate(),
                    Location: locationE.ToDtoUpdate()
                );

                // 3) Применяем JSON-Patch операции
                patchDoc.ApplyTo(dto);

                // 4) Валидация результирующего DTO
                var validationContext = new ValidationContext(dto);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
                {
                    var errors = validationResults
                        .GroupBy(e => e.MemberNames.FirstOrDefault())
                        .ToDictionary(g => g.Key ?? string.Empty, g => g.Select(e => e.ErrorMessage!).ToArray());
                    return Results.ValidationProblem(errors);
                }

                // 5) Применяем изменения из DTO в сущности
                aboutE.UpdateFrom(dto.About!);
                prefsE.UpdateFrom(dto.Preferences!);
                locationE.UpdateFrom(dto.Location!);

                // 6) Сохраняем в БД
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
}