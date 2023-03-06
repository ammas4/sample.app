namespace AppSample.CoreTools.Logging;

internal static class QueryStringMasker
{
    public static string Mask(string url, string[] sensitiveParams, MaskingStrategiesForSpan.Mask mask)
    {
        if (url == null)
            throw new ArgumentNullException(nameof(url));
        if (mask == null)
            throw new ArgumentNullException(nameof(mask));
        if (sensitiveParams == null || sensitiveParams.Length == 0)
            return url;

        var queryStringStartPos = url.IndexOf('?');
        if (queryStringStartPos == -1)
            return url;

        var chars = url.ToCharArray();
        MaskQueryStringInternal(chars.AsSpan(queryStringStartPos + 1), sensitiveParams, mask);
        return new string(chars);
    }

    static void MaskQueryStringInternal(Span<char> queryString, string[] sensitiveParams, MaskingStrategiesForSpan.Mask mask)
    {
        int i = 0;

        while (i < queryString.Length)
        {
            int nameStart = i;
            int equalsIndex = -1;

            while (i < queryString.Length)
            {
                char ch = queryString[i];
                if (ch == '=')
                {
                    if (equalsIndex < 0)
                        equalsIndex = i;
                }
                else if (ch == '&')
                    break;

                i++;
            }

            if (equalsIndex >= 0)
                MaskParam(
                    queryString.Slice(nameStart, equalsIndex - nameStart),
                    queryString.Slice(equalsIndex + 1, i - equalsIndex - 1));

            i++;
        }

        void MaskParam(ReadOnlySpan<char> name, Span<char> value)
        {
            foreach (var sensitiveParam in sensitiveParams)
                if (MemoryExtensions.Equals(name, sensitiveParam.AsSpan(), StringComparison.Ordinal))
                    mask(value);
        }
    }
}