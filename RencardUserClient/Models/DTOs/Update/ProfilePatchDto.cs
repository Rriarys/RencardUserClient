namespace RencardUserClient.Models.DTOs.Update
{
    public record ProfilePatchDto(
       AboutPatchDto? About = null,
       PreferencesPatchDto? Preferences = null,
       LocationPatchDto? Location = null
   );
}
