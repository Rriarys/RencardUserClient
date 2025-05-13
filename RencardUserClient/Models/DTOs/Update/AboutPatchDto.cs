namespace RencardUserClient.Models.DTOs.Update
{
    // Здесь и далле, для PUT /me, чтобы частично обновлять данные юзера 
    public record AboutPatchDto(
       string? Description = null,
       bool? IsSmoker = null,
       string? Alcohol = null,
       string? Religion = null
   );
}
