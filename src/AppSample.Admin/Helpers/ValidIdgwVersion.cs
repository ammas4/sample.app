using System.ComponentModel.DataAnnotations;
using AppSample.CoreTools.Helpers;

namespace AppSample.Admin.Helpers;

public class ValidIdgwVersion : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        string? str = value as string;
        if (string.IsNullOrEmpty(str) || (str != "1" && str != "2"))
        {
            return new ValidationResult("Неверный формат версии");
        }

        return ValidationResult.Success;
    }
}