using System.ComponentModel.DataAnnotations;

namespace AppSample.Admin.Helpers;

public class ValidEmptyOrPositiveInteger : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        string? str = value as string;
        if (string.IsNullOrEmpty(str) == false)
        {
            if (int.TryParse(str, out int temp) == false)
            {
                return new ValidationResult("Неверный формат числа");
            }
            else
            {
                if (temp <= 0) return new ValidationResult("Число должно быть положительным");
            }
        }

        return ValidationResult.Success;
    }
}