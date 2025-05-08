using RencardUserClient.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace RencardUserClient.Models.Photos
{
    public class UserPhoto
    {
        public User? User { get; set; } // Обратная ссылка

        [Required]
        public required string UserId { get; set; }
        [Url]
        public string? BlobUrl { get; set; } = null; // URL на фото в Blob Storage
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
