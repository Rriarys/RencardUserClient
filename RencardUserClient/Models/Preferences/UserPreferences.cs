using RencardUserClient.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace RencardUserClient.Models.Preferences
{
    public class UserPreferences
    {
        public User? User { get; set; } // Обратная ссылка

        [Required]
        public required string UserId { get; set; }
        public string PreferredSex { get; set; } = "both"; // По умолчанию оба пола, но после регистрации будет уточняться, для остальных полей так же
        public int MinPreferredAge { get; set; } = 18;
        public int MaxPreferredAge { get; set; } = 100;
        public int SearchRadiusKm { get; set; } = 1;
    }
}
