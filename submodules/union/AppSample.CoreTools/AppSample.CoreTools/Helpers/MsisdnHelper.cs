namespace AppSample.CoreTools.Helpers;

public static class MsisdnHelper
{
    /// <summary>
    /// Проверка, что номер является номером телефона России - ("7" и 10 цифр)
    /// </summary>
    /// <param name="msisdn"></param>
    /// <returns></returns>
    public static bool IsRussianNumber(long msisdn)
    {
        return msisdn is >= 70_000_000_000 and <= 79_999_999_999;
    }

    /// <summary>
    /// Возврат номера телефона без кода страны (10 цифр) для номера телефона России
    /// </summary>
    /// <param name="msisdn"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static long GetCtnFromRussianNumber(long msisdn)
    {
        if (IsRussianNumber(msisdn) == false) throw new ArgumentOutOfRangeException(nameof(msisdn));
        return msisdn - 70_000_000_000;
    }
}