using System.ComponentModel.DataAnnotations;
using AppSample.Domain.Helpers;

namespace AppSample.Admin.Helpers;

public class ValidEmptyOrUriList : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        string? str = value as string;

        var parts = StringsHelper.SplitList(str);
        foreach (string part in parts)
        {
            if (Uri.IsWellFormedUriString(part, UriKind.Absolute) == false)
            {
                return new ValidationResult("Неверный формат url: " + part);
            }
        }

        return ValidationResult.Success;
    }
}