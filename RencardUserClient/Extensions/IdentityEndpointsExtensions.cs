using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using RencardUserClient.Database;
using RencardUserClient.Models.About;
using RencardUserClient.Models.Identity;
using RencardUserClient.Models.Location;
using RencardUserClient.Models.Preferences;
using System.ComponentModel.DataAnnotations;

namespace RencardUserClient.Extensions
{
    public static class IdentityEndpointsExtensions
    {
        public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
        {
            // Registration
            endpoints.MapPost("/identity/register", async (
                UserManager<User> userManager,
                RegistrationRequest request)
            =>
            {
                // Атрибутная валидация
                var context = new ValidationContext(request);
                var results = new List<ValidationResult>();
                if (!Validator.TryValidateObject(request, context, results, true))
                {
                    var errors = results.Select(r => r.ErrorMessage!).ToArray();
                    return Results.BadRequest(errors);
                }

                // Проверка уникальности email
                if (await userManager.FindByEmailAsync(request.Email) is not null)
                {
                    return Results.BadRequest(new[] { "Email is already taken." });
                }

                // Проверка уникальности phone
                if (await userManager.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber))
                {
                    return Results.BadRequest(new[] { "Phone number is already taken." });
                }

                var user = new User
                {
                    UserName = request.Email,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    BirthDate = request.BirthDate,
                    Sex = request.Sex
                };
                var create = await userManager.CreateAsync(user, request.Password);
                if (!create.Succeeded)
                    return Results.ValidationProblem(create.Errors.ToDictionary(e => e.Code, e => new[] { e.Description }));

                return Results.Ok(new { user.Id, user.Email, user.PhoneNumber });
            })
            .AllowAnonymous();

            // Logout
            endpoints.MapPost("/identity/logout", async (
                SignInManager<User> signIn)
            =>
            {
                await signIn.SignOutAsync();
                return Results.NoContent();
            })
            .RequireAuthorization();

            // Get full profile (about, location, preferences)
            endpoints.MapGet("/users/me/details", async (
                UserManager<User> userManager,
                RencardUserDbContext db,
                HttpContext ctx)
            =>
            {
                var user = await userManager.GetUserAsync(ctx.User);
                if (user == null) return Results.Unauthorized();

                var about = await db.AboutUsers.FindAsync(user.Id);
                var loc = await db.UserLocations.FindAsync(user.Id);
                var pref = await db.UserPreferences.FindAsync(user.Id);

                return Results.Ok(new ProfileResponse
                {
                    About = about ?? new AboutUser { UserId = user.Id },
                    Location = loc,
                    Preferences = pref
                });
            })
            .RequireAuthorization();

            // Update full profile
            endpoints.MapPut("/users/me/details", async (
                UserManager<User> userManager,
                RencardUserDbContext db,
                HttpContext ctx,
                ProfileUpdateRequest req)
            =>
            {
                var user = await userManager.GetUserAsync(ctx.User);
                if (user == null) return Results.Unauthorized();

                // About
                var about = await db.AboutUsers.FindAsync(user.Id)
                            ?? new AboutUser { UserId = user.Id };
                about.Description = req.About.Description;
                about.IsSmoker = req.About.IsSmoker;
                about.Alcohol = req.About.Alcohol;
                about.Religion = req.About.Religion;
                db.Entry(about).State = about.User != null ? EntityState.Modified : EntityState.Added;

                // Preferences
                var pref = await db.UserPreferences.FindAsync(user.Id)
                           ?? new UserPreferences { UserId = user.Id };
                pref.PreferredSex = req.Preferences.PreferredSex;
                pref.MinPreferredAge = req.Preferences.MinPreferredAge;
                pref.MaxPreferredAge = req.Preferences.MaxPreferredAge;
                pref.SearchRadiusKm = req.Preferences.SearchRadiusKm;
                db.Entry(pref).State = pref.User != null ? EntityState.Modified : EntityState.Added;

                // Location
                var loc = await db.UserLocations.FindAsync(user.Id)
                          ?? new UserLocation { UserId = user.Id };
                loc.Geography = new Point(req.Location.Longitude, req.Location.Latitude) { SRID = 4326 };
                loc.LastUpdated = DateTime.UtcNow;
                db.Entry(loc).State = loc.User != null ? EntityState.Modified : EntityState.Added;

                await db.SaveChangesAsync();
                return Results.NoContent();
            })
            .RequireAuthorization();

            return endpoints;
        }
    }

    // Request and response DTOs
    public class RegistrationRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;

        [Required, Phone]
        public string PhoneNumber { get; set; } = default!;

        [Required]
        [CustomAgeValidation(18)]
        public DateTime BirthDate { get; set; }

        [Required, RegularExpression("male|female")]
        public string Sex { get; set; } = default!;
    }

    public class ProfileResponse
    {
        public AboutUser About { get; set; } = default!;
        public UserLocation? Location { get; set; }
        public UserPreferences? Preferences { get; set; }
    }

    public class ProfileUpdateRequest
    {
        [Required]
        public AboutDto About { get; set; } = default!;
        [Required]
        public LocationDto Location { get; set; } = default!;
        [Required]
        public PreferencesDto Preferences { get; set; } = default!;
    }

    public class AboutDto
    {
        public string Description { get; set; } = string.Empty;
        public bool IsSmoker { get; set; }
        public string Alcohol { get; set; } = "None";
        public string Religion { get; set; } = "Other";
    }

    public class LocationDto
    {
        [Range(-180, 180)] public double Longitude { get; set; }
        [Range(-90, 90)] public double Latitude { get; set; }
    }

    public class PreferencesDto
    {
        public string PreferredSex { get; set; } = "both";
        public int MinPreferredAge { get; set; } = 18;
        public int MaxPreferredAge { get; set; } = 100;
        public int SearchRadiusKm { get; set; } = 1;
    }

    // Custom validation attribute for age
    public class CustomAgeValidationAttribute : ValidationAttribute
    {
        private readonly int _minAge;
        public CustomAgeValidationAttribute(int minAge) { _minAge = minAge; }
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
}
