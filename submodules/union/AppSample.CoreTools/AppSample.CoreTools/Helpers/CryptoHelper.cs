using System.Security.Cryptography;
using System.Text;

namespace AppSample.CoreTools.Helpers;

public static class CryptoHelper
{
    /// <summary>
    /// Returns the hash as an array of 32 bytes
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>

    public static byte[] Sha256Hash(string s)
    {
        using var crypt = SHA256.Create();
        byte[] hashBytes = crypt.ComputeHash(Encoding.UTF8.GetBytes(s));
        return hashBytes;
    }

}