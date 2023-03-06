using AppSample.Domain.DAL.DTOs;

namespace AppSample.Domain.Helpers;

public static class AcrValuesHelper
{
    static readonly (string AcrStr, int? Acr)[] SupportedValues =
    {
        ("2", 2),
        ("3", 3)
    };

    public static int? GetFirstSupportedValue(string acrValues) =>
        SupportedValues
            .Where(sv => acrValues.Contains(sv.AcrStr))
            .Select(sv => sv.Acr)
            .FirstOrDefault();

    public static AcrValues GetAcrValues(this string acrValuesStr)
    {
        var acrValues = AcrValues.NoValue;

        if (acrValuesStr.Contains("0"))
            acrValues |= AcrValues.LoA0;
        if (acrValuesStr.Contains("1"))
            acrValues |= AcrValues.LoA1;
        if (acrValuesStr.Contains("2"))
            acrValues |= AcrValues.LoA2;
        if (acrValuesStr.Contains("3"))
            acrValues |= AcrValues.LoA3;
        if (acrValuesStr.Contains("4"))
            acrValues |= AcrValues.LoA4;

        return acrValues & AcrValues.Supported;
    }

    public static AcrValues Min(this AcrValues acrValues)
    {
        if (acrValues == AcrValues.NoValue)
            return AcrValues.NoValue;
        
        if (acrValues.HasFlag(AcrValues.LoA0))
            return AcrValues.LoA0;
        if (acrValues.HasFlag(AcrValues.LoA1))
            return AcrValues.LoA1;
        if (acrValues.HasFlag(AcrValues.LoA2))
            return AcrValues.LoA2;
        if (acrValues.HasFlag(AcrValues.LoA3))
            return AcrValues.LoA3;
        if (acrValues.HasFlag(AcrValues.LoA4))
            return AcrValues.LoA4;
        
        return AcrValues.NoValue;
    }
}