using System.ComponentModel.DataAnnotations;

namespace AppSample.Admin.Helpers;

public class ValidUri : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        string? str = value as string;
        if (string.IsNullOrEmpty(str) || Uri.IsWellFormedUriString(str, UriKind.Absolute) == false)
        {
            return new ValidationResult("Неверный формат url");
        }

        return ValidationResult.Success;
    }


}