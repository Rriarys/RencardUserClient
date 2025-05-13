namespace RencardUserClient.Models.DTOs.Update
{
    public record PreferencesPatchDto(
        string? PreferredSex = null,
        int? MinPreferredAge = null,
        int? MaxPreferredAge = null,
        int? SearchRadiusKm = null
    );
}
