using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;

namespace AppSample.Domain.Interfaces;

public interface IConsentSmsService
{
    Task<bool> SendAsync(ConsentSmsRequest request, Guid code, bool isPaymentScopeSelected);
    Task<bool> SendOtpMessageAsync(ConsentSmsRequest request, string otpCode);
}