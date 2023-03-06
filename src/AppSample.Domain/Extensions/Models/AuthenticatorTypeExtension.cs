using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;

namespace AppSample.Domain.Extensions.Models
{
    public static class AuthenticatorTypeExtension
    {
        public static IdgwAuthMode GetAuthMode(this AuthenticatorType authenticatorType)
        {
            return authenticatorType switch
            {
                AuthenticatorType.Ussd => IdgwAuthMode.Ussd,
                AuthenticatorType.NoValue => IdgwAuthMode.None,
                AuthenticatorType.SmsOtp => IdgwAuthMode.SmsOTP,
                AuthenticatorType.SmsWithUrl => IdgwAuthMode.SmsWithUrl,
                AuthenticatorType.Seamless => IdgwAuthMode.Seamless,
                AuthenticatorType.PushMc => IdgwAuthMode.OldMcPush,
                AuthenticatorType.PushDstk => IdgwAuthMode.DstkPush,
                _ => throw new Exception("Type 'AuthenticatorType' could not be converted into 'IdgwAuthMode'")
            };
        }
    }
}
