namespace AppSample.CoreTools.Logging;

public static class MaskingStrategiesForSpan
{
    public delegate void Mask(Span<char> value);

    public static Mask Strategy(MaskingStrategy maskingStrategy, byte percent) =>
        maskingStrategy switch
        {
            MaskingStrategy.LastSymbolsByPercent => LastSymbolsByPercent(percent),
            MaskingStrategy.Full => Full,

            _ => LastSymbolsByPercent(percent),
        };

    public static Mask Full { get; } = FullMasking;

    public static Mask LastSymbolsByPercent(byte percent) =>
        (value) => LastSymbolsByPercentInternal(value, percent);

    static void FullMasking(Span<char> value)
    {
        for (var pos = 0; pos < value.Length; pos++)
            value[pos] = '*';
    }

    static void LastSymbolsByPercentInternal(Span<char> value, byte percent)
    {
        var previewLength = value.Length * percent / 100;
        for (var pos = previewLength; pos < value.Length; pos++)
            value[pos] = '*';
    }
}