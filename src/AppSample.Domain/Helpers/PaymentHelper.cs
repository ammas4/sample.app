using System.Globalization;

namespace AppSample.Domain.Helpers;

public static class PaymentHelper
{
	public static decimal? ParseOrderSumString(string value)
    {
        value = value.Replace(",", ".");

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            // Проверка, что после запятой не больше двух знаков со значением
            var rounded = Math.Round(result, 2);
            if (rounded == result)
            {
                return result;
            }
        }

        return null;
    }
}