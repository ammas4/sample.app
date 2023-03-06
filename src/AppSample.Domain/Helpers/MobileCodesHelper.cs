using System.Text.RegularExpressions;

namespace AppSample.Domain.Helpers;

public static class MobileCodesHelper
{
    /// <summary>
    /// мобильный код страны
    /// </summary>
    public const string RussiaMCC = "250";

    public static  bool IsValidMobileCode(string? s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        return Regex.IsMatch(s, @"^\d+-\d+$");
    }
}