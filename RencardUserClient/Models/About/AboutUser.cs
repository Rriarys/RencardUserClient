using RencardUserClient.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace RencardUserClient.Models.About
{
    public class AboutUser
    {
        public User? User { get; set; } // Обратная ссылка

        [Required]
        public required string UserId { get; set; }
        public string Description { get; set; } = ""; // Описание пользователя, по умолчанию пустое
        public bool IsSmoker { get; set; } = false; // По умолчанию не курит
        public string Alcohol { get; set; } = "None"; // "None", "Rarely", "Frequently"
        public string Religion { get; set; } = "Other"; // "Christian", "Muslim", "Buddhist", "Hindu", "Agnostic", "Atheist"
    }
}
