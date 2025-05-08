using Microsoft.AspNetCore.Identity;
using RencardUserClient.Models.About;
using RencardUserClient.Models.Location;
using RencardUserClient.Models.Photos;
using RencardUserClient.Models.Preferences;
using System.ComponentModel.DataAnnotations;

namespace RencardUserClient.Models.Identity
{
    public class User : IdentityUser
    {
        // Навигационные свойства для связей 1:1
        public virtual UserPreferences? Preferences { get; set; }
        public virtual UserLocation? Location { get; set; }
        public virtual AboutUser? About { get; set; }
        public virtual UserPhoto? Photo { get; set; }

        [Required]
        public DateTime? BirthDate { get; set; }
        // На контроллере проставлю эти значения как обязательные

        [Required(ErrorMessage = "Sex is required.")]
        [RegularExpression("male|female")]
        public string? Sex { get; set; }  
    }
}
