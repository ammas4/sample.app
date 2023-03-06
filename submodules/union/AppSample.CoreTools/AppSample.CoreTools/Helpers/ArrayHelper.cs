namespace AppSample.CoreTools.Helpers;

public static class ArrayHelper
{
    public static bool ByteArrayCompare(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
    {
        return a1.SequenceEqual(a2);
    }
}