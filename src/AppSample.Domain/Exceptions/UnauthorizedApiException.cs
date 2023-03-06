namespace AppSample.Domain.Exceptions;

public class UnauthorizedApiException : BaseApiException
{
    public UnauthorizedApiException(string? info = null)
    {
        HttpCode = 401;
        Error = "Unauthorized";
        Description = "";
    }

}