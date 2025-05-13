using System.ComponentModel.DataAnnotations;

namespace RencardUserClient.Models.Validation
{
    public class CustomAgeValidationAttribute : ValidationAttribute
    {
        private readonly int _minAge;

        public CustomAgeValidationAttribute(int minAge)
        {
            _minAge = minAge;
            // Задаём дефолтное сообщение, если оно не указано
            ErrorMessage ??= $"You must be at least {_minAge} years old";
        }

        // Обратите внимание: возвращаем ValidationResult? и возвращаем null для успеха
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is not DateTime birthDate)
                return new ValidationResult("Invalid date format", new[] { context.MemberName! });

            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age))
                age--;

            if (age < _minAge)
                // обязательно передаём непустое сообщение
                return new ValidationResult(ErrorMessage, new[] { context.MemberName! });

            // Возвращаем null — значит, ошибок нет
            return null;
        }
    }
}
