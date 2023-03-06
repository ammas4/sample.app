namespace AppSample.CoreTools.Helpers;

public static class UrlHelper
{
    static string CombineEnsureSingleSeparator(string a, string b, char separator)
    {
        if (string.IsNullOrEmpty(a)) return b;
        if (string.IsNullOrEmpty(b)) return a;
        return a.TrimEnd(separator) + separator + b.TrimStart(separator);
    }

    public static string Combine(params string[] parts)
    {
        string result = "";
        bool inQuery = false;
        bool inFragment = false;
        foreach (var part in parts)
        {
            if (string.IsNullOrEmpty(part))
                continue;

            if (result.EndsWith('?') || part.StartsWith('?'))
                result = CombineEnsureSingleSeparator(result, part, '?');
            else if (result.EndsWith('#') || part.StartsWith('#'))
                result = CombineEnsureSingleSeparator(result, part, '#');
            else if (inFragment)
                result += part;
            else if (inQuery)
                result = CombineEnsureSingleSeparator(result, part, '&');
            else
                result = CombineEnsureSingleSeparator(result, part, '/');

            if (part.Contains('#'))
            {
                inQuery = false;
                inFragment = true;
            }
            else if (inFragment == false && part.Contains('?'))
            {
                inQuery = true;
            }
        }

        return result;
    }
}