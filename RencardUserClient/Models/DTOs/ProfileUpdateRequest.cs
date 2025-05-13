using System.ComponentModel.DataAnnotations;

namespace RencardUserClient.Models.DTOs
{
    public record ProfileUpdateRequest(
        [Required] AboutDto About,
        [Required] LocationDto Location,
        [Required] PreferencesDto Preferences
    );
}
