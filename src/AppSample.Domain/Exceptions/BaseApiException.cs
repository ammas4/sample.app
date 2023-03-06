namespace AppSample.Domain.Exceptions;

/// <summary>
/// </summary>
public abstract class BaseApiException : System.Exception
{
    /// <summary>
    /// код HTTP ответа
    /// </summary>
    public short HttpCode { get; set; } = 500;

    /// <summary>
    /// 
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Description { get; set; }
}