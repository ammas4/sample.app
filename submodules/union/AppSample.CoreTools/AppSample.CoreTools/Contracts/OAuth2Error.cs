namespace AppSample.CoreTools.Contracts;

public enum OAuth2Error
{
	InvalidRequest,
	UnauthorizedClient,
	AccessDenied,
	UnsupportedResponseType,
	InvalidScope,
	ServerError,
	TemporarilyUnavailable,
	Accepted,
	NotFoundEntity,
	Timeout,

	// beyond oauth2:

	InvalidGrant,
	UnsupportedGrantType,
	UserInfoError,
	PremiumInfoError,
	PaymentFail,
	AuthorizationError,
	StatusConflict,
}