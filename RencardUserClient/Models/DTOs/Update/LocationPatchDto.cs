namespace RencardUserClient.Models.DTOs.Update
{
    public record LocationPatchDto(
        double? Longitude = null,
        double? Latitude = null
    );
}
