namespace AppSample.Domain.Helpers;

public static class StringsHelper
{
    public static List<string> SplitList(string? s)
    {
        var list = (s ?? "")
            .Split(new[] {'\n', ' '}, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => x != String.Empty)
            .ToList();
        return list;
    }

    public static string JoinList(List<string> parts)
    {
        var rez = string.Join('\n', parts);
        return rez;
    }
}