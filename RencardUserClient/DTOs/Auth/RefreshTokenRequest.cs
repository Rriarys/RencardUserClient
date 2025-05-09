using System.ComponentModel.DataAnnotations;

namespace RencardUserClient.DTOs.Auth
{
    public class RefreshTokenRequest
    {
        [Required]
        public required string RefreshToken { get; set; }

        [Required]
        public required string UserId { get; set; }
    }
}
