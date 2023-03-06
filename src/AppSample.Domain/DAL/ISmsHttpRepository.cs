using AppSample.Domain.DAL.DTOs;
using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;

namespace AppSample.Domain.DAL
{
    public interface ISmsHttpRepository
    {
        string GetSmsOtpEndpoint(Guid smsotpEndpointPathId);
        Task NotifyAboutOtpSmsAsync(AuthorizationRequestDto entity, string otpNotificationUrl);
        Task NotifyAboutSuccessAsync(
            AuthorizationRequestDto entity, AuthenticatorType authenticatorType, ServiceProviderEntity serviceProvider);
        Task NotifyAboutFailureAsync(AuthorizationRequestDto authReqDto, NotificationError notificationError);
    }
}
