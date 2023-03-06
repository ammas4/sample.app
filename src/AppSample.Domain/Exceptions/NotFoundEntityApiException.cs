namespace AppSample.Domain.Exceptions;

public class NotFoundEntityApiException : BaseApiException
{
    public NotFoundEntityApiException(string info)
    {
        HttpCode = 404;
        Error = "Not_Found_Entity";
        Description = info;
    }
}