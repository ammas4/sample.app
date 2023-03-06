using AppSample.CoreTools.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppSample.Admin.Helpers;

public static class SelectListItemsHelper
{
    public static IEnumerable<SelectListItem> Build<T>(params T[] values) where T : Enum
    {
        foreach (T value in values)
        {
            yield return new SelectListItem(EnumHelper<T>.GetDescription(value), value.ToString("D"));
        }
    }
}