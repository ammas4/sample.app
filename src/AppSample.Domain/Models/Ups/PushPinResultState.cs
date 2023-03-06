namespace AppSample.Domain.Models.Ups;

/// <summary>
/// Статусы выполнения запросов
/// </summary>
public enum PushPinResultState
{
    NoValue = 0,
    Sent = 1,
    NotSent = 2,
    ProfileNotFound = 3,
    UserBlocked = 4,
}

public static class PushPinResultStateExtensions
{
    public static AuthenticatorStartResultType ToAuthenticatorStartResultType(this PushPinResultState sendResult) =>
        sendResult switch
        {
            PushPinResultState.NoValue => AuthenticatorStartResultType.NoValue,
            PushPinResultState.Sent => AuthenticatorStartResultType.Started,
            PushPinResultState.UserBlocked => AuthenticatorStartResultType.UserBlocked,
            PushPinResultState.ProfileNotFound => AuthenticatorStartResultType.UserInfoNotFound,
            PushPinResultState.NotSent => AuthenticatorStartResultType.NotSent,
            _ => AuthenticatorStartResultType.NotSent
        };
}