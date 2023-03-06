using System.Text.RegularExpressions;

namespace AppSample.Domain.Helpers;

public static class MsisdnHelper
{
    static readonly Regex MsisdnRegex = new(@"^[\d]+$");

    public static bool IsValid(string msisdn) => MsisdnRegex.IsMatch(msisdn);
}