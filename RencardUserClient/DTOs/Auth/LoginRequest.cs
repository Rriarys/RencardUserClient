using System.ComponentModel.DataAnnotations;

namespace RencardUserClient.DTOs.Auth
{
    public class LoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public bool UseJwt { get; set; } = false;
    }
}
