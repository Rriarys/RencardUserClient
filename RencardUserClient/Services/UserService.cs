using Microsoft.AspNetCore.Identity;
using RencardUserClient.Database;
using RencardUserClient.Interfaces;
using RencardUserClient.Models.About;
using RencardUserClient.Models.Identity;
using RencardUserClient.Models.Location;
using RencardUserClient.Models.Photos;
using RencardUserClient.Models.Preferences;

namespace RencardUserClient.Services
{
    public class UserService : IUserService
    {
        private readonly RencardUserDbContext _db;
        private readonly UserManager<User> _userManager;

        public UserService(RencardUserDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<User> CreateAsync(string email, string password, string phone, DateTime birthDate, string sex)
        {
            // 1. Создаём пользователя
            var user = new User
            {
                UserName = email,
                Email = email,
                PhoneNumber = phone,
                BirthDate = birthDate,
                Sex = sex
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(';', result.Errors.Select(e => e.Description)));

            // 2. Создаём связанные записи с дефолтами
            var prefs = new UserPreferences { UserId = user.Id };
            var loc = new UserLocation { UserId = user.Id };
            var about = new AboutUser { UserId = user.Id };
            var photo = new UserPhoto { UserId = user.Id };

            _db.UserPreferences.Add(prefs);
            _db.UserLocations.Add(loc);
            _db.AboutUsers.Add(about);
            _db.UserPhotos.Add(photo);

            await _db.SaveChangesAsync();
            return user;
        }
    }
}
