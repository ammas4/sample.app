using Newtonsoft.Json;

namespace AppSample.CoreTools.Logging;

public static class JsonMasker
{
    public struct PropertyInfo
    {
        readonly JsonReader _reader;

        internal PropertyInfo(JsonReader reader, string name)
        {
            _reader = reader;
            Name = name;
        }

        public string Name { get; }

        public string GetPath() => _reader.Path;
    }

    public delegate bool FieldSelector(PropertyInfo filedIfo);

    public static string MaskByPropertyName(string json, string propertyName)
        => Mask(json, p => p.Name == propertyName);

    public static string MaskByPropertyName(string json, params string[] propertyNames)
        => Mask(json, p => propertyNames?.Contains(p.Name) ?? false);

    public static string MaskByPropertyName(string json, string propertyName, MaskingStrategiesForSpan.Mask mask = null)
        => Mask(json, p => p.Name == propertyName, mask);

    public static string MaskByPropertyName(string json, MaskingStrategiesForSpan.Mask mask = null, params string[] propertyNames)
        => Mask(json, p => propertyNames?.Contains(p.Name) ?? false, mask);

    public static string Mask(string json, FieldSelector selector, MaskingStrategiesForSpan.Mask mask = null)
    {
        if (string.IsNullOrEmpty(json))
            throw new ArgumentException("json must be not empty.", nameof(json));
        if (selector is null)
            throw new ArgumentNullException(nameof(selector));

        var chars = json.ToCharArray();

        using (var jsonTextReader = new JsonTextReader(new StringReader(json)))
            ReadAndMask(jsonTextReader, chars, selector, mask ?? MaskingStrategiesForSpan.Full);

        var maskedJson = new string(chars);
        return maskedJson;
    }

    static void ReadAndMask(JsonTextReader reader, char[] chars,
        FieldSelector fieldSelector, MaskingStrategiesForSpan.Mask mask)
    {
        var walker = new CharsWalker(chars);
        string propertyName = null;
        var prevLine = 0;
        var prevLinePosition = 0;
        do
        {
            if (reader.TokenType == JsonToken.PropertyName)
                propertyName = reader.Value as string;

            if (reader.TokenType == JsonToken.String && propertyName != null
                                                     && fieldSelector(new PropertyInfo(reader, propertyName)))
            {
                walker.GoTo(prevLine, prevLinePosition);
                walker.FindSymbol('"');
                var startPosition = walker.Position + 1;

                walker.GoTo(reader.LineNumber, reader.LinePosition);
                walker.FindSymbolBackWard('"');
                var endPosition = walker.Position - 1;

                Span<char> propertyValue = chars.AsSpan(startPosition, endPosition - startPosition + 1);
                mask(propertyValue);
            }
            prevLine = reader.LineNumber;
            prevLinePosition = reader.LinePosition;
        }
        while (reader.Read());
    }
}