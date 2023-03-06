namespace AppSample.Domain.Models;

public enum NotificationError
{
    UserDenied,
    Timeout,
    PreviousNotFinished,
    RunOutOfAttempts,
}

public static class NotificationErrorExtensions
{
    /// <summary>
    /// См. https://wiki.mydomain.ru/pages/viewpage.action?pageId=34013414
    /// </summary>
    /// <param name="notificationError"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static (string Error, string Description) GetValues(this NotificationError notificationError) =>
        notificationError switch
        {
            NotificationError.UserDenied => ("authorization_error", "Subscriber denied the request"),
            NotificationError.Timeout => ("authorization_error", "Subscriber consent waiting time expired"),
            NotificationError.PreviousNotFinished => ("authorization_error", "Previous authentication transaction is not finished"),
            NotificationError.RunOutOfAttempts => ("access_denied", "the client has run out of attempts to authorize by sms code"),
            _ => throw new ArgumentOutOfRangeException(nameof(notificationError), notificationError, null)
        };
}