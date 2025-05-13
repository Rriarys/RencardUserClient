using RencardUserClient.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace RencardUserClient.Models.DTOs
{
    public record PhoneSexAgeRequest
    {
        [Required(ErrorMessage = "Birth date is required")]
        [CustomAgeValidation(18, ErrorMessage = "You must be at least 18 years old")]
        public DateTime BirthDate { get; init; }

        [Required(ErrorMessage = "Sex is required")]
        [RegularExpression(@"^(male|female)$", ErrorMessage = "Sex must be either 'male' or 'female'")]
        public string Sex { get; init; } = default!;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(12, MinimumLength = 10, ErrorMessage = "Phone number must be 10-12 digits")]
        [RegularExpression(@"^\+?[0-9]+$", ErrorMessage = "Phone number can only contain numbers and optional '+' prefix")]
        public string PhoneNumber { get; init; } = default!;
    }
}
