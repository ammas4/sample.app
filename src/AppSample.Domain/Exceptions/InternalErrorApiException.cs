namespace AppSample.Domain.Exceptions;

public class InternalErrorApiException : BaseApiException
{
    public InternalErrorApiException(string? info = null)
    {
        HttpCode = 500;
        Error = "Internal error";
        Description = "";
    }
}