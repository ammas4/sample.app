namespace AppSample.Domain.Exceptions;

public class BadRequestApiException : BaseApiException
{
    public BadRequestApiException(string info)
    {
        HttpCode = 400;
        Error = "Invalid_Request";
        Description = $"Invalid {info}";
    }
}