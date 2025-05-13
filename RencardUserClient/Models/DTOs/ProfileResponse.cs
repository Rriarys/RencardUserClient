namespace RencardUserClient.Models.DTOs
{
    public record ProfileResponse(
        AboutDto? About,
        LocationDto? Location,
        PreferencesDto? Preferences
    );
}
