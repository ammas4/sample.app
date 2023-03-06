using System.Security.Cryptography;
using System.Text;

namespace AppSample.Domain.Helpers;

public class HashHelper
{
    public static string HashStringFromUtf8(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }
}