namespace RencardUserClient.Models.DTOs
{
    public record PreferencesDto(
        string PreferredSex = "both",
        int MinPreferredAge = 18,
        int MaxPreferredAge = 100,
        int SearchRadiusKm = 1
    );
}
