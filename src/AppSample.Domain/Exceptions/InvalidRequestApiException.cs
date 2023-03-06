namespace AppSample.Domain.Exceptions;

public class InvalidRequestApiException : BaseApiException
{
    public InvalidRequestApiException(string info )
    {
        HttpCode = 400;
        Error = "Invalid_Request";
        Description = info;
    }

}