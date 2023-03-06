using System.ComponentModel.DataAnnotations;
using AppSample.Domain.Models;

namespace AppSample.Admin.Helpers;

public class ValidAuthMode : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        string? str = value as string;
        if (string.IsNullOrEmpty(str) ||  Enum.TryParse<IdgwAuthMode>(str,out _)==false)
        {
            return new ValidationResult("Неверный формат");
        }

        return ValidationResult.Success;
    }
}