namespace RencardUserClient.Models.DTOs
{
    public record LocationDto(
       double Longitude = 0,
       double Latitude = 0,
       DateTime LastUpdated = default
   );
}
