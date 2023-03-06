using System.Text;
using System.Text.RegularExpressions;

namespace AppSample.CoreTools.Helpers;

public static class StringHelper
{
    public static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) { return input; }

        var startUnderscores = Regex.Match(input, @"^_+");
        return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
    }

    /// <summary>
    /// Преобразование GUID в строку длиной 22 символа через Base64-кодирование
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToShortGuid(Guid value)
    {
        string encoded = Convert.ToBase64String(value.ToByteArray());
        encoded = encoded.Replace('/', '_').Replace('+', '-');
        return encoded.Substring(0, 22);
    }

    /// <summary>
    /// Попытка разбора GUID из строки в 22 символа, полученной через Base64-кодирование
    /// </summary>
    /// <param name="value"></param>
    /// <param name="guid"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static bool TryParseShortGuid(string? value, out Guid guid)
    {
        guid = default;
        if (value?.Length != 22) //должно быть ровно 22 символа
        {
            return false;
        }

        string base64 = value.Replace('_', '/').Replace('-', '+') + "==";
        byte[] array = new byte[16];
        if (Convert.TryFromBase64String(base64, array, out int bytesWritten) == false || bytesWritten != 16) //не удалось распарсить Base64 или число байтов другое
        {
            return false;
        }

        guid = new Guid(array);
        return true;
    }

    public static string GeneratePassword(int length, string allowedChars)
    {
        char[] buf = new char[length];
        for (int i = 0; i < length; i++)
            buf[i] = allowedChars[Random.Shared.Next(allowedChars.Length)];
        var rez = new string(buf);
        return rez;
    }

    public static string MultipleReplace(string text, IReadOnlyDictionary<string, string>? replacements)
    {
        if (replacements == null || replacements.Count == 0) return text;
        if (string.IsNullOrEmpty(text)) return text;

        StringBuilder sb = new StringBuilder();
        sb.Append('(');
        bool isFirst = true;
        foreach (string key in replacements.Keys)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException(nameof(replacements));

            if (isFirst)
                isFirst = false;
            else
                sb.Append('|');
            sb.Append(Regex.Escape(key));
        }
        sb.Append(')');
        
        var pattern = sb.ToString();

        text = Regex.Replace(text, pattern, m => replacements[m.Value]);
        return text;
    }
}