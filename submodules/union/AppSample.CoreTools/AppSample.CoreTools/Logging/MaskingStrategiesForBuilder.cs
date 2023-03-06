using System.Text;

namespace AppSample.CoreTools.Logging;

public static class MaskingStrategiesForBuilder
{
    public delegate void Mask(StringBuilder stringBuilder, int startPosition, int endPosition);

    public static MaskingStrategiesForBuilder.Mask Strategy(MaskingStrategy maskingStrategy, byte percent) =>
        maskingStrategy switch
        {
            MaskingStrategy.LastSymbolsByPercent => LastSymbolsByPercent(percent),
            MaskingStrategy.Full => Full,

            _ => LastSymbolsByPercent(percent),
        };

    public static Mask Full { get; } = FullMasking;

    public static Mask LastSymbolsByPercent(byte previewPercent) =>
        (sb, stP, enP) => LastSymbolsByPercentInternal(sb, stP, enP, previewPercent);

    static void FullMasking(StringBuilder stringBuilder, int startPosition, int endPosition)
    {
        for (var pos = startPosition; pos <= endPosition; pos++)
            stringBuilder[pos] = '*';
    }

    static void LastSymbolsByPercentInternal(StringBuilder stringBuilder,
        int startPosition, int endPosition, byte previewPercent)
    {
        var valueLength = 1 + endPosition - startPosition;
        var previewLength = valueLength * previewPercent / 100;
        for (var pos = startPosition + previewLength; pos <= endPosition; pos++)
            stringBuilder[pos] = '*';
    }
}