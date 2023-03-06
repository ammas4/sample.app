using System.ComponentModel;
using System.Reflection;

namespace AppSample.CoreTools.Helpers;

public static class EnumHelper<T>
{
    /// <summary>
    /// Получить описание перечисления
    /// </summary>
    /// <param name="enumValue"></param>
    /// <param name="defDesc"></param>
    /// <returns></returns>
    public static string GetDescription(T enumValue, string defDesc)
    {
        FieldInfo? fi = enumValue.GetType().GetField(enumValue.ToString());

        var attrs = fi?.GetCustomAttributes(typeof(DescriptionAttribute), true);
        if (attrs != null && attrs.Length > 0)
            return ((DescriptionAttribute) attrs[0]).Description;

        return defDesc;
    }

    /// <summary>
    /// Получить описание перечисления
    /// </summary>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    public static string GetDescription(T enumValue)
    {
        return GetDescription(enumValue, string.Empty);
    }
}