using System.ComponentModel.DataAnnotations;

namespace RencardUserClient.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required, MinLength(6)]
        public required string Password { get; set; }

        [Required]
        public required string PhoneNumber { get; set; }

        [Required]
        public required DateTime BirthDate { get; set; }

        [Required]
        [RegularExpression("male|female")]
        public required string Sex { get; set; }

        public bool UseJwt { get; set; } = false;
    }
}
