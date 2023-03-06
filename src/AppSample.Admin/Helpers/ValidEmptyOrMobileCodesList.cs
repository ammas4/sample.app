using System.ComponentModel.DataAnnotations;
using AppSample.Domain.Helpers;

namespace AppSample.Admin.Helpers;

public class ValidEmptyOrMobileCodesList : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        string? str = value as string;

        var parts = StringsHelper.SplitList(str);
        foreach (string part in parts)
        {
            if (MobileCodesHelper.IsValidMobileCode(part) == false)
            {
                return new ValidationResult("Неверный формат кода: " + part);
            }
        }

        return ValidationResult.Success;
    }
}