using NetTopologySuite.Geometries;
using RencardUserClient.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace RencardUserClient.Models.Location
{
    public class UserLocation
    {
        public User? User { get; set; } // Обратная ссылка

        [Required]
        public required string UserId { get; set; }
        public Point? Geography { get; set; } = null;// PostGIS Point, чтобы не ставить рандомную точку, если что в контроллере юудем менять. Пока здусь null, подбор анкет не начнется
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow; // Дата последнего обновления геолокации
    }
}
