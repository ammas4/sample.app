namespace AppSample.Domain.Helpers;

public static class LoginHintHelper
{
    public static bool ContainsMsisdn(string loginHint) => loginHint.StartsWith("MSISDN:");
    public static bool ContainsEncrMsisdn(string loginHint) => loginHint.StartsWith("ENCR_MSISDN:");
    public static bool ContainsPcr(string loginHint) => loginHint.StartsWith("PCR:");

    public static string GetValueWithoutPrefix(string loginHint) =>
        loginHint.Split(":", StringSplitOptions.RemoveEmptyEntries).Last();

    public static string FormatForMsisdn(string msisdn) => "MSISDN:" + msisdn;
    public static string FormatForEncrMsisdn(string encrMsisdn) => "ENCR_MSISDN:" + encrMsisdn;
}