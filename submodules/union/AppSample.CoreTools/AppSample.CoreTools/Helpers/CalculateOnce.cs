namespace AppSample.CoreTools.Helpers;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TValue"></typeparam>
public class CalculateOnce<TValue> where TValue : class
{
    TValue? _value;
    readonly object _lockObject = new();

    /// <summary>
    /// Возврат запомненного значения или его расчет
    /// </summary>
    /// <param name="generator"></param>
    /// <returns></returns>
    public TValue Get(Func<TValue> generator)
    {
        if (_value == null)
        {
            lock (_lockObject)
            {
                if (_value == null)
                {
                    _value = generator();
                }
            }
        }

        return _value;
    }
}